using System;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// 검색 기록 및 검색 결과 캐싱 관련 DB 작업을 담당하는 DAO(Data Access Object) 클래스.
/// [DAO 레이어] - 검색과 관련된 통계·이력·캐시 데이터를 DB와 주고받는 역할을 한다.
///
/// 대상 테이블:
///   - SearchHistory      : 유저가 입력한 검색어와 AI 분류 키워드, 검색 시각을 저장
///   - SearchResultCache  : 인기 검색어 TOP 10의 검색 결과를 캐시로 저장
///
/// 주요 기능:
///   - 검색어 저장 (AddSearchHistory) - 저장 프로시저 procAddSearchHistory 사용
///   - 최근 검색어 조회 (GetRecentSearches) - 중복 제거, 최신순
///   - 특정 검색어 삭제 (DeleteSearchHistory)
///   - 전체/개인 인기 검색어 조회 (GetTopSearches, GetUserTopSearches)
///   - 통계 조회 (GetTotalSearchCount, GetTopKeyword)
///   - 캐시 조회/저장 (GetCachedResults, SaveResultCache)
///
/// SearchResults.aspx, MyPage.aspx, Settings.aspx 등에서 사용된다.
/// </summary>
public class SearchDao
{
    /// <summary>
    /// 로그인한 유저의 검색 기록을 SearchHistory 테이블에 저장한다.
    /// 검색이 실행될 때마다 호출되며, 저장 프로시저 procAddSearchHistory를 사용한다.
    /// </summary>
    /// <param name="userID">검색을 수행한 회원 아이디</param>
    /// <param name="query">검색어 문자열 (예: "서울 맛집")</param>
    /// <param name="keyword">ClaudeApi.Classify()가 반환한 키워드 (예: "국내맛집")</param>
    /// <returns>1 = 저장 성공, -1 = 오류 발생</returns>
    public int AddSearchHistory(string userID, string query, string keyword)
    {
        // 결과 초기값 0
        int result = 0;
        try
        {
            // DB 연결 열기
            SqlConnection conn = DbMan.Open();
            // 직접 INSERT 쿼리 사용 (SP 의존 제거 — 컬럼명 Category→Keyword 변경 반영)
            SqlCommand cmd = new SqlCommand(
                "INSERT INTO SearchHistory(UserID, Query, Keyword, SearchTime) VALUES(@UserID, @Query, @Keyword, GETDATE())",
                conn);
            // 유저 아이디 파라미터 (Char 15자)
            cmd.Parameters.Add(new SqlParameter("@UserID",  SqlDbType.Char,     15)).Value = userID;
            // 검색어 파라미터 (유니코드 최대 200자)
            cmd.Parameters.Add(new SqlParameter("@Query",   SqlDbType.NVarChar, 200)).Value = query;
            // AI 분류 키워드 파라미터 (유니코드 최대 50자)
            cmd.Parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar, 50)).Value = keyword;
            // INSERT 쿼리 실행
            cmd.ExecuteNonQuery();
            // 성공 시 1 반환
            result = 1;
        }
        catch
        {
            // 예외 발생 시 -1 반환 (서비스 중단 방지)
            result = -1;
        }
        finally
        {
            // 성공/실패 무관하게 DB 연결 닫기
            DbMan.Close();
        }
        return result;
    }

    /// <summary>
    /// 특정 유저의 최근 검색어 목록을 중복 없이 최신순으로 조회한다.
    /// 같은 검색어가 여러 번 있으면 가장 최근 시각 기준으로 1개만 표시한다(GROUP BY).
    /// 검색창 드롭다운 히스토리와 MyPage.aspx "최근 검색어" 카드에 사용된다.
    /// </summary>
    /// <param name="userID">검색 기록을 조회할 회원 아이디</param>
    /// <param name="top">반환할 최대 검색어 수 (기본값: 10)</param>
    /// <returns>Query(검색어), SearchTime(가장 최근 검색 시각) 컬럼을 가진 DataTable</returns>
    public DataTable GetRecentSearches(string userID, int top = 10)
    {
        // GROUP BY Query: 같은 검색어를 하나로 묶어 중복 제거
        // MAX(SearchTime): 같은 검색어 중 가장 최근 시각을 대표값으로 선택
        // ORDER BY MAX(SearchTime) DESC: 가장 최근에 검색한 순서로 정렬
        string sql = string.Format(@"
            SELECT TOP {0} Query, MAX(SearchTime) AS SearchTime
            FROM SearchHistory
            WHERE UserID = '{1}'
            GROUP BY Query
            ORDER BY MAX(SearchTime) DESC", top, userID);
        // DataAdapterFill로 결과를 DataSet에 담고 첫 번째 테이블 반환
        DataSet ds = DbMan.DataAdapterFill(sql, "RecentSearches");
        return ds.Tables[0];
    }

    /// <summary>
    /// 특정 유저의 특정 검색어 기록을 SearchHistory 테이블에서 모두 삭제한다.
    /// 검색 드롭다운의 X(삭제) 버튼을 클릭할 때 호출된다.
    /// SQL Injection(악의적 SQL 삽입) 방지를 위해 단일 따옴표(')를 이중('')으로 이스케이프한다.
    /// </summary>
    /// <param name="userID">삭제할 기록의 소유자 아이디</param>
    /// <param name="query">삭제할 검색어 문자열</param>
    /// <returns>1 = 삭제 성공, -1 = 오류 발생</returns>
    public int DeleteSearchHistory(string userID, string query)
    {
        // 결과 초기값 0
        int result = 0;
        try
        {
            // 해당 유저의 특정 검색어를 모두 DELETE
            // query.Replace("'", "''"): 검색어 안에 작은따옴표가 있을 경우 SQL 오류 방지
            string sql = string.Format(@"
                DELETE FROM SearchHistory
                WHERE UserID = '{0}' AND Query = N'{1}'",
                userID, query.Replace("'", "''"));
            // 결과 없는 DELETE 쿼리 실행
            DbMan.ExecuteNonQuery(sql);
            // ExecuteNonQuery 내부에서 Open()을 호출하므로 별도로 Close() 필요
            DbMan.Close();
            // 성공 시 1 반환
            result = 1;
        }
        catch
        {
            // 예외 발생 시 -1 반환
            result = -1;
        }
        return result;
    }

    /// <summary>
    /// 전체 유저를 기준으로 가장 많이 검색된 검색어 TOP N개를 반환한다.
    /// 모든 유저의 SearchHistory를 집계하여 검색 횟수(COUNT) 기준 내림차순 정렬한다.
    /// SearchResults.aspx 사이드바의 "인기 검색어" 섹션에 표시된다.
    /// </summary>
    /// <param name="top">반환할 최대 검색어 수 (기본값: 10)</param>
    /// <returns>Query(검색어), SearchCount(총 검색 횟수) 컬럼을 가진 DataTable</returns>
    public DataTable GetTopSearches(int top = 10)
    {
        // 유저 구분 없이 전체 SearchHistory를 Query 기준으로 집계
        // COUNT(*): 각 검색어가 몇 번 검색되었는지 집계
        // ORDER BY SearchCount DESC: 검색 횟수 많은 순으로 정렬
        string sql = string.Format(@"
            SELECT TOP {0} Query, COUNT(*) AS SearchCount
            FROM SearchHistory
            GROUP BY Query
            ORDER BY SearchCount DESC", top);
        DataSet ds = DbMan.DataAdapterFill(sql, "TopSearches");
        return ds.Tables[0];
    }

    /// <summary>
    /// 특정 유저가 가장 많이 검색한 검색어 TOP N개를 반환한다.
    /// WHERE UserID 조건으로 해당 유저의 기록만 집계한다.
    /// MyPage.aspx "최근 검색어" 카드 또는 통계 섹션에 사용된다.
    /// </summary>
    /// <param name="userID">검색어를 조회할 회원 아이디</param>
    /// <param name="top">반환할 최대 검색어 수 (기본값: 5)</param>
    /// <returns>Query(검색어), SearchCount(해당 유저의 검색 횟수) 컬럼을 가진 DataTable</returns>
    public DataTable GetUserTopSearches(string userID, int top = 5)
    {
        // WHERE UserID = '{1}': 특정 유저의 기록만 필터링
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
    /// 특정 유저의 전체 검색 횟수(SearchHistory 행 수)를 반환한다.
    /// MyPage.aspx 및 Settings.aspx의 "총 검색 횟수" 통계 카드에 사용된다.
    /// </summary>
    /// <param name="userID">검색 횟수를 집계할 회원 아이디</param>
    /// <returns>해당 유저의 총 검색 횟수 (정수), 오류 발생 시 0 반환</returns>
    public int GetTotalSearchCount(string userID)
    {
        try
        {
            // COUNT(*): 해당 유저의 SearchHistory 행 수 = 총 검색 횟수
            SqlDataReader r = DbMan.ExecuteReader(string.Format(
                "SELECT COUNT(*) FROM SearchHistory WHERE UserID='{0}'", userID));
            // Read()로 결과 행을 읽은 뒤 첫 번째 컬럼(r[0])을 int로 변환
            int count = r.Read() ? Convert.ToInt32(r[0]) : 0;
            // Reader 닫기
            r.Close();
            // DB 연결 닫기
            DbMan.Close();
            return count;
        }
        catch
        {
            // 예외 발생 시 연결 닫고 0 반환
            DbMan.Close();
            return 0;
        }
    }

    /// <summary>
    /// 특정 유저가 가장 많이 검색한 키워드 1개를 반환한다.
    /// MyPage.aspx의 "최다 검색어" 통계 카드에 표시된다.
    /// </summary>
    /// <param name="userID">최다 검색어를 조회할 회원 아이디</param>
    /// <returns>가장 많이 검색한 검색어 문자열, 기록 없거나 오류 시 "-" 반환</returns>
    public string GetTopKeyword(string userID)
    {
        try
        {
            // TOP 1: 상위 1개만 가져옴
            // GROUP BY Query ORDER BY COUNT(*) DESC: 검색 횟수가 가장 많은 검색어 1개 선택
            SqlDataReader r = DbMan.ExecuteReader(string.Format(@"
                SELECT TOP 1 Query FROM SearchHistory
                WHERE UserID='{0}'
                GROUP BY Query ORDER BY COUNT(*) DESC", userID));
            // 결과가 있으면 검색어를 읽고, 없으면 "-" 반환
            string keyword = r.Read() ? r[0].ToString() : "-";
            // Reader 닫기
            r.Close();
            // DB 연결 닫기
            DbMan.Close();
            return keyword;
        }
        catch
        {
            // 예외 발생 시 연결 닫고 "-" 반환
            DbMan.Close();
            return "-";
        }
    }

    /// <summary>
    /// 특정 유저의 검색 기록 중 AI 가 가장 많이 분류한 키워드 1개를 반환한다.
    /// Keyword 컬럼(AI 분류값, 예: "뉴스", "쇼핑")을 기준으로 집계한다.
    /// Settings.aspx 개인정보 탭의 "최다 키워드" 카드에 사용된다.
    /// </summary>
    /// <param name="userID">조회할 회원 아이디</param>
    /// <returns>가장 많이 분류된 AI 키워드 문자열, 없거나 오류 시 "-" 반환</returns>
    public string GetTopAIKeyword(string userID)
    {
        try
        {
            // TOP 1: Keyword 컬럼 기준 최다 분류 키워드 1개 선택
            SqlDataReader r = DbMan.ExecuteReader(string.Format(@"
                SELECT TOP 1 Keyword FROM SearchHistory
                WHERE UserID='{0}' AND Keyword IS NOT NULL AND Keyword <> ''
                GROUP BY Keyword ORDER BY COUNT(*) DESC", userID));
            string keyword = r.Read() ? r[0].ToString() : "-";
            r.Close();
            DbMan.Close();
            return keyword;
        }
        catch
        {
            DbMan.Close();
            return "-";
        }
    }

    /// <summary>
    /// SearchResultCache 테이블에서 특정 검색어에 대해 캐싱된 결과를 조회한다.
    /// 인기 검색어 TOP 10에 해당하는 검색 결과를 미리 저장해두어 크롤링 시간을 단축한다.
    /// </summary>
    /// <param name="query">캐시를 조회할 검색어</param>
    /// <returns>Title, Url, Description, Source 컬럼을 가진 DataTable (없으면 빈 테이블)</returns>
    public DataTable GetCachedResults(string query)
    {
        // SQL 인젝션 방지를 위해 작은따옴표 이스케이프
        string q = query.Replace("'", "''");
        // 7일 이내 캐시만 유효 (오래된 캐시는 캐시 미스로 처리)
        string sql = string.Format(@"
            SELECT Title, Url, Description, Source
            FROM SearchResultCache
            WHERE Query = N'{0}' AND CachedAt > DATEADD(DAY, -7, GETDATE())
            ORDER BY CacheID", q);
        DataSet ds = DbMan.DataAdapterFill(sql, "Cache");
        return ds.Tables[0];
    }

    /// <summary>
    /// SearchResultCache 테이블에 검색 결과를 캐시로 저장한다.
    /// 기존 캐시가 있으면 삭제 후 새로 저장한다 (새로고침).
    /// 인기 검색어 TOP 10 진입 시 해당 검색어의 결과를 미리 저장해두는 데 사용된다.
    /// </summary>
    /// <param name="query">캐시할 검색어</param>
    /// <param name="results">저장할 검색 결과 DataTable (Title, Url, Description, Source 컬럼 필요)</param>
    public void SaveResultCache(string query, System.Data.DataTable results)
    {
        string q = query.Replace("'", "''");
        try
        {
            // 기존 캐시 먼저 삭제 (동일 검색어 중복 방지)
            DbMan.ExecuteNonQuery(string.Format("DELETE FROM SearchResultCache WHERE Query=N'{0}'", q));
            DbMan.Close();

            // 각 결과 행을 SearchResultCache 테이블에 INSERT
            foreach (System.Data.DataRow row in results.Rows)
            {
                string title = row["Title"].ToString().Replace("'", "''");
                string url   = row["Url"].ToString().Replace("'", "''");
                string desc  = row["Description"].ToString().Replace("'", "''");
                string src   = row["Source"].ToString().Replace("'", "''");

                // CachedAt: 캐시 저장 시각 (GETDATE()로 현재 시각 자동 기록)
                string sql = string.Format(@"
                    INSERT INTO SearchResultCache(Query, Title, Url, Description, Source, CachedAt)
                    VALUES(N'{0}', N'{1}', N'{2}', N'{3}', N'{4}', GETDATE())",
                    q, title, url, desc, src);
                DbMan.ExecuteNonQuery(sql);
                DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
    }
}
