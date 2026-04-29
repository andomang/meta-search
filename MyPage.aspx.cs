using System;
using System.Data;
using System.Web.UI;

/// <summary>
/// 마이페이지 코드비하인드 (새 페이지)
/// 기능:
///  - 내 검색 통계 (총 검색 횟수, 총 클릭 횟수, 최다 검색 키워드)
///  - 최근 검색어 TOP 5 목록
///  - 내가 쓴 게시글 최근 5개 목록
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class MyPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // 비로그인 접근 차단
        if (Session["UserID"] == null)
        {
            Response.Redirect("Default.aspx");
            return;
        }

        // 다국어 텍스트 바인딩
        litPageTitle.Text       = Lang.Get("my.title");
        litWelcome.Text         = Lang.Get("my.welcome");
        litRecentSearchLbl.Text = Lang.Get("my.recentSearch");
        litMyPostsLbl.Text      = Lang.Get("my.myPosts");
        litTotalSearchLbl.Text  = Lang.Get("my.totalSearch");
        litTotalClickLbl.Text   = Lang.Get("my.totalClick");
        litTopKeywordLbl.Text   = Lang.Get("my.topKeyword");
        litViewAllSearch.Text   = Lang.Get("my.viewAll");
        litViewAllPosts.Text    = Lang.Get("my.viewAll");

        if (!IsPostBack)
        {
            LoadStats();
            LoadRecentSearches();
            LoadMyPosts();
        }
    }

    /// <summary>
    /// 검색 통계 로드
    /// SearchDao를 통해 총 검색 횟수, 총 클릭 횟수, 최다 검색어 조회
    /// 각각 litTotalSearch, litTotalClick, litTopKeyword Literal에 바인딩
    /// </summary>
    private void LoadStats()
    {
        string userID = Session["UserID"].ToString();
        SearchDao dao = new SearchDao();

        litTotalSearch.Text = dao.GetTotalSearchCount(userID).ToString();
        litTotalClick.Text  = dao.GetTotalClickCount(userID).ToString();
        litTopKeyword.Text  = dao.GetTopKeyword(userID);
    }

    /// <summary>
    /// 최근 검색어 TOP 5 로드
    /// SearchDao.GetRecentSearches로 최신 5개 조회 후 rptRecentSearch Repeater에 바인딩
    /// 데이터 없으면 litNoSearch Literal 표시
    /// </summary>
    private void LoadRecentSearches()
    {
        string userID = Session["UserID"].ToString();
        SearchDao dao = new SearchDao();
        DataTable dt = dao.GetRecentSearches(userID, 5);

        if (dt.Rows.Count == 0)
        {
            litNoSearch.Text     = Lang.Get("my.noSearch");
            litNoSearch.Visible  = true;
            rptRecentSearch.Visible = false;
        }
        else
        {
            litNoSearch.Visible     = false;
            rptRecentSearch.DataSource = dt;
            rptRecentSearch.DataBind();
        }
    }

    /// <summary>
    /// 내 게시글 최근 5개 로드
    /// MemberDao.GetMyPosts로 최신 5개 조회 후 rptMyPosts Repeater에 바인딩
    /// 데이터 없으면 litNoPosts Literal 표시
    /// </summary>
    private void LoadMyPosts()
    {
        string userID = Session["UserID"].ToString();
        MemberDao dao = new MemberDao();
        DataTable dt = dao.GetMyPosts(userID, 5);

        if (dt.Rows.Count == 0)
        {
            litNoPosts.Text     = Lang.Get("my.noPosts");
            litNoPosts.Visible  = true;
            rptMyPosts.Visible  = false;
        }
        else
        {
            litNoPosts.Visible    = false;
            rptMyPosts.DataSource = dt;
            rptMyPosts.DataBind();
        }
    }
}
