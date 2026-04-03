using System;
public partial class Settings : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserID"] == null) Response.Redirect("Default.aspx");
        if (!IsPostBack) LoadCurrentSettings();
    }
    private void LoadCurrentSettings()
    {
        bool isDark; string lang;
        new MemberDao().GetUserSettings(Session["UserID"].ToString(), out isDark, out lang);
        chkDarkMode.Checked = isDark;
        ddlLanguage.SelectedValue = lang;
        Session["DarkMode"] = isDark;
    }
    protected void SaveSettings(object sender, EventArgs e)
    {
        bool isDark = chkDarkMode.Checked;
        new MemberDao().UpdateSettings(Session["UserID"].ToString(), isDark, ddlLanguage.SelectedValue);
        Session["DarkMode"] = isDark; // 💎 세션 즉시 업데이트
        Response.Redirect(Request.RawUrl); // 💎 화면 새로고침으로 마스터페이지 적용
    }
}