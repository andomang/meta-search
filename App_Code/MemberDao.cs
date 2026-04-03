using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

public class MemberDao
{
    public MemberDao() { }

    // 1. 로그인 인증
    public bool Authenticate(string id, string pw)
    {
        bool isAuthen = false;
        string encryptedPw = GetMD5(pw);
        string sQuery = string.Format("SELECT userid FROM members WHERE userid='{0}' AND passwd='{1}' AND (status='true' OR status='1')", id, encryptedPw);
        try
        {
            SqlDataReader mReader = DbMan.ExecuteReader(sQuery);
            if (mReader.Read()) isAuthen = true;
            mReader.Close();
        }
        finally { DbMan.Close(); }
        return isAuthen;
    }

    // 2. 아이디 중복 체크
    public bool VerifyUserID(string id)
    {
        bool result = true;
        string sQuery = string.Format("SELECT userid FROM members WHERE userid = '{0}'", id);
        try
        {
            SqlDataReader myReader = DbMan.ExecuteReader(sQuery);
            if (myReader.Read()) result = false;
            myReader.Close();
        }
        finally { DbMan.Close(); }
        return result;
    }

    // 3. 회원가입
    public int RegisterUser(MemberDo mDo)
    {
        int result = 0;
        try
        {
            SqlConnection conn = DbMan.Open();
            SqlCommand myCmd = new SqlCommand("procAddMember", conn);
            myCmd.CommandType = CommandType.StoredProcedure;
            myCmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Char, 15)).Value = mDo.Userid;
            myCmd.Parameters.Add(new SqlParameter("@passwd", SqlDbType.Char, 32)).Value = GetMD5(mDo.Passwd);
            myCmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NChar, 10)).Value = mDo.Name;
            myCmd.Parameters.Add(new SqlParameter("@nickname", SqlDbType.NChar, 10)).Value = mDo.Nickname;
            myCmd.Parameters.Add(new SqlParameter("@email", SqlDbType.Char, 20)).Value = mDo.Email;
            SqlParameter pOut = new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            myCmd.Parameters.Add(pOut);
            myCmd.ExecuteNonQuery();
            result = (int)pOut.Value;
        }
        finally { DbMan.Close(); }
        return result;
    }

    // 4. 설정값 가져오기 (다크모드/언어)
    public void GetUserSettings(string userid, out bool darkMode, out string lang)
    {
        darkMode = false;
        lang = "ko";
        string sql = string.Format("SELECT DarkMode, [Language] FROM members WHERE userid = '{0}'", userid);
        try
        {
            SqlDataReader dr = DbMan.ExecuteReader(sql);
            if (dr.Read())
            {
                darkMode = dr["DarkMode"] != DBNull.Value && Convert.ToBoolean(dr["DarkMode"]);
                lang = dr["Language"] != DBNull.Value ? dr["Language"].ToString().Trim() : "ko";
            }
            dr.Close();
        }
        finally { DbMan.Close(); }
    }

    // 5. 설정값 업데이트 (중복 제거됨)
    public void UpdateSettings(string userid, bool darkMode, string lang)
    {
        string sql = string.Format("UPDATE members SET DarkMode = {0}, [Language] = N'{1}' WHERE userid = '{2}'", darkMode ? 1 : 0, lang, userid);
        try { DbMan.ExecuteNonQuery(sql); }
        finally { DbMan.Close(); }
    }

    // 6. 게시글 작성
    public int WriteBbs(string title, string contents, string author, string fileName, int fileSize)
    {
        int result = 0;
        try
        {
            SqlConnection conn = DbMan.Open();
            SqlCommand myCmd = new SqlCommand("procAddBbs", conn);
            myCmd.CommandType = CommandType.StoredProcedure;
            myCmd.Parameters.Add("@Title", SqlDbType.NVarChar, 100).Value = title;
            myCmd.Parameters.Add("@Contents", SqlDbType.NVarChar, -1).Value = contents;
            myCmd.Parameters.Add("@Author", SqlDbType.Char, 15).Value = author;
            myCmd.Parameters.Add("@BbsID", SqlDbType.Int).Value = 1;
            myCmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 200).Value = (object)fileName ?? DBNull.Value;
            myCmd.Parameters.Add("@FileSize", SqlDbType.Int).Value = fileSize;
            SqlParameter pOut = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            myCmd.Parameters.Add(pOut);
            myCmd.ExecuteNonQuery();
            result = Convert.ToInt32(pOut.Value);
        }
        catch { result = -1; }
        finally { DbMan.Close(); }
        return result;
    }

    public string GetNickname(string uid)
    {
        string nick = "사용자";
        try
        {
            SqlDataReader r = DbMan.ExecuteReader("SELECT nickname FROM members WHERE userid='" + uid + "'");
            if (r.Read()) nick = r["nickname"].ToString().Trim();
            r.Close();
        }
        finally { DbMan.Close(); }
        return nick;
    }

    private string GetMD5(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        using (MD5 md5 = MD5.Create())
        {
            byte[] b = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < b.Length; i++) sb.Append(b[i].ToString("X2"));
            return sb.ToString();
        }
    }
}