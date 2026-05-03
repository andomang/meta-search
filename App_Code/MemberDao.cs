using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 회원(Member) 관련 데이터베이스 작업을 담당하는 DAO(Data Access Object) 클래스.
/// [DAO 레이어] - UI(aspx 페이지)와 DB 사이에서 데이터를 주고받는 중간 계층.
///
/// 대상 테이블: members (회원 정보), Bbs (게시글)
///
/// 주요 기능:
///   - 로그인 인증 (Authenticate)
///   - 아이디 중복 확인 (VerifyUserID)
///   - 회원가입 (RegisterUser) - 저장 프로시저 procAddMember 사용
///   - 다크모드/언어 설정 조회·수정 (GetUserSettings, UpdateSettings)
///   - 게시글 작성·수정 (WriteBbs, UpdateBbs) - 저장 프로시저 procAddBbs 사용
///   - 닉네임 조회 (GetNickname)
///   - 내 게시글 목록 조회 (GetMyPosts)
///
/// Login.aspx, Register.aspx, Settings.aspx, MyPage.aspx,
/// CommunityWrite.aspx, CommunityEdit.aspx 등 여러 페이지에서 사용된다.
/// </summary>
public class MemberDao
{
    // 기본 생성자 (별도 초기화 로직 없음)
    public MemberDao() { }

    /// <summary>
    /// 사용자 아이디와 비밀번호를 검증하여 로그인 가능 여부를 반환한다.
    /// 비밀번호는 MD5로 해시(암호화)한 뒤 DB에 저장된 해시값과 비교하고,
    /// status 컬럼이 'true' 또는 '1'인 활성 계정만 인증을 허용한다.
    /// </summary>
    /// <param name="id">로그인 시 입력한 아이디</param>
    /// <param name="pw">로그인 시 입력한 평문(Plain Text) 비밀번호</param>
    /// <returns>인증 성공이면 true, 실패(아이디 없음/비밀번호 불일치/비활성 계정)면 false</returns>
    public bool Authenticate(string id, string pw)
    {
        // 인증 결과 초기값은 false (인증 실패)
        bool isAuthen = false;
        // 입력 비밀번호를 MD5 해시값으로 변환 (DB에는 해시값이 저장되어 있음)
        string encryptedPw = GetMD5(pw);
        // 아이디·해시된 비밀번호·활성 상태를 동시에 확인하는 SELECT 쿼리
        string sQuery = string.Format(
            "SELECT userid FROM members WHERE userid='{0}' AND passwd='{1}' AND (status='true' OR status='1')",
            id, encryptedPw);
        try
        {
            // 쿼리 실행 후 SqlDataReader로 결과를 읽음
            SqlDataReader mReader = DbMan.ExecuteReader(sQuery);
            // Read()가 true이면 해당 조건의 행이 존재 → 인증 성공
            if (mReader.Read()) isAuthen = true;
            // Reader 닫기 (연결 유지 중이므로 반드시 닫아야 함)
            mReader.Close();
        }
        finally
        {
            // 예외 발생 여부와 관계없이 DB 연결 반드시 닫기
            DbMan.Close();
        }
        return isAuthen;
    }

    /// <summary>
    /// 입력한 아이디가 이미 members 테이블에 존재하는지 확인한다.
    /// 회원가입 폼에서 중복 아이디 검사 시 호출된다.
    /// </summary>
    /// <param name="id">중복 여부를 확인할 아이디 문자열</param>
    /// <returns>사용 가능한 아이디면 true, 이미 존재하면 false</returns>
    public bool VerifyUserID(string id)
    {
        // 기본값: 사용 가능 (true)
        bool result = true;
        // 해당 아이디가 존재하는지 조회하는 SELECT 쿼리
        string sQuery = string.Format("SELECT userid FROM members WHERE userid = '{0}'", id);
        try
        {
            SqlDataReader myReader = DbMan.ExecuteReader(sQuery);
            // 결과 행이 있으면 이미 존재하는 아이디 → false로 변경
            if (myReader.Read()) result = false;
            // Reader 닫기
            myReader.Close();
        }
        finally
        {
            // DB 연결 반드시 닫기
            DbMan.Close();
        }
        return result;
    }

