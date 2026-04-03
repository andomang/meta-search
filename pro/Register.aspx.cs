using System;
using System.Web;
using System.Web.UI;

public partial class Register : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void btnRegister_Click(object sender, EventArgs e)
    {
        // 입력값 검증 (매우 간단한 예시)
        if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtEmail.Text))
        {
            Response.Write("<script>alert('모든 필드를 입력해주세요.');</script>");
            return;
        }

        if (txtPassword.Text != txtPasswordConfirm.Text)
        {
            Response.Write("<script>alert('비밀번호가 일치하지 않습니다.');</script>");
            return;
        }

        // 여기에 DB 저장 로직(INSERT)을 작성하시면 됩니다.

        // 가입 완료 후 메인으로 이동
        Response.Write("<script>alert('회원가입이 완료되었습니다!'); location.href='Default.aspx';</script>");
    }
}