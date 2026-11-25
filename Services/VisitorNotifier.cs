using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace MetaSearch.Services;

/// <summary>접속자 텔레그램 알림 — IP당 하루(KST) 1회. 토큰은 환경변수 또는 ~/.telegram.json에서 읽음.</summary>
public class VisitorNotifier
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<VisitorNotifier> _log;
    private readonly ConcurrentDictionary<string, string> _seen = new();
    private readonly string _token;
    private readonly string _chatId;
    private static readonly TimeZoneInfo Kst = FindKst();

    public bool Enabled => !string.IsNullOrEmpty(_token) && !string.IsNullOrEmpty(_chatId);

    public VisitorNotifier(IHttpClientFactory http, ILogger<VisitorNotifier> log)
    {
        _http = http; _log = log;
        _token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        _chatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");
        if (!Enabled)
        {
            foreach (var p in new[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".telegram.json"),
                "/home/domang/.telegram.json" })
            {
                try
                {
                    if (!File.Exists(p)) continue;
                    using var d = JsonDocument.Parse(File.ReadAllText(p));
                    _token = d.RootElement.GetProperty("token").GetString();
                    _chatId = d.RootElement.GetProperty("chat_id").ToString();
                    break;
                }
                catch { }
            }
        }
        _log.LogInformation("방문 알림 {State}", Enabled ? "활성" : "비활성(텔레그램 설정 없음)");
    }

    private static TimeZoneInfo FindKst()
    {
        foreach (var id in new[] { "Asia/Seoul", "Korea Standard Time" })
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }
        return TimeZoneInfo.Utc;
    }

    private static bool IsPrivate(string ip)
    {
        if (ip == "::1" || ip.StartsWith("127.") || ip.StartsWith("10.") || ip.StartsWith("192.168.")) return true;
        if (ip.StartsWith("172.") && int.TryParse(ip.Split('.')[1], out var b) && b >= 16 && b <= 31) return true;
        return ip.StartsWith("fe80") || ip.StartsWith("fc") || ip.StartsWith("fd");
    }

    private static bool IsBot(string ua)
    {
        if (string.IsNullOrEmpty(ua)) return true;
        var u = ua.ToLowerInvariant();
        foreach (var k in new[] { "bot", "spider", "crawl", "uptimerobot", "monitor", "curl/", "wget", "python-requests", "go-http", "slurp", "preview", "facebookexternalhit", "headless" })
            if (u.Contains(k)) return true;
        return false;
    }

    private static string Esc(string s) => (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static string Device(string ua)
    {
        ua ??= "";
        string os = ua.Contains("Android") ? "Android"
            : ua.Contains("iPhone") ? "iPhone"
            : ua.Contains("iPad") ? "iPad"
            : ua.Contains("Windows") ? "Windows"
            : ua.Contains("Mac OS X") ? "macOS"
            : ua.Contains("Linux") ? "Linux" : "기타";
        string br = ua.Contains("Whale") ? "Whale"
            : ua.Contains("SamsungBrowser") ? "삼성 인터넷"
            : (ua.Contains("Edg/") || ua.Contains("EdgA/")) ? "Edge"
            : ua.Contains("OPR/") ? "Opera"
            : (ua.Contains("Chrome/") || ua.Contains("CriOS")) ? "Chrome"
            : ua.Contains("Firefox") ? "Firefox"
            : ua.Contains("Safari") ? "Safari" : "기타";
        string kind = ua.Contains("iPad") ? "📱 태블릿"
            : (ua.Contains("Mobile") || ua.Contains("iPhone") || ua.Contains("Android")) ? "📱 모바일" : "💻 PC";
        return $"{kind} · {os} · {br}";
    }

    /// <summary>방문 처리 (미들웨어에서 호출). IP당 하루 1회만 전송.</summary>
    public void Visit(HttpContext ctx)
    {
        // Cloudflare → Nginx → Kestrel 체인에서 실제 클라이언트 IP 추출
        var ip = ctx.Request.Headers["CF-Connecting-IP"].FirstOrDefault()?.Trim()
              ?? ctx.Request.Headers["X-Real-IP"].FirstOrDefault()?.Trim()
              ?? ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
              ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "";
        if (string.IsNullOrEmpty(ip) || IsPrivate(ip)) return;
        var ua = ctx.Request.Headers.UserAgent.ToString();
        if (IsBot(ua)) return;

        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Kst);
        var day = now.ToString("yyyy-MM-dd");
        if (_seen.TryGetValue(ip, out var d) && d == day) return;
        _seen[ip] = day;
        if (_seen.Count > 5000)
            foreach (var kv in _seen) if (kv.Value != day) _seen.TryRemove(kv.Key, out _);

        var path = ctx.Request.Path + ctx.Request.QueryString.ToString();
        var refr = ctx.Request.Headers.Referer.ToString();
        if (path.Length > 150) path = path[..150] + "…";
        if (refr.Length > 200) refr = refr[..200] + "…";
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "visits");
            if (!Directory.Exists(dir) && Directory.Exists("/home/domang")) dir = "/home/domang/visits";
            Directory.CreateDirectory(dir);
            var rec = JsonSerializer.Serialize(new { t = now.ToString("HH:mm:ss"), ip, path, dev = Device(ua), @ref = refr });
            File.AppendAllText(Path.Combine(dir, day + ".jsonl"), rec + "\n");
        }
        catch { }
        if (!Enabled) return;
        var text = new StringBuilder()
            .Append("👤 <b>새 방문자</b>\n")
            .Append($"🌐 IP: <code>{Esc(ip)}</code>\n")
            .Append($"📄 페이지: {Esc(path)}\n")
            .Append($"🕐 시각: {now:yyyy-MM-dd HH:mm:ss} (KST)\n")
            .Append($"🖥 기기: {Esc(Device(ua))}\n")
            .Append($"🔗 유입: {(string.IsNullOrEmpty(refr) ? "직접 접속" : Esc(refr))}")
            .ToString();

        _ = Task.Run(async () =>
        {
            try
            {
                var c = _http.CreateClient();
                c.Timeout = TimeSpan.FromSeconds(10);
                await c.PostAsync($"https://api.telegram.org/bot{_token}/sendMessage",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["chat_id"] = _chatId, ["text"] = text, ["parse_mode"] = "HTML",
                        ["disable_web_page_preview"] = "true"
                    }));
            }
            catch (Exception e) { _log.LogWarning("방문 알림 실패: {m}", e.Message); }
        });
    }
}
