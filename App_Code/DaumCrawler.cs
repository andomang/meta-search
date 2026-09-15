using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;

/// <summary>
/// 카카오 블로그 검색 API를 사용하는 다음(Daum) 검색 크롤러.
/// API 문서: https://developers.kakao.com/docs/latest/ko/daum-search/dev-guide
/// 엔드포인트: GET https://dapi.kakao.com/v2/search/blog  ← /web 대신 /blog 사용
/// /web 엔드포인트는 thumbnail 필드를 반환하지 않음.
/// /blog 엔드포인트는 thumbnail 필드를 포함하여 이미지 표시 가능.
/// 인증 헤더: Authorization: KakaoAK {REST_API_KEY}
/// 무료 한도: 일 300,000건
/// </summary>
public static class DaumCrawler
{
    // Web.config의 AppSettings에서 카카오 REST API 키를 읽어옴
    private static readonly string ApiKey = WebConfigurationManager.AppSettings["KakaoApiKey"];

    /// <summary>
    /// 검색어를 카카오 검색 API에 전송하여 결과 목록을 반환한다.
    /// API 호출 실패나 파싱 오류 시 빈 목록을 반환 (예외 전파 없음).
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="count">가져올 결과 수 (최대 50, 기본 10)</param>
    /// <param name="page">페이지 번호 (1-based, 기본 1). 배치 페이지네이션에 사용.
    /// 배치 1: page=1, 배치 2: page=2 등 1 단위 증가. 최대 page=50.</param>
    public static List<SearchResult> Search(string query, int count = 10, int page = 1)
    {
        var results = new List<SearchResult>();
        try
        {
            // 카카오 블로그 검색 엔드포인트 (web보다 thumbnail 필드 제공됨)
            string url = string.Format(
                "https://dapi.kakao.com/v2/search/blog?query={0}&size={1}&page={2}",
                Uri.EscapeDataString(query), count, page);

            // HttpWebRequest로 타임아웃 제어 (5초)
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method                 = "GET";
            req.Timeout                = 5000;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate; // gzip 자동 해제
            // 카카오 API는 "KakaoAK {키}" 형식의 Authorization 헤더 사용
            req.Headers.Add("Authorization", "KakaoAK " + ApiKey);
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
            System.Diagnostics.Debug.WriteLine("DaumCrawler 오류: " + ex.Message);
        }
        return results;
    }

    // 카카오 API JSON 응답을 SearchResult 목록으로 파싱
    // 응답 형식: { "documents": [ { "title": "...", "url": "...", "contents": "..." } ] }
    private static List<SearchResult> ParseJson(string json)
    {
        var list = new List<SearchResult>();
        try
        {
            var ser  = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
            var root = ser.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null || !root.ContainsKey("documents")) return list;

            // documents 배열 순회
            var docs = root["documents"] as object[];
            if (docs == null) return list;

            foreach (object docObj in docs)
            {
                var doc = docObj as Dictionary<string, object>;
                if (doc == null) continue;

                // 각 필드 추출 (카카오는 description 대신 contents, 이미지는 thumbnail 사용)
                string title     = doc.ContainsKey("title")     ? doc["title"].ToString()     : "";
                string url       = doc.ContainsKey("url")       ? doc["url"].ToString()       : "";
                string contents  = doc.ContainsKey("contents")  ? doc["contents"].ToString()  : "";
                // thumbnail: 결과 페이지의 대표 이미지 URL (없으면 빈 문자열)
                string thumbnail = doc.ContainsKey("thumbnail") ? doc["thumbnail"].ToString() : "";

                if (string.IsNullOrEmpty(url)) continue;

                // 카카오 API도 title/contents에 HTML 태그가 포함될 수 있으므로 제거
                list.Add(new SearchResult
                {
                    Title       = StripHtml(title),
                    Url         = url,
                    Description = StripHtml(contents),
                    Source      = "daum",
                    ImageUrl    = thumbnail  // 다음만 썸네일 제공
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("DaumCrawler JSON 파싱 오류: " + ex.Message);
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
