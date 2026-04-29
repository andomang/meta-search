using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

public partial class CommunityView : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string no = Request.QueryString["no"];
        if (string.IsNullOrEmpty(no)) { Response.Redirect("Community.aspx"); return; }

        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action, no); return; }

        litBoardLabel.Text   = Lang.Get("view.board");
        litBackBtn.Text      = Lang.Get("view.back");
        btnDelete.Text       = Lang.Get("view.delete");
        litCommentsLabel.Text = Lang.Get("comm.comments");
        litNoComments.Text   = Lang.Get("comm.noComments");
        btnDelete.OnClientClick = string.Format("return confirm('{0}');", Lang.Get("view.confirmDel"));

        hdnPostNo.Value  = no;
        hdnIsLogin.Value = Session["UserID"] != null ? "true" : "false";

        if (!IsPostBack)
        {
            UpdateHits(no);
            LoadPost(no);
            LoadComments(no);
        }

        if (Session["UserID"] != null)
        {
            phCommentForm.Visible = true;
            litCommentBtn.Text    = Lang.Get("comm.commentBtn");
            litCommentPh.Text     = Lang.Get("comm.commentPh");
            string nick = Session["UserName"] != null ? Session["UserName"].ToString() : "?";
            litMyAvatar.Text = nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?";
        }
    }

    private void HandleAjax(string action, string no)
    {
        Response.ContentType = "application/json";
        string uid = Session["UserID"] != null ? Session["UserID"].ToString() : null;

        if (action == "getLike")
        {
            int count = 0; bool liked = false;
            try
            {
                SqlDataReader r = DbMan.ExecuteReader(
                    string.Format("SELECT COUNT(*) FROM BbsLike WHERE BbsNo={0}", no));
                if (r.Read()) count = Convert.ToInt32(r[0]);
                r.Close(); DbMan.Close();
                if (uid != null)
                {
                    SqlDataReader r2 = DbMan.ExecuteReader(
                        string.Format("SELECT COUNT(*) FROM BbsLike WHERE BbsNo={0} AND UserID='{1}'", no, uid));
                    if (r2.Read()) liked = Convert.ToInt32(r2[0]) > 0;
                    r2.Close(); DbMan.Close();
                }
            }
            catch { DbMan.Close(); }
            Response.Write(string.Format("{{\"count\":{0},\"liked\":{1}}}", count, liked.ToString().ToLower()));
        }
        else if (action == "toggleLike")
        {
            if (uid == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            bool liked = false; int count = 0;
            try
            {
                SqlConnection conn = DbMan.Open();
                SqlCommand cmd = new SqlCommand("procToggleLike", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BbsNo",  int.Parse(no));
                cmd.Parameters.AddWithValue("@UserID", uid);
                var pLiked = cmd.Parameters.Add("@Liked",     SqlDbType.Bit); pLiked.Direction = System.Data.ParameterDirection.Output;
                var pCount = cmd.Parameters.Add("@LikeCount", SqlDbType.Int); pCount.Direction = System.Data.ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                liked = (bool)pLiked.Value;
                count = (int)pCount.Value;
                DbMan.Close();

                // 좋아요 알림
                if (liked)
                {
                    SqlConnection conn2 = DbMan.Open();
                    SqlCommand cmd2 = new SqlCommand("procAddNotification", conn2);
                    cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd2.Parameters.AddWithValue("@ActorID",   uid);
                    cmd2.Parameters.AddWithValue("@BbsNo",     int.Parse(no));
                    cmd2.Parameters.Add("@CommentID", SqlDbType.Int).Value = DBNull.Value;
                    cmd2.Parameters.AddWithValue("@Type", "like");
                    cmd2.ExecuteNonQuery();
                    DbMan.Close();
                }
            }
            catch { DbMan.Close(); }
            Response.Write(string.Format("{{\"liked\":{0},\"count\":{1}}}", liked.ToString().ToLower(), count));
        }
        else if (action == "addComment")
        {
            if (uid == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            string content = Request.QueryString["content"] ?? "";
            if (string.IsNullOrWhiteSpace(content)) { Response.Write("{\"result\":\"empty\"}"); Response.End(); return; }

            int commentID = 0;
            try
            {
                SqlConnection conn = DbMan.Open();
                SqlCommand cmd = new SqlCommand("procAddComment", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BbsNo",   int.Parse(no));
                cmd.Parameters.AddWithValue("@Author",  uid);
                cmd.Parameters.AddWithValue("@Content", content);
                var pID = cmd.Parameters.Add("@CommentID", SqlDbType.Int);
                pID.Direction = System.Data.ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                commentID = (int)pID.Value;
                DbMan.Close();

                // 댓글 알림
                SqlConnection conn2 = DbMan.Open();
                SqlCommand cmd2 = new SqlCommand("procAddNotification", conn2);
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                cmd2.Parameters.AddWithValue("@ActorID",   uid);
                cmd2.Parameters.AddWithValue("@BbsNo",     int.Parse(no));
                cmd2.Parameters.AddWithValue("@CommentID", commentID);
                cmd2.Parameters.AddWithValue("@Type",      "comment");
                cmd2.ExecuteNonQuery();
                DbMan.Close();
            }
            catch { DbMan.Close(); Response.Write("{\"result\":\"error\"}"); Response.End(); return; }

            // 닉네임 + 프로필 조회
            string nick = ""; string profileImg = "";
            try
            {
                SqlDataReader r = DbMan.ExecuteReader(
                    string.Format("SELECT Nickname, ProfileImg FROM members WHERE userid='{0}'", uid));
                if (r.Read()) { nick = r["Nickname"].ToString().Trim(); profileImg = r["ProfileImg"] != DBNull.Value ? r["ProfileImg"].ToString() : ""; }
                r.Close(); DbMan.Close();
            }
            catch { DbMan.Close(); }

            int totalCount = 0;
            try
            {
                SqlDataReader r = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM BbsComment WHERE BbsNo={0}", no));
                if (r.Read()) totalCount = Convert.ToInt32(r[0]);
                r.Close(); DbMan.Close();
            }
            catch { DbMan.Close(); }

            string avatarHtml = !string.IsNullOrEmpty(profileImg)
                ? string.Format("<img src='uploads/{0}' class='w-full h-full object-cover' />", profileImg)
                : (nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?");

            string delBtnText = Lang.Get("comm.delComment");
            string createdAt  = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
            string safeContent = content.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
            string safeNick    = nick.Replace("\"", "\\\"");

            Response.Write(string.Format(
                "{{\"result\":\"ok\",\"commentID\":{0},\"nick\":\"{1}\",\"content\":\"{2}\",\"createdAt\":\"{3}\",\"avatarHtml\":\"{4}\",\"delBtnText\":\"{5}\",\"totalCount\":{6}}}",
                commentID, safeNick, safeContent, createdAt, avatarHtml.Replace("\"","\\\""), delBtnText, totalCount));
        }
        else if (action == "deleteComment")
        {
            if (uid == null) { Response.Write("{\"result\":\"unauthorized\"}"); Response.End(); return; }
            string commentID = Request.QueryString["commentID"] ?? "";
            try
            {
                DbMan.ExecuteNonQuery(string.Format(
                    "DELETE FROM BbsComment WHERE CommentID={0} AND Author='{1}'", commentID, uid));
                DbMan.Close();
            }
            catch { DbMan.Close(); Response.Write("{\"result\":\"error\"}"); Response.End(); return; }

            int totalCount = 0;
            try
            {
                SqlDataReader r = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM BbsComment WHERE BbsNo={0}", no));
                if (r.Read()) totalCount = Convert.ToInt32(r[0]);
                r.Close(); DbMan.Close();
            }
            catch { DbMan.Close(); }

            Response.Write(string.Format("{{\"result\":\"ok\",\"totalCount\":{0}}}", totalCount));
        }

        Response.End();
    }

    private void UpdateHits(string no)
    {
        try { DbMan.ExecuteNonQuery("UPDATE Bbs SET Hits = Hits + 1 WHERE No = " + no); DbMan.Close(); }
        catch { DbMan.Close(); }
    }

    private void LoadPost(string no)
    {
        string sql = string.Format(@"
            SELECT b.*, m.Nickname, m.ProfileImg
            FROM Bbs b INNER JOIN members m ON b.Author = m.userid
            WHERE b.No = {0}", no);

        DataSet ds = DbMan.DataAdapterFill(sql, "View");
        if (ds.Tables[0].Rows.Count == 0) { Response.Redirect("Community.aspx"); return; }

        DataRow dr = ds.Tables[0].Rows[0];
        litTitle.Text    = dr["Title"].ToString();
        litContents.Text = System.Web.HttpUtility.HtmlEncode(dr["Contents"].ToString());
        litAuthor.Text   = dr["Nickname"].ToString().Trim();
        litDate.Text     = Convert.ToDateTime(dr["UploadTime"]).ToString("yyyy.MM.dd HH:mm");
        litHits.Text     = dr["Hits"].ToString();

        // 아바타 (프로필 사진 우선)
        string profileImg = dr["ProfileImg"] != DBNull.Value ? dr["ProfileImg"].ToString() : "";
        string nick       = dr["Nickname"].ToString().Trim();
        litAvatar.Text    = !string.IsNullOrEmpty(profileImg)
            ? string.Format("<img src='uploads/{0}' class='w-full h-full object-cover' />", profileImg)
            : (nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?");

        if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
        {
            phFile.Visible         = true;
            litFileName.Text       = dr["FileName"].ToString();
            litFileSize.Text       = (Convert.ToInt32(dr["FileSize"]) / 1024).ToString();
            litDownloadBtn.Text    = Lang.Get("view.download");
            hlDownload.NavigateUrl = "~/bbs/" + dr["FileName"].ToString();
        }

        if (Session["UserID"] != null && Session["UserID"].ToString() == dr["Author"].ToString().Trim())
        {
            phOwnerActions.Visible = true;
            hlEdit.Text            = Lang.Get("view.edit");
            hlEdit.NavigateUrl     = "CommunityEdit.aspx?no=" + no;
        }
    }

    private void LoadComments(string no)
    {
        string sql = string.Format(@"
            SELECT c.CommentID, c.Author, c.Content, c.CreatedAt,
                   m.Nickname AS AuthorNick, m.ProfileImg
            FROM BbsComment c
            INNER JOIN members m ON c.Author = m.userid
            WHERE c.BbsNo = {0}
            ORDER BY c.CommentID ASC", no);
        try
        {
            DataSet ds = DbMan.DataAdapterFill(sql, "Comments");
            rptComments.DataSource = ds;
            rptComments.DataBind();
            litCommentCount.Text = ds.Tables[0].Rows.Count.ToString();
        }
        catch { DbMan.Close(); }
    }

    // 댓글 아바타 HTML 반환 (프로필 사진 우선)
    protected string GetAvatarHtml(string nick, string profileImg)
    {
        if (!string.IsNullOrEmpty(profileImg))
            return string.Format("<img src='uploads/{0}' class='w-full h-full object-cover rounded-full' />", profileImg);
        return nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?";
    }

    // 본인 댓글에만 삭제 버튼 표시
    protected string GetDeleteBtn(string author, string commentID)
    {
        if (Session["UserID"] == null) return "";
        if (Session["UserID"].ToString().Trim() != author.Trim()) return "";
        return string.Format(
            "<button type='button' onclick='deleteComment({0},this)' class='text-xs text-red-400 hover:text-red-600 transition-colors px-2 py-1'>{1}</button>",
            commentID, Lang.Get("comm.delComment"));
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string no = Request.QueryString["no"];
        try
        {
            DbMan.ExecuteNonQuery("DELETE FROM BbsComment WHERE BbsNo = " + no); DbMan.Close();
            DbMan.ExecuteNonQuery("DELETE FROM BbsLike    WHERE BbsNo = " + no); DbMan.Close();
            DbMan.ExecuteNonQuery("DELETE FROM Notifications WHERE BbsNo = " + no); DbMan.Close();
            DbMan.ExecuteNonQuery("DELETE FROM Bbs WHERE No = " + no); DbMan.Close();
        }
        catch { DbMan.Close(); }
        bool isEn = Lang.Get("view.delete") == "Delete";
        Response.Write(string.Format(
            "<script>alert('{0}'); location.href='Community.aspx';</script>",
            isEn ? "Post deleted." : "삭제되었습니다."));
    }
}
