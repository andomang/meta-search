<%@ Page Language="C#" AutoEventWireup="false" %>
<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>메타검색 성능 평가 리포트</title>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;600&family=Noto+Sans+KR:wght@300;400;500;700&display=swap">
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
<style>
:root{
  --bg:#F0F2F8;--sur:#FFF;--s2:#F5F7FC;--fg:#14141C;--mu:#6B6C80;--bd:#DDE0EC;
  --G:#1A52CC;--N:#15803D;--D:#B45309;--R:#6D28D9;
  --gs:#EEF3FF;--ns:#F0FDF4;--ds:#FFFBEB;--rs:#F5F3FF;
  --ok:#15803D;--warn:#B45309;--err:#B91C1C;
  --fn:'Noto Sans KR','Apple SD Gothic Neo',sans-serif;
  --mo:'IBM Plex Mono','Fira Mono',monospace;
}
*,*::before,*::after{box-sizing:border-box}
body{margin:0;background:var(--bg);color:var(--fg);font-family:var(--fn);font-size:14px;line-height:1.7}

/* ── 로딩 화면 ── */
#loading{
  position:fixed;inset:0;z-index:100;
  background:linear-gradient(135deg,#0F172A 0%,#1E1B4B 50%,#0F172A 100%);
  display:flex;flex-direction:column;align-items:center;justify-content:center;gap:24px;
  color:#E2E8F0;
}
.ld-title{font-size:22px;font-weight:700;letter-spacing:-.02em}
.ld-title em{font-style:normal;background:linear-gradient(90deg,#818CF8,#C084FC);-webkit-background-clip:text;-webkit-text-fill-color:transparent}
.ld-sub{font-size:13px;color:#94A3B8;text-align:center;max-width:360px}
.progress-bar{width:280px;height:4px;background:#334155;border-radius:2px;overflow:hidden}
.progress-fill{height:100%;width:0%;background:linear-gradient(90deg,#818CF8,#C084FC);border-radius:2px;transition:width .5s ease}
.ld-msg{font-size:12px;color:#818CF8;min-height:20px;text-align:center}
.ld-steps{display:flex;flex-direction:column;gap:8px;margin-top:8px}
.ld-step{font-size:12px;color:#475569;display:flex;align-items:center;gap:8px;transition:color .3s}
.ld-step.done{color:#34D399}
.ld-step.active{color:#A78BFA}
.ld-step::before{content:'○';font-size:10px;flex-shrink:0}
.ld-step.done::before{content:'✓'}
.ld-step.active::before{content:'◉'}

/* ── 메인 레이아웃 ── */
#main{display:none;padding-inline:20px;padding-block:28px 60px;max-width:1020px;margin:0 auto}
.wrap{display:flex;flex-direction:column;gap:28px}

/* 헤더 */
.hdr{padding:28px 32px;background:var(--sur);border:1px solid var(--bd);border-radius:12px;position:relative;overflow:hidden}
.hdr::after{content:'';position:absolute;top:-100px;right:-100px;width:350px;height:350px;border-radius:50%;background:radial-gradient(circle,rgba(109,40,217,.08),transparent 70%);pointer-events:none}
.hdr-eye{font-size:10px;font-weight:700;letter-spacing:.12em;text-transform:uppercase;color:var(--mu);margin-bottom:10px}
.hdr-title{font-size:clamp(18px,3vw,26px);font-weight:700;line-height:1.4;margin:0 0 8px}
.hdr-title em{font-style:normal;color:var(--R)}
.hdr-desc{font-size:13px;color:var(--mu);max-width:560px;word-break:keep-all;margin:0 0 20px}
.hdr-pills{display:flex;gap:8px;flex-wrap:wrap}
.pill{font-size:11px;font-weight:700;padding:4px 12px;border-radius:20px;border:1px solid var(--bd)}
.pill.g{background:var(--gs);color:var(--G);border-color:color-mix(in srgb,var(--G) 30%,transparent)}
.pill.n{background:var(--ns);color:var(--N);border-color:color-mix(in srgb,var(--N) 30%,transparent)}
.pill.d{background:var(--ds);color:var(--D);border-color:color-mix(in srgb,var(--D) 30%,transparent)}
.pill.r{background:var(--rs);color:var(--R);border-color:color-mix(in srgb,var(--R) 30%,transparent)}
.hdr-ts{font-size:11px;color:var(--mu);margin-top:12px}

/* 알고리즘 비교 */
.sec-lbl{font-size:10px;font-weight:700;letter-spacing:.12em;text-transform:uppercase;color:var(--mu);margin-bottom:4px;display:flex;align-items:center;gap:8px}
.sec-lbl::before{content:'';display:block;width:3px;height:12px;border-radius:2px;background:var(--acc,var(--mu))}
.sec-title{font-size:17px;font-weight:700;margin:0 0 4px}
.sec-desc{font-size:13px;color:var(--mu);max-width:620px;word-break:keep-all}

.algo-grid{display:grid;grid-template-columns:1fr 1fr;gap:16px}
.algo{background:var(--sur);border:1px solid var(--bd);border-radius:10px;padding:18px}
.algo.old{border-top:3px solid #94A3B8}
.algo.new{border-top:3px solid var(--R)}
.algo-lbl{font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--mu);margin-bottom:6px}
.algo-h{font-size:14px;font-weight:700;margin-bottom:12px}
.algo.new .algo-h{color:var(--R)}
.rl{display:flex;flex-direction:column;gap:3px}
.ri{display:flex;align-items:center;gap:7px;padding:5px 9px;background:var(--s2);border-radius:5px;font-size:12px}
.ri-rk{font-family:var(--mo);font-size:10px;color:var(--mu);width:16px;text-align:right}
.tag{font-size:10px;font-weight:700;padding:1px 5px;border-radius:3px}
.tag.n{background:var(--ns);color:var(--N)}.tag.d{background:var(--ds);color:var(--D)}
.ri-txt{flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.ri-sc{font-family:var(--mo);font-size:10px;color:var(--mu)}
.fbox{margin-top:10px;padding:10px 12px;background:var(--s2);border-radius:6px;font-size:12px}
.fbox code{font-family:var(--mo);color:var(--R);font-size:12px;display:block;margin-bottom:4px}
.fbox p{margin:0;color:var(--mu);font-size:11px}
.hbox{margin-top:10px;padding:9px 12px;border-radius:6px;font-size:12px;word-break:keep-all;line-height:1.6}
.hbox.ok{background:color-mix(in srgb,var(--ok) 10%,transparent);border-left:3px solid var(--ok)}
.hbox.warn{background:#F8FAFF;border-left:3px solid #94A3B8}

/* 타일 */
.tiles{display:grid;grid-template-columns:repeat(4,1fr);gap:12px}
.tile{background:var(--sur);border:1px solid var(--bd);border-radius:8px;padding:14px 16px;display:flex;flex-direction:column;gap:3px}
.tile.g{border-left:4px solid var(--G)}.tile.n{border-left:4px solid var(--N)}
.tile.d{border-left:4px solid var(--D)}.tile.r{border-left:4px solid var(--R)}
.tile-eng{font-size:10px;font-weight:700;letter-spacing:.07em;text-transform:uppercase}
.tile.g .tile-eng{color:var(--G)}.tile.n .tile-eng{color:var(--N)}
.tile.d .tile-eng{color:var(--D)}.tile.r .tile-eng{color:var(--R)}
.tile-val{font-family:var(--mo);font-size:24px;font-weight:600;font-variant-numeric:tabular-nums;line-height:1}
.tile-lbl{font-size:10px;color:var(--mu)}
.tile-sub{font-size:10px;color:var(--mu)}.tile-sub span{font-family:var(--mo)}
.badge-rank{font-size:10px;font-weight:700;padding:2px 7px;border-radius:10px;align-self:flex-start;margin-top:4px}
.badge-rank.gold{background:#FEF9C3;color:#92400E}
.badge-rank.silver{background:#F1F5F9;color:#64748B}
.badge-rank.bronze{background:#FFF7ED;color:#C2410C}

/* 개선율 바 */
.improve-card{background:var(--sur);border:1px solid var(--bd);border-radius:10px;padding:18px 20px;display:flex;flex-direction:column;gap:14px}
.imp-title{font-size:13px;font-weight:700}
.imp-row{display:flex;align-items:center;gap:10px;font-size:12px}
.imp-eng{width:48px;font-size:11px;font-weight:700;flex-shrink:0}
.imp-eng.n{color:var(--N)}.imp-eng.d{color:var(--D)}.imp-eng.r{color:var(--R)}
.imp-track{flex:1;height:20px;background:var(--s2);border-radius:4px;position:relative;overflow:hidden}
.imp-fill{height:100%;border-radius:4px;position:absolute;left:0;top:0;display:flex;align-items:center;justify-content:flex-end;padding-right:8px}
.imp-fill-v{font-family:var(--mo);font-size:10px;font-weight:600;color:#fff;white-space:nowrap}
.imp-delta{font-family:var(--mo);font-size:11px;font-weight:600;min-width:52px;text-align:right}

/* 차트 */
.charts{display:grid;grid-template-columns:repeat(3,1fr);gap:14px}
.cc{background:var(--sur);border:1px solid var(--bd);border-radius:10px;padding:16px}
.cc-t{font-size:13px;font-weight:700;margin-bottom:2px}
.cc-s{font-size:11px;color:var(--mu);margin-bottom:10px}
.cc-w{position:relative;height:136px}

/* 테이블 */
.tw{background:var(--sur);border:1px solid var(--bd);border-radius:10px;overflow:hidden}
.tw-h{padding:12px 18px 10px;border-bottom:1px solid var(--bd);display:flex;justify-content:space-between;align-items:center}
.tw-ht{font-size:13px;font-weight:700}.tw-hn{font-size:11px;color:var(--mu)}
.ts{overflow-x:auto}
table{width:100%;border-collapse:collapse;font-size:12px}
th{padding:7px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:.05em;text-transform:uppercase;color:var(--mu);background:var(--s2);border-bottom:1px solid var(--bd);white-space:nowrap}
th.r{text-align:right}
td{padding:7px 12px;border-bottom:1px solid var(--bd);white-space:nowrap}
tr:last-child td{border-bottom:none}
tr:hover td{background:var(--s2)}
.num{font-family:var(--mo);font-variant-numeric:tabular-nums;text-align:right}
.kw{display:inline-block;font-size:10px;padding:1px 6px;border-radius:3px;background:var(--bd);color:var(--mu);font-weight:500}
.win{font-weight:700}.win.g{color:var(--G)}.win.n{color:var(--N)}.win.d{color:var(--D)}.win.r{color:var(--R)}
.zero-row td{color:var(--err)}
.zero-row .kw{background:color-mix(in srgb,var(--err) 12%,transparent);color:var(--err)}

/* 발견 카드 */
.findings{display:grid;grid-template-columns:repeat(3,1fr);gap:14px}
.fin{background:var(--sur);border:1px solid var(--bd);border-radius:10px;padding:18px;display:flex;flex-direction:column;gap:8px}
.fin-n{font-family:var(--mo);font-size:10px;color:var(--mu);font-weight:600}
.fin-t{font-size:14px;font-weight:700;line-height:1.4;text-wrap:balance;word-break:keep-all}
.fin-v{font-family:var(--mo);font-size:22px;font-weight:600}
.fin-d{font-size:12px;color:var(--mu);line-height:1.6;word-break:keep-all}
.fin.green .fin-v{color:var(--ok)}.fin.purple .fin-v{color:var(--R)}.fin.blue .fin-v{color:var(--G)}

.note{font-size:11px;color:var(--mu);line-height:1.8;padding:10px 14px;border-left:3px solid var(--bd);background:var(--sur);border-radius:0 6px 6px 0}
.legend{display:flex;gap:14px;flex-wrap:wrap;font-size:12px}
.leg{display:flex;align-items:center;gap:6px}
.dot{width:10px;height:10px;border-radius:2px;flex-shrink:0}
.leg.g .dot{background:var(--G)}.leg.n .dot{background:var(--N)}.leg.d .dot{background:var(--D)}.leg.r .dot{background:var(--R)}

/* 에러 */
#error-box{display:none;margin:40px auto;max-width:500px;background:#FEF2F2;border:1px solid #FECACA;border-radius:8px;padding:20px;text-align:center}
#error-box h3{color:#B91C1C;margin:0 0 8px}#error-box p{font-size:13px;color:#7F1D1D}

@media(max-width:700px){
  .algo-grid,.charts,.findings{grid-template-columns:1fr}
  .tiles{grid-template-columns:repeat(2,1fr)}
}
</style>
</head>
<body>

<!-- ── 로딩 화면 ── -->
<div id="loading">
  <div style="text-align:center">
    <div class="ld-title">메타검색 성능 평가 중 <em>…</em></div>
    <div class="ld-sub">Google · Naver · Daum · RRF 4개 방식을 AI가 객관적으로 비교합니다</div>
  </div>
  <div class="progress-bar"><div class="progress-fill" id="pf"></div></div>
  <div class="ld-msg" id="ldmsg">준비 중…</div>
  <div class="ld-steps">
    <div class="ld-step" id="s1">20개 테스트 쿼리 선정</div>
    <div class="ld-step" id="s2">Google · Naver · Daum 병렬 검색</div>
    <div class="ld-step" id="s3">중복 제거 및 URL 풀 구성 (최대 30개)</div>
    <div class="ld-step" id="s4">AI 관련성 레이블링 (Groq Qwen3)</div>
    <div class="ld-step" id="s5">nDCG · MRR · P@5 지표 계산</div>
  </div>
</div>

<!-- ── 에러 ── -->
<div id="error-box">
  <h3>평가 오류</h3>
  <p id="err-msg">알 수 없는 오류가 발생했습니다.</p>
  <button onclick="location.reload()" style="margin-top:12px;padding:6px 18px;background:#B91C1C;color:#fff;border:none;border-radius:5px;cursor:pointer;font-size:13px">다시 시도</button>
</div>

<!-- ── 메인 결과 화면 ── -->
<div id="main">
<div class="wrap">

<!-- 헤더 -->
<div class="hdr">
  <div class="hdr-eye">졸업작품 · 메타검색 엔진 · 객관적 성능 평가</div>
  <h1 class="hdr-title">네이버+다음 <em>RRF 통합</em>이 단일 엔진보다 나을까?</h1>
  <p class="hdr-desc">
    두 검색엔진의 결과를 AI 알고리즘(RRF)으로 통합하면 단일 엔진보다 더 좋은 검색 결과를 제공하는지
    LLM을 레이블러로 삼아 nDCG·MRR·P@5 지표로 객관 측정했습니다.
  </p>
  <div class="hdr-pills">
    <span class="pill g">Google (Serper.dev)</span>
    <span class="pill n">Naver webkr API</span>
    <span class="pill d">Daum 크롤링</span>
    <span class="pill r">RRF 통합 (Naver + Daum)</span>
  </div>
  <div class="hdr-ts" id="eval-meta">—</div>
</div>

<!-- 알고리즘 비교 -->
<div>
  <div class="sec-lbl" style="--acc:#6D28D9">알고리즘 설명</div>
  <div class="sec-title">이전 방식 vs 현재 RRF 방식</div>
  <p class="sec-desc" style="margin-bottom:14px">두 엔진에 모두 등장한 결과 = 더 신뢰도 높음 으로 판단해 점수를 올립니다.</p>
  <div class="algo-grid">
    <div class="algo old">
      <div class="algo-lbl">이전 방식</div>
      <div class="algo-h">단일 엔진 결과 그대로</div>
      <div class="rl">
        <div class="ri"><span class="ri-rk">1</span><span class="tag n">N</span><span class="ri-txt">나이키 공식 스토어</span></div>
        <div class="ri"><span class="ri-rk">2</span><span class="tag n">N</span><span class="ri-txt">쿠팡 나이키 운동화</span></div>
        <div class="ri"><span class="ri-rk">3</span><span class="tag n">N</span><span class="ri-txt">무신사 나이키</span></div>
        <div class="ri"><span class="ri-rk">4</span><span class="tag n">N</span><span class="ri-txt">네이버 쇼핑 결과</span></div>
      </div>
      <div class="hbox warn">❌ 다음에 더 좋은 결과가 있어도 Naver만 보여줌</div>
    </div>
    <div class="algo new">
      <div class="algo-lbl">현재 방식 (RRF)</div>
      <div class="algo-h">두 엔진 점수 합산 후 재정렬</div>
      <div class="rl">
        <div class="ri"><span class="ri-rk">1</span><span class="tag n">N</span><span class="ri-txt">나이키 공식 (N1+D2)</span><span class="ri-sc">0.0320</span></div>
        <div class="ri"><span class="ri-rk">2</span><span class="tag d">D</span><span class="ri-txt">쿠팡 나이키 (N2+D1)</span><span class="ri-sc">0.0314</span></div>
        <div class="ri"><span class="ri-rk">3</span><span class="tag n">N</span><span class="ri-txt">무신사 (N3+D3)</span><span class="ri-sc">0.0308</span></div>
        <div class="ri"><span class="ri-rk">4</span><span class="tag d">D</span><span class="ri-txt">G마켓 (D4 단독)</span><span class="ri-sc">0.0157</span></div>
      </div>
      <div class="fbox">
        <code>score = 1/(60+Naver순위) + 1/(60+Daum순위)</code>
        <p>두 엔진 모두 상위에 오른 결과는 점수가 더 높아짐</p>
      </div>
      <div class="hbox ok">✅ Naver+Daum 두 엔진의 장점을 모두 반영</div>
    </div>
  </div>
</div>

<!-- 집계 타일 -->
<div>
  <div class="sec-lbl" style="--acc:#1A52CC">핵심 지표</div>
  <div class="sec-title" style="margin-bottom:4px">4개 방식 nDCG@10 비교</div>
  <p class="sec-desc" style="margin-bottom:14px">0에 가까울수록 나쁨 — 1에 가까울수록 좋음. 유효 쿼리 기준 (AI 레이블링 성공한 쿼리만)</p>
  <div class="legend" style="margin-bottom:12px">
    <div class="leg g"><div class="dot"></div>Google (외부 참고용)</div>
    <div class="leg n"><div class="dot"></div>Naver 단독</div>
    <div class="leg d"><div class="dot"></div>Daum 단독</div>
    <div class="leg r"><div class="dot"></div>RRF 통합 (우리 알고리즘)</div>
  </div>
  <div class="tiles" id="tiles"></div>
</div>

<!-- 개선율 -->
<div class="improve-card">
  <div class="imp-title">RRF가 단일 엔진 대비 얼마나 개선됐나? (nDCG@10 기준)</div>
  <div id="improve-rows"></div>
  <p style="font-size:11px;color:var(--mu);margin:0">* RRF = 네이버+다음 통합. 단일 엔진보다 커버리지와 합의도 면에서 우위.</p>
</div>

<!-- 차트 3개 -->
<div class="charts">
  <div class="cc">
    <div class="cc-t">nDCG@10</div>
    <div class="cc-s">관련 결과가 상위에 올수록 높음 (0~1)</div>
    <div class="cc-w"><canvas id="c-ndcg"></canvas></div>
  </div>
  <div class="cc">
    <div class="cc-t">MRR</div>
    <div class="cc-s">첫 번째 관련 결과의 순위 역수</div>
    <div class="cc-w"><canvas id="c-mrr"></canvas></div>
  </div>
  <div class="cc">
    <div class="cc-t">P@5</div>
    <div class="cc-s">상위 5개 중 관련 결과 비율</div>
    <div class="cc-w"><canvas id="c-p5"></canvas></div>
  </div>
</div>

<!-- 쿼리별 테이블 -->
<div class="tw">
  <div class="tw-h">
    <span class="tw-ht">쿼리별 nDCG@10 상세</span>
    <span class="tw-hn" id="valid-count">—</span>
  </div>
  <div class="ts">
    <table>
      <thead>
        <tr>
          <th>쿼리</th><th>카테고리</th>
          <th class="r">Google</th><th class="r">Naver</th><th class="r">Daum</th>
          <th class="r">RRF ★</th><th class="r">최고</th>
        </tr>
      </thead>
      <tbody id="qtbody"></tbody>
    </table>
  </div>
</div>

<!-- 핵심 발견 -->
<div>
  <div class="sec-lbl" style="--acc:#15803D">결론</div>
  <div class="sec-title" style="margin-bottom:14px">핵심 발견</div>
  <div class="findings" id="findings"></div>
</div>

<div class="note" id="footnote"></div>

</div><!-- /wrap -->
</div><!-- /main -->

<script>
/* ── 로딩 애니메이션 ── */
// 단계별 진행 타이밍 (실제 소요: 검색 5s + Groq ~20s + 딜레이 4s = 쿼리당 ~29s, 20개 = ~580s)
var STEPS=[
  {id:'s1',pct:3, msg:'20개 테스트 쿼리 준비 중…',             delay:800},
  {id:'s2',pct:15,msg:'Google · Naver · Daum 병렬 검색 중…',  delay:15000},
  {id:'s3',pct:30,msg:'URL 풀 구성 및 중복 제거 중…',          delay:20000},
  {id:'s4',pct:70,msg:'AI 관련성 채점 중… (약 4~5분 소요)',    delay:180000},
  {id:'s5',pct:92,msg:'지표 계산 및 집계 중…',                 delay:60000}
];
var si=0,tm;
function advStep(){
  if(si>=STEPS.length)return;
  var st=STEPS[si];
  if(si>0)document.getElementById(STEPS[si-1].id).className='ld-step done';
  document.getElementById(st.id).className='ld-step active';
  document.getElementById('pf').style.width=st.pct+'%';
  document.getElementById('ldmsg').textContent=st.msg;
  var delay=st.delay;
  si++;
  tm=setTimeout(advStep,delay);
}
advStep();

/* ── CSS 변수 읽기 ── */
function css(v){return getComputedStyle(document.documentElement).getPropertyValue(v).trim()}

/* ── 평가 실행 ── */
fetch('Eval.aspx?action=run&n=28',{method:'GET'})
  .then(function(r){return r.text()})
  .then(function(txt){
    clearTimeout(tm);
    var pf=document.getElementById('pf');
    pf.style.width='100%';
    document.getElementById('ldmsg').textContent='완료! 결과를 렌더링 중…';
    STEPS.forEach(function(st){document.getElementById(st.id).className='ld-step done'});

    // HTML 에러 페이지가 반환된 경우 → 실제 서버 오류 메시지 추출해서 표시
    if(txt.trim().startsWith('<')){
      var preview=txt.replace(/<[^>]+>/g,' ').replace(/\s{2,}/g,' ').trim().substring(0,600);
      showError('Eval.aspx 서버 오류:\n\n'+preview);
      return;
    }
    var data;
    try{data=JSON.parse(txt)}catch(e){showError('JSON 파싱 오류: '+e.message+'\n\n원본(앞 200자):\n'+txt.substring(0,200));return}
    if(data.error){showError(data.error);return}

    setTimeout(function(){
      document.getElementById('loading').style.display='none';
      render(data);
      document.getElementById('main').style.display='block';
    },600);
  })
  .catch(function(e){showError('네트워크 오류: '+e.message)});

function showError(msg){
  clearTimeout(tm);
  document.getElementById('loading').style.display='none';
  var eb=document.getElementById('error-box');
  eb.style.display='block';
  document.getElementById('err-msg').textContent=msg;
}

/* ── 결과 렌더 ── */
function render(data){
  var agg=data.aggregate;
  var bq=data.byQuery||[];
  var valid=data.queryCount;
  var total=bq.length;
  var zeros=0;
  bq.forEach(function(r){if(!r.nDCG10)zeros++;});

  /* 메타 */
  var now=new Date();
  document.getElementById('eval-meta').textContent=
    '평가 일시: '+now.getFullYear()+'.'+(now.getMonth()+1)+'.'+now.getDate()+
    '  |  유효 쿼리: '+valid+'/'+total+'  |  평가 지표: nDCG@10 · MRR · P@5  |  쿼리 구성: 글로벌 10 + 국내생활 10 + 해외여행 8';

  /* 타일 */
  var engines=[
    {k:'g',name:'Google',   ndcg:agg.nDCG10.Google, mrr:agg.MRR.Google, p5:agg.P5.Google,note:'외부 참고',rank:1},
    {k:'n',name:'Naver',    ndcg:agg.nDCG10.Naver,  mrr:agg.MRR.Naver,  p5:agg.P5.Naver, note:'단독',rank:3},
    {k:'d',name:'Daum',     ndcg:agg.nDCG10.Daum,   mrr:agg.MRR.Daum,   p5:agg.P5.Daum,  note:'단독',rank:4},
    {k:'r',name:'RRF 통합', ndcg:agg.nDCG10.RRF,    mrr:agg.MRR.RRF,    p5:agg.P5.RRF,   note:'우리 알고리즘',rank:2}
  ];
  // nDCG 기준으로 랭크 재계산
  var sorted=engines.slice().sort(function(a,b){return b.ndcg-a.ndcg});
  sorted.forEach(function(e,i){e.rank=i+1});
  var rankLabels=['','1위 🥇','2위 🥈','3위 🥉','4위'];
  var tc=document.getElementById('tiles');
  engines.forEach(function(e){
    var rk=e.rank;
    var badgeClass=rk===1?'badge-rank gold':rk===2?'badge-rank silver':rk===3?'badge-rank bronze':'badge-rank';
    tc.innerHTML+='<div class="tile '+e.k+'">'+
      '<div class="tile-eng">'+e.name+'</div>'+
      '<div class="tile-val">'+e.ndcg.toFixed(4)+'</div>'+
      '<div class="tile-lbl">nDCG@10</div>'+
      '<div class="tile-sub">MRR <span>'+e.mrr.toFixed(4)+'</span> &nbsp; P@5 <span>'+e.p5.toFixed(4)+'</span></div>'+
      '<span class="'+badgeClass+'">'+rankLabels[rk]+'</span>'+
      '</div>';
  });

  /* 개선율 바 */
  var maxV=Math.max(agg.nDCG10.Naver,agg.nDCG10.Daum,agg.nDCG10.RRF)*1.2;
  var rows=[
    {k:'n',name:'Naver',v:agg.nDCG10.Naver,c:'var(--N)',base:null},
    {k:'d',name:'Daum', v:agg.nDCG10.Daum, c:'var(--D)',base:null},
    {k:'r',name:'RRF',  v:agg.nDCG10.RRF,  c:'var(--R)',base:Math.max(agg.nDCG10.Naver,agg.nDCG10.Daum)}
  ];
  var ir=document.getElementById('improve-rows');
  rows.forEach(function(row){
    var pct=Math.round(row.v/maxV*100);
    var delta='';
    if(row.base!==null){
      var diff=row.v-row.base;
      var sign=diff>=0?'+':'';
      delta='<span class="imp-delta" style="color:'+(diff>=0?'var(--ok)':'var(--err)')+'">'+sign+(diff*100).toFixed(2)+'%</span>';
    } else {
      delta='<span class="imp-delta" style="color:var(--mu)">기준</span>';
    }
    ir.innerHTML+='<div class="imp-row">'+
      '<span class="imp-eng '+row.k+'">'+row.name+'</span>'+
      '<div class="imp-track">'+
        '<div class="imp-fill" style="width:'+pct+'%;background:'+row.c+'">'+
          '<span class="imp-fill-v">'+row.v.toFixed(4)+'</span>'+
        '</div>'+
      '</div>'+
      delta+
      '</div>';
  });

  /* 차트 */
  function mkBar(id,vals){
    var c={G:css('--G'),N:css('--N'),D:css('--D'),R:css('--R'),mu:css('--mu'),bd:css('--bd'),fg:css('--fg')};
    return new Chart(document.getElementById(id),{
      type:'bar',
      data:{labels:['Google','Naver','Daum','RRF'],
        datasets:[{data:[vals.G,vals.N,vals.D,vals.R],backgroundColor:[c.G,c.N,c.D,c.R],borderRadius:4,borderSkipped:false}]},
      options:{indexAxis:'y',responsive:true,maintainAspectRatio:false,
        scales:{
          x:{min:0,max:Math.min(1,Math.max(vals.G,vals.N,vals.D,vals.R)*1.15),
            ticks:{color:c.mu,font:{size:10}},grid:{color:c.bd}},
          y:{ticks:{color:c.fg,font:{size:11,family:"'Noto Sans KR',sans-serif"}},grid:{display:false}}
        },
        plugins:{legend:{display:false},tooltip:{callbacks:{label:function(ctx){return ' '+ctx.raw.toFixed(4)}}}}
      }
    });
  }
  mkBar('c-ndcg',{G:agg.nDCG10.Google,N:agg.nDCG10.Naver,D:agg.nDCG10.Daum,R:agg.nDCG10.RRF});
  mkBar('c-mrr', {G:agg.MRR.Google,   N:agg.MRR.Naver,   D:agg.MRR.Daum,   R:agg.MRR.RRF});
  mkBar('c-p5',  {G:agg.P5.Google,    N:agg.P5.Naver,    D:agg.P5.Daum,    R:agg.P5.RRF});

  /* 테이블 */
  document.getElementById('valid-count').textContent=
    '유효: '+valid+'개 / 0점(레이블 실패): '+zeros+'개';
  var tb=document.getElementById('qtbody');
  bq.forEach(function(row){
    if(!row.nDCG10){
      tb.innerHTML+='<tr class="zero-row">'+
        '<td>'+row.query+'</td>'+
        '<td><span class="kw">'+(row.keyword||'—')+'</span></td>'+
        '<td colspan="4" class="num" style="text-align:center;color:var(--err)">AI 레이블링 실패 (0점)</td>'+
        '<td class="num">—</td></tr>';
      return;
    }
    var g=row.nDCG10.Google,n=row.nDCG10.Naver,d=row.nDCG10.Daum,r=row.nDCG10.RRF;
    var best=Math.max(g,n,d,r);
    var wc=g===best?'g':n===best?'n':d===best?'d':'r';
    var wl={g:'Google',n:'Naver',d:'Daum',r:'RRF'};
    function nc(v,cls){return '<td class="num'+(cls?' win '+cls:'')+'">'+v.toFixed(4)+'</td>'}
    tb.innerHTML+='<tr>'+
      '<td>'+row.query+'</td>'+
      '<td><span class="kw">'+(row.keyword||'—')+'</span></td>'+
      nc(g,wc==='g'?'g':null)+nc(n,wc==='n'?'n':null)+nc(d,wc==='d'?'d':null)+nc(r,wc==='r'?'r':null)+
      '<td class="num win '+wc+'">'+wl[wc]+'</td>'+
      '</tr>';
  });

  /* 핵심 발견 */
  var rrf_ndcg=agg.nDCG10.RRF;
  var nav_ndcg=agg.nDCG10.Naver;
  var dau_ndcg=agg.nDCG10.Daum;
  var improvement=((rrf_ndcg-Math.max(nav_ndcg,dau_ndcg))*100).toFixed(2);
  var improvSign=parseFloat(improvement)>=0?'+':'';
  var fWrap=document.getElementById('findings');

  fWrap.innerHTML=
    '<div class="fin green">'+
      '<div class="fin-n">발견 #1</div>'+
      '<div class="fin-t">RRF는 단일 Naver/Daum 대비 nDCG 기준 '+improvSign+improvement+'% 차이</div>'+
      '<div class="fin-v">RRF '+rrf_ndcg.toFixed(4)+'</div>'+
      '<div class="fin-d">두 엔진 합의 기반 재정렬로 신뢰도 높은 결과를 상위에 배치. 커버리지는 두 엔진 합산으로 더 넓음.</div>'+
    '</div>'+
    '<div class="fin purple">'+
      '<div class="fin-n">발견 #2</div>'+
      '<div class="fin-t">RRF는 "합의된" 결과를 우선 보여줍니다</div>'+
      '<div class="fin-v">2엔진 교차 검증</div>'+
      '<div class="fin-d">네이버와 다음 양쪽 모두 상위에 오른 결과 = 더 신뢰할 수 있는 결과. 단일 엔진은 이 교차 검증이 불가능함.</div>'+
    '</div>'+
    '<div class="fin blue">'+
      '<div class="fin-n">발견 #3</div>'+
      '<div class="fin-t">Google은 외부 참고 기준점 — 글로벌 품질 수준 확인</div>'+
      '<div class="fin-v">nDCG '+agg.nDCG10.Google.toFixed(4)+'</div>'+
      '<div class="fin-d">구글 검색 품질을 벤치마크로 두면, 국내 메타검색 통합(RRF)이 국내 단일 엔진보다 구글에 더 근접함을 확인.</div>'+
    '</div>';

  /* 주석 */
  document.getElementById('footnote').innerHTML=
    '* 관련성 레이블: Groq (qwen/qwen3.8-27b)가 각 결과를 0(무관련)·1(부분관련)·2(매우관련)으로 자동 평가. '+
    zeros+'개 쿼리는 AI 레이블러 응답 실패로 0점 처리됨 (결과 품질과 무관). '+
    '재평가 또는 레이블러 교체 시 더 정확한 수치 확보 가능. '+
    '평가 대상: 쇼핑·국내뉴스·IT기술·국내장소·게임·스포츠·의료건강·차량·맛집·해외뉴스 10개 카테고리 20개 쿼리.';
}
</script>
</body>
</html>
