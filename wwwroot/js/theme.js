// 다크 모드 버튼 (#themeBtn): 누르면 라이트↔다크를 바꾸고 이 브라우저에 기억
// 저장된 값을 그리기 전에 적용하는 부분은 각 페이지 <head>의 인라인 스크립트가 맡음 (깜빡임 방지)
(function () {
    var root = document.documentElement, btn = document.getElementById('themeBtn');
    if (!btn) return;
    // 직접 고른 값이 있으면 그것, 없으면 기기 설정
    var isDark = function () { var t = root.getAttribute('data-theme'); return t ? t === 'dark' : matchMedia('(prefers-color-scheme: dark)').matches; };
    var label = function () { btn.setAttribute('aria-label', isDark() ? '라이트 모드로 바꾸기' : '다크 모드로 바꾸기'); };
    label();
    btn.addEventListener('click', function () {
        var next = isDark() ? 'light' : 'dark';
        root.setAttribute('data-theme', next);
        try { localStorage.setItem('theme', next); } catch (e) { }
        label();
    });
})();
