using System;
using System.Data;
using System.Web;

/// <summary>
/// 검색 결과 페이지 코드비하인드
/// URL: SearchResults.aspx?q=검색어
/// 주요 기능:
///  - 검색어를 쿼리스트링으로 받아 결과를 Repeater에 바인딩
///  - 로그인 사용자의 검색어를 SearchHistory 테이블에 자동 저장
///  - ClaudeApi.Classify()로 검색어를 카테고리 분류하여 함께 저장
///  - AJAX 방식(fetch)으로 검색 기록 조회/삭제 처리
///  - AJAX 방식으로 인기 검색어 조회 처리
///  - AJAX 방식으로 자동완성 키워드 조회 처리
///  - AJAX 방식으로 검색 결과 클릭 기록 저장 처리
/// </summary>
public partial class SearchResults : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - action 파라미터가 있으면 AJAX 요청으로 처리 후 종료 (일반 렌더링 생략)
    /// - 다국어 UI 텍스트를 바인딩하고 숨김 필드에 JavaScript 사용 값 저장
    /// - 검색어(q)가 있으면 검색 기록 저장, 인기 검색어 사이드바, 검색 결과 바인딩 수행
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // AJAX 요청 처리: action 파라미터가 있으면 JSON 응답 후 종료
        // (JavaScript fetch가 action 파라미터를 붙여 호출하는 경우)
        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action); return; }

        // 다국어 텍스트 바인딩: 검색 결과 레이블, 관련 검색어 레이블, 인기 검색어 레이블 등
        litResultsLabel.Text  = Lang.Get("search.results");
        litRelatedLabel.Text  = Lang.Get("search.related");
        litPopularLabel.Text  = Lang.Get("search.popular");
        litLoadingLabel.Text  = Lang.Get("search.loading");
        litNoResults.Text     = Lang.Get("search.noResults");
        litNoResultsSub.Text  = Lang.Get("search.noResultsSub");

        // 현재 언어가 영어이면 "Search" 버튼, 한국어이면 "검색" 버튼
        btnSearch.Text        = Lang.Get("search.placeholder").Contains("Search") ? "Search" : "검색";

        // 숨김 필드(hidden field)에 JavaScript에서 사용할 다국어 텍스트와 현재 검색어 저장
        hdnRecentLabel.Value  = Lang.Get("search.recent");    // 최근 검색어 레이블
        hdnNoHistLabel.Value  = Lang.Get("search.noHistory"); // 검색 기록 없음 메시지
        hdnSearchPh.Value     = Lang.Get("search.placeholder"); // 검색창 플레이스홀더
        hdnTimesLabel.Value   = Lang.Get("search.times");     // "회" 단위 텍스트
        hdnCurrentQuery.Value = Request.QueryString["q"] ?? ""; // JavaScript에서 현재 검색어 참조용
        txtSearch.Attributes["placeholder"] = Lang.Get("search.placeholder");

        // 최초 로드 시에만 DB 조회 수행 (PostBack 시 중복 실행 방지)
        if (!IsPostBack)
        {
            string q = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(q))
            {
                // 검색창에 현재 검색어를 미리 채워 사용자 편의 제공
                txtSearch.Text = q;

                // 로그인 유저만 검색 기록 저장
                // ClaudeApi.Classify()로 검색어의 카테고리를 AI로 분류하여 함께 저장
                // (예: "파이썬 기초" → "IT/개발", "맛집 추천" → "음식" 등)
                if (Session["UserID"] != null)
                {
                    string category = ClaudeApi.Classify(q); // Claude AI API를 호출하여 검색어 카테고리 분류
                    SearchDao dao = new SearchDao();
                    dao.AddSearchHistory(Session["UserID"].ToString(), q, category); // SearchHistory 테이블에 INSERT
                }

                // 인기 검색어 사이드바 바인딩: SearchHistory 전체 집계 기준 TOP 10
                BindPopularSearches();

                // 검색 결과 바인딩 (현재 더미 데이터, 추후 크롤러로 교체)
                BindDummySearch(q);
            }
        }
    }

    /// <summary>
    /// AJAX 요청 분기 처리 메서드
    /// JavaScript fetch API에서 action 파라미터를 붙여 호출할 때 JSON 응답을 반환
    /// 모든 분기 처리 후 Response.End()로 응답을 완전히 종료
    ///
    /// - getHistory    : 로그인 사용자의 최근 검색 기록 목록 반환 (비로그인 시 빈 배열)
    /// - deleteHistory : 특정 검색어 기록 삭제 (로그인 필요)
    /// - getPopular    : 전체 인기 검색어 TOP 10 반환 (비로그인도 가능)
    /// - getSuggest    : 자동완성용 검색어 제안 반환 (입력어로 시작하는 인기 기록)
    /// - saveClick     : 검색 결과 클릭 기록 저장 (로그인 필요)
    /// </summary>
    private void HandleAjax(string action)
    {
        // 응답 형식을 JSON으로 설정 (브라우저 JavaScript에서 JSON.parse 처리)
        Response.ContentType = "application/json";

        // 세션에서 로그인한 사용자 ID를 가져옴 (비로그인이면 null)
        string userID = Session["UserID"] != null ? Session["UserID"].ToString() : null;
        SearchDao dao = new SearchDao();

        // --- action == "getHistory": 최근 검색 기록 조회 ---
        // 검색창 포커스 시 드롭다운에 최근 검색어 목록을 표시하기 위해 JavaScript가 호출
        // 비로그인 시 빈 배열 반환
        if (action == "getHistory")
        {
            if (userID == null) { Response.Write("[]"); Response.End(); return; }

            // SearchHistory 테이블에서 해당 사용자의 최근 검색어 목록 조회
            DataTable dt = dao.GetRecentSearches(userID);

            // JSON 배열을 직접 조립: JsonConvert 등 외부 라이브러리 없이 StringBuilder 사용
            // 형식: [{"query":"검색어1"},{"query":"검색어2"}, ...]
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            // 검색 기록 행을 순회하며 각 검색어를 JSON 객체로 변환
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(","); // 첫 번째 항목 제외하고 쉼표로 구분
                string q = dt.Rows[i]["Query"].ToString().Replace("\"", "\\\""); // JSON 문자열 내 따옴표 이스케이프
                sb.AppendFormat("{{\"query\":\"{0}\"}}", q);
            }
            sb.Append("]");
            Response.Write(sb.ToString()); // 완성된 JSON 배열을 응답으로 전송
        }
        // --- action == "deleteHistory": 특정 검색어 기록 삭제 ---
        // 최근 검색어 드롭다운에서 X 버튼 클릭 시 JavaScript가 호출
        else if (action == "deleteHistory")
        {
            // 비로그인 사용자는 삭제 불가
            if (userID == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }

            string query = Request.QueryString["q"]; // 삭제할 검색어
            if (!string.IsNullOrEmpty(query))
            {
                // SearchHistory 테이블에서 해당 사용자의 특정 검색어 기록 DELETE
                dao.DeleteSearchHistory(userID, query);
                Response.Write("{\"result\":\"ok\"}"); // 삭제 성공 응답
            }
        }
        // --- action == "getPopular": 전체 인기 검색어 TOP 10 조회 ---
        // 인기 검색어 사이드바 또는 드롭다운을 동적으로 업데이트할 때 호출
        // 비로그인 사용자도 확인 가능
        else if (action == "getPopular")
        {
            // SearchHistory 테이블 전체를 집계하여 검색 횟수 기준 TOP 10 조회
            DataTable dt = dao.GetTopSearches(10);

            // JSON 배열을 직접 조립: 각 인기 검색어와 검색 횟수를 포함
            // 형식: [{"query":"검색어","count":횟수}, ...]
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            // 인기 검색어 행을 순회하며 각 항목을 JSON 객체로 변환
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(","); // 쉼표로 항목 구분
                string q     = dt.Rows[i]["Query"].ToString().Replace("\"", "\\\""); // 따옴표 이스케이프
                string count = dt.Rows[i]["SearchCount"].ToString();                 // 검색 횟수
                sb.AppendFormat("{{\"query\":\"{0}\",\"count\":{1}}}", q, count);
            }
            sb.Append("]");
            Response.Write(sb.ToString()); // 완성된 JSON 배열을 응답으로 전송
        }
        // --- action == "getSuggest": 자동완성 제안어 조회 ---
        // 검색창에 글자를 입력할 때마다 JavaScript가 호출하여 드롭다운 자동완성 목록을 구성
        // 입력한 글자로 시작하는 인기 검색어를 최대 5개 반환
        else if (action == "getSuggest")
        {
            string keyword = Request.QueryString["q"]; // 현재 입력 중인 검색어
            if (string.IsNullOrEmpty(keyword)) { Response.Write("[]"); Response.End(); return; }

            // SearchHistory 테이블에서 입력어로 시작하는 검색어를 집계하여 인기순 TOP 5 조회
            // LIKE N'키워드%': 입력한 글자로 시작하는 검색어만 필터링
            string sql = string.Format(@"
                SELECT TOP 5 Query, COUNT(*) AS SearchCount
                FROM SearchHistory
                WHERE Query LIKE N'{0}%'
                GROUP BY Query
                ORDER BY SearchCount DESC",
                keyword.Replace("'", "''")); // SQL 인젝션 방지를 위해 작은따옴표 이스케이프
            DataSet ds = DbMan.DataAdapterFill(sql, "Suggest");

            // JSON 문자열 배열을 직접 조립: ["제안어1","제안어2", ...]
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            // 자동완성 제안어 행을 순회하며 각 검색어를 JSON 문자열로 변환
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (i > 0) sb.Append(","); // 쉼표로 항목 구분
                string q = ds.Tables[0].Rows[i]["Query"].ToString().Replace("\"", "\\\""); // 따옴표 이스케이프
                sb.AppendFormat("\"{0}\"", q); // 각 제안어를 JSON 문자열로 감쌈
            }
            sb.Append("]");
            Response.Write(sb.ToString()); // 완성된 JSON 배열을 응답으로 전송
        }
        // --- action == "saveClick": 검색 결과 클릭 기록 저장 ---
        // 사용자가 검색 결과 항목을 클릭할 때 JavaScript가 호출하여 클릭 이력을 DB에 저장
        // 나중에 클릭 기반 검색 품질 향상 및 사용자 행동 분석에 활용
        else if (action == "saveClick")
        {
            // 비로그인 사용자의 클릭은 저장하지 않음
            if (userID == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }

            // URL 파라미터로 클릭한 결과 항목의 정보를 받아옴
            string query    = Request.QueryString["q"]     ?? "";       // 검색에 사용한 키워드
            string url      = Request.QueryString["url"]   ?? "";       // 클릭한 결과 URL
            string title    = Request.QueryString["title"] ?? "";       // 클릭한 결과 제목
            string category = Request.QueryString["cat"]   ?? "일반";  // 결과 카테고리 (없으면 '일반')

            if (!string.IsNullOrEmpty(url))
            {
                // SearchHistory 또는 별도 클릭 테이블에 클릭 기록 INSERT
                dao.AddSearchClick(userID, query, category, url, title);
                Response.Write("{\"result\":\"ok\"}"); // 저장 성공 응답
            }
            else
            {
                // URL이 없는 경우 저장 불가 → 오류 응답
                Response.Write("{\"result\":\"error\"}");
            }
        }

        // 모든 AJAX 처리 완료 후 응답 전송을 완전히 종료하여 뒤의 HTML 렌더링 방지
        Response.End();
    }

    /// <summary>
    /// 검색 버튼 클릭 이벤트 핸들러
    /// 검색어를 URL 인코딩 후 SearchResults.aspx?q=... 로 리다이렉트
    /// 검색어가 없으면 이동하지 않음
    /// </summary>
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtSearch.Text))
            // 검색어를 URL에 안전하게 포함하기 위해 UrlEncode 처리 후 리다이렉트
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(txtSearch.Text));
    }

    /// <summary>
    /// 인기 검색어 사이드바 바인딩
    /// SearchHistory 전체 집계 기준 TOP 10을 rptPopular Repeater에 바인딩
    /// 실패하더라도 전체 페이지에 영향을 주지 않도록 예외를 조용히 처리
    /// </summary>
    private void BindPopularSearches()
    {
        try
        {
            SearchDao dao = new SearchDao();
            // SearchHistory 테이블에서 전체 사용자 기준 검색 횟수 TOP 10 조회
            DataTable dt = dao.GetTopSearches(10);
            rptPopular.DataSource = dt;
            rptPopular.DataBind(); // rptPopular Repeater에 인기 검색어 목록 바인딩
        }
        catch { /* 인기 검색어 로드 실패 시 사이드바만 빈 상태로 표시 */ }
    }

    /// <summary>
    /// 더미 검색 결과 바인딩 (크롤러 구현 전 임시 사용)
    /// 실제 외부 검색 엔진 연동이 완성되기 전까지 테스트용 가상 데이터를 표시
    /// 추후 SearchClassifier → SearchCrawler → SearchRanker 파이프라인으로 교체 예정
    /// </summary>
    /// <param name="q">사용자가 입력한 검색어</param>
    private void BindDummySearch(string q)
    {
        // 검색 결과 구조를 흉내 내는 임시 DataTable 생성 (컬럼: 제목, URL, 설명, 출처)
        DataTable dt = new DataTable();
        dt.Columns.Add("Title");
        dt.Columns.Add("Url");
        dt.Columns.Add("Description");
        dt.Columns.Add("Source");

        // 현재 언어 설정 확인 (영어 여부에 따라 더미 데이터 언어 결정)
        bool isEn = Lang.Get("search.results").Contains("Search");

        // 더미 결과 1: 구글 검색 링크를 가리키는 임시 결과
        dt.Rows.Add(
            q + (isEn ? " - Result 1"     : "에 대한 검색 결과 1"),
            "https://google.com/search?q=" + q,
            isEn ? "This area will display results from external search engines."
                 : "이 영역은 나중에 외부 엔진에서 긁어온 상세 설명이 표시될 자리입니다.",
            "google");

        // 더미 결과 2: 네이버를 가리키는 임시 결과
        dt.Rows.Add(
            q + (isEn ? " - Related Info" : "와 관련된 유용한 정보"),
            "https://naver.com",
            isEn ? "Google/Naver logic will be combined for more accurate results."
                 : "더 정확한 결과를 위해 구글/네이버 로직을 결합할 예정입니다.",
            "naver");

        // 더미 결과 3: GitHub 링크를 가리키는 임시 결과 (개발자용 테스트 항목)
        dt.Rows.Add(
            "ASP.NET Development Guide",
            "https://github.com/andomang",
            isEn ? "Custom search engine build in progress..."
                 : "사용자 정의 검색 엔진 구축 테스트 중...",
            "daum");

        // 완성된 더미 DataTable을 rptResults Repeater에 바인딩하여 화면에 표시
        rptResults.DataSource = dt;
        rptResults.DataBind();
    }
}
