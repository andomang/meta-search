using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

/// <summary>
/// 커뮤니티 게시글 상세 보기 페이지 코드비하인드
/// URL: CommunityView.aspx?no=게시글번호
/// 주요 기능:
///  - 게시글 제목, 내용, 작성자, 날짜, 첨부파일 표시
///  - 조회수 자동 증가 (페이지 로드 시 Hits +1)
///  - 좋아요 토글 (AJAX, procToggleLike 저장프로시저 호출)
///  - 댓글 추가/삭제 (AJAX, procAddComment 저장프로시저 호출)
///  - 좋아요/댓글 작성 시 게시글 작성자에게 알림 생성 (procAddNotification)
///  - 본인 게시글에만 수정/삭제 버튼 표시
///  - 다국어 텍스트 바인딩 (Lang.Get)
/// </summary>
public partial class CommunityView : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - URL의 no 파라미터로 게시글 번호를 확인하고, 없으면 목록으로 리다이렉트
    /// - action 파라미터가 있으면 AJAX 요청으로 처리하고 일반 페이지 렌더링 생략
    /// - 최초 로드 시 조회수 증가, 게시글 내용, 댓글 목록을 순서대로 로드
    /// - 로그인 상태이면 댓글 작성 폼을 표시하고 아바타 첫 글자를 설정
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // URL 파라미터 no가 없으면 게시글을 특정할 수 없으므로 목록으로 돌려보냄
        string no = Request.QueryString["no"];
        if (string.IsNullOrEmpty(no)) { Response.Redirect("Community.aspx"); return; }

        // action 파라미터가 있으면 AJAX 요청이므로 JSON 응답 처리 후 일반 렌더링 중단
        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action, no); return; }

        // 다국어 UI 텍스트 바인딩 (게시판 라벨, 뒤로가기 버튼, 삭제 버튼 등)
        litBoardLabel.Text   = Lang.Get("view.board");
        litBackBtn.Text      = Lang.Get("view.back");
        btnDelete.Text       = Lang.Get("view.delete");
        litCommentsLabel.Text = Lang.Get("comm.comments");
        litNoComments.Text   = Lang.Get("comm.noComments");

        // 삭제 버튼 클릭 시 브라우저 확인 다이얼로그 표시 (자바스크립트 confirm)
        btnDelete.OnClientClick = string.Format("return confirm('{0}');", Lang.Get("view.confirmDel"));

        // 숨김 필드에 게시글 번호와 로그인 여부를 저장 (JavaScript에서 AJAX 요청 시 사용)
        hdnPostNo.Value  = no;
        hdnIsLogin.Value = Session["UserID"] != null ? "true" : "false";

        // 최초 로드 시에만 DB 조회 수행 (PostBack 시 중복 조회 방지)
        if (!IsPostBack)
        {
            // 조회수를 1 증가시킨 후 게시글 내용과 댓글 목록을 불러옴
            UpdateHits(no);
            LoadPost(no);
            LoadComments(no);
        }

        // 로그인한 사용자에게만 댓글 작성 폼 표시
        if (Session["UserID"] != null)
        {
            phCommentForm.Visible = true;
            litCommentBtn.Text    = Lang.Get("comm.commentBtn");
            litCommentPh.Text     = Lang.Get("comm.commentPh");

            // 프로필 사진이 없는 경우 닉네임 첫 글자를 아바타로 표시
            string nick = Session["UserName"] != null ? Session["UserName"].ToString() : "?";
            litMyAvatar.Text = nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?";
        }
    }

    /// <summary>
    /// AJAX 요청 분기 처리 메서드
    /// fetch API로 action 파라미터와 함께 호출될 때 JSON 응답을 반환하고 Response.End()로 종료
    /// - getLike      : 현재 좋아요 수와 내가 좋아요 눌렀는지 여부 반환
    /// - toggleLike   : 좋아요 추가/취소 토글 (procToggleLike 저장프로시저), 알림 생성
    /// - addComment   : 댓글 추가 (procAddComment 저장프로시저), 알림 생성, 새 댓글 HTML 정보 반환
    /// - deleteComment: 본인 댓글 삭제 (BbsComment 테이블 DELETE)
    /// </summary>
    private void HandleAjax(string action, string no)
    {
        // 응답 형식을 JSON으로 설정 (브라우저에서 fetch로 파싱)
        Response.ContentType = "application/json";

        // 세션에서 로그인한 사용자 ID를 가져옴 (비로그인이면 null)
        string uid = Session["UserID"] != null ? Session["UserID"].ToString() : null;

        // --- action == "getLike": 좋아요 수 조회 ---
        // 현재 게시글의 전체 좋아요 수와 현재 사용자가 좋아요를 눌렀는지 여부를 반환
        if (action == "getLike")
        {
            int count = 0; bool liked = false;
            try
            {
                // BbsLike 테이블에서 해당 게시글의 전체 좋아요 수 조회
                SqlDataReader r = DbMan.ExecuteReader(
                    string.Format("SELECT COUNT(*) FROM BbsLike WHERE BbsNo={0}", no));
                if (r.Read()) count = Convert.ToInt32(r[0]);
                r.Close(); DbMan.Close();

                // 로그인 상태이면 현재 사용자가 이미 좋아요를 눌렀는지 추가 확인
                if (uid != null)
                {
                    SqlDataReader r2 = DbMan.ExecuteReader(
                        string.Format("SELECT COUNT(*) FROM BbsLike WHERE BbsNo={0} AND UserID='{1}'", no, uid));
                    if (r2.Read()) liked = Convert.ToInt32(r2[0]) > 0;
                    r2.Close(); DbMan.Close();
                }
            }
            catch { DbMan.Close(); }

            // {"count": 좋아요수, "liked": true/false} 형태로 JSON 응답
            Response.Write(string.Format("{{\"count\":{0},\"liked\":{1}}}", count, liked.ToString().ToLower()));
        }
        // --- action == "toggleLike": 좋아요 토글 ---
        // 이미 좋아요를 눌렀으면 취소, 안 눌렀으면 추가 (procToggleLike 저장프로시저)
        // 좋아요를 새로 추가한 경우 게시글 작성자에게 알림도 생성
        else if (action == "toggleLike")
        {
            // 비로그인 사용자는 좋아요 불가
            if (uid == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            bool liked = false; int count = 0;
            try
            {
                // procToggleLike 저장프로시저 호출: 좋아요 추가/취소 처리
                // 출력 파라미터 @Liked(토글 후 상태), @LikeCount(최신 좋아요 수) 반환
                SqlConnection conn = DbMan.Open();
                SqlCommand cmd = new SqlCommand("procToggleLike", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BbsNo",  int.Parse(no));  // 대상 게시글 번호
                cmd.Parameters.AddWithValue("@UserID", uid);             // 요청한 사용자 ID
                var pLiked = cmd.Parameters.Add("@Liked",     SqlDbType.Bit); pLiked.Direction = System.Data.ParameterDirection.Output;
                var pCount = cmd.Parameters.Add("@LikeCount", SqlDbType.Int); pCount.Direction = System.Data.ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                liked = (bool)pLiked.Value;  // true: 좋아요 추가됨, false: 취소됨
                count = (int)pCount.Value;   // 토글 후 최신 좋아요 수
                DbMan.Close();

                // 좋아요 알림
                // 좋아요를 새로 추가한 경우에만 게시글 작성자에게 'like' 타입 알림 생성
                if (liked)
                {
                    SqlConnection conn2 = DbMan.Open();
                    SqlCommand cmd2 = new SqlCommand("procAddNotification", conn2);
                    cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd2.Parameters.AddWithValue("@ActorID",   uid);             // 좋아요를 누른 사용자
                    cmd2.Parameters.AddWithValue("@BbsNo",     int.Parse(no));   // 대상 게시글 번호
                    cmd2.Parameters.Add("@CommentID", SqlDbType.Int).Value = DBNull.Value; // 댓글 없으므로 NULL
                    cmd2.Parameters.AddWithValue("@Type", "like");               // 알림 타입: 좋아요
                    cmd2.ExecuteNonQuery();
                    DbMan.Close();
                }
            }
            catch { DbMan.Close(); }

            // {"liked": true/false, "count": 좋아요수} 형태로 JSON 응답
            Response.Write(string.Format("{{\"liked\":{0},\"count\":{1}}}", liked.ToString().ToLower(), count));
        }
        // --- action == "addComment": 댓글 추가 ---
        // 댓글을 DB에 저장하고, 게시글 작성자에게 'comment' 알림 생성
        // 응답에 새 댓글 HTML 구성에 필요한 정보(닉네임, 아바타, 시간 등)를 함께 반환
        else if (action == "addComment")
        {
            // 비로그인 사용자는 댓글 작성 불가
            if (uid == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }

            // 댓글 내용이 비어 있으면 오류 반환
            string content = Request.QueryString["content"] ?? "";
            if (string.IsNullOrWhiteSpace(content)) { Response.Write("{\"result\":\"empty\"}"); Response.End(); return; }

            int commentID = 0;
            try
            {
                // procAddComment 저장프로시저 호출: BbsComment 테이블에 댓글 INSERT
                // 출력 파라미터 @CommentID로 새로 생성된 댓글 ID를 받아옴
                SqlConnection conn = DbMan.Open();
                SqlCommand cmd = new SqlCommand("procAddComment", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BbsNo",   int.Parse(no)); // 댓글이 달릴 게시글 번호
                cmd.Parameters.AddWithValue("@Author",  uid);           // 댓글 작성자 ID
                cmd.Parameters.AddWithValue("@Content", content);       // 댓글 내용
                var pID = cmd.Parameters.Add("@CommentID", SqlDbType.Int);
                pID.Direction = System.Data.ParameterDirection.Output;  // 새로 생성된 댓글 ID (OUTPUT)
                cmd.ExecuteNonQuery();
                commentID = (int)pID.Value;  // 방금 INSERT된 댓글의 고유 번호
                DbMan.Close();

                // 댓글 알림: 게시글 작성자에게 'comment' 타입 알림 생성 (procAddNotification)
                SqlConnection conn2 = DbMan.Open();
                SqlCommand cmd2 = new SqlCommand("procAddNotification", conn2);
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                cmd2.Parameters.AddWithValue("@ActorID",   uid);             // 댓글 작성자
                cmd2.Parameters.AddWithValue("@BbsNo",     int.Parse(no));   // 대상 게시글
                cmd2.Parameters.AddWithValue("@CommentID", commentID);        // 방금 생성된 댓글 ID
                cmd2.Parameters.AddWithValue("@Type",      "comment");        // 알림 타입: 댓글
                cmd2.ExecuteNonQuery();
                DbMan.Close();
            }
            catch { DbMan.Close(); Response.Write("{\"result\":\"error\"}"); Response.End(); return; }

            // 닉네임 + 프로필 조회: 댓글 아바타를 화면에 즉시 표시하기 위해 필요
            string nick = ""; string profileImg = "";
            try
            {
                // members 테이블에서 닉네임과 프로필 이미지 파일명 조회
                SqlDataReader r = DbMan.ExecuteReader(
                    string.Format("SELECT Nickname, ProfileImg FROM members WHERE userid='{0}'", uid));
                if (r.Read()) { nick = r["Nickname"].ToString().Trim(); profileImg = r["ProfileImg"] != DBNull.Value ? r["ProfileImg"].ToString() : ""; }
                r.Close(); DbMan.Close();
            }
            catch { DbMan.Close(); }

            // 댓글 추가 후 해당 게시글의 전체 댓글 수를 다시 집계 (UI 카운터 업데이트용)
            int totalCount = 0;
            try
            {
                // BbsComment 테이블에서 현재 게시글의 댓글 총 개수 조회
                SqlDataReader r = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM BbsComment WHERE BbsNo={0}", no));
                if (r.Read()) totalCount = Convert.ToInt32(r[0]);
                r.Close(); DbMan.Close();
            }
            catch { DbMan.Close(); }

            // 아바타 HTML 생성: 프로필 이미지가 있으면 <img> 태그, 없으면 닉네임 첫 글자
            string avatarHtml = !string.IsNullOrEmpty(profileImg)
                ? string.Format("<img src='uploads/{0}' class='w-full h-full object-cover' />", profileImg)
                : (nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?");

            // JSON 직접 조립: 새 댓글을 화면에 즉시 추가하기 위해 JavaScript가 필요한 모든 정보를 포함
            // (JSON 라이브러리 의존성 없이 StringBuilder로 직접 구성)
            string delBtnText = Lang.Get("comm.delComment");
            string createdAt  = DateTime.Now.ToString("yyyy.MM.dd HH:mm");

            // JSON 문자열에 들어갈 내용의 특수문자를 이스케이프 처리
            string safeContent = content.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
            string safeNick    = nick.Replace("\"", "\\\"");

            // 응답 JSON: 댓글 ID, 닉네임, 내용, 날짜, 아바타 HTML, 삭제 버튼 텍스트, 전체 댓글 수 포함
            Response.Write(string.Format(
                "{{\"result\":\"ok\",\"commentID\":{0},\"nick\":\"{1}\",\"content\":\"{2}\",\"createdAt\":\"{3}\",\"avatarHtml\":\"{4}\",\"delBtnText\":\"{5}\",\"totalCount\":{6}}}",
                commentID, safeNick, safeContent, createdAt, avatarHtml.Replace("\"","\\\""), delBtnText, totalCount));
        }
        // --- action == "deleteComment": 댓글 삭제 ---
        // 본인이 작성한 댓글만 삭제 가능 (SQL WHERE 절에 Author 조건 포함)
        else if (action == "deleteComment")
        {
            // 비로그인 사용자는 댓글 삭제 불가
            if (uid == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }

            string commentID = Request.QueryString["commentID"] ?? "";
            try
            {
                // BbsComment 테이블에서 해당 댓글을 삭제
                // WHERE 절에 Author=uid 조건을 추가하여 타인의 댓글은 삭제 불가
                DbMan.ExecuteNonQuery(string.Format(
                    "DELETE FROM BbsComment WHERE CommentID={0} AND Author='{1}'", commentID, uid));
                DbMan.Close();
            }
            catch { DbMan.Close(); Response.Write("{\"result\":\"error\"}"); Response.End(); return; }

            // 삭제 후 해당 게시글의 남은 댓글 수를 다시 집계 (UI 카운터 업데이트용)
            int totalCount = 0;
            try
            {
                // BbsComment 테이블에서 현재 게시글의 남은 댓글 수 조회
                SqlDataReader r = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM BbsComment WHERE BbsNo={0}", no));
                if (r.Read()) totalCount = Convert.ToInt32(r[0]);
                r.Close(); DbMan.Close();
            }
            catch { DbMan.Close(); }

            // {"result": "ok", "totalCount": 남은댓글수} 형태로 JSON 응답
            Response.Write(string.Format("{{\"result\":\"ok\",\"totalCount\":{0}}}", totalCount));
        }

        // 모든 AJAX 응답 처리 후 응답 전송을 완전히 종료하여 뒤의 HTML 렌더링 방지
        Response.End();
    }

    /// <summary>
    /// 게시글 조회수 증가
    /// 페이지 최초 로드 시 Bbs 테이블의 Hits 컬럼을 1 증가시킨다
    /// </summary>
    /// <param name="no">조회수를 증가시킬 게시글 번호</param>
    private void UpdateHits(string no)
    {
        try
        {
            // Bbs 테이블의 해당 게시글 Hits(조회수) 컬럼을 1 증가
            DbMan.ExecuteNonQuery("UPDATE Bbs SET Hits = Hits + 1 WHERE No = " + no);
            DbMan.Close();
        }
        catch { DbMan.Close(); }
    }

    /// <summary>
    /// 게시글 상세 내용 로드
    /// Bbs 테이블과 members 테이블을 JOIN하여 제목, 내용, 작성자, 날짜, 첨부파일 정보를 바인딩
    /// 본인 게시글이면 수정/삭제 버튼을 표시, 첨부파일이 있으면 다운로드 링크를 표시
    /// 게시글이 없으면 목록으로 리다이렉트
    /// </summary>
    /// <param name="no">조회할 게시글 번호</param>
    private void LoadPost(string no)
    {
        // Bbs 테이블(게시글)과 members 테이블(작성자 정보)을 JOIN하여 상세 내용 조회
        string sql = string.Format(@"
            SELECT b.*, m.Nickname, m.ProfileImg
            FROM Bbs b INNER JOIN members m ON b.Author = m.userid
            WHERE b.No = {0}", no);

        DataSet ds = DbMan.DataAdapterFill(sql, "View");

        // 게시글이 존재하지 않으면 목록으로 이동
        if (ds.Tables[0].Rows.Count == 0) { Response.Redirect("Community.aspx"); return; }

        DataRow dr = ds.Tables[0].Rows[0];

        // 조회된 게시글 정보를 화면의 Literal 컨트롤에 각각 바인딩
        litTitle.Text    = dr["Title"].ToString();
        litContents.Text = System.Web.HttpUtility.HtmlEncode(dr["Contents"].ToString()); // XSS 방지를 위해 HTML 인코딩
        litAuthor.Text   = dr["Nickname"].ToString().Trim();
        litDate.Text     = Convert.ToDateTime(dr["UploadTime"]).ToString("yyyy.MM.dd HH:mm");
        litHits.Text     = dr["Hits"].ToString();

        // 아바타 (프로필 사진 우선)
        // 프로필 이미지가 있으면 <img> 태그로, 없으면 닉네임 첫 글자로 아바타 표시
        string profileImg = dr["ProfileImg"] != DBNull.Value ? dr["ProfileImg"].ToString() : "";
        string nick       = dr["Nickname"].ToString().Trim();
        litAvatar.Text    = !string.IsNullOrEmpty(profileImg)
            ? string.Format("<img src='uploads/{0}' class='w-full h-full object-cover' />", profileImg)
            : (nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?");

        // 첨부파일이 있는 경우에만 파일 다운로드 영역을 표시
        if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
        {
            phFile.Visible         = true;
            litFileName.Text       = dr["FileName"].ToString();
            litFileSize.Text       = (Convert.ToInt32(dr["FileSize"]) / 1024).ToString(); // 바이트 → KB 변환
            litDownloadBtn.Text    = Lang.Get("view.download");
            hlDownload.NavigateUrl = "~/bbs/" + dr["FileName"].ToString(); // 첨부파일 저장 경로
        }

        // 현재 로그인한 사용자가 게시글 작성자인 경우에만 수정/삭제 버튼 영역 표시
        if (Session["UserID"] != null && Session["UserID"].ToString() == dr["Author"].ToString().Trim())
        {
            phOwnerActions.Visible = true;
            hlEdit.Text            = Lang.Get("view.edit");
            hlEdit.NavigateUrl     = "CommunityEdit.aspx?no=" + no; // 수정 페이지로 이동하는 링크
        }
    }

    /// <summary>
    /// 댓글 목록 로드
    /// BbsComment 테이블과 members 테이블을 JOIN하여 댓글 목록을 rptComments Repeater에 바인딩
    /// 작성 순서(CommentID ASC)대로 정렬하여 오래된 댓글이 위에 표시
    /// </summary>
    /// <param name="no">댓글을 불러올 게시글 번호</param>
    private void LoadComments(string no)
    {
        // BbsComment 테이블(댓글)과 members 테이블(작성자 정보)을 JOIN하여 댓글 목록 조회
        // CommentID 오름차순 정렬로 오래된 댓글이 먼저 표시됨
        string sql = string.Format(@"
            SELECT c.CommentID, c.Author, c.Content, c.CreatedAt,
                   m.Nickname AS AuthorNick, m.ProfileImg
            FROM BbsComment c
            INNER JOIN members m ON c.Author = m.userid
            WHERE c.BbsNo = {0}
            ORDER BY c.CommentID ASC", no);
        try
        {
            // 조회 결과를 rptComments Repeater에 바인딩하고 댓글 수를 표시
            DataSet ds = DbMan.DataAdapterFill(sql, "Comments");
            rptComments.DataSource = ds;
            rptComments.DataBind();
            litCommentCount.Text = ds.Tables[0].Rows.Count.ToString(); // 댓글 총 개수 표시
        }
        catch { DbMan.Close(); }
    }

    /// <summary>
    /// 댓글 아바타 HTML 반환 헬퍼 메서드 (ASPX 템플릿에서 직접 호출)
    /// 프로필 이미지가 있으면 &lt;img&gt; 태그를, 없으면 닉네임 첫 글자(대문자)를 반환
    /// Repeater의 ItemTemplate에서 &lt;%# GetAvatarHtml(...) %&gt; 형태로 호출됨
    /// </summary>
    /// <param name="nick">사용자 닉네임 (프로필 이미지 없을 때 첫 글자 사용)</param>
    /// <param name="profileImg">프로필 이미지 파일명 (없으면 빈 문자열)</param>
    /// <returns>아바타 표시용 HTML 문자열</returns>
    protected string GetAvatarHtml(string nick, string profileImg)
    {
        // 프로필 이미지 파일명이 있으면 uploads 폴더에서 이미지를 원형으로 표시
        if (!string.IsNullOrEmpty(profileImg))
            return string.Format("<img src='uploads/{0}' class='w-full h-full object-cover rounded-full' />", profileImg);

        // 프로필 이미지가 없으면 닉네임 첫 글자(대문자)를 아바타 대신 사용
        return nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?";
    }

    /// <summary>
    /// 댓글 삭제 버튼 HTML 반환 헬퍼 메서드 (ASPX 템플릿에서 직접 호출)
    /// 현재 로그인한 사용자가 해당 댓글의 작성자일 때만 삭제 버튼 HTML을 반환
    /// 다른 사람의 댓글이거나 비로그인 상태이면 빈 문자열 반환 (버튼 미표시)
    /// Repeater의 ItemTemplate에서 &lt;%# GetDeleteBtn(...) %&gt; 형태로 호출됨
    /// </summary>
    /// <param name="author">댓글 작성자의 UserID</param>
    /// <param name="commentID">삭제할 댓글의 고유 번호</param>
    /// <returns>삭제 버튼 HTML 문자열, 또는 빈 문자열</returns>
    protected string GetDeleteBtn(string author, string commentID)
    {
        // 비로그인 상태이면 삭제 버튼 미표시
        if (Session["UserID"] == null) return "";

        // 현재 로그인한 사용자가 댓글 작성자가 아니면 삭제 버튼 미표시
        if (Session["UserID"].ToString().Trim() != author.Trim()) return "";

        // 본인 댓글이면 deleteComment() 자바스크립트 함수를 호출하는 삭제 버튼 HTML 반환
        return string.Format(
            "<button type='button' onclick='deleteComment({0},this)' class='text-xs text-red-400 hover:text-red-600 transition-colors px-2 py-1'>{1}</button>",
            commentID, Lang.Get("comm.delComment"));
    }

    /// <summary>
    /// 게시글 삭제 버튼 클릭 이벤트 핸들러
    /// 게시글에 연결된 댓글, 좋아요, 알림을 먼저 삭제한 후 게시글 본문을 삭제
    /// (외래 키 제약조건이 없는 경우를 대비해 연관 데이터를 순서대로 수동 삭제)
    /// 삭제 완료 후 자바스크립트 alert을 표시하고 게시글 목록으로 이동
    /// </summary>
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string no = Request.QueryString["no"];
        try
        {
            // 1단계: BbsComment 테이블에서 해당 게시글의 모든 댓글 삭제
            DbMan.ExecuteNonQuery("DELETE FROM BbsComment WHERE BbsNo = " + no); DbMan.Close();

            // 2단계: BbsLike 테이블에서 해당 게시글의 모든 좋아요 삭제
            DbMan.ExecuteNonQuery("DELETE FROM BbsLike    WHERE BbsNo = " + no); DbMan.Close();

            // 3단계: Notifications 테이블에서 해당 게시글 관련 알림 삭제
            DbMan.ExecuteNonQuery("DELETE FROM Notifications WHERE BbsNo = " + no); DbMan.Close();

            // 4단계: Bbs 테이블에서 게시글 본문 삭제 (연관 데이터 모두 제거 후 마지막에 삭제)
            DbMan.ExecuteNonQuery("DELETE FROM Bbs WHERE No = " + no); DbMan.Close();
        }
        catch { DbMan.Close(); }

        // 삭제 완료 알림 후 게시글 목록 페이지로 이동 (자바스크립트 alert + location.href)
        bool isEn = Lang.Get("view.delete") == "Delete";
        Response.Write(string.Format(
            "<script>alert('{0}'); location.href='Community.aspx';</script>",
            isEn ? "Post deleted." : "삭제되었습니다."));
    }
}
