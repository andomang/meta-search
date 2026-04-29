using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

public partial class Notify : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json";
        string uid = Session["UserID"] != null ? Session["UserID"].ToString() : null;
        if (uid == null) { Response.Write("[]"); Response.End(); return; }

        string action = Request.QueryString["action"];

        if (action == "markRead")
        {
            try
            {
                SqlConnection conn = DbMan.Open();
                SqlCommand cmd = new SqlCommand("procMarkNotificationsRead", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", uid);
                cmd.ExecuteNonQuery();
                DbMan.Close();
            }
            catch { DbMan.Close(); }
            Response.Write("{\"result\":\"ok\"}");
            Response.End();
            return;
        }

        if (action == "markOne")
        {
            int notifID = 0;
            int.TryParse(Request.QueryString["notifID"], out notifID);
            if (notifID > 0)
            {
                try
                {
                    SqlConnection conn = DbMan.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Notifications SET IsRead=1 WHERE NotifID=@NID AND UserID=@UID", conn);
                    cmd.Parameters.AddWithValue("@NID", notifID);
                    cmd.Parameters.AddWithValue("@UID", uid);
                    cmd.ExecuteNonQuery();
                    DbMan.Close();
                }
                catch { DbMan.Close(); }
            }
            Response.Write("{\"result\":\"ok\"}");
            Response.End();
            return;
        }

        if (action == "delete")
        {
            int notifID = 0;
            int.TryParse(Request.QueryString["notifID"], out notifID);
            if (notifID > 0)
            {
                try
                {
                    SqlConnection conn = DbMan.Open();
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Notifications WHERE NotifID=@NID AND UserID=@UID", conn);
                    cmd.Parameters.AddWithValue("@NID", notifID);
                    cmd.Parameters.AddWithValue("@UID", uid);
                    cmd.ExecuteNonQuery();
                    DbMan.Close();
                }
                catch { DbMan.Close(); }
            }
            Response.Write("{\"result\":\"ok\"}");
            Response.End();
            return;
        }

        // getList (기본)
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

        try
        {
            DataSet ds = DbMan.DataAdapterFill(sql, "Notif");
            DataTable dt = ds.Tables[0];
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                DataRow dr = dt.Rows[i];
                string title     = dr["BbsTitle"].ToString().Replace("\"", "\\\"");
                string actorNick = dr["ActorNick"].ToString().Replace("\"", "\\\"");
                string type      = dr["Type"].ToString();
                bool   isRead    = dr["IsRead"] != DBNull.Value && Convert.ToBoolean(dr["IsRead"]);
                sb.AppendFormat(
                    "{{\"notifID\":{0},\"type\":\"{1}\",\"bbsNo\":{2},\"bbsTitle\":\"{3}\",\"actorNick\":\"{4}\",\"isRead\":{5}}}",
                    dr["NotifID"], type, dr["BbsNo"], title, actorNick, isRead.ToString().ToLower());
            }
            sb.Append("]");
            Response.Write(sb.ToString());
        }
        catch { DbMan.Close(); Response.Write("[]"); }

        Response.End();
    }
}
