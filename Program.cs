// ASP.NET Core 8 Razor Pages 앱 진입점
// MSSQL → PostgreSQL + Dapper, 쿠키 인증 + 세션 기반

using MetaSearch;
using MetaSearch.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// 대용량 파일 업로드 허용 (Kestrel 요청 본문 제한 해제)
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = null);

// ── 서비스 등록 ──────────────────────────────────────────────────────────
// Razor Pages
builder.Services.AddRazorPages(options =>
{
    // 포트폴리오 경로 구조: /metasearch/... 하위 라우트 별칭
    options.Conventions.AddPageRoute("/Community", "/metasearch/community");
    options.Conventions.AddPageRoute("/CommunityView", "/metasearch/community/view");
    options.Conventions.AddPageRoute("/CommunityWrite", "/metasearch/community/write");
    options.Conventions.AddPageRoute("/CommunityEdit", "/metasearch/community/edit");
    options.Conventions.AddPageRoute("/SearchResults", "/metasearch/results");
    options.Conventions.AddPageRoute("/Bookmarks", "/metasearch/bookmarks");
    options.Conventions.AddPageRoute("/Dashboard", "/metasearch/dashboard");
    options.Conventions.AddPageRoute("/Settings", "/metasearch/settings");
    options.Conventions.AddPageRoute("/Login", "/metasearch/login");
    options.Conventions.AddPageRoute("/Register", "/metasearch/register");
    options.Conventions.AddPageRoute("/EvalReport", "/metasearch/report");
    options.Conventions.AddPageRoute("/Eval", "/metasearch/eval");
    options.Conventions.AddPageRoute("/Admin", "/metasearch/admin");
    options.Conventions.AddPageRoute("/FindAccount", "/metasearch/find-account");
});

// 세션 (Web Forms Session 대체) — 쿠키 보안 강화
builder.Services.AddDistributedMemoryCache();
// 자동 로그인 쿠키 서명용 키: 고정 폴더에 저장해 서버를 재시작해도 기존 쿠키가 유효하게 유지
builder.Services.AddDataProtection()
    .SetApplicationName("domang")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".domang-keys")));

builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;           // JS에서 세션 쿠키 접근 차단
    opt.Cookie.IsEssential = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS에서만 전송
    opt.Cookie.SameSite = SameSiteMode.Lax;               // CSRF 방지
});

// HttpContextAccessor (Lang 등에서 세션 접근용)
builder.Services.AddHttpContextAccessor();

// DB 연결 팩토리 (Dapper + SqlClient)
builder.Services.AddSingleton<DbService>();

// DAO 서비스
builder.Services.AddScoped<MemberDao>();
builder.Services.AddScoped<SearchDao>();

// API 호출 수 카운팅 (Serper·Brave 무료 티어 한도 관리)
builder.Services.AddSingleton<ApiUsageService>();

// HTTP 크롤러·API 서비스
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GoogleCrawler>();
builder.Services.AddSingleton<NaverCrawler>();
builder.Services.AddSingleton<DaumCrawler>();
builder.Services.AddSingleton<BraveCrawler>();
builder.Services.AddSingleton<ClaudeApi>();
builder.Services.AddSingleton<ExtraApi>();

// 클라우드 드라이브 서비스
builder.Services.AddSingleton<DriveService>();

// 실시간 접속 IP 추적
builder.Services.AddSingleton<ConnectionTracker>();

// 접속자 텔레그램 알림 (IP당 하루 1회)
builder.Services.AddSingleton<VisitorNotifier>();

// 이메일 인증 서비스 (SMTP)
builder.Services.AddSingleton<EmailService>();

// 다국어 텍스트 서비스
builder.Services.AddSingleton<Lang>();

// ── 앱 빌드 ──────────────────────────────────────────────────────────────
// 응답 압축: 게임 스크립트·데이터·API(JSON)·글꼴만 (HTML 페이지는 제외 — 세션/토큰이 섞인 페이지 압축 공격(BREACH) 방지)
//   Cloudflare가 한 번 받아 캐시하면 다음부터는 라즈베리파이가 다시 압축할 일이 거의 없음
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    o.MimeTypes = new[] { "application/javascript", "text/javascript", "application/json", "text/css",
        "font/ttf", "application/x-font-ttf", "font/otf", "application/vnd.ms-fontobject", "image/svg+xml" };
});
// 라즈베리파이 CPU 부담을 줄이려고 빠른 압축 (게임 묶음 파일은 배포 때 최고 압축으로 미리 만들어 둠)
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

var app = builder.Build();
app.UseResponseCompression();

// 옛 메타서치 주소 → /metasearch/... 로 영구 이동(301): 한 페이지에 주소가 둘이라 구글이 '대체 페이지'로 색인을 빼던 문제
//   페이지를 여는 GET/HEAD만 옮김 (POST 폼 전송은 301에서 내용이 사라지므로 그대로 둠), 쿼리(?no= 등)는 유지
var legacyRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["/SearchResults"] = "/metasearch/results", ["/Community"] = "/metasearch/community",
    ["/CommunityView"] = "/metasearch/community/view", ["/CommunityWrite"] = "/metasearch/community/write",
    ["/CommunityEdit"] = "/metasearch/community/edit", ["/Bookmarks"] = "/metasearch/bookmarks",
    ["/Dashboard"] = "/metasearch/dashboard", ["/Settings"] = "/metasearch/settings",
    ["/Login"] = "/metasearch/login", ["/Register"] = "/metasearch/register",
    ["/FindAccount"] = "/metasearch/find-account", ["/EvalReport"] = "/metasearch/report",
    ["/Eval"] = "/metasearch/eval", ["/Index"] = "/metasearch", ["/Search"] = "/metasearch/results",
};
app.Use(async (ctx, next) =>
{
    if ((HttpMethods.IsGet(ctx.Request.Method) || HttpMethods.IsHead(ctx.Request.Method))
        && ctx.Request.Path.HasValue && legacyRoutes.TryGetValue(ctx.Request.Path.Value!.TrimEnd('/'), out var to))
    {
        ctx.Response.Redirect(to + ctx.Request.QueryString, permanent: true);
        return;
    }
    await next();
});

// 드라이브 스키마 마이그레이션 + 기본 계정 생성
// DB가 앱보다 늦게 시작될 수 있으므로 재시도 포함
// 검색 기록 7일 자동 삭제 (회의 결정사항 / 개인정보처리방침·검색 안내 문구와 일치)
//   SearchDao.CleanupOldRecords(): 7일 넘은 검색 기록·검색 결과 캐시 삭제
//   예전엔 함수만 있고 아무 데서도 호출하지 않아 실제로는 지워지지 않았음 → 서버 시작 1분 뒤 1번, 이후 24시간마다
_ = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromMinutes(1));   // DB가 서버보다 늦게 준비될 수 있으므로 잠시 기다림
    while (true)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<SearchDao>().CleanupOldRecords();
            Console.WriteLine("[정리] 7일 지난 검색 기록·캐시 삭제 완료");
        }
        catch (Exception ex) { Console.WriteLine("[정리] 검색 기록 정리 실패: " + ex.Message); }
        await Task.Delay(TimeSpan.FromHours(24));
    }
});

var drv = app.Services.GetRequiredService<DriveService>();
_ = Task.Run(async () =>
{
    // 최대 5회 재시도 (2초 간격) — systemd 서비스 순서에 따라 DB 준비 지연 가능
    for (int attempt = 0; attempt < 5; attempt++)
    {
        try
        {
            drv.EnsureSchema();
            Console.WriteLine("[Drive] 스키마 마이그레이션 완료 (시도 " + (attempt + 1) + ")");
            return;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[Drive] 스키마 초기화 실패 (시도 " + (attempt + 1) + "): " + ex.Message);
            await Task.Delay(2000);
        }
    }
});

// Meta DB 마이그레이션 — members 테이블에 isadmin 컬럼 자동 추가 (0=일반, 1=관리자)
_ = Task.Run(async () =>
{
    for (int attempt = 0; attempt < 5; attempt++)
    {
        try
        {
            using var conn = app.Services.GetRequiredService<DbService>().OpenConnection();
            Dapper.SqlMapper.Execute(conn,
                "ALTER TABLE members ADD COLUMN IF NOT EXISTS isadmin INT NOT NULL DEFAULT 0");
            Console.WriteLine("[Meta] isadmin 컬럼 마이그레이션 완료");
            return;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[Meta] isadmin 마이그레이션 실패 (시도 " + (attempt + 1) + "): " + ex.Message);
            await Task.Delay(2000);
        }
    }
});

