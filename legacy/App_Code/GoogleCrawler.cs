using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

/// <summary>
/// Serper.dev API를 사용하는 구글 검색 크롤러.
/// 가입 시 2,500 무료 크레딧 제공, 실제 구글 검색 결과 반환.
/// </summary>
public static class GoogleCrawler
{
    // Serper.dev API 키 (Dashboard에서 발급)
    private const string ApiKey = "f8658f4353d2ecc4db97c01f5f57701109bb5491";

    /// <summary>
    /// Serper.dev로 구글 검색 결과를 가져온다.
    /// 실패 시 빈 목록 반환 (예외 전파 없음).
    /// </summary>
    public static List<SearchResult> Search(string query, int count = 10)
    {
        var results = new List<SearchResult>();
        try
        {
            var req = (HttpWebRequest)WebRequest.Create("https://google.serper.dev/search");
            req.Method      = "POST";
            req.ContentType = "application/json";
            req.Timeout     = 8000;
            req.Headers.Add("X-API-KEY", ApiKey);
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            // 한국 지역(gl=kr), 한국어(hl=ko) 설정
            string body = string.Format(
                "{{\"q\":\"{0}\",\"gl\":\"kr\",\"hl\":\"ko\",\"num\":{1}}}",
                query.Replace("\"", "\\\""),
                Math.Min(count, 100));

            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
            req.ContentLength = bodyBytes.Length;
            using (var stream = req.GetRequestStream())
                stream.Write(bodyBytes, 0, bodyBytes.Length);

            string json;
            using (var resp   = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                json = reader.ReadToEnd();

            results = ParseSerperJson(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("GoogleCrawler(Serper) 오류: " + ex.Message);
        }
        return results;
    }

    // Serper.dev JSON 응답 파서.
    // organic[] 배열에서 title, link, snippet, imageUrl 추출.
    private static List<SearchResult> ParseSerperJson(string json)
    {
        var results = new List<SearchResult>();
        try
        {
            var ser  = new JavaScriptSerializer();
            var root = ser.Deserialize<Dictionary<string, object>>(json);

            if (!root.ContainsKey("organic")) return results;

            var organic = root["organic"] as System.Collections.ArrayList;
            if (organic == null) return results;

            foreach (Dictionary<string, object> item in organic)
            {
                string title   = Get(item, "title");
                string link    = Get(item, "link");
                string snippet = Get(item, "snippet");
                string imgUrl  = Get(item, "imageUrl");

                if (string.IsNullOrEmpty(link) || string.IsNullOrEmpty(title)) continue;

                results.Add(new SearchResult
                {
                    Title       = title,
                    Url         = link,
                    Description = snippet,
                    Source      = "google",
                    ImageUrl    = imgUrl
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("ParseSerperJson 오류: " + ex.Message);
        }
        return results;
    }

    // Dictionary에서 키 값을 안전하게 문자열로 꺼냄
    private static string Get(Dictionary<string, object> d, string key)
    {
        object val;
        return d.TryGetValue(key, out val) && val != null ? val.ToString() : "";
    }
}
