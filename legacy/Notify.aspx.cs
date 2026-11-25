using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

/// <summary>
/// 알림(Notifications) 처리 페이지 코드비하인드
/// URL: Notify.aspx?action=액션명[&notifID=번호]
/// 이 페이지는 HTML을 렌더링하지 않고 항상 JSON만 반환하는 순수 AJAX 엔드포인트
/// 주요 기능:
///  - 비로그인 사용자 접근 차단 (빈 배열 반환)
///  - action == "markRead"  : 해당 사용자의 모든 알림을 읽음 처리 (procMarkNotificationsRead 저장프로시저)
///  - action == "markOne"   : 특정 알림 하나만 읽음 처리 (Notifications 테이블 UPDATE)
///  - action == "delete"    : 특정 알림 삭제 (Notifications 테이블 DELETE)
///  - action 없음 (getList) : 최근 알림 TOP 20 목록을 JSON 배열로 반환
/// </summary>
public partial class Notify : System.Web.UI.Page
{
    /// <summary>
    /// 페이지 로드 이벤트 핸들러
    /// - 이 페이지는 항상 JSON을 반환하므로 ContentType을 application/json으로 설정
    /// - 비로그인 상태이면 빈 배열([])을 반환하고 즉시 종료
    /// - action 파라미터에 따라 markRead, markOne, delete, getList(기본) 중 하나를 처리
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 이 페이지는 순수 JSON API 엔드포인트이므로 ContentType을 JSON으로 고정
        Response.ContentType = "application/json";

        // 세션에서 로그인한 사용자 ID를 가져옴 (비로그인이면 null)
        string uid = Session["UserID"] != null ? Session["UserID"].ToString() : null;

        // 비로그인 사용자는 알림을 볼 수 없으므로 빈 배열 반환 후 즉시 종료
        if (uid == null) { Response.Write("[]"); Response.End(); return; }

        // URL 파라미터에서 수행할 동작(action)을 읽어옴
        string action = Request.QueryString["action"];

