using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

/// <summary>
/// AI 분류 키워드에 따라 외부 API를 호출하여 추가 검색 결과를 제공하는 유틸리티 클래스.
/// 카카오 로컬, 네이버 쇼핑/뉴스/책, Wikipedia REST, ExchangeRate-API를 지원한다.
/// 모든 메서드는 실패 시 "{}" 반환하며 절대 예외를 상위로 던지지 않는다.
/// </summary>
public static class ExtraApi
{
    // 네이버 오픈 API 인증 정보 (web.config appSettings에서 읽기)
    private static string NaverId     { get { return System.Web.Configuration.WebConfigurationManager.AppSettings["NaverClientId"]; } }
    private static string NaverSecret { get { return System.Web.Configuration.WebConfigurationManager.AppSettings["NaverClientSecret"]; } }
    // 카카오 REST API 키
    private static string KakaoKey    { get { return System.Web.Configuration.WebConfigurationManager.AppSettings["KakaoApiKey"]; } }
    // YouTube Data API v3 키
    private static string YoutubeKey  { get { return System.Web.Configuration.WebConfigurationManager.AppSettings["YouTubeApiKey"]; } }
    // OpenWeatherMap API 키
    private static string WeatherKey  { get { return System.Web.Configuration.WebConfigurationManager.AppSettings["WeatherApiKey"]; } }

    /// <summary>
    /// 검색어와 AI 분류 키워드를 받아 적합한 외부 API 결과를 JSON 문자열로 반환한다.
    /// 환율 관련 검색어는 키워드와 무관하게 환율 카드를 우선 반환한다.
    /// </summary>
    /// <param name="query">원본 검색어</param>
    /// <param name="keyword">AI 분류 키워드 (예: 국내맛집, 쇼핑)</param>
    /// <returns>JSON 문자열 {"type":"...", "items":[...]} 또는 "{}"</returns>
    public static string GetExtra(string query, string keyword)
    {
        try
        {
            // 환율 관련 검색어는 키워드와 무관하게 환율 카드 우선 처리
            if (IsExchangeQuery(query)) return GetExchangeRate();

            switch (keyword)
            {
                case "국내장소":
                case "국내맛집":
                    return GetKakaoPlaces(query);

                case "쇼핑":
                    // shop.json 권한 없을 시 블로그 리뷰로 폴백
                    string shopping = GetNaverShopping(query);
                    return shopping != "{}" ? shopping : GetNaverBlog(query + " 추천 후기");

                case "차량":
                case "게임":
                case "스포츠":
                    // YouTube 영상: 차량 리뷰, 게임 플레이, 스포츠 하이라이트
                    return GetYoutube(query);

                case "국내뉴스":
                    return GetNaverNews(query);

                case "IT기술":
                    return GetWikipedia(query);

                case "해외장소":
                    // 날씨 우선 (도시명 인식 성공 시), 실패하면 Wikipedia로 폴백
                    string wx = GetWeather(query);
                    return wx != "{}" ? wx : GetWikipedia(query);

                case "해외맛집":
                case "해외뉴스":
                case "의료건강":
                    return GetWikipedia(query);

                case "일반":
                    // book.json은 SE05로 비활성화 → Naver 블로그로 대체
                    return GetNaverBlog(query);

                default:
                    return "{}";
            }
        }
        catch { return "{}"; }
    }

    // ── 환율 감지 ──────────────────────────────────────────────────────────────

    /// <summary>검색어에 환율 관련 단어가 포함되어 있는지 확인한다.</summary>
    private static bool IsExchangeQuery(string query)
    {
        string q = (query ?? "").ToLower();
        return q.Contains("환율") || q.Contains("달러") || q.Contains("엔화") ||
               q.Contains("유로") || q.Contains("위안") || q.Contains("원달러") ||
               q.Contains("원엔") || q.Contains("usd") || q.Contains("jpy") ||
               q.Contains("eur")  || q.Contains("cny") || q.Contains("gbp");
    }