// 포켓몬 아스테라 회원 데이터 테이블 (회원별·종류별 JSON 한 덩어리: save=게임 기록, cfg=배속/키 설정, opts=음량 등 옵션)
_ = Task.Run(async () =>
{
    for (int attempt = 0; attempt < 5; attempt++)
    {
        try
        {
            using var conn = app.Services.GetRequiredService<DbService>().OpenConnection();
            Dapper.SqlMapper.Execute(conn, @"
                CREATE TABLE IF NOT EXISTS astera_data (
                    userid     VARCHAR(50) NOT NULL,
                    kind       VARCHAR(16) NOT NULL,
                    data       JSONB       NOT NULL,
                    updated_at TIMESTAMP   NOT NULL DEFAULT NOW(),
                    PRIMARY KEY (userid, kind)
                )");
            // 이상한 소포: 관리자가 만든 소포(포켓몬/도구)를 포켓몬센터 배달원에게서 받음. 계정당 1번
            Dapper.SqlMapper.Execute(conn, @"
                CREATE TABLE IF NOT EXISTS astera_parcels (
                    id         SERIAL       PRIMARY KEY,
                    title      VARCHAR(60)  NOT NULL,
                    message    VARCHAR(400) NOT NULL DEFAULT '',
                    gift       JSONB        NOT NULL,
                    targets    TEXT         NULL,
                    starts_at  TIMESTAMP    NULL,
                    ends_at    TIMESTAMP    NULL,
                    active     BOOLEAN      NOT NULL DEFAULT TRUE,
                    created_by VARCHAR(50)  NOT NULL,
                    created_at TIMESTAMP    NOT NULL DEFAULT NOW()
                );
                CREATE TABLE IF NOT EXISTS astera_parcel_claims (
                    parcel_id  INT          NOT NULL REFERENCES astera_parcels(id) ON DELETE CASCADE,
                    userid     VARCHAR(50)  NOT NULL,
                    claimed_at TIMESTAMP    NOT NULL DEFAULT NOW(),
                    PRIMARY KEY (parcel_id, userid)
                )");
            Console.WriteLine("[Pokemon] 아스테라 데이터 테이블 준비 완료");
            return;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[Pokemon] 세이브 테이블 생성 실패 (시도 " + (attempt + 1) + "): " + ex.Message);
            await Task.Delay(2000);
        }
    }
});

// 게임 코드 버전: 시작 시 게임 파일 내용으로 해시 → 주소에 넣어서 브라우저/Cloudflare 캐시와 섞이지 않게 함
string pokemonRoot = Path.Combine(app.Environment.WebRootPath, "pokemon");
string pokemonVersion = ComputePokemonVersion(pokemonRoot);
string? pokemonIndexHtml = null;

static string InjectPokemonBase(string html, string version, string assetRev)
{
    string tags = $"<base href=\"/pokemon/_v{version}/\"><meta name=\"astera-server\" content=\"1\"><meta name=\"astera-assets\" content=\"{assetRev}\">";
    // 예전 방식(이미 base가 있는 index.html)도 지원
    if (html.Contains("<base href=\"/pokemon/\">")) return html.Replace("<base href=\"/pokemon/\">", tags);
    int head = html.IndexOf("<head>", StringComparison.OrdinalIgnoreCase);
    return head < 0 ? tags + html : html.Insert(head + "<head>".Length, tags);
}

static string ComputePokemonVersion(string root)
{
    using var md5 = System.Security.Cryptography.MD5.Create();
    var files = new List<string>();
    if (File.Exists(Path.Combine(root, "index.html"))) files.Add(Path.Combine(root, "index.html"));
    // 게임 코드 폴더 (아스테라는 game/)
    foreach (var dir in new[] { "game", "src" })
        if (Directory.Exists(Path.Combine(root, dir)))
            files.AddRange(Directory.GetFiles(Path.Combine(root, dir), "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal));
    foreach (var f in files)
    {
        var bytes = File.ReadAllBytes(f);
        md5.TransformBlock(bytes, 0, bytes.Length, null, 0);
    }
    md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
    return Convert.ToHexString(md5.Hash!)[..10].ToLowerInvariant();
}

// /pokemon/_v{버전}/... 요청을 실제 파일 경로 /pokemon/... 로 바꿔서 정적 파일로 서빙
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    var prefix = "/pokemon/_v";
    if (path.StartsWith(prefix, StringComparison.Ordinal))
    {
        int slash = path.IndexOf('/', prefix.Length);
        if (slash > 0)
        {
            ctx.Request.Path = "/pokemon" + path[slash..];
            // 버전이 주소에 들어 있으므로 오래 캐시해도 안전
            ctx.Response.OnStarting(() =>
            {
                ctx.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                return Task.CompletedTask;
            });
        }
    }
    await next();
});

// 미리 압축해 둔 게임 파일(.br/.gz) 서빙: deploy 때 묶음 파일(astera.bundle.js)을 최고 압축으로 만들어 둠
//   요청마다 압축할 필요 없이 바로 보냄 (라즈베리파이 CPU 절약, 전송량 약 1/6)
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    if (path.StartsWith("/pokemon/game/", StringComparison.Ordinal) && path.EndsWith(".js", StringComparison.Ordinal) && !path.Contains(".."))
    {
        string accept = ctx.Request.Headers.AcceptEncoding.ToString();
        string file = Path.Combine(app.Environment.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        foreach (var (enc, ext) in new[] { ("br", ".br"), ("gzip", ".gz") })
        {
            if (!accept.Contains(enc) || !File.Exists(file + ext)) continue;
            ctx.Response.Headers.ContentEncoding = enc;
            ctx.Response.Headers.Vary = "Accept-Encoding";
            ctx.Response.ContentType = "text/javascript; charset=utf-8";
            if (string.IsNullOrEmpty(ctx.Response.Headers.CacheControl)) ctx.Response.Headers.CacheControl = "no-cache";
            await ctx.Response.SendFileAsync(file + ext);
            return;
        }
    }
    await next();
});

// 아스테라 에셋(그래픽/사운드)은 배포 패키지에 넣지 않고 서버의 별도 폴더에서 바로 서빙
// 게임 코드의 에셋 경로 '../Pokemon Another Red_PWT_250821/'가 /pokemon/ 기준으로 풀리므로 그 주소에 연결
// 기본값: 라즈베리파이 SSD (/mnt/ssd/pokemon 아래에 원본 에셋 폴더를 그대로 둠)
string asteraAssetPath = app.Configuration["Astera:AssetPath"] ?? "/mnt/ssd/pokemon/Pokemon Another Red_PWT_250821";
// 에셋 버전: 파일 수 + 가장 최근 수정 시각 → 에셋 주소 뒤에 붙여서(?r=) 예전에 캐시된 응답(404 포함)을 쓰지 않게 함
string asteraAssetRev = "0";
if (Directory.Exists(asteraAssetPath))
{
    var all = new DirectoryInfo(asteraAssetPath).EnumerateFiles("*", SearchOption.AllDirectories).ToList();
    long latest = all.Count == 0 ? 0 : all.Max(f => f.LastWriteTimeUtc.Ticks);
    asteraAssetRev = (all.Count.ToString("x") + latest.ToString("x"))[^10..];
}
if (Directory.Exists(asteraAssetPath))
{
    // 원본 에셋은 Windows에서 만들어져 파일 이름 대소문자/확장자가 코드와 다른 경우가 있음
    // (예: 코드는 Indoor.png, 실제 파일은 Indoor.PNG / 코드는 A_FLOW.png, 실제는 A_FLOW.jpg)
    // 리눅스는 대소문자를 구분하므로, 없는 파일을 요청하면 대소문자 무시 → 확장자 무시 순으로 실제 파일을 찾아 연결
    const string assetPrefix = "/pokemon/Pokemon Another Red_PWT_250821/";
    var exactIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var noExtIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var full in Directory.EnumerateFiles(asteraAssetPath, "*", SearchOption.AllDirectories))
    {
        string rel = Path.GetRelativePath(asteraAssetPath, full).Replace('\\', '/').Normalize(System.Text.NormalizationForm.FormC);
        exactIndex.TryAdd(rel, rel);
        noExtIndex.TryAdd(Path.ChangeExtension(rel, null), rel);
    }
    Console.WriteLine($"[Pokemon] 아스테라 에셋 {exactIndex.Count}개 색인");
    app.Use(async (ctx, next) =>
    {
        string path = ctx.Request.Path.Value ?? "";
        if (path.StartsWith(assetPrefix, StringComparison.Ordinal))
        {
            string rel = path[assetPrefix.Length..].Normalize(System.Text.NormalizationForm.FormC);
            // 경로 조작(..) 방지: 색인에 있는 파일 이름으로만 바꿔줌
            if (!File.Exists(Path.Combine(asteraAssetPath, rel)) &&
                (exactIndex.TryGetValue(rel, out var actual) || noExtIndex.TryGetValue(Path.ChangeExtension(rel, null), out actual)))
            {
                ctx.Request.Path = assetPrefix + actual;
            }
            // 없는 파일의 404를 Cloudflare/브라우저가 캐시하면, 파일을 올린 뒤에도 한동안 안 보임 → 404는 캐시 금지
            ctx.Response.OnStarting(() =>
            {
                if (ctx.Response.StatusCode == 404) ctx.Response.Headers.CacheControl = "no-store";
                return Task.CompletedTask;
            });
        }
        await next();
    });

    var types = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    types.Mappings[".ogg"] = "audio/ogg";
    types.Mappings[".ttf"] = "font/ttf";
    types.Mappings[".mid"] = "audio/midi";
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(asteraAssetPath),
        RequestPath = "/pokemon/Pokemon Another Red_PWT_250821",
        ContentTypeProvider = types,
        // 에셋은 거의 바뀌지 않으므로 7일 캐시
        OnPrepareResponse = c => c.Context.Response.Headers.CacheControl = "public, max-age=604800"
    });
    Console.WriteLine("[Pokemon] 아스테라 에셋 경로: " + asteraAssetPath);
}
else
{
    Console.WriteLine("[Pokemon] 아스테라 에셋 폴더 없음: " + asteraAssetPath);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Nginx 리버스 프록시 뒤에서 실제 클라이언트 IP 읽기
var fwdOpts = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwdOpts.KnownNetworks.Clear();
fwdOpts.KnownProxies.Clear();
app.UseForwardedHeaders(fwdOpts);

// Cloudflare + Nginx 뒤에서 실제 클라이언트 IP를 가져오는 헬퍼
// 우선순위: CF-Connecting-IP → X-Real-IP → X-Forwarded-For 첫 번째 → RemoteIpAddress
static string GetRealIp(HttpContext ctx)
{
    // Cloudflare가 추가하는 실제 클라이언트 IP 헤더
    var cfIp = ctx.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
    if (!string.IsNullOrEmpty(cfIp)) return cfIp.Trim();

    // Nginx가 설정하는 X-Real-IP 헤더
    var realIp = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
    if (!string.IsNullOrEmpty(realIp)) return realIp.Trim();

    // X-Forwarded-For의 첫 번째 IP (원본 클라이언트)
    var xff = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrEmpty(xff))
    {
        var first = xff.Split(',')[0].Trim();
        if (!string.IsNullOrEmpty(first)) return first;
    }

    return ctx.Connection.RemoteIpAddress?.ToString() ?? "";
}

app.UseStaticFiles(new StaticFileOptions
{
    // 게임 코드/데이터는 배포 직후 바로 반영되도록 매번 변경 여부 확인 (안 바뀌었으면 304)
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path;
        var ext = Path.GetExtension(path.Value ?? "").ToLowerInvariant();
        if (path.StartsWithSegments("/pokemon") && (ext == ".js" || ext == ".json" || ext == ".html"))
            ctx.Context.Response.Headers.CacheControl = "no-cache";
    }
});

// 보안 헤더 + 캐시 제어 미들웨어
app.Use(async (ctx, next) =>
{
    // ── 보안 헤더 (모든 응답에 적용) ──
    ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";          // 클릭재킹 방지
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";      // MIME 스니핑 방지
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";     // 브라우저 XSS 필터
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    // HSTS — Nginx에서도 설정하지만 앱 레벨에서도 이중 적용
    ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    // CSP — 인라인 스크립트 허용 (Razor Pages 특성상 필요)
    // CSP 보안 헤더 — 외부 CDN(Tailwind, Lucide 등) 허용 포함
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://unpkg.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com " +
        // 구글 애드센스 (광고 스크립트·광고 서버)
        "https://pagead2.googlesyndication.com https://*.googlesyndication.com https://*.doubleclick.net https://adservice.google.com https://*.google.com https://*.gstatic.com https://*.adtrafficquality.google; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        // 포켓몬 게임용 픽셀 폰트(둥근모꼴)가 jsdelivr에서 오므로 허용
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://cdn.tailwindcss.com https://*.google.com https://*.googlesyndication.com https://*.doubleclick.net https://*.gstatic.com https://*.adtrafficquality.google; " +
        // 광고는 iframe으로 들어옴
        "frame-src 'self' https://*.googlesyndication.com https://*.doubleclick.net https://*.google.com https://*.adtrafficquality.google; " +
        "frame-ancestors 'self'";

    // ── HTML 페이지 캐시 방지 (로그아웃 후 뒤로가기 시 로그인 상태 잔존 방지) ──
    if (!ctx.Request.Path.StartsWithSegments("/lib") &&
        !ctx.Request.Path.StartsWithSegments("/css") &&
        !ctx.Request.Path.StartsWithSegments("/js") &&
        !ctx.Request.Path.StartsWithSegments("/images") &&
        !ctx.Request.Path.StartsWithSegments("/pokemon"))
    {
        ctx.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Response.Headers["Pragma"] = "no-cache";
    }
    await next();
});

app.UseRouting();

