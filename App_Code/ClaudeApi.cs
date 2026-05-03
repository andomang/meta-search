using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;

/// <summary>
/// Anthropic의 Claude AI API를 호출하는 유틸리티(Utility) 클래스.
/// [유틸 레이어] - 검색어 카테고리 분류와 검색 결과 재정렬 두 가지 AI 기능을 제공한다.
///
/// 사용하는 AI 모델: claude-haiku-4-5 (빠른 응답 속도 중시)
/// API 엔드포인트: https://api.anthropic.com/v1/messages
///
/// 주요 기능:
///   1. Classify(query)     : 검색어를 13개 카테고리 중 하나로 분류
///   2. RankResults(...)    : 검색 결과를 관련도 높은 순서로 재정렬
///   3. CallClaude(message) : 실제 HTTP 요청을 보내고 응답 텍스트를 반환 (내부 전용)
///
/// API 키는 web.config의 appSettings["ClaudeApiKey"]에서 읽어온다.
/// 모든 멤버가 static이므로 new ClaudeApi() 없이 ClaudeApi.Classify("서울 맛집") 방식으로 사용한다.
/// API 오류 발생 시 서비스가 중단되지 않도록 기본값("일반")을 반환한다.
/// </summary>
public static class ClaudeApi
{
    // Claude API 요청을 보낼 URL (Anthropic 공식 메시지 엔드포인트)
    private const string API_URL = "https://api.anthropic.com/v1/messages";

    // 사용할 Claude 모델 이름 (Haiku: 빠른 응답, 낮은 비용)
    private const string MODEL   = "claude-haiku-4-5";

    /// <summary>
    /// web.config의 appSettings 섹션에서 Claude API 키를 읽어오는 프로퍼티.
    /// 키가 없거나 잘못되면 API 호출 시 401 Unauthorized 오류가 발생한다.
    /// 보안상 API 키를 코드에 직접 작성하지 않고 설정 파일에서 읽는 방식을 사용한다.
    /// </summary>
    private static string ApiKey
    {
        get
        {
            // WebConfigurationManager를 통해 web.config의 <appSettings> 섹션에서 값 읽기
            return System.Web.Configuration.WebConfigurationManager.AppSettings["ClaudeApiKey"];
        }
    }

    /// <summary>
    /// 검색어를 Claude AI에 보내어 미리 정의된 13개 카테고리 중 하나로 분류한다.
    /// 분류 결과는 SearchDao.AddSearchHistory()에 저장되고, 검색 통계에 활용된다.
    /// AI 응답이 유효하지 않거나 API 오류 발생 시 "일반"을 반환한다.
    /// </summary>
    /// <param name="query">분류할 검색어 (예: "테슬라 모델3", "강남 삼겹살")</param>
    /// <returns>
    /// 카테고리 문자열 중 하나:
    /// 차량 / 국내장소 / 해외장소 / 국내맛집 / 해외맛집 / 게임 /
    /// IT기술 / 쇼핑 / 국내뉴스 / 해외뉴스 / 의료건강 / 스포츠 / 일반
    /// </returns>
    public static string Classify(string query)
    {
        // 검색어가 비어 있으면 AI 호출 없이 바로 "general" 반환
        if (string.IsNullOrWhiteSpace(query)) return "general";

        // Claude에게 보낼 프롬프트(지시문) 구성
        // - 13개 카테고리 목록을 명시하고 반드시 그 중 하나만 답하도록 지시
        // - {0} 자리에 실제 검색어가 들어감
        string prompt = string.Format(
            "다음 검색어를 아래 카테고리 중 하나로만 분류해. 키워드 하나만 답해. 다른 말은 하지 마.\n" +
            "카테고리: 차량, 국내장소, 해외장소, 국내맛집, 해외맛집, 게임, IT기술, 쇼핑, 국내뉴스, 해외뉴스, 의료건강, 스포츠, 일반\n" +
            "검색어: {0}", query);

        // Claude API를 호출하고 응답 텍스트를 받아 앞뒤 공백 제거
        string result = CallClaude(prompt).Trim();

        // 유효한 카테고리 목록 (AI가 이 중 하나를 포함한 텍스트를 반환해야 함)
        string[] valid = { "차량","국내장소","해외장소","국내맛집","해외맛집","게임","IT기술","쇼핑","국내뉴스","해외뉴스","의료건강","스포츠","일반" };

        // AI 응답에 유효한 카테고리 단어가 포함되어 있으면 해당 카테고리 반환
        foreach (string v in valid)
            if (result.Contains(v)) return v;

        // 매칭되는 카테고리가 없으면 기본값 "일반" 반환
        return "일반";
    }

