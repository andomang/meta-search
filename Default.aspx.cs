using System;
using System.Web.UI;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        litSubtitle.Text = Lang.Get("home.subtitle");
        litCommunityTitle.Text = Lang.Get("home.community");
        litCommunitySub.Text = Lang.Get("home.commSub");
        litSettingsTitle.Text = Lang.Get("home.settings");
        litSettingsSub.Text = Lang.Get("home.settSub");
        btnSearch.Text = Lang.Get("nav.community") == "Community" ? "Search" : "검색";
        txtSearch.Attributes["placeholder"] = Lang.Get("search.placeholder");
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string query = txtSearch.Text.Trim();
        if (!string.IsNullOrEmpty(query))
            Response.Redirect("SearchResults.aspx?q=" + Server.UrlEncode(query));
        else
            ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                "alert('" + (Lang.Get("search.placeholder")) + "');", true);
    }
}