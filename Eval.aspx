<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Eval.aspx.cs" Inherits="Eval" %>
<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>검색 품질 평가</title>
<style>
  body { font-family: 'Noto Sans KR', sans-serif; background:#f5f5f5; color:#1a1a1a; margin:0; padding:24px; }
  .wrap { max-width:700px; margin:0 auto; background:#fff; border-radius:8px; padding:32px; box-shadow:0 2px 8px rgba(0,0,0,.08); }
  h1 { font-size:20px; margin:0 0 6px; }
  p.sub { color:#666; font-size:13px; margin:0 0 28px; }
  label { font-size:14px; font-weight:600; display:block; margin-bottom:6px; }
  input[type=number] { width:80px; padding:6px 10px; border:1px solid #ddd; border-radius:4px; font-size:14px; }
  button { margin-left:12px; padding:7px 20px; background:#2d4a6e; color:#fff; border:none; border-radius:4px; font-size:14px; cursor:pointer; }
  button:disabled { background:#aaa; cursor:not-allowed; }
  #status { margin-top:16px; font-size:13px; color:#555; min-height:20px; }
  #out { margin-top:16px; width:100%; height:320px; font-size:12px; font-family:monospace; border:1px solid #ddd; border-radius:4px; padding:10px; resize:vertical; display:none; }
  .note { margin-top:16px; font-size:12px; color:#999; }
  .copy-btn { font-size:12px; padding:4px 12px; background:#eee; border:1px solid #ccc; border-radius:4px; cursor:pointer; margin-left:8px; }
</style>
</head>
<body>
<div class="wrap">
  <h1>검색 품질 평가</h1>
  <p class="sub">SearchHistory 상위 쿼리를 기준으로 Google / Naver / Daum / RRF 를 비교합니다.</p>

  <label for="nInput">평가할 쿼리 수</label>
  <div style="display:flex;align-items:center">
    <input id="nInput" type="number" value="20" min="3" max="50">
    <button id="runBtn" onclick="runEval()">평가 실행</button>
  </div>

  <div id="status"></div>

  <div id="outWrap" style="display:none">
    <div style="display:flex;align-items:center;margin-top:16px;margin-bottom:4px">
      <span style="font-size:13px;font-weight:600">결과 JSON</span>
      <button class="copy-btn" onclick="copyJson()">복사</button>
    </div>
    <textarea id="out" readonly></textarea>
  </div>

  <p class="note">쿼리당 약 3초 소요 (Groq 30 RPM 대응). 20개 기준 약 1분.</p>
</div>

<script>
function runEval() {
    var n   = document.getElementById('nInput').value;
    var btn = document.getElementById('runBtn');
    var st  = document.getElementById('status');
    var out = document.getElementById('out');

    btn.disabled = true;
    st.textContent = '평가 실행 중... (' + n + '개 쿼리, 약 ' + Math.ceil(n*3/60) + '분 소요)';
    document.getElementById('outWrap').style.display = 'none';

    fetch('Eval.aspx?action=run&n=' + encodeURIComponent(n))
        .then(function(r) { return r.text(); })
        .then(function(text) {
            try {
                // JSON 예쁘게 포매팅
                var parsed = JSON.parse(text);
                out.value = JSON.stringify(parsed, null, 2);
                document.getElementById('outWrap').style.display = 'block';
                st.textContent = '완료! ' + (parsed.queryCount || '?') + '개 쿼리 평가됨.';
            } catch(e) {
                out.value = text;
                document.getElementById('outWrap').style.display = 'block';
                st.textContent = '완료 (JSON 파싱 오류 - 원문 표시)';
            }
            btn.disabled = false;
        })
        .catch(function(err) {
            st.textContent = '오류: ' + err.message;
            btn.disabled = false;
        });
}

function copyJson() {
    var out = document.getElementById('out');
    if (navigator.clipboard) {
        navigator.clipboard.writeText(out.value).catch(function() {
            out.select(); document.execCommand('copy');
        });
    } else {
        out.select(); document.execCommand('copy');
    }
}
</script>
</body>
</html>
