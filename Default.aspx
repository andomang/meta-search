<%-- ===== 페이지 지시문 =====
     Title       : 브라우저 탭에 표시될 페이지 제목 ("Home")
     Language    : 코드비하인드 언어 - C# 사용
     MasterPageFile: ~/Site.Master 를 레이아웃 틀로 사용한다.
                    Site.Master 가 공통 헤더/푸터/CSS 를 포함하며,
                    이 페이지의 콘텐츠는 Master 의 ContentPlaceHolder 에 삽입된다.
     AutoEventWireup: true → Page_Load 같은 이벤트 메서드를 자동으로 연결한다.
     CodeFile    : 이 aspx 파일과 짝을 이루는 C# 코드비하인드 파일 경로
     Inherits    : 코드비하인드의 클래스 이름 (_Default) --%>
<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%-- ===== 메인 콘텐츠 영역 =====
     Site.Master 의 "MainContent" ContentPlaceHolder 자리에 이 블록이 삽입된다.
     runat="server" → 서버에서 처리하는 ASP.NET 컨트롤임을 의미한다. --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ===== 전체 페이지 레이아웃 래퍼 =====
         flex flex-col items-center justify-center : 자식 요소를 세로로 쌓고 화면 중앙에 배치
         min-h-[calc(100vh-80px)] : 헤더 높이(80px)를 제외한 전체 화면 높이 이상
         px-4 : 좌우 패딩
         bg-white dark:bg-slate-950 : 라이트/다크 모드 배경색
         transition-colors duration-300 : 테마 전환 시 색상을 부드럽게 바꾼다 --%>
    <div class="flex flex-col items-center justify-center min-h-[calc(100vh-80px)] px-4 bg-white dark:bg-slate-950 transition-colors duration-300">

        <%-- w-full max-w-3xl : 너비 100% 이되 최대 48rem(768px) 제한
             text-center : 텍스트 가운데 정렬 --%>
        <div class="w-full max-w-3xl text-center">

            <%-- ===== 메인 타이틀 =====
                 text-6xl font-black : 초대형 두꺼운 폰트
                 bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 : 파란→보라→분홍 그라디언트
                 bg-clip-text text-transparent : 그라디언트를 텍스트 색상으로 적용 --%>
            <h1 class="text-6xl font-black mb-2 pb-4 bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent tracking-tighter leading-tight">
                Meta Search Engine
            </h1>

            <%-- 부제목 텍스트 영역: 코드비하인드에서 litSubtitle.Text 로 값을 설정한다.
                 asp:Literal 은 HTML 태그 없이 순수 텍스트(또는 HTML 문자열)만 출력한다.
                 runat="server" → 서버 컨트롤이므로 C# 코드에서 접근 가능하다. --%>
            <p class="text-gray-500 dark:text-gray-400 text-xl mb-12 font-medium"><asp:Literal ID="litSubtitle" runat="server"></asp:Literal></p>

            <%-- ===== 검색 박스 영역 =====
                 relative mb-16 : 자식의 absolute 위치 기준점, 아래 여백 64px
                 group : 자식 요소에서 group-focus-within 같은 유틸리티를 사용하기 위한 그룹 지정 --%>
            <div class="relative mb-16 group" id="searchBox">

                <%-- 돋보기 아이콘 (왼쪽 고정)
                     absolute left-6 top-1/2 -translate-y-1/2 : 입력창 세로 정중앙 왼쪽에 고정
                     group-focus-within:text-blue-500 : 입력창에 포커스 시 아이콘 색을 파란색으로 변경
                     data-lucide="search" : Lucide 아이콘 라이브러리의 돋보기(검색) 아이콘 --%>
                <div class="absolute left-6 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-blue-500 transition-colors z-10">
                    <i data-lucide="search" class="w-6 h-6"></i>
                </div>

                <%-- 검색어 입력 필드
                     ID="txtSearch" : 코드비하인드에서 txtSearch.Text 로 입력값을 읽는다.
                     runat="server" → 서버 컨트롤 (C# 에서 제어 가능)
                     pl-16 : 왼쪽 아이콘 공간 확보(패딩-left)
                     pr-36 : 오른쪽 버튼 공간 확보(패딩-right)
                     py-6 : 위아래 패딩으로 높이감을 준다
                     rounded-full : 완전 둥근 모서리 (알약 모양)
                     autocomplete="off" : 브라우저 자동완성 비활성화 --%>
                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="w-full pl-16 pr-36 py-6 text-xl border border-gray-100 dark:border-slate-700 rounded-full shadow-xl shadow-blue-100/20 dark:shadow-none focus:outline-none focus:ring-4 focus:ring-blue-500/10 dark:focus:ring-blue-500/20 bg-white dark:bg-slate-800 dark:text-white transition-all placeholder:text-gray-300 dark:placeholder:text-gray-500"
                    autocomplete="off" />

                <%-- 검색 실행 버튼
                     ID="btnSearch" : 코드비하인드의 btnSearch_Click 이벤트와 연결된다.
                     OnClick="btnSearch_Click" : 버튼 클릭 시 서버에서 btnSearch_Click 메서드가 실행된다.
                     runat="server" → 서버에서 처리
                     absolute right-3 top-3 bottom-3 : 입력창 오른쪽 안쪽에 고정
                     rounded-full : 완전 둥근 버튼 --%>
                <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click"
                    CssClass="absolute right-3 top-3 bottom-3 px-8 bg-slate-900 dark:bg-blue-600 text-white rounded-full font-bold hover:bg-blue-800 dark:hover:bg-blue-500 transition-all cursor-pointer active:scale-95 shadow-lg shadow-blue-200 dark:shadow-none" />

                <%-- ===== 최근 검색어 드롭다운 =====
                     JavaScript 의 loadHistory() 함수가 호출될 때 hidden 클래스를 제거하여 표시한다.
                     absolute left-0 right-0 top-full mt-2 : 검색창 바로 아래에 전체 너비로 배치
                     z-50 : 다른 요소 위에 표시 (z-index: 50) --%>
                <!-- 최근 검색어 드롭다운 -->
                <div id="historyDropdown"
                     class="hidden absolute left-0 right-0 top-full mt-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 rounded-2xl shadow-lg z-50 overflow-hidden text-left">

                    <%-- 드롭다운 헤더: "최근 검색어" 레이블 표시 영역
                         실제 텍스트는 아래 JavaScript 에서 litRecentLabel.textContent 로 주입된다. --%>
                    <div class="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-slate-700">
                        <span id="litRecentLabel" class="text-xs font-semibold text-gray-500 dark:text-slate-400"></span>
                    </div>

                    <%-- 검색 기록 목록: JavaScript 의 loadHistory() 가 <li> 요소를 동적으로 추가한다.
                         max-h-64 overflow-y-auto : 최대 높이 설정 후 스크롤 가능 --%>
                    <ul id="historyList" class="py-1 max-h-64 overflow-y-auto"></ul>

                    <%-- 기록 없음 메시지: 검색 기록이 없을 때 표시한다.
                         텍스트는 JavaScript 에서 주입된다. --%>
                    <div id="historyEmpty" class="hidden px-4 py-4 text-sm text-gray-400 dark:text-slate-500 text-center"></div>
                </div>
            </div>

            <%-- ===== 하단 바로가기 카드 그리드 =====
                 grid md:grid-cols-2 : 중간 화면(768px) 이상에서 2열 그리드
                 gap-6 : 카드 간 간격
                 max-w-2xl mx-auto : 최대 너비 제한 + 가운데 정렬 --%>
            <div class="grid md:grid-cols-2 gap-6 max-w-2xl mx-auto text-left">

                <%-- 커뮤니티 바로가기 카드
                     hover:border-blue-500/50 : 마우스 오버 시 파란 테두리 반투명 표시
                     group : 자식 아이콘의 group-hover 효과 활성화 --%>
                <a href="Community.aspx" class="p-6 bg-gray-50 dark:bg-slate-800/50 rounded-3xl border border-transparent hover:border-blue-500/50 dark:hover:border-blue-500/50 transition-all group flex items-center gap-5 shadow-sm">
                    <div class="p-4 bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded-2xl group-hover:scale-110 transition-transform">
                        <%-- data-lucide="message-circle" : Lucide 아이콘 - 말풍선(메시지) 아이콘 --%>
                        <i data-lucide="message-circle" class="w-6 h-6"></i>
                    </div>
                    <div>
                        <%-- 커뮤니티 카드 제목: 코드비하인드에서 litCommunityTitle.Text 로 설정 --%>
                        <h3 class="font-bold text-gray-900 dark:text-white text-lg"><asp:Literal ID="litCommunityTitle" runat="server"></asp:Literal></h3>
                        <%-- 커뮤니티 카드 부제목: 코드비하인드에서 litCommunitySub.Text 로 설정 --%>
                        <p class="text-sm text-gray-500 dark:text-gray-400"><asp:Literal ID="litCommunitySub" runat="server"></asp:Literal></p>
                    </div>
                </a>

                <%-- 설정 바로가기 카드
                     hover:border-purple-500/50 : 마우스 오버 시 보라색 테두리 반투명 표시 --%>
                <a href="Settings.aspx" class="p-6 bg-gray-50 dark:bg-slate-800/50 rounded-3xl border border-transparent hover:border-purple-500/50 dark:hover:border-purple-500/50 transition-all group flex items-center gap-5 shadow-sm">
                    <div class="p-4 bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400 rounded-2xl group-hover:scale-110 transition-transform">
                        <%-- data-lucide="sliders" : Lucide 아이콘 - 슬라이더(설정 조절) 아이콘 --%>
                        <i data-lucide="sliders" class="w-6 h-6"></i>
                    </div>
                    <div>
                        <%-- 설정 카드 제목: 코드비하인드에서 litSettingsTitle.Text 로 설정 --%>
                        <h3 class="font-bold text-gray-900 dark:text-white text-lg"><asp:Literal ID="litSettingsTitle" runat="server"></asp:Literal></h3>
                        <%-- 설정 카드 부제목: 코드비하인드에서 litSettingsSub.Text 로 설정 --%>
                        <p class="text-sm text-gray-500 dark:text-gray-400"><asp:Literal ID="litSettingsSub" runat="server"></asp:Literal></p>
                    </div>
                </a>
            </div>
        </div>
    </div>

    <%-- ===== 인라인 JavaScript =====
         검색창 포커스 시 검색 기록 드롭다운을 제어하는 클라이언트 스크립트 --%>
    <script>
        // ===== 서버에서 주입되는 다국어 문자열 =====
        // Lang.Get("...") : 서버에서 현재 언어 설정에 맞는 문자열을 반환한다 (<= ... %> 는 서버 출력 표현식)
        var recentLabel      = '<%= Lang.Get("search.recent") %>';      // "최근 검색어" 레이블
        var noHistoryLabel   = '<%= Lang.Get("search.noHistory") %>';   // 기록 없음 메시지
        var searchPlaceholder = '<%= Lang.Get("search.placeholder") %>'; // 검색창 placeholder 텍스트

        // 서버에서 받은 텍스트를 해당 HTML 요소에 적용
        document.getElementById('litRecentLabel').textContent = recentLabel;
        document.getElementById('historyEmpty').textContent   = noHistoryLabel;

        // ===== DOM 요소 참조 =====
        // txtSearch.ClientID : 서버 컨트롤 ID가 ASP.NET에 의해 변환된 실제 HTML id를 얻는다
        const searchInput  = document.getElementById('<%= txtSearch.ClientID %>');
        searchInput.placeholder = searchPlaceholder; // placeholder 텍스트 적용

        const dropdown    = document.getElementById('historyDropdown'); // 드롭다운 전체 컨테이너
        const historyList  = document.getElementById('historyList');    // 기록 목록 <ul>
        const historyEmpty = document.getElementById('historyEmpty');   // 기록 없음 메시지

        // ===== 이벤트 리스너 =====
        // 입력창에 포커스가 오면 기록 목록을 로드하여 드롭다운을 표시한다
        searchInput.addEventListener('focus', function () { loadHistory(); });

        // 검색박스 바깥 클릭 시 드롭다운을 숨긴다
        document.addEventListener('click', function (e) {
            if (!document.getElementById('searchBox').contains(e.target)) dropdown.classList.add('hidden');
        });

        // Enter 키 입력 시 검색결과 페이지로 이동한다
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault(); // 폼 기본 제출 방지
                var q = searchInput.value.trim();
                if (q) location.href = 'SearchResults.aspx?q=' + encodeURIComponent(q);
            }
        });

        // ===== loadHistory() =====
        // SearchResults.aspx?action=getHistory 에 Ajax 요청을 보내 최근 검색 기록을 가져온다.
        // 응답은 JSON 배열 (예: [{ query: "검색어" }, ...]) 형태
        function loadHistory() {
            fetch('SearchResults.aspx?action=getHistory')
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    historyList.innerHTML = ''; // 기존 목록 초기화
                    if (!data || data.length === 0) {
                        // 기록이 없으면 "기록 없음" 메시지를 표시하고 목록은 숨긴다
                        historyEmpty.classList.remove('hidden');
                        historyList.classList.add('hidden');
                    } else {
                        // 기록이 있으면 "기록 없음" 메시지를 숨기고 목록을 표시한다
                        historyEmpty.classList.add('hidden');
                        historyList.classList.remove('hidden');
                        // 각 기록 항목을 <li> 요소로 생성하여 목록에 추가
                        data.forEach(function (item) {
                            var li = document.createElement('li');
                            li.className = 'flex items-center justify-between px-4 py-2 hover:bg-gray-50 dark:hover:bg-slate-700 cursor-pointer';
                            // 검색어 텍스트 + 시계 아이콘, 삭제(X) 버튼으로 구성된 HTML 생성
                            li.innerHTML = '<span class="flex items-center gap-2 text-sm text-gray-700 dark:text-slate-300 flex-1 truncate search-query"><svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' + escapeHtml(item.query) + '</span><button class="delete-btn ml-2 text-gray-300 hover:text-red-400 dark:text-slate-600 dark:hover:text-red-400 flex-shrink-0" data-query="' + escapeHtml(item.query) + '"><svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>';
                            // 검색어 클릭 → 검색결과 페이지로 이동
                            li.querySelector('.search-query').addEventListener('click', function () { location.href = 'SearchResults.aspx?q=' + encodeURIComponent(item.query); });
                            // 삭제 버튼 클릭 → deleteHistory() 호출
                            li.querySelector('.delete-btn').addEventListener('click', function (e) { e.stopPropagation(); deleteHistory(item.query, li); });
                            historyList.appendChild(li);
                        });
                    }
                    dropdown.classList.remove('hidden'); // 드롭다운 표시
                }).catch(function () { dropdown.classList.add('hidden'); }); // 오류 시 드롭다운 숨김
        }

        // ===== deleteHistory(query, liElement) =====
        // 특정 검색어 기록을 서버에서 삭제하고 DOM 에서도 해당 <li> 를 제거한다.
        // query     : 삭제할 검색어 문자열
        // liElement : 화면에서 제거할 <li> DOM 요소
        function deleteHistory(query, liElement) {
            fetch('SearchResults.aspx?action=deleteHistory&q=' + encodeURIComponent(query))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.result === 'ok') {
                        liElement.remove(); // DOM 에서 항목 제거
                        // 모든 항목 삭제 후 목록이 비었으면 "기록 없음" 메시지 표시
                        if (historyList.children.length === 0) {
                            historyEmpty.classList.remove('hidden');
                            historyList.classList.add('hidden');
                        }
                    }
                });
        }

        // ===== escapeHtml(str) =====
        // XSS(크로스사이트 스크립팅) 방지를 위해 HTML 특수문자를 이스케이프한다.
        // & → &amp;  < → &lt;  > → &gt;  " → &quot;
        function escapeHtml(str) { return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }
    </script>
</asp:Content>
