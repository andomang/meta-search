using System;
using System.Data;
using System.Web.UI;
using System.Data.SqlClient;

public partial class Community : System.Web.UI.Page
{
    private const int PAGE_SIZE = 10;

    protected void Page_Load(object sender, EventArgs e)
    {
        litPageTitle.Text = Lang.Get("comm.title");
        litWriteBtn.Text  = Lang.Get("comm.write");
        litColNo.Text     = Lang.Get("comm.no");
        litColTitle.Text  = Lang.Get("comm.titleCol");
        litColAuthor.Text = Lang.Get("comm.author");
        litColDate.Text   = Lang.Get("comm.date");
        btnSearch.Text    = Lang.Get("comm.searchBtn");
        litResetBtn.Text  = Lang.Get("comm.searchReset");
        txtSearch.Attributes["placeholder"] = Lang.Get("comm.searchPh");

        // 검색어 유지 (PostBack 시엔 덮어쓰지 않음 - 재검색 버그 방지)
        if (!IsPostBack)
        {
            string q = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(q)) txtSearch.Text = q;
        }

        if (!IsPostBack) BindData();
    }

    private void BindData()
    {
        int page = 1;
        if (!string.IsNullOrEmpty(Request.QueryString["page"]))
            int.TryParse(Request.QueryString["page"], out page);
        if (page < 1) page = 1;

        string q = (Request.QueryString["q"] ?? "").Trim().Replace("'", "''");
        string where = string.IsNullOrEmpty(q)
            ? ""
            : string.Format("WHERE b.Title LIKE N'%{0}%' OR b.Contents LIKE N'%{0}%'", q);

        int offset = (page - 1) * PAGE_SIZE;
        int totalCount = 0;

        try
        {
            string countSql = string.Format("SELECT COUNT(*) FROM Bbs b {0}", where);
            SqlDataReader r = DbMan.ExecuteReader(countSql);
            if (r.Read()) totalCount = Convert.ToInt32(r[0]);
            r.Close(); DbMan.Close();
        }
        catch { DbMan.Close(); }

        int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / PAGE_SIZE));

        string sql = string.Format(@"
            SELECT No, Title, UploadTime, Hits, FileName, AuthorName, LikeCount, CommentCount FROM (
                SELECT b.No, b.Title, b.UploadTime, b.Hits, b.FileName,
                       m.Nickname AS AuthorName,
                       (SELECT COUNT(*) FROM BbsLike    l WHERE l.BbsNo = b.No) AS LikeCount,
                       (SELECT COUNT(*) FROM BbsComment c WHERE c.BbsNo = b.No) AS CommentCount,
                       ROW_NUMBER() OVER (ORDER BY b.No DESC) AS RowNum
                FROM Bbs b
                INNER JOIN members m ON b.Author = m.userid
                {0}
            ) AS Paged
            WHERE RowNum BETWEEN {1} AND {2}",
            where, offset + 1, offset + PAGE_SIZE);

        try
        {
            DataSet ds = DbMan.DataAdapterFill(sql, "Bbs");
            rptPosts.DataSource = ds;
            rptPosts.DataBind();
        }
        catch (Exception ex) { DbMan.Close(); Response.Write("<script>console.error('" + ex.Message.Replace("'","") + "');</script>"); }

        string qParam = string.IsNullOrEmpty(q) ? "" : "&q=" + Server.UrlEncode(q);
        btnPrev.Enabled         = page > 1;
        btnPrev.CommandArgument = (page - 1).ToString() + "|" + q;
        btnPrev.Text            = Lang.Get("comm.prev");
        btnNext.Enabled         = page < totalPages;
        btnNext.CommandArgument = (page + 1).ToString() + "|" + q;
        btnNext.Text            = Lang.Get("comm.next");
        litPageInfo.Text        = string.Format("{0} / {1}", page, totalPages);
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string q = txtSearch.Text.Trim();
        string url = string.IsNullOrEmpty(q) ? "Community.aspx" : "Community.aspx?q=" + Server.UrlEncode(q);
        Response.Redirect(url);
    }

    protected void btnPage_Click(object sender, EventArgs e)
    {
        var btn = (System.Web.UI.WebControls.Button)sender;
        string[] parts = btn.CommandArgument.Split('|');
        string page = parts[0];
        string q    = parts.Length > 1 ? parts[1] : "";
        string url  = string.IsNullOrEmpty(q)
            ? "Community.aspx?page=" + page
            : "Community.aspx?page=" + page + "&q=" + Server.UrlEncode(q);
        Response.Redirect(url);
    }
}
