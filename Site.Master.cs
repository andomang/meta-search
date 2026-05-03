using System;
using System.Data.SqlClient;
using System.Web.UI;

/// <summary>
/// 사이트 전체에 공통 적용되는 마스터 페이지 코드비하인드 (Site.Master.cs)
/// 모든 콘텐츠 페이지가 이 마스터 페이지를 상속하여 공통 헤더/내비게이션을 제공한다.
/// 주요 기능:
///   - WebView2 앱에서 주입한 쿠키를 이용한 자동 로그인 처리
///   - DarkMode 쿠키와 Language 쿠키를 세션에 동기화
///   - 알림(Notification) 벨 아이콘 및 읽지 않은 알림 수 표시
///   - 로그인 상태에 따른 내비게이션 메뉴 동적 구성 (로그인/로그아웃 버튼)
///   - 사이트 전역 다국어(Lang) 텍스트 바인딩
///   - 로그인 처리 (btnLogin_Click) 및 로그아웃 처리 (btnLogout_Click)
/// </summary>
public partial class SiteMaster : System.Web.UI.MasterPage
{
    /// <summary>
    /// 마스터 페이지가 로드될 때마다 실행되는 이벤트 핸들러.
    /// 자동 로그인, 세션/쿠키 동기화, 알림 표시, 내비게이션 구성,
    /// 다국어 텍스트 바인딩 등 공통 처리를 수행한다.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // WebView2 앱에서 주입한 쿠키로 자동 로그인
        // 세션에 UserID 가 없지만 UserID 쿠키가 있으면 쿠키 값으로 세션을 복원
        if (Session["UserID"] == null && Request.Cookies["UserID"] != null)
        {
            string uid = Request.Cookies["UserID"].Value;

            // 쿠키의 UserID 를 세션에 저장
            Session["UserID"] = uid;

            // MemberDao 를 통해 DB에서 닉네임을 조회하여 세션에 저장
            MemberDao dao = new MemberDao();
            Session["UserName"] = dao.GetNickname(uid);

            // DB에서 해당 사용자의 다크 모드 설정과 언어 설정을 조회하여 세션에 저장
            bool dm; string lg;
            dao.GetUserSettings(uid, out dm, out lg);
            Session["IsDark"] = dm;
            Session["Lang"] = lg;

            // 세션 복원 후 현재 페이지를 새로고침하여 세션이 적용된 상태로 재로드
            Response.Redirect(Request.RawUrl);
            return;
        }

        // 쿠키와 세션이 불일치할 경우 동기화하고 페이지를 새로고침하는 플래그
        bool needRedirect = false;

        // DarkMode 쿠키가 있을 경우 세션의 IsDark 와 비교하여 불일치 시 동기화
        if (Request.Cookies["DarkMode"] != null)
        {
            bool cookieDark   = Request.Cookies["DarkMode"].Value == "1";
            bool sessionDark  = Session["IsDark"] != null && (bool)Session["IsDark"];

            // 세션에 값이 없거나 쿠키와 세션 값이 다르면 세션을 쿠키 값으로 업데이트
            if (Session["IsDark"] == null || sessionDark != cookieDark)
            { Session["IsDark"] = cookieDark; needRedirect = true; }
        }

        // Language 쿠키가 있을 경우 세션의 Lang 과 비교하여 불일치 시 동기화
        if (Request.Cookies["Language"] != null)
        {
            string cookieLang = Request.Cookies["Language"].Value;

            // 세션에 값이 없거나 쿠키와 세션 값이 다르면 세션을 쿠키 값으로 업데이트
            if (Session["Lang"] == null || Session["Lang"].ToString() != cookieLang)
            { Session["Lang"] = cookieLang; needRedirect = true; }
        }

        // 쿠키-세션 동기화가 발생했으면 변경 사항이 즉시 반영되도록 현재 페이지를 새로고침
        if (needRedirect) { Response.Redirect(Request.RawUrl); return; }

        // 알림 벨
        // 로그인 상태일 때만 알림 영역을 표시
        if (Session["UserID"] != null)
        {
            // 알림 패널(phNotif)을 화면에 표시
            phNotif.Visible      = true;

            // 알림 관련 다국어 텍스트 바인딩
            litNotifTitle.Text   = Lang.Get("notif.title");
            litNotifMarkRead.Text = Lang.Get("notif.markRead");
            litNotifEmpty.Text   = Lang.Get("notif.noNotif");
            litNotifComment.Text = Lang.Get("notif.comment");
            litNotifLike.Text    = Lang.Get("notif.like");

            try
            {
                // Notifications 테이블에서 읽지 않은(IsRead=0) 알림 건수를 조회
                SqlDataReader rn = DbMan.ExecuteReader(
                    string.Format("SELECT COUNT(*) FROM Notifications WHERE UserID='{0}' AND IsRead=0", Session["UserID"]));
                int unread = rn.Read() ? Convert.ToInt32(rn[0]) : 0;
                rn.Close(); DbMan.Close();

                // 읽지 않은 알림이 1개 이상이면 알림 벨 위에 빨간 배지 표시
                // 10개 이상이면 "9+" 로 표시
                if (unread > 0)
                    litNotifBadge.Text = string.Format(
                        "<span class='absolute -top-1 -right-1 w-4 h-4 bg-red-500 text-white text-[10px] rounded-full flex items-center justify-center font-bold pointer-events-none'>{0}</span>",
                        unread > 9 ? "9+" : unread.ToString());
            }
            catch { DbMan.Close(); }
        }