    // ── 카카오 로컬 API ────────────────────────────────────────────────────────

    /// <summary>
    /// 카카오 로컬 키워드 검색 API로 장소·맛집 정보를 최대 5개 조회한다.
    /// 반환: {"type":"places","items":[{name,address,phone,category,url,lat,lng},...]}
    /// </summary>
    private static string GetKakaoPlaces(string query)
    {
        string url  = "https://dapi.kakao.com/v2/local/search/keyword.json?query=" +
                      Uri.EscapeDataString(query) + "&size=5";
        string json = HttpGet(url, "KakaoAK " + KakaoKey, null);
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);
            if (!root.ContainsKey("documents")) return "{}";

            var docs = (object[])root["documents"];
            if (docs == null || docs.Length == 0) return "{}";

            var items = new List<Dictionary<string, string>>();
            foreach (object d in docs)
            {
                var doc = (Dictionary<string, object>)d;
                // road_address_name이 없으면 address_name으로 대체
                string addr = Get(doc, "road_address_name");
                if (string.IsNullOrEmpty(addr)) addr = Get(doc, "address_name");

                items.Add(new Dictionary<string, string>
                {
                    { "name",     Get(doc, "place_name")   },
                    { "address",  addr                      },
                    { "phone",    Get(doc, "phone")         },
                    { "category", Get(doc, "category_name") },
                    { "url",      Get(doc, "place_url")     },
                    { "lat",      Get(doc, "y")             },
                    { "lng",      Get(doc, "x")             }
                });
            }

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",  "places" },
                { "items", items }
            });
        }
        catch { return "{}"; }
    }

    // ── 네이버 쇼핑 API ────────────────────────────────────────────────────────

    /// <summary>
    /// 네이버 쇼핑 API로 상품을 최대 6개 조회한다.
    /// 반환: {"type":"shopping","items":[{title,price,mall,image,link},...]}
    /// </summary>
    private static string GetNaverShopping(string query)
    {
        string url  = "https://openapi.naver.com/v1/search/shop.json?query=" +
                      Uri.EscapeDataString(query) + "&display=6&sort=sim";
        string json = HttpGet(url, null, NaverHeaders());
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);
            if (!root.ContainsKey("items")) return "{}";

            var raw = (object[])root["items"];
            if (raw == null || raw.Length == 0) return "{}";

            var items = new List<Dictionary<string, string>>();
            foreach (object it in raw)
            {
                var item  = (Dictionary<string, object>)it;
                string price = Get(item, "lprice");
                if (price == "0") price = "";

                items.Add(new Dictionary<string, string>
                {
                    { "title", StripHtml(Get(item, "title")) },
                    { "price", price                          },
                    { "mall",  Get(item, "mallName")         },
                    { "image", Get(item, "image")            },
                    { "link",  Get(item, "link")             }
                });
            }

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",  "shopping" },
                { "items", items }
            });
        }
        catch { return "{}"; }
    }

    // ── 네이버 블로그 API ──────────────────────────────────────────────────────

    /// <summary>
    /// 네이버 블로그 API로 관련 포스트 5개를 조회한다. (일반 카테고리 폴백용)
    /// 반환: {"type":"blog","items":[{title,desc,link,blogger,date},...]}
    /// </summary>
    private static string GetNaverBlog(string query)
    {
        string url  = "https://openapi.naver.com/v1/search/blog.json?query=" +
                      Uri.EscapeDataString(query) + "&display=5&sort=sim";
        string json = HttpGet(url, null, NaverHeaders());
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);
            if (!root.ContainsKey("items")) return "{}";

            var raw = (object[])root["items"];
            if (raw == null || raw.Length == 0) return "{}";

            var items = new List<Dictionary<string, string>>();
            foreach (object it in raw)
            {
                var item = (Dictionary<string, object>)it;
                items.Add(new Dictionary<string, string>
                {
                    { "title",   StripHtml(Get(item, "title"))       },
                    { "desc",    StripHtml(Get(item, "description")) },
                    { "link",    Get(item, "link")                   },
                    { "blogger", Get(item, "bloggername")            },
                    { "date",    Get(item, "postdate")               }
                });
            }

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",  "blog" },
                { "items", items }
            });
        }
        catch { return "{}"; }
    }

    // ── 네이버 뉴스 API ────────────────────────────────────────────────────────

    /// <summary>
    /// 네이버 뉴스 API로 최신 뉴스 5개를 조회한다.
    /// 반환: {"type":"news","items":[{title,desc,link,pub},...]}
    /// </summary>
    private static string GetNaverNews(string query)
    {
        string url  = "https://openapi.naver.com/v1/search/news.json?query=" +
                      Uri.EscapeDataString(query) + "&display=5&sort=date";
        string json = HttpGet(url, null, NaverHeaders());
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);
            if (!root.ContainsKey("items")) return "{}";

            var raw = (object[])root["items"];
            if (raw == null || raw.Length == 0) return "{}";

            var items = new List<Dictionary<string, string>>();
            foreach (object it in raw)
            {
                var item = (Dictionary<string, object>)it;
                items.Add(new Dictionary<string, string>
                {
                    { "title", StripHtml(Get(item, "title"))       },
                    { "desc",  StripHtml(Get(item, "description")) },
                    { "link",  Get(item, "link")                   },
                    { "pub",   Get(item, "pubDate")                }
                });
            }

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",  "news" },
                { "items", items }
            });
        }
        catch { return "{}"; }
    }

    // ── Wikipedia REST API ─────────────────────────────────────────────────────

    /// <summary>
    /// Wikipedia REST API에서 검색어 요약을 조회한다.
    /// 한국어 Wikipedia 우선, 없으면 영어 Wikipedia 시도.
    /// 반환: {"type":"wiki","title":"...","extract":"...","thumbnail":"...","url":"..."}
    /// </summary>
    private static string GetWikipedia(string query)
    {
        string result = FetchWikiSummary("ko", query);
        if (result != "{}") return result;
        return FetchWikiSummary("en", query);
    }

    private static string FetchWikiSummary(string lang, string query)
    {
        string url  = string.Format("https://{0}.wikipedia.org/api/rest_v1/page/summary/{1}",
                                    lang, Uri.EscapeDataString(query));
        string json = HttpGet(url, null, null);
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var dict = (Dictionary<string, object>)ser.DeserializeObject(json);

            // disambiguation(동음이의어 페이지) 또는 요약 없는 페이지는 스킵
            string pageType = Get(dict, "type");
            if (pageType == "disambiguation" || pageType == "no-extract") return "{}";

            string extract = Get(dict, "extract");
            if (string.IsNullOrWhiteSpace(extract)) return "{}";

            // 요약을 200자로 자름
            if (extract.Length > 200) extract = extract.Substring(0, 200) + "...";

            // 썸네일 이미지 URL 추출
            string thumbnail = "";
            if (dict.ContainsKey("thumbnail") && dict["thumbnail"] != null)
            {
                var thumb = (Dictionary<string, object>)dict["thumbnail"];
                thumbnail = Get(thumb, "source");
            }

            // 데스크톱 페이지 URL 추출 (content_urls.desktop.page)
            string pageUrl = "";
            if (dict.ContainsKey("content_urls") && dict["content_urls"] != null)
            {
                try
                {
                    var cu      = (Dictionary<string, object>)dict["content_urls"];
                    var desktop = (Dictionary<string, object>)cu["desktop"];
                    pageUrl     = desktop["page"].ToString();
                }
                catch { }
            }

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",      "wiki"          },
                { "title",     Get(dict, "title") },
                { "extract",   extract          },
                { "thumbnail", thumbnail        },
                { "url",       pageUrl          }
            });
        }
        catch { return "{}"; }
    }

    // ── YouTube Data API v3 ────────────────────────────────────────────────────

    /// <summary>
    /// YouTube Data API로 관련 영상을 최대 4개 조회한다.
    /// 반환: {"type":"youtube","items":[{id,title,channel,thumb},...]}
    /// </summary>
    private static string GetYoutube(string query)
    {
        // part=snippet: 제목·채널·썸네일 포함, type=video: 채널/재생목록 제외
        string url  = "https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&maxResults=4&q=" +
                      Uri.EscapeDataString(query) + "&key=" + YoutubeKey;
        string json = HttpGet(url, null, null);
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);
            if (!root.ContainsKey("items")) return "{}";

            var raw = (object[])root["items"];
            if (raw == null || raw.Length == 0) return "{}";

            var items = new List<Dictionary<string, string>>();
            foreach (object it in raw)
            {
                var item = (Dictionary<string, object>)it;

                // id.videoId 추출 (채널·재생목록 결과는 videoId가 없으므로 스킵)
                string videoId = "";
                if (item.ContainsKey("id") && item["id"] != null)
                {
                    var idObj = (Dictionary<string, object>)item["id"];
                    videoId = Get(idObj, "videoId");
                }
                if (string.IsNullOrEmpty(videoId)) continue;

                // snippet에서 제목·채널·썸네일 추출
                string title = "", channel = "", thumb = "";
                if (item.ContainsKey("snippet") && item["snippet"] != null)
                {
                    var snippet = (Dictionary<string, object>)item["snippet"];
                    title   = Get(snippet, "title");
                    channel = Get(snippet, "channelTitle");
                    // medium(320×180) 우선, 없으면 default(120×90) 사용
                    if (snippet.ContainsKey("thumbnails") && snippet["thumbnails"] != null)
                    {
                        var thumbs = (Dictionary<string, object>)snippet["thumbnails"];
                        if (thumbs.ContainsKey("medium"))
                            thumb = Get((Dictionary<string, object>)thumbs["medium"], "url");
                        else if (thumbs.ContainsKey("default"))
                            thumb = Get((Dictionary<string, object>)thumbs["default"], "url");
                    }
                }

                items.Add(new Dictionary<string, string>
                {
                    { "id",      videoId },
                    { "title",   title   },
                    { "channel", channel },
                    { "thumb",   thumb   }
                });
            }

            if (items.Count == 0) return "{}";

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",  "youtube" },
                { "items", items }
            });
        }
        catch { return "{}"; }
    }

    // 한국어 주요 도시명 → OpenWeatherMap 인식 영어명 매핑
    private static readonly System.Collections.Generic.Dictionary<string, string> _cityMap =
        new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            {"파리","Paris"},         {"런던","London"},        {"뉴욕","New York"},
            {"도쿄","Tokyo"},         {"오사카","Osaka"},        {"베이징","Beijing"},
            {"상하이","Shanghai"},    {"방콕","Bangkok"},        {"싱가포르","Singapore"},
            {"로스앤젤레스","Los Angeles"}, {"LA","Los Angeles"}, {"시드니","Sydney"},
            {"두바이","Dubai"},       {"로마","Rome"},           {"바르셀로나","Barcelona"},
            {"암스테르담","Amsterdam"},{"베를린","Berlin"},      {"마드리드","Madrid"},
            {"홍콩","Hong Kong"},     {"하노이","Hanoi"},        {"호치민","Ho Chi Minh City"},
            {"타이베이","Taipei"},    {"자카르타","Jakarta"},    {"쿠알라룸푸르","Kuala Lumpur"},
            {"마닐라","Manila"},      {"뭄바이","Mumbai"},       {"델리","Delhi"},
            {"카이로","Cairo"},       {"모스크바","Moscow"},     {"이스탄불","Istanbul"},
            {"멕시코시티","Mexico City"}, {"상파울루","Sao Paulo"}, {"부에노스아이레스","Buenos Aires"},
            {"시카고","Chicago"},     {"토론토","Toronto"},      {"밴쿠버","Vancouver"},
            {"시애틀","Seattle"},     {"샌프란시스코","San Francisco"}, {"라스베가스","Las Vegas"},
        };

    // ── OpenWeatherMap API ─────────────────────────────────────────────────────

    /// <summary>
    /// OpenWeatherMap Current Weather API로 도시 날씨를 조회한다.
    /// 한국어 도시명은 영어 매핑 테이블로 변환 후 호출.
    /// 도시명을 인식 못하면(404 등) "{}" 반환 → 호출자가 Wikipedia로 폴백.
    /// 반환: {"type":"weather","city","country","temp","feels","humidity","wind","desc","icon"}
    /// </summary>
    private static string GetWeather(string query)
    {
        // 한국어 도시명이면 영어로 변환, 없으면 원본 사용
        string cityQuery = query.Trim();
        string mapped;
        if (_cityMap.TryGetValue(cityQuery, out mapped)) cityQuery = mapped;

        // units=metric: 섭씨, lang=ko: 날씨 설명 한국어
        string url  = "https://api.openweathermap.org/data/2.5/weather?q=" +
                      Uri.EscapeDataString(cityQuery) + "&appid=" + WeatherKey + "&units=metric&lang=ko";
        string json = HttpGet(url, null, null);
        if (string.IsNullOrEmpty(json)) return "{}"; // HTTP 404 등 → HttpGet이 "" 반환

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);

            // cod: 성공 시 정수 200, 실패 시 문자열 "404" 등
            if (root.ContainsKey("cod") && root["cod"].ToString() != "200") return "{}";

            var mainDict    = root.ContainsKey("main")    ? (Dictionary<string, object>)root["main"]    : null;
            var windDict    = root.ContainsKey("wind")    ? (Dictionary<string, object>)root["wind"]    : null;
            var sysDict     = root.ContainsKey("sys")     ? (Dictionary<string, object>)root["sys"]     : null;
            var weatherArr  = root.ContainsKey("weather") ? (object[])root["weather"]                   : null;

            if (mainDict == null) return "{}";

            // 날씨 설명 및 아이콘 코드 (배열 첫 번째 항목)
            string desc = "", icon = "";
            if (weatherArr != null && weatherArr.Length > 0)
            {
                var w = (Dictionary<string, object>)weatherArr[0];
                desc = Get(w, "description");
                icon = Get(w, "icon");
            }

            // 온도값 소수점 1자리 포맷 (API는 25.3 같은 float 반환)
            double tv, fv;
            string tempStr  = double.TryParse(mainDict.ContainsKey("temp")       ? mainDict["temp"].ToString()       : "", out tv) ? tv.ToString("F1") : "";
            string feelsStr = double.TryParse(mainDict.ContainsKey("feels_like") ? mainDict["feels_like"].ToString() : "", out fv) ? fv.ToString("F1") : "";

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",     "weather"                                                                  },
                { "city",     Get(root, "name")                                                          },
                { "country",  sysDict != null ? Get(sysDict, "country") : ""                            },
                { "temp",     tempStr                                                                     },
                { "feels",    feelsStr                                                                    },
                { "humidity", mainDict.ContainsKey("humidity") ? mainDict["humidity"].ToString() : ""    },
                { "wind",     windDict != null && windDict.ContainsKey("speed") ? windDict["speed"].ToString() : "" },
                { "desc",     desc },
                { "icon",     icon }
            });
        }
        catch { return "{}"; }
    }

    // ── ExchangeRate-API (무료, 키 불필요) ─────────────────────────────────────

    /// <summary>
    /// ExchangeRate-API로 주요 통화의 원화 환율을 조회한다. (USD 기준)
    /// 반환: {"type":"exchange","rates":{"USD":"1300.00","JPY":"8.67",...},"updated":"..."}
    /// </summary>
    private static string GetExchangeRate()
    {
        string json = HttpGet("https://open.er-api.com/v6/latest/USD", null, null);
        if (string.IsNullOrEmpty(json)) return "{}";

        try
        {
            var ser  = new JavaScriptSerializer();
            var root = (Dictionary<string, object>)ser.DeserializeObject(json);

            if (!root.ContainsKey("result") || root["result"].ToString() != "success") return "{}";

            var ratesRaw = (Dictionary<string, object>)root["rates"];

            // 1 USD = X KRW 기준값
            double krwPerUsd = 1300.0;
            if (ratesRaw.ContainsKey("KRW")) double.TryParse(ratesRaw["KRW"].ToString(), out krwPerUsd);

            // 주요 통화별 1단위 = X KRW 계산
            var rates = new Dictionary<string, string>();
            rates["USD"] = krwPerUsd.ToString("F2"); // 1 USD = KRW

            foreach (string code in new[] { "JPY", "EUR", "CNY", "GBP" })
            {
                if (!ratesRaw.ContainsKey(code)) continue;
                double perUsd;
                if (!double.TryParse(ratesRaw[code].ToString(), out perUsd) || perUsd == 0) continue;
                // 1 외화 = (krwPerUsd / 외화의 USD 비율) KRW
                rates[code] = (krwPerUsd / perUsd).ToString("F2");
            }

            return ser.Serialize(new Dictionary<string, object>
            {
                { "type",    "exchange"                       },
                { "rates",   rates                            },
                { "updated", Get(root, "time_last_update_utc") }
            });
        }
        catch { return "{}"; }
    }

    // ── 공통 헬퍼 ─────────────────────────────────────────────────────────────

    /// <summary>네이버 API 공통 인증 헤더 배열 반환 ("이름:값" 쌍)</summary>
    private static string[] NaverHeaders()
    {
        return new[]
        {
            "X-Naver-Client-Id:"     + NaverId,
            "X-Naver-Client-Secret:" + NaverSecret
        };
    }

    /// <summary>Dictionary에서 키로 값을 안전하게 꺼낸다. 없으면 빈 문자열 반환.</summary>
    private static string Get(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null) return "";
        return dict[key].ToString();
    }

    /// <summary>
    /// HTTP GET 요청을 보내고 응답 본문을 문자열로 반환한다.
    /// 실패(404 포함) 시 빈 문자열 반환.
    /// </summary>
    /// <param name="url">요청 URL</param>
    /// <param name="authHeader">Authorization 헤더값 (예: "KakaoAK xxxx"), null이면 미사용</param>
    /// <param name="extraHeaders">"Name:Value" 쌍 배열, null이면 미사용</param>
    private static string HttpGet(string url, string authHeader, string[] extraHeaders)
    {
        try
        {
            var req       = (HttpWebRequest)WebRequest.Create(url);
            req.Method    = "GET";
            req.Timeout   = 5000; // 5초 타임아웃: 느린 API가 전체 로딩을 막지 않도록
            req.UserAgent = "Mozilla/5.0";
            req.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en;q=0.8");

            if (!string.IsNullOrEmpty(authHeader))
                req.Headers.Add("Authorization", authHeader);

            if (extraHeaders != null)
            {
                foreach (string h in extraHeaders)
                {
                    // "Name:Value" 형식에서 첫 번째 콜론으로 분리
                    int colon = h.IndexOf(':');
                    if (colon > 0)
                        req.Headers.Add(h.Substring(0, colon), h.Substring(colon + 1));
                }
            }

            using (var resp   = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return reader.ReadToEnd();
        }
        catch { return ""; }
    }

    /// <summary>HTML 태그 제거 (네이버 API 응답의 강조 태그 &lt;b&gt; 등 제거용)</summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        return Regex.Replace(html, "<[^>]+>", "");
    }
}
