using System.Data;
using System.Data.SqlClient;

/// <summary>
/// 데이터베이스 연결 및 쿼리 실행을 전담하는 유틸리티(Utility) 클래스.
/// [유틸 레이어] - 직접 화면(UI)이나 비즈니스 로직과 관계없이,
/// DB와 통신하는 가장 기본적인 기능만 모아 두었다.
///
/// 핵심 역할:
///   - SQL Server에 연결(Open)하고 닫기(Close)
///   - INSERT/UPDATE/DELETE 같이 결과가 없는 쿼리 실행 (ExecuteNonQuery)
///   - SELECT 결과를 SqlDataReader 또는 DataSet 형태로 반환
///   - 저장 프로시저(Stored Procedure)의 OUTPUT 파라미터 처리
///
/// 모든 멤버가 static(정적)이므로 new DbMan() 없이 DbMan.Open() 처럼 바로 사용한다.
/// MemberDao, SearchDao 등 모든 DAO 클래스가 이 클래스를 통해 DB 작업을 수행한다.
/// </summary>
public class DbMan
{
    // ──────────────────────────────────────────────
    // 모듈(클래스) 수준 변수 선언
    // ──────────────────────────────────────────────

    // 데이터베이스 연결 문자열 (Connection String)
    // 로컬 SQL Server Express 인스턴스에 meta 계정으로 접속
    // (주석 처리된 윗줄은 외부 호스팅 서버용 연결 문자열 - 배포 시 교체하여 사용)
    //static string dataSource = @"Data Source=db49734.databaseasp.net;Initial Catalog=db49734;User ID=db49734;Password=d1k2s3123?";
    static string dataSource = @"Server=(local)\SQLEXPRESS; uid=meta; pwd=metapass; database=meta";

    // 앱 전체에서 공유하는 SqlConnection 객체 (static이므로 한 개만 존재)
    // 연결을 열 때 Open()이 새로 생성하고, Close()가 닫는다.
    static SqlConnection myConnection;

    /// <summary>
    /// SQL Server에 연결을 열고 SqlConnection 객체를 반환한다.
    /// 저장 프로시저를 직접 호출해야 하는 DAO 메서드에서는
    /// 이 메서드가 반환한 SqlConnection을 SqlCommand 생성자에 넘긴다.
    /// </summary>
    /// <returns>열린(Open) 상태의 SqlConnection 객체</returns>
    public static SqlConnection Open()
    {
        // dataSource 문자열을 이용해 SqlConnection 객체 생성
        myConnection = new SqlConnection(dataSource);
        // 실제 네트워크 연결을 열기
        myConnection.Open();
        // 연결된 객체를 호출자에게 반환 (저장 프로시저 등에서 사용)
        return myConnection;
    }

    /// <summary>
    /// 열려 있는 DB 연결을 닫는다.
    /// SqlDataReader를 다 읽은 뒤, 또는 쿼리 실행 후 반드시 호출해야
    /// 연결 풀(Connection Pool) 자원 낭비를 막을 수 있다.
    /// </summary>
    public static void Close()
    {
        // 현재 열려 있는 연결 객체를 닫음
        myConnection.Close();
    }

    /// <summary>
    /// 반환 결과가 없는 SQL 쿼리(INSERT, UPDATE, DELETE)를 실행한다.
    /// 단순 문자열 쿼리를 받아 내부에서 연결을 열고 실행까지 처리한다.
    /// </summary>
    /// <param name="strSQL">실행할 SQL 쿼리 문자열 (예: "DELETE FROM ...")</param>
    public static void ExecuteNonQuery(string strSQL)
    {
        // SqlCommand 객체 생성: 쿼리 문자열과 열린 연결을 함께 전달
        SqlCommand myCommnad =
            new SqlCommand(strSQL, Open());
        // 쿼리 실행 (결과 행 수는 무시)
        myCommnad.ExecuteNonQuery();
    }