    /// <summary>
    /// 검색 결과 목록을 Claude AI에 전달하여 관련도 높은 순서로 재정렬한 결과를 반환한다.
    /// AI가 "3,1,4,2" 형태의 인덱스 순서를 반환하면 호출자가 이를 파싱하여 결과 목록을 재배열한다.
    /// </summary>
    /// <param name="query">원래 검색어 (관련도 판단의 기준이 됨)</param>
    /// <param name="resultsJson">검색 결과 목록을 JSON 문자열로 직렬화한 값</param>
    /// <returns>
    /// 관련도 높은 순서대로 인덱스를 쉼표로 나열한 문자열 (예: "3,1,4,2")
    /// API 오류 시 "일반" 반환
    /// </returns>
    public static string RankResults(string query, string resultsJson)
    {
        // Claude에게 보낼 프롬프트 구성
        // - 검색어와 JSON 결과 목록을 제공하고 관련도 순 인덱스만 반환하도록 지시
        string prompt = string.Format(
            "검색어: \"{0}\"\n" +
            "아래 검색 결과들을 관련도 높은 순서로 번호만 쉼표로 나열해. 예: 3,1,4,2\n" +
            "결과:\n{1}", query, resultsJson);

        // Claude API 호출 후 응답 텍스트의 앞뒤 공백 제거하여 반환
        return CallClaude(prompt).Trim();
    }

    /// <summary>
    /// Claude API에 HTTP POST 요청을 전송하고 AI의 응답 텍스트를 반환하는 내부 메서드.
    /// Classify()와 RankResults()가 공통으로 사용하는 API 통신 로직을 담당한다.
    /// 오류 발생 시 서비스를 중단하지 않고 "일반"을 반환하여 안전하게 처리한다.
    /// (private: 클래스 외부에서 직접 호출 불가)
    /// </summary>
    /// <param name="userMessage">Claude에게 전달할 사용자 메시지(프롬프트) 문자열</param>
    /// <returns>Claude AI의 응답 텍스트, 오류 발생 시 "일반" 반환</returns>
    private static string CallClaude(string userMessage)
    {
        try
        {
            // HttpClient: HTTP 요청/응답을 처리하는 객체 (using으로 자동 자원 해제)
            using (var client = new HttpClient())
            {
                // API 응답 대기 최대 시간: 15초 (초과 시 TimeoutException 발생)
                client.Timeout = TimeSpan.FromSeconds(15);
                // 요청 헤더에 API 인증 키 추가 (Anthropic 인증 방식)
                client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                // Anthropic API 버전 헤더 추가 (필수 헤더)
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                // 요청 본문(Body) 구성: 모델명, 최대 토큰 수, 메시지 배열
                var body = new
                {
                    model      = MODEL,       // 사용할 Claude 모델 (claude-haiku-4-5)
                    max_tokens = 256,          // 응답 최대 토큰 수 (카테고리명/인덱스만 반환하므로 256으로 충분)
                    // 메시지 배열: role = "user"로 사용자 입력 메시지 전달
                    messages   = new[] { new { role = "user", content = userMessage } }
                };

                // 요청 본문 객체를 JSON 문자열로 직렬화
                var json    = new JavaScriptSerializer().Serialize(body);
                // JSON 문자열을 UTF-8 인코딩의 HTTP 콘텐츠로 변환, Content-Type: application/json 설정
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // API_URL에 POST 요청 전송 (비동기를 동기로 처리: Task.Run + .Result)
                var response = System.Threading.Tasks.Task.Run(() => client.PostAsync(API_URL, content)).Result;
                // 응답 본문을 문자열로 읽기
                var result   = System.Threading.Tasks.Task.Run(() => response.Content.ReadAsStringAsync()).Result;

                // 응답 JSON에서 실제 텍스트 추출 (content[0].text 경로)
                var ser  = new JavaScriptSerializer();
                // 응답 JSON을 동적(dynamic) 객체로 역직렬화
                dynamic obj = ser.DeserializeObject(result);
                // "content" 배열 꺼내기 (Claude 응답 구조: { content: [ { type: "text", text: "..." } ] })
                var arr = (object[])obj["content"];
                // 배열의 첫 번째 항목을 Dictionary로 캐스팅
                var first = (System.Collections.Generic.Dictionary<string, object>)arr[0];
                // "text" 키의 값이 실제 AI 응답 텍스트
                return first["text"].ToString();
            }
        }
        catch (Exception ex)
        {
            // 네트워크 오류, 타임아웃, JSON 파싱 오류 등 모든 예외를 catch
            // 디버그 콘솔에 오류 메시지 출력 (개발 중 확인용, 사용자에게는 노출되지 않음)
            System.Diagnostics.Debug.WriteLine("Claude API error: " + ex.Message);
            // API 실패해도 서비스가 중단되지 않도록 기본 카테고리 "일반" 반환
            return "일반";
        }
    }
}
