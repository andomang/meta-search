// App_Code/SearchResult.cs

/// <summary>
/// 검색 결과 항목 하나를 나타내는 데이터 전달 객체 (DO: Domain Object).
/// [도메인 레이어] - 네이버/다음/구글 등 각 검색 엔진에서 가져온
/// 검색 결과 1건의 정보를 담아 SearchResults.aspx로 전달하는 데 사용된다.
/// </summary>
public class SearchResult
{
    // 검색 결과의 제목 (예: "서울 맛집 추천 TOP 10 - 네이버 블로그")
    public string Title { get; set; }

    // 검색 결과가 가리키는 원본 페이지 URL
    public string Url { get; set; }

    // 검색 결과 요약 설명 (스니펫)
    public string Description { get; set; }

    // 결과를 제공한 검색 엔진 구분자 ("naver", "daum", "google")
    public string Source { get; set; }

    // 결과 페이지의 대표 이미지 URL (없으면 빈 문자열).
    // 현재 다음(카카오) API만 thumbnail 필드를 제공하므로 해당 결과에서만 값이 채워진다.
    public string ImageUrl { get; set; }

    // 관련도 점수 - 가중치 정렬 계산용
    public int Score { get; set; }
}
