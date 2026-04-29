using System;
using System.Data.SqlClient;
using System.Web.UI;

public partial class SiteMaster : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // WebView2 앱에서 주입한 쿠키로 자동 로그인
        if (Session["UserID"] == null && Request.Cookies["UserID"] != null)
        {
            string uid = Request.Cookies["UserID"].Value;
            Session["UserID"] = uid;
            MemberDao dao = new MemberDao();
            Session["UserName"] = dao.GetNickname(uid);
            bool dm; string lg;
            dao.GetUserSettings(uid, out dm, out lg);
            Session["IsDark"] = dm;
            Session["Lang"] = lg;
            Response.Redirect(Request.RawUrl);
            return;
        }

        bool needRedirect = false;
        if (Request.Cookies["DarkMode"] != null)
        {
            bool cookieDark   = Request.Cookies["DarkMode"].Value == "1";
            bool sessionDark  = Session["IsDark"] != null && (bool)Session["IsDark"];
            if (Session["IsDark"] == null || sessionDark != cookieDark)
            { Session["IsDark"] = cookieDark; needRedirect = true; }
        }
        if (Request.Cookies["Language"] != null)
        {
            string cookieLang = Request.Cookies["Language"].Value;
            if (Session["Lang"] == null || Session["Lang"].ToString() != cookieLang)
            { Session["Lang"] = cookieLang; needRedirect = true; }
        }
        if (needRedirect) { Response.Redirect(Request.RawUrl); return; }

        // 알림 벨
        if (Session["UserID"] != null)
        {
            phNotif.Visible      = true;
            litNotifTitle.Text   = Lang.Get("notif.title");
            litNotifMarkRead.Text = Lang.Get("notif.markRead");
            litNotifEmpty.Text   = Lang.Get("notif.noNotif");
            litNotifComment.Text = Lang.Get("notif.comment");
            litNotifLike.Text    = Lang.Get("notif.like");
            try
            {
                SqlDataReader rn = DbMan.ExecuteReader(
                    string.Format("SELECT COUNT(*) FROM Notifications WHERE UserID='{0}' AND IsRead=0", Session["UserID"]));
                int unread = rn.Read() ? Convert.ToInt32(rn[0]) : 0;
                rn.Close(); DbMan.Close();
                if (unread > 0)
                    litNotifBadge.Text = string.Format(
                        "<span class='absolute -top-1 -right-1 w-4 h-4 bg-red-500 text-white text-[10px] rounded-full flex items-center justify-center font-bold pointer-events-none'>{0}</span>",
                        unread > 9 ? "9+" : unread.ToString());
            }
            catch { DbMan.Close(); }
        }

        bool isDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        litThemeScript.Text = string.Format("<script>initTheme({0});</script>", isDark.ToString().ToLower());

        litNavCommunity.Text = Lang.Get("nav.community");
        litNavSettings.Text = Lang.Get("nav.settings");
        litNavLogout.Text = Lang.Get("nav.logout");
        litLoginTitle.Text = Lang.Get("login.title");
        litNoAccount.Text = Lang.Get("login.noAccount") + " ";
        litRegisterLink.Text = Lang.Get("login.register");
        litLoginClose.Text = Lang.Get("login.close");
        btnLogin.Text = Lang.Get("login.btn");
        txtLoginId.Attributes["placeholder"] = Lang.Get("login.id");
        txtLoginPw.Attributes["placeholder"] = Lang.Get("login.pw");

        if (Session["UserID"] != null)
        {
            string name = Session["UserName"] != null ? Session["UserName"].ToString() : Session["UserID"].ToString();
            string greeting = Lang.Get("nav.greeting");
            string mypage = Lang.Get("nav.mypage");
            litUserNav.Text = string.Format(
                "<a href='MyPage.aspx' class='hover:text-blue-500'>{0}</a>" +
                "<span class='font-bold dark:text-slate-300'>{1}{2}</span>",
                mypage, name, greeting);
            btnLogout.Visible = true;
        }
        else
        {
            string loginBtn = Lang.Get("nav.login");
            litUserNav.Text = string.Format("<button type='button' onclick='openLogin()' class='px-4 py-2 bg-black dark:bg-blue-600 text-white rounded-full text-xs font-bold'>{0}</button>", loginBtn);
            btnLogout.Visible = false;
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        string id = txtLoginId.Text.Trim();
        string pw = txtLoginPw.Text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Response.Write("<script>alert('" + Lang.Get("login.id") + "/" + Lang.Get("login.pw") + " required');</script>");
            return;
        }

        MemberDao dao = new MemberDao();
        if (dao.Authenticate(id, pw))
        {
            Session["UserID"] = id;
            Session["UserName"] = dao.GetNickname(id);
            bool darkMode; string lang;
            dao.GetUserSettings(id, out darkMode, out lang);
            Session["IsDark"] = darkMode;
            Session["Lang"] = lang;
            Response.Redirect(Request.RawUrl);
        }
        else
        {
            Response.Write("<script>alert('아이디 또는 비밀번호가 틀렸습니다.');</script>");
        }
    }

    protected void btnLogout_Click(object sender, EventArgs e)
    {
        Session.Clear();
        Response.Redirect("Default.aspx");
    }
}