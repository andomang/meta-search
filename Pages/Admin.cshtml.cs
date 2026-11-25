using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MetaSearch.Services;

namespace MetaSearch.Pages;

/// <summary>
/// 통합 관리자 페이지 (/admin, 예전 /metasearch/admin 주소도 그대로 열림).
/// config AdminUsers 또는 DB isadmin=1 인 계정만 접근. 데이터는 각 관리자 API가 서버에서 다시 권한을 확인함.
/// </summary>
public class AdminModel : PageModel
{
    private readonly DbService _db;
    private readonly IConfiguration _config;

    public AdminModel(DbService db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>로그인한 관리자 아이디 (화면 표시용)</summary>
    public string AdminId { get; private set; } = "";
    /// <summary>아스테라 게임 데이터 스크립트 캐시 무효화용 (1분 단위)</summary>
    public long Bust { get; private set; }

    /// <summary>로그인 안 했으면 통합 로그인으로, 관리자가 아니면 403</summary>
    public IActionResult OnGet()
    {
        string? uid = HttpContext.Session.GetString("UserID");
        if (uid == null) return Redirect("/signin?returnUrl=/admin");
        if (!IsAdmin(uid)) return StatusCode(403, "관리자만 볼 수 있는 페이지입니다.");

        AdminId = uid;
        Bust = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute;
        Response.Headers.CacheControl = "no-store";
        return Page();
    }

    private bool IsAdmin(string uid)
    {
        // 1. config 파일에 등록된 관리자 (설정이 없으면 아무도 아님 — 예전엔 'test'가 기본 관리자였음)
        var cfgAdmins = _config.GetSection("AdminUsers").Get<string[]>() ?? Array.Empty<string>();
        if (cfgAdmins.Any(a => string.Equals(a.Trim(), uid, StringComparison.OrdinalIgnoreCase)))
            return true;

        // 2. DB isadmin 컬럼 확인
        try
        {
            using var conn = _db.OpenConnection();
            return Dapper.SqlMapper.ExecuteScalar<int?>(conn,
                "SELECT isadmin FROM members WHERE userid = @uid", new { uid }) == 1;
        }
        catch { return false; }
    }
}
