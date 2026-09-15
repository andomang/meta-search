using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

/// <summary>
/// 네이버 검색 API(webkr)를 사용하는 웹 검색 크롤러.
/// API 문서: https://developers.naver.com/docs/serviceapi/search/web/web.md
/// 엔드포인트: GET https://openapi.naver.com/v1/search/webkr.json
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
    /// API 호출 실패나 파싱 오류 시 빈 목록을 반환 (예외 전파 없음).
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="count">가져올 결과 수 (최대 100, 기본 10)</param>
    /// <param name="start">시작 인덱스 (1-based, 기본 1). 배치 페이지네이션에 사용.
    /// 배치 2: start=101, 배치 3: start=201 등 100 단위 증가.</param>
    public static List<SearchResult> Search(string query, int count = 10, int start = 1)
    {
        var results = new List<SearchResult>();
        try
        {
            // 네이버 webkr 엔드포인트: 일반 웹 문서 검색
            // start 파라미터로 오프셋 지정 (최대 start=1000)
            string url = string.Format(
                "https://openapi.naver.com/v1/search/webkr.json?query={0}&display={1}&start={2}",
                Uri.EscapeDataString(query), count, start);

            // HttpWebRequest로 타임아웃 제어 (5초)
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method                 = "GET";
            req.Timeout                = 5000;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate; // gzip 자동 해제
            req.Headers.Add("X-Naver-Client-Id",     ClientId);
            req.Headers.Add("X-Naver-Client-Secret", ClientSecret);
            req.Accept = "application/json";

            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                string json = reader.ReadToEnd();
                results = ParseJson(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("NaverCrawler 오류: " + ex.Message);
        }
        return results;
    }

    // 네이버 API JSON 응답을 SearchResult 목록으로 파싱
    // 응답 형식: { "items": [ { "title": "...", "link": "...", "description": "..." } ] }
    private static List<SearchResult> ParseJson(string json)
    {
        var list = new List<SearchResult>();
        try
        {
            var ser  = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
            var root = ser.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null || !root.ContainsKey("items")) return list;

            // items 배열 순회
            var items = root["items"] as object[];
            if (items == null) return list;

            foreach (object itemObj in items)
            {
                var item = itemObj as Dictionary<string, object>;
                if (item == null) continue;

                // 각 필드 추출 (키가 없으면 빈 문자열)
                string title = item.ContainsKey("title")       ? item["title"].ToString()       : "";
                string link  = item.ContainsKey("link")        ? item["link"].ToString()        : "";
                string desc  = item.ContainsKey("description") ? item["description"].ToString() : "";

                if (string.IsNullOrEmpty(link)) continue;

                // 네이버 API는 title/description에 <b>태그로 강조를 표시하므로 제거
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
            System.Diagnostics.Debug.WriteLine("NaverCrawler JSON 파싱 오류: " + ex.Message);
        }
        return list;
    }

    // HTML 태그(<b>, <strong> 등)와 주요 HTML 엔티티를 제거하여 순수 텍스트 반환
    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        // HTML 태그 제거
        string text = Regex.Replace(html, @"<[^>]+>", "");
        // 자주 쓰이는 HTML 엔티티 디코딩
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
