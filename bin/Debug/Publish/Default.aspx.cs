using System;
using System.Web;
using System.Web.UI;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // 페이지 로드 시 초기화가 필요한 경우 작성
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string query = txtSearch.Text.Trim();

        if (!string.IsNullOrEmpty(query))
        {
            // 검색 결과를 보여줄 페이지(SearchResults.aspx)로 이동
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(query));
        }
        else
        {
            // 검색어가 없을 때 브라우저 알림
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('검색어를 입력해 주세요.');", true);
        }
    }
}