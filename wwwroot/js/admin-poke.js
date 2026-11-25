/* ─────────────────────────────────────────────────────────────
   통합 관리자 페이지(/admin)의 '포켓몬스터 아스테라' 메뉴 스크립트
   예전 /pokeadmin 페이지 스크립트를 그대로 옮김 (화면 모양만 admin.css로 바뀜)
   admin.js가 게임 데이터(gamedata.js, ko*.js, admin_meta.js)를 먼저 불러온 뒤 이 파일을 불러옴
   회원별 게임 기록(진행·돈·파티·박스·가방) 보기·수정 + 이상한 소포 보내기
   데이터 읽기/쓰기는 /api/astera/admin/* (서버에서 다시 관리자 확인)
   ───────────────────────────────────────────────────────────── */
'use strict';
// ── 게임 데이터 ──
var G = window.GAMEDATA, SP = G.species, MV = G.moves, IT = G.items, META = window.ASTERA_META;
var ASSET = '/pokemon/Pokemon Another Red_PWT_250821/';
var BADGES = META.badges.map(function (b) { return b[0]; });
// 주요 진행 플래그 (게임 코드의 Flags 이름)
var STORY_FLAGS = [['starter', '스타팅 받음'], ['pokedex', '도감 받음'], ['rival1', '라이벌 1차전'], ['rival2', '라이벌 2차전'], ['megaring', '메가링'], ['origin_done', '균열단 사건 해결'], ['champion', '챔피언'], ['gift_cleffa', '천문대 삐'], ['gift_lapras', '등대 라프라스']];

var $ = function (id) { return document.getElementById(id); };
function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
function spName(k) { var d = SP[k]; if (!d) return k; return d.name + (d.form && d.fname ? ' (' + d.fname + ')' : ''); }
function iconUrl(k, shiny) { return encodeURI(ASSET + 'Graphics/Pokemon/Icons' + (shiny ? ' shiny' : '') + '/' + k + '.png'); }
function icon(k, shiny, big) { return '<img class="ic' + (big ? ' big' : '') + '" loading="lazy" src="' + iconUrl(k, shiny) + '" onerror="this.onerror=null;this.src=\'' + iconUrl(String(k).replace(/_\d+$/, ''), false) + '\'" alt="">'; }
function mvName(id) { return MV[id] ? MV[id].name : id; }
function itName(id) { return IT[id] ? IT[id].name : id; }
function fmtTime(sec) { sec = +sec || 0; var h = Math.floor(sec / 3600), m = Math.floor(sec % 3600 / 60); return h + '시간 ' + m + '분'; }
function mapName(k) { return (META.maps && META.maps[k]) || k || '-'; }
function api(url, opt) { return fetch(url, Object.assign({ credentials: 'same-origin', cache: 'no-store' }, opt || {})); }