        // 현재 다크 모드 상태를 세션에서 읽어 JavaScript 초기화 스크립트 생성
        // initTheme(true/false) 함수를 호출하여 클라이언트 측 테마를 초기화
        bool isDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        litThemeScript.Text = string.Format("<script>initTheme({0});</script>", isDark.ToString().ToLower());

        // 내비게이션 메뉴 항목 문구를 현재 언어에 맞게 바인딩
        litNavCommunity.Text = Lang.Get("nav.community");
        litNavSettings.Text = Lang.Get("nav.settings");
        litNavLogout.Text = Lang.Get("nav.logout");

        // 로그인 모달의 문구를 현재 언어에 맞게 바인딩
        litLoginTitle.Text = Lang.Get("login.title");
        litNoAccount.Text = Lang.Get("login.noAccount") + " ";
        litRegisterLink.Text = Lang.Get("login.register");
        litLoginClose.Text = Lang.Get("login.close");
        btnLogin.Text = Lang.Get("login.btn");

        // 로그인 폼 입력란의 placeholder(안내 문구)를 현재 언어에 맞게 설정
        txtLoginId.Attributes["placeholder"] = Lang.Get("login.id");
        txtLoginPw.Attributes["placeholder"] = Lang.Get("login.pw");

        if (Session["UserID"] != null)
        {
            // 로그인 상태: 닉네임(없으면 UserID)과 인사말을 내비게이션에 표시
            string name = Session["UserName"] != null ? Session["UserName"].ToString() : Session["UserID"].ToString();
            string greeting = Lang.Get("nav.greeting");
            string mypage = Lang.Get("nav.mypage");

            // 마이페이지 링크와 닉네임+인사말 조합 문자열을 litUserNav 에 바인딩
            litUserNav.Text = string.Format(
                "<a href='MyPage.aspx' class='hover:text-blue-500'>{0}</a>" +
                "<span class='font-bold dark:text-slate-300'>{1}{2}</span>",
                mypage, name, greeting);

            // 로그아웃 버튼을 표시
            btnLogout.Visible = true;
        }
        else
        {
            // 비로그인 상태: 로그인 모달을 여는 버튼을 litUserNav 에 바인딩
            string loginBtn = Lang.Get("nav.login");
            litUserNav.Text = string.Format("<button type='button' onclick='openLogin()' class='px-4 py-2 bg-black dark:bg-blue-600 text-white rounded-full text-xs font-bold'>{0}</button>", loginBtn);

            // 로그아웃 버튼을 숨김
            btnLogout.Visible = false;
        }
    }

    /// <summary>
    /// 로그인 버튼(btnLogin)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 입력된 아이디와 비밀번호를 MemberDao.Authenticate 로 검증한다.
    /// 인증 성공 시 세션에 UserID, UserName, IsDark, Lang 을 저장하고 현재 페이지를 새로고침한다.
    /// 인증 실패 시 오류 메시지를 JavaScript alert 으로 표시한다.
    /// </summary>
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        // 로그인 폼에서 아이디와 비밀번호를 가져와 앞뒤 공백 제거
        string id = txtLoginId.Text.Trim();
        string pw = txtLoginPw.Text.Trim();

        // 아이디 또는 비밀번호가 비어 있으면 필수 입력 안내 메시지를 표시하고 중단
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Response.Write("<script>alert('" + Lang.Get("login.id") + "/" + Lang.Get("login.pw") + " required');</script>");
            return;
        }

        // MemberDao 를 통해 아이디/비밀번호 인증 (DB의 members 테이블에서 검증)
        MemberDao dao = new MemberDao();
        if (dao.Authenticate(id, pw))
        {
            // 인증 성공: 세션에 사용자 ID 와 닉네임(UserName)을 저장
            Session["UserID"] = id;
            Session["UserName"] = dao.GetNickname(id);

            // DB에서 해당 사용자의 다크 모드 설정과 언어 설정을 조회하여 세션에 저장
            bool darkMode; string lang;
            dao.GetUserSettings(id, out darkMode, out lang);
            Session["IsDark"] = darkMode;
            Session["Lang"] = lang;

            // 로그인 세션이 적용된 상태로 현재 페이지를 새로고침
            Response.Redirect(Request.RawUrl);
        }
        else
        {
            // 인증 실패: 아이디 또는 비밀번호 불일치 안내 메시지를 표시
            Response.Write("<script>alert('아이디 또는 비밀번호가 틀렸습니다.');</script>");
        }
    }

    /// <summary>
    /// 로그아웃 버튼(btnLogout)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 서버 세션 전체를 초기화(Clear)하여 로그인 상태를 해제한 뒤
    /// 메인 페이지(Default.aspx)로 이동한다.
    /// </summary>
    protected void btnLogout_Click(object sender, EventArgs e)
    {
        // 모든 세션 값을 초기화하여 로그아웃 처리
        Session.Clear();

        // 메인 페이지로 리다이렉트
        Response.Redirect("Default.aspx");
    }
}
