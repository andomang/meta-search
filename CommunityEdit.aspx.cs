using System;
using System.Data;
using System.Web.UI;

/// <summary>
/// 게시글 수정 페이지 코드비하인드 (새 페이지)
/// 기능:
///  - 게시글 번호(no)로 기존 내용 불러오기
///  - 본인 글인지 세션으로 검증 (타인 접근 차단)
///  - 수정 내용 저장 (MemberDao.UpdateBbs 호출)
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class CommunityEdit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // 비로그인 접근 차단
        if (Session["UserID"] == null)
        {
            bool isEn = Lang.Get("nav.login") == "Login";
            Response.Write(string.Format(
                "<script>alert('{0}'); location.href='Default.aspx';</script>",
                isEn ? "Please log in first." : "로그인 후 이용 가능합니다."));
            return;
        }

        // 게시글 번호 없으면 목록으로 리다이렉트
        string no = Request.QueryString["no"];
        if (string.IsNullOrEmpty(no)) Response.Redirect("Community.aspx");

        // 다국어 텍스트 바인딩
        litPageTitle.Text  = Lang.Get("edit.title");
        litTitleLbl.Text   = Lang.Get("edit.titleLbl");
        litContentLbl.Text = Lang.Get("edit.content");
        litCancelBtn.Text  = Lang.Get("edit.cancel");
        btnSave.Text       = Lang.Get("edit.submit");

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
    private void LoadPost(string no)
    {
        string sql = string.Format(
            "SELECT No, Title, Contents, Author FROM Bbs WHERE No = {0}", no);

        DataSet ds = DbMan.DataAdapterFill(sql, "Edit");
        if (ds.Tables[0].Rows.Count > 0)
        {
            DataRow dr = ds.Tables[0].Rows[0];

            // 본인 글인지 검증 (타인이 URL 직접 입력하는 경우 차단)
            if (dr["Author"].ToString().Trim() != Session["UserID"].ToString())
            {
                bool isEn = Lang.Get("nav.login") == "Login";
                Response.Write(string.Format(
                    "<script>alert('{0}'); location.href='Community.aspx';</script>",
                    isEn ? "You don't have permission." : "수정 권한이 없습니다."));
                return;
            }

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
    /// 저장 버튼 클릭 이벤트
    /// MemberDao.UpdateBbs 호출 후 상세 페이지로 리다이렉트
    /// </summary>
    protected void btnSave_Click(object sender, EventArgs e)
    {
        string no       = Request.QueryString["no"];
        string title    = txtTitle.Text.Trim();
        string contents = txtContent.Text.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(contents))
        {
            bool isEn = Lang.Get("nav.login") == "Login";
            Response.Write(string.Format(
                "<script>alert('{0}');</script>",
                isEn ? "Please fill in all fields." : "제목과 내용을 모두 입력해주세요."));
            return;
        }

        MemberDao dao = new MemberDao();
        int result = dao.UpdateBbs(Convert.ToInt32(no), title, contents);

        if (result > 0)
        {
            // 수정 성공 시 상세 페이지로 이동
            Response.Redirect("CommunityView.aspx?no=" + no);
        }
        else
        {
            bool isEn = Lang.Get("nav.login") == "Login";
            Response.Write(string.Format(
                "<script>alert('{0}');</script>",
                isEn ? "Failed to update post." : "수정 중 오류가 발생했습니다."));
        }
    }
}
