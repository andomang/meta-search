using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

public partial class Register : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        litPageTitle.Text = Lang.Get("reg.title");
        litIdLbl.Text = Lang.Get("reg.id");
        litPwLbl.Text = Lang.Get("reg.pw");
        litNameLbl.Text = Lang.Get("reg.name");
        litNickLbl.Text = Lang.Get("reg.nick");
        litEmailLbl.Text = Lang.Get("reg.email");
        litCancelBtn.Text = Lang.Get("reg.cancel");
        btnRegister.Text = Lang.Get("reg.submit");
        txtUserID.Attributes["placeholder"] = Lang.Get("reg.idPh");
        txtPassword.Attributes["placeholder"] = Lang.Get("reg.pwPh");
    }

    protected void btnRegister_Click(object sender, EventArgs e)
    {
        string userId = txtUserID.Text.Trim();
        string rawPassword = txtPassword.Text.Trim();
        string name = txtName.Text.Trim();
        string nickname = txtNickname.Text.Trim();
        string email = txtEmail.Text.Trim();

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(rawPassword)) return;

        try
        {
            string md5Password = GetMd5Hash(rawPassword);
            using (SqlConnection conn = DbMan.Open())
            {
                string sql = "INSERT INTO members (UserID, passwd, Name, Nickname, Email, DarkMode, status) VALUES (@id, @pw, @name, @nick, @email, 0, '1')";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@pw", md5Password);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@nick", nickname);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.ExecuteNonQuery();
                DbMan.Close();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ok",
                    "alert('회원가입이 완료되었습니다!'); location.href='Default.aspx';", true);
            }
        }
        catch (Exception ex)
        {
            DbMan.Close();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "err",
                "alert('가입 실패: " + ex.Message.Replace("'", "") + "');", true);
        }
    }

    private string GetMd5Hash(string input)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("X2"));
            return sBuilder.ToString();
        }
    }
}