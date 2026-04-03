using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

public partial class Settings : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserID"] == null) { Response.Redirect("Default.aspx"); return; }
        if (!IsPostBack) LoadUserData();
    }

    private void LoadUserData()
    {
        try
        {
            string sql = string.Format("SELECT Name, Email FROM members WHERE UserID = '{0}'", Session["UserID"]);
            using (SqlDataReader reader = DbMan.ExecuteReader(sql))
            {
                if (reader.Read())
                {
                    editName.Text = reader["Name"].ToString().Trim();
                    editEmail.Text = reader["Email"].ToString().Trim();
                }
                DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                string sql = "UPDATE members SET Name=@name, Email=@email WHERE UserID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", editName.Text.Trim());
                cmd.Parameters.AddWithValue("@email", editEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@id", Session["UserID"]);
                cmd.ExecuteNonQuery();
                DbMan.Close();
                Session["UserName"] = editName.Text.Trim();
                Response.Redirect("Settings.aspx");
            }
        }
        catch { DbMan.Close(); }
    }

    protected void btnToggleDark_Click(object sender, EventArgs e)
    {
        bool nextDark = !(Session["IsDark"] != null && (bool)Session["IsDark"]);
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                string sql = "UPDATE members SET DarkMode=@dark WHERE UserID=@id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@dark", nextDark ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", Session["UserID"]);
                cmd.ExecuteNonQuery();
                DbMan.Close();
                Session["IsDark"] = nextDark;
                Response.Redirect("Settings.aspx");
            }
        }
        catch { DbMan.Close(); }
    }
}