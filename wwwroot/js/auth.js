// ─────────────────────────────────────────────────────────────
// domang 통합 계정 창: 로그인 / 회원가입(이메일 인증) / 아이디·비밀번호 찾기
//  - 창으로 열기:   DomangAuth.open('login' | 'register' | 'find')
//                  또는 data-auth="login|register|find" 가 붙은 링크·버튼을 누르면 열림
//  - 페이지에 넣기: DomangAuth.mount(요소, { onDone })   (/signin, /drive 처럼 창 없이)
//  - 로그인·가입에 성공하면 onDone(결과) 호출 (기본: 새로고침)
// 서버 API는 기존 것을 그대로 씀: /api/astera/login, /api/register/check-id,
//   /api/email/send-code, /api/email/verify-code, /api/account/register, /api/account/find-*
// ─────────────────────────────────────────────────────────────
(function () {
    // JSON POST 도우미 (같은 사이트 세션 쿠키 포함)
    function post(url, data) {
        return fetch(url, {
            method: 'POST', credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data || {})
        }).then(function (r) { return r.json(); });
    }
    var NET = '서버에 연결하지 못했습니다. 잠시 후 다시 시도해주세요.';
    // 이메일 발송 API 결과 코드 → 문구
    var MAIL_MSG = {
        rate_limited: '요청이 너무 잦습니다. 1분 후 다시 시도해주세요.',
        invalid_email: '이메일 주소를 확인해주세요.',
        duplicate_email: '이미 가입된 이메일입니다. 아이디 찾기를 이용해주세요.',
        not_found: '일치하는 계정이 없습니다.',
        invalid_input: '입력한 내용을 확인해주세요.',
        send_failed: '메일을 보내지 못했습니다. 잠시 후 다시 시도해주세요.'
    };
    var PW_RULE = '8자 이상, 영문·숫자·특수문자를 모두 넣어주세요.';
    var pwOk = function (s) { return s.length >= 8 && /[A-Za-z]/.test(s) && /[0-9]/.test(s) && /[^A-Za-z0-9]/.test(s); };

    // 화면 틀 (글자는 모두 고정 문구라 innerHTML로 넣어도 안전. 사용자 입력은 textContent로만 표시)
    var X = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><path d="M6 6l12 12M18 6 6 18"/></svg>';
    var NOTE = '<p class="da-note"><strong>domang 통합 계정</strong> 하나로 Meta Search · Cloud Drive · 포켓몬스터 아스테라를 모두 이용합니다. 어느 서비스에서 가입해도 같은 아이디예요.</p>';

    var VIEWS = {
        login:
            '<form data-form="login" novalidate>' +
            '<label>아이디<input name="uid" autocomplete="username" autocapitalize="off" spellcheck="false" required></label>' +
            '<label>비밀번호<input name="pw" type="password" autocomplete="current-password" required></label>' +
            '<p class="da-err" data-err></p>' +
            '<button class="da-btn" type="submit">로그인</button>' +
            '</form>' +
            '<div class="da-links"><button type="button" data-go="find">아이디·비밀번호 찾기</button><button type="button" data-go="register">처음이신가요? 회원가입</button></div>',
        register:
            '<form data-form="register" novalidate>' +
            '<label>아이디<input name="userid" autocomplete="username" autocapitalize="off" spellcheck="false" maxlength="20" required><span class="hint" data-hint="userid">영문 소문자·숫자·밑줄, 3~20자</span></label>' +
            '<label>비밀번호<input name="passwd" type="password" autocomplete="new-password" required><span class="hint" data-hint="passwd">' + PW_RULE + '</span></label>' +
            '<label>비밀번호 확인<input name="passwdConfirm" type="password" autocomplete="new-password" required></label>' +
            '<label>이름<input name="fullname" autocomplete="name" maxlength="20" required></label>' +
            '<label>닉네임<input name="nickname" autocomplete="nickname" maxlength="20" required></label>' +
            '<div class="row2"><label>이메일<input name="email" type="email" autocomplete="email" required></label><button class="da-btn line" type="button" data-act="send">인증코드 받기</button></div>' +
            '<div class="row2" data-code hidden><label>인증코드<input name="code" inputmode="numeric" autocomplete="one-time-code"></label><button class="da-btn line" type="button" data-act="verify">확인</button></div>' +
            '<span class="hint" data-hint="email"></span>' +
            '<p class="da-err" data-err></p>' +
            '<button class="da-btn" type="submit">가입하기</button>' +
            '</form>' +
            '<div class="da-links"><button type="button" data-go="login">이미 계정이 있어요</button></div>',
        find:
            '<div class="da-tabs" role="tablist"><button type="button" role="tab" data-find="id" aria-selected="true">아이디 찾기</button><button type="button" role="tab" data-find="pw" aria-selected="false">비밀번호 찾기</button></div>' +
            '<form data-form="find" novalidate>' +
            '<label data-only="pw" hidden>아이디<input name="userid" autocomplete="username" autocapitalize="off" spellcheck="false"></label>' +
            '<div class="row2"><label>가입한 이메일<input name="email" type="email" autocomplete="email"></label><button class="da-btn line" type="button" data-act="send">인증코드 받기</button></div>' +
            '<div class="row2" data-code hidden><label>인증코드<input name="code" inputmode="numeric" autocomplete="one-time-code"></label><button class="da-btn line" type="button" data-act="verify">확인</button></div>' +
            '<div data-newpw hidden style="display:grid;gap:12px">' +
            '<label>새 비밀번호<input name="newPw" type="password" autocomplete="new-password"><span class="hint">' + PW_RULE + '</span></label>' +
            '<label>새 비밀번호 확인<input name="newPw2" type="password" autocomplete="new-password"></label>' +
            '<button class="da-btn" type="submit">비밀번호 바꾸기</button></div>' +
            '<p class="da-ok" data-result hidden></p>' +
            '<p class="da-err" data-err></p>' +
            '</form>' +
            '<div class="da-links"><button type="button" data-go="login">로그인으로 돌아가기</button></div>'
    };
    var TITLES = { login: '로그인', register: '회원가입', find: '계정 찾기' };

    // 한 덩어리(창 또는 페이지 안)를 만들고 동작을 붙임
    function build(root, opts) {
        var done = opts.onDone || function () { location.reload(); };
        root.innerHTML =
            '<div class="da"><div class="da-head"><h2 data-title></h2>' + (opts.close ? '<button type="button" class="da-x" data-close aria-label="닫기">' + X + '</button>' : '') + '</div>' +
            NOTE + '<div data-body></div></div>';
        var body = root.querySelector('[data-body]'), title = root.querySelector('[data-title]');
        var findMode = 'id';

        function show(view) {
            title.textContent = TITLES[view];
            body.innerHTML = VIEWS[view];
            var f = body.querySelector('form');
            (view === 'login' ? loginForm : view === 'register' ? registerForm : findForm)(f);
            // 화면에 보이는 첫 입력칸에 커서
            var first = Array.prototype.find.call(body.querySelectorAll('input'), function (i) { return i.offsetParent !== null; });
            if (first) setTimeout(function () { first.focus(); }, 30);
        }
        root.addEventListener('click', function (e) {
            var go = e.target.closest('[data-go]');
            if (go) show(go.getAttribute('data-go'));
            if (e.target.closest('[data-close]') && opts.close) opts.close();
        });

        // ── 로그인 ──
        function loginForm(f) {
            var err = f.querySelector('[data-err]'), btn = f.querySelector('[type=submit]');
            f.addEventListener('submit', function (e) {
                e.preventDefault(); err.textContent = '';
                var id = f.uid.value.trim(), pw = f.pw.value;   // f.id는 폼 자체 id라서 입력칸 이름은 uid
                if (!id || !pw) { err.textContent = '아이디와 비밀번호를 입력해주세요.'; return; }
                btn.disabled = true;
                post('/api/astera/login', { id: id, pw: pw }).then(function (j) {
                    if (j.ok) return done(j);
                    err.textContent = j.error || '로그인에 실패했습니다.'; btn.disabled = false;
                }).catch(function () { err.textContent = NET; btn.disabled = false; });
            });
        }

        // ── 회원가입 (이메일 인증 → 가입 → 바로 로그인) ──
        function registerForm(f) {
            var err = f.querySelector('[data-err]'), btn = f.querySelector('[type=submit]');
            var hint = function (k) { return f.querySelector('[data-hint="' + k + '"]'); };
            var setHint = function (k, text, cls) { var h = hint(k); h.textContent = text; h.className = 'hint' + (cls ? ' ' + cls : ''); };
            var verified = '';   // 인증을 마친 이메일 (바꾸면 다시 인증)

            // 아이디 중복 확인 (입력을 마치면)
            f.userid.addEventListener('change', function () {
                var id = f.userid.value.trim().toLowerCase();
                if (!id) return;
                fetch('/api/register/check-id?id=' + encodeURIComponent(id), { credentials: 'same-origin' })
                    .then(function (r) { return r.json(); }).then(function (j) {
                        if (j.result === 'ok') setHint('userid', '사용할 수 있는 아이디입니다.', 'ok');
                        else if (j.result === 'duplicate') setHint('userid', '이미 사용 중인 아이디입니다.', 'bad');
                        else setHint('userid', '영문 소문자·숫자·밑줄로 3~20자를 입력해주세요.', 'bad');
                    }).catch(function () { });
            });
            f.passwd.addEventListener('input', function () {
                setHint('passwd', PW_RULE, f.passwd.value && !pwOk(f.passwd.value) ? 'bad' : (f.passwd.value ? 'ok' : ''));
            });
            f.email.addEventListener('input', function () { if (verified && f.email.value.trim().toLowerCase() !== verified) { verified = ''; setHint('email', '이메일이 바뀌어 다시 인증해야 합니다.', 'bad'); } });

            f.querySelector('[data-act=send]').addEventListener('click', function () {
                var b = this, email = f.email.value.trim();
                if (!/^\S+@\S+\.\S+$/.test(email)) { setHint('email', '이메일 주소를 확인해주세요.', 'bad'); return; }
                b.disabled = true; setHint('email', '보내는 중…');
                post('/api/email/send-code', { email: email }).then(function (j) {
                    b.disabled = false;
                    if (j.result === 'ok') { f.querySelector('[data-code]').hidden = false; f.code.focus(); setHint('email', email + ' 로 인증코드를 보냈습니다. 메일함(스팸함 포함)을 확인해주세요.', 'ok'); b.textContent = '다시 받기'; }
                    else setHint('email', MAIL_MSG[j.result] || '메일을 보내지 못했습니다.', 'bad');
                }).catch(function () { b.disabled = false; setHint('email', NET, 'bad'); });
            });
            f.querySelector('[data-act=verify]').addEventListener('click', function () {
                var email = f.email.value.trim(), code = f.code.value.trim();
                if (!code) { setHint('email', '받은 인증코드를 입력해주세요.', 'bad'); return; }
                post('/api/email/verify-code', { email: email, code: code }).then(function (j) {
                    if (j.result === 'ok') { verified = email.toLowerCase(); f.querySelector('[data-code]').hidden = true; setHint('email', '이메일 인증이 끝났습니다.', 'ok'); }
                    else setHint('email', '인증코드가 맞지 않거나 시간이 지났습니다.', 'bad');
                }).catch(function () { setHint('email', NET, 'bad'); });
            });

            f.addEventListener('submit', function (e) {
                e.preventDefault(); err.textContent = '';
                if (!f.userid.value.trim() || !f.passwd.value || !f.fullname.value.trim() || !f.nickname.value.trim()) { err.textContent = '모든 항목을 입력해주세요.'; return; }
                if (!pwOk(f.passwd.value)) { err.textContent = '비밀번호는 ' + PW_RULE; return; }
                if (f.passwd.value !== f.passwdConfirm.value) { err.textContent = '비밀번호 확인이 일치하지 않습니다.'; return; }
                if (!verified) { err.textContent = '이메일 인증을 먼저 완료해주세요.'; return; }
                btn.disabled = true;
                post('/api/account/register', {
                    userid: f.userid.value, passwd: f.passwd.value, passwdConfirm: f.passwdConfirm.value,
                    name: f.fullname.value, nickname: f.nickname.value, email: f.email.value
                }).then(function (j) {
                    if (j.ok) return done(j);
                    err.textContent = j.error || '가입에 실패했습니다.'; btn.disabled = false;
                }).catch(function () { err.textContent = NET; btn.disabled = false; });
            });
        }

        // ── 아이디·비밀번호 찾기 ──
        function findForm(f) {
            var err = f.querySelector('[data-err]'), result = f.querySelector('[data-result]');
            var codeRow = f.querySelector('[data-code]'), newpw = f.querySelector('[data-newpw]');
            var reset = function () { err.textContent = ''; result.hidden = true; codeRow.hidden = true; newpw.hidden = true; };
            body.querySelectorAll('[data-find]').forEach(function (t) {
                t.addEventListener('click', function () {
                    findMode = t.getAttribute('data-find');
                    body.querySelectorAll('[data-find]').forEach(function (o) { o.setAttribute('aria-selected', String(o === t)); });
                    f.querySelector('[data-only=pw]').hidden = findMode !== 'pw';
                    reset();
                });
            });
            f.querySelector('[data-act=send]').addEventListener('click', function () {
                var b = this; reset();
                var data = { email: f.email.value.trim() };
                if (findMode === 'pw') data.userid = f.userid.value.trim();
                if (!/^\S+@\S+\.\S+$/.test(data.email) || (findMode === 'pw' && !data.userid)) { err.textContent = findMode === 'pw' ? '아이디와 이메일을 입력해주세요.' : '가입한 이메일을 입력해주세요.'; return; }
                b.disabled = true;
                post(findMode === 'pw' ? '/api/account/find-pw' : '/api/account/find-id', data).then(function (j) {
                    b.disabled = false;
                    if (j.result === 'ok') { codeRow.hidden = false; f.code.focus(); result.hidden = false; result.textContent = '인증코드를 보냈습니다. 메일함(스팸함 포함)을 확인해주세요.'; }
                    else err.textContent = MAIL_MSG[j.result] || '요청을 처리하지 못했습니다.';
                }).catch(function () { b.disabled = false; err.textContent = NET; });
            });
            f.querySelector('[data-act=verify]').addEventListener('click', function () {
                err.textContent = '';
                var data = { email: f.email.value.trim(), code: f.code.value.trim() };
                post(findMode === 'pw' ? '/api/account/find-pw/verify' : '/api/account/find-id/verify', data).then(function (j) {
                    if (j.result !== 'ok') { err.textContent = j.result === 'not_found' ? '일치하는 계정이 없습니다.' : j.result === 'rate_limited' ? MAIL_MSG.rate_limited : '인증코드가 맞지 않거나 시간이 지났습니다. (5번 틀리면 새 코드를 받아야 합니다)'; return; }
                    codeRow.hidden = true; result.hidden = false;
                    if (findMode === 'id') {
                        // 보안상 아이디 일부는 가려서 보여 줌
                        result.textContent = '가입한 아이디: ';
                        var b = document.createElement('b'); b.textContent = j.userid; result.appendChild(b);
                    } else { result.textContent = '인증되었습니다. 새 비밀번호를 정해주세요.'; newpw.hidden = false; f.newPw.focus(); }
                }).catch(function () { err.textContent = NET; });
            });
            // 비밀번호 재설정
            f.addEventListener('submit', function (e) {
                e.preventDefault(); err.textContent = '';
                if (findMode !== 'pw' || newpw.hidden) return;
                if (!pwOk(f.newPw.value)) { err.textContent = '비밀번호는 ' + PW_RULE; return; }
                if (f.newPw.value !== f.newPw2.value) { err.textContent = '비밀번호 확인이 일치하지 않습니다.'; return; }
                post('/api/account/find-pw/reset', { userid: f.userid.value.trim(), newPw: f.newPw.value }).then(function (j) {
                    if (j.result === 'ok') { newpw.hidden = true; result.textContent = '비밀번호를 바꿨습니다. 새 비밀번호로 로그인해주세요.'; }
                    else err.textContent = j.result === 'weak_password' ? '비밀번호는 ' + PW_RULE : '처음부터 다시 시도해주세요. (인증이 만료되었거나 정보가 맞지 않습니다)';
                }).catch(function () { err.textContent = NET; });
            });
        }

        return show;
    }

    // ── 창(dialog)으로 열기 ──
    var dlg, showIn;
    function open(view) {
        if (!dlg) {
            dlg = document.createElement('dialog');
            dlg.className = 'da-dialog';
            dlg.setAttribute('aria-label', 'domang 계정');
            document.body.appendChild(dlg);
            showIn = build(dlg, { close: function () { dlg.close(); }, onDone: window.DomangAuthDone });
            // 바깥(어두운 부분)을 누르면 닫힘
            dlg.addEventListener('click', function (e) { if (e.target === dlg) dlg.close(); });
        }
        showIn(view || 'login');
        if (!dlg.open) dlg.showModal();
    }

    // data-auth 링크·버튼 → 창 열기 (자바스크립트가 없으면 원래 링크(/signin)로 이동)
    document.addEventListener('click', function (e) {
        var a = e.target.closest('[data-auth]');
        if (!a) return;
        e.preventDefault();
        open(a.getAttribute('data-auth'));
    });

    window.DomangAuth = {
        open: open,
        // 창 없이 페이지 안에 넣기
        mount: function (el, opts) { el.classList.add('da-inline'); build(el, opts || {})((opts && opts.view) || 'login'); }
    };
})();
