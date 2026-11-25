using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetaSearch.Pages;

/// <summary>
/// 독립 로그인 페이지 (/signin?returnUrl=/pokemon).
/// 휴대폰에서는 게임의 설정 패널(로그인 칸)이 숨겨져 있어서, 여기서 로그인하고 게임으로 돌아간다.
/// 실제 인증은 /api/astera/login (IP당 시도 횟수 제한 있음).
/// </summary>
public class SignInModel : PageModel
{
    public string? UserId { get; private set; }
    public string UserName { get; private set; } = "";
    public string ReturnUrl { get; private set; } = "/";
    // 처음 보여 줄 화면: login(기본) / register / find  → /signin?view=register 로 회원가입부터 열 수 있음
    public string View { get; private set; } = "login";

    public void OnGet(string? returnUrl, string? view)
    {
        if (view == "register" || view == "find") View = view;
        UserId = HttpContext.Session.GetString("UserID");
        UserName = HttpContext.Session.GetString("UserName") ?? UserId ?? "";
        // 열린 리다이렉트 방지: 같은 사이트 경로(/로 시작, //·\ 제외)만 허용
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") && !returnUrl.Contains('\\'))
            ReturnUrl = returnUrl;
    }
}
