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
            return;
        }

        litPageTitle.Text = Lang.Get("write.title");
        litTitleLbl.Text = Lang.Get("write.titleLbl");
        litContentLbl.Text = Lang.Get("write.content");
        litFileLbl.Text = Lang.Get("write.file");
        litCancelBtn.Text = Lang.Get("write.cancel");
        btnSave.Text = Lang.Get("write.submit");
        txtTitle.Attributes["placeholder"] = Lang.Get("write.titlePh");
        txtContent.Attributes["placeholder"] = Lang.Get("write.contentPh");
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        string title = txtTitle.Text.Trim();
        string contents = txtContent.Text.Trim();
        string fileName = "";
        int fileSize = 0;

        if (fileUpload.HasFile)
        {
            fileName = System.IO.Path.GetFileName(fileUpload.PostedFile.FileName);
            fileSize = fileUpload.PostedFile.ContentLength;
            fileUpload.SaveAs(Server.MapPath("~/bbs/") + fileName);
        }

        MemberDao dao = new MemberDao();
        int result = dao.WriteBbs(title, contents, Session["UserID"].ToString(), fileName, fileSize);

        if (result > 0)
            Response.Write("<script>alert('등록되었습니다.'); location.href='Community.aspx';</script>");
        else
            Response.Write("<script>alert('등록 중 오류가 발생했습니다.');</script>");
    }
}