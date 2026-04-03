using System;
using System.Collections.Generic;

public partial class Community : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) BindData();
    }

    private void BindData()
    {
        var posts = new List<object> {
            new { Author="김철수", Avatar="K", Time="2시간 전", Content="새로운 검색 기능이 정말 유용하네요!" },
            new { Author="이영희", Avatar="L", Time="5시간 전", Content="다크 모드 언제 추가되나요?" }
        };
        rptPosts.DataSource = posts;
        rptPosts.DataBind();
    }

    protected void btnPost_Click(object sender, EventArgs e)
    {
        txtNewPost.Text = ""; // 입력창 초기화
        BindData();
    }
}