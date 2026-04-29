using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 회원 관련 DB 작업 클래스
/// members, Bbs 테이블을 대상으로
/// 로그인/회원가입/설정/게시글 작성 등을 저장 프로시저 방식으로 처리
/// </summary>
public class MemberDao
{
    public MemberDao() { }

    /// <summary>
    /// 로그인 인증
    /// 입력받은 비밀번호를 MD5로 해시 후 DB와 비교
    /// status가 'true' 또는 '1'인 활성 계정만 인증
    /// </summary>
    public bool Authenticate(string id, string pw)
    {
        bool isAuthen = false;
        string encryptedPw = GetMD5(pw);
        string sQuery = string.Format(
            "SELECT userid FROM members WHERE userid='{0}' AND passwd='{1}' AND (status='true' OR status='1')",
            id, encryptedPw);
        try
        {
            SqlDataReader mReader = DbMan.ExecuteReader(sQuery);
            if (mReader.Read()) isAuthen = true;
            mReader.Close();
        }
        finally { DbMan.Close(); }
        return isAuthen;
    }

    /// <summary>
    /// 아이디 중복 체크
    /// 이미 존재하면 false, 사용 가능하면 true 반환
    /// </summary>
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

    /// <summary>
    /// 회원가입 처리
    /// 저장 프로시저 procAddMember 호출 (중복 체크 포함)
    /// 반환값: 1 = 성공, -1 = 아이디 중복
    /// </summary>
    public int RegisterUser(MemberDo mDo)
    {
        int result = 0;
        try
        {
            SqlConnection conn = DbMan.Open();
            SqlCommand myCmd = new SqlCommand("procAddMember", conn);
            myCmd.CommandType = CommandType.StoredProcedure;
            myCmd.Parameters.Add(new SqlParameter("@userid",   SqlDbType.Char,   15)).Value = mDo.Userid;
            myCmd.Parameters.Add(new SqlParameter("@passwd",   SqlDbType.Char,   32)).Value = GetMD5(mDo.Passwd);
            myCmd.Parameters.Add(new SqlParameter("@name",     SqlDbType.NChar,  10)).Value = mDo.Name;
            myCmd.Parameters.Add(new SqlParameter("@nickname", SqlDbType.NChar,  10)).Value = mDo.Nickname;
            myCmd.Parameters.Add(new SqlParameter("@email",    SqlDbType.Char,   20)).Value = mDo.Email;
            SqlParameter pOut = new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            myCmd.Parameters.Add(pOut);
            myCmd.ExecuteNonQuery();
            result = (int)pOut.Value;
        }
        finally { DbMan.Close(); }
        return result;
    }

    /// <summary>
    /// 유저 설정값 조회 (다크모드, 언어)
    /// 로그인 시 세션에 저장하기 위해 사용
    /// out 파라미터로 다크모드 여부와 언어 코드 반환
    /// </summary>
    public void GetUserSettings(string userid, out bool darkMode, out string lang)
    {
        darkMode = false;
        lang = "ko";
        string sql = string.Format(
            "SELECT DarkMode, [Language] FROM members WHERE userid = '{0}'", userid);
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

    /// <summary>
    /// 유저 설정값 업데이트 (다크모드, 언어)
    /// </summary>
    public void UpdateSettings(string userid, bool darkMode, string lang)
    {
        string sql = string.Format(
            "UPDATE members SET DarkMode = {0}, [Language] = N'{1}' WHERE userid = '{2}'",
            darkMode ? 1 : 0, lang, userid);
        try { DbMan.ExecuteNonQuery(sql); }
        finally { DbMan.Close(); }
    }

    /// <summary>
    /// 게시글 작성
    /// 저장 프로시저 procAddBbs 호출
    /// BbsID는 현재 1(자유게시판)로 고정
    /// 반환값: 1 = 성공, -1 = 실패
    /// </summary>
    public int WriteBbs(string title, string contents, string author, string fileName, int fileSize)
    {
        int result = 0;
        try
        {
            SqlConnection conn = DbMan.Open();
            SqlCommand myCmd = new SqlCommand("procAddBbs", conn);
            myCmd.CommandType = CommandType.StoredProcedure;
            myCmd.Parameters.Add("@Title",    SqlDbType.NVarChar, 100).Value = title;
            myCmd.Parameters.Add("@Contents", SqlDbType.NVarChar, -1).Value  = contents;
            myCmd.Parameters.Add("@Author",   SqlDbType.Char,     15).Value  = author;
            myCmd.Parameters.Add("@BbsID",    SqlDbType.Int).Value           = 1;
            myCmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 200).Value = (object)fileName ?? DBNull.Value;
            myCmd.Parameters.Add("@FileSize", SqlDbType.Int).Value           = fileSize;
            SqlParameter pOut = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            myCmd.Parameters.Add(pOut);
            myCmd.ExecuteNonQuery();
            result = Convert.ToInt32(pOut.Value);
        }
        catch { result = -1; }
        finally { DbMan.Close(); }
        return result;
    }

    /// <summary>
    /// 게시글 수정
    /// No(게시글 번호) 기준으로 제목과 내용 UPDATE
    /// Author 검증은 CommunityEdit.aspx.cs에서 세션으로 처리
    /// </summary>
    public int UpdateBbs(int no, string title, string contents)
    {
        int result = 0;
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Bbs SET Title=@title, Contents=@contents WHERE No=@no", conn);
                cmd.Parameters.AddWithValue("@title",    title);
                cmd.Parameters.AddWithValue("@contents", contents);
                cmd.Parameters.AddWithValue("@no",       no);
                result = cmd.ExecuteNonQuery();
                DbMan.Close();
            }
        }
        catch { DbMan.Close(); result = -1; }
        return result;
    }

    /// <summary>
    /// 닉네임 조회
    /// 로그인 시 Session["UserName"]에 저장하기 위해 사용
    /// </summary>
    public string GetNickname(string uid)
    {
        string nick = "사용자";
        try
        {
            SqlDataReader r = DbMan.ExecuteReader(
                "SELECT nickname FROM members WHERE userid='" + uid + "'");
            if (r.Read()) nick = r["nickname"].ToString().Trim();
            r.Close();
        }
        finally { DbMan.Close(); }
        return nick;
    }

    /// <summary>
    /// 특정 유저의 게시글 목록 조회
    /// 마이페이지에서 "내 게시글" 표시에 사용
    /// 최신순 정렬, top 개수 제한 가능
    /// </summary>
    public DataTable GetMyPosts(string userID, int top = 5)
    {
        string sql = string.Format(@"
            SELECT TOP {0} No, Title, UploadTime, Hits
            FROM Bbs
            WHERE Author = '{1}'
            ORDER BY No DESC", top, userID);
        DataSet ds = DbMan.DataAdapterFill(sql, "MyPosts");
        return ds.Tables[0];
    }

    /// <summary>
    /// MD5 해시 함수
    /// 비밀번호 암호화에 사용 (ASCII 인코딩 기반)
    /// 대문자 HEX 문자열 반환
    /// </summary>
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
