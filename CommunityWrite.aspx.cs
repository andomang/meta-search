using System;
using System.IO;
using System.Web.UI;

public partial class CommunityWrite : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserID"] == null)
        {
            Response.Write("<script>alert('로그인 후 이용 가능합니다.'); location.href='Default.aspx';</script>");
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        string title = txtTitle.Text.Trim();
        string contents = txtContent.Text.Trim();
        string fileName = "";
        int fileSize = 0;

        // 1. 파일 업로드 처리
        if (fileUpload.HasFile)
        {
            fileName = Path.GetFileName(fileUpload.PostedFile.FileName);
            fileSize = fileUpload.PostedFile.ContentLength;

            // 서버 내 bbs 폴더 경로 확인 (반드시 프로젝트에 bbs 폴더가 있어야 함)
            string savePath = Server.MapPath("~/bbs/") + fileName;
            fileUpload.SaveAs(savePath);
        }

        // 2. MemberDao의 저장 프로시저 메서드 호출
        MemberDao dao = new MemberDao();
        int result = dao.WriteBbs(title, contents, Session["UserID"].ToString(), fileName, fileSize);

        if (result > 0)
        {
            Response.Write("<script>alert('등록되었습니다.'); location.href='Community.aspx';</script>");
        }
        else
        {
            Response.Write("<script>alert('등록 중 오류가 발생했습니다.');</script>");
        }
    }
}