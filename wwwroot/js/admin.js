/* ─────────────────────────────────────────────────────────────
   통합 관리자 페이지 (/admin) 스크립트
   - 메뉴는 주소 #이름으로 전환, 각 메뉴 데이터는 처음 열 때 한 번 불러옴
   - 사용자 입력(닉네임·댓글 등)은 모두 textContent로 넣음 (예전 화면은 닉네임을 onclick 코드 안에
     그대로 끼워 넣어서, 따옴표가 든 닉네임으로 관리자 화면에서 코드를 실행할 수 있었음)
   - 아스테라 메뉴는 게임 데이터가 커서 열 때만 /js/admin-poke.js 와 게임 데이터 스크립트를 불러옴
   ───────────────────────────────────────────────────────────── */
(function () {
    'use strict';
    var $ = function (id) { return document.getElementById(id); };
    var PANES = ['members', 'connections', 'stats', 'usage', 'ratings', 'drive', 'poke'];
    var loaded = {};

    // ── 작은 도구들 ──
    function el(tag, text, cls) { var e = document.createElement(tag); if (text != null) e.textContent = text; if (cls) e.className = cls; return e; }
    function td(text, cls) { return el('td', text, cls); }
    function btn(text, cls, onClick) { var b = el('button', text, 'btn sm' + (cls ? ' ' + cls : '')); b.type = 'button'; b.addEventListener('click', onClick); return b; }
    function emptyRow(tbody, cols, text) { tbody.textContent = ''; var tr = el('tr'), c = td(text, 'empty'); c.colSpan = cols; tr.appendChild(c); tbody.appendChild(tr); }
    function getJson(url) { return fetch(url, { credentials: 'same-origin', cache: 'no-store' }).then(function (r) { return r.json(); }); }
    function postJson(url, body) {
        return fetch(url, { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) })
            .then(function (r) { return r.json(); });
    }
    function n(v) { return (Number(v) || 0).toLocaleString('ko-KR'); }
    function css(name) { return getComputedStyle(document.documentElement).getPropertyValue(name).trim(); }
    function loadScript(src) {
        return new Promise(function (ok, fail) { var s = document.createElement('script'); s.src = src; s.onload = ok; s.onerror = fail; document.body.appendChild(s); });
    }

    // ── 메뉴 전환 (#이름) ──
    function show() {
        var name = location.hash.slice(1);
        if (PANES.indexOf(name) < 0) name = 'members';
        PANES.forEach(function (p) { $('pane-' + p).hidden = p !== name; });
        document.querySelectorAll('.adm-nav a').forEach(function (a) {
            if (a.getAttribute('href') === '#' + name) a.setAttribute('aria-current', 'page'); else a.removeAttribute('aria-current');
        });
        if (!loaded[name]) { loaded[name] = true; LOADERS[name](); }
    }
    window.addEventListener('hashchange', show);

    // ═══ 회원 ═══
    var members = [];
    function loadMembers() {
        getJson('/api/admin/members').then(function (d) {
            if (d.error) throw 0;
            members = d.members || [];
            $('memberCount').textContent = '총 ' + n(d.totalMembers) + '명';
            renderMembers();
        }).catch(function () { emptyRow($('memberRows'), 7, '회원 목록을 불러오지 못했습니다.'); });
    }
    function renderMembers() {
        var q = $('memberQ').value.trim().toLowerCase(), tb = $('memberRows');
        var rows = members.filter(function (m) { return !q || (m.userid + ' ' + m.nickname).toLowerCase().indexOf(q) >= 0; });
        if (!rows.length) { emptyRow(tb, 7, q ? '검색 결과가 없습니다.' : '회원이 없습니다.'); return; }
        tb.textContent = '';
        rows.forEach(function (m) {
            var tr = el('tr');
            tr.appendChild(td(m.userid, 'mono'));
            var nick = td(m.nickname); nick.style.fontWeight = '700'; tr.appendChild(nick);
            var st = td(); st.appendChild(el('span', m.status === 'true' ? '활성' : '비활성', 'st ' + (m.status === 'true' ? 'ok' : 'bad'))); tr.appendChild(st);
            var role = td(); role.appendChild(el('span', m.isAdmin ? '관리자' : '일반', 'st' + (m.isAdmin ? ' role' : '')));
            role.appendChild(document.createTextNode(' '));
            role.appendChild(btn(m.isAdmin ? '해제' : '부여', '', function () { toggleAdmin(m); }));
            tr.appendChild(role);
            tr.appendChild(td(m.created_at, 'muted hide-sm'));
            tr.appendChild(td(m.last_login, 'muted hide-sm'));
            var act = td(); act.className = 'num'; act.appendChild(btn('탈퇴', 'danger', function () { deleteMember(m, tr); })); tr.appendChild(act);
            tb.appendChild(tr);
        });
    }
    function toggleAdmin(m) {
        var action = m.isAdmin ? '해제' : '부여';
        if (!confirm('[' + m.nickname + '] (' + m.userid + ') 회원의 관리자 권한을 ' + action + '할까요?')) return;
        postJson('/api/admin/members/toggle-admin', { userid: m.userid }).then(function (d) {
            if (d.result === 'ok') { m.isAdmin = !m.isAdmin; renderMembers(); }
            else alert(d.result === 'cannot_change_self' ? '본인의 관리자 권한은 바꿀 수 없습니다.' : '바꾸지 못했습니다: ' + (d.error || d.result));
        }).catch(function () { alert('서버에 연결하지 못했습니다.'); });
    }
    function deleteMember(m, tr) {
        if (!confirm('[' + m.nickname + '] (' + m.userid + ') 회원을 탈퇴시킬까요?\n\n검색기록·별점·북마크·게시글이 모두 삭제되고, 같은 이메일로 다시 가입할 수 있게 됩니다.')) return;
        if (!confirm('정말 삭제할까요? 되돌릴 수 없습니다.')) return;
        postJson('/api/admin/members/delete', { userid: m.userid }).then(function (d) {
            if (d.result === 'ok') { members = members.filter(function (x) { return x !== m; }); tr.remove(); }
            else alert(d.result === 'cannot_delete_self' ? '관리자 본인은 삭제할 수 없습니다.' : '삭제하지 못했습니다: ' + (d.error || d.result));
        }).catch(function () { alert('서버에 연결하지 못했습니다.'); });
    }
    $('memberQ').addEventListener('input', renderMembers);

    // ═══ 접속 현황 ═══
    function loadConnections() {
        getJson('/api/admin/connections').then(function (d) {
            if (d.error) throw 0;
            $('connCount').textContent = n(d.count) + '명';
            var tb = $('connRows'), list = d.connections || [];
            if (!list.length) { emptyRow(tb, 4, '지금 접속한 사람이 없습니다.'); return; }
            tb.textContent = '';
            list.forEach(function (c) {
                var tr = el('tr');
                tr.appendChild(td(c.ip, 'mono'));
                tr.appendChild(td(c.nickname, c.userid !== '-' ? '' : 'muted'));
                tr.appendChild(td(c.userid, 'mono'));
                tr.appendChild(td(c.lastSeen, 'muted'));
                tb.appendChild(tr);
            });
        }).catch(function () { emptyRow($('connRows'), 4, '접속 현황을 불러오지 못했습니다.'); });
    }
    $('connReload').addEventListener('click', loadConnections);

    // ═══ 통계 (차트는 Chart.js를 이 메뉴를 열 때만 불러옴) ═══
    function loadStats() {
        Promise.all([getJson('/api/admin/stats'), loadScript('https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js')]).then(function (res) {
            var d = res[0]; if (d.error) throw 0;
            $('st-members').textContent = n(d.totals.members);
            $('st-searches').textContent = n(d.totals.searches);
            $('st-posts').textContent = n(d.totals.posts);
            $('st-bookmarks').textContent = n(d.totals.bookmarks);
            // 한 계열씩이라 범례 없음, 색은 페이지 토큰에서 (라이트/다크 모두 맞게)
            var ink = css('--ink'), accent = css('--accent'), sub = css('--ink-3'), rule = css('--rule');
            Chart.defaults.font.family = getComputedStyle(document.body).fontFamily;
            function draw(id, rows, type, color) {
                if (!rows || !rows.length) return;
                new Chart($(id), {
                    type: type,
                    data: { labels: rows.map(function (r) { return r.day; }), datasets: [{ data: rows.map(function (r) { return r.count; }),
                        backgroundColor: type === 'bar' ? color : 'transparent', borderColor: color, borderWidth: 2, borderRadius: 4, tension: .3, pointRadius: 0, pointHitRadius: 10 }] },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } },
                        scales: { x: { ticks: { color: sub, font: { size: 10 }, maxRotation: 0, autoSkip: true, maxTicksLimit: 8 }, grid: { display: false }, border: { color: rule } },
                                  y: { beginAtZero: true, ticks: { color: sub, precision: 0 }, grid: { color: rule }, border: { display: false } } } }
                });
            }
            draw('chartSignups', d.dailySignups, 'bar', ink);
            draw('chartSearches', d.dailySearches, 'line', accent);
            draw('chartPosts', d.dailyPosts, 'bar', ink);
        }).catch(function () { $('st-members').textContent = '불러오지 못함'; });
    }

    // ═══ API 사용량 ═══
    function loadUsage() {
        getJson('/api/admin/usage').then(function (d) {
            if (d.error) throw 0;
            var L = d.limits || {};
            $('usageLimits').textContent = '무료 한도 · Google(Serper) ' + L.serper + ' · Brave ' + L.brave + ' · Naver ' + L.naver + ' · Daum(Kakao) ' + L.kakao;
            var tb = $('usageRows'), list = d.usage || [];
            if (!list.length) { emptyRow(tb, 4, '아직 기록이 없습니다.'); return; }
            tb.textContent = '';
            list.forEach(function (u) {
                var tr = el('tr'), name = td(u.engine); name.style.fontWeight = '700';
                tr.appendChild(name); tr.appendChild(td(n(u.today), 'num')); tr.appendChild(td(n(u.monthly), 'num')); tr.appendChild(td(n(u.total), 'num'));
                tb.appendChild(tr);
            });
        }).catch(function () { emptyRow($('usageRows'), 4, '사용량을 불러오지 못했습니다.'); });
    }

    // ═══ 별점 평가 ═══
    function loadRatings() {
        var list = $('ratingList');
        getJson('/api/admin/ratings').then(function (d) {
            if (d.error) throw 0;
            $('avgRating').textContent = d.avgRating + ' / 5';
            $('totalCount').textContent = n(d.totalCount) + '건';
            list.textContent = '';
            if (!d.ratings || !d.ratings.length) { list.appendChild(el('p', '아직 평가가 없습니다.', 'empty')); return; }
            d.ratings.forEach(function (r) {
                var row = el('div', null, 'ritem');
                var stars = el('span', '★★★★★'.slice(0, r.rating), 'stars'); stars.appendChild(el('span', '★★★★★'.slice(0, 5 - r.rating), 'off'));
                stars.setAttribute('aria-label', '별점 ' + r.rating + '점'); row.appendChild(stars);
                var body = el('div'), who = el('div', r.nickname + ' ', 'who'); who.appendChild(el('small', '(' + r.userid + ')'));
                body.appendChild(who); if (r.comment) body.appendChild(el('p', r.comment)); body.appendChild(el('time', r.created_at));
                row.appendChild(body);
                row.appendChild(btn('삭제', 'danger', function () {
                    if (!confirm('이 평가를 삭제할까요?')) return;
                    postJson('/api/admin/ratings/delete', { id: r.id }).then(function (x) { if (x.result === 'ok') row.remove(); else alert('삭제하지 못했습니다.'); });
                }));
                list.appendChild(row);
            });
        }).catch(function () { list.textContent = ''; list.appendChild(el('p', '평가를 불러오지 못했습니다.', 'empty')); });
    }

    // ═══ Cloud Drive (회원 드라이브 열람·삭제) ═══
    // 관리자 본인의 사이트 세션으로 드라이브 토큰을 받아서 씀 (서버가 관리자 여부를 다시 확인)
    var DT = '', QUOTA = 5 * 1024 * 1024 * 1024, cur = null;
    function size(b) { if (!b) return '0 B'; var u = ['B', 'KB', 'MB', 'GB', 'TB'], i = Math.floor(Math.log(b) / Math.log(1024)); return (b / Math.pow(1024, i)).toFixed(i ? 1 : 0) + ' ' + u[i]; }
    function dget(url) {
        return fetch(url, { headers: { Authorization: 'Bearer ' + DT }, credentials: 'same-origin', cache: 'no-store' })
            .then(function (r) { if (!r.ok) throw new Error(String(r.status)); return r.json(); });
    }
    function dfail() { $('dmsg').hidden = false; $('dmsg').textContent = '드라이브 정보를 불러오지 못했습니다. 잠시 후 새로고침해 주세요.'; }
    function loadDrive() {
        fetch('/api/drive/sso', { method: 'POST', credentials: 'same-origin' }).then(function (r) { if (!r.ok) throw 0; return r.json(); })
            .then(function (j) { DT = j.token; driveUsers(); }).catch(dfail);
    }
    function driveUsers() {
        dget('/api/drive/admin/users').then(function (d) {
            QUOTA = d.quota || QUOTA;
            var tb = $('dUserRows'); tb.textContent = '';
            d.users.forEach(function (u) {
                var tr = el('tr', null, 'pick'); tr.tabIndex = 0;
                var id = td(u.username); if (u.isAdmin) { id.appendChild(document.createTextNode(' ')); id.appendChild(el('span', '관리자', 'st role')); }
                tr.appendChild(id);
                tr.appendChild(td(u.nickname || '—', 'hide-sm'));
                var use = td(), bar = el('span', null, 'bar'), fill = el('i');
                var lim = u.quotaBytes || QUOTA;   // 따로 정한 한도가 없으면 기본값
                var pct = u.isAdmin ? 0 : Math.min(100, u.usage / lim * 100);
                fill.style.width = pct + '%'; if (pct >= 95) fill.className = 'full';
                bar.appendChild(fill); use.appendChild(bar);
                var useText = document.createTextNode(size(u.usage) + (u.isAdmin ? ' (무제한)' : ' / ' + size(lim)));
                use.appendChild(useText);
                tr.appendChild(use);
                tr.appendChild(td(n(u.files), 'num'));
                tr.appendChild(quotaCell(u, function (newLim) {   // 한도를 바꾸면 사용량 막대·글자도 바로 갱신
                    var p2 = Math.min(100, u.usage / newLim * 100); fill.style.width = p2 + '%'; fill.className = p2 >= 95 ? 'full' : '';
                    useText.textContent = size(u.usage) + ' / ' + size(newLim);
                }));
                var open = function () { cur = { id: u.id, username: u.username }; driveFolder(null); };
                tr.addEventListener('click', open);
                tr.addEventListener('keydown', function (e) { if (e.key === 'Enter') open(); });
                tb.appendChild(tr);
            });
            $('dmsg').hidden = true; $('dFiles').hidden = true; $('dUsers').hidden = false;
        }).catch(dfail);
    }
    // 회원별 한도 칸: 숫자(GB) 입력 + 저장. 비우고 저장하면 기본 5GB로. 관리자는 무제한이라 칸 없음
    function quotaCell(u, onSaved) {
        var c = td(); if (u.isAdmin) { c.textContent = '—'; return c; }
        var box = el('div', null, 'act'); box.style.justifyContent = 'flex-start';
        var inp = el('input'); inp.type = 'number'; inp.min = '1'; inp.max = '1000'; inp.step = '1'; inp.style.width = '76px';
        inp.placeholder = String(Math.round(QUOTA / 1073741824)); inp.setAttribute('aria-label', u.username + ' 한도(GB)');
        if (u.quotaBytes) inp.value = Math.round(u.quotaBytes / 1073741824);
        var save = btn('저장', '', function () {
            var v = inp.value.trim(), gb = v === '' ? null : Number(v);
            if (gb !== null && (!Number.isInteger(gb) || gb < 1 || gb > 1000)) { alert('1~1000 사이 정수(GB)로 입력하세요. 비우면 기본 5GB입니다.'); return; }
            if (gb !== null && gb * 1073741824 < u.usage && !confirm('지금 사용량(' + size(u.usage) + ')보다 작습니다. 이 회원은 파일을 지울 때까지 더 올릴 수 없게 됩니다. 그래도 바꿀까요?')) return;
            save.disabled = true;
            fetch('/api/drive/admin/quota', { method: 'POST', credentials: 'same-origin', headers: { Authorization: 'Bearer ' + DT, 'Content-Type': 'application/json' }, body: JSON.stringify({ user: u.id, gb: gb }) })
                .then(function (r) { return r.json().then(function (j) { if (!r.ok || !j.ok) throw new Error(j.error || '실패'); return j; }); })
                .then(function (j) { u.quotaBytes = gb === null ? null : j.quota; onSaved(j.quota); save.textContent = '저장됨'; setTimeout(function () { save.textContent = '저장'; save.disabled = false; }, 1200); })
                .catch(function (e) { alert('한도를 바꾸지 못했습니다. ' + e.message); save.disabled = false; });
        });
        box.appendChild(inp); box.appendChild(save); c.appendChild(box);
        // 행을 누르면 폴더가 열리므로, 이 칸 안의 클릭·키 입력은 행까지 전달하지 않음
        c.addEventListener('click', function (e) { e.stopPropagation(); });
        c.addEventListener('keydown', function (e) { e.stopPropagation(); if (e.key === 'Enter') save.click(); });
        return c;
    }
    function driveFolder(folderId) {
        dget('/api/drive/admin/list?user=' + cur.id + (folderId ? '&folder=' + folderId : '')).then(function (d) {
            var cr = $('dCrumbs'); cr.textContent = '';
            var all = el('button', '회원 목록', 'linkish'); all.type = 'button'; all.onclick = driveUsers; cr.appendChild(all);
            cr.appendChild(el('span', '/'));
            var root = el('button', d.username || cur.username, 'linkish'); root.type = 'button'; root.onclick = function () { driveFolder(null); }; cr.appendChild(root);
            d.breadcrumb.forEach(function (b) {
                cr.appendChild(el('span', '/'));
                var bb = el('button', b.name, 'linkish'); bb.type = 'button'; bb.onclick = function () { driveFolder(b.id); }; cr.appendChild(bb);
            });
            var tb = $('dFileRows'); tb.textContent = '';
            if (!d.files.length) emptyRow(tb, 4, '비어 있습니다.');
            d.files.forEach(function (f) {
                var tr = el('tr'), name = td();
                if (f.isFolder) { var fb = el('button', f.fileName + '/', 'linkish'); fb.type = 'button'; fb.onclick = function () { driveFolder(f.id); }; name.appendChild(fb); }
                else name.textContent = f.fileName;
                tr.appendChild(name);
                tr.appendChild(td(f.isFolder ? '—' : size(f.fileSize), 'num'));
                tr.appendChild(td(new Date(f.updatedAt).toLocaleDateString('ko-KR'), 'num hide-sm'));
                var act = td(), box = el('div', null, 'act');
                if (!f.isFolder) {
                    var dl = el('a', '내려받기', 'btn sm');
                    dl.href = '/api/drive/admin/download/' + cur.id + '/' + f.id + '?token=' + encodeURIComponent(DT);
                    box.appendChild(dl);
                }
                box.appendChild(btn('삭제', 'danger', function () {
                    if (!confirm('"' + f.fileName + '"' + (f.isFolder ? ' 폴더와 안의 파일을 모두' : '을(를)') + ' 지웁니다. 되돌릴 수 없습니다.')) return;
                    fetch('/api/drive/admin/delete/' + cur.id + '/' + f.id, { method: 'DELETE', headers: { Authorization: 'Bearer ' + DT }, credentials: 'same-origin' })
                        .then(function (r) { if (!r.ok) throw 0; tr.remove(); })
                        .catch(function () { alert('삭제하지 못했습니다.'); });
                }));
                act.appendChild(box); tr.appendChild(act);
                tb.appendChild(tr);
            });
            $('dUsers').hidden = true; $('dFiles').hidden = false;
        }).catch(dfail);
    }

    // ═══ 포켓몬스터 아스테라 — 게임 데이터(수 MB)를 이 메뉴를 열 때만 불러옴 ═══
    function loadPoke() {
        var b = document.body.getAttribute('data-bust') || '0';
        var files = ['gamedata.js', 'ko.js', 'ko_moves.js', 'ko_misc.js', 'admin_meta.js'].map(function (f) { return '/pokemon/game/' + f + '?b=' + b; });
        files.push('/js/admin-poke.js?b=' + b);
        // 순서대로 (뒤 파일이 앞 파일의 전역 데이터를 씀)
        files.reduce(function (p, src) { return p.then(function () { return loadScript(src); }); }, Promise.resolve())
            .then(function () { $('pokeLoading').hidden = true; $('pokeMain').hidden = false; $('pokeParcel').hidden = false; })
            .catch(function () { $('pokeLoading').textContent = '게임 데이터를 불러오지 못했습니다. 새로고침해 주세요.'; });
    }

    var LOADERS = { members: loadMembers, connections: loadConnections, stats: loadStats, usage: loadUsage, ratings: loadRatings, drive: loadDrive, poke: loadPoke };
    show();
})();
