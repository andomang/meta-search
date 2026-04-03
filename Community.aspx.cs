using System;
using System.Data;
using System.Web.UI;

public partial class Community : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) BindData();
    }

    private void BindData()
    {
        // 다이어그램 구조: Bbs 테이블과 members(또는 User) 조인
        string sql = @"
            SELECT b.No, b.Title, b.UploadTime, b.Hits, b.FileName, m.nickname as AuthorName 
            FROM Bbs b 
            INNER JOIN members m ON b.Author = m.userid 
            ORDER BY b.No DESC";

        try
        {
            DataSet ds = DbMan.DataAdapterFill(sql, "Bbs");
            rptPosts.DataSource = ds;
            rptPosts.DataBind();
        }
        catch (Exception ex)
        {
            Response.Write("<script>alert('데이터 로드 실패: " + ex.Message.Replace("'", "") + "');</script>");
        }
    }
}