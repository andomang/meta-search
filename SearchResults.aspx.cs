using System;
using System.Collections.Generic;

public partial class SearchResults : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string query = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(query))
            {
                txtSearch.Text = query;
            }
            BindResults();
        }
    }

    private void BindResults()
    {
        var results = new List<object>
        {
            new { Title="Wikipedia - 위키백과", Url="https://ko.wikipedia.org", Description="누구나 참여할 수 있는 자유 백과사전입니다." },
            new { Title="GitHub - 개발자 플랫폼", Url="https://github.com", Description="코드 공유 및 협업을 위한 세계 최대의 플랫폼입니다." },
            new { Title="Stack Overflow", Url="https://stackoverflow.com", Description="프로그래밍 질문과 답변을 위한 커뮤니티입니다." }
        };
        rptResults.DataSource = results;
        rptResults.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(txtSearch.Text));
    }
}