// 세션 미들웨어 (UseRouting 뒤, MapRazorPages 앞)
app.UseSession();

app.UseAuthorization();

// 접속 IP 추적 미들웨어
app.Use(async (ctx, next) =>
{
    var tracker = ctx.RequestServices.GetRequiredService<ConnectionTracker>();
    string ip = GetRealIp(ctx);
    if (!string.IsNullOrEmpty(ip)) tracker.Track(ip);
    var p0 = ctx.Request.Path.Value ?? "";
    if (HttpMethods.IsGet(ctx.Request.Method) && !p0.StartsWith("/api", StringComparison.OrdinalIgnoreCase) && !p0.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        ctx.RequestServices.GetRequiredService<VisitorNotifier>().Visit(ctx);
    await next();
});

app.MapRazorPages();

// /pokemon 경로에서 게임 index.html 직접 서빙
app.MapGet("/pokemon", (HttpContext ctx) =>
{
    // 첫 화면은 항상 새로 받고, 그 안의 기준 주소를 현재 버전 주소로 바꿔서 내려줌
    ctx.Response.Headers.CacheControl = "no-cache";
    // 게임 파일의 상대 경로(game/*.js)가 버전 주소 기준으로 풀리도록 <base> 주입
    // astera-server 표시가 있으면 game/server.js가 회원 로그인/서버 저장 모드로 동작
    pokemonIndexHtml ??= InjectPokemonBase(File.ReadAllText(Path.Combine(pokemonRoot, "index.html")), pokemonVersion, asteraAssetRev);
    return Results.Content(pokemonIndexHtml, "text/html; charset=utf-8");
});

// ── 레이트 리미팅 (IP 기반 요청 제한) ────────────────────────────────────────
var _rateLimits = new System.Collections.Concurrent.ConcurrentDictionary<string, (int Count, DateTime ResetAt)>();

// 레이트 리미트 체크 헬퍼: 제한 초과 시 true 반환
int _rateCalls = 0;
bool IsRateLimited(string key, int maxRequests, int windowSeconds)
{
    var now = DateTime.UtcNow;
    // 1000번째 호출마다 시간 창이 끝난 기록을 지움 (새 IP·봇마다 항목이 계속 늘던 문제)
    if (Interlocked.Increment(ref _rateCalls) % 1000 == 0)
        foreach (var old in _rateLimits.Where(x => now > x.Value.ResetAt).Select(x => x.Key).ToList()) _rateLimits.TryRemove(old, out _);
    var entry = _rateLimits.GetOrAdd(key, _ => (0, now.AddSeconds(windowSeconds)));
    if (now > entry.ResetAt)
    {
        _rateLimits[key] = (1, now.AddSeconds(windowSeconds));
        return false;
    }
    if (entry.Count >= maxRequests) return true;
    _rateLimits[key] = (entry.Count + 1, entry.ResetAt);
    return false;
}

// ── 포켓몬 아스테라 회원 API ──
// 사이트 회원 계정(members, BCrypt)으로 로그인하고, 저장 권한은 세션으로만 판단
// (평문 UserID 쿠키는 위조 가능하므로 쓰지 않음)

// 관리자 여부 (members.isadmin = 1)
static bool IsSiteAdmin(DbService db, string uid)
{
    using var conn = db.OpenConnection();
    return Dapper.SqlMapper.ExecuteScalar<int?>(conn, "SELECT isadmin FROM members WHERE userid = @uid", new { uid }) == 1;
}

// 로그인 상태
app.MapGet("/api/pokemon/me", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    ctx.Response.Headers.CacheControl = "no-store";
    bool admin = false;
    if (uid != null) { try { admin = IsSiteAdmin(db, uid); } catch { /* DB 오류 시 일반 회원으로 */ } }
    return Results.Json(new { loggedIn = uid != null, userId = uid, nick = ctx.Session.GetString("UserName"), isAdmin = admin });
});

// 게임 패널에서 로그인 (사이트 로그인과 같은 세션을 만듦)
app.MapPost("/api/astera/login", async (HttpContext ctx, MemberDao dao, DbService db) =>
{
    string ip = GetRealIp(ctx);
    // 비밀번호 대입 방지: IP당 5분에 10회
    if (IsRateLimited("astera-login:" + ip, 10, 300))
        return Results.Json(new { ok = false, error = "로그인 시도가 너무 많습니다. 5분 후 다시 시도해주세요." });

    System.Text.Json.JsonElement body;
    try { body = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(ctx.Request.Body); }
    catch (System.Text.Json.JsonException) { return Results.BadRequest(); }
    string id = (body.TryGetProperty("id", out var i) ? i.GetString() ?? "" : "").Trim().ToLowerInvariant();
    string pw = body.TryGetProperty("pw", out var p) ? p.GetString() ?? "" : "";
    if (id.Length == 0 || pw.Length == 0 || !dao.Authenticate(id, pw))
        return Results.Json(new { ok = false, error = "아이디 또는 비밀번호가 틀렸습니다." });

    string nick = dao.GetNickname(id);
    ctx.Session.SetString("UserID", id);
    ctx.Session.SetString("UserName", nick);
    return Results.Json(new { ok = true, userId = id, nick, isAdmin = IsSiteAdmin(db, id) });
});

// 로그아웃 (세션 + 자동로그인 쿠키 정리)
app.MapPost("/api/astera/logout", (HttpContext ctx) =>
{
    ctx.Session.Clear();
    ctx.Response.Cookies.Delete("UserID");
    return Results.Ok(new { ok = true });
});

// 회원 데이터 종류 (이 외의 값은 거절)
var asteraKinds = new HashSet<string> { "save", "cfg", "opts" };

// 불러오기
app.MapGet("/api/astera/data/{kind}", (string kind, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!asteraKinds.Contains(kind)) return Results.BadRequest();
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var row = Dapper.SqlMapper.QueryFirstOrDefault(conn,
        "SELECT data::text AS data FROM astera_data WHERE userid = @uid AND kind = @kind", new { uid, kind });
    if (row == null) return Results.NotFound();
    return Results.Content((string)row.data, "application/json; charset=utf-8");
});

// 저장 (덮어쓰기)
app.MapPost("/api/astera/data/{kind}", async (string kind, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!asteraKinds.Contains(kind)) return Results.BadRequest();
    if (IsRateLimited("astera-save:" + uid, 60, 60)) return Results.StatusCode(429);

    // 게임 기록은 박스까지 합쳐도 수백 KB 수준. 비정상적으로 큰 요청은 거절
    const int maxBytes = 4 * 1024 * 1024;
    if (ctx.Request.ContentLength is > maxBytes) return Results.StatusCode(413);
    using var reader = new StreamReader(ctx.Request.Body);
    string body = await reader.ReadToEndAsync();
    if (body.Length > maxBytes) return Results.StatusCode(413);
    try
    {
        using var doc = System.Text.Json.JsonDocument.Parse(body);
        if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object) return Results.BadRequest();
    }
    catch (System.Text.Json.JsonException)
    {
        return Results.BadRequest();
    }

    using var conn = db.OpenConnection();
    Dapper.SqlMapper.Execute(conn, @"
        INSERT INTO astera_data (userid, kind, data, updated_at) VALUES (@uid, @kind, CAST(@body AS JSONB), NOW())
        ON CONFLICT (userid, kind) DO UPDATE SET data = EXCLUDED.data, updated_at = NOW()",
        new { uid, kind, body });
    return Results.Ok(new { saved = true });
});

// ── 관리자 선물: 다른 회원에게 포켓몬/도구 보내기 ──
// 받는 사람의 기록을 직접 고치지 않고 "선물함(kind='gifts')"에 쌓아 두면,
// 그 사람이 게임을 켜고 필드에 있을 때 게임이 받아 가서 파티/박스에 넣고 저장한다.
// (플레이 중인 사람의 저장이 서버 수정본을 덮어써 사라지는 문제가 없음)
app.MapPost("/api/astera/admin/gift", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);

    System.Text.Json.JsonElement body;
    try { body = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(ctx.Request.Body); }
    catch (System.Text.Json.JsonException) { return Results.BadRequest(); }
    string to = (body.TryGetProperty("to", out var t) ? t.GetString() ?? "" : "").Trim().ToLowerInvariant();
    if (to.Length == 0 || !body.TryGetProperty("gift", out var gift) || gift.ValueKind != System.Text.Json.JsonValueKind.Object)
        return Results.BadRequest();
    string giftJson = gift.GetRawText();
    if (giftJson.Length > 64 * 1024) return Results.StatusCode(413);

    using var conn = db.OpenConnection();
    // 받는 회원이 실제로 있는지 확인
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(userid) = @to", new { to });
    if (exists == null) return Results.Json(new { ok = false, error = "그런 회원이 없습니다: " + to });

    // 선물함 배열 끝에 추가 (없으면 새로 만듦)
    Dapper.SqlMapper.Execute(conn, @"
        INSERT INTO astera_data (userid, kind, data, updated_at)
        VALUES (@to, 'gifts', jsonb_build_object('list', jsonb_build_array(CAST(@g AS JSONB))), NOW())
        ON CONFLICT (userid, kind) DO UPDATE
        SET data = jsonb_set(astera_data.data, '{list}', COALESCE(astera_data.data->'list', '[]'::jsonb) || jsonb_build_array(CAST(@g AS JSONB))),
            updated_at = NOW()",
        new { to = exists, g = giftJson });
    return Results.Json(new { ok = true, to = exists });
});

// ── 관리자: 회원별 게임 기록 보기/수정 (/pokeadmin 페이지에서 사용) ──
static System.Text.Json.JsonElement JsonOr(string? text, string fallback)
{
    try { return System.Text.Json.JsonDocument.Parse(string.IsNullOrEmpty(text) ? fallback : text).RootElement.Clone(); }
    catch { return System.Text.Json.JsonDocument.Parse(fallback).RootElement.Clone(); }
}

// 회원 목록 + 요약 (진행·돈·파티 등). 기록 전체는 크므로 필요한 값만 DB에서 뽑음
app.MapGet("/api/astera/admin/players", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var rows = Dapper.SqlMapper.Query(conn, @"
        SELECT a.userid, COALESCE(m.nickname, '') AS nick, a.updated_at,
               a.data->>'name' AS pname, (a.data->'badges')::text AS badges, a.data->>'money' AS money,
               a.data->>'playTime' AS playtime, a.data->>'map' AS map, (a.data->'party')::text AS party,
               (SELECT count(*) FROM jsonb_object_keys(CASE WHEN jsonb_typeof(a.data->'caught') = 'object' THEN a.data->'caught' ELSE '{}'::jsonb END)) AS caught,
               (SELECT COALESCE(sum(jsonb_array_length(b)), 0) FROM jsonb_array_elements(CASE WHEN jsonb_typeof(a.data->'boxes') = 'array' THEN a.data->'boxes' ELSE '[]'::jsonb END) AS b) AS boxed,
               (a.data->'flags')::text AS flags
        FROM astera_data a
        LEFT JOIN members m ON LOWER(m.userid) = a.userid
        WHERE a.kind = 'save'
        ORDER BY a.updated_at DESC");
    var list = rows.Select(r => new
    {
        userId = (string)r.userid, nick = (string)r.nick, updatedAt = (DateTime)r.updated_at,
        name = (string?)r.pname, money = (string?)r.money, playTime = (string?)r.playtime, map = (string?)r.map,
        caught = (long)r.caught, boxed = (long)r.boxed,
        badges = JsonOr((string?)r.badges, "[]"), party = JsonOr((string?)r.party, "[]"), flags = JsonOr((string?)r.flags, "{}")
    });
    return Results.Json(list);
});

