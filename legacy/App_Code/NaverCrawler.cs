using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

/// <summary>
/// 네이버 검색 API를 사용하는 웹 검색 크롤러.
/// webkr(일반 웹문서) + blog(블로그) 혼합 검색으로 국내 쿼리 커버리지 향상.
/// webkr 엔드포인트: GET https://openapi.naver.com/v1/search/webkr.json
/// blog  엔드포인트: GET https://openapi.naver.com/v1/search/blog.json
/// 인증 헤더: X-Naver-Client-Id, X-Naver-Client-Secret
/// 무료 한도: 일 25,000건 / 검색 API
/// </summary>
public static class NaverCrawler
{
    // Web.config의 AppSettings에서 인증 키를 읽어옴
    private static readonly string ClientId     = WebConfigurationManager.AppSettings["NaverClientId"];
    private static readonly string ClientSecret = WebConfigurationManager.AppSettings["NaverClientSecret"];

    /// <summary>
    /// 검색어를 네이버 검색 API에 전송하여 결과 목록을 반환한다.
    /// webkr(웹문서) 절반 + blog(블로그) 절반을 혼합하여 국내 맛집·여행 등에서 경쟁력 확보.
    /// API 호출 실패나 파싱 오류 시 빈 목록을 반환 (예외 전파 없음).
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="count">가져올 총 결과 수 (기본 10 → webkr 5 + blog 5)</param>
    /// <param name="start">시작 인덱스 (webkr 기준, 1-based)</param>
    public static List<SearchResult> Search(string query, int count = 10, int start = 1)
    {
        // webkr 절반 + blog 절반으로 나눠 가져온 뒤 병합
        int half    = count / 2;          // 5
        int remain  = count - half;       // 5 (홀수면 blog에 1개 더)

        var webResults  = FetchEndpoint("webkr", query, half,   start);
        var blogResults = FetchEndpoint("blog",  query, remain, 1);

        // URL 기준 중복 제거 후 병합
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

    // 지정한 네이버 검색 엔드포인트(webkr 또는 blog)를 호출하여 결과 반환
    // endpoint: "webkr" | "blog"
    private static List<SearchResult> FetchEndpoint(string endpoint, string query, int count, int start)
    {
        var results = new List<SearchResult>();
        if (count <= 0) return results;
        try
        {
            // webkr: query/display/start 파라미터 사용
            // blog : query/display/start 파라미터 동일 (start 최대 1000)
            string url = string.Format(
                "https://openapi.naver.com/v1/search/{0}.json?query={1}&display={2}&start={3}",
                endpoint, Uri.EscapeDataString(query), count, start);

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method                 = "GET";
            req.Timeout                = 5000;   // 5초 타임아웃
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Headers.Add("X-Naver-Client-Id",     ClientId);
            req.Headers.Add("X-Naver-Client-Secret", ClientSecret);
            req.Accept = "application/json";

            using (var resp   = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                // blog 엔드포인트는 link 필드 대신 bloggername 등 추가 필드 포함
                // 응답 구조는 동일: { "items": [ { "title", "link", "description" } ] }
                results = ParseItems(json, endpoint);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("NaverCrawler({0}) 오류: {1}", endpoint, ex.Message));
        }
        return results;
    }

    // 네이버 API JSON 응답을 SearchResult 목록으로 파싱
    // webkr/blog 모두 { "items": [ { "title", "link", "description" } ] } 형식
    private static List<SearchResult> ParseItems(string json, string endpoint)
    {
        var list = new List<SearchResult>();
        try
        {
            var ser  = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
            var root = ser.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null || !root.ContainsKey("items")) return list;

            var items = root["items"] as object[];
            if (items == null) return list;

            foreach (object itemObj in items)
            {
                var item = itemObj as Dictionary<string, object>;
                if (item == null) continue;

                string title = item.ContainsKey("title")       ? item["title"].ToString()       : "";
                string link  = item.ContainsKey("link")        ? item["link"].ToString()        : "";
                // blog 엔드포인트는 "posturl" 필드에도 URL이 있을 수 있음
                if (string.IsNullOrEmpty(link) && item.ContainsKey("posturl"))
                    link = item["posturl"].ToString();
                string desc  = item.ContainsKey("description") ? item["description"].ToString() : "";

                if (string.IsNullOrEmpty(link)) continue;

                // 네이버 API는 title/description에 <b>태그 강조 포함 → 제거
                list.Add(new SearchResult
                {
                    Title       = StripHtml(title),
                    Url         = link,
                    Description = StripHtml(desc),
                    Source      = "naver"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("NaverCrawler JSON 파싱({0}) 오류: {1}", endpoint, ex.Message));
        }
        return list;
    }

    // HTML 태그(<b>, <strong> 등)와 주요 HTML 엔티티를 제거하여 순수 텍스트 반환
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