    /// <summary>
    /// 반환 결과가 없는 쿼리를 실행한다. (메서드 오버로딩 - 저장 프로시저 전용)
    /// 저장 프로시저 이름과 파라미터가 이미 설정된 SqlCommand 객체를 받아 실행한다.
    /// OUTPUT 파라미터가 필요 없는 저장 프로시저 호출 시 사용.
    /// </summary>
    /// <param name="myCommand">CommandType이 StoredProcedure로 설정된 SqlCommand 객체</param>
    public static void ExecuteNonQuery(SqlCommand myCommand)
    {
        // 이미 구성된 SqlCommand 객체로 저장 프로시저 실행
        myCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// 저장 프로시저를 실행하고 OUTPUT 파라미터에서 정수(int) 결과를 반환한다.
    /// 예: procAddMember가 @result OUTPUT으로 성공/실패 코드를 돌려줄 때 사용.
    /// </summary>
    /// <param name="myCommand">실행할 저장 프로시저 SqlCommand 객체</param>
    /// <param name="ParamOut">Direction이 Output으로 설정된 SqlParameter 객체</param>
    /// <returns>저장 프로시저의 OUTPUT 파라미터 값 (int)</returns>
    public static int ExecuteStoredProcedure(SqlCommand myCommand, SqlParameter ParamOut)
    {
        // 저장 프로시저 실행 (OUTPUT 파라미터 값이 채워짐)
        myCommand.ExecuteNonQuery();
        // OUTPUT 파라미터 값을 int로 캐스팅하여 로컬 변수에 저장
        int returnValue = (int)ParamOut.Value;
        // DB 연결 닫기
        Close();
        // 결과 정수값 반환
        return returnValue;
    }

    /// <summary>
    /// 저장 프로시저를 ExecuteScalar() 방식으로 실행하고 정수를 반환한다.
    /// (현재는 기본값 0만 반환하는 미구현 상태 - 필요 시 로직 추가)
    /// </summary>
    /// <param name="myCommand">실행할 저장 프로시저 SqlCommand 객체</param>
    /// <returns>쿼리 첫 번째 행 첫 번째 열 값 (현재 항상 0 반환)</returns>
    public static int ExecuteScalar(SqlCommand myCommand)
    {
        // 명령문 실행 및 결과 리턴 (미구현)
        return 0;
    }

    /// <summary>
    /// 저장 프로시저를 실행하고 OUTPUT 파라미터에서 문자열(string) 결과를 반환한다.
    /// (현재는 빈 문자열만 반환하는 미구현 상태 - 필요 시 로직 추가)
    /// </summary>
    /// <param name="myCommand">실행할 저장 프로시저 SqlCommand 객체</param>
    /// <param name="ParamOut">Direction이 Output으로 설정된 SqlParameter 객체</param>
    /// <returns>저장 프로시저의 OUTPUT 파라미터 문자열 값 (현재 항상 "" 반환)</returns>
    public static string ExecuteStoredProcedureStr(SqlCommand myCommand, SqlParameter ParamOut)
    {
        // 명령문 실행 (미구현)
        return "";
    }

    /// <summary>
    /// SELECT 쿼리를 실행하고 결과를 SqlDataReader로 반환한다.
    /// SqlDataReader는 한 줄씩 순차적으로 읽는 방식(커서 방식)이므로
    /// 사용 후 반드시 reader.Close()와 DbMan.Close()를 호출해야 한다.
    /// </summary>
    /// <param name="strSql">실행할 SELECT 쿼리 문자열</param>
    /// <returns>결과를 담고 있는 SqlDataReader 객체 (연결 유지 상태)</returns>
    public static SqlDataReader ExecuteReader(string strSql)
    {
        // SqlCommand 생성: SELECT 쿼리 문자열과 열린 연결 전달
        SqlCommand myCommand =
            new SqlCommand(strSql, Open());
        // SqlDataReader 방식은 연결을 유지한 상태에서 읽으므로 Close를 여기서 하지 않음
        // (호출한 쪽에서 reader.Close() → DbMan.Close() 순서로 닫아야 함)
        return myCommand.ExecuteReader();
    }

    /// <summary>
    /// SELECT 쿼리를 실행하고 결과를 DataSet으로 반환한다.
    /// SqlDataAdapter가 데이터를 메모리에 모두 올린 후 연결을 자동으로 닫기 때문에
    /// SqlDataReader보다 사용이 간편하다. GridView 바인딩 등에 주로 활용된다.
    /// </summary>
    /// <param name="strSql">실행할 SELECT 쿼리 문자열</param>
    /// <param name="mytable">DataSet 내에서 테이블에 붙일 이름 (예: "Members")</param>
    /// <returns>쿼리 결과가 담긴 DataSet 객체</returns>
    public static DataSet DataAdapterFill(string strSql, string mytable)
    {
        // 지정한 테이블 이름으로 빈 DataSet 생성
        DataSet myDs = new DataSet(mytable);
        // SqlDataAdapter 생성: SELECT 쿼리와 열린 연결 전달
        SqlDataAdapter myAdapter = new SqlDataAdapter(strSql, Open());
        // 쿼리 결과를 DataSet의 지정 테이블에 채움 (Fill)
        myAdapter.Fill(myDs, mytable);
        // SqlDataAdapter 자원 해제 (메모리 누수 방지)
        myAdapter.Dispose();
        // DB 연결 닫기 (DataAdapter는 연결을 자동으로 닫아주지만 명시적으로 처리)
        Close();
        // 결과 DataSet 반환
        return myDs;
    }
}
