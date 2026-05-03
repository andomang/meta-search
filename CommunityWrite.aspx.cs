using System;
using System.IO;
using System.Web.UI;

/// <summary>
/// 커뮤니티 게시글 작성 페이지 코드비하인드
/// URL: CommunityWrite.aspx
/// 주요 기능:
///  - 비로그인 사용자 접근 차단 (Session["UserID"] 확인)
///  - 게시글 제목, 내용 입력 폼 표시
///  - 파일 첨부 기능 (서버의 ~/bbs/ 폴더에 저장)
///  - MemberDao.WriteBbs() 호출로 Bbs 테이블에 게시글 INSERT
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class CommunityWrite : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - 비로그인 상태이면 경고창 표시 후 메인 페이지로 이동
    /// - 로그인 상태이면 다국어 UI 텍스트를 바인딩하고 폼을 표시
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 세션에 UserID가 없으면 비로그인 상태 → 경고 후 Default.aspx로 이동
        if (Session["UserID"] == null)
        {
            // 자바스크립트 alert으로 경고 표시 후 메인 페이지로 강제 이동
            Response.Write("<script>alert('로그인 후 이용 가능합니다.'); location.href='Default.aspx';</script>");
            return;
        }

        // 다국어 텍스트 바인딩: 페이지 제목, 입력 필드 레이블, 버튼 텍스트, 플레이스홀더 등
        litPageTitle.Text = Lang.Get("write.title");
        litTitleLbl.Text = Lang.Get("write.titleLbl");
        litContentLbl.Text = Lang.Get("write.content");
        litFileLbl.Text = Lang.Get("write.file");
        litCancelBtn.Text = Lang.Get("write.cancel");
        btnSave.Text = Lang.Get("write.submit");
        txtTitle.Attributes["placeholder"] = Lang.Get("write.titlePh");
        txtContent.Attributes["placeholder"] = Lang.Get("write.contentPh");
    }

    /// <summary>
    /// 저장 버튼 클릭 이벤트 핸들러
    /// - 첨부파일이 있으면 서버의 ~/bbs/ 폴더에 저장하고 파일명/크기를 기록
    /// - MemberDao.WriteBbs()를 호출하여 Bbs 테이블에 게시글을 INSERT
    /// - 등록 성공 시 목록 페이지로 이동, 실패 시 오류 메시지 표시
    /// </summary>
    protected void btnSave_Click(object sender, EventArgs e)
    {
        // 폼에서 입력된 제목과 내용을 가져옴 (앞뒤 공백 제거)
        string title = txtTitle.Text.Trim();
        string contents = txtContent.Text.Trim();
        string fileName = "";  // 첨부파일 이름 (없으면 빈 문자열)
        int fileSize = 0;      // 첨부파일 크기(바이트), 없으면 0

        // 파일이 첨부된 경우에만 서버에 저장
        if (fileUpload.HasFile)
        {
            // 업로드된 파일의 원본 파일명 추출 (경로 없이 파일명만)
            fileName = System.IO.Path.GetFileName(fileUpload.PostedFile.FileName);
            fileSize = fileUpload.PostedFile.ContentLength; // 파일 크기(바이트)

            // 서버의 ~/bbs/ 폴더에 파일을 실제로 저장
            fileUpload.SaveAs(Server.MapPath("~/bbs/") + fileName);
        }

        // MemberDao를 통해 Bbs 테이블에 게시글 INSERT
        // 파라미터: 제목, 내용, 작성자 ID(세션), 파일명, 파일 크기
        MemberDao dao = new MemberDao();
        int result = dao.WriteBbs(title, contents, Session["UserID"].ToString(), fileName, fileSize);

        if (result > 0)
            // 등록 성공: 자바스크립트 alert 표시 후 게시글 목록 페이지로 이동
            Response.Write("<script>alert('등록되었습니다.'); location.href='Community.aspx';</script>");
        else
            // 등록 실패: 오류 메시지를 alert으로 표시하고 현재 페이지에 머뭄
            Response.Write("<script>alert('등록 중 오류가 발생했습니다.');</script>");
    }
}
