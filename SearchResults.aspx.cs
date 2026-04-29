using System;
using System.Data;
using System.Web;

/// <summary>
/// 검색 결과 페이지 코드비하인드
/// 기능:
///  - 검색어를 쿼리스트링으로 받아 결과를 Repeater에 바인딩
///  - AJAX 방식(fetch)으로 검색 기록 조회/삭제 처리
///  - AJAX 방식으로 인기 검색어 조회 처리
///  - AJAX 방식으로 자동완성 키워드 조회 처리
///  - AJAX 방식으로 검색 결과 클릭 기록 저장 처리
///  - 로그인 유저의 검색어를 SearchHistory 테이블에 자동 저장
/// </summary>
public partial class SearchResults : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // AJAX 요청 처리: action 파라미터가 있으면 JSON 응답 후 종료
        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action); return; }

        // 다국어 텍스트 바인딩
        litResultsLabel.Text  = Lang.Get("search.results");
        litRelatedLabel.Text  = Lang.Get("search.related");
        litPopularLabel.Text  = Lang.Get("search.popular");
        litLoadingLabel.Text  = Lang.Get("search.loading");
        litNoResults.Text     = Lang.Get("search.noResults");
        litNoResultsSub.Text  = Lang.Get("search.noResultsSub");
        btnSearch.Text        = Lang.Get("search.placeholder").Contains("Search") ? "Search" : "검색";

        hdnRecentLabel.Value  = Lang.Get("search.recent");
        hdnNoHistLabel.Value  = Lang.Get("search.noHistory");
        hdnSearchPh.Value     = Lang.Get("search.placeholder");
        hdnTimesLabel.Value   = Lang.Get("search.times");
        hdnCurrentQuery.Value = Request.QueryString["q"] ?? "";
        txtSearch.Attributes["placeholder"] = Lang.Get("search.placeholder");

        if (!IsPostBack)
        {
            string q = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(q))
            {
                txtSearch.Text = q;

                // 로그인 유저만 검색 기록 저장 (크롤러 붙으면 카테고리 교체)
                if (Session["UserID"] != null)
                {
                    SearchDao dao = new SearchDao();
                    dao.AddSearchHistory(Session["UserID"].ToString(), q, "일반");
                }

                // 인기 검색어 사이드바 바인딩
                BindPopularSearches();

                // 검색 결과 바인딩 (현재 더미 데이터, 추후 크롤러로 교체)
                BindDummySearch(q);
            }
        }
    }

    /// <summary>
    /// AJAX 요청 분기 처리
    /// action 파라미터 값에 따라 JSON 응답 후 Response.End() 호출
    /// - getHistory    : 최근 검색 기록 목록 반환
    /// - deleteHistory : 특정 검색어 기록 삭제
    /// - getPopular    : 전체 인기 검색어 TOP 10 반환
    /// - getSuggest    : 자동완성용 검색어 제안 반환
    /// - saveClick     : 검색 결과 클릭 기록 저장
    /// </summary>
    private void HandleAjax(string action)
    {
        Response.ContentType = "application/json";
        string userID = Session["UserID"] != null ? Session["UserID"].ToString() : null;
        SearchDao dao = new SearchDao();

        // 최근 검색 기록 조회 (비로그인 시 빈 배열 반환)
        if (action == "getHistory")
        {
            if (userID == null) { Response.Write("[]"); Response.End(); return; }
            DataTable dt = dao.GetRecentSearches(userID);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                string q = dt.Rows[i]["Query"].ToString().Replace("\"", "\\\"");
                sb.AppendFormat("{{\"query\":\"{0}\"}}", q);
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        // 특정 검색어 기록 삭제 (로그인 필요)
        else if (action == "deleteHistory")
        {
            if (userID == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            string query = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(query))
            {
                dao.DeleteSearchHistory(userID, query);
                Response.Write("{\"result\":\"ok\"}");
            }
        }
        // 전체 인기 검색어 TOP 10 조회 (비로그인도 가능)
        else if (action == "getPopular")
        {
            DataTable dt = dao.GetTopSearches(10);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                string q     = dt.Rows[i]["Query"].ToString().Replace("\"", "\\\"");
                string count = dt.Rows[i]["SearchCount"].ToString();
                sb.AppendFormat("{{\"query\":\"{0}\",\"count\":{1}}}", q, count);
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        // 자동완성 제안어 조회 - 입력 중인 검색어로 시작하는 인기 기록 반환
        else if (action == "getSuggest")
        {
            string keyword = Request.QueryString["q"];
            if (string.IsNullOrEmpty(keyword)) { Response.Write("[]"); Response.End(); return; }
            string sql = string.Format(@"
                SELECT TOP 5 Query, COUNT(*) AS SearchCount
                FROM SearchHistory
                WHERE Query LIKE N'{0}%'
                GROUP BY Query
                ORDER BY SearchCount DESC",
                keyword.Replace("'", "''"));
            DataSet ds = DbMan.DataAdapterFill(sql, "Suggest");
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                string q = ds.Tables[0].Rows[i]["Query"].ToString().Replace("\"", "\\\"");
                sb.AppendFormat("\"{0}\"", q);
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        // 검색 결과 클릭 기록 저장 (로그인 필요)
        else if (action == "saveClick")
        {
            if (userID == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            string query    = Request.QueryString["q"]     ?? "";
            string url      = Request.QueryString["url"]   ?? "";
            string title    = Request.QueryString["title"] ?? "";
            string category = Request.QueryString["cat"]   ?? "일반";
            if (!string.IsNullOrEmpty(url))
            {
                dao.AddSearchClick(userID, query, category, url, title);
                Response.Write("{\"result\":\"ok\"}");
            }
            else { Response.Write("{\"result\":\"error\"}"); }
        }

        Response.End();
    }

    /// <summary>
    /// 검색 버튼 클릭 이벤트
    /// 검색어를 URL 인코딩 후 SearchResults.aspx?q=... 로 리다이렉트
    /// </summary>
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtSearch.Text))
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(txtSearch.Text));
    }

    /// <summary>
    /// 인기 검색어 사이드바 바인딩
    /// SearchHistory 전체 집계 기준 TOP 10을 rptPopular Repeater에 바인딩
    /// </summary>
    private void BindPopularSearches()
    {
        try
        {
            SearchDao dao = new SearchDao();
            DataTable dt = dao.GetTopSearches(10);
            rptPopular.DataSource = dt;
            rptPopular.DataBind();
        }
        catch { /* 인기 검색어 로드 실패 시 사이드바만 빈 상태로 표시 */ }
    }

    /// <summary>
    /// 더미 검색 결과 바인딩 (크롤러 구현 전 임시 사용)
    /// 추후 SearchClassifier → SearchCrawler → SearchRanker 파이프라인으로 교체 예정
    /// </summary>
    private void BindDummySearch(string q)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Title");
        dt.Columns.Add("Url");
        dt.Columns.Add("Description");
        dt.Columns.Add("Source");

        bool isEn = Lang.Get("search.results").Contains("Search");

        dt.Rows.Add(
            q + (isEn ? " - Result 1"     : "에 대한 검색 결과 1"),
            "https://google.com/search?q=" + q,
            isEn ? "This area will display results from external search engines."
                 : "이 영역은 나중에 외부 엔진에서 긁어온 상세 설명이 표시될 자리입니다.",
            "google");
        dt.Rows.Add(
            q + (isEn ? " - Related Info" : "와 관련된 유용한 정보"),
            "https://naver.com",
            isEn ? "Google/Naver logic will be combined for more accurate results."
                 : "더 정확한 결과를 위해 구글/네이버 로직을 결합할 예정입니다.",
            "naver");
        dt.Rows.Add(
            "ASP.NET Development Guide",
            "https://github.com/andomang",
            isEn ? "Custom search engine build in progress..."
                 : "사용자 정의 검색 엔진 구축 테스트 중...",
            "daum");

        rptResults.DataSource = dt;
        rptResults.DataBind();
    }
}
