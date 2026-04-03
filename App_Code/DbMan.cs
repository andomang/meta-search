using System.Data;
using System.Data.SqlClient;

/// <summary>
/// 데이터베이스 연결 및 실행과 관련한 동작 모음
/// 객체를 생성하지 않고 이용하기 위해 static 멤버변수 및 메서드로 구성
/// </summary>
public class DbMan
{
    //모듈변수 선언
    //데이터베이스 관련 정보
    static string dataSource = @"Server=(local)\SQLEXPRESS; uid=meta; pwd=metapass; database=meta";

    //SqlConnection 객체
    static SqlConnection myConnection;

    //DB 연결 
    //입력 : 데이터베이스 정보, 출력 : SqlConnection 객체 --> 저장 프로시저를 이용 시 SqlConnection 객체 필요
    public static SqlConnection Open()
    {
        myConnection = new SqlConnection(dataSource);
        myConnection.Open();
        return myConnection;
    }

    //DB 닫기
    //입력 : 없음, 축력 : 없음
    public static void Close()
    {
        myConnection.Close();
    }

    //결과없는 쿼리문 실행
    //입력 : 경과 없는 쿼리문, 출력 : 없음
    public static void ExecuteNonQuery(string strSQL)
    {
        //명령문 구성(입력으로 쿼리문과 SqlConnection 객체 필요) 및 실행 
        SqlCommand myCommnad =
            new SqlCommand(strSQL, Open());
        myCommnad.ExecuteNonQuery();
    }

    //결과없는 쿼리문 실행 --> 저장프로시저 이용, 메서드 오버로딩
    //입력 : 저장프로시저 실행 SqlCommand 객체, 출력 : 없음
    public static void ExecuteNonQuery(SqlCommand myCommand)
    {
        //명령문 구성 및 실행 
        myCommand.ExecuteNonQuery();

    }

    //정수 반환 쿼리문 실행 --> 저장프로시저의 OUTPUT 파라미타 이용
    //입력 : 저장프로시저 실행 SqlCommand 객체, SqlParameter 객체
    //출력 : int
    public static int ExecuteStoredProcedure(SqlCommand myCommand, SqlParameter ParamOut)
    {
        myCommand.ExecuteNonQuery();
        //결과 반환
        int returnValue = (int)ParamOut.Value;
        Close();

        return returnValue;
    }

    //정수 반환 쿼리문 실행 --> 저장프로시저를 ExecuteScalar() 메서드 이용
    //입력 : 저장프로시저 실행 SqlCommand 객체
    //출력 : int
    public static int ExecuteScalar(SqlCommand myCommand)
    {
        //명령문 실행 및 결과 리턴

        return 0;
    }

    //문자열 반환 쿼리문 실행 --> 저장프로시저의 OUTPUT 파라미타 이용
    //입력 : 저장프로시저 실행 SqlCommand 객체, SqlParameter 객체
    //출력 : string
    public static string ExecuteStoredProcedureStr(SqlCommand myCommand, SqlParameter ParamOut)
    {
        //명령문 실행 

        return "";
    }

    //SQLDataReader 객체 반환 쿼리문 실행
    //입력 : SELECT 쿼리문, 출력 : SqlDataReader 객체
    public static SqlDataReader ExecuteReader(string strSql)
    {
        //명령문 구성(입력으로 쿼리문과 SqlConnection 객체 필요) 및 실행
        SqlCommand myCommand =
            new SqlCommand(strSql, Open());
        //SqlDataReader 방식으로 자료를 가져갈 때에는 연결을 유지해야 함
        return myCommand.ExecuteReader();
    }

    //DataSet 객체 반환 쿼리문 실행
    //입력 : SELECT 쿼리문, DataSet 객체의 테이블 이름, 출력 : DataSet 객체
    public static DataSet DataAdapterFill(string strSql, string mytable)
    {
        DataSet myDs = new DataSet(mytable);
        //명령문 구성 및 실행
        SqlDataAdapter myAdapter = new SqlDataAdapter(strSql, Open());
        myAdapter.Fill(myDs, mytable);
        myAdapter.Dispose();
        Close();

        //결과 반환
        return myDs;
    }
}