// ── 회원 목록 ──
var players = [], cur = null, save = null, dirty = false, selBox = null;
function loadPlayers() {
    api('/api/astera/admin/players').then(function (r) { return r.json(); }).then(function (list) { players = list; renderList(); })
        .catch(function () { $('plist').innerHTML = '<div class="empty">목록을 불러오지 못했습니다.</div>'; });
}
function renderList() {
    var q = $('q').value.trim().toLowerCase();
    var rows = players.filter(function (p) { return !q || (p.userId + ' ' + p.nick + ' ' + (p.name || '')).toLowerCase().indexOf(q) >= 0; });
    if (!rows.length) { $('plist').innerHTML = '<div class="empty">게임 기록이 있는 회원이 없습니다.</div>'; return; }
    $('plist').innerHTML = rows.map(function (p) {
        var party = (p.party || []).map(function (m) { return icon(m.s, m.sh); }).join('');
        var champ = p.flags && p.flags.champion ? ' <span class="tag y">챔피언</span>' : '';
        return '<div class="pl' + (cur === p.userId ? ' on' : '') + '" data-id="' + esc(p.userId) + '">' +
            '<div><span class="id">' + esc(p.userId) + '</span> <span class="meta">' + esc(p.nick) + ' · 주인공 ' + esc(p.name || '-') + champ + '</span>' +
            '<div class="meta">배지 ' + (p.badges || []).length + ' · ' + Number(p.money || 0).toLocaleString() + '원 · 포획 ' + p.caught + ' · 박스 ' + p.boxed + ' · ' + esc(mapName(p.map)) + ' · ' + fmtTime(p.playTime) + '</div></div>' +
            '<div class="r">' + new Date(p.updatedAt).toLocaleString('ko-KR', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' }) + '<br>마지막 저장</div>' +
            '<div class="icons">' + party + '</div></div>';
    }).join('');
}
$('plist').addEventListener('click', function (e) {
    var el = e.target.closest('.pl'); if (!el) return;
    if (dirty && !window.confirm('저장하지 않은 수정이 있습니다. 버리고 다른 회원을 볼까요?')) return;
    openPlayer(el.dataset.id);
});
$('q').addEventListener('input', renderList);
$('reload').addEventListener('click', loadPlayers);

// ── 상세 ──
function openPlayer(id) {
    cur = id; selBox = null; renderList();
    $('detailPanel').innerHTML = '<div class="empty">불러오는 중...</div>';
    api('/api/astera/admin/save/' + encodeURIComponent(id)).then(function (r) { if (!r.ok) throw 0; return r.json(); })
        .then(function (s) { save = s; save.boxes = save.boxes || []; save.bag = save.bag || {}; save.flags = save.flags || {}; save.badges = save.badges || []; setDirty(false); renderDetail(); })
        .catch(function () { $('detailPanel').innerHTML = '<div class="empty">기록을 불러오지 못했습니다.</div>'; });
}
function setDirty(v) { dirty = v; var m = $('saveMsg'); if (m) { m.textContent = v ? '저장하지 않은 수정이 있습니다.' : '수정 사항 없음'; m.className = 'msg' + (v ? ' dirty' : ''); } }

function monCard(m, where, i) {
    var moves = (m.mv || []).map(function (x) { return mvName(x.id); }).join(', ') || '(기술은 게임에서 자동 설정)';
    return '<div class="mon" data-w="' + where + '" data-i="' + i + '">' + icon(m.s, m.sh, true) +
        '<div><div class="nm">' + (m.sh ? '<span class="sh">★</span> ' : '') + esc(m.n || spName(m.s)) + '</div>' +
        '<div class="meta" style="color:var(--sub)">' + esc(m.s) + (m.it ? ' · ' + esc(itName(m.it)) : '') + (m.gm ? ' · 거다이맥스' : '') + '</div></div>' +
        '<div class="mv">' + esc(moves) + '</div>' +
        '<div class="ctl">Lv <input type="number" min="1" max="100" value="' + m.l + '" data-act="lv">' +
        '<label><input type="checkbox" data-act="shiny"' + (m.sh ? ' checked' : '') + '> 이로치</label>' +
        '<button data-act="heal">회복</button><button data-act="moves">기술</button>' +
        (where === 'party' ? '<button data-act="toBox">박스로</button>' : '<button data-act="toParty">파티로</button>') +
        '<button class="del" data-act="del">놓아주기</button></div></div>';
}

function renderDetail() {
    var s = save, p = players.find(function (x) { return x.userId === cur; }) || {};
    var badgeHtml = BADGES.map(function (b, i) { return '<label><input type="checkbox" data-badge="' + esc(b) + '"' + (s.badges.indexOf(b) >= 0 ? ' checked' : '') + '>' + (i + 1) + '. ' + esc(b) + ' <small style="color:var(--muted)">' + esc(META.badges[i][1]) + '</small></label>'; }).join('');
    var flagHtml = STORY_FLAGS.map(function (f) { return '<label><input type="checkbox" data-flag="' + f[0] + '"' + (s.flags[f[0]] ? ' checked' : '') + '>' + f[1] + '</label>'; }).join('');
    var caught = Object.keys(s.caught || {}).length, seen = Object.keys(s.seen || {}).length;
    var boxes = (s.boxes || []).map(function (b, bi) {
        return '<div class="boxrow"><div>박스 ' + (bi + 1) + ' <small>(' + b.length + '/30)</small></div><div class="boxgrid">' +
            b.map(function (m, mi) { var on = selBox && selBox[0] === bi && selBox[1] === mi; return '<span class="bm' + (on ? ' on' : '') + '" data-b="' + bi + '" data-i="' + mi + '" title="' + esc(spName(m.s)) + ' Lv.' + m.l + '">' + icon(m.s, m.sh) + (m.sh ? '<span class="s">★</span>' : '') + '<i>' + m.l + '</i></span>'; }).join('') +
            '</div></div>';
    }).join('');
    var selMon = selBox && s.boxes[selBox[0]] && s.boxes[selBox[0]][selBox[1]];
    var bag = Object.keys(s.bag).sort(function (a, b) { return itName(a).localeCompare(itName(b), 'ko'); }).map(function (id) {
        return '<tr><td>' + esc(itName(id)) + ' <small style="color:var(--muted)">' + esc(id) + '</small></td><td><input type="number" min="0" max="999" value="' + s.bag[id] + '" data-item="' + esc(id) + '"></td></tr>';
    }).join('');

    $('detailPanel').innerHTML =
        '<div class="ph"><b>' + esc(cur) + '</b><span class="meta" style="color:var(--sub)">' + esc(p.nick || '') + '</span><span class="sp"></span>' +
        '<span class="tag">위치: ' + esc(mapName(s.map)) + ' (' + s.x + ', ' + s.y + ')</span><span class="tag">플레이 ' + fmtTime(s.playTime) + '</span></div>' +
        '<div class="detail">' +
        // 기본 정보
        '<div class="sec"><h3>기본 정보</h3><div class="kv">' +
        '<label>주인공 이름<input id="f-name" value="' + esc(s.name) + '"></label>' +
        '<label>성별<select id="f-gender"><option value="M"' + (s.gender === 'M' ? ' selected' : '') + '>남</option><option value="F"' + (s.gender === 'F' ? ' selected' : '') + '>여</option></select></label>' +
        '<label>돈<input id="f-money" type="number" min="0" max="9999999" value="' + (s.money || 0) + '"></label>' +
        '<label>도감<span class="v">포획 ' + caught + ' · 발견 ' + seen + '</span></label>' +
        '<label>시작한 날<span class="v">' + esc(s.started || '-') + '</span></label>' +
        '<label>마지막 회복 장소<span class="v">' + esc(mapName(s.lastHeal && s.lastHeal.map)) + '</span></label>' +
        '</div></div>' +
        // 진행
        '<div class="sec"><h3>체육관 배지 <small>' + s.badges.length + '/8</small></h3><div class="badges">' + badgeHtml + '</div></div>' +
        '<div class="sec"><h3>스토리 진행</h3><div class="badges">' + flagHtml + '</div></div>' +
        // 파티
        '<div class="sec"><h3>파티 <small>' + s.party.length + '/6</small></h3><div class="mons">' + s.party.map(function (m, i) { return monCard(m, 'party', i); }).join('') + '</div></div>' +
        // 포켓몬 추가
        '<div class="sec"><h3>포켓몬 넣기 <small>모든 포켓몬·폼 선택 가능</small></h3><div class="row">' +
        '<div class="picker"><input id="add-sp" placeholder="이름·ID·도감번호 (예: 레쿠쟈, 384)" style="width:240px" autocomplete="off"><div class="list" id="add-list" hidden></div></div>' +
        'Lv <input id="add-lv" type="number" min="1" max="100" value="50" style="width:60px">' +
        '<label><input type="checkbox" id="add-sh"> 이로치</label>' +
        '<label><input type="checkbox" id="add-iv" checked> 개체값 최대</label>' +
        '<select id="add-to"><option value="party">파티 (가득 차면 박스)</option><option value="box">박스</option></select>' +
        '<button class="pri" id="add-go">넣기</button><span id="add-msg" style="color:var(--sub)"></span></div></div>' +
        // 박스
        '<div class="sec"><h3>박스 <small>아이콘을 누르면 아래에서 수정</small></h3><div class="boxes">' + (boxes || '<div style="color:var(--muted)">비어 있음</div>') + '</div>' +
        (selMon ? '<div class="mons" style="margin-top:8px">' + monCard(selMon, 'box', selBox[0] + ':' + selBox[1]) + '</div>' : '') + '</div>' +
        // 가방
        '<div class="sec"><h3>가방 <small>개수를 0으로 하면 삭제</small></h3><div class="row" style="margin-bottom:6px">' +
        '<input id="bag-id" list="item-list" placeholder="도구 이름 또는 ID" style="width:220px"><input id="bag-n" type="number" min="1" max="999" value="1" style="width:70px"><button id="bag-add">추가</button></div>' +
        '<table><tr><th>도구</th><th>개수</th></tr>' + bag + '</table></div>' +
        // 고급
        '<div class="sec"><h3>고급: 기록 원본(JSON) <button id="raw-show">펼치기</button></h3><div id="raw-wrap" hidden><textarea id="raw"></textarea><div class="row" style="margin-top:6px"><button id="raw-apply">이 내용으로 바꾸기</button><small style="color:var(--muted)">잘못 고치면 게임이 안 열릴 수 있으니 주의</small></div></div></div>' +
        '</div>' +
        '<div class="savebar"><span class="msg" id="saveMsg"></span><button class="pri" id="saveBtn">서버에 저장</button></div>';
    setDirty(dirty);
    bindDetail();
}

// 종 목록 (기본 종 + 폼), 도감 번호 순
var ALL_SP = Object.keys(SP).map(function (k) { return { k: k, n: SP[k].n, name: spName(k), en: String(SP[k].en || k).toUpperCase() }; })
    .sort(function (a, b) { return a.n - b.n || (a.k.length - b.k.length); });
function filterSp(q) {
    q = (q || '').trim(); if (!q) return ALL_SP;
    var u = q.toUpperCase(), num = /^\d+$/.test(q) ? +q : null;
    return ALL_SP.filter(function (e) { return num !== null ? String(e.n).indexOf(q) === 0 : (e.name.indexOf(q) >= 0 || e.k.indexOf(u) >= 0 || e.en.indexOf(u) >= 0); });
}

function bindDetail() {
    var s = save, P = $('detailPanel');
    var mark = function () { setDirty(true); };
    $('f-name').addEventListener('input', function (e) { s.name = e.target.value; mark(); });
    $('f-gender').addEventListener('change', function (e) { s.gender = e.target.value; mark(); });
    $('f-money').addEventListener('input', function (e) { s.money = Math.max(0, Math.min(9999999, +e.target.value || 0)); mark(); });
    P.querySelectorAll('[data-badge]').forEach(function (cb) { cb.addEventListener('change', function () {
        var set = {}; P.querySelectorAll('[data-badge]').forEach(function (c) { if (c.checked) set[c.dataset.badge] = 1; });
        s.badges = BADGES.filter(function (b) { return set[b]; }); mark(); renderDetail();
    }); });
    P.querySelectorAll('[data-flag]').forEach(function (cb) { cb.addEventListener('change', function () { if (cb.checked) s.flags[cb.dataset.flag] = true; else delete s.flags[cb.dataset.flag]; mark(); }); });
    // 포켓몬 카드 조작
    P.querySelectorAll('.mon').forEach(function (card) {
        var get = function () {
            if (card.dataset.w === 'party') return { arr: s.party, i: +card.dataset.i };
            var bi = card.dataset.i.split(':'); return { arr: s.boxes[+bi[0]], i: +bi[1] };
        };
        card.addEventListener('change', function (e) {
            var g = get(), m = g.arr[g.i], act = e.target.dataset.act;
            if (act === 'lv') { m.l = Math.max(1, Math.min(100, +e.target.value || 1)); delete m.e; delete m.hp; mark(); }
            if (act === 'shiny') { m.sh = e.target.checked; mark(); renderDetail(); }
        });
        card.addEventListener('click', function (e) {
            var act = e.target.dataset.act; if (!act || e.target.tagName !== 'BUTTON') return;
            var g = get(), m = g.arr[g.i];
            if (act === 'heal') { delete m.hp; m.st = null; (m.mv || []).forEach(function (x) { x.pp = x.max; }); mark(); e.target.textContent = '회복됨'; return; }
            if (act === 'moves') { openMoveEditor(card, m); return; }
            if (act === 'del') { if (card.dataset.w === 'party' && s.party.length <= 1) { alertMsg('파티에는 최소 1마리가 있어야 합니다.'); return; } if (!window.confirm(spName(m.s) + '를 놓아줄까요?')) return; g.arr.splice(g.i, 1); selBox = null; }
            if (act === 'toBox') { if (s.party.length <= 1) { alertMsg('파티에는 최소 1마리가 있어야 합니다.'); return; } g.arr.splice(g.i, 1); deposit(m); }
            if (act === 'toParty') { if (s.party.length >= 6) { alertMsg('파티가 가득 찼습니다.'); return; } g.arr.splice(g.i, 1); s.party.push(m); selBox = null; }
            mark(); renderDetail();
        });
    });
    P.querySelectorAll('.bm').forEach(function (el) { el.addEventListener('click', function () { selBox = [+el.dataset.b, +el.dataset.i]; renderDetail(); }); });
    // 포켓몬 넣기 (전체 목록에서 고르기)
    var inp = $('add-sp'), list = $('add-list'), chosen = null;
    var showList = function () {
        var rows = filterSp(inp.value);
        list.innerHTML = rows.length ? rows.map(function (e) { return '<div class="it" data-k="' + e.k + '">' + icon(e.k) + '<span class="no">' + String(e.n).padStart(4, '0') + '</span><b>' + esc(e.name) + '</b><span class="k">' + e.k + '</span></div>'; }).join('') : '<div class="it">없음</div>';
        list.hidden = false;
    };
    inp.addEventListener('focus', showList); inp.addEventListener('input', function () { chosen = null; showList(); });
    inp.addEventListener('blur', function () { setTimeout(function () { list.hidden = true; }, 150); });
    list.addEventListener('mousedown', function (e) { var it = e.target.closest('.it'); if (!it || !it.dataset.k) return; e.preventDefault(); chosen = it.dataset.k; inp.value = spName(chosen) + ' (' + chosen + ')'; list.hidden = true; });
    $('add-go').addEventListener('click', function () {
        var k = chosen || (SP[inp.value.trim().toUpperCase()] ? inp.value.trim().toUpperCase() : null);
        if (!k) { $('add-msg').textContent = '목록에서 포켓몬을 골라 주세요.'; return; }
        var lv = Math.max(1, Math.min(100, +$('add-lv').value || 5));
        // 게임이 불러올 때 빠진 값(경험치·성별·기술·능력치)은 자동으로 채움
        var m = { s: k, l: lv, sh: $('add-sh').checked, mv: [], na: Math.floor(Math.random() * 25), b: 'POKEBALL', mm: '관리자', ml: lv };
        if ($('add-iv').checked) m.iv = [31, 31, 31, 31, 31, 31];
        if ($('add-to').value === 'party' && s.party.length < 6) s.party.push(m); else deposit(m);
        s.seen = s.seen || {}; s.caught = s.caught || {};
        var base = SP[k] && SP[k].form ? k.replace(/_\d+$/, '') : k; s.seen[base] = true; s.caught[base] = true;
        mark(); renderDetail(); $('add-msg').textContent = (m.sh ? '★' : '') + spName(k) + ' Lv.' + lv + ' 넣음 (저장해야 반영)';
    });
    // 가방
    P.querySelectorAll('[data-item]').forEach(function (el) { el.addEventListener('change', function () { var n = Math.max(0, Math.min(999, +el.value || 0)); if (n) s.bag[el.dataset.item] = n; else delete s.bag[el.dataset.item]; mark(); }); });
    $('bag-add').addEventListener('click', function () {
        var v = $('bag-id').value.trim(), id = IT[v.toUpperCase()] ? v.toUpperCase() : Object.keys(IT).find(function (k) { return IT[k].name === v; });
        if (!id) { alertMsg('도구를 찾을 수 없습니다.'); return; }
        s.bag[id] = Math.min(999, (s.bag[id] || 0) + Math.max(1, +$('bag-n').value || 1)); mark(); renderDetail();
    });
    // 원본 JSON
    $('raw-show').addEventListener('click', function () { var w = $('raw-wrap'); w.hidden = !w.hidden; if (!w.hidden) $('raw').value = JSON.stringify(s, null, 1); });
    $('raw-apply').addEventListener('click', function () {
        try { var o = JSON.parse($('raw').value); if (!Array.isArray(o.party)) throw 0; save = o; mark(); renderDetail(); } catch (e) { alertMsg('JSON 형식이 올바르지 않습니다 (party 배열 필요).'); }
    });
    $('saveBtn').addEventListener('click', doSave);
}
// ── 기술 고치기 ──
// 진화 전 모습 찾기 (진화 데이터를 거꾸로)
var PREVO = {};
Object.keys(SP).forEach(function (k) { (SP[k].evo || []).forEach(function (e) { if (!PREVO[e[0]]) PREVO[e[0]] = k; }); });
// 원작 데이터 기준으로 배울 수 있는 기술 → { id: 설명 }
function learnable(sp) {
    var out = {}, s = sp, guard = 0;
    var add = function (id, why) { if (MV[id] && !out[id]) out[id] = why; };
    while (s && SP[s] && guard++ < 4) {
        var d = SP[s], pre = s !== sp ? ' (' + d.name + ')' : '';
        (d.mv || []).forEach(function (x) { add(x[1], (x[0] === 0 ? '진화 시' : '레벨업 Lv.' + x[0]) + pre); });
        (d.tm || []).forEach(function (id) { add(id, '기술머신·가르침' + pre); });
        s = PREVO[String(s).replace(/_\d+$/, '')];
    }
    return out;
}
function openMoveEditor(card, m) {
    var old = card.querySelector('.mved'); if (old) { old.remove(); return; }
    var learn = learnable(m.s), box = document.createElement('div');
    box.className = 'mved'; box.style.cssText = 'grid-column:1/-1;display:flex;flex-direction:column;gap:4px;border-top:1px solid var(--line);padding-top:6px';
    var build = function (all) {
        var ids = all ? Object.keys(MV).filter(function (k) { return !/^(MAX|GMAX)/.test(k); }) : Object.keys(learn);
        ids.sort(function (a, b) { return mvName(a).localeCompare(mvName(b), 'ko'); });
        var opts = function (cur) {
            return '<option value="">(비움)</option>' + ids.map(function (id) {
                return '<option value="' + id + '"' + (id === cur ? ' selected' : '') + '>' + esc(mvName(id)) + (learn[id] ? ' · ' + esc(learn[id]) : ' · 원래는 못 배움') + '</option>';
            }).join('');
        };
        box.innerHTML = '<label style="font-size:11px;color:var(--sub)"><input type="checkbox" class="mv-all"' + (all ? ' checked' : '') + '> 모든 기술 보기 (배울 수 있는 기술 ' + Object.keys(learn).length + '개)</label>' +
            [0, 1, 2, 3].map(function (k) { var cur = (m.mv || [])[k]; return '<select data-slot="' + k + '" style="width:100%">' + opts(cur ? cur.id : '') + '</select>'; }).join('') +
            '<div class="row"><button class="pri mv-ok">기술 적용</button><button class="mv-auto">레벨에 맞게 (게임에서 자동)</button></div>';
        box.querySelector('.mv-all').addEventListener('change', function (e) { build(e.target.checked); });
        box.querySelector('.mv-ok').addEventListener('click', function () {
            var pick = [].slice.call(box.querySelectorAll('[data-slot]')).map(function (s) { return s.value; }).filter(Boolean);
            pick = pick.filter(function (id, i) { return pick.indexOf(id) === i; }).slice(0, 4);
            if (!pick.length) { alertMsg('기술을 하나 이상 고르세요.'); return; }
            // 이미 있던 기술은 PP 유지, 새 기술은 PP 가득
            m.mv = pick.map(function (id) { var have = (m.mv || []).find(function (x) { return x.id === id; }); return have || { id: id, pp: MV[id].pp, max: MV[id].pp }; });
            setDirty(true); renderDetail();
        });
        box.querySelector('.mv-auto').addEventListener('click', function () { m.mv = []; setDirty(true); renderDetail(); });   // 비워 두면 게임이 불러올 때 레벨에 맞는 기술로 채움
    };
    build(false);
    card.appendChild(box);
}
function deposit(m) {
    var s = save; s.boxes = s.boxes || [];
    for (var b = 0; b < s.boxes.length; b++) if (s.boxes[b].length < 30) { s.boxes[b].push(m); return; }
    s.boxes.push([m]);
}
function alertMsg(t) { var m = $('saveMsg'); if (m) { m.textContent = t; m.className = 'msg dirty'; } }
function doSave() {
    var btn = $('saveBtn'); btn.disabled = true;
    api('/api/astera/admin/save/' + encodeURIComponent(cur), { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(save) })
        .then(function (r) { return r.json().catch(function () { return {}; }).then(function (j) { return { r: r, j: j }; }); })
        .then(function (x) {
            btn.disabled = false;
            if (!x.r.ok || !x.j.ok) { alertMsg('저장 실패 (' + x.r.status + ')'); return; }
            setDirty(false);
            $('saveMsg').textContent = '저장했습니다. 지금 플레이 중이면 30초 안에 게임이 수정본으로 다시 불러옵니다.';
            loadPlayers();
        }).catch(function () { btn.disabled = false; alertMsg('서버에 연결하지 못했습니다.'); });
}
window.addEventListener('beforeunload', function (e) { if (dirty) { e.preventDefault(); e.returnValue = ''; } });

// 도구 이름 자동완성
(function () { var dl = document.createElement('datalist'); dl.id = 'item-list'; dl.innerHTML = Object.keys(IT).map(function (k) { return '<option value="' + esc(IT[k].name) + '">' + k + '</option>'; }).join(''); document.body.appendChild(dl); })();
loadPlayers();

// ── 이상한 소포 ──
var parcel = { mons: [], items: {} };
function monLabel(m) { return (m.sh ? '★' : '') + spName(m.s) + ' Lv.' + m.l + (m.gm ? ' (거다이맥스)' : ''); }
function renderParcel() {
    var parts = parcel.mons.map(function (m, i) { return '<span class="tag y" style="margin:2px">' + icon(m.s, m.sh) + esc(monLabel(m)) + ' <a href="#" data-rm-mon="' + i + '" style="color:var(--red)">✕</a></span>'; })
        .concat(Object.keys(parcel.items).map(function (id) { return '<span class="tag" style="margin:2px">' + esc(itName(id)) + ' ×' + parcel.items[id] + ' <a href="#" data-rm-item="' + esc(id) + '" style="color:var(--red)">✕</a></span>'; }));
    $('pc-contents').innerHTML = parts.length ? '소포 내용: ' + parts.join('') : '소포가 비어 있습니다.';
}
$('pc-contents').addEventListener('click', function (e) {
    var a = e.target; if (a.tagName !== 'A') return; e.preventDefault();
    if (a.dataset.rmMon) parcel.mons.splice(+a.dataset.rmMon, 1);
    if (a.dataset.rmItem) delete parcel.items[a.dataset.rmItem];
    renderParcel();
});
// 포켓몬 고르기 (전체 목록)
(function () {
    var inp = $('pc-sp'), list = $('pc-list');
    var show = function () {
        var rows = filterSp(inp.value);
        list.innerHTML = rows.length ? rows.map(function (e) { return '<div class="it" data-k="' + e.k + '">' + icon(e.k) + '<span class="no">' + String(e.n).padStart(4, '0') + '</span><b>' + esc(e.name) + '</b><span class="k">' + e.k + '</span></div>'; }).join('') : '<div class="it">없음</div>';
        list.hidden = false;
    };
    inp.addEventListener('focus', show); inp.addEventListener('input', function () { inp.dataset.k = ''; show(); });
    inp.addEventListener('blur', function () { setTimeout(function () { list.hidden = true; }, 150); });
    list.addEventListener('mousedown', function (e) { var it = e.target.closest('.it'); if (!it || !it.dataset.k) return; e.preventDefault(); inp.dataset.k = it.dataset.k; inp.value = spName(it.dataset.k) + ' (' + it.dataset.k + ')'; list.hidden = true; });
})();
$('pc-addmon').addEventListener('click', function () {
    var inp = $('pc-sp'), k = inp.dataset.k || (SP[inp.value.trim().toUpperCase()] ? inp.value.trim().toUpperCase() : '');
    if (!k) { $('pc-msgout').textContent = '목록에서 포켓몬을 골라 주세요.'; return; }
    var lv = Math.max(1, Math.min(100, +$('pc-lv').value || 5));
    // 게임이 받을 때 빠진 값(경험치·성별·기술·능력치)은 자동으로 채움. 성격은 받을 때마다 무작위가 되지 않도록 여기서 정함
    var m = { s: k, l: lv, sh: $('pc-sh').checked, mv: [], na: Math.floor(Math.random() * 25), b: 'CHERISHBALL', ml: lv };
    if (!IT.CHERISHBALL) m.b = 'POKEBALL';
    if ($('pc-iv').checked) m.iv = [31, 31, 31, 31, 31, 31];
    if ($('pc-gm').checked) m.gm = true;
    parcel.mons.push(m); renderParcel(); $('pc-msgout').textContent = '';
});
$('pc-additem').addEventListener('click', function () {
    var v = $('pc-item').value.trim(), id = IT[v.toUpperCase()] ? v.toUpperCase() : Object.keys(IT).find(function (k) { return IT[k].name === v; });
    if (!id) { $('pc-msgout').textContent = '도구를 찾을 수 없습니다.'; return; }
    parcel.items[id] = Math.min(999, (parcel.items[id] || 0) + Math.max(1, +$('pc-itemn').value || 1)); renderParcel();
});
$('pc-create').addEventListener('click', function () {
    var title = $('pc-title').value.trim();
    if (!title) { $('pc-msgout').textContent = '제목을 입력하세요.'; return; }
    if (!parcel.mons.length && !Object.keys(parcel.items).length) { $('pc-msgout').textContent = '포켓몬이나 도구를 하나 이상 넣으세요.'; return; }
    var iso = function (v) { return v ? new Date(v).toISOString() : ''; };   // 내 시간(KST) → UTC
    var body = { title: title, message: $('pc-msg').value.trim(), targets: $('pc-targets').value.trim(), startsAt: iso($('pc-start').value), endsAt: iso($('pc-end').value), gift: parcel };
    api('/api/astera/admin/parcels', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) })
        .then(function (r) { return r.json(); }).then(function (j) {
            if (!j.ok) { $('pc-msgout').textContent = '실패: ' + (j.error || ''); return; }
            $('pc-msgout').textContent = '소포 #' + j.id + ' 을(를) 보냈습니다.';
            parcel = { mons: [], items: {} }; renderParcel(); $('pc-title').value = ''; $('pc-msg').value = '';
            loadParcels();
        }).catch(function () { $('pc-msgout').textContent = '서버에 연결하지 못했습니다.'; });
});
function fmtDate(d) { return d ? new Date(d + (String(d).endsWith('Z') ? '' : 'Z')).toLocaleString('ko-KR', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' }) : ''; }
function loadParcels() {
    api('/api/astera/admin/parcels').then(function (r) { return r.json(); }).then(function (list) {
        if (!list.length) { $('pc-table').innerHTML = '<div style="color:var(--muted)">아직 보낸 소포가 없습니다.</div>'; return; }
        $('pc-table').innerHTML = '<table><tr><th>#</th><th>제목</th><th>내용</th><th>받는 사람</th><th>기간</th><th>받은 사람</th><th>상태</th><th></th></tr>' +
            list.map(function (p) {
                var g = p.gift || {}, inside = (g.mons || []).map(monLabel).concat(Object.keys(g.items || {}).map(function (id) { return itName(id) + '×' + g.items[id]; })).join(', ');
                var period = (p.startsAt || p.endsAt) ? (fmtDate(p.startsAt) || '지금') + ' ~ ' + (fmtDate(p.endsAt) || '계속') : '제한 없음';
                return '<tr><td>' + p.id + '</td><td><b>' + esc(p.title) + '</b><br><small style="color:var(--muted)">' + esc(p.message) + '</small></td><td>' + esc(inside) + '</td>' +
                    '<td>' + esc(p.targets || '전원') + '</td><td>' + esc(period) + '</td>' +
                    '<td title="' + esc(p.claimers || '') + '">' + p.claims + '명' + (p.claimers ? '<br><small style="color:var(--muted)">' + esc(p.claimers.length > 60 ? p.claimers.slice(0, 60) + '…' : p.claimers) + '</small>' : '') + '</td>' +
                    '<td><label><input type="checkbox" data-pact="' + p.id + '"' + (p.active ? ' checked' : '') + '> 받기 가능</label></td>' +
                    '<td><button class="del" data-pdel="' + p.id + '">삭제</button></td></tr>';
            }).join('') + '</table>';
    }).catch(function () { $('pc-table').textContent = '목록을 불러오지 못했습니다.'; });
}
$('pc-table').addEventListener('change', function (e) {
    var id = e.target.dataset.pact; if (!id) return;
    api('/api/astera/admin/parcels/' + id + '/active', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ active: e.target.checked }) }).then(loadParcels);
});
$('pc-table').addEventListener('click', function (e) {
    var id = e.target.dataset.pdel; if (!id) return;
    if (!window.confirm('소포 #' + id + ' 을(를) 삭제할까요? 아직 안 받은 사람은 더 이상 받을 수 없습니다.')) return;
    api('/api/astera/admin/parcels/' + id, { method: 'DELETE' }).then(loadParcels);
});
loadParcels();
