using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

/// <summary>
/// 카카오 검색 API를 사용하는 다음(Daum) 검색 크롤러.
/// web(웹문서) + blog(블로그) 혼합 검색으로 뉴스·맛집 모두 커버.
/// /web  엔드포인트: GET https://dapi.kakao.com/v2/search/web   ← 뉴스·공식사이트 강점
/// /blog 엔드포인트: GET https://dapi.kakao.com/v2/search/blog  ← 맛집·여행 후기 강점
/// 인증 헤더: Authorization: KakaoAK {REST_API_KEY}
/// 무료 한도: 일 300,000건
/// </summary>
public static class DaumCrawler
{
    // Web.config의 AppSettings에서 카카오 REST API 키를 읽어옴
    private static readonly string ApiKey = WebConfigurationManager.AppSettings["KakaoApiKey"];

    /// <summary>
    /// 검색어를 카카오 검색 API에 전송하여 결과 목록을 반환한다.
    /// web(웹문서) 절반 + blog(블로그) 절반을 혼합하여 폭넓은 쿼리 유형 커버.
    /// API 호출 실패나 파싱 오류 시 빈 목록을 반환 (예외 전파 없음).
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="count">가져올 총 결과 수 (기본 10 → web 5 + blog 5)</param>
    /// <param name="page">페이지 번호 (web 기준, 1-based)</param>
    public static List<SearchResult> Search(string query, int count = 10, int page = 1)
    {
        // web 절반 + blog 절반으로 나눠 가져온 뒤 병합
        int half   = count / 2;    // 5
        int remain = count - half; // 5

        var webResults  = FetchEndpoint("web",  query, half,   page);
        var blogResults = FetchEndpoint("blog", query, remain, page);

        // URL 기준 중복 제거 후 병합 (web 결과 우선 배치)
        var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var merged = new List<SearchResult>();

        foreach (var r in webResults)
        {
            if (!string.IsNullOrEmpty(r.Url) && seen.Add(r.Url))
                merged.Add(r);
        }
        foreach (var r in blogResults)
        {
            if (!string.IsNullOrEmpty(r.Url) && seen.Add(r.Url))
                merged.Add(r);
        }

        return merged;
    }

    // 지정한 카카오 검색 엔드포인트(web 또는 blog)를 호출하여 결과 반환
    // endpoint: "web" | "blog"
    private static List<SearchResult> FetchEndpoint(string endpoint, string query, int count, int page)
    {
        var results = new List<SearchResult>();
        if (count <= 0) return results;
        try
        {
            // web/blog 모두 query/size/page 파라미터 사용
            string url = string.Format(
                "https://dapi.kakao.com/v2/search/{0}?query={1}&size={2}&page={3}",
                endpoint, Uri.EscapeDataString(query), count, page);

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method                 = "GET";
            req.Timeout                = 5000;  // 5초 타임아웃
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            // 카카오 API는 "KakaoAK {키}" 형식의 Authorization 헤더 사용
            req.Headers.Add("Authorization", "KakaoAK " + ApiKey);
            req.Accept = "application/json";

            using (var resp   = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                results = ParseDocuments(json, endpoint);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("DaumCrawler({0}) 오류: {1}", endpoint, ex.Message));
        }
        return results;
    }

    // 카카오 API JSON 응답을 SearchResult 목록으로 파싱
    // web/blog 모두 { "documents": [ { "title", "url", "contents" } ] } 형식
    // blog에는 thumbnail 필드 추가, web에는 없음
    private static List<SearchResult> ParseDocuments(string json, string endpoint)
    {
        var list = new List<SearchResult>();
        try
        {
            var ser  = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
            var root = ser.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null || !root.ContainsKey("documents")) return list;

            var docs = root["documents"] as object[];
            if (docs == null) return list;

            foreach (object docObj in docs)
            {
                var doc = docObj as Dictionary<string, object>;
                if (doc == null) continue;

                string title     = doc.ContainsKey("title")     ? doc["title"].ToString()     : "";
                string url       = doc.ContainsKey("url")       ? doc["url"].ToString()       : "";
                string contents  = doc.ContainsKey("contents")  ? doc["contents"].ToString()  : "";
                // thumbnail: blog는 이미지 URL 제공, web은 빈 문자열
                string thumbnail = doc.ContainsKey("thumbnail") ? doc["thumbnail"].ToString() : "";

                if (string.IsNullOrEmpty(url)) continue;

                // 카카오 API도 title/contents에 HTML 태그가 포함될 수 있으므로 제거
                list.Add(new SearchResult
                {
                    Title       = StripHtml(title),
                    Url         = url,
                    Description = StripHtml(contents),
                    Source      = "daum",
                    ImageUrl    = thumbnail
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("DaumCrawler JSON 파싱({0}) 오류: {1}", endpoint, ex.Message));
        }
        return list;
    }

    // HTML 태그와 주요 HTML 엔티티를 제거하여 순수 텍스트 반환
    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        string text = Regex.Replace(html, @"<[^>]+>", "");
        text = text.Replace("&lt;",   "<")
                   .Replace("&gt;",   ">")
                   .Replace("&amp;",  "&")
                   .Replace("&quot;", "\"")
                   .Replace("&#39;",  "'")
                   .Replace("&apos;", "'")
                   .Replace("&nbsp;", " ");
        return text.Trim();
    }
}