// 한 회원의 기록 전체
app.MapGet("/api/astera/admin/save/{target}", (string target, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var data = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT data::text FROM astera_data WHERE userid = @t AND kind = 'save'", new { t = target.ToLowerInvariant() });
    return data == null ? Results.NotFound() : Results.Content(data, "application/json; charset=utf-8");
});

// 한 회원의 기록 수정 (전체 덮어쓰기)
//   _savedAt을 지금으로 → 다음에 게임을 켤 때 서버 기록이 이김
//   'adminedit' 시각 기록 → 지금 플레이 중이면 게임이 30초 안에 알아채고 수정본으로 다시 불러옴(덮어쓰기 방지)
app.MapPost("/api/astera/admin/save/{target}", async (string target, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    const int maxBytes = 4 * 1024 * 1024;
    if (ctx.Request.ContentLength is > maxBytes) return Results.StatusCode(413);
    using var reader = new StreamReader(ctx.Request.Body);
    string body = await reader.ReadToEndAsync();
    if (body.Length > maxBytes) return Results.StatusCode(413);
    System.Text.Json.Nodes.JsonObject? obj;
    try { obj = System.Text.Json.Nodes.JsonNode.Parse(body) as System.Text.Json.Nodes.JsonObject; }
    catch (System.Text.Json.JsonException) { return Results.BadRequest(); }
    if (obj == null || obj["party"] is not System.Text.Json.Nodes.JsonArray) return Results.BadRequest();

    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    obj["_savedAt"] = now;
    obj["_adminEdit"] = now;
    string t = target.ToLowerInvariant();
    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM astera_data WHERE userid = @t AND kind = 'save'", new { t });
    if (exists == null) return Results.NotFound();
    Dapper.SqlMapper.Execute(conn, @"
        UPDATE astera_data SET data = CAST(@d AS JSONB), updated_at = NOW() WHERE userid = @t AND kind = 'save';
        INSERT INTO astera_data (userid, kind, data, updated_at) VALUES (@t, 'adminedit', jsonb_build_object('ts', @now), NOW())
        ON CONFLICT (userid, kind) DO UPDATE SET data = EXCLUDED.data, updated_at = NOW();",
        new { t, d = obj.ToJsonString(), now });
    Console.WriteLine($"[Pokemon] 관리자 {uid} 가 {t} 의 기록을 수정");
    return Results.Json(new { ok = true, savedAt = now });
});

// 받을 수 있는 소포 조건: 켜져 있음 + 기간 안(비어 있으면 제한 없음) + 대상(비어 있으면 전원, 아니면 쉼표로 나눈 아이디 목록) + 아직 안 받음
const string parcelAvailableSql = @"
    p.active AND (p.starts_at IS NULL OR p.starts_at <= (NOW() AT TIME ZONE 'UTC')) AND (p.ends_at IS NULL OR p.ends_at > (NOW() AT TIME ZONE 'UTC'))
    AND (p.targets IS NULL OR p.targets = '' OR @uid = ANY(string_to_array(REPLACE(LOWER(p.targets), ' ', ''), ',')))
    AND NOT EXISTS (SELECT 1 FROM astera_parcel_claims c WHERE c.parcel_id = p.id AND c.userid = @uid)";

// ads.txt (애드센스 필수): 설정의 게시자 ID(ca-pub-…)로 자동 생성. ID가 없으면 404
app.MapGet("/ads.txt", (IConfiguration cfg) =>
{
    string client = cfg["AdSense:Client"] ?? "";
    if (!client.StartsWith("ca-pub-")) return Results.NotFound();
    string pub = client.Substring(3);   // "ca-pub-123" → "pub-123"
    return Results.Text($"google.com, {pub}, DIRECT, f08c47fec0942fa0" + "\n", "text/plain");
});

// 게임의 주기 확인을 한 번에: 관리자 수정 시각 + 받을 선물 수 + 받을 소포 수 (DB 조회 1번)
//   예전엔 접속자마다 30초에 요청 2번(선물 받기 시도 + 수정 시각) → 이제 1번, 선물이 있을 때만 받기 요청
app.MapGet("/api/astera/poll", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var r = Dapper.SqlMapper.QueryFirst(conn, @"
        SELECT
          (SELECT data->>'ts' FROM astera_data WHERE userid = @uid AND kind = 'adminedit') AS ts,
          (SELECT COALESCE(jsonb_array_length(data->'list'), 0) FROM astera_data WHERE userid = @uid AND kind = 'gifts') AS gifts,
          (SELECT count(*) FROM astera_parcels p WHERE " + parcelAvailableSql + @") AS parcels",
        new { uid });
    return Results.Json(new
    {
        ts = long.TryParse((string?)r.ts, out var v) ? v : 0,
        gifts = r.gifts == null ? 0 : (int)r.gifts,
        parcels = (long)r.parcels
    });
});

// 내 기록이 관리자에게 수정된 시각 (게임이 30초마다 확인)
app.MapGet("/api/astera/adminedit", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var ts = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT data->>'ts' FROM astera_data WHERE userid = @uid AND kind = 'adminedit'", new { uid });
    return Results.Json(new { ts = long.TryParse(ts, out var v) ? v : 0 });
});

// ── 이상한 소포 ──

// 내가 받을 수 있는 소포 목록 (배달원에게 말 걸 때 / 포켓몬센터에 들어갈 때)
app.MapGet("/api/astera/parcels", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var rows = Dapper.SqlMapper.Query(conn,
        "SELECT p.id, p.title, p.message, p.gift::text AS gift FROM astera_parcels p WHERE " + parcelAvailableSql + " ORDER BY p.id",
        new { uid });
    return Results.Json(rows.Select(r => new { id = (int)r.id, title = (string)r.title, message = (string)r.message, gift = JsonOr((string)r.gift, "{}") }));
});

// 소포 받기: 받은 기록을 먼저 남기고(중복이면 실패) 내용을 돌려줌 → 같은 소포를 두 번 받을 수 없음
app.MapPost("/api/astera/parcels/{id:int}/claim", (int id, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    using var conn = db.OpenConnection();
    var gift = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT p.gift::text FROM astera_parcels p WHERE p.id = @id AND " + parcelAvailableSql, new { id, uid });
    if (gift == null) return Results.Json(new { ok = false, error = "받을 수 없는 소포입니다." });
    int inserted = Dapper.SqlMapper.Execute(conn,
        "INSERT INTO astera_parcel_claims (parcel_id, userid) VALUES (@id, @uid) ON CONFLICT DO NOTHING", new { id, uid });
    if (inserted == 0) return Results.Json(new { ok = false, error = "이미 받은 소포입니다." });
    return Results.Json(new { ok = true, gift = JsonOr(gift, "{}") });
});

// 관리자: 소포 목록 (받은 사람 수 포함)
app.MapGet("/api/astera/admin/parcels", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var rows = Dapper.SqlMapper.Query(conn, @"
        SELECT p.id, p.title, p.message, p.gift::text AS gift, p.targets, p.starts_at, p.ends_at, p.active, p.created_by, p.created_at,
               (SELECT count(*) FROM astera_parcel_claims c WHERE c.parcel_id = p.id) AS claims,
               (SELECT string_agg(c.userid, ', ' ORDER BY c.claimed_at) FROM astera_parcel_claims c WHERE c.parcel_id = p.id) AS claimers
        FROM astera_parcels p ORDER BY p.id DESC");
    return Results.Json(rows.Select(r => new
    {
        id = (int)r.id, title = (string)r.title, message = (string)r.message, gift = JsonOr((string)r.gift, "{}"),
        targets = (string?)r.targets, startsAt = (DateTime?)r.starts_at, endsAt = (DateTime?)r.ends_at, active = (bool)r.active,
        createdBy = (string)r.created_by, createdAt = (DateTime)r.created_at, claims = (long)r.claims, claimers = (string?)r.claimers
    }));
});

// 관리자: 소포 만들기
app.MapPost("/api/astera/admin/parcels", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    System.Text.Json.JsonElement b;
    try { b = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(ctx.Request.Body); }
    catch (System.Text.Json.JsonException) { return Results.BadRequest(); }
    string S(string k) => b.TryGetProperty(k, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? (v.GetString() ?? "").Trim() : "";
    // 기간은 UTC로 저장 (페이지가 toISOString()으로 보냄 → 서버 DB 시간대와 상관없이 정확)
    DateTime? D(string k) => DateTime.TryParse(S(k), System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var d) ? d : null;
    string title = S("title"), message = S("message"), targets = S("targets");
    if (title.Length == 0 || title.Length > 60 || message.Length > 400) return Results.Json(new { ok = false, error = "제목(60자 이하)과 메시지(400자 이하)를 확인하세요." });
    if (!b.TryGetProperty("gift", out var gift) || gift.ValueKind != System.Text.Json.JsonValueKind.Object) return Results.BadRequest();
    string giftJson = gift.GetRawText();
    if (giftJson.Length > 64 * 1024) return Results.StatusCode(413);
    using var conn = db.OpenConnection();
    int id = Dapper.SqlMapper.ExecuteScalar<int>(conn, @"
        INSERT INTO astera_parcels (title, message, gift, targets, starts_at, ends_at, created_by)
        VALUES (@title, @message, CAST(@giftJson AS JSONB), NULLIF(@targets, ''), @starts, @ends, @uid) RETURNING id",
        new { title, message, giftJson, targets, starts = D("startsAt"), ends = D("endsAt"), uid });
    return Results.Json(new { ok = true, id });
});

// 관리자: 켜기/끄기, 삭제
app.MapPost("/api/astera/admin/parcels/{id:int}/active", async (int id, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    var b = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(ctx.Request.Body);
    bool on = b.TryGetProperty("active", out var a) && a.ValueKind == System.Text.Json.JsonValueKind.True;
    using var conn = db.OpenConnection();
    Dapper.SqlMapper.Execute(conn, "UPDATE astera_parcels SET active = @on WHERE id = @id", new { id, on });
    return Results.Json(new { ok = true });
});
app.MapDelete("/api/astera/admin/parcels/{id:int}", (int id, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    if (!IsSiteAdmin(db, uid)) return Results.StatusCode(403);
    using var conn = db.OpenConnection();
    Dapper.SqlMapper.Execute(conn, "DELETE FROM astera_parcels WHERE id = @id", new { id });
    return Results.Json(new { ok = true });
});

// 내 선물 받기: 선물함을 비우면서 내용을 돌려줌 (한 번만 받도록 DELETE ... RETURNING)
app.MapPost("/api/astera/gifts/claim", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Unauthorized();
    ctx.Response.Headers.CacheControl = "no-store";
    using var conn = db.OpenConnection();
    var data = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "DELETE FROM astera_data WHERE userid = @uid AND kind = 'gifts' RETURNING data::text", new { uid });
    return Results.Content(data ?? "{\"list\":[]}", "application/json; charset=utf-8");
});


