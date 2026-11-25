using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MetaSearch.Models;

namespace MetaSearch.Pages;

/// <summary>회원가입 페이지 — 폼 제출 시 DB에 회원 INSERT</summary>
public class RegisterModel : PageModel
{
    private readonly Services.MemberDao _dao;
    private readonly Services.DbService _db;
    public readonly Services.Lang Lang;

    public RegisterModel(Services.MemberDao dao, Services.DbService db, Services.Lang lang)
    {
        _dao = dao;
        _db = db;
        Lang = lang;
    }

    // 입력값 바인딩
    [BindProperty] public MemberDo Input { get; set; } = new();

    // 비밀번호 확인 필드
    [BindProperty] public string PasswdConfirm { get; set; } = "";

    // 오류 메시지
    public string ErrorMessage { get; set; } = "";

    public void OnGet() { }

    /// <summary>회원가입 폼 제출 처리</summary>
    public IActionResult OnPost()
    {
        // 검증 규칙은 로그인 모달의 회원가입 API(/api/account/register)와 같은 Validate를 씀
        string? err = Validate(Input, PasswdConfirm, HttpContext.Session.GetString("EmailVerified"), _db, Lang.Get("reg.fillAll"));
        if (err != null) { ErrorMessage = err; return Page(); }

        // DB에 회원 등록 시도
        int result = _dao.RegisterUser(Input);
        if (result == -1)
        {
            ErrorMessage = Lang.Get("reg.duplicate");
            return Page();
        }

        // 등록 성공 → 홈으로 이동
        TempData["ToastMsg"] = "회원가입이 완료되었습니다. 로그인해주세요.";
        TempData["ToastType"] = "success";
        return Redirect("/metasearch");
    }

    /// <summary>
    /// 회원가입 입력 검증 (페이지 폼·JSON API 공용). 통과하면 null, 아니면 오류 문구.
    /// 통과 시 input의 아이디(소문자)·이름·닉네임(태그 제거)·이메일(공백 제거)을 정리된 값으로 바꿔 둠
    /// </summary>
    public static string? Validate(MemberDo input, string passwdConfirm, string? verifiedEmail, Services.DbService db, string fillAllMsg)
    {
        // 필수값 검증
        if (string.IsNullOrWhiteSpace(input.Userid) || string.IsNullOrWhiteSpace(input.Passwd) ||
            string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Nickname))
            return fillAllMsg;

        // 비밀번호 확인 일치 검증
        if (input.Passwd != passwdConfirm) return "비밀번호가 일치하지 않습니다.";

        // 비밀번호 정책 검증 (8자 이상, 영문+숫자+특수문자 포함)
        if (input.Passwd.Length < 8 ||
            !System.Text.RegularExpressions.Regex.IsMatch(input.Passwd, @"[A-Za-z]") ||
            !System.Text.RegularExpressions.Regex.IsMatch(input.Passwd, @"[0-9]") ||
            !System.Text.RegularExpressions.Regex.IsMatch(input.Passwd, @"[^A-Za-z0-9]"))
            return "비밀번호는 8자 이상, 영문+숫자+특수문자를 포함해야 합니다.";

        // 아이디 소문자 정규화 + 공백 제거, 형식 검사 (영문+숫자+밑줄, 3~20자)
        input.Userid = input.Userid.Trim().ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(input.Userid, @"^[a-z0-9_]{3,20}$"))
            return "아이디는 영문 소문자, 숫자, 밑줄(_)만 사용 가능합니다. (3~20자)";

        // 닉네임·이름 길이 제한 + 태그 제거 (XSS 방지)
        input.Name = StripTags(input.Name.Trim());
        input.Nickname = StripTags(input.Nickname.Trim());
        input.Email = input.Email?.Trim() ?? "";
        if (input.Nickname.Length == 0 || input.Name.Length == 0) return fillAllMsg;
        if (input.Nickname.Length > 20 || input.Name.Length > 20) return "이름/닉네임은 20자 이내로 입력해주세요.";

        // 서버 측 이메일 인증 확인 (세션에 저장된 인증 이메일과 비교)
        string emailLower = input.Email.ToLowerInvariant();
        if (verifiedEmail == null || verifiedEmail != emailLower) return "이메일 인증을 완료해주세요.";

        // 이메일 중복 검사 (서버 측 이중 체크)
        using var conn = db.OpenConnection();
        var emailExists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
            "SELECT userid FROM members WHERE LOWER(email) = @emailLower AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
            new { emailLower });
        return emailExists != null ? "이미 가입된 이메일입니다." : null;
    }

    /// <summary>HTML 태그 제거 (XSS 방지)</summary>
    private static string StripTags(string input) =>
        System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", "");
}
