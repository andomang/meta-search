using System;
using System.Data;
using System.Web.UI;

public partial class CommunityView : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string no = Request.QueryString["no"];
        if (string.IsNullOrEmpty(no)) Response.Redirect("Community.aspx");

        if (!IsPostBack)
        {
            UpdateHits(no);
            LoadPost(no);
        }
    }

    private void UpdateHits(string no)
    {
        // Hits 증가 쿼리
        DbMan.ExecuteNonQuery("UPDATE Bbs SET Hits = Hits + 1 WHERE No = " + no);
        DbMan.Close();
    }

    private void LoadPost(string no)
    {
        // 다이어그램 구조 반영: No, Title, Contents, Author, UploadTime, Hits, FileName, FileSize
        string sql = string.Format(@"
            SELECT b.*, m.nickname 
            FROM Bbs b 
            INNER JOIN members m ON b.Author = m.userid 
            WHERE b.No = {0}", no);

        DataSet ds = DbMan.DataAdapterFill(sql, "View");
        if (ds.Tables[0].Rows.Count > 0)
        {
            DataRow dr = ds.Tables[0].Rows[0];
            litTitle.Text = dr["Title"].ToString();
            litContents.Text = dr["Contents"].ToString(); // whitespace-pre-wrap으로 공백 유지
            litAuthor.Text = dr["nickname"].ToString();
            litAvatar.Text = dr["nickname"].ToString().Substring(0, 1);
            litDate.Text = Convert.ToDateTime(dr["UploadTime"]).ToString("yyyy.MM.dd HH:mm");
            litHits.Text = dr["Hits"].ToString();

            if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
            {
                phFile.Visible = true;
                litFileName.Text = dr["FileName"].ToString();
                litFileSize.Text = (Convert.ToInt32(dr["FileSize"]) / 1024).ToString();
                hlDownload.NavigateUrl = "~/bbs/" + dr["FileName"].ToString();
            }

            // 본인 확인 (수정/삭제 권한)
            if (Session["UserID"] != null && Session["UserID"].ToString() == dr["Author"].ToString().Trim())
            {
                phOwnerActions.Visible = true;
            }
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string no = Request.QueryString["no"];
        DbMan.ExecuteNonQuery("DELETE FROM Bbs WHERE No = " + no);
        DbMan.Close();
        Response.Write("<script>alert('삭제되었습니다.'); location.href='Community.aspx';</script>");
    }
}