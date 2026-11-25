using System;
using System.Data;
using System.Web.UI;

/// <summary>
/// 게시글 수정 페이지 코드비하인드
/// URL: CommunityEdit.aspx?no=게시글번호
/// 주요 기능:
///  - 게시글 번호(no)로 기존 내용 불러오기
///  - 본인 글인지 세션으로 검증 (타인 접근 차단)
///  - 수정 내용 저장 (MemberDao.UpdateBbs 호출)
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class CommunityEdit : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - 비로그인 상태이면 경고 후 메인 페이지로 이동
    /// - URL 파라미터 no가 없으면 목록으로 리다이렉트
    /// - 다국어 UI 텍스트를 바인딩
    /// - 최초 로드 시에만 기존 게시글 내용을 폼에 채워넣음
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 비로그인 접근 차단: 세션에 UserID가 없으면 경고 후 메인 페이지로 강제 이동
        if (Session["UserID"] == null)
        {
            bool isEn = Lang.Get("nav.login") == "Login";
            Response.Write(string.Format(
                "<script>alert('{0}'); location.href='Default.aspx';</script>",
                isEn ? "Please log in first." : "로그인 후 이용 가능합니다."));
            return;
        }

        // 게시글 번호 없으면 목록으로 리다이렉트: no 파라미터가 없으면 어떤 글인지 알 수 없으므로 이동
        string no = Request.QueryString["no"];
        if (string.IsNullOrEmpty(no)) Response.Redirect("Community.aspx");

        // 다국어 텍스트 바인딩: 페이지 제목, 입력 레이블, 취소/저장 버튼 텍스트
        litPageTitle.Text  = Lang.Get("edit.title");
        litTitleLbl.Text   = Lang.Get("edit.titleLbl");
        litContentLbl.Text = Lang.Get("edit.content");
        litCancelBtn.Text  = Lang.Get("edit.cancel");
        btnSave.Text       = Lang.Get("edit.submit");

        // 최초 로드 시에만 기존 게시글 내용을 폼에 채움 (PostBack 시 재로드 방지)
        if (!IsPostBack)
        {
            LoadPost(no);
        }
    }

    /// <summary>
    /// 기존 게시글 데이터 로드
    /// Bbs 테이블에서 해당 No의 제목/내용 조회 후 텍스트박스에 바인딩
    /// 본인 글이 아니면 접근 차단 (Author 컬럼과 세션 비교)
    /// </summary>
    /// <param name="no">수정할 게시글 번호</param>
    private void LoadPost(string no)
    {
        // Bbs 테이블에서 게시글 번호로 제목, 내용, 작성자 ID 조회
        string sql = string.Format(
            "SELECT No, Title, Contents, Author FROM Bbs WHERE No = {0}", no);

        DataSet ds = DbMan.DataAdapterFill(sql, "Edit");
        if (ds.Tables[0].Rows.Count > 0)
        {
            DataRow dr = ds.Tables[0].Rows[0];

            // 본인 글인지 검증 (타인이 URL 직접 입력하는 경우 차단)
            // DB의 Author 컬럼(작성자 ID)과 현재 세션의 UserID를 비교
            if (dr["Author"].ToString().Trim() != Session["UserID"].ToString())
            {
                // 작성자가 아닌 경우: 경고창 표시 후 게시글 목록으로 이동
                bool isEn = Lang.Get("nav.login") == "Login";
                Response.Write(string.Format(
                    "<script>alert('{0}'); location.href='Community.aspx';</script>",
                    isEn ? "You don't have permission." : "수정 권한이 없습니다."));
                return;
            }

            // 본인 글이 확인되면 기존 제목과 내용을 폼에 채워 넣음
            txtTitle.Text   = dr["Title"].ToString();
            txtContent.Text = dr["Contents"].ToString();
        }
        else
        {
            // 존재하지 않는 게시글이면 목록으로 이동
            Response.Redirect("Community.aspx");
        }
    }

    /// <summary>
    /// 저장 버튼 클릭 이벤트 핸들러
    /// - 제목 또는 내용이 비어 있으면 경고 메시지 표시
    /// - MemberDao.UpdateBbs() 호출로 Bbs 테이블의 게시글 내용을 UPDATE
    /// - 수정 성공 시 해당 게시글 상세 페이지로 리다이렉트
    /// - 수정 실패 시 오류 메시지를 alert으로 표시
    /// </summary>
    protected void btnSave_Click(object sender, EventArgs e)
    {
        // URL 파라미터에서 게시글 번호, 폼에서 수정된 제목과 내용 가져오기
        string no       = Request.QueryString["no"];
        string title    = txtTitle.Text.Trim();
        string contents = txtContent.Text.Trim();

        // 제목이나 내용 중 하나라도 비어 있으면 저장 중단하고 경고 표시
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(contents))
        {
            bool isEn = Lang.Get("nav.login") == "Login";
            Response.Write(string.Format(
                "<script>alert('{0}');</script>",
                isEn ? "Please fill in all fields." : "제목과 내용을 모두 입력해주세요."));
            return;
        }

        // MemberDao를 통해 Bbs 테이블의 해당 게시글 제목/내용을 UPDATE
        MemberDao dao = new MemberDao();
        int result = dao.UpdateBbs(Convert.ToInt32(no), title, contents);

        if (result > 0)
        {
            // 수정 성공 시 수정된 게시글의 상세 보기 페이지로 리다이렉트
            Response.Redirect("CommunityView.aspx?no=" + no);
        }
        else
        {
            // 수정 실패 시 오류 메시지를 alert으로 표시하고 현재 페이지에 머뭄
            bool isEn = Lang.Get("nav.login") == "Login";
            Response.Write(string.Format(
                "<script>alert('{0}');</script>",
                isEn ? "Failed to update post." : "수정 중 오류가 발생했습니다."));
        }
    }
}
