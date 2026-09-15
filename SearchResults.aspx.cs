using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// 검색 결과 페이지 코드비하인드
/// URL: SearchResults.aspx?q=검색어
/// 주요 기능:
///  - 네이버/다음/구글 3개 엔진을 병렬 크롤링하여 결과를 JSON으로 클라이언트에 전달
///  - JavaScript에서 키워드 점수 기반 가중치 믹스 및 엔진 탭 전환 처리
///  - 로그인 사용자의 검색어를 SearchHistory 테이블에 저장 (7일 자동 삭제)
///  - 인기 검색어 TOP 10 결과를 SearchResultCache 테이블에 캐싱
///  - AJAX 방식으로 검색 기록 조회/삭제, 자동완성 처리
/// </summary>
public partial class SearchResults : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드: action 파라미터가 있으면 AJAX 처리 후 종료,
    /// 없으면 UI 텍스트 바인딩 → 인기 검색어 → 크롤링 결과 바인딩 순으로 처리
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // AJAX 요청 처리: action 파라미터가 있으면 JSON 응답 후 종료
        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action); return; }

        // 다국어 텍스트 바인딩
        litResultsLabel.Text = Lang.Get("search.results");
        litRelatedLabel.Text = Lang.Get("search.related");
        litPopularLabel.Text = Lang.Get("search.popular");
        litLoadingLabel.Text = Lang.Get("search.loading");
        litNoResults.Text    = Lang.Get("search.noResults");
        litNoResultsSub.Text = Lang.Get("search.noResultsSub");
        btnSearch.Text       = Lang.Get("search.placeholder").Contains("Search") ? "Search" : "검색";

        // HiddenField에 JavaScript용 다국어 텍스트와 현재 검색어 저장
        hdnRecentLabel.Value  = Lang.Get("search.recent");
        hdnNoHistLabel.Value  = Lang.Get("search.noHistory");
        hdnSearchPh.Value     = Lang.Get("search.placeholder");
        hdnTimesLabel.Value   = Lang.Get("search.times");
        hdnCurrentQuery.Value = Request.QueryString["q"] ?? "";
        txtSearch.Attributes["placeholder"] = Lang.Get("search.placeholder");

        if (!IsPostBack)
        {
            string q = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(q))
            {
                // 검색창에 현재 검색어 표시
                txtSearch.Text = q;
                BindPopularSearches();
                // 크롤링 + 분류 + 기록 저장 + 캐시 처리를 BindSearch 내에서 일괄 처리
                BindSearch(q);
            }
        }
    }

    /// <summary>
    /// AJAX 요청 분기 처리: JavaScript fetch에서 action 파라미터를 붙여 호출
    /// getHistory / deleteHistory / getPopular / getSuggest 4가지 분기 처리
    /// </summary>
    private void HandleAjax(string action)
    {
        Response.ContentType = "application/json";
        string userID = Session["UserID"] != null ? Session["UserID"].ToString() : null;
        SearchDao dao = new SearchDao();

        // ExtraApi 전체 카테고리 테스트 (디버그용, 확인 후 제거)
        // 예) /SearchResults.aspx?action=debugExtra
        // 특정 카테고리만: /SearchResults.aspx?action=debugExtra&q=서울&kw=국내장소
        if (action == "debugExtra")
        {
            // 단일 테스트 (q, kw 둘 다 있을 때)
            string dq  = Request.QueryString["q"];
            string dkw = Request.QueryString["kw"];
            if (!string.IsNullOrEmpty(dq) && !string.IsNullOrEmpty(dkw))
            {
                Response.ContentType = "application/json; charset=utf-8";
                Response.Write(ExtraApi.GetExtra(dq, dkw));
                Response.End();
                return;
            }

            // 전체 카테고리 테스트: 카테고리별 대표 쿼리로 ExtraApi 호출
            var extraTests = new[]
            {
                new { keyword = "국내장소",  query = "경복궁",  apiLabel = "카카오맵"      },
                new { keyword = "국내맛집",  query = "홍대맛집", apiLabel = "카카오맵"      },
                new { keyword = "쇼핑",      query = "나이키",   apiLabel = "네이버쇼핑"    },
                new { keyword = "차량",      query = "테슬라",   apiLabel = "유튜브"        },
                new { keyword = "게임",      query = "리그오브레전드", apiLabel = "유튜브"  },
                new { keyword = "스포츠",    query = "손흥민",   apiLabel = "유튜브"        },
                new { keyword = "국내뉴스",  query = "코스피",   apiLabel = "네이버뉴스"    },
                new { keyword = "IT기술",    query = "파이썬",   apiLabel = "Wikipedia"     },
                new { keyword = "해외장소",  query = "파리",     apiLabel = "날씨/Wikipedia"},
                new { keyword = "해외맛집",  query = "라멘",     apiLabel = "Wikipedia"     },
                new { keyword = "해외뉴스",  query = "트럼프",   apiLabel = "Wikipedia"     },
                new { keyword = "의료건강",  query = "독감",     apiLabel = "Wikipedia"     },
                new { keyword = "일반",      query = "날씨",     apiLabel = "네이버책"      },
                new { keyword = "해외뉴스",  query = "달러 환율", apiLabel = "환율(우선처리)"},
            };

            // 순차 호출 (외부 API 동시 요청 부하 방지)
            var sb3 = new StringBuilder("[");
            for (int i = 0; i < extraTests.Length; i++)
            {
                if (i > 0) sb3.Append(",");
                var t      = extraTests[i];
                string res = ExtraApi.GetExtra(t.query, t.keyword);
                bool   ok  = res != "{}";

                // type 필드만 파싱해서 어떤 API가 응답했는지 확인
                string resType = "";
                try
                {
                    var parsed = new JavaScriptSerializer().DeserializeObject(res)
                                 as System.Collections.Generic.Dictionary<string, object>;
                    if (parsed != null && parsed.ContainsKey("type"))
                        resType = parsed["type"].ToString();
                }
                catch { }

                sb3.AppendFormat(
                    "{{\"keyword\":\"{0}\",\"query\":\"{1}\",\"api\":\"{2}\",\"type\":\"{3}\",\"ok\":{4}}}",
                    t.keyword, t.query, t.apiLabel, resType, ok ? "true" : "false");
            }
            sb3.Append("]");
            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(sb3.ToString());
            Response.End();
            return;
        }

        // 네이버 API 원시 응답 확인 (디버그용, 확인 후 제거)
        // 예) /SearchResults.aspx?action=debugNaver&q=나이키&type=shop
        // type: shop / book / news
        if (action == "debugNaver")
        {
            string dq   = Request.QueryString["q"]    ?? "나이키";
            string type = Request.QueryString["type"] ?? "shop";
            string navUrl = string.Format(
                "https://openapi.naver.com/v1/search/{0}.json?query={1}&display=3",
                type, Uri.EscapeDataString(dq));

            string raw = "";
            int    statusCode = 0;
            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(navUrl);
                req.Method    = "GET";
                req.Timeout   = 5000;
                req.UserAgent = "Mozilla/5.0";
                req.Headers.Add("X-Naver-Client-Id",     System.Web.Configuration.WebConfigurationManager.AppSettings["NaverClientId"]);
                req.Headers.Add("X-Naver-Client-Secret", System.Web.Configuration.WebConfigurationManager.AppSettings["NaverClientSecret"]);
                try
                {
                    using (var resp   = (System.Net.HttpWebResponse)req.GetResponse())
                    using (var reader = new System.IO.StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        statusCode = (int)resp.StatusCode;
                        raw        = reader.ReadToEnd();
                    }
                }
                catch (System.Net.WebException wex)
                {
                    if (wex.Response != null)
                    {
                        statusCode = (int)((System.Net.HttpWebResponse)wex.Response).StatusCode;
                        using (var r = new System.IO.StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                            raw = r.ReadToEnd();
                    }
                    else raw = wex.Message;
                }
            }
            catch (Exception ex) { raw = ex.Message; }

            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(string.Format("{{\"url\":\"{0}\",\"status\":{1},\"body\":{2}}}",
                navUrl.Replace("\"", "\\\""),
                statusCode,
                string.IsNullOrEmpty(raw) ? "\"\"" : raw));
            Response.End();
            return;
        }

        // 카테고리 분류 전체 테스트 (디버그용, 확인 후 제거)
        // 예) /SearchResults.aspx?action=debugClassify
        if (action == "debugClassify")
        {
            // 카테고리당 대표 쿼리 1개씩 — 기대 결과와 함께 병렬 분류
            var tests = new[]
            {
                new { query = "테슬라",   expect = "차량"   },
                new { query = "경복궁",   expect = "국내장소" },
                new { query = "파리",     expect = "해외장소" },
                new { query = "삼겹살",   expect = "국내맛집" },
                new { query = "라멘",     expect = "해외맛집" },
                new { query = "배그",     expect = "게임"   },
                new { query = "파이썬",   expect = "IT기술" },
                new { query = "나이키",   expect = "쇼핑"   },
                new { query = "코스피",   expect = "국내뉴스" },
                new { query = "트럼프",   expect = "해외뉴스" },
                new { query = "독감",     expect = "의료건강" },
                new { query = "손흥민",   expect = "스포츠" },
                new { query = "날씨",     expect = "일반"   },
            };

            // 순차 분류 — 병렬로 쏘면 Groq 무료 RPM 초과로 전부 "일반" 반환됨
            var results = new string[tests.Length];
            for (int i = 0; i < tests.Length; i++)
            {
                results[i] = ClaudeApi.Classify(tests[i].query);
                System.Threading.Thread.Sleep(2000); // Groq 무료 30 RPM → 2초 간격 필요
            }

            // JSON 배열로 결과 반환: [{query, expect, actual, ok}]
            var sb2 = new StringBuilder("[");
            for (int i = 0; i < tests.Length; i++)
            {
                if (i > 0) sb2.Append(",");
                string actual = results[i] ?? "TIMEOUT";
                bool   ok     = actual == tests[i].expect;
                sb2.AppendFormat(
                    "{{\"query\":\"{0}\",\"expect\":\"{1}\",\"actual\":\"{2}\",\"ok\":{3}}}",
                    tests[i].query, tests[i].expect, actual, ok ? "true" : "false");
            }
            sb2.Append("]");
            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(sb2.ToString());
            Response.End();
            return;
        }

        // 최근 검색 기록 조회 (검색창 드롭다운용)
        if (action == "getHistory")
        {
            if (userID == null) { Response.Write("[]"); Response.End(); return; }
            DataTable dt = dao.GetRecentSearches(userID);
            var sb = new System.Text.StringBuilder("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                string q = dt.Rows[i]["Query"].ToString().Replace("\"", "\\\"");
                sb.AppendFormat("{{\"query\":\"{0}\"}}", q);
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        // 특정 검색어 기록 삭제 (드롭다운 X 버튼)
        else if (action == "deleteHistory")
        {
            if (userID == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            string query = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(query))
            {
                dao.DeleteSearchHistory(userID, query);
                Response.Write("{\"result\":\"ok\"}");
            }
        }
        // 인기 검색어 TOP 10 조회
        else if (action == "getPopular")
        {
            DataTable dt = dao.GetTopSearches(10);
            var sb = new System.Text.StringBuilder("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                string q     = dt.Rows[i]["Query"].ToString().Replace("\"", "\\\"");
                string count = dt.Rows[i]["SearchCount"].ToString();
                sb.AppendFormat("{{\"query\":\"{0}\",\"count\":{1}}}", q, count);
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        // 다음 배치 크롤링: 11페이지+ 이동 시 클라이언트 JavaScript에서 AJAX 호출
        // Naver: start 오프셋, Daum: page 오프셋으로 다음 100개 결과 반환
        // DDG(구글 탭)는 POST 페이지네이션이 필요해 제외 → 빈 배열 반환
        else if (action == "crawlMore")
        {
            string q = Request.QueryString["q"];
            if (string.IsNullOrEmpty(q)) { Response.Write("{\"google\":[],\"naver\":[],\"daum\":[]}"); Response.End(); return; }

            int batchNo = 2;
            int.TryParse(Request.QueryString["batch"], out batchNo);
            if (batchNo < 2) batchNo = 2;

            // Naver: 배치마다 100개씩 오프셋 (배치 2 → start=101, 배치 3 → start=201)
            int naverStart = (batchNo - 1) * 100 + 1;
            // Daum: 배치마다 한 페이지씩 이동 (배치 2 → page=2, 배치 3 → page=3)
            int daumPage = batchNo;

            Task<List<SearchResult>> naverTask = Task.Run(() => NaverCrawler.Search(q, 100, naverStart));
            Task<List<SearchResult>> daumTask  = Task.Run(() => DaumCrawler.Search(q, 50, daumPage));
            Task.WaitAll(new Task[] { naverTask, daumTask }, 12000);

            var naverList = (naverTask.IsCompleted && !naverTask.IsFaulted) ? naverTask.Result : new List<SearchResult>();
            var daumList  = (daumTask.IsCompleted  && !daumTask.IsFaulted)  ? daumTask.Result  : new List<SearchResult>();

            // google은 DDG 페이지네이션 미지원으로 빈 배열, 클라이언트가 naver+daum만 믹스
            Response.Write("{\"google\":[],\"naver\":" + ToJson(naverList) + ",\"daum\":" + ToJson(daumList) + "}");
            Response.End();
            return;
        }

        // 결과 URL 목록에서 og:image / twitter:image 추출
        // POST body: JSON 배열 ["url1","url2",...] (최대 10개)
        // 응답: JSON 객체 {"url1":"imageUrl1","url2":"", ...}
        else if (action == "getImages")
        {
            // POST body에서 URL JSON 배열 읽기 (Content-Type: application/json)
            string reqBody = "";
            try
            {
                using (var sr = new StreamReader(Request.InputStream, Encoding.UTF8))
                    reqBody = sr.ReadToEnd();
            }
            catch { }

            var urls = new List<string>();
            try
            {
                var ser    = new JavaScriptSerializer();
                var parsed = ser.DeserializeObject(reqBody) as object[];
                if (parsed != null)
                    foreach (object u in parsed)
                        if (u != null) urls.Add(u.ToString());
            }
            catch { }

            // 한 페이지(10개) 기준으로만 처리 (서버 부하 제한)
            if (urls.Count > 10) urls = urls.GetRange(0, 10);

            // 각 URL에서 og:image 병렬 추출 (개별 2초 타임아웃, 전체 최대 8초 대기)
            var imgResults = new string[urls.Count];
            var imgTasks   = new Task[urls.Count];
            for (int i = 0; i < urls.Count; i++)
            {
                int    idx = i;       // 클로저 캡처: 루프 변수 직접 캡처 금지
                string u   = urls[i];
                imgTasks[idx] = Task.Run(() => { imgResults[idx] = FetchOgImage(u); });
            }
            Task.WaitAll(imgTasks, 8000);

            // JSON 객체 빌드: {"url":"imageUrl",...}
            var sb = new StringBuilder("{");
            for (int i = 0; i < urls.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.AppendFormat("\"{0}\":\"{1}\"",
                    JsonEscapeStr(urls[i]),
                    JsonEscapeStr(imgResults[i] ?? ""));
            }
            sb.Append("}");
            Response.Write(sb.ToString());
            Response.End();
            return;
        }

        // 자동완성 제안어 조회 (입력 중 드롭다운)
        else if (action == "getSuggest")
        {
            string keyword = Request.QueryString["q"];
            if (string.IsNullOrEmpty(keyword)) { Response.Write("[]"); Response.End(); return; }
            string sql = string.Format(@"
                SELECT TOP 5 Query, COUNT(*) AS SearchCount
                FROM SearchHistory
                WHERE Query LIKE N'{0}%'
                GROUP BY Query
                ORDER BY SearchCount DESC",
                keyword.Replace("'", "''"));
            DataSet ds = DbMan.DataAdapterFill(sql, "Suggest");
            var sb = new System.Text.StringBuilder("[");
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                string q = ds.Tables[0].Rows[i]["Query"].ToString().Replace("\"", "\\\"");
                sb.AppendFormat("\"{0}\"", q);
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        Response.End();
    }

    /// <summary>
    /// 검색 버튼 클릭: 검색어를 URL에 담아 리다이렉트
    /// </summary>
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtSearch.Text))
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(txtSearch.Text));
    }

    /// <summary>
    /// 인기 검색어 사이드바 바인딩 (실패해도 전체 페이지에 영향 없음)
    /// </summary>
    private void BindPopularSearches()
    {
        try
        {
            DataTable dt = new SearchDao().GetTopSearches(10);
            rptPopular.DataSource = dt;
            rptPopular.DataBind();
        }
        catch { }
    }

    /// <summary>
    /// 메인 검색 로직:
    ///  1. SearchResultCache 테이블에서 캐시 조회
    ///  2. NIM AI로 키워드 분류 (동기, ~75ms) → 분류 결과를 크롤러·ExtraApi에 전달
    ///  3. 캐시 미스 시 구글·네이버·다음 크롤링 + ExtraApi를 병렬 실행
    ///  4. 캐시 히트 시 ExtraApi만 단독 호출
    ///  5. 로그인 유저의 검색 기록 저장 (7일 이상 지난 기록 자동 삭제 후)
    ///  6. 인기 검색어 TOP 10이면 캐시 저장
    ///  7. 엔진별 결과 + 추가 API 결과 JSON을 HiddenField에 저장 → JavaScript로 렌더링
    /// </summary>
    private void BindSearch(string q)
    {
        var dao           = new SearchDao();
        var googleResults = new List<SearchResult>();
        var naverResults  = new List<SearchResult>();
        var daumResults   = new List<SearchResult>();

        // ── 캐시 조회 ─────────────────────────────────────────────────────────────
        bool cacheHit = false;
        try
        {
            DataTable cached = dao.GetCachedResults(q);
            if (cached != null && cached.Rows.Count > 0)
            {
                googleResults = DataTableToList(cached, "google");
                naverResults  = DataTableToList(cached, "naver");
                daumResults   = DataTableToList(cached, "daum");
                cacheHit      = true;
            }
        }
        catch { }

        // ── AI 분류 (항상 동기로 실행, NIM ~75ms) ─────────────────────────────────
        // ExtraApi 호출·기록 저장·배지 표시 모두 keyword를 쓰므로 캐시 여부와 무관하게 실행
        string keyword = ClaudeApi.Classify(q);

        // ── 캐시 미스: 크롤러 + ExtraApi + 관련검색어 병렬 실행 ──────────────────
        string extraJson   = "{}";
        string relatedJson = "[]";
        if (!cacheHit)
        {
            // DDG 30 + Naver 100 + Daum 50 + ExtraApi + 관련검색어를 동시에 실행하여 대기 시간 최소화
            var googleTask = Task.Run(() => GoogleCrawler.Search(q, 30));
            var naverTask  = Task.Run(() => NaverCrawler.Search(q, 100));
            var daumTask   = Task.Run(() => DaumCrawler.Search(q, 50));
            var extraTask  = Task.Run(() => ExtraApi.GetExtra(q, keyword));
            var relTask    = Task.Run(() => ClaudeApi.GetRelated(q));

            // 최대 10초 대기 (타임아웃 초과 시 완료된 것만 사용)
            Task.WaitAll(new Task[] { googleTask, naverTask, daumTask, extraTask, relTask }, 10000);

            googleResults = (googleTask.IsCompleted && !googleTask.IsFaulted) ? googleTask.Result : new List<SearchResult>();
            naverResults  = (naverTask.IsCompleted  && !naverTask.IsFaulted)  ? naverTask.Result  : new List<SearchResult>();
            daumResults   = (daumTask.IsCompleted   && !daumTask.IsFaulted)   ? daumTask.Result   : new List<SearchResult>();
            extraJson     = (extraTask.IsCompleted  && !extraTask.IsFaulted)  ? extraTask.Result  : "{}";
            relatedJson   = (relTask.IsCompleted    && !relTask.IsFaulted)    ? relTask.Result    : "[]";

            // TOP 10 인기 검색어이면 캐시 저장
            int totalCount = googleResults.Count + naverResults.Count + daumResults.Count;
            if (totalCount > 0)
            {
                try
                {
                    DataTable top10 = dao.GetTopSearches(10);
                    foreach (DataRow row in top10.Rows)
                    {
                        if (row["Query"].ToString() != q) continue;
                        var allResults = new List<SearchResult>(googleResults);
                        allResults.AddRange(naverResults);
                        allResults.AddRange(daumResults);
                        dao.SaveResultCache(q, ResultsToDataTable(allResults));
                        break;
                    }
                }
                catch { }
            }
        }
        else
        {
            // 캐시 히트 시 ExtraApi + 관련검색어를 병렬로 호출 (크롤러 불필요)
            var extraTask2 = Task.Run(() => ExtraApi.GetExtra(q, keyword));
            var relTask2   = Task.Run(() => ClaudeApi.GetRelated(q));
            Task.WaitAll(new Task[] { extraTask2, relTask2 }, 6000);
            extraJson   = (extraTask2.IsCompleted && !extraTask2.IsFaulted) ? extraTask2.Result : "{}";
            relatedJson = (relTask2.IsCompleted   && !relTask2.IsFaulted)   ? relTask2.Result   : "[]";
        }

        // ── 로그인 유저: 검색 기록 저장 + 엔진 점수 갱신 ────────────────────────
        if (Session["UserID"] != null)
        {
            string uid = Session["UserID"].ToString();
            // 7일 이상 지난 검색 기록·캐시 자동 삭제
            try { DbMan.ExecuteNonQuery("DELETE FROM SearchHistory WHERE SearchTime < DATEADD(DAY, -7, GETDATE())"); DbMan.Close(); }
            catch { DbMan.Close(); }
            try { DbMan.ExecuteNonQuery("DELETE FROM SearchResultCache WHERE CachedAt < DATEADD(DAY, -7, GETDATE())"); DbMan.Close(); }
            catch { DbMan.Close(); }
            try { dao.AddSearchHistory(uid, q, keyword); } catch { }
            // 엔진 점수 감쇠(×0.97) 후 이번 검색 키워드 델타 누적
            double[] delta = KeywordToScoreDelta(keyword);
            try { new MemberDao().ApplyEngineScore(uid, delta[0], delta[1], delta[2]); } catch { }
        }

        // ── HiddenField에 JSON 직렬화 저장 → 클라이언트 JavaScript에서 렌더링 ─────
        hdnGoogleJson.Value = ToJson(googleResults);
        hdnNaverJson.Value  = ToJson(naverResults);
        hdnDaumJson.Value   = ToJson(daumResults);
        // 로그인 유저: 개인화 엔진 점수 사용. 비로그인·신규: 키워드 고정 점수
        int[] scores = BuildEngineScores(keyword);
        // AI 분류 키워드 (배지 표시용)
        hdnKeyword.Value   = keyword;
        // 외부 API 추가 결과 (카카오 장소, 네이버 쇼핑/뉴스/책, Wikipedia, 환율)
        hdnExtraJson.Value   = extraJson;
        // NIM AI 생성 관련 검색어 JSON 배열 (사이드바 관련 검색어 섹션)
        hdnRelatedJson.Value = relatedJson;

        int total      = googleResults.Count + naverResults.Count + daumResults.Count;
        bool isEnglish = Lang.Get("search.results").Contains("Search");
        litResultsLabel.Text = isEnglish
            ? string.Format("{0} results for &ldquo;{1}&rdquo;", total, Server.HtmlEncode(q))
            : string.Format("&ldquo;{0}&rdquo;에 대한 검색 결과 {1}개", Server.HtmlEncode(q), total);
    }

    // DataTable의 Source 컬럼으로 필터링하여 SearchResult 목록으로 변환
    private List<SearchResult> DataTableToList(DataTable dt, string source)
    {
        var list = new List<SearchResult>();
        foreach (DataRow row in dt.Rows)
        {
            if (row["Source"].ToString() != source) continue;
            list.Add(new SearchResult
            {
                Title       = row["Title"].ToString(),
                Url         = row["Url"].ToString(),
                Description = row["Description"].ToString(),
                Source      = source
            });
        }
        return list;
    }

    // SearchResult 목록을 SaveResultCache가 요구하는 DataTable 형식으로 변환
    private DataTable ResultsToDataTable(List<SearchResult> results)
    {
        var dt = new DataTable();
        dt.Columns.Add("Title");
        dt.Columns.Add("Url");
        dt.Columns.Add("Description");
        dt.Columns.Add("Source");
        foreach (var r in results)
            dt.Rows.Add(r.Title ?? "", r.Url ?? "", r.Description ?? "", r.Source ?? "");
        return dt;
    }

    // SearchResult 목록을 소문자 키 JSON 배열로 직렬화 (JavaScript에서 item.title 등으로 접근)
    private string ToJson(List<SearchResult> list)
    {
        var ser = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
        // 소문자 키로 변환: JavaScriptSerializer는 프로퍼티 이름을 그대로 직렬화하므로
        // 명시적 Dictionary로 소문자 키 보장
        // img: 다음(카카오) API가 반환하는 thumbnail URL (없으면 빈 문자열)
        var output = new List<Dictionary<string, string>>();
        foreach (var r in list)
        {
            output.Add(new Dictionary<string, string>
            {
                { "title", r.Title       ?? "" },
                { "url",   r.Url         ?? "" },
                { "desc",  r.Description ?? "" },
                { "src",   r.Source      ?? "" },
                { "img",   r.ImageUrl    ?? "" }
            });
        }
        return ser.Serialize(output);
    }

    /// <summary>
    /// URL에서 og:image 또는 twitter:image 메타태그 값을 추출한다.
    /// 타임아웃 2초로 UI 지연 최소화. 실패·비HTML 응답·상대경로 → 빈 문자열 반환.
    /// </summary>
    private static string FetchOgImage(string url)
    {
        try
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method                       = "GET";
            req.Timeout                      = 2000;  // 2초: 느린 사이트는 건너뜀
            req.UserAgent                    = "Mozilla/5.0 (compatible; Googlebot/2.1)";
            req.AllowAutoRedirect            = true;
            req.MaximumAutomaticRedirections = 3;
            req.AutomaticDecompression       = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Accept                       = "text/html";
            req.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9");

            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                // HTML이 아닌 응답(이미지·PDF 등)은 무시
                if (!(resp.ContentType ?? "").Contains("text/html")) return "";

                // og:image는 <head>에 위치하므로 처음 50KB만 읽어 처리
                using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    char[] buf  = new char[51200];
                    int    len  = reader.Read(buf, 0, buf.Length);
                    string html = new string(buf, 0, len);

                    // property/name 속성 순서에 무관하게 og:image·twitter:image 모두 시도
                    var patterns = new[]
                    {
                        @"<meta[^>]+property=[""']og:image[""'][^>]+content=[""']([^""']+)[""']",
                        @"<meta[^>]+content=[""']([^""']+)[""'][^>]+property=[""']og:image[""']",
                        @"<meta[^>]+name=[""']twitter:image[""'][^>]+content=[""']([^""']+)[""']",
                        @"<meta[^>]+content=[""']([^""']+)[""'][^>]+name=[""']twitter:image[""']"
                    };
                    foreach (string pat in patterns)
                    {
                        var m = Regex.Match(html, pat, RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            string imgUrl = m.Groups[1].Value.Trim();
                            // 절대 URL만 반환 (상대경로는 도메인 미보유로 표시 불가)
                            if (imgUrl.StartsWith("http")) return imgUrl;
                        }
                    }
                }
            }
        }
        catch { }
        return "";
    }

    // JSON 문자열 이스케이프: 역슬래시·큰따옴표·제어문자 처리
    private static string JsonEscapeStr(string s)
    {
        if (s == null) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    /// <summary>
    /// 키워드 카테고리에 따라 이번 검색에서 적립할 엔진별 점수 델타를 반환한다.
    /// 대표 엔진 +0.1, 나머지 -0.05 (하한 0은 ApplyEngineScore SQL에서 보장).
    /// 반환 순서: [googleDelta, naverDelta, daumDelta]
    /// </summary>
    private static double[] KeywordToScoreDelta(string keyword)
    {
        const double W =  0.10;  // 승자 포인트
        const double L = -0.05;  // 패자 포인트

        switch (keyword)
        {
            // 국내 특화 → Naver 승
            case "국내장소": case "국내맛집":
            case "국내뉴스": case "쇼핑": case "의료건강":
                return new double[] { L, W, L };

            // 해외·글로벌·기술 → Google 승
            case "해외장소": case "해외맛집":
            case "해외뉴스": case "IT기술":
                return new double[] { W, L, L };

            // 오락·생활 → Daum 승
            case "차량": case "게임": case "스포츠":
                return new double[] { L, L, W };

            // 일반: 판단 불가 → 점수 변동 없음
            default:
                return new double[] { 0, 0, 0 };
        }
    }

    /// <summary>
    /// 로그인 유저라면 누적 엔진 점수로 개인화 가중치를 계산하고,
    /// 비로그인·신규 유저라면 키워드 기반 고정 점수를 반환한다.
    /// 결과는 weightedMix()에 전달되는 [구글, 네이버, 다음] 정수 배열이다.
    /// </summary>
    private int[] BuildEngineScores(string keyword)
    {
        if (Session["UserID"] != null)
        {
            double gScore, nScore, dScore;
            try { new MemberDao().GetEngineScores(Session["UserID"].ToString(), out gScore, out nScore, out dScore); }
            catch { gScore = 0; nScore = 0; dScore = 0; }

            double total = gScore + nScore + dScore;
            // 새 시스템: 검색 1회(0.1점)만 있어도 개인화 적용
            if (total >= 0.1)
            {
                // 누적 점수 비율 → 1~10 정수 스케일로 변환
                return new int[]
                {
                    Math.Max(1, (int)Math.Round(gScore / total * 10)),
                    Math.Max(1, (int)Math.Round(nScore / total * 10)),
                    Math.Max(1, (int)Math.Round(dScore / total * 10))
                };
            }
            // 검색 이력 부족(신규 유저): 키워드 기반 고정 점수
        }
        return SearchScoreTable.GetScores(keyword);
    }
}
