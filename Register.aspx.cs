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
        // ... 필드 검증 로직 ...

        MemberDo newMember = new MemberDo();
        newMember.Userid = txtUserID.Text.Trim();
        newMember.Passwd = txtPassword.Text.Trim();
        newMember.Name = txtName.Text.Trim();
        newMember.Nickname = txtNickname.Text.Trim();
        newMember.Email = txtEmail.Text.Trim();

        MemberDao dao = new MemberDao();

        // 1. 중복 체크
        if (!dao.VerifyUserID(newMember.Userid))
        {
            Response.Write("<script>alert('이미 사용 중인 아이디입니다.');</script>");
            return;
        }

        // 2. 저장프로시저 호출
        int result = dao.RegisterUser(newMember);

        if (result > 0)
        {
            Response.Write("<script>alert('가입 성공!'); location.href='Default.aspx';</script>");
        }
        else
        {
            Response.Write("<script>alert('가입 실패');</script>");
        }
    }
}