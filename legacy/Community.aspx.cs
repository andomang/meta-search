using System;
using System.Data;
using System.Web.UI;
using System.Data.SqlClient;

/// <summary>
/// 커뮤니티 게시판 목록 페이지 코드비하인드
/// URL: Community.aspx (페이지네이션: ?page=N, 검색: ?q=키워드)
/// 주요 기능:
///  - 게시글 목록을 10개씩 페이지 단위로 표시 (ROW_NUMBER 기반 서버 페이징)
///  - 제목/내용 키워드 검색 지원
///  - 각 게시글에 좋아요 수, 댓글 수, 첨부파일 여부 표시
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class Community : System.Web.UI.Page
{
    // 한 페이지에 표시할 게시글 수 (고정값)
    private const int PAGE_SIZE = 10;

    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - 다국어 UI 텍스트를 바인딩한다
    /// - 최초 로드(IsPostBack=false)일 때만 게시글 목록을 불러온다
    /// - PostBack 시에는 검색어 덮어쓰기를 방지한다
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 다국어 텍스트 바인딩: 페이지 제목, 버튼, 컬럼 헤더 등을 언어 설정에 맞게 표시
        litPageTitle.Text = Lang.Get("comm.title");
        litWriteBtn.Text  = Lang.Get("comm.write");
        litColNo.Text     = Lang.Get("comm.no");
        litColTitle.Text  = Lang.Get("comm.titleCol");
        litColAuthor.Text = Lang.Get("comm.author");
        litColDate.Text   = Lang.Get("comm.date");
        btnSearch.Text    = Lang.Get("comm.searchBtn");
        litResetBtn.Text  = Lang.Get("comm.searchReset");
        txtSearch.Attributes["placeholder"] = Lang.Get("comm.searchPh");

        // 검색어 유지 (PostBack 시엔 덮어쓰지 않음 - 재검색 버그 방지)
        // URL의 q 파라미터 값을 검색 입력창에 미리 채워 사용자 편의 제공
        if (!IsPostBack)
        {
            string q = Request.QueryString["q"];
            if (!string.IsNullOrEmpty(q)) txtSearch.Text = q;
        }

        // 최초 페이지 로드 시에만 DB에서 게시글 목록을 조회한다
        // PostBack(버튼 클릭 등)에서는 중복 조회를 방지
        if (!IsPostBack) BindData();
    }

    /// <summary>
    /// 게시글 목록 조회 및 Repeater 바인딩
    /// - URL 파라미터 page(현재 페이지), q(검색어)를 읽어 SQL을 동적으로 구성
    /// - ROW_NUMBER()를 이용한 서버 사이드 페이징으로 대용량 데이터도 효율적으로 처리
    /// - 좋아요(BbsLike), 댓글(BbsComment) 수를 서브쿼리로 집계
    /// - 이전/다음 버튼의 활성화 여부와 페이지 정보를 설정
    /// </summary>
    private void BindData()
    {
        // URL에서 현재 페이지 번호 읽기 (없으면 1페이지, 음수면 1로 보정)
        int page = 1;
        if (!string.IsNullOrEmpty(Request.QueryString["page"]))
            int.TryParse(Request.QueryString["page"], out page);
        if (page < 1) page = 1;

        // 검색어 처리: 앞뒤 공백 제거, SQL 인젝션 방지를 위해 작은따옴표 이스케이프
        string q = (Request.QueryString["q"] ?? "").Trim().Replace("'", "''");

        // 검색어가 있으면 제목 또는 내용에 키워드가 포함된 게시글만 조회하는 WHERE 절 생성
        string where = string.IsNullOrEmpty(q)
            ? ""
            : string.Format("WHERE b.Title LIKE N'%{0}%' OR b.Contents LIKE N'%{0}%'", q);

        // 현재 페이지의 시작 오프셋 계산 (예: 2페이지 → offset=10)
        int offset = (page - 1) * PAGE_SIZE;
        int totalCount = 0;

        try
        {
            // Bbs 테이블에서 검색 조건에 맞는 전체 게시글 수를 먼저 조회 (페이지 수 계산용)
            string countSql = string.Format("SELECT COUNT(*) FROM Bbs b {0}", where);
            SqlDataReader r = DbMan.ExecuteReader(countSql);
            if (r.Read()) totalCount = Convert.ToInt32(r[0]);
            r.Close(); DbMan.Close();
        }
        catch { DbMan.Close(); }

        // 전체 페이지 수 계산 (최소 1페이지 보장)
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / PAGE_SIZE));

        // 현재 페이지 범위의 게시글을 ROW_NUMBER 기반 서버 페이징으로 조회
        // - Bbs 테이블과 members 테이블을 JOIN하여 작성자 닉네임 포함
        // - BbsLike, BbsComment 서브쿼리로 좋아요 수와 댓글 수 집계
        // - No 기준 내림차순 정렬 (최신 게시글이 위에 표시)
        string sql = string.Format(@"
            SELECT No, Title, UploadTime, Hits, FileName, AuthorName, LikeCount, CommentCount FROM (
                SELECT b.No, b.Title, b.UploadTime, b.Hits, b.FileName,
                       m.Nickname AS AuthorName,
                       (SELECT COUNT(*) FROM BbsLike    l WHERE l.BbsNo = b.No) AS LikeCount,
                       (SELECT COUNT(*) FROM BbsComment c WHERE c.BbsNo = b.No) AS CommentCount,
                       ROW_NUMBER() OVER (ORDER BY b.No DESC) AS RowNum
                FROM Bbs b
                INNER JOIN members m ON b.Author = m.userid
                {0}
            ) AS Paged
            WHERE RowNum BETWEEN {1} AND {2}",
            where, offset + 1, offset + PAGE_SIZE);

        try
        {
            // 조회 결과를 DataSet에 담아 rptPosts Repeater에 바인딩
            DataSet ds = DbMan.DataAdapterFill(sql, "Bbs");
            rptPosts.DataSource = ds;
            rptPosts.DataBind();
        }
        catch (Exception ex) { DbMan.Close(); Response.Write("<script>console.error('" + ex.Message.Replace("'","") + "');</script>"); }

        // URL에 검색어 파라미터 추가 (페이지 이동 시에도 검색어 유지)
        string qParam = string.IsNullOrEmpty(q) ? "" : "&q=" + Server.UrlEncode(q);

        // 이전 페이지 버튼: 1페이지이면 비활성화, CommandArgument에 "페이지번호|검색어" 저장
        btnPrev.Enabled         = page > 1;
        btnPrev.CommandArgument = (page - 1).ToString() + "|" + q;
        btnPrev.Text            = Lang.Get("comm.prev");

        // 다음 페이지 버튼: 마지막 페이지이면 비활성화
        btnNext.Enabled         = page < totalPages;
        btnNext.CommandArgument = (page + 1).ToString() + "|" + q;
        btnNext.Text            = Lang.Get("comm.next");

        // "현재 페이지 / 전체 페이지" 형식으로 페이지 정보 표시
        litPageInfo.Text        = string.Format("{0} / {1}", page, totalPages);
    }

    /// <summary>
    /// 검색 버튼 클릭 이벤트 핸들러
    /// - 검색 입력창의 텍스트를 URL 파라미터로 인코딩하여 같은 페이지로 리다이렉트
    /// - 검색어가 없으면 전체 목록(Community.aspx)으로 이동
    /// </summary>
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string q = txtSearch.Text.Trim();
        // 검색어가 있으면 q 파라미터를 붙여 리다이렉트, 없으면 목록 첫 페이지로 이동
        string url = string.IsNullOrEmpty(q) ? "Community.aspx" : "Community.aspx?q=" + Server.UrlEncode(q);
        Response.Redirect(url);
    }

    /// <summary>
    /// 이전/다음 페이지 버튼 클릭 이벤트 핸들러
    /// - CommandArgument에서 "페이지번호|검색어" 형식으로 값을 분리하여 URL 구성
    /// - 검색 중 페이지 이동 시에도 검색어가 유지되도록 q 파라미터 포함
    /// </summary>
    protected void btnPage_Click(object sender, EventArgs e)
    {
        // 버튼의 CommandArgument에서 페이지 번호와 검색어를 분리
        var btn = (System.Web.UI.WebControls.Button)sender;
        string[] parts = btn.CommandArgument.Split('|');
        string page = parts[0];
        string q    = parts.Length > 1 ? parts[1] : "";

        // 검색어가 있으면 q 파라미터를 포함하여 해당 페이지로 이동
        string url  = string.IsNullOrEmpty(q)
            ? "Community.aspx?page=" + page
            : "Community.aspx?page=" + page + "&q=" + Server.UrlEncode(q);
        Response.Redirect(url);
    }
}
