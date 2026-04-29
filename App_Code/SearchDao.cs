using System;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// 검색 기록 및 클릭 기록 관련 DB 작업 클래스
/// SearchHistory, SearchClickHistory 테이블을 대상으로
/// 저장/조회/삭제 기능을 저장 프로시저 방식으로 처리
/// </summary>
public class SearchDao
{
    /// <summary>
    /// 검색 기록 저장
    /// 로그인 유저가 검색할 때마다 SearchHistory 테이블에 INSERT
    /// 저장 프로시저: procAddSearchHistory
    /// </summary>
    public int AddSearchHistory(string userID, string query, string category)
    {
        int result = 0;
        try
        {
            SqlConnection conn = DbMan.Open();
            SqlCommand cmd = new SqlCommand("procAddSearchHistory", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@UserID",   SqlDbType.Char,     15)).Value = userID;
            cmd.Parameters.Add(new SqlParameter("@Query",    SqlDbType.NVarChar, 200)).Value = query;
            cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar, 50)).Value = category;
            cmd.ExecuteNonQuery();
            result = 1;
        }
        catch { result = -1; }
        finally { DbMan.Close(); }
        return result;
    }

    /// <summary>
    /// 검색 결과 클릭 기록 저장
    /// 사용자가 검색 결과 링크를 클릭할 때 SearchClickHistory 테이블에 INSERT
    /// 저장 프로시저: procAddSearchClick
    /// </summary>
    public int AddSearchClick(string userID, string query, string category, string clickedUrl, string clickedTitle)
    {
        int result = 0;
        try
        {
            SqlConnection conn = DbMan.Open();
            SqlCommand cmd = new SqlCommand("procAddSearchClick", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@UserID",       SqlDbType.Char,     15)).Value = userID;
            cmd.Parameters.Add(new SqlParameter("@Query",        SqlDbType.NVarChar, 200)).Value = query;
            cmd.Parameters.Add(new SqlParameter("@Category",     SqlDbType.NVarChar, 50)).Value = category;
            cmd.Parameters.Add(new SqlParameter("@ClickedUrl",   SqlDbType.NVarChar, 500)).Value = clickedUrl;
            cmd.Parameters.Add(new SqlParameter("@ClickedTitle", SqlDbType.NVarChar, 300)).Value = clickedTitle;
            cmd.ExecuteNonQuery();
            result = 1;
        }
        catch { result = -1; }
        finally { DbMan.Close(); }
        return result;
    }

    /// <summary>
    /// 최근 검색 기록 조회 (중복 제거, 최신순)
    /// 같은 검색어가 여러 번 있으면 가장 최근 시간 기준으로 1개만 반환
    /// 드롭다운 히스토리, 마이페이지에서 사용
    /// </summary>
    public DataTable GetRecentSearches(string userID, int top = 10)
    {
        string sql = string.Format(@"
            SELECT TOP {0} Query, MAX(SearchTime) AS SearchTime
            FROM SearchHistory
            WHERE UserID = '{1}'
            GROUP BY Query
            ORDER BY MAX(SearchTime) DESC", top, userID);
        DataSet ds = DbMan.DataAdapterFill(sql, "RecentSearches");
        return ds.Tables[0];
    }

    /// <summary>
    /// 특정 검색어 기록 삭제
    /// 드롭다운의 X 버튼 클릭 시 해당 검색어의 모든 기록 삭제
    /// SQL Injection 방지를 위해 단일 따옴표 이스케이프 처리
    /// </summary>
    public int DeleteSearchHistory(string userID, string query)
    {
        int result = 0;
        try
        {
            string sql = string.Format(@"
                DELETE FROM SearchHistory
                WHERE UserID = '{0}' AND Query = N'{1}'",
                userID, query.Replace("'", "''"));
            DbMan.ExecuteNonQuery(sql);
            DbMan.Close();
            result = 1;
        }
        catch { result = -1; }
        return result;
    }

    /// <summary>
    /// 전체 인기 검색어 TOP N 조회
    /// 모든 유저의 SearchHistory를 집계하여 검색 횟수 기준 내림차순 정렬
    /// SearchResults.aspx 사이드바 인기 검색어에 사용
    /// </summary>
    public DataTable GetTopSearches(int top = 10)
    {
        string sql = string.Format(@"
            SELECT TOP {0} Query, COUNT(*) AS SearchCount
            FROM SearchHistory
            GROUP BY Query
            ORDER BY SearchCount DESC", top);
        DataSet ds = DbMan.DataAdapterFill(sql, "TopSearches");
        return ds.Tables[0];
    }

    /// <summary>
    /// 특정 유저의 인기 검색어 TOP N 조회
    /// 마이페이지 최근 검색어 카드에 사용
    /// </summary>
    public DataTable GetUserTopSearches(string userID, int top = 5)
    {
        string sql = string.Format(@"
            SELECT TOP {0} Query, COUNT(*) AS SearchCount
            FROM SearchHistory
            WHERE UserID = '{1}'
            GROUP BY Query
            ORDER BY SearchCount DESC", top, userID);
        DataSet ds = DbMan.DataAdapterFill(sql, "UserTopSearches");
        return ds.Tables[0];
    }

    /// <summary>
    /// 유저의 총 검색 횟수 조회
    /// 마이페이지 통계 카드에 사용
    /// </summary>
    public int GetTotalSearchCount(string userID)
    {
        try
        {
            SqlDataReader r = DbMan.ExecuteReader(string.Format(
                "SELECT COUNT(*) FROM SearchHistory WHERE UserID='{0}'", userID));
            int count = r.Read() ? Convert.ToInt32(r[0]) : 0;
            r.Close();
            DbMan.Close();
            return count;
        }
        catch { DbMan.Close(); return 0; }
    }

    /// <summary>
    /// 유저의 총 클릭 횟수 조회
    /// 마이페이지 통계 카드에 사용
    /// </summary>
    public int GetTotalClickCount(string userID)
    {
        try
        {
            SqlDataReader r = DbMan.ExecuteReader(string.Format(
                "SELECT COUNT(*) FROM SearchClickHistory WHERE UserID='{0}'", userID));
            int count = r.Read() ? Convert.ToInt32(r[0]) : 0;
            r.Close();
            DbMan.Close();
            return count;
        }
        catch { DbMan.Close(); return 0; }
    }

    /// <summary>
    /// 유저의 최다 검색 키워드 1개 조회
    /// 마이페이지 통계 카드에 사용
    /// </summary>
    public string GetTopKeyword(string userID)
    {
        try
        {
            SqlDataReader r = DbMan.ExecuteReader(string.Format(@"
                SELECT TOP 1 Query FROM SearchHistory
                WHERE UserID='{0}'
                GROUP BY Query ORDER BY COUNT(*) DESC", userID));
            string keyword = r.Read() ? r[0].ToString() : "-";
            r.Close();
            DbMan.Close();
            return keyword;
        }
        catch { DbMan.Close(); return "-"; }
    }
}
