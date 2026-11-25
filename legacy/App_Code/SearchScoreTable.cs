using System.Collections.Generic;

/// <summary>
/// 키워드별 검색 엔진 우선순위 점수를 정의하는 정적 클래스.
/// GetScores(keyword)를 호출하면 [구글점수, 네이버점수, 다음점수] 배열을 반환한다.
/// 배열 인덱스: 0=구글, 1=네이버, 2=다음
/// 점수가 높을수록 해당 엔진의 결과를 상위에 더 많이 배치한다.
/// </summary>
public static class SearchScoreTable
{
    // 키워드 → [구글점수, 네이버점수, 다음점수] 매핑 딕셔너리
    // 컨펌된 점수표 (회의 결정사항):
    //   해외 계열(해외뉴스/맛집/장소, IT기술): 구글 3 > 네이버 2 > 다음 1
    //   국내 계열(국내뉴스/맛집/장소, 쇼핑):  네이버 3 > 다음 2 > 구글 1
    //   게임/차량/의료건강/스포츠:             네이버 3 > 구글 2 > 다음 1
    //   일반:                                  구글 1 = 네이버 1 = 다음 1 (균등)
    private static readonly Dictionary<string, int[]> _scores = new Dictionary<string, int[]>
    {
        // 해외 계열: 구글 최우선 (글로벌 콘텐츠 강점)
        { "해외뉴스", new[] { 3, 2, 1 } },
        { "해외맛집", new[] { 3, 2, 1 } },
        { "해외장소", new[] { 3, 2, 1 } },
        { "IT기술",   new[] { 3, 2, 1 } },

        // 국내 계열: 네이버 최우선 (국내 콘텐츠 강점)
        { "국내뉴스", new[] { 1, 3, 2 } },
        { "국내맛집", new[] { 1, 3, 2 } },
        { "국내장소", new[] { 1, 3, 2 } },
        { "쇼핑",     new[] { 1, 3, 2 } },

        // 전문 영역: 네이버 커뮤니티 강점, 구글 차순위
        { "게임",     new[] { 2, 3, 1 } },
        { "차량",     new[] { 2, 3, 1 } },
        { "의료건강", new[] { 2, 3, 1 } },
        { "스포츠",   new[] { 2, 3, 1 } },

        // 일반: 세 엔진 균등 배치
        { "일반",     new[] { 1, 1, 1 } },
    };

    /// <summary>
    /// 키워드에 해당하는 [구글, 네이버, 다음] 점수 배열을 반환한다.
    /// 알 수 없는 키워드는 균등(1,1,1)을 반환하여 세 엔진을 동등하게 처리한다.
    /// </summary>
    public static int[] GetScores(string keyword)
    {
        // 키워드가 없거나 매핑에 없으면 기본값(균등) 반환
        if (!string.IsNullOrEmpty(keyword) && _scores.ContainsKey(keyword))
            return _scores[keyword];
        return new[] { 1, 1, 1 };
    }
}
