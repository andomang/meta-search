<%@ Application Language="C#" %>
<%@ Import Namespace="System.Threading" %>
<script runat="server">
    // 7일 주기 캐시·기록 자동 삭제 타이머
    private static System.Threading.Timer _cleanupTimer;

    void Application_Start(object sender, EventArgs e)
    {
        // 앱 시작 1분 후 첫 실행, 이후 7일 간격
        _cleanupTimer = new System.Threading.Timer(RunCleanup, null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromDays(7));
    }

    void Application_End(object sender, EventArgs e)
    {
        if (_cleanupTimer != null) _cleanupTimer.Dispose();
    }

    private static void RunCleanup(object state)
    {
        try
        {
            DbMan.ExecuteNonQuery(
                "DELETE FROM SearchResultCache WHERE CachedAt < DATEADD(DAY, -7, GETDATE())");
            DbMan.Close();
        }
        catch { try { DbMan.Close(); } catch { } }

        try
        {
            DbMan.ExecuteNonQuery(
                "DELETE FROM SearchHistory WHERE SearchTime < DATEADD(DAY, -7, GETDATE())");
            DbMan.Close();
        }
        catch { try { DbMan.Close(); } catch { } }
    }
</script>