    /// <summary>
    /// 새 회원을 DB에 등록한다.
    /// 저장 프로시저 procAddMember를 호출하며,
    /// 프로시저 내부에서 아이디 중복 체크와 INSERT를 함께 처리한다.
    /// 비밀번호는 이 메서드에서 MD5로 해시한 후 전달한다.
    /// </summary>
    /// <param name="mDo">
    /// 회원 정보를 담은 MemberDo 도메인 객체
    /// (Userid, Passwd, Name, Nickname, Email 필드 사용)
    /// </param>
    /// <returns>1 = 회원가입 성공, -1 = 아이디 중복으로 실패</returns>
    public int RegisterUser(MemberDo mDo)
    {
        // 결과 초기값 0 (아직 실행 전)
        int result = 0;
        try
        {
            // DB 연결 열기
            SqlConnection conn = DbMan.Open();
            // 저장 프로시저 procAddMember를 호출하는 SqlCommand 생성
            SqlCommand myCmd = new SqlCommand("procAddMember", conn);
            // 명령 유형을 StoredProcedure로 설정 (기본값은 Text)
            myCmd.CommandType = CommandType.StoredProcedure;
            // 각 파라미터를 SqlDbType과 최대 길이를 지정하여 추가
            myCmd.Parameters.Add(new SqlParameter("@userid",   SqlDbType.Char,   15)).Value = mDo.Userid;
            // 비밀번호는 MD5 해시값으로 변환하여 저장 (평문 저장 금지)
            myCmd.Parameters.Add(new SqlParameter("@passwd",   SqlDbType.Char,   32)).Value = GetMD5(mDo.Passwd);
            myCmd.Parameters.Add(new SqlParameter("@name",     SqlDbType.NChar,  10)).Value = mDo.Name;
            myCmd.Parameters.Add(new SqlParameter("@nickname", SqlDbType.NChar,  10)).Value = mDo.Nickname;
            myCmd.Parameters.Add(new SqlParameter("@email",    SqlDbType.Char,   20)).Value = mDo.Email;
            // OUTPUT 파라미터 추가: 저장 프로시저가 성공/실패 코드를 여기에 담아 반환
            SqlParameter pOut = new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            myCmd.Parameters.Add(pOut);
            // 저장 프로시저 실행
            myCmd.ExecuteNonQuery();
            // OUTPUT 파라미터에서 결과값 꺼내기 (1: 성공, -1: 중복)
            result = (int)pOut.Value;
        }
        finally
        {
            // 예외 발생 여부와 관계없이 DB 연결 닫기
            DbMan.Close();
        }
        return result;
    }

    /// <summary>
    /// 특정 유저의 설정값(다크모드 여부, 언어 코드)을 DB에서 읽어온다.
    /// 로그인 성공 후 세션(Session["DarkMode"], Session["Lang"])에 저장하기 위해 사용된다.
    /// C# out 키워드를 사용하여 두 값을 동시에 반환한다.
    /// </summary>
    /// <param name="userid">설정값을 조회할 회원 아이디</param>
    /// <param name="darkMode">DB에서 읽은 다크모드 활성 여부 (true/false)</param>
    /// <param name="lang">DB에서 읽은 언어 코드 (예: "ko", "en")</param>
    public void GetUserSettings(string userid, out bool darkMode, out string lang)
    {
        // out 파라미터의 기본값 설정 (DB 조회 실패 시 이 값이 사용됨)
        darkMode = false;
        lang = "ko";
        // DarkMode와 Language 두 컬럼을 조회하는 SELECT 쿼리
        string sql = string.Format(
            "SELECT DarkMode, [Language] FROM members WHERE userid = '{0}'", userid);
        try
        {
            SqlDataReader dr = DbMan.ExecuteReader(sql);
            if (dr.Read())
            {
                // DarkMode 컬럼: DBNull(값 없음)이면 false, 값이 있으면 bool로 변환
                darkMode = dr["DarkMode"] != DBNull.Value && Convert.ToBoolean(dr["DarkMode"]);
                // Language 컬럼: DBNull이면 기본값 "ko", 값이 있으면 공백 제거 후 반환
                lang = dr["Language"] != DBNull.Value ? dr["Language"].ToString().Trim() : "ko";
            }
            // Reader 닫기
            dr.Close();
        }
        finally
        {
            // DB 연결 닫기
            DbMan.Close();
        }
    }

