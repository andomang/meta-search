using System;
using System.Web.UI;

public partial class Site : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) UpdateAuthUI();
    }

    private void UpdateAuthUI()
    {
        if (Session["UserID"] != null)
        {
            phAnonymous.Visible = false;
            phAuthorized.Visible = true;
            litUserName.Text = Session["UserName"] != null ? Session["UserName"].ToString() : "사용자";
        }
        else
        {
            phAnonymous.Visible = true;
            phAuthorized.Visible = false;
        }
    }

    protected void btnLoginSubmit_Click(object sender, EventArgs e)
    {
        string id = Request.Form["loginId"];
        string pw = Request.Form["loginPw"];

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)) return;

        MemberDao dao = new MemberDao();
        if (dao.Authenticate(id, pw))
        {
            Session["UserID"] = id;
            Session["UserName"] = dao.GetNickname(id);

            // 💎 로그인 시 DB에서 다크모드/언어 설정 로드
            bool isDark; string lang;
            dao.GetUserSettings(id, out isDark, out lang);
            Session["DarkMode"] = isDark;
            Session["Language"] = lang;

            Response.Redirect(Request.RawUrl); // 현재 페이지 새로고침하여 적용
        }
        else
        {
            Response.Write("<script>alert('아이디 또는 비밀번호가 올바르지 않습니다.');</script>");
        }
    }

    protected void btnLogout_Click(object sender, EventArgs e)
    {
        Session.Clear();
        Session.Abandon();
        Response.Redirect("~/Default.aspx");
    }
}