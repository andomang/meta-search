using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

public partial class SiteMaster : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // 1. 테마 로직: 세션의 IsDark 값을 읽어 자바스크립트 함수 호출
        bool isDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        litThemeScript.Text = string.Format("<script>initTheme({0});</script>", isDark.ToString().ToLower());

        // 2. 유저 정보 UI
        if (Session["UserID"] != null)
        {
            litUserNav.Text = string.Format("<span class='dark:text-slate-300 font-bold mr-2'>{0}님</span>", Session["UserName"]);
            btnLogout.Visible = true;
        }
        else
        {
            litUserNav.Text = "<button type='button' onclick='openLogin()' class='px-5 py-2 bg-black text-white rounded-full text-sm font-bold'>로그인</button>";
            btnLogout.Visible = false;
        }
    }

    protected void btnLoginSubmit_Click(object sender, EventArgs e)
    {
        string id = txtLoginId.Text.Trim();
        string pw = GetMd5Hash(txtLoginPw.Text.Trim());

        try
        {
            string sql = string.Format("SELECT UserID, Name, ISNULL(DarkMode, 0) as DarkMode FROM members WHERE UserID='{0}' AND passwd='{1}'", id, pw);
            using (SqlDataReader reader = DbMan.ExecuteReader(sql))
            {
                if (reader.Read())
                {
                    Session["UserID"] = reader["UserID"].ToString().Trim();
                    Session["UserName"] = reader["Name"].ToString().Trim();
                    Session["IsDark"] = Convert.ToBoolean(reader["DarkMode"]);
                    DbMan.Close();
                    Response.Redirect(Request.RawUrl);
                }
                else
                {
                    DbMan.Close();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "fail", "alert('정보 불일치');", true);
                }
            }
        }
        catch { DbMan.Close(); }
    }

    private string GetMd5Hash(string input)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }
    }

    protected void btnLogout_Click(object sender, EventArgs e) { Session.Clear(); Response.Redirect("Default.aspx"); }
}