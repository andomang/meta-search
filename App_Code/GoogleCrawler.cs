using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// DuckDuckGo HTML 엔드포인트를 사용하는 웹 검색 크롤러.
/// 구글 HTML 스크래핑은 CAPTCHA·봇 차단으로 사실상 불가능하므로
/// DuckDuckGo(html.duckduckgo.com/html/)로 교체한다.
/// DuckDuckGo는 봇 차단 없이 안정적으로 HTML을 반환하며
/// Bing 인덱스 기반으로 국내외 검색 품질이 충분하다.
/// 외부에서는 여전히 "google" 탭으로 표시된다(UI 탭 구조 변경 불필요).
/// </summary>
public static class GoogleCrawler
{
    // 일반 크롬 브라우저로 위장하는 User-Agent
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";

    /// <summary>
    /// DuckDuckGo에서 검색 결과를 가져온다.
    /// 실패 시 빈 목록 반환 (예외 전파 없음).
    /// </summary>
    public static List<SearchResult> Search(string query, int count = 10)
    {
        var results = new List<SearchResult>();
        try
        {
            // kl=kr-kr: 한국어 지역 설정, dc=숫자로 페이지 오프셋(추가 결과) 가능
            string url = string.Format(
                "https://html.duckduckgo.com/html/?q={0}&kl=kr-kr",
                Uri.EscapeDataString(query));

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method                 = "GET";
            req.Timeout                = 8000;
            req.UserAgent              = UserAgent;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8");

            string html;
            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                html = reader.ReadToEnd();
            }

            results = ParseDuckDuckGo(html, count);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("GoogleCrawler(DDG) 오류: " + ex.Message);
        }
        return results;
    }

    // DuckDuckGo HTML에서 제목·URL·설명을 추출한다.
    // result__a 클래스의 링크에서 uddg 파라미터를 디코딩하여 실제 URL을 얻는다.
    private static List<SearchResult> ParseDuckDuckGo(string html, int maxCount)
    {
        var results = new List<SearchResult>();

        // result__a: 검색 결과 제목 링크. href의 uddg 파라미터에 실제 URL이 인코딩되어 있음
        var titleMatches = Regex.Matches(html,
            @"<a\s[^>]*class=""result__a""[^>]*href=""([^""]+)""[^>]*>([\s\S]{1,300}?)</a>",
            RegexOptions.IgnoreCase);

        // result__snippet: 결과 요약 설명 텍스트
        var snipMatches = Regex.Matches(html,
            @"<a\s[^>]*class=""result__snippet""[^>]*>([\s\S]{0,500}?)</a>",
            RegexOptions.IgnoreCase);

        int pairs = Math.Min(titleMatches.Count, maxCount);
        for (int i = 0; i < pairs; i++)
        {
            string href  = titleMatches[i].Groups[1].Value;
            string title = StripHtml(titleMatches[i].Groups[2].Value).Trim();
            string desc  = i < snipMatches.Count
                ? StripHtml(snipMatches[i].Groups[1].Value).Trim()
                : "";

            // DDG 리다이렉트 URL(//duckduckgo.com/l/?uddg=...)에서 실제 URL 추출
            string actualUrl = ExtractUddg(href);
            if (string.IsNullOrEmpty(actualUrl) || title.Length < 2) continue;

            results.Add(new SearchResult
            {
                Title       = title,
                Url         = actualUrl,
                Description = desc,
                Source      = "google", // UI 탭("구글")과 호환성 유지
                ImageUrl    = ""        // DDG HTML에는 이미지 없음
            });
        }
        return results;
    }

    // DuckDuckGo 리다이렉트 href에서 uddg 파라미터 값을 디코딩하여 실제 URL 반환.
    // 예: //duckduckgo.com/l/?uddg=https%3A%2F%2Fexample.com&rut=... → https://example.com
    private static string ExtractUddg(string href)
    {
        // HTML 엔티티(&amp;) 복원 후 uddg 파라미터 추출
        string decoded = href.Replace("&amp;", "&");
        var m = Regex.Match(decoded, @"uddg=([^&]+)");
        if (!m.Success) return "";
        try { return Uri.UnescapeDataString(m.Groups[1].Value); }
        catch { return ""; }
    }

    // HTML 태그 제거 + 주요 엔티티 디코딩
    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        string t = Regex.Replace(html, @"<[^>]+>", "");
        return t.Replace("&lt;",   "<").Replace("&gt;",   ">")
                .Replace("&amp;",  "&").Replace("&quot;", "\"")
                .Replace("&#39;",  "'").Replace("&nbsp;", " ").Trim();
    }
}
