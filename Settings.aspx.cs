using System;
using System.Data.SqlClient;
using System.Web.UI;

public partial class Settings : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserID"] == null) { Response.Redirect("Default.aspx"); return; }
        if (!IsPostBack) LoadUserData();

        // 현재 테마 상태를 On/Off로 시각화
        bool isDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        litThemeStatus.Text = isDark ? "<span class='text-blue-500 font-bold uppercase'>On</span>" : "<span class='text-gray-400 font-bold uppercase'>Off</span>";
    }

    private void LoadUserData()
    {
        try
        {
            string sql = string.Format("SELECT Name, Email FROM members WHERE UserID = '{0}'", Session["UserID"]);
            using (SqlDataReader reader = DbMan.ExecuteReader(sql))
            {
                if (reader.Read())
                {
                    editName.Text = reader["Name"].ToString().Trim();
                    editEmail.Text = reader["Email"].ToString().Trim();
                }
                DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
    }

    protected void btnToggleDark_Click(object sender, EventArgs e)
    {
        bool currentDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        bool nextDark = !currentDark;

        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                string sql = "UPDATE members SET DarkMode=@dark WHERE UserID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@dark", nextDark ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery();
                DbMan.Close();

                Session["IsDark"] = nextDark;
                // [핵심] 다시 불러와야 마스터의 initTheme()가 호출되며 클래스가 바뀜
                Response.Redirect("Settings.aspx");
            }
        }
        catch { DbMan.Close(); }
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                string sql = "UPDATE members SET Name=@name, Email=@email WHERE UserID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", editName.Text.Trim());
                cmd.Parameters.AddWithValue("@email", editEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery();
                DbMan.Close();
                Session["UserName"] = editName.Text.Trim();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ok", "alert('정보가 업데이트되었습니다.'); location.href='Settings.aspx';", true);
            }
        }
        catch { DbMan.Close(); }
    }
}