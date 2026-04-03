using System;
using System.Collections.Generic;
using System.Data;

public partial class SearchResults : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string q = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(q))
            {
                txtSearch.Text = q;
                BindDummySearch(q); // 나중에 실제 크롤링/API 로직으로 교체하세요.
            }
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtSearch.Text))
        {
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(txtSearch.Text));
        }
    }

    private void BindDummySearch(string q)
    {
        // 나중에 구글/네이버 긁어온 데이터를 담을 구조
        DataTable dt = new DataTable();
        dt.Columns.Add("Title");
        dt.Columns.Add("Url");
        dt.Columns.Add("Description");

        // 검색어에 따른 샘플 데이터 (테스트용)
        dt.Rows.Add(q + "에 대한 검색 결과 1", "https://google.com/search?q=" + q, "이 영역은 나중에 외부 엔진에서 긁어온 상세 설명이 표시될 자리입니다.");
        dt.Rows.Add(q + "와 관련된 유용한 정보", "https://naver.com", "더 정확한 결과를 위해 구글/네이버 로직을 결합할 예정입니다.");
        dt.Rows.Add("ASP.NET 개발 가이드", "https://github.com/andomang", "사용자 정의 검색 엔진 구축 테스트 중...");

        rptResults.DataSource = dt;
        rptResults.DataBind();
    }
}