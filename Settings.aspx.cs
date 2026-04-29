using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

public partial class Settings : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserID"] == null) { Response.Redirect("Default.aspx"); return; }

        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action); return; }

        if (Request.Cookies["DarkMode"] != null)
        {
            string dmVal = Request.Cookies["DarkMode"].Value;
            Session["IsDark"] = (dmVal == "1" || dmVal == "on");
        }

        if (!IsPostBack) { LoadUserData(); LoadStats(); SetLangButtons(); }

        bool isDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        string on = "<span class='text-blue-500 font-bold uppercase'>" + Lang.Get("set.themeOn") + "</span>";
        string off = "<span class='text-gray-400 font-bold uppercase'>" + Lang.Get("set.themeOff") + "</span>";
        litThemeStatus.Text = isDark ? on : off;

        litProfilePhotoLabel.Text = Lang.Get("set.profilePhoto");
        litProfilePhotoSub.Text   = Lang.Get("set.profilePhotoSub");
        btnUploadPhoto.Text       = Lang.Get("set.photoBtn");

        // 현재 프로필 사진 또는 닉네임 첫글자 표시
        try
        {
            SqlDataReader rp = DbMan.ExecuteReader(
                string.Format("SELECT Nickname, ProfileImg FROM members WHERE userid='{0}'", Session["UserID"]));
            if (rp.Read())
            {
                string pImg = rp["ProfileImg"] != DBNull.Value ? rp["ProfileImg"].ToString() : "";
                string nick = rp["Nickname"].ToString().Trim();
                litCurrentAvatar.Text = !string.IsNullOrEmpty(pImg)
                    ? string.Format("<img src='uploads/{0}' class='w-full h-full object-cover' />", pImg)
                    : (nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?");
            }
            rp.Close(); DbMan.Close();
        }
        catch { DbMan.Close(); }

        litPageTitle.Text = Lang.Get("set.title");
        litTabGeneral.Text = Lang.Get("set.tabGeneral");
        litTabPrivacy.Text = Lang.Get("set.tabPrivacy");
        litThemeLabel.Text = Lang.Get("set.theme");
        litThemeSub.Text = Lang.Get("set.themeSub");
        btnToggleDark.Text = Lang.Get("set.themeBtn");
        litLangLabel.Text = Lang.Get("set.lang");
        litLangSub.Text = Lang.Get("set.langSub");
        litMyInfoLabel.Text = Lang.Get("set.myInfo");
        litNameLbl.Text = Lang.Get("set.name");
        litNickLbl.Text = Lang.Get("set.nick");
        litEmailLbl.Text = Lang.Get("set.email");
        litChangePwBtn.Text = Lang.Get("set.changePw");
        btnUpdate.Text = Lang.Get("set.updateBtn");
        litStatsLabel.Text = Lang.Get("set.stats");
        litTotalSearchLbl.Text = Lang.Get("set.totalSearch");
        litTotalClickLbl.Text = Lang.Get("set.totalClick");
        litTopCategoryLbl.Text = Lang.Get("set.topCategory");
        litTop5Lbl.Text = Lang.Get("set.top5");
        litDelSearchLabel.Text = Lang.Get("set.delSearch");
        litDelSearchSub.Text = Lang.Get("set.delSearchSub");
        litDelSearchBtn.Text = Lang.Get("set.delBtn");
        litDelClickLabel.Text = Lang.Get("set.delClick");
        litDelClickSub.Text = Lang.Get("set.delClickSub");
        litDelClickBtn.Text = Lang.Get("set.delBtn");
        litWithdrawLabel.Text = Lang.Get("set.withdraw");
        litWithdrawSub.Text = Lang.Get("set.withdrawSub");
        litWithdrawBtn.Text = Lang.Get("set.withdrawBtn");
        litDelModalDesc.Text = Lang.Get("set.cancel") == "Cancel" ? "Select period to delete." : "삭제할 기간을 선택하세요.";
        lit1Hour.Text = Lang.Get("set.delModal1h");
        lit12Hour.Text = Lang.Get("set.delModal12h");
        lit1Day.Text = Lang.Get("set.delModal1d");
        lit7Day.Text = Lang.Get("set.delModal7d");
        lit30Day.Text = Lang.Get("set.delModal30d");
        litDelAll.Text = Lang.Get("set.delModalAll");
        litDelCancel.Text = Lang.Get("set.cancel");
        litPwModalTitle.Text = Lang.Get("set.pwModal");
        litPwCurLbl.Text = Lang.Get("set.pwCur");
        litPwNewLbl.Text = Lang.Get("set.pwNew");
        litPwConfirmLbl.Text = Lang.Get("set.pwConfirm");
        litPwSubmitBtn.Text = Lang.Get("set.pwBtn");
        litPwCancelBtn.Text = Lang.Get("set.cancel");
        litWdModalTitle.Text = Lang.Get("set.wdModal");
        litWdDesc.Text = Lang.Get("set.wdDesc");
        litWdSubmitBtn.Text = Lang.Get("set.wdBtn");
        litWdCancelBtn.Text = Lang.Get("set.cancel");
    }

    private void HandleAjax(string action)
    {
        Response.ContentType = "application/json";
        string userID = Session["UserID"].ToString();
        try
        {
            if (action == "deleteHistory")
            {
                string target = Request.QueryString["target"];
                string range = Request.QueryString["range"];
                string table = target == "click" ? "SearchClickHistory" : "SearchHistory";
                string timeCol = target == "click" ? "ClickTime" : "SearchTime";
                string condition = GetDateCondition(range);
                string sql = condition != null
                    ? string.Format("DELETE FROM {0} WHERE UserID='{1}' AND {2} >= {3}", table, userID, timeCol, condition)
                    : string.Format("DELETE FROM {0} WHERE UserID='{1}'", table, userID);
                DbMan.ExecuteNonQuery(sql); DbMan.Close();
                Response.Write("{\"result\":\"ok\"}");
            }
            else if (action == "changePw")
            {
                string cur = Request.QueryString["cur"];
                string nw = Request.QueryString["nw"];
                string encCur = GetMD5(cur);
                string encNew = GetMD5(nw);
                SqlDataReader r = DbMan.ExecuteReader(string.Format(
                    "SELECT COUNT(*) FROM members WHERE userid='{0}' AND passwd='{1}'", userID, encCur));
                bool valid = r.Read() && Convert.ToInt32(r[0]) > 0;
                r.Close(); DbMan.Close();
                if (!valid) { Response.Write("{\"result\":\"wrong\"}"); }
                else
                {
                    DbMan.ExecuteNonQuery(string.Format(
                        "UPDATE members SET passwd='{0}' WHERE userid='{1}'", encNew, userID));
                    DbMan.Close();
                    Response.Write("{\"result\":\"ok\"}");
                }
            }
            else if (action == "withdraw")
            {
                string pw = Request.QueryString["pw"];
                string encPw = GetMD5(pw);
                SqlDataReader r = DbMan.ExecuteReader(string.Format(
                    "SELECT COUNT(*) FROM members WHERE userid='{0}' AND passwd='{1}'", userID, encPw));
                bool valid = r.Read() && Convert.ToInt32(r[0]) > 0;
                r.Close(); DbMan.Close();
                if (!valid) { Response.Write("{\"result\":\"wrong\"}"); }
                else
                {
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM SearchHistory WHERE UserID='{0}'", userID)); DbMan.Close();
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM SearchClickHistory WHERE UserID='{0}'", userID)); DbMan.Close();
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM Bbs WHERE Author='{0}'", userID)); DbMan.Close();
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM members WHERE userid='{0}'", userID)); DbMan.Close();
                    Session.Clear();
                    Response.Write("{\"result\":\"ok\"}");
                }
            }
        }
        catch { DbMan.Close(); Response.Write("{\"result\":\"error\"}"); }
        Response.End();
    }

    private string GetDateCondition(string range)
    {
        switch (range)
        {
            case "1hour": return "DATEADD(HOUR, -1, GETDATE())";
            case "12hour": return "DATEADD(HOUR, -12, GETDATE())";
            case "1day": return "DATEADD(DAY, -1, GETDATE())";
            case "7day": return "DATEADD(DAY, -7, GETDATE())";
            case "30day": return "DATEADD(DAY, -30, GETDATE())";
            default: return null;
        }
    }

    private void LoadUserData()
    {
        try
        {
            string sql = string.Format("SELECT Name, Nickname, Email, Language FROM members WHERE UserID='{0}'", Session["UserID"]);
            using (SqlDataReader reader = DbMan.ExecuteReader(sql))
            {
                if (reader.Read())
                {
                    editName.Text = reader["Name"].ToString().Trim();
                    editNickname.Text = reader["Nickname"].ToString().Trim();
                    editEmail.Text = reader["Email"].ToString().Trim();
                    Session["Lang"] = reader["Language"] != DBNull.Value ? reader["Language"].ToString().Trim() : "ko";
                }
                DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
    }

    private void LoadStats()
    {
        try
        {
            string uid = Session["UserID"].ToString();
            SqlDataReader r1 = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM SearchHistory WHERE UserID='{0}'", uid));
            litTotalSearch.Text = r1.Read() ? r1[0].ToString() : "0"; r1.Close(); DbMan.Close();
            SqlDataReader r2 = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM SearchClickHistory WHERE UserID='{0}'", uid));
            litTotalClick.Text = r2.Read() ? r2[0].ToString() : "0"; r2.Close(); DbMan.Close();
            SqlDataReader r3 = DbMan.ExecuteReader(string.Format(
                "SELECT TOP 1 Category FROM SearchHistory WHERE UserID='{0}' GROUP BY Category ORDER BY COUNT(*) DESC", uid));
            litTopCategory.Text = r3.Read() ? r3[0].ToString() : "-"; r3.Close(); DbMan.Close();
            DataSet ds = DbMan.DataAdapterFill(string.Format(
                "SELECT TOP 5 Query, COUNT(*) AS SearchCount FROM SearchHistory WHERE UserID='{0}' GROUP BY Query ORDER BY SearchCount DESC", uid), "Top");
            rptTopKeywords.DataSource = ds.Tables[0];
            rptTopKeywords.DataBind();
        }
        catch { DbMan.Close(); }
    }

    private void SetLangButtons()
    {
        string lang = Session["Lang"] != null ? Session["Lang"].ToString() : "ko";
        string activeClass = "px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer bg-blue-600 text-white border-blue-600";
        string inactClass = "px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer dark:bg-slate-700 dark:text-white dark:border-slate-600 border-gray-300 text-gray-700 hover:border-blue-400";
        btnLangKo.CssClass = lang == "ko" ? activeClass : inactClass;
        btnLangEn.CssClass = lang == "en" ? activeClass : inactClass;
    }

    protected void btnLangKo_Click(object sender, EventArgs e) { UpdateLang("ko"); }
    protected void btnLangEn_Click(object sender, EventArgs e) { UpdateLang("en"); }

    private void UpdateLang(string lang)
    {
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                SqlCommand cmd = new SqlCommand("UPDATE members SET [Language]=@lang WHERE userid=@id", conn);
                cmd.Parameters.AddWithValue("@lang", lang);
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery(); DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
        Session["Lang"] = lang;
        Response.Cookies["Language"].Value = lang;
        Response.Cookies["Language"].Expires = DateTime.Now.AddYears(1);
        Response.Redirect("Settings.aspx");
    }

    protected void btnToggleDark_Click(object sender, EventArgs e)
    {
        bool next = !(Session["IsDark"] != null && (bool)Session["IsDark"]);
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                SqlCommand cmd = new SqlCommand("UPDATE members SET DarkMode=@dark WHERE UserID=@id", conn);
                cmd.Parameters.AddWithValue("@dark", next ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery();
            }
        }
        catch { DbMan.Close(); }
        Session["IsDark"] = next;
        Response.Cookies["DarkMode"].Value = next ? "1" : "0";
        Response.Cookies["DarkMode"].Expires = DateTime.Now.AddYears(1);
        Response.Redirect("Settings.aspx");
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                SqlCommand cmd = new SqlCommand("UPDATE members SET Name=@name, Nickname=@nick, Email=@email WHERE UserID=@id", conn);
                cmd.Parameters.AddWithValue("@name", editName.Text.Trim());
                cmd.Parameters.AddWithValue("@nick", editNickname.Text.Trim());
                cmd.Parameters.AddWithValue("@email", editEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery(); DbMan.Close();
                Session["UserName"] = editNickname.Text.Trim();
                string msg = Lang.Get("set.cancel") == "Cancel" ? "Updated successfully." : "정보가 업데이트되었습니다.";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ok",
                    string.Format("alert('{0}'); location.href='Settings.aspx';", msg), true);
            }
        }
        catch { DbMan.Close(); }
    }

    protected void btnUploadPhoto_Click(object sender, EventArgs e)
    {
        if (!fuPhoto.HasFile) return;
        string ext = System.IO.Path.GetExtension(fuPhoto.FileName).ToLower();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "photoErr",
                string.Format("alert('{0}');", Lang.Get("set.photoError")), true);
            return;
        }
        string uid      = Session["UserID"].ToString();
        string fileName = uid.Trim() + ext;
        string savePath = Server.MapPath("~/uploads/") + fileName;
        if (!System.IO.Directory.Exists(Server.MapPath("~/uploads/")))
            System.IO.Directory.CreateDirectory(Server.MapPath("~/uploads/"));
        fuPhoto.SaveAs(savePath);
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                SqlCommand cmd = new SqlCommand("UPDATE members SET ProfileImg=@img WHERE userid=@id", conn);
                cmd.Parameters.AddWithValue("@img", fileName);
                cmd.Parameters.AddWithValue("@id",  uid);
                cmd.ExecuteNonQuery(); DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
        ScriptManager.RegisterStartupScript(this, GetType(), "photoOk",
            string.Format("alert('{0}'); location.href='Settings.aspx';", Lang.Get("set.photoUpdated")), true);
    }

    private string GetMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] b = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte x in b) sb.Append(x.ToString("X2"));
            return sb.ToString();
        }
    }
}