using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        // 입력된 검색어 가져오기
        string query = txtSearch.Text.Trim();

        // 검색어가 비어있지 않을 경우 결과 페이지로 이동
        if (!string.IsNullOrEmpty(query))
        {
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(query));
        }
    }
}