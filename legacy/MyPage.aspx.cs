using System;
using System.Data;
using System.Web.UI;

/// <summary>
/// 마이페이지 코드비하인드
/// URL: MyPage.aspx
/// 주요 기능:
///  - 내 검색 통계 (총 검색 횟수, 총 클릭 횟수, 최다 검색 키워드)
///  - 최근 검색어 TOP 5 목록
///  - 내가 쓴 게시글 최근 5개 목록
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class MyPage : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - 비로그인 상태이면 메인 페이지로 리다이렉트
    /// - 다국어 UI 텍스트를 Literal 컨트롤에 바인딩
    /// - 최초 로드 시에만 통계, 최근 검색어, 내 게시글 목록을 DB에서 불러옴
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 비로그인 접근 차단: 세션에 UserID가 없으면 메인 페이지로 강제 이동
        if (Session["UserID"] == null)
        {
            Response.Redirect("Default.aspx");
            return;
        }

        // 다국어 텍스트 바인딩: 페이지 제목, 환영 메시지, 각 섹션 레이블 등
        litPageTitle.Text       = Lang.Get("my.title");
        litWelcome.Text         = Lang.Get("my.welcome");
        litRecentSearchLbl.Text = Lang.Get("my.recentSearch");
        litMyPostsLbl.Text      = Lang.Get("my.myPosts");
        litTotalSearchLbl.Text  = Lang.Get("my.totalSearch");
        litTotalClickLbl.Text   = Lang.Get("my.totalClick");
        litTopKeywordLbl.Text   = Lang.Get("my.topKeyword");
        litViewAllSearch.Text   = Lang.Get("my.viewAll");
        litViewAllPosts.Text    = Lang.Get("my.viewAll");

        // 최초 로드 시에만 DB 조회 수행 (PostBack 시 중복 조회 방지)
        if (!IsPostBack)
        {
            LoadStats();           // 검색 통계 로드
            LoadRecentSearches();  // 최근 검색어 목록 로드
            LoadMyPosts();         // 내 게시글 목록 로드
        }
    }

    /// <summary>
    /// 검색 통계 로드
    /// SearchDao를 통해 총 검색 횟수, 총 클릭 횟수, 최다 검색어 조회
    /// 각각 litTotalSearch, litTotalClick, litTopKeyword Literal에 바인딩
    /// </summary>
    private void LoadStats()
    {
        // 현재 로그인한 사용자의 ID를 세션에서 가져옴
        string userID = Session["UserID"].ToString();
        SearchDao dao = new SearchDao();

        // SearchHistory 테이블에서 해당 사용자의 검색 횟수, 클릭 횟수, 최다 검색어를 집계하여 표시
        litTotalSearch.Text = dao.GetTotalSearchCount(userID).ToString(); // 전체 검색 횟수
        litTotalClick.Text  = dao.GetTotalClickCount(userID).ToString();  // 전체 클릭 횟수
        litTopKeyword.Text  = dao.GetTopKeyword(userID);                  // 가장 많이 검색한 키워드
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

        // SearchHistory 테이블에서 해당 사용자의 최근 검색어 5개 조회
        DataTable dt = dao.GetRecentSearches(userID, 5);

        if (dt.Rows.Count == 0)
        {
            // 검색 기록이 없으면 "검색 기록이 없습니다" 메시지 표시, Repeater는 숨김
            litNoSearch.Text     = Lang.Get("my.noSearch");
            litNoSearch.Visible  = true;
            rptRecentSearch.Visible = false;
        }
        else
        {
            // 검색 기록이 있으면 빈 메시지 숨기고 Repeater에 데이터 바인딩
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

        // Bbs 테이블에서 해당 사용자가 작성한 게시글 최근 5개 조회
        DataTable dt = dao.GetMyPosts(userID, 5);

        if (dt.Rows.Count == 0)
        {
            // 작성한 게시글이 없으면 "게시글이 없습니다" 메시지 표시, Repeater는 숨김
            litNoPosts.Text     = Lang.Get("my.noPosts");
            litNoPosts.Visible  = true;
            rptMyPosts.Visible  = false;
        }
        else
        {
            // 게시글이 있으면 빈 메시지 숨기고 Repeater에 데이터 바인딩
            litNoPosts.Visible    = false;
            rptMyPosts.DataSource = dt;
            rptMyPosts.DataBind();
        }
    }
}
