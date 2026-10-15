using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

/// <summary>
/// 검색엔진 품질 평가 페이지 코드비하인드.
/// Google / Naver / Daum / RRF(Naver+Daum) 4개 조건을 비교하여
/// nDCG@10, MRR, P@5 지표를 JSON으로 반환한다.
///
/// 사용법:
///   GET /Eval.aspx              → 평가 UI 페이지
///   GET /Eval.aspx?action=run&n=20  → 평가 실행, JSON 반환
/// </summary>
public partial class Eval : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string action = Request.QueryString["action"];
        if (action != "run") return;

        // 28개 쿼리 × (검색 5s + Groq 20s + 딜레이 12s) ≈ 1050s → 2400초 허용
        Server.ScriptTimeout = 2400;

        // JSON 결과 반환 후 종료
        Response.ContentType = "application/json; charset=utf-8";

        int n = 20;
        if (!int.TryParse(Request.QueryString["n"], out n) || n < 1) n = 20;
        if (n > 50) n = 50;

        Response.Write(RunEval(n));
        Response.End();
    }

    // ──────────────────────────────────────────────────────────────
    // 평가 메인 메서드
    // ──────────────────────────────────────────────────────────────

    private string RunEval(int queryCount)
    {
        // 1. SearchHistory에서 인기 쿼리 추출 (없으면 기본 테스트셋 사용)
        var queries = GetTopQueries(queryCount);
        if (queries.Count == 0)
            queries = GetDefaultQueries(queryCount);

        // 엔진별 누적 지표
        double sumNDCG_G = 0, sumNDCG_N = 0, sumNDCG_D = 0, sumNDCG_R = 0;
        double sumMRR_G  = 0, sumMRR_N  = 0, sumMRR_D  = 0, sumMRR_R  = 0;
        double sumP5_G   = 0, sumP5_N   = 0, sumP5_D   = 0, sumP5_R   = 0;
        int    valid     = 0;

        var byQuery = new List<object>();

        foreach (var kv in queries)
        {
            string q       = kv.Key;
            string keyword = kv.Value;

            try
            {
                // 2. 병렬 검색 (최대 12초)
                var gTask = Task.Run(() => GoogleCrawler.Search(q, 10));
                var nTask = Task.Run(() => NaverCrawler.Search(q, 10));
                var dTask = Task.Run(() => DaumCrawler.Search(q, 10));
                Task.WaitAll(new Task[] { gTask, nTask, dTask }, 12000);

                var gRes = (gTask.IsCompleted && !gTask.IsFaulted) ? gTask.Result : new List<SearchResult>();
                var nRes = (nTask.IsCompleted && !nTask.IsFaulted) ? nTask.Result : new List<SearchResult>();
                var dRes = (dTask.IsCompleted && !dTask.IsFaulted) ? dTask.Result : new List<SearchResult>();

                // 3. RRF(Google + Naver + Daum) - 메타검색엔진: 모든 엔진 통합
                var rRes = RrfCombine3(gRes, nRes, dRes, 10);

                // 4. 전체 고유 URL 풀 (최대 30개) → LLM 레이블링
                var pool = BuildPool(gRes, nRes, dRes);
                if (pool.Count == 0) continue;

                int pn = pool.Count;
                string[] titles   = new string[pn];
                string[] snippets = new string[pn];
                for (int i = 0; i < pn; i++)
                {
                    titles[i]   = pool[i].Title       ?? "";
                    snippets[i] = pool[i].Description ?? "";
                }

                // 5. LLM 관련성 레이블 (쿼리당 1회 Groq 호출)
                int[] rawScores = ClaudeApi.LabelRelevance(q, titles, snippets);

                // URL → 점수 맵 구성
                var labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < pn; i++)
                    labels[pool[i].Url] = rawScores[i];

                // Groq TPM(토큰/분) 한도 대응: 12초 대기 (20아이템×~830토큰 × 5호출/분 = 4,150 TPM)
                System.Threading.Thread.Sleep(12000);

                // 6. 지표 계산
                double ndcg_g = ComputeNDCG(gRes, labels, 10);
                double ndcg_n = ComputeNDCG(nRes, labels, 10);
                double ndcg_d = ComputeNDCG(dRes, labels, 10);
                double ndcg_r = ComputeNDCG(rRes, labels, 10);

                double mrr_g = ComputeMRR(gRes, labels);
                double mrr_n = ComputeMRR(nRes, labels);
                double mrr_d = ComputeMRR(dRes, labels);
                double mrr_r = ComputeMRR(rRes, labels);

                double p5_g = ComputeP(gRes, labels, 5);
                double p5_n = ComputeP(nRes, labels, 5);
                double p5_d = ComputeP(dRes, labels, 5);
                double p5_r = ComputeP(rRes, labels, 5);

                sumNDCG_G += ndcg_g; sumNDCG_N += ndcg_n; sumNDCG_D += ndcg_d; sumNDCG_R += ndcg_r;
                sumMRR_G  += mrr_g;  sumMRR_N  += mrr_n;  sumMRR_D  += mrr_d;  sumMRR_R  += mrr_r;
                sumP5_G   += p5_g;   sumP5_N   += p5_n;   sumP5_D   += p5_d;   sumP5_R   += p5_r;
                valid++;

                byQuery.Add(new {
                    query   = q,
                    keyword = keyword,
                    nDCG10  = new { Google = R(ndcg_g), Naver = R(ndcg_n), Daum = R(ndcg_d), RRF = R(ndcg_r) },
                    MRR     = new { Google = R(mrr_g),  Naver = R(mrr_n),  Daum = R(mrr_d),  RRF = R(mrr_r)  },
                    P5      = new { Google = R(p5_g),   Naver = R(p5_n),   Daum = R(p5_d),   RRF = R(p5_r)   }
                });
            }
            catch (Exception ex)
            {
                byQuery.Add(new { query = q, keyword = keyword, error = ex.Message });
            }
        }

        if (valid == 0) return "{\"error\":\"유효한 평가 결과 없음\"}";

        // 7. 집계 결과 직렬화
        var output = new {
            queryCount = valid,
            aggregate  = new {
                nDCG10 = new { Google = R(sumNDCG_G/valid), Naver = R(sumNDCG_N/valid), Daum = R(sumNDCG_D/valid), RRF = R(sumNDCG_R/valid) },
                MRR    = new { Google = R(sumMRR_G /valid), Naver = R(sumMRR_N /valid), Daum = R(sumMRR_D /valid), RRF = R(sumMRR_R /valid) },
                P5     = new { Google = R(sumP5_G  /valid), Naver = R(sumP5_N  /valid), Daum = R(sumP5_D  /valid), RRF = R(sumP5_R  /valid) }
            },
            byQuery = byQuery
        };

        return new JavaScriptSerializer().Serialize(output);
    }

    // ──────────────────────────────────────────────────────────────
    // DB 헬퍼: SearchHistory 상위 쿼리 추출
    // ──────────────────────────────────────────────────────────────

    private List<KeyValuePair<string, string>> GetTopQueries(int n)
    {
        var list = new List<KeyValuePair<string, string>>();
        SqlDataReader reader = null;
        try
        {
            // 빈도순 상위 N개, 카테고리(Keyword)도 함께 가져옴
            string sql = string.Format(
                "SELECT TOP {0} Query, MAX(Keyword) AS Keyword " +
                "FROM SearchHistory " +
                "GROUP BY Query " +
                "ORDER BY COUNT(*) DESC", n);

            reader = DbMan.ExecuteReader(sql);
            while (reader.Read())
            {
                string q = reader["Query"].ToString().Trim();
                string k = reader["Keyword"] != DBNull.Value ? reader["Keyword"].ToString() : "일반";
                if (!string.IsNullOrEmpty(q)) list.Add(new KeyValuePair<string, string>(q, k));
            }
        }
        catch { }
        finally
        {
            if (reader != null && !reader.IsClosed) reader.Close();
            try { DbMan.Close(); } catch { }
        }
        return list;
    }

    // SearchHistory가 비어 있을 때 사용하는 카테고리별 기본 테스트 쿼리 28개
    // 구성: 글로벌(구글 강점) 10 + 국내생활(네이버/다음 강점) 10 + 해외여행(한국인 블로그 네이버 강점) 8
    private List<KeyValuePair<string, string>> GetDefaultQueries(int max)
    {
        var all = new List<KeyValuePair<string, string>>
        {
            // ── 구글이 강한 글로벌·IT 쿼리 (10개) ──────────────────────────
            new KeyValuePair<string,string>("나이키 운동화",        "쇼핑"),
            new KeyValuePair<string,string>("아이폰16",             "쇼핑"),
            new KeyValuePair<string,string>("파이썬 튜토리얼",     "IT기술"),
            new KeyValuePair<string,string>("챗GPT",                "IT기술"),
            new KeyValuePair<string,string>("코스피",               "국내뉴스"),
            new KeyValuePair<string,string>("나스닥",               "해외뉴스"),
            new KeyValuePair<string,string>("테슬라 모델3",         "차량"),
            new KeyValuePair<string,string>("리그오브레전드",       "게임"),
            new KeyValuePair<string,string>("배틀그라운드",         "게임"),
            new KeyValuePair<string,string>("트럼프",               "해외뉴스"),

            // ── 네이버·다음이 강한 국내 생활·여행 쿼리 (10개) ─────────────
            new KeyValuePair<string,string>("홍대 맛집",            "국내맛집"),
            new KeyValuePair<string,string>("강남 삼겹살",          "국내맛집"),
            new KeyValuePair<string,string>("서울 근교 맛집 추천",  "국내맛집"),
            new KeyValuePair<string,string>("부산 맛집 추천",       "국내맛집"),
            new KeyValuePair<string,string>("제주도 맛집",          "국내맛집"),
            new KeyValuePair<string,string>("국내 여행지 추천",     "국내장소"),
            new KeyValuePair<string,string>("경복궁",               "국내장소"),
            new KeyValuePair<string,string>("KBO 순위",             "스포츠"),
            new KeyValuePair<string,string>("독감 증상",            "의료건강"),
            new KeyValuePair<string,string>("고혈압",               "의료건강"),

            // ── 해외여행(한국인 블로그) · 혼합 쿼리 (8개) ─────────────────
            new KeyValuePair<string,string>("오사카 맛집",          "해외맛집"),
            new KeyValuePair<string,string>("방콕 여행 코스",       "해외장소"),
            new KeyValuePair<string,string>("베트남 다낭 여행",     "해외장소"),
            new KeyValuePair<string,string>("도쿄 여행 코스",       "해외장소"),
            new KeyValuePair<string,string>("손흥민",               "스포츠"),
            new KeyValuePair<string,string>("한국 금리",            "국내뉴스"),
            new KeyValuePair<string,string>("현대 아이오닉",        "차량"),
            new KeyValuePair<string,string>("한국 드라마 추천",     "일반"),
        };
        return all.GetRange(0, Math.Min(max, all.Count));
    }

    // ──────────────────────────────────────────────────────────────
    // 결과 풀 구성: 3개 엔진 결과를 URL 기준 중복 제거 (최대 30개)
    // ──────────────────────────────────────────────────────────────

    // 엔진별 균등 배분으로 풀 구성 (Google 7 + Naver 7 + Daum 6 = 최대 20)
    // 순차 채우기 방식은 Daum URL이 풀에서 빠져 nDCG=0이 되는 버그가 있음
    private List<SearchResult> BuildPool(List<SearchResult> g, List<SearchResult> n, List<SearchResult> d)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pool = new List<SearchResult>();

        // 각 엔진에서 최대 할당량만큼 먼저 확보
        int[] quotas  = { 7, 7, 6 };
        var   sources = new List<List<SearchResult>> { g, n, d };

        for (int s = 0; s < sources.Count; s++)
        {
            int added = 0;
            foreach (var r in sources[s])
            {
                if (string.IsNullOrEmpty(r.Url)) continue;
                if (seen.Add(r.Url)) { pool.Add(r); added++; }
                if (added >= quotas[s]) break;
            }
        }

        return pool;
    }

    // ──────────────────────────────────────────────────────────────
    // RRF 통합: Google + Naver + Daum 3중 통합 → 상위 topN
    // 메타검색엔진 방식: 모든 엔진 결과를 순위 기반 융합
    // ──────────────────────────────────────────────────────────────

    private List<SearchResult> RrfCombine3(List<SearchResult> google, List<SearchResult> naver, List<SearchResult> daum, int topN)
    {
        const int k = 60;
        var scoreMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var itemMap  = new Dictionary<string, SearchResult>(StringComparer.OrdinalIgnoreCase);

        // 세 엔진의 순위 점수를 합산: score(d) = Σ 1/(k + rank)
        foreach (var engine in new[] { google, naver, daum })
        {
            for (int i = 0; i < engine.Count; i++)
            {
                string url = engine[i].Url;
                if (string.IsNullOrEmpty(url)) continue;
                if (!scoreMap.ContainsKey(url)) { scoreMap[url] = 0; itemMap[url] = engine[i]; }
                scoreMap[url] += 1.0 / (k + i + 1);
            }
        }

        var sorted = new List<KeyValuePair<string, double>>(scoreMap);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

        var result = new List<SearchResult>();
        for (int i = 0; i < Math.Min(topN, sorted.Count); i++)
            result.Add(itemMap[sorted[i].Key]);
        return result;
    }

    // ──────────────────────────────────────────────────────────────
    // RRF 통합: Naver + Daum → 상위 topN (레거시 - 현재 미사용)
    // ──────────────────────────────────────────────────────────────

    private List<SearchResult> RrfCombine(List<SearchResult> naver, List<SearchResult> daum, int topN)
    {
        const int k = 60;
        var scoreMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var itemMap  = new Dictionary<string, SearchResult>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < naver.Count; i++)
        {
            string url = naver[i].Url;
            if (string.IsNullOrEmpty(url)) continue;
            if (!scoreMap.ContainsKey(url)) { scoreMap[url] = 0; itemMap[url] = naver[i]; }
            scoreMap[url] += 1.0 / (k + i + 1);
        }
        for (int i = 0; i < daum.Count; i++)
        {
            string url = daum[i].Url;
            if (string.IsNullOrEmpty(url)) continue;
            if (!scoreMap.ContainsKey(url)) { scoreMap[url] = 0; itemMap[url] = daum[i]; }
            scoreMap[url] += 1.0 / (k + i + 1);
        }

        var sorted = new List<KeyValuePair<string, double>>(scoreMap);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

        var result = new List<SearchResult>();
        for (int i = 0; i < Math.Min(topN, sorted.Count); i++)
            result.Add(itemMap[sorted[i].Key]);
        return result;
    }

    // ──────────────────────────────────────────────────────────────
    // 평가 지표 계산
    // ──────────────────────────────────────────────────────────────

    // nDCG@k: 이상적 DCG 대비 실제 DCG 비율
    private double ComputeNDCG(List<SearchResult> results, Dictionary<string, int> labels, int k)
    {
        if (results == null || results.Count == 0) return 0;
        int limit = Math.Min(results.Count, k);

        double dcg = 0;
        var rels = new List<int>(limit);
        for (int i = 0; i < limit; i++)
        {
            int rel = GetLabel(results[i].Url, labels);
            rels.Add(rel);
            dcg += (Math.Pow(2, rel) - 1) / Math.Log(i + 2, 2);
        }

        // IDCG: 내림차순 정렬 후 DCG
        rels.Sort((a, b) => b.CompareTo(a));
        double idcg = 0;
        for (int i = 0; i < rels.Count; i++)
            idcg += (Math.Pow(2, rels[i]) - 1) / Math.Log(i + 2, 2);

        return idcg < 1e-9 ? 0 : dcg / idcg;
    }

    // MRR: 첫 번째 관련(rel>=1) 결과 순위의 역수
    private double ComputeMRR(List<SearchResult> results, Dictionary<string, int> labels)
    {
        if (results == null) return 0;
        for (int i = 0; i < results.Count; i++)
            if (GetLabel(results[i].Url, labels) >= 1)
                return 1.0 / (i + 1);
        return 0;
    }

    // P@k: 상위 k개 중 관련(rel>=1) 비율
    private double ComputeP(List<SearchResult> results, Dictionary<string, int> labels, int k)
    {
        if (results == null || results.Count == 0) return 0;
        int limit = Math.Min(results.Count, k);
        int hits  = 0;
        for (int i = 0; i < limit; i++)
            if (GetLabel(results[i].Url, labels) >= 1) hits++;
        return (double)hits / limit;
    }

    private int GetLabel(string url, Dictionary<string, int> labels)
    {
        int v;
        return labels.TryGetValue(url ?? "", out v) ? v : 0;
    }

    // 소수점 4자리 반올림
    private double R(double v) { return Math.Round(v, 4); }
}