        // --- action == "markRead": 모든 알림 읽음 처리 ---
        // 사용자가 알림 패널을 열었을 때 JavaScript가 호출하여 모든 알림의 IsRead를 1로 변경
        if (action == "markRead")
        {
            try
            {
                // procMarkNotificationsRead 저장프로시저 호출: 해당 사용자의 모든 알림 IsRead=1 UPDATE
                SqlConnection conn = DbMan.Open();
                SqlCommand cmd = new SqlCommand("procMarkNotificationsRead", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", uid); // 읽음 처리할 사용자 ID
                cmd.ExecuteNonQuery();
                DbMan.Close();
            }
            catch { DbMan.Close(); }

            // 처리 완료 응답 후 종료
            Response.Write("{\"result\":\"ok\"}");
            Response.End();
            return;
        }

        // --- action == "markOne": 특정 알림 하나만 읽음 처리 ---
        // 사용자가 특정 알림 항목을 클릭했을 때 JavaScript가 호출하여 해당 알림만 읽음 처리
        if (action == "markOne")
        {
            // URL 파라미터에서 읽음 처리할 알림의 고유 번호를 가져옴
            int notifID = 0;
            int.TryParse(Request.QueryString["notifID"], out notifID);
            if (notifID > 0)
            {
                try
                {
                    // Notifications 테이블에서 해당 알림의 IsRead를 1로 UPDATE
                    // WHERE 조건에 UserID도 포함하여 타인의 알림은 변경 불가
                    SqlConnection conn = DbMan.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Notifications SET IsRead=1 WHERE NotifID=@NID AND UserID=@UID", conn);
                    cmd.Parameters.AddWithValue("@NID", notifID); // 읽음 처리할 알림 번호
                    cmd.Parameters.AddWithValue("@UID", uid);     // 요청한 사용자 ID (보안 확인)
                    cmd.ExecuteNonQuery();
                    DbMan.Close();
                }
                catch { DbMan.Close(); }
            }

            // 처리 완료 응답 후 종료
            Response.Write("{\"result\":\"ok\"}");
            Response.End();
            return;
        }

        // --- action == "delete": 특정 알림 삭제 ---
        // 사용자가 알림 항목의 삭제 버튼을 클릭했을 때 JavaScript가 호출
        if (action == "delete")
        {
            // URL 파라미터에서 삭제할 알림의 고유 번호를 가져옴
            int notifID = 0;
            int.TryParse(Request.QueryString["notifID"], out notifID);
            if (notifID > 0)
            {
                try
                {
                    // Notifications 테이블에서 해당 알림 행을 DELETE
                    // WHERE 조건에 UserID도 포함하여 타인의 알림은 삭제 불가
                    SqlConnection conn = DbMan.Open();
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Notifications WHERE NotifID=@NID AND UserID=@UID", conn);
                    cmd.Parameters.AddWithValue("@NID", notifID); // 삭제할 알림 번호
                    cmd.Parameters.AddWithValue("@UID", uid);     // 요청한 사용자 ID (보안 확인)
                    cmd.ExecuteNonQuery();
                    DbMan.Close();
                }
                catch { DbMan.Close(); }
            }

            // 처리 완료 응답 후 종료
            Response.Write("{\"result\":\"ok\"}");
            Response.End();
            return;
        }

        // --- action 없음(기본): 알림 목록 조회 (getList) ---
        // 페이지 로드 또는 알림 패널 열기 시 JavaScript가 action 없이 호출하여 알림 목록을 가져감
        // Notifications 테이블과 Bbs, members 테이블을 JOIN하여 알림 상세 정보 포함
        string sql = string.Format(@"
            SELECT TOP 20
                n.NotifID, n.Type, n.BbsNo, n.CommentID, n.IsRead,
                b.Title AS BbsTitle,
                CASE
                    WHEN n.ActorID IS NOT NULL
                    THEN (SELECT RTRIM(m2.Nickname) FROM members m2 WHERE RTRIM(m2.userid) = RTRIM(n.ActorID))
                    ELSE ''
                END AS ActorNick
            FROM Notifications n
            INNER JOIN Bbs b ON n.BbsNo = b.No
            WHERE n.UserID = '{0}'
            ORDER BY n.NotifID DESC", uid);
        // 해당 사용자의 알림을 최신 순(NotifID DESC)으로 최대 20개 조회
        // Bbs 테이블 JOIN으로 게시글 제목 포함, members 서브쿼리로 알림 발생자 닉네임 포함

        try
        {
            DataSet ds = DbMan.DataAdapterFill(sql, "Notif");
            DataTable dt = ds.Tables[0];

            // JSON 배열을 StringBuilder로 직접 조립
            // JSON 라이브러리 없이 구성하여 외부 의존성 없이 동작
            // 형식: [{"notifID":1,"type":"like","bbsNo":5,"bbsTitle":"제목","actorNick":"닉네임","isRead":false}, ...]
            StringBuilder sb = new StringBuilder("[");

            // 알림 행을 순회하며 각 알림을 JSON 객체로 변환
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(","); // 첫 번째 항목 이후에만 쉼표로 구분
                DataRow dr = dt.Rows[i];

                // JSON 문자열 값에서 특수문자(따옴표) 이스케이프 처리
                string title     = dr["BbsTitle"].ToString().Replace("\"", "\\\""); // 게시글 제목
                string actorNick = dr["ActorNick"].ToString().Replace("\"", "\\\""); // 알림 발생자 닉네임
                string type      = dr["Type"].ToString(); // 알림 타입 ("like" 또는 "comment")
                bool   isRead    = dr["IsRead"] != DBNull.Value && Convert.ToBoolean(dr["IsRead"]); // 읽음 여부

                // 각 알림의 정보를 JSON 객체 형태로 StringBuilder에 추가
                sb.AppendFormat(
                    "{{\"notifID\":{0},\"type\":\"{1}\",\"bbsNo\":{2},\"bbsTitle\":\"{3}\",\"actorNick\":\"{4}\",\"isRead\":{5}}}",
                    dr["NotifID"],  // 알림 고유 번호
                    type,           // 알림 종류 (like/comment)
                    dr["BbsNo"],    // 관련 게시글 번호
                    title,          // 게시글 제목 (알림 클릭 시 이동용)
                    actorNick,      // 좋아요/댓글을 남긴 사용자 닉네임
                    isRead.ToString().ToLower()); // JSON boolean: true 또는 false
            }
            sb.Append("]");

            // 완성된 JSON 배열 문자열을 응답으로 전송
            Response.Write(sb.ToString());
        }
        catch { DbMan.Close(); Response.Write("[]"); } // 조회 실패 시 빈 배열 반환

        // 알림 목록 JSON 응답 전송 후 페이지 렌더링 완전히 종료
        Response.End();
    }
}
