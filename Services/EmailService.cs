using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace MetaSearch.Services;

/// <summary>이메일 인증 서비스 — Brevo API로 인증코드 발송 (300통/일 무료).</summary>
public class EmailService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly ILogger<EmailService> _log;

    // 인증코드 임시 저장 (이메일 → 코드+만료시각+틀린 횟수)
    private static readonly ConcurrentDictionary<string, (string Code, DateTime Expires, int Fails)> _codes = new();
    // 코드 하나당 틀려도 되는 횟수 — 넘으면 코드를 버림 (6자리를 무작정 대입해 남의 비밀번호를 바꾸는 것 방지)
    private const int MAX_FAILS = 5;

    public EmailService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<EmailService> log)
    {
        _config = config;
        _http = httpFactory.CreateClient();
        _log = log;
    }

    /// <summary>이메일 용도별 제목·본문 생성</summary>
    private static (string Subject, string Body) GetEmailContent(string purpose, string code)
    {
        return purpose switch
        {
            // 회원가입 인증
            "register" => (
                "[Meta Search] 회원가입 인증 코드",
                $"Meta Search 회원가입 인증 코드입니다.\n\n인증 코드: {code}\n\n5분 이내에 입력해주세요."
            ),
            // 비밀번호 변경/찾기
            "password" => (
                "[Meta Search] 비밀번호 변경 인증 코드",
                $"Meta Search 비밀번호 변경을 위한 인증 코드입니다.\n\n인증 코드: {code}\n\n본인이 요청하지 않았다면 이 메일을 무시하세요.\n5분 이내에 입력해주세요."
            ),
            // 아이디 찾기
            "find-id" => (
                "[Meta Search] 아이디 찾기 인증 코드",
                $"Meta Search 아이디 찾기를 위한 인증 코드입니다.\n\n인증 코드: {code}\n\n5분 이내에 입력해주세요."
            ),
            // 기본값
            _ => (
                "[Meta Search] 인증 코드",
                $"Meta Search 인증 코드입니다.\n\n인증 코드: {code}\n\n5분 이내에 입력해주세요."
            )
        };
    }

    /// <summary>6자리 인증코드 생성 후 Brevo API로 발송 (5분 유효). purpose: register, password, find-id</summary>
    public async Task<bool> SendVerificationCodeAsync(string toEmail, string purpose = "register")
    {
        try
        {
            // 예측할 수 없는 난수로 생성 (Random.Shared는 보안용 난수가 아님)
            string code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            foreach (var old in _codes.Where(x => x.Value.Expires < DateTime.UtcNow).Select(x => x.Key).ToList()) _codes.TryRemove(old, out _);   // 만료된 코드 정리
            _codes[toEmail.ToLowerInvariant()] = (code, DateTime.UtcNow.AddMinutes(5), 0);

            string apiKey = _config["Brevo:ApiKey"] ?? "";
            string senderEmail = _config["Brevo:SenderEmail"] ?? "";
            string senderName = _config["Brevo:SenderName"] ?? "Meta Search";

            var (subject, body) = GetEmailContent(purpose, code);

            // Brevo Transactional Email API
            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail } },
                subject,
                textContent = body
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode) return true;
            // 실패 이유를 로그에 남김 (예: 401 등록되지 않은 IP, 일일 발송 한도 초과). API 키는 기록하지 않음
            string reason = await response.Content.ReadAsStringAsync();
            if (reason.Length > 500) reason = reason[..500];
            _log.LogWarning("[이메일] Brevo 발송 실패 {Status}: {Reason}", (int)response.StatusCode, reason);
            return false;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "[이메일] Brevo 요청 중 오류");
            return false;
        }
    }

    /// <summary>인증코드 검증 (일치하면 true, 사용 후 삭제)</summary>
    public bool VerifyCode(string email, string code)
    {
        string key = email.ToLowerInvariant();
        if (!_codes.TryGetValue(key, out var entry)) return false;
        if (DateTime.UtcNow > entry.Expires)
        {
            _codes.TryRemove(key, out _);
            return false;
        }
        if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(entry.Code), System.Text.Encoding.UTF8.GetBytes(code.Trim())))
        {
            // 틀린 횟수 누적 → 한도를 넘으면 코드를 버려 새로 받아야 함
            if (entry.Fails + 1 >= MAX_FAILS) _codes.TryRemove(key, out _);
            else _codes.TryUpdate(key, entry with { Fails = entry.Fails + 1 }, entry);
            return false;
        }

        _codes.TryRemove(key, out _);
        return true;
    }
}
