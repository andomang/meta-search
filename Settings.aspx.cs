using System;
using System.Data.SqlClient;
using System.Web.UI;

public partial class Settings : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // 세션에 저장된 ID가 있다면 사용하고, 없으면 테스트용 'testUser'를 사용합니다.
            string userId = (Session["UserID"] != null) ? Session["UserID"].ToString() : "testUser";
            LoadUserData(userId);
        }
    }

    private void LoadUserData(string userId)
    {
        SqlDataReader reader = null;
        try
        {
            // [수정] 테이블 이름을 Users에서 Members로 변경했습니다.
            string sql = string.Format("SELECT Name, Email FROM Members WHERE UserID = '{0}'", userId);
            reader = DbMan.ExecuteReader(sql);

            if (reader.Read())
            {
                string name = reader["Name"].ToString();
                string email = reader["Email"].ToString();

                lblUserName.Text = name;
                lblEmail.Text = email;
                litUserNameTitle.Text = name;
                litAvatar.Text = !string.IsNullOrEmpty(name) ? name.Substring(0, 1) : "M";
            }
            else
            {
                lblUserName.Text = "미등록 사용자";
                lblEmail.Text = "Members 테이블에 해당 ID가 없습니다.";
                litUserNameTitle.Text = "Guest";
                litAvatar.Text = "?";
            }
        }
        catch (Exception ex)
        {
            // 이제 '개체 이름 유효하지 않음' 에러가 사라져야 합니다.
            Response.Write("<script>alert('조회 에러: " + ex.Message.Replace("'", "") + "');</script>");
        }
        finally
        {
            if (reader != null) reader.Close();
            DbMan.Close();
        }
    }
}