// ── 미니멀 API 엔드포인트 (알림·검색 AJAX 호출용) ──────────────────────────

// 알림 API (레이아웃 JS에서 fetch 호출)
app.MapGet("/api/notify", (HttpContext ctx, DbService db) =>
{
    string action = ctx.Request.Query["action"].ToString();
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(Array.Empty<object>());

    using var conn = db.OpenConnection();

    if (action == "getList")
    {
        // 최근 알림 20건 — PK 컬럼명은 'id' (notifid 아님), AS notifid로 별칭
        var list = Dapper.SqlMapper.Query(conn,
            @"SELECT n.id AS notifid, n.bbsno, n.type, n.isread, n.actorid,
                     b.title AS bbstitle, m.nickname AS actornick
              FROM notifications n
              LEFT JOIN bbs b ON n.bbsno = b.no
              LEFT JOIN members m ON n.actorid = m.userid
              WHERE n.userid = @uid
              ORDER BY n.createdat DESC
              LIMIT 20", new { uid });

        var items = new List<object>();
        foreach (var r in list)
        {
            // PostgreSQL은 컬럼명을 소문자로 반환, 별칭 notifid로 접근
            items.Add(new
            {
                notifID  = Convert.ToInt32(r.notifid ?? 0),
                bbsNo    = Convert.ToInt32(r.bbsno ?? 0),
                type     = ((string)(r.type ?? "comment")).Trim(),
                isRead   = r.isread is bool b ? b : false,
                actorNick = ((string)(r.actornick ?? "")).Trim(),
                bbsTitle  = (string)(r.bbstitle ?? "")
            });
        }
        return Results.Json(items);
    }

    if (action == "markRead")
    {
        // PostgreSQL 불리언은 true/false (MSSQL의 1/0 불가)
        Dapper.SqlMapper.Execute(conn, "UPDATE notifications SET isread=true WHERE userid=@uid", new { uid });
        return Results.Json(new { result = "ok" });
    }

    if (action == "markOne")
    {
        string nid = ctx.Request.Query["notifID"].ToString();
        if (int.TryParse(nid, out int notifID))
            // PK 컬럼명은 id
            Dapper.SqlMapper.Execute(conn, "UPDATE notifications SET isread=true WHERE id=@notifID AND userid=@uid", new { notifID, uid });
        return Results.Json(new { result = "ok" });
    }

    if (action == "delete")
    {
        string nid = ctx.Request.Query["notifID"].ToString();
        if (int.TryParse(nid, out int notifID))
            // PK 컬럼명은 id
            Dapper.SqlMapper.Execute(conn, "DELETE FROM notifications WHERE id=@notifID AND userid=@uid", new { notifID, uid });
        return Results.Json(new { result = "ok" });
    }

    return Results.Json(new { result = "unknown" });
});

// 검색 API (Index/SearchResults JS에서 fetch 호출)
app.MapGet("/api/search", (HttpContext ctx, SearchDao searchDao, DbService db) =>
{
    string action = ctx.Request.Query["action"].ToString();
    string? uid = ctx.Session.GetString("UserID");

    if (action == "history")
    {
        if (uid == null) return Results.Json(Array.Empty<object>());
        var items = searchDao.GetRecentSearches(uid, 10)
            .Select(r => new { id = (int)r.id, query = (string)r.query });
        return Results.Json(items);
    }

    if (action == "deleteHistory")
    {
        if (uid == null) return Results.Json(new { result = "ok" });
        string idStr = ctx.Request.Query["id"].ToString();
        if (int.TryParse(idStr, out int id))
        {
            using var conn = db.OpenConnection();
            // 해당 id의 검색어와 동일한 모든 기록 삭제 (GROUP BY query 대응)
            Dapper.SqlMapper.Execute(conn,
                "DELETE FROM searchhistory WHERE userid=@uid AND query = (SELECT query FROM searchhistory WHERE no=@id AND userid=@uid)",
                new { id, uid });
        }
        return Results.Json(new { result = "ok" });
    }

    return Results.Json(new { result = "unknown" });
});

// 관리자 권한 확인 헬퍼 (구성 파일 + DB ugrade 컬럼 모두 확인)
var adminUsers = new HashSet<string>(
    // 설정이 없으면 아무도 관리자가 아님 (예전엔 'test'가 기본 관리자로 들어가 있었음)
    (builder.Configuration.GetSection("AdminUsers").Get<string[]>() ?? Array.Empty<string>())
    .Select(u => u.Trim().ToLowerInvariant()));
bool IsAdmin(string? uid)
{
    if (uid == null) return false;
    // 1. 구성 파일(appsettings.json)에 등록된 관리자
    if (adminUsers.Contains(uid.ToLowerInvariant())) return true;
    // 2. DB members 테이블의 isadmin 컬럼이 1인 회원
    try
    {
        using var conn = app.Services.GetRequiredService<DbService>().OpenConnection();
        var val = Dapper.SqlMapper.ExecuteScalar<int?>(conn,
            "SELECT isadmin FROM members WHERE userid = @uid", new { uid });
        return val == 1;
    }
    catch { return false; }
}

// 관리자 API 사용량 확인
app.MapGet("/api/admin/usage", (HttpContext ctx, ApiUsageService usage) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    var summary = usage.GetAllUsageSummary();
    var result = new List<object>();
    foreach (var row in summary)
    {
        result.Add(new
        {
            engine = (string)row.engine,
            today = Convert.ToInt32(row.today_count),
            monthly = Convert.ToInt32(row.monthly_count),
            total = Convert.ToInt32(row.total_count)
        });
    }

    return Results.Json(new
    {
        limits = new
        {
            serper = "2,500 total (one-time)",
            brave = "2,000/month",
            naver = "25,000/day",
            kakao = "300/day"
        },
        usage = result
    });
});

// ── 별점 평가 API ─────────────────────────────────────────────────────────

// 평가 여부 확인 (로그인 사용자)
app.MapGet("/api/rating/check", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(new { rated = false });

    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<int?>(conn,
        "SELECT rating FROM user_ratings WHERE userid = @uid LIMIT 1", new { uid });
    return Results.Json(new { rated = exists != null });
});

// 별점 제출 (로그인 필요, 계정당 1회)
app.MapPost("/api/rating", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(new { result = "unauthorized" });

    // 중복 평가 방지
    using var conn = db.OpenConnection();
    var already = Dapper.SqlMapper.QueryFirstOrDefault<int?>(conn,
        "SELECT id FROM user_ratings WHERE userid = @uid LIMIT 1", new { uid });
    if (already != null) return Results.Json(new { result = "already_rated" });

    // JSON 본문 파싱 (JsonElement → 명시적 변환)
    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    int rating = body.TryGetProperty("rating", out var rVal) ? rVal.GetInt32() : 0;
    string comment = body.TryGetProperty("comment", out var cVal) ? cVal.GetString() ?? "" : "";

    if (rating < 1 || rating > 5) return Results.Json(new { result = "invalid_rating" });

    // XSS 방지: HTML 태그 제거 + 길이 제한 (500자)
    comment = System.Text.RegularExpressions.Regex.Replace(comment.Trim(), "<.*?>", "");
    if (comment.Length > 500) comment = comment[..500];

    Dapper.SqlMapper.Execute(conn,
        "INSERT INTO user_ratings (userid, rating, comment) VALUES (@uid, @rating, @comment)",
        new { uid, rating, comment });

    return Results.Json(new { result = "ok" });
});

// 공개 평가 목록 (비로그인 포함, 최근 20건 + 평균)
app.MapGet("/api/ratings/public", (DbService db) =>
{
    using var conn = db.OpenConnection();

    // 전체 통계
    var stats = Dapper.SqlMapper.QueryFirstOrDefault(conn,
        "SELECT COUNT(*) AS cnt, COALESCE(AVG(rating::DECIMAL), 0) AS avg_rating FROM user_ratings");

    // 최근 한줄평 있는 평가 20건
    var list = Dapper.SqlMapper.Query(conn,
        @"SELECT r.rating, r.comment, r.created_at,
                 COALESCE(m.nickname, '익명') AS nickname
          FROM user_ratings r
          LEFT JOIN members m ON r.userid = m.userid
          WHERE r.comment IS NOT NULL AND TRIM(r.comment) != ''
          ORDER BY r.created_at DESC
          LIMIT 20");

    var items = new List<object>();
    foreach (var r in list)
    {
        items.Add(new
        {
            nickname = (string)(r.nickname ?? "익명"),
            rating = Convert.ToInt32(r.rating),
            comment = (string)(r.comment ?? ""),
            date = ((DateTime)r.created_at).ToString("yyyy-MM-dd")
        });
    }

    return Results.Json(new
    {
        totalCount = Convert.ToInt32(stats?.cnt ?? 0),
        avgRating = Math.Round(Convert.ToDecimal(stats?.avg_rating ?? 0), 1),
        reviews = items
    });
});

// 관리자 별점 조회
app.MapGet("/api/admin/ratings", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    using var conn = db.OpenConnection();

    // 전체 통계
    var stats = Dapper.SqlMapper.QueryFirstOrDefault(conn,
        @"SELECT COUNT(*) AS cnt, COALESCE(AVG(rating::DECIMAL), 0) AS avg_rating
          FROM user_ratings");

    // 최근 평가 50건
    var list = Dapper.SqlMapper.Query(conn,
        @"SELECT r.id, r.userid, r.rating, r.comment, r.created_at,
                 COALESCE(m.nickname, r.userid) AS nickname
          FROM user_ratings r
          LEFT JOIN members m ON r.userid = m.userid
          ORDER BY r.created_at DESC
          LIMIT 50");

    var items = new List<object>();
    foreach (var r in list)
    {
        items.Add(new
        {
            id = Convert.ToInt32(r.id),
            userid = (string)(r.userid ?? ""),
            nickname = (string)(r.nickname ?? ""),
            rating = Convert.ToInt32(r.rating),
            comment = (string)(r.comment ?? ""),
            created_at = ((DateTime)r.created_at).ToString("yyyy-MM-dd HH:mm")
        });
    }

    return Results.Json(new
    {
        totalCount = Convert.ToInt32(stats?.cnt ?? 0),
        avgRating = Math.Round(Convert.ToDecimal(stats?.avg_rating ?? 0), 1),
        ratings = items
    });
});

// 관리자 회원 목록
app.MapGet("/api/admin/members", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    using var conn = db.OpenConnection();
    var list = Dapper.SqlMapper.Query(conn,
        @"SELECT userid, nickname, name, email, status,
                 COALESCE(last_ip, '') AS last_ip,
                 COALESCE(created_at, NOW()) AS created_at,
                 COALESCE(last_login, NOW()) AS last_login,
                 COALESCE(isadmin, 0) AS isadmin
          FROM members ORDER BY created_at DESC");

    // IP 앞 두 자리만 노출 (개인정보 보호)
    string maskIp(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return "-";
        var parts = ip.Split('.');
        if (parts.Length >= 2) return parts[0] + "." + parts[1] + ".*.*";
        return ip;
    }

    var items = new List<object>();
    foreach (var r in list)
    {
        items.Add(new
        {
            userid = (string)(r.userid ?? ""),
            nickname = (string)(r.nickname ?? ""),
            name = (string)(r.name ?? ""),
            email = (string)(r.email ?? ""),
            status = ((string)(r.status ?? "")).Trim(),
            ip = maskIp(((string)(r.last_ip ?? "")).Trim()),
            created_at = ((DateTime)r.created_at).ToString("yyyy-MM-dd"),
            last_login = ((DateTime)r.last_login).ToString("yyyy-MM-dd HH:mm"),
            // 관리자 여부 (DB isadmin=1 또는 config 기반)
            isAdmin = Convert.ToInt32(r.isadmin ?? 0) == 1
                      || adminUsers.Contains(((string)(r.userid ?? "")).ToLowerInvariant())
        });
    }

    return Results.Json(new { totalMembers = items.Count, members = items });
});

// 관리자 별점 평가 삭제
app.MapPost("/api/admin/ratings/delete", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    int id = body.TryGetProperty("id", out var v) ? v.GetInt32() : 0;
    if (id <= 0) return Results.Json(new { result = "invalid" });

    using var conn = db.OpenConnection();
    Dapper.SqlMapper.Execute(conn, "DELETE FROM user_ratings WHERE id = @id", new { id });
    return Results.Json(new { result = "ok" });
});

// 관리자 회원 강제 탈퇴 (DB에서 완전 삭제, 이메일 재가입 가능)
app.MapPost("/api/admin/members/delete", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string target = body.TryGetProperty("userid", out var v) ? v.GetString() ?? "" : "";
    if (string.IsNullOrEmpty(target)) return Results.Json(new { result = "invalid" });

    // 관리자 본인은 삭제 불가
    if (string.Equals(target, uid, StringComparison.OrdinalIgnoreCase))
        return Results.Json(new { result = "cannot_delete_self" });

    using var conn = db.OpenConnection();

    // 관련 데이터 전부 삭제 (이메일 재가입 가능하도록)
    Dapper.SqlMapper.Execute(conn, "DELETE FROM searchhistory WHERE userid = @target", new { target });
    Dapper.SqlMapper.Execute(conn, "DELETE FROM user_ratings WHERE userid = @target", new { target });
    try { Dapper.SqlMapper.Execute(conn, "DELETE FROM bookmarks WHERE userid = @target", new { target }); } catch { }
    try { Dapper.SqlMapper.Execute(conn, "DELETE FROM bbs WHERE userid = @target", new { target }); } catch { }
    Dapper.SqlMapper.Execute(conn, "DELETE FROM members WHERE userid = @target", new { target });

    return Results.Json(new { result = "ok" });
});

