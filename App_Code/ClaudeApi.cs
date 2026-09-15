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
    // Claude API 요청을 보낼 URL (Anthropic 공식 메시지 엔드포인트) - RankResults 전용
    private const string API_URL = "https://api.anthropic.com/v1/messages";

    // RankResults에서 사용할 Claude 모델
    private const string MODEL   = "claude-haiku-4-5";

    // Groq API 엔드포인트 (OpenAI 호환) - Classify·GetRelated 전용
    private const string NIM_URL   = "https://api.groq.com/openai/v1/chat/completions";

    // Qwen3: 한국어·아시아 언어 특화, 무료, think 태그 자동 제거 처리됨
    private const string NIM_MODEL = "qwen/qwen3.8-27b";

    /// <summary>
    /// web.config의 appSettings 섹션에서 Claude API 키를 읽어오는 프로퍼티.
    /// 키가 없거나 잘못되면 API 호출 시 401 Unauthorized 오류가 발생한다.
    /// 보안상 API 키를 코드에 직접 작성하지 않고 설정 파일에서 읽는 방식을 사용한다.
    /// </summary>
    // Anthropic API 키 (RankResults 전용)
    private static string ApiKey
    {
        get { return System.Web.Configuration.WebConfigurationManager.AppSettings["ClaudeApiKey"]; }
    }

    // Groq API 키 (Classify·GetRelated 전용)
    private static string NimApiKey
    {
        get { return System.Web.Configuration.WebConfigurationManager.AppSettings["GroqApiKey"]; }
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
        // 검색어가 비어 있으면 API 호출 없이 바로 "일반" 반환
        if (string.IsNullOrWhiteSpace(query)) return "일반";

        // system에 모든 규칙·예시, user에는 검색어만 전달 (올바른 chat 구조)
        string systemMsg =
            "너는 한국어 검색어 분류 AI다. 아래 카테고리 중 정확히 하나만 출력해. 절대 설명하지 마.\n\n" +
            "카테고리:\n" +
            "차량(자동차·오토바이·전기차·트럭·SUV·차종·제조사)\n" +
            "국내장소(한국 도시·관광지·명소·산·해변: 서울·부산·제주도·경복궁·설악산)\n" +
            "해외장소(한국 외 모든 나라·도시·주(州)·지명: 미국·캐나다·일본·프랑스·텍사스·뉴욕·파리·도쿄·로스앤젤레스·런던·베이징·방콕)\n" +
            "국내맛집(한국 음식·국내 식당·카페·배달: 삼겹살·치킨·홍대 카페)\n" +
            "해외맛집(외국 음식·해외 식당: 파스타·라멘·타코)\n" +
            "게임(비디오게임·모바일게임·e스포츠: 롤·오버워치·마인크래프트)\n" +
            "IT기술(컴퓨터·프로그래밍·AI·소프트웨어·보안: 파이썬·챗GPT·자바스크립트)\n" +
            "쇼핑(제품구매·브랜드·가격비교·쇼핑몰: 나이키·아이폰가격·쿠팡)\n" +
            "국내뉴스(한국 정치·경제·사회·사건: 코스피·태풍·삼성실적)\n" +
            "해외뉴스(외국 정치·국제분쟁·글로벌경제: 트럼프·우크라이나·나스닥)\n" +
            "의료건강(질병·증상·의약품·건강관리: 독감·혈압·타이레놀·우울증)\n" +
            "스포츠(운동종목·경기·선수·구단: 손흥민·KBO·올림픽·헬스)\n" +
            "일반(위 항목 해당 없음)\n\n" +
            "예시:\n" +
            // 차량 (10개)
            "테슬라 → 차량\n" +
            "현대아이오닉 → 차량\n" +
            "BMW 5시리즈 → 차량\n" +
            "포르쉐 → 차량\n" +
            "람보르기니 → 차량\n" +
            "렉서스 → 차량\n" +
            "하이브리드차 → 차량\n" +
            "전기차 보조금 → 차량\n" +
            "오토바이 면허 → 차량\n" +
            "SUV 추천 → 차량\n" +
            // 국내장소 (10개)
            "제주도 → 국내장소\n" +
            "부산 → 국내장소\n" +
            "경복궁 → 국내장소\n" +
            "설악산 → 국내장소\n" +
            "한강공원 → 국내장소\n" +
            "인사동 → 국내장소\n" +
            "강릉 → 국내장소\n" +
            "남산타워 → 국내장소\n" +
            "광화문 → 국내장소\n" +
            "속초 → 국내장소\n" +
            // 해외장소 (14개)
            "캐나다 → 해외장소\n" +
            "텍사스 → 해외장소\n" +
            "미국 → 해외장소\n" +
            "일본 → 해외장소\n" +
            "프랑스 → 해외장소\n" +
            "뉴욕 → 해외장소\n" +
            "파리 → 해외장소\n" +
            "도쿄 → 해외장소\n" +
            "방콕 → 해외장소\n" +
            "싱가포르 → 해외장소\n" +
            "이탈리아 → 해외장소\n" +
            "호주 → 해외장소\n" +
            "베트남 → 해외장소\n" +
            "오사카 → 해외장소\n" +
            // 국내맛집 (10개)
            "삼겹살 → 국내맛집\n" +
            "치킨 맛집 → 국내맛집\n" +
            "냉면 → 국내맛집\n" +
            "비빔밥 → 국내맛집\n" +
            "홍대 카페 → 국내맛집\n" +
            "강남 한식 → 국내맛집\n" +
            "김치찌개 → 국내맛집\n" +
            "순대국 → 국내맛집\n" +
            "떡볶이 → 국내맛집\n" +
            "곱창 → 국내맛집\n" +
            // 해외맛집 (10개)
            "파스타 → 해외맛집\n" +
            "라멘 → 해외맛집\n" +
            "타코 → 해외맛집\n" +
            "스시 → 해외맛집\n" +
            "피자 → 해외맛집\n" +
            "딤섬 → 해외맛집\n" +
            "버거 → 해외맛집\n" +
            "파에야 → 해외맛집\n" +
            "케밥 → 해외맛집\n" +
            "팟타이 → 해외맛집\n" +
            // 게임 (10개)
            "롤 → 게임\n" +
            "배그 → 게임\n" +
            "오버워치 → 게임\n" +
            "마인크래프트 → 게임\n" +
            "스타크래프트 → 게임\n" +
            "메이플스토리 → 게임\n" +
            "로블록스 → 게임\n" +
            "발로란트 → 게임\n" +
            "원신 → 게임\n" +
            "포트나이트 → 게임\n" +
            // IT기술 (10개)
            "파이썬 → IT기술\n" +
            "챗GPT → IT기술\n" +
            "자바스크립트 → IT기술\n" +
            "도커 → IT기술\n" +
            "리눅스 → IT기술\n" +
            "클라우드 컴퓨팅 → IT기술\n" +
            "C++ → IT기술\n" +
            "깃허브 → IT기술\n" +
            "머신러닝 → IT기술\n" +
            "사이버보안 → IT기술\n" +
            // 쇼핑 (10개)
            "아이폰 가격 → 쇼핑\n" +
            "나이키 운동화 → 쇼핑\n" +
            "삼성갤럭시 → 쇼핑\n" +
            "에어팟 → 쇼핑\n" +
            "쿠팡 로켓배송 → 쇼핑\n" +
            "명품 가방 → 쇼핑\n" +
            "애플워치 → 쇼핑\n" +
            "다이슨 청소기 → 쇼핑\n" +
            "노트북 최저가 → 쇼핑\n" +
            "아디다스 → 쇼핑\n" +
            // 국내뉴스 (10개)
            "코스피 → 국내뉴스\n" +
            "삼성 실적 → 국내뉴스\n" +
            "부동산 정책 → 국내뉴스\n" +
            "한국 금리 → 국내뉴스\n" +
            "태풍 → 국내뉴스\n" +
            "대통령 지지율 → 국내뉴스\n" +
            "총선 → 국내뉴스\n" +
            "국회 → 국내뉴스\n" +
            "서울 집값 → 국내뉴스\n" +
            "카카오 실적 → 국내뉴스\n" +
            // 해외뉴스 (10개)
            "트럼프 → 해외뉴스\n" +
            "우크라이나 전쟁 → 해외뉴스\n" +
            "나스닥 → 해외뉴스\n" +
            "이스라엘 → 해외뉴스\n" +
            "달러 환율 → 해외뉴스\n" +
            "바이든 → 해외뉴스\n" +
            "중국 경제 → 해외뉴스\n" +
            "NATO → 해외뉴스\n" +
            "연준 금리 → 해외뉴스\n" +
            "엔화 → 해외뉴스\n" +
            // 의료건강 (10개)
            "독감 → 의료건강\n" +
            "고혈압 → 의료건강\n" +
            "당뇨 → 의료건강\n" +
            "타이레놀 → 의료건강\n" +
            "다이어트 방법 → 의료건강\n" +
            "코로나 백신 → 의료건강\n" +
            "우울증 → 의료건강\n" +
            "불면증 → 의료건강\n" +
            "비타민 → 의료건강\n" +
            "건강검진 → 의료건강\n" +
            // 스포츠 (10개)
            "손흥민 → 스포츠\n" +
            "류현진 → 스포츠\n" +
            "KBO → 스포츠\n" +
            "올림픽 → 스포츠\n" +
            "헬스 → 스포츠\n" +
            "축구 국가대표 → 스포츠\n" +
            "NBA → 스포츠\n" +
            "이강인 → 스포츠\n" +
            "골프 → 스포츠\n" +
            "테니스 → 스포츠";

        string userMsg = query;
        // 공백 포함 변형("해외 장소" 등) 대비: 공백 제거 후 매칭
        string result = CallNim(userMsg, systemMsg).Trim().Replace(" ", "");

        // 유효한 카테고리 목록에 포함된 키워드를 응답에서 찾아 반환
        string[] valid = { "차량","국내장소","해외장소","국내맛집","해외맛집","게임","IT기술","쇼핑","국내뉴스","해외뉴스","의료건강","스포츠","일반" };
        foreach (string v in valid)
            if (result.Contains(v)) return v;

        return "일반";
    }

    /// <summary>
    /// 검색어와 관련된 검색어 5개를 NIM AI로 생성하여 JSON 배열 문자열로 반환한다.
    /// 실패 시 빈 배열 "[]" 반환.
    /// </summary>
    public static string GetRelated(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return "[]";

        // 쉼표 구분 5개 요청 - 숫자·번호 없이 순수 검색어만 나열하도록 지시
        string prompt = string.Format(
            "검색어 '{0}'과(와) 관련된 검색어 5개를 쉼표로만 나열해. 다른 말은 절대 하지 마.\n예) 강남 맛집, 홍대 카페, 이태원 레스토랑, 성수동 맛집, 여의도 레스토랑",
            query);

        string result = CallNim(prompt).Trim();

        // 쉼표 분리 후 JSON 배열로 직렬화 (최대 5개)
        var sb    = new StringBuilder("[");
        int count = 0;
        foreach (string raw in result.Split(','))
        {
            string t = raw.Trim();
            // 모델이 "1. 강남 맛집" 처럼 번호를 붙인 경우 제거
            if (t.Length > 2 && char.IsDigit(t[0]) && (t[1] == '.' || t[1] == ')'))
                t = t.Substring(2).Trim();
            if (string.IsNullOrEmpty(t) || t.Length > 50) continue;
            if (count > 0) sb.Append(",");
            sb.Append("\"").Append(t.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"");
            count++;
            if (count >= 5) break;
        }
        sb.Append("]");
        return sb.ToString();
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
    /// NVIDIA NIM API(OpenAI 호환)에 요청을 보내고 응답 텍스트를 반환하는 내부 메서드.
    /// system/user 분리 메시지 구조 사용. DeepSeek 계열의 &lt;think&gt;태그 자동 제거.
    /// </summary>
    private static string CallNim(string userMessage, string systemMessage = null)
    {
        // 429(RPM 한도 초과) 시 1회 재시도
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    // OpenAI 호환 방식: Bearer 토큰 인증
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + NimApiKey);

                    // system 메시지가 있으면 [system, user] 배열, 없으면 [user] 단독
                    object messages;
                    if (!string.IsNullOrEmpty(systemMessage))
                        messages = new object[]
                        {
                            new { role = "system", content = systemMessage },
                            new { role = "user",   content = userMessage   }
                        };
                    else
                        messages = new object[] { new { role = "user", content = userMessage } };

                    var body = new
                    {
                        model       = NIM_MODEL,
                        max_tokens  = 128,
                        temperature = 0.1,
                        messages    = messages
                    };

                    var json    = new JavaScriptSerializer().Serialize(body);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = System.Threading.Tasks.Task.Run(() => client.PostAsync(NIM_URL, content)).Result;

                    // 429 Rate Limit: 2초 후 재시도
                    if (response.StatusCode == (System.Net.HttpStatusCode)429)
                    {
                        if (attempt == 0) { System.Threading.Thread.Sleep(2000); continue; }
                        return "일반";
                    }

                    var raw = System.Threading.Tasks.Task.Run(() => response.Content.ReadAsStringAsync()).Result;

                    // OpenAI 호환 응답 파싱: choices[0].message.content
                    dynamic obj  = new JavaScriptSerializer().DeserializeObject(raw);
                    var choices  = (object[])obj["choices"];
                    var first    = (System.Collections.Generic.Dictionary<string, object>)choices[0];
                    var msg      = (System.Collections.Generic.Dictionary<string, object>)first["message"];
                    string text  = msg["content"].ToString();

                    // Qwen3 등 추론 모델의 <think>...</think> 블록 제거
                    int thinkEnd = text.LastIndexOf("</think>");
                    if (thinkEnd >= 0) text = text.Substring(thinkEnd + 8);

                    return text.Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Groq API error: " + ex.Message);
                if (attempt == 0) { System.Threading.Thread.Sleep(1000); continue; }
                return "일반";
            }
        }
        return "일반";
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
