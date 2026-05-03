// App_Code/SearchResult.cs

/// <summary>
/// 검색 결과 항목 하나를 나타내는 데이터 전달 객체 (DO: Domain Object).
/// [도메인 레이어] - 네이버/다음/구글 등 각 검색 엔진에서 가져온
/// 검색 결과 1건의 정보를 담아 SearchResults.aspx로 전달하는 데 사용된다.
///
/// 사용 흐름 예시:
///   1. SearchResults.aspx.cs에서 각 검색 API 결과를 파싱(분석)한다.
///   2. 파싱된 데이터를 SearchResult 객체에 담는다.
///   3. SearchResult 목록(List)을 ClaudeApi.RankResults()로 재정렬한 뒤
///      화면(Repeater/ListView 등)에 바인딩하여 출력한다.
/// </summary>
public class SearchResult
{
    // 검색 결과의 제목 (예: "서울 맛집 추천 TOP 10 - 네이버 블로그")
    /// <summary>
    /// 검색 결과 항목의 제목 텍스트.
    /// 검색 결과 목록에서 링크 텍스트로 표시된다.
    /// </summary>
    public string Title { get; set; }

    // 검색 결과가 가리키는 원본 페이지 URL
    /// <summary>
    /// 검색 결과 항목의 원본 페이지 URL.
    /// 사용자가 클릭하면 이 주소로 이동하며, AddSearchClick()에 기록된다.
    /// </summary>
    public string Url { get; set; }

    // 검색 결과 요약 설명 (스니펫, Snippet) - 제목 아래에 회색 글씨로 표시되는 본문 일부
    /// <summary>
    /// 검색 결과 항목의 요약 설명 (스니펫).
    /// 검색 엔진이 제공하는 페이지 본문 일부로, 제목 아래에 작은 글씨로 표시된다.
    /// </summary>
    public string Description { get; set; }

    // 결과를 제공한 검색 엔진 구분자 (출처 표시 및 필터링에 사용)
    // 허용값: "naver" (네이버), "daum" (다음), "google" (구글)
    /// <summary>
    /// 이 검색 결과를 제공한 검색 엔진 이름.
    /// 예: "naver", "daum", "google"
    /// 탭 필터(네이버만 보기 등) 구현 시 이 값으로 분류한다.
    /// </summary>
    public string Source { get; set; }   // "naver", "daum", "google"

    // 관련도 점수 (Score) - 클로드 AI 재정렬 또는 자체 점수제 계산에 활용
    // 높을수록 검색어와 관련성이 높다고 판단된 결과
    /// <summary>
    /// 검색 결과의 관련도 점수.
    /// ClaudeApi.RankResults()를 통해 AI가 관련도를 평가할 때 사용되는 값이며,
    /// 점수가 높을수록 검색어와 더 관련 있는 결과로 간주된다.
    /// </summary>
    public int Score { get; set; }       // 점수제 계산용
}
