using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

/// <summary>
/// 회원가입 페이지 코드비하인드 (Register.aspx.cs)
/// URL: Register.aspx
/// 주요 기능:
///   - 아이디, 비밀번호, 이름, 닉네임, 이메일을 입력받아 신규 회원을 등록한다.
///   - 비밀번호는 MD5 해시로 암호화한 뒤 DB에 저장한다.
///   - 가입 성공 시 JavaScript alert 후 Default.aspx(메인 페이지)로 이동한다.
///   - 가입 실패(중복 아이디 등) 시 오류 메시지를 alert 으로 안내한다.
///   - 다국어(Lang) 리소스를 이용해 폼 레이블·버튼 문구를 동적으로 바인딩한다.
/// </summary>
public partial class Register : System.Web.UI.Page
{
    /// <summary>
    /// 페이지가 로드될 때마다 실행되는 이벤트 핸들러.
    /// PostBack 여부와 관계없이 매번 다국어 텍스트를 바인딩한다.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 회원가입 페이지 타이틀 문구를 현재 언어에 맞게 설정
        litPageTitle.Text = Lang.Get("reg.title");

        // 각 입력 필드의 레이블 문구를 현재 언어에 맞게 설정
        litIdLbl.Text = Lang.Get("reg.id");
        litPwLbl.Text = Lang.Get("reg.pw");
        litNameLbl.Text = Lang.Get("reg.name");
        litNickLbl.Text = Lang.Get("reg.nick");
        litEmailLbl.Text = Lang.Get("reg.email");

        // 취소 버튼과 가입 제출 버튼의 문구를 현재 언어에 맞게 설정
        litCancelBtn.Text = Lang.Get("reg.cancel");
        btnRegister.Text = Lang.Get("reg.submit");

        // 아이디·비밀번호 입력란의 placeholder(안내 문구)를 현재 언어에 맞게 설정
        txtUserID.Attributes["placeholder"] = Lang.Get("reg.idPh");
        txtPassword.Attributes["placeholder"] = Lang.Get("reg.pwPh");
    }

    /// <summary>
    /// 회원가입 버튼(btnRegister)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 입력값을 검증한 뒤 members 테이블에 신규 회원 레코드를 INSERT 한다.
    /// 성공 시 메인 페이지로 이동하고, 실패 시 오류 메시지를 alert 으로 표시한다.
    /// </summary>
    protected void btnRegister_Click(object sender, EventArgs e)
    {
        // 폼 입력값에서 앞뒤 공백을 제거하여 변수에 저장
        string userId = txtUserID.Text.Trim();
        string rawPassword = txtPassword.Text.Trim();
        string name = txtName.Text.Trim();
        string nickname = txtNickname.Text.Trim();
        string email = txtEmail.Text.Trim();

        // 필수 항목(아이디, 비밀번호)이 비어 있으면 처리를 중단
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(rawPassword)) return;

        try
        {
            // 입력된 비밀번호를 MD5 해시로 암호화 (평문 그대로 저장하지 않음)
            string md5Password = GetMd5Hash(rawPassword);

            // DB 연결을 열고 회원 정보를 members 테이블에 INSERT
            using (SqlConnection conn = DbMan.Open())
            {
                // members 테이블에 아이디·암호화된 비밀번호·이름·닉네임·이메일·다크모드 기본값·상태값을 저장
                // DarkMode 기본값: 0 (라이트 모드), status: '1' (정상 계정)
                string sql = "INSERT INTO members (UserID, passwd, Name, Nickname, Email, DarkMode, status) VALUES (@id, @pw, @name, @nick, @email, 0, '1')";
                SqlCommand cmd = new SqlCommand(sql, conn);

                // SQL 인젝션을 방지하기 위해 파라미터 바인딩 사용
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@pw", md5Password);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@nick", nickname);
                cmd.Parameters.AddWithValue("@email", email);

                // INSERT 쿼리 실행
                cmd.ExecuteNonQuery();

                // DB 연결 닫기
                DbMan.Close();

                // 가입 성공 메시지를 alert 으로 표시한 뒤 메인 페이지로 이동
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ok",
                    "alert('회원가입이 완료되었습니다!'); location.href='Default.aspx';", true);
            }
        }
        catch (Exception ex)
        {
            // 예외 발생 시 DB 연결을 닫고 오류 메시지를 alert 으로 표시
            // (예: 아이디 중복으로 인한 UNIQUE 제약 위반 등)
            DbMan.Close();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "err",
                "alert('가입 실패: " + ex.Message.Replace("'", "") + "');", true);
        }
    }

    /// <summary>
    /// 입력된 문자열을 MD5 해시값(16진수 대문자 문자열)으로 변환하는 유틸리티 메서드.
    /// 비밀번호를 DB에 저장하기 전에 암호화하는 데 사용된다.
    /// </summary>
    /// <param name="input">해시 처리할 평문 문자열 (예: 비밀번호)</param>
    /// <returns>MD5 해시값 (32자리 16진수 대문자 문자열)</returns>
    private string GetMd5Hash(string input)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            // 입력 문자열을 ASCII 바이트 배열로 변환한 뒤 MD5 해시 계산
            byte[] data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(input));

            // 바이트 배열을 16진수 대문자 문자열로 변환하여 반환
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("X2"));
            return sBuilder.ToString();
        }
    }
}