    /// <summary>
    /// 특정 유저의 다크모드 여부와 언어 설정을 DB에 저장한다.
    /// Settings.aspx에서 사용자가 설정을 변경할 때 호출된다.
    /// </summary>
    /// <param name="userid">설정을 변경할 회원 아이디</param>
    /// <param name="darkMode">저장할 다크모드 상태 (true = 활성화)</param>
    /// <param name="lang">저장할 언어 코드 (예: "ko", "en")</param>
    public void UpdateSettings(string userid, bool darkMode, string lang)
    {
        // DarkMode는 bool을 1(true)/0(false)로 변환하여 저장
        // Language는 유니코드 문자열이므로 N'' 접두사(N-prefix) 사용
        string sql = string.Format(
            "UPDATE members SET DarkMode = {0}, [Language] = N'{1}' WHERE userid = '{2}'",
            darkMode ? 1 : 0, lang, userid);
        try
        {
            // 결과 없는 UPDATE 쿼리 실행
            DbMan.ExecuteNonQuery(sql);
        }
        finally
        {
            // DB 연결 닫기
            DbMan.Close();
        }
    }

    /// <summary>
    /// 새 게시글을 Bbs 테이블에 저장한다.
    /// 저장 프로시저 procAddBbs를 호출하며 파일 첨부 정보도 함께 저장한다.
    /// BbsID는 현재 1(자유게시판)로 고정되어 있다.
    /// </summary>
    /// <param name="title">게시글 제목</param>
    /// <param name="contents">게시글 본문 내용 (HTML 허용, NVarChar MAX)</param>
    /// <param name="author">작성자 아이디 (세션에서 가져온 값)</param>
    /// <param name="fileName">첨부 파일 이름 (없으면 null 또는 빈 문자열)</param>
    /// <param name="fileSize">첨부 파일 크기(바이트) (없으면 0)</param>
    /// <returns>1 = 작성 성공, -1 = 오류 발생</returns>
    public int WriteBbs(string title, string contents, string author, string fileName, int fileSize)
    {
        // 결과 초기값 0
        int result = 0;
        try
        {
            // DB 연결 열기
            SqlConnection conn = DbMan.Open();
            // 저장 프로시저 procAddBbs를 호출하는 SqlCommand 생성
            SqlCommand myCmd = new SqlCommand("procAddBbs", conn);
            // 명령 유형을 StoredProcedure로 설정
            myCmd.CommandType = CommandType.StoredProcedure;
            // 제목 파라미터 (유니코드 문자열, 최대 100자)
            myCmd.Parameters.Add("@Title",    SqlDbType.NVarChar, 100).Value = title;
            // 본문 파라미터 (-1은 NVarChar(MAX)를 의미 - 제한 없는 긴 텍스트)
            myCmd.Parameters.Add("@Contents", SqlDbType.NVarChar, -1).Value  = contents;
            // 작성자 아이디 파라미터 (Char 15자)
            myCmd.Parameters.Add("@Author",   SqlDbType.Char,     15).Value  = author;
            // 게시판 구분 ID: 1 = 자유게시판 (고정값)
            myCmd.Parameters.Add("@BbsID",    SqlDbType.Int).Value           = 1;
            // 파일 이름: fileName이 null이면 DBNull.Value로 저장 (null 병합 연산자 활용)
            myCmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 200).Value = (object)fileName ?? DBNull.Value;
            // 파일 크기 파라미터 (첨부 없으면 0)
            myCmd.Parameters.Add("@FileSize", SqlDbType.Int).Value           = fileSize;
            // OUTPUT 파라미터: 저장 프로시저 성공/실패 결과를 받음
            SqlParameter pOut = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            myCmd.Parameters.Add(pOut);
            // 저장 프로시저 실행
            myCmd.ExecuteNonQuery();
            // OUTPUT 파라미터 값을 정수로 변환하여 저장
            result = Convert.ToInt32(pOut.Value);
        }
        catch
        {
            // 예외 발생 시 실패를 나타내는 -1 반환
            result = -1;
        }
        finally
        {
            // 성공/실패 무관하게 DB 연결 닫기
            DbMan.Close();
        }
        return result;
    }

    /// <summary>
    /// 기존 게시글의 제목과 내용을 수정한다.
    /// 게시글 번호(No)를 기준으로 Bbs 테이블의 해당 행을 UPDATE한다.
    /// 작성자 본인 확인은 CommunityEdit.aspx.cs에서 세션으로 미리 처리되어 있다.
    /// </summary>
    /// <param name="no">수정할 게시글의 고유 번호 (Primary Key)</param>
    /// <param name="title">새 제목</param>
    /// <param name="contents">새 본문 내용</param>
    /// <returns>영향받은 행 수 (보통 1 = 성공), -1 = 오류 발생</returns>
    public int UpdateBbs(int no, string title, string contents)
    {
        // 결과 초기값 0
        int result = 0;
        try
        {
            // using 블록: 블록을 벗어나면 SqlConnection이 자동으로 Dispose(자원 반환)됨
            using (SqlConnection conn = DbMan.Open())
            {
                // 게시글 번호(@no)를 기준으로 제목(@title)과 내용(@contents)을 UPDATE하는 쿼리
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Bbs SET Title=@title, Contents=@contents WHERE No=@no", conn);
                // SQL Injection 방지를 위해 파라미터 바인딩 방식 사용 (AddWithValue)
                cmd.Parameters.AddWithValue("@title",    title);
                cmd.Parameters.AddWithValue("@contents", contents);
                cmd.Parameters.AddWithValue("@no",       no);
                // UPDATE 실행 후 영향받은 행 수를 result에 저장
                result = cmd.ExecuteNonQuery();
                // DB 연결 명시적으로 닫기
                DbMan.Close();
            }
        }
        catch
        {
            // 예외 발생 시 연결 닫고 -1 반환
            DbMan.Close();
            result = -1;
        }
        return result;
    }

    /// <summary>
    /// 특정 아이디의 닉네임을 members 테이블에서 조회한다.
    /// 로그인 성공 후 Session["UserName"]에 저장하여 화면에 "OOO 님" 형태로 표시하는 데 사용된다.
    /// </summary>
    /// <param name="uid">닉네임을 조회할 회원 아이디</param>
    /// <returns>해당 아이디의 닉네임 문자열, 조회 실패 시 기본값 "사용자" 반환</returns>
    public string GetNickname(string uid)
    {
        // 조회 실패 시 반환할 기본 닉네임
        string nick = "사용자";
        try
        {
            // 해당 아이디의 nickname 컬럼을 SELECT
            SqlDataReader r = DbMan.ExecuteReader(
                "SELECT nickname FROM members WHERE userid='" + uid + "'");
            // 결과 행이 존재하면 nickname 값을 읽어 앞뒤 공백 제거 후 저장
            if (r.Read()) nick = r["nickname"].ToString().Trim();
            // Reader 닫기
            r.Close();
        }
        finally
        {
            // DB 연결 닫기
            DbMan.Close();
        }
        return nick;
    }

    /// <summary>
    /// 특정 유저가 작성한 게시글 목록을 최신순으로 조회한다.
    /// MyPage.aspx의 "내 게시글" 카드에 표시되며,
    /// top 파라미터로 조회 개수를 제한할 수 있다 (기본값 5개).
    /// </summary>
    /// <param name="userID">게시글을 조회할 회원 아이디</param>
    /// <param name="top">가져올 최대 게시글 수 (기본값: 5)</param>
    /// <returns>No(번호), Title(제목), UploadTime(작성일), Hits(조회수) 컬럼을 가진 DataTable</returns>
    public DataTable GetMyPosts(string userID, int top = 5)
    {
        // TOP {top}으로 최대 개수 제한, Author 조건으로 해당 유저 게시글만 조회
        // ORDER BY No DESC: 게시글 번호 내림차순 = 최신 게시글이 먼저
        string sql = string.Format(@"
            SELECT TOP {0} No, Title, UploadTime, Hits
            FROM Bbs
            WHERE Author = '{1}'
            ORDER BY No DESC", top, userID);
        // DataAdapterFill로 DataSet에 담은 뒤 첫 번째 테이블(Tables[0]) 반환
        DataSet ds = DbMan.DataAdapterFill(sql, "MyPosts");
        return ds.Tables[0];
    }

    /// <summary>
    /// 문자열을 MD5 해시값(대문자 HEX 문자열)으로 변환하는 내부 헬퍼 메서드.
    /// 비밀번호를 DB에 저장하거나 비교할 때 사용된다.
    /// MD5는 단방향 암호화(복호화 불가)이므로 원본 비밀번호를 알 수 없다.
    /// </summary>
    /// <param name="input">해시할 평문 문자열 (주로 비밀번호)</param>
    /// <returns>32자리 대문자 HEX 문자열 (예: "5F4DCC3B5AA765D61D8327DEB882CF99")</returns>
    private string GetMD5(string input)
    {
        // null 또는 빈 문자열 입력 시 빈 문자열 반환
        if (string.IsNullOrEmpty(input)) return string.Empty;
        // MD5 객체 생성 (using으로 자동 자원 해제)
        using (MD5 md5 = MD5.Create())
        {
            // ASCII 인코딩으로 바이트 배열 변환 후 MD5 해시 계산
            byte[] b = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            // 각 바이트를 2자리 대문자 16진수(HEX) 문자열로 변환하여 연결
            for (int i = 0; i < b.Length; i++) sb.Append(b[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
