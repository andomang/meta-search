using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

public partial class Register : System.Web.UI.Page
{
    protected void btnRegister_Click(object sender, EventArgs e)
    {
        // An님의 ASPX ID와 100% 매칭
        string userId = txtUserID.Text.Trim();
        string rawPassword = txtPassword.Text.Trim();
        string name = txtName.Text.Trim();
        string nickname = txtNickname.Text.Trim();
        string email = txtEmail.Text.Trim();

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(rawPassword)) return;

        try
        {
            // [중요] 회원가입 시 MD5 암호화 적용
            string md5Password = GetMd5Hash(rawPassword);

            using (SqlConnection conn = DbMan.Open())
            {
                string sql = "INSERT INTO members (UserID, passwd, Name, Nickname, Email, DarkMode) VALUES (@id, @pw, @name, @nick, @email, 0)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@pw", md5Password);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@nick", nickname);
                cmd.Parameters.AddWithValue("@email", email);

                cmd.ExecuteNonQuery();
                DbMan.Close();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ok", "alert('회원가입이 완료되었습니다!'); location.href='Default.aspx';", true);
            }
        }
        catch (Exception ex)
        {
            DbMan.Close();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "err", "alert('가입 실패: " + ex.Message.Replace("'", "") + "');", true);
        }
    }

    // 깃허브 원본과 동일한 MD5 해시 함수
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
}