// 관리자 권한 부여/해제 토글 (ugrade 컬럼 변경)
app.MapPost("/api/admin/members/toggle-admin", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string target = body.TryGetProperty("userid", out var v) ? v.GetString() ?? "" : "";
    if (string.IsNullOrEmpty(target)) return Results.Json(new { result = "invalid" });

    // 관리자 본인의 권한 해제 방지
    if (string.Equals(target, uid, StringComparison.OrdinalIgnoreCase))
        return Results.Json(new { result = "cannot_change_self" });

    using var conn = db.OpenConnection();

    // 현재 isadmin 값 조회 후 토글 (0↔1)
    var current = Dapper.SqlMapper.ExecuteScalar<int?>(conn,
        "SELECT COALESCE(isadmin, 0) FROM members WHERE userid = @target", new { target });
    int newVal = (current == 1) ? 0 : 1;

    Dapper.SqlMapper.Execute(conn,
        "UPDATE members SET isadmin = @newVal WHERE userid = @target",
        new { newVal, target });

    return Results.Json(new { result = "ok", isAdmin = newVal == 1 });
});

// 관리자 실시간 접속 현황
app.MapGet("/api/admin/connections", (HttpContext ctx, ConnectionTracker tracker, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    var active = tracker.GetActiveConnections();

    // 세션에서 IP→유저ID 매핑은 불가하므로 DB에서 last_ip로 매칭
    using var conn = db.OpenConnection();
    var members = Dapper.SqlMapper.Query(conn,
        "SELECT userid, nickname, COALESCE(last_ip, '') AS last_ip FROM members WHERE last_ip IS NOT NULL AND last_ip != ''");
    var ipToUser = new Dictionary<string, (string userid, string nickname)>();
    foreach (var m in members)
    {
        string mIp = ((string)(m.last_ip ?? "")).Trim();
        if (!string.IsNullOrEmpty(mIp))
            ipToUser[mIp] = ((string)m.userid, (string)m.nickname);
    }

    // IP 앞 두 자리만 노출
    string maskIp(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length >= 2) return parts[0] + "." + parts[1] + ".*.*";
        return ip;
    }

    var items = active.Select(c =>
    {
        bool isMember = ipToUser.ContainsKey(c.Ip);
        return new
        {
            ip = maskIp(c.Ip),
            nickname = isMember ? ipToUser[c.Ip].nickname : "비회원",
            userid = isMember ? ipToUser[c.Ip].userid : "-",
            lastSeen = c.LastSeen.ToLocalTime().ToString("HH:mm:ss")
        };
    });

    return Results.Json(new { count = active.Count, connections = items });
});

// ── 회원가입 중복확인 API ────────────────────────────────────────────────

// 아이디 중복확인
app.MapGet("/api/register/check-id", (HttpContext ctx, MemberDao dao) =>
{
    string id = ctx.Request.Query["id"].ToString().Trim().ToLowerInvariant();
    if (string.IsNullOrEmpty(id))
        return Results.Json(new { result = "empty" });

    // 영문+숫자+밑줄, 3~20자 형식 검증
    if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-z0-9_]{3,20}$"))
        return Results.Json(new { result = "invalid_format" });

    bool available = dao.VerifyUserID(id);
    return Results.Json(new { result = available ? "ok" : "duplicate" });
});

// 이메일 중복확인
app.MapGet("/api/register/check-email", (HttpContext ctx, DbService db) =>
{
    string email = ctx.Request.Query["email"].ToString().Trim().ToLowerInvariant();
    if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        return Results.Json(new { result = "invalid" });

    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(email) = @email AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
        new { email });
    return Results.Json(new { result = exists == null ? "ok" : "duplicate" });
});

