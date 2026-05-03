using System;
using System.Web.UI;

/// <summary>
/// 메인(홈) 페이지 코드비하인드 (Default.aspx.cs)
/// URL: Default.aspx (사이트 진입점)
/// 주요 기능:
///   - 다국어(Lang) 리소스를 이용해 페이지 텍스트를 동적으로 바인딩한다.
///   - 커뮤니티·설정 섹션의 제목/설명 문구를 현재 언어에 맞게 표시한다.
///   - 검색 텍스트박스와 검색 버튼을 제공하며, 검색어 입력 시
///     SearchResults.aspx 로 쿼리스트링을 넘겨 이동한다.
/// </summary>
public partial class _Default : System.Web.UI.Page
{
    /// <summary>
    /// 페이지가 로드될 때마다 실행되는 이벤트 핸들러.
    /// PostBack 여부와 관계없이 매번 다국어 텍스트를 바인딩한다.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 홈 페이지 부제목 텍스트를 현재 언어에 맞게 설정
        litSubtitle.Text = Lang.Get("home.subtitle");

        // 커뮤니티 섹션의 제목과 부제목을 현재 언어에 맞게 설정
        litCommunityTitle.Text = Lang.Get("home.community");
        litCommunitySub.Text = Lang.Get("home.commSub");

        // 설정 섹션의 제목과 부제목을 현재 언어에 맞게 설정
        litSettingsTitle.Text = Lang.Get("home.settings");
        litSettingsSub.Text = Lang.Get("home.settSub");

        // 현재 언어가 영어이면 "Search", 그 외(한국어)이면 "검색"으로 버튼 텍스트 결정
        btnSearch.Text = Lang.Get("nav.community") == "Community" ? "Search" : "검색";

        // 검색 입력란의 placeholder(안내 문구)를 현재 언어에 맞게 설정
        txtSearch.Attributes["placeholder"] = Lang.Get("search.placeholder");
    }

    /// <summary>
    /// 검색 버튼(btnSearch)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 입력된 검색어를 URL 인코딩하여 SearchResults.aspx 로 이동한다.
    /// 검색어가 비어 있으면 placeholder 문구를 alert 으로 안내한다.
    /// </summary>
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        // 검색 텍스트박스에서 앞뒤 공백을 제거한 검색어를 가져옴
        string query = txtSearch.Text.Trim();

        if (!string.IsNullOrEmpty(query))
            // 검색어가 있으면 URL 인코딩 후 SearchResults.aspx 로 이동
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(query));
        else
            // 검색어가 비어 있으면 안내 메시지를 JavaScript alert 으로 표시
            ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                "alert('" + (Lang.Get("search.placeholder")) + "');", true);
    }
}