// 회원가입 (로그인 모달용 JSON API) — 성공하면 바로 로그인 상태로 만듦
app.MapPost("/api/account/register", async (HttpContext ctx, MemberDao dao, DbService db) =>
{
    // 가입 남용 방지: IP당 1시간에 5회
    string ip = GetRealIp(ctx);
    if (IsRateLimited("register:" + (string.IsNullOrEmpty(ip) ? "unknown" : ip), 5, 3600))
        return Results.Json(new { ok = false, error = "가입 시도가 너무 많습니다. 잠시 후 다시 시도해주세요." });

    System.Text.Json.JsonElement body;
    try { body = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(ctx.Request.Body); }
    catch (System.Text.Json.JsonException) { return Results.BadRequest(); }
    string S(string k) => body.TryGetProperty(k, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() ?? "" : "";

    var input = new MetaSearch.Models.MemberDo
    {
        Userid = S("userid"), Passwd = S("passwd"), Name = S("name"), Nickname = S("nickname"), Email = S("email")
    };
    string? err = MetaSearch.Pages.RegisterModel.Validate(input, S("passwdConfirm"), ctx.Session.GetString("EmailVerified"), db, "모든 항목을 입력해주세요.");
    if (err != null) return Results.Json(new { ok = false, error = err });

    if (dao.RegisterUser(input) == -1)
        return Results.Json(new { ok = false, error = "이미 사용 중인 아이디입니다." });

    // 인증 기록은 한 번만 쓰고, 가입한 계정으로 바로 로그인
    ctx.Session.Remove("EmailVerified");
    ctx.Session.SetString("UserID", input.Userid);
    ctx.Session.SetString("UserName", input.Nickname);
    return Results.Json(new { ok = true, userId = input.Userid, nick = input.Nickname });
});

// ── 이메일 인증 API ──────────────────────────────────────────────────────

// 인증코드 발송 (Brevo API) — 이메일 중복도 사전 체크 + 레이트 리미팅
app.MapPost("/api/email/send-code", async (HttpContext ctx, EmailService email, DbService db) =>
{
    // 레이트 리미팅: IP당 1분에 3회
    string ip = GetRealIp(ctx);
    if (string.IsNullOrEmpty(ip)) ip = "unknown";
    if (IsRateLimited($"email:{ip}", 3, 60))
        return Results.Json(new { result = "rate_limited" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string addr = body.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
    addr = addr.Trim().ToLowerInvariant();

    if (string.IsNullOrEmpty(addr) || !addr.Contains('@'))
        return Results.Json(new { result = "invalid_email" });

    // 이메일 중복 체크
    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(email) = @addr AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
        new { addr });
    if (exists != null)
        return Results.Json(new { result = "duplicate_email" });

    bool sent = await email.SendVerificationCodeAsync(addr);
    return Results.Json(new { result = sent ? "ok" : "send_failed" });
});

// 인증코드 확인
app.MapPost("/api/email/verify-code", async (HttpContext ctx, EmailService email) =>
{
    // 인증코드 무작정 대입 방지: IP당 1분에 10번까지 (코드별 5번 틀리면 폐기와 함께)
    if (IsRateLimited($"verify:{GetRealIp(ctx)}", 10, 60)) return Results.Json(new { result = "rate_limited" });
    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string addr = body.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
    string code = body.TryGetProperty("code", out var c) ? c.GetString() ?? "" : "";

    bool ok = email.VerifyCode(addr.Trim(), code.Trim());
    if (ok) ctx.Session.SetString("EmailVerified", addr.Trim().ToLowerInvariant());

    return Results.Json(new { result = ok ? "ok" : "invalid" });
});

// ── 계정 찾기 API (아이디/비밀번호 찾기) ─────────────────────────────────────

// 아이디 찾기: 이메일로 인증코드 발송
app.MapPost("/api/account/find-id", async (HttpContext ctx, EmailService email, DbService db) =>
{
    // 레이트 리미팅: IP당 1분에 3회
    string ip = GetRealIp(ctx);
    if (string.IsNullOrEmpty(ip)) ip = "unknown";
    if (IsRateLimited($"find-id:{ip}", 3, 60))
        return Results.Json(new { result = "rate_limited" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string addr = body.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
    addr = addr.Trim().ToLowerInvariant();

    if (string.IsNullOrEmpty(addr) || !addr.Contains('@'))
        return Results.Json(new { result = "invalid_email" });

    // 해당 이메일로 가입된 계정 존재 여부 확인
    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(email) = @addr AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
        new { addr });
    if (exists == null)
        return Results.Json(new { result = "not_found" });

    // 아이디 찾기용 인증코드 발송
    bool sent = await email.SendVerificationCodeAsync(addr, "find-id");
    return Results.Json(new { result = sent ? "ok" : "send_failed" });
});

// 아이디 찾기: 인증코드 확인 후 마스킹된 아이디 반환
app.MapPost("/api/account/find-id/verify", (HttpContext ctx, EmailService email, DbService db) =>
{
    // 인증코드 무작정 대입 방지: IP당 1분에 10번까지 (코드별 5번 틀리면 폐기와 함께)
    if (IsRateLimited($"verify:{GetRealIp(ctx)}", 10, 60)) return Results.Json(new { result = "rate_limited" });
    var body = ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>().Result;
    string addr = body.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
    string code = body.TryGetProperty("code", out var c) ? c.GetString() ?? "" : "";

    bool ok = email.VerifyCode(addr.Trim(), code.Trim());
    if (!ok) return Results.Json(new { result = "invalid_code" });

    // 아이디 조회 후 마스킹 (앞 2자리 + *** + 뒤 1자리)
    using var conn = db.OpenConnection();
    string? userid = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(email) = @addr AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
        new { addr = addr.Trim().ToLowerInvariant() });

    if (userid == null) return Results.Json(new { result = "not_found" });

    userid = userid.Trim();
    // 마스킹: 3자 이하 → 첫 글자 + **, 4자 이상 → 앞2 + *** + 끝1
    string masked = userid.Length <= 3
        ? userid[0] + new string('*', userid.Length - 1)
        : userid[..2] + new string('*', userid.Length - 3) + userid[^1..];

    return Results.Json(new { result = "ok", userid = masked });
});

// 비밀번호 찾기: 아이디+이메일 확인 후 인증코드 발송
app.MapPost("/api/account/find-pw", async (HttpContext ctx, EmailService email, DbService db) =>
{
    string ip = GetRealIp(ctx);
    if (string.IsNullOrEmpty(ip)) ip = "unknown";
    if (IsRateLimited($"find-pw:{ip}", 3, 60))
        return Results.Json(new { result = "rate_limited" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string userid = body.TryGetProperty("userid", out var u) ? u.GetString() ?? "" : "";
    string addr = body.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
    userid = userid.Trim().ToLowerInvariant();
    addr = addr.Trim().ToLowerInvariant();

    if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(addr) || !addr.Contains('@'))
        return Results.Json(new { result = "invalid_input" });

    // 아이디+이메일 일치 확인
    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(userid) = @userid AND LOWER(email) = @addr AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
        new { userid, addr });
    if (exists == null)
        return Results.Json(new { result = "not_found" });

    // 비밀번호 찾기용 인증코드 발송
    bool sent = await email.SendVerificationCodeAsync(addr, "password");
    return Results.Json(new { result = sent ? "ok" : "send_failed" });
});

// 비밀번호 찾기: 인증코드 확인
app.MapPost("/api/account/find-pw/verify", (HttpContext ctx, EmailService email) =>
{
    // 인증코드 무작정 대입 방지: IP당 1분에 10번까지 (코드별 5번 틀리면 폐기와 함께)
    if (IsRateLimited($"verify:{GetRealIp(ctx)}", 10, 60)) return Results.Json(new { result = "rate_limited" });
    var body = ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>().Result;
    string addr = body.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
    string code = body.TryGetProperty("code", out var c) ? c.GetString() ?? "" : "";

    bool ok = email.VerifyCode(addr.Trim(), code.Trim());
    if (!ok) return Results.Json(new { result = "invalid_code" });

    // 세션에 비밀번호 재설정 허용 플래그 저장
    ctx.Session.SetString("PwResetEmail", addr.Trim().ToLowerInvariant());
    return Results.Json(new { result = "ok" });
});

// 비밀번호 찾기: 새 비밀번호 설정
app.MapPost("/api/account/find-pw/reset", (HttpContext ctx, DbService db) =>
{
    var body = ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>().Result;
    string userid = body.TryGetProperty("userid", out var u) ? u.GetString() ?? "" : "";
    string newPw = body.TryGetProperty("newPw", out var p) ? p.GetString() ?? "" : "";

    userid = userid.Trim().ToLowerInvariant();

    // 세션에서 비밀번호 재설정 허용 여부 확인
    string? resetEmail = ctx.Session.GetString("PwResetEmail");
    if (string.IsNullOrEmpty(resetEmail))
        return Results.Json(new { result = "unauthorized" });

    // 비밀번호 정책 검증 (8자 이상, 영문+숫자+특수문자 포함)
    if (newPw.Length < 8 ||
        !System.Text.RegularExpressions.Regex.IsMatch(newPw, @"[A-Za-z]") ||
        !System.Text.RegularExpressions.Regex.IsMatch(newPw, @"[0-9]") ||
        !System.Text.RegularExpressions.Regex.IsMatch(newPw, @"[^A-Za-z0-9]"))
        return Results.Json(new { result = "weak_password" });

    // 아이디+이메일 일치 확인 후 비밀번호 재설정
    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<string>(conn,
        "SELECT userid FROM members WHERE LOWER(userid) = @userid AND LOWER(email) = @resetEmail AND (TRIM(status) = 'true' OR TRIM(status) = '1')",
        new { userid, resetEmail });
    if (exists == null)
        return Results.Json(new { result = "not_found" });

    // BCrypt 해싱 후 저장
    string hash = BCrypt.Net.BCrypt.HashPassword(newPw, workFactor: 11);
    Dapper.SqlMapper.Execute(conn,
        "UPDATE members SET passwd = @hash WHERE LOWER(userid) = @userid",
        new { hash, userid });

    // 세션 플래그 제거
    ctx.Session.Remove("PwResetEmail");
    return Results.Json(new { result = "ok" });
});

// ── 사이트맵 (구글·네이버 제출용) ─────────────────────────────────────────
// 서버가 요청 때마다 만듦 → 새 게시글이 바로 들어감 (예전 정적 sitemap.xml은 게시글이 빠져 있었고,
//   이 코드는 옛 도메인 metasearch.kro.kr 로 적혀 있었는데 정적 파일에 가려 쓰이지도 않았음)
// 주소는 모두 301 이동이 없는 '대표 주소'만 (옛 /Community 같은 주소를 넣으면 구글이 대체 페이지로 처리)
app.MapGet("/sitemap.xml", (HttpContext ctx, DbService db) =>
{
    const string site = "https://domang.org";
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
    void Url(string path, string? lastmod = null) =>
        sb.AppendLine($"  <url><loc>{site}{path}</loc>{(lastmod != null ? $"<lastmod>{lastmod}</lastmod>" : "")}</url>");

    // 고정 페이지: 메인 포트폴리오 → 프로젝트 소개 → 서비스
    foreach (var path in new[] { "/", "/metaindex", "/pokeindex", "/driveindex", "/ottindex",
                                 "/metasearch", "/metasearch/community", "/metasearch/report", "/metasearch/eval",
                                 "/pokemon", "/Privacy" })   // /drive·/signin 은 로그인 입구라 noindex → 사이트맵에서 뺌
        Url(path);

    // 커뮤니티 게시글 전체 (게시글마다 따로 검색에 나오게, 최근 글 먼저 최대 5000개)
    try
    {
        using var conn = db.OpenConnection();
        var posts = Dapper.SqlMapper.Query<(int no, DateTime uploadtime)>(conn,
            "SELECT no, uploadtime FROM bbs ORDER BY no DESC LIMIT 5000");
        foreach (var p in posts)
            Url($"/metasearch/community/view?no={p.no}", p.uploadtime.ToString("yyyy-MM-dd"));
    }
    catch (Exception ex) { Console.WriteLine("[Sitemap] 게시글 목록 실패: " + ex.Message); }

    sb.AppendLine("</urlset>");
    ctx.Response.Headers.CacheControl = "public, max-age=3600";   // 1시간 캐시 (Cloudflare·검색 로봇)
    return Results.Content(sb.ToString(), "application/xml; charset=utf-8");
});

// ── 북마크 API (검색 결과 저장/조회/삭제) ──────────────────────────────────

// 북마크 목록 조회
app.MapGet("/api/bookmarks", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(new { items = Array.Empty<object>() });

    using var conn = db.OpenConnection();
    var list = Dapper.SqlMapper.Query(conn,
        "SELECT id, title, url, description, source, folder, created_at FROM bookmarks WHERE userid = @uid ORDER BY created_at DESC LIMIT 100",
        new { uid });

    var items = new List<object>();
    foreach (var r in list)
    {
        items.Add(new
        {
            id = Convert.ToInt32(r.id),
            title = (string)(r.title ?? ""),
            url = (string)(r.url ?? ""),
            description = (string)(r.description ?? ""),
            source = (string)(r.source ?? ""),
            folder = (string)(r.folder ?? "기본"),
            created_at = ((DateTime)r.created_at).ToString("yyyy-MM-dd HH:mm")
        });
    }

    // 폴더 목록도 함께 반환
    var folders = Dapper.SqlMapper.Query<string>(conn,
        "SELECT DISTINCT folder FROM bookmarks WHERE userid = @uid ORDER BY folder", new { uid });

    return Results.Json(new { items, folders });
});

// 북마크 추가
app.MapPost("/api/bookmarks", async (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(new { result = "unauthorized" });

    var body = await ctx.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    string title = body.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
    string url = body.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
    string desc = body.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
    string source = body.TryGetProperty("source", out var s) ? s.GetString() ?? "" : "";
    string folder = body.TryGetProperty("folder", out var f) ? f.GetString() ?? "기본" : "기본";

    if (string.IsNullOrEmpty(url)) return Results.Json(new { result = "invalid" });

    // XSS 방지
    title = System.Text.RegularExpressions.Regex.Replace(title, "<.*?>", "");
    if (title.Length > 200) title = title[..200];
    if (folder.Length > 50) folder = folder[..50];

    // 중복 확인
    using var conn = db.OpenConnection();
    var exists = Dapper.SqlMapper.QueryFirstOrDefault<int?>(conn,
        "SELECT id FROM bookmarks WHERE userid = @uid AND url = @url LIMIT 1",
        new { uid, url });
    if (exists != null) return Results.Json(new { result = "duplicate" });

    Dapper.SqlMapper.Execute(conn,
        "INSERT INTO bookmarks (userid, title, url, description, source, folder) VALUES (@uid, @title, @url, @desc, @source, @folder)",
        new { uid, title, url, desc, source, folder });

    return Results.Json(new { result = "ok" });
});

// 북마크 삭제
app.MapDelete("/api/bookmarks/{id:int}", (int id, HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(new { result = "unauthorized" });

    using var conn = db.OpenConnection();
    Dapper.SqlMapper.Execute(conn,
        "DELETE FROM bookmarks WHERE id = @id AND userid = @uid",
        new { id, uid });

    return Results.Json(new { result = "ok" });
});

// ── 인기검색어 트렌드 API (시간대별 TOP 10) ─────────────────────────────────

app.MapGet("/api/trends", (DbService db) =>
{
    using var conn = db.OpenConnection();

    // 현재 인기검색어 TOP 10 (최근 24시간)
    var top10 = Dapper.SqlMapper.Query(conn,
        @"SELECT query, COUNT(*) AS cnt
          FROM searchhistory WHERE searchtime > NOW() - INTERVAL '24 hours'
          GROUP BY query ORDER BY cnt DESC LIMIT 10");

    var items = new List<object>();
    int rank = 1;
    foreach (var r in top10)
    {
        items.Add(new { rank = rank++, query = (string)(r.query ?? ""), count = Convert.ToInt32(r.cnt) });
    }

    // 시간대별 검색량 (최근 24시간, 시간 단위 집계)
    var hourly = Dapper.SqlMapper.Query(conn,
        @"SELECT DATE_TRUNC('hour', searchtime) AS hour, COUNT(*) AS cnt
          FROM searchhistory WHERE searchtime > NOW() - INTERVAL '24 hours'
          GROUP BY DATE_TRUNC('hour', searchtime) ORDER BY hour");

    var chart = new List<object>();
    foreach (var r in hourly)
    {
        chart.Add(new { hour = ((DateTime)r.hour).ToString("HH:00"), count = Convert.ToInt32(r.cnt) });
    }

    // 총 검색 수 (오늘)
    long todayTotal = Dapper.SqlMapper.ExecuteScalar<long>(conn,
        "SELECT COUNT(*) FROM searchhistory WHERE searchtime > CURRENT_DATE");

    return Results.Json(new { top10 = items, hourly = chart, todayTotal = Convert.ToInt32(todayTotal) });
});

// ── AI 검색 요약 API ─────────────────────────────────────────────────────────

app.MapGet("/api/ai-summary", async (HttpContext ctx, ClaudeApi ai) =>
{
    string q = ctx.Request.Query["q"].ToString().Trim();
    if (string.IsNullOrEmpty(q) || q.Length > 200)
        return Results.Json(new { summary = "" });

    // 레이트 리미팅: IP당 1분에 5회
    string ip = GetRealIp(ctx);
    if (string.IsNullOrEmpty(ip)) ip = "unknown";
    if (IsRateLimited($"ai-summary:{ip}", 5, 60))
        return Results.Json(new { summary = "", limited = true });

    try
    {
        string summary = await ai.SummarizeQueryAsync(q);
        return Results.Json(new { summary });
    }
    catch
    {
        return Results.Json(new { summary = "" });
    }
});

// ── 관리자 통계 API (대시보드 차트용) ────────────────────────────────────────

app.MapGet("/api/admin/stats", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (!IsAdmin(uid)) return Results.Json(new { error = "unauthorized" });

    using var conn = db.OpenConnection();

    // 일별 가입자 수 (최근 30일)
    var dailySignups = Dapper.SqlMapper.Query(conn,
        @"SELECT DATE(created_at) AS day, COUNT(*) AS cnt
          FROM members WHERE created_at > NOW() - INTERVAL '30 days'
          GROUP BY DATE(created_at) ORDER BY day");

    var signups = new List<object>();
    foreach (var r in dailySignups)
        signups.Add(new { day = ((DateTime)r.day).ToString("MM-dd"), count = Convert.ToInt32(r.cnt) });

    // 일별 검색량 (최근 30일)
    var dailySearches = Dapper.SqlMapper.Query(conn,
        @"SELECT DATE(searchtime) AS day, COUNT(*) AS cnt
          FROM searchhistory WHERE searchtime > NOW() - INTERVAL '30 days'
          GROUP BY DATE(searchtime) ORDER BY day");

    var searches = new List<object>();
    foreach (var r in dailySearches)
        searches.Add(new { day = ((DateTime)r.day).ToString("MM-dd"), count = Convert.ToInt32(r.cnt) });

    // 일별 게시글 수 (최근 30일)
    var dailyPosts = Dapper.SqlMapper.Query(conn,
        @"SELECT DATE(uploadtime) AS day, COUNT(*) AS cnt
          FROM bbs WHERE uploadtime > NOW() - INTERVAL '30 days'
          GROUP BY DATE(uploadtime) ORDER BY day");

    var posts = new List<object>();
    foreach (var r in dailyPosts)
        posts.Add(new { day = ((DateTime)r.day).ToString("MM-dd"), count = Convert.ToInt32(r.cnt) });

    // 총계
    long totalMembers = Dapper.SqlMapper.ExecuteScalar<long>(conn, "SELECT COUNT(*) FROM members");
    long totalSearches = Dapper.SqlMapper.ExecuteScalar<long>(conn, "SELECT COUNT(*) FROM searchhistory");
    long totalPosts = Dapper.SqlMapper.ExecuteScalar<long>(conn, "SELECT COUNT(*) FROM bbs");
    long totalBookmarks = 0;
    try { totalBookmarks = Dapper.SqlMapper.ExecuteScalar<long>(conn, "SELECT COUNT(*) FROM bookmarks"); } catch { }

    return Results.Json(new
    {
        dailySignups = signups,
        dailySearches = searches,
        dailyPosts = posts,
        totals = new
        {
            members = Convert.ToInt32(totalMembers),
            searches = Convert.ToInt32(totalSearches),
            posts = Convert.ToInt32(totalPosts),
            bookmarks = Convert.ToInt32(totalBookmarks)
        }
    });
});

// ── Extra 패널 재호출 API (카카오맵, 쇼핑, 뉴스, 유튜브, 날씨 등) ──────────

app.MapGet("/api/search/extra", async (HttpContext ctx, ExtraApi extra) =>
{
    string q = ctx.Request.Query["q"].ToString().Trim();
    string type = ctx.Request.Query["type"].ToString().Trim();
    if (string.IsNullOrEmpty(q) || string.IsNullOrEmpty(type))
        return Results.Json(new { data = "{}" });

    // 타입별 개별 호출
    string json = type switch
    {
        "places"   => await extra.GetExtraByTypeAsync(q, "places"),
        "shopping" => await extra.GetExtraByTypeAsync(q, "shopping"),
        "news"     => await extra.GetExtraByTypeAsync(q, "news"),
        "blog"     => await extra.GetExtraByTypeAsync(q, "blog"),
        "youtube"  => await extra.GetExtraByTypeAsync(q, "youtube"),
        "wiki"     => await extra.GetExtraByTypeAsync(q, "wiki"),
        "weather"  => await extra.GetExtraByTypeAsync(q, "weather"),
        "exchange" => await extra.GetExtraByTypeAsync(q, "exchange"),
        _ => "{}"
    };

    return Results.Json(new { data = json });
});

// ── 관련 검색어 재호출 API ───────────────────────────────────────────────────

app.MapGet("/api/search/related", async (HttpContext ctx, ClaudeApi ai) =>
{
    string q = ctx.Request.Query["q"].ToString().Trim();
    if (string.IsNullOrEmpty(q)) return Results.Json(new { related = "[]" });

    string json = await ai.GetRelatedAsync(q);
    return Results.Json(new { related = json });
});

// ── 검색 자동완성 API ────────────────────────────────────────────────────────

app.MapGet("/api/autocomplete", (HttpContext ctx, DbService db) =>
{
    string q = ctx.Request.Query["q"].ToString().Trim();
    if (q.Length < 1 || q.Length > 100)
        return Results.Json(new { suggestions = Array.Empty<string>() });

    using var conn = db.OpenConnection();

    // 최근 검색어 중 일치하는 것 (빈도순 정렬, ILIKE 와일드카드 이스케이프)
    var rows = Dapper.SqlMapper.Query<string>(conn,
        @"SELECT query FROM (
            SELECT query, COUNT(*) AS cnt
            FROM searchhistory
            WHERE query ILIKE @pattern ESCAPE '\'
            GROUP BY query ORDER BY cnt DESC LIMIT 8
          ) sub",
        new { pattern = DriveService.EscapeLikePattern(q) + "%" });

    return Results.Json(new { suggestions = rows.ToList() });
});

// ── 사용자 대시보드 API (개인 검색 통계) ─────────────────────────────────────

app.MapGet("/api/dashboard", (HttpContext ctx, DbService db) =>
{
    string? uid = ctx.Session.GetString("UserID");
    if (uid == null) return Results.Json(new { error = "login_required" });

    using var conn = db.OpenConnection();

    // 내 검색 수 (최근 30일, 일별)
    var mySearches = Dapper.SqlMapper.Query(conn,
        @"SELECT DATE(searchtime) AS day, COUNT(*) AS cnt
          FROM searchhistory WHERE userid = @uid AND searchtime > NOW() - INTERVAL '30 days'
          GROUP BY DATE(searchtime) ORDER BY day",
        new { uid });

    var searches = new List<object>();
    foreach (var r in mySearches)
        searches.Add(new { day = ((DateTime)r.day).ToString("MM-dd"), count = Convert.ToInt32(r.cnt) });

    // 내 자주 사용하는 키워드 TOP 10
    var topKeywords = Dapper.SqlMapper.Query(conn,
        @"SELECT query, COUNT(*) AS cnt
          FROM searchhistory WHERE userid = @uid
          GROUP BY query ORDER BY cnt DESC LIMIT 10",
        new { uid });

    var keywords = new List<object>();
    foreach (var r in topKeywords)
        keywords.Add(new { query = (string)(r.query ?? ""), count = Convert.ToInt32(r.cnt) });

    // 내 게시글 수
    long myPosts = Dapper.SqlMapper.ExecuteScalar<long>(conn,
        "SELECT COUNT(*) FROM bbs WHERE author = @uid", new { uid });

    // 내 북마크 수
    long myBookmarks = 0;
    try { myBookmarks = Dapper.SqlMapper.ExecuteScalar<long>(conn,
        "SELECT COUNT(*) FROM bookmarks WHERE userid = @uid", new { uid }); } catch { }

    // 총 검색 수
    long totalSearches = Dapper.SqlMapper.ExecuteScalar<long>(conn,
        "SELECT COUNT(*) FROM searchhistory WHERE userid = @uid", new { uid });

    return Results.Json(new
    {
        dailySearches = searches,
        topKeywords = keywords,
        totals = new
        {
            searches = Convert.ToInt32(totalSearches),
            posts = Convert.ToInt32(myPosts),
            bookmarks = Convert.ToInt32(myBookmarks)
        }
    });
});

// Cloud Drive API (/api/drive/*) — DriveApi.cs 로 분리 (드라이브 저장소에 따로 올리기 위해)
app.MapDriveApi();
// 예전 아스테라 관리 주소 → 통합 관리자 페이지
app.MapGet("/pokeadmin", () => Results.Redirect("/admin#poke"));

// ── 보안 정리 타이머 — 만료 항목 자동 정리 (메모리 누수 방지) ──────────────
var _cleanupTimer = new System.Threading.Timer(_ =>
{
    // 만료된 레이트리밋 항목 정리
    var now = DateTime.UtcNow;
    foreach (var key in _rateLimits.Keys.ToArray())
    {
        if (_rateLimits.TryGetValue(key, out var entry) && now > entry.ResetAt)
        {
            (int Count, DateTime ResetAt) removed;
            _rateLimits.TryRemove(key, out removed);
        }
    }

    // (업로드·드라이브 세션 정리는 DriveApi.cs 의 타이머가 함)
}, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

// Kestrel 포트 설정: 같은 기계의 Cloudflare 터널(cloudflared)만 접속 → 라즈베리파이 안(127.0.0.1)에서만 열기
//   예전엔 Nginx 앞단 시절 설정(0.0.0.0)이라 집 안 랜 기기도 암호화 없이 바로 들어오고,
//   X-Forwarded-For 를 위조해 IP 기반 제한(로그인 시도 제한 등)을 피할 수 있었음
app.Run("http://127.0.0.1:5000");
