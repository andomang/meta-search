<%@ Page Title="Search Results" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SearchResults.aspx.cs" Inherits="SearchResults" %>
<%--
    [SearchResults.aspx]
    검색 결과 표시 페이지입니다.
    코드비하인드: SearchResults.aspx.cs / Inherits="SearchResults"
    - URL 쿼리스트링 ?q=검색어 로 검색어를 받아 결과를 표시합니다.
    - 검색 결과는 rptResults Repeater로, 인기 검색어는 rptPopular Repeater로 표시합니다.
    - 최근 검색 기록 드롭다운, 자동완성 제안, 검색 결과 클릭 저장 기능을
      JavaScript fetch(AJAX)로 처리합니다.
    - 다국어 텍스트는 asp:HiddenField 5개를 통해 서버→클라이언트로 전달됩니다.
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-white dark:bg-slate-900 transition-colors">

        <%-- ===== 상단 고정 검색바 ===== --%>
        <%-- sticky top-0: 스크롤해도 화면 상단에 고정됩니다. z-10: 다른 요소 위에 표시됩니다. --%>
        <div class="border-b border-gray-200 dark:border-slate-800 bg-white dark:bg-slate-900 sticky top-0 z-10">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 py-4">
                <div class="flex flex-col sm:flex-row sm:items-center gap-4">
                    <%-- "Search" 로고 링크: 클릭 시 Default.aspx(홈)으로 이동합니다. --%>
                    <a href="Default.aspx" class="text-2xl font-semibold bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent flex-shrink-0">Search</a>

                    <%-- ===== 검색창 + 버튼 + 드롭다운 영역 ===== --%>
                    <div class="flex-1 max-w-2xl flex gap-2 relative" id="searchBox">
                        <div class="relative flex-1">
                            <%-- data-lucide="search": 돋보기 아이콘. 검색 입력 필드 왼쪽에 표시됩니다. --%>
                            <i data-lucide="search" class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5 z-10"></i>
                            <%--
                                txtSearch: 검색어 입력 텍스트 박스.
                                코드비하인드에서 txtSearch.Text 로 입력값을 읽어 검색에 사용합니다.
                                autocomplete="off": 브라우저 자동완성 기능을 끕니다 (자체 드롭다운과 충돌 방지).
                                placeholder 텍스트는 JavaScript에서 hdnSearchPh 값으로 동적으로 설정합니다.
                            --%>
                            <asp:TextBox ID="txtSearch" runat="server"
                                CssClass="w-full pl-12 pr-4 py-3 border border-gray-300 dark:border-slate-600 rounded-full shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-slate-800 dark:text-white"
                                autocomplete="off" />

                            <%-- ===== 최근 검색어 / 자동완성 통합 드롭다운 ===== --%>
                            <%--
                                historyDropdown: 검색창 포커스 시 나타나는 드롭다운 패널.
                                기본값은 hidden (숨김). JavaScript의 loadHistory() / renderDropdown()에서 표시합니다.
                                최근 검색 기록 또는 자동완성 제안어를 목록으로 보여줍니다.
                            --%>
                            <div id="historyDropdown"
                                 class="hidden absolute left-0 right-0 top-full mt-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 rounded-2xl shadow-lg z-50 overflow-hidden">
                                <div class="flex items-center px-4 py-3 border-b border-gray-100 dark:border-slate-700">
                                    <%-- litRecentLabel: "최근 검색어" 드롭다운 헤더 텍스트. JavaScript에서 hdnRecentLabel 값으로 동적으로 설정합니다. --%>
                                    <span id="litRecentLabel" class="text-xs font-semibold text-gray-500 dark:text-slate-400"></span>
                                </div>
                                <%-- historyList: 최근 검색 기록 항목들이 동적으로 추가되는 ul 목록입니다. --%>
                                <ul id="historyList" class="py-1 max-h-64 overflow-y-auto"></ul>
                                <%-- historyEmpty: 검색 기록이 없을 때 표시되는 "기록 없음" 안내 텍스트 영역입니다. --%>
                                <div id="historyEmpty" class="hidden px-4 py-4 text-sm text-gray-400 dark:text-slate-500 text-center"></div>
                            </div>
                        </div>
                        <%--
                            btnSearch: 검색 실행 버튼.
                            OnClick="btnSearch_Click" → 코드비하인드의 btnSearch_Click 이벤트 핸들러를 호출합니다.
                            txtSearch.Text의 값을 쿼리스트링으로 받아 검색 결과를 로드하고 페이지를 다시 렌더링합니다.
                            버튼 텍스트(예: "검색")는 코드비하인드에서 Text 속성에 설정합니다.
                        --%>
                        <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click"
                            CssClass="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-full transition-all flex-shrink-0 cursor-pointer" />
                </div>
            </div>
        </div>

        <div class="max-w-7xl mx-auto px-4 sm:px-6 py-8">
            <div class="flex flex-col lg:flex-row gap-8">

                <%-- ===== 검색 결과 영역 ===== --%>
                <div class="flex-1">
                    <%-- litResultsLabel: "검색어에 대한 N개의 결과" 같은 결과 개수 안내 텍스트. 코드비하인드에서 주입합니다. --%>
                    <p class="text-sm text-gray-600 dark:text-slate-400 mb-6"><asp:Literal ID="litResultsLabel" runat="server"></asp:Literal></p>

                    <%-- ===== 로딩 스피너 ===== --%>
                    <%--
                        loadingSpinner: 검색 버튼 클릭 또는 엔터키 입력 시 표시되는 로딩 애니메이션.
                        JavaScript의 showLoading() 함수에서 hidden 클래스를 제거하여 표시합니다.
                        litLoadingLabel: "검색 중..." 텍스트. 코드비하인드에서 다국어 값을 주입합니다.
                    --%>
                    <div id="loadingSpinner" class="hidden flex flex-col items-center justify-center py-20">
                        <div class="w-10 h-10 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mb-4"></div>
                        <p class="text-gray-400 dark:text-slate-500 text-sm"><asp:Literal ID="litLoadingLabel" runat="server"></asp:Literal></p>
                    </div>

                    <%-- ===== 검색 결과 없음 UI ===== --%>
                    <%--
                        noResults: 검색어는 있으나 결과가 0개일 때 표시되는 안내 UI.
                        기본값 display:none. JavaScript의 window.onload 이벤트에서
                        검색어가 있고 rptResults 결과가 없으면 display:flex 로 전환합니다.
                        data-lucide="search-x": 검색 불가 아이콘.
                        litNoResults, litNoResultsSub: "결과 없음" 제목 및 보조 텍스트. 코드비하인드에서 다국어 값을 주입합니다.
                    --%>
                    <div id="noResults" style="display:none" class="flex-col items-center justify-center py-20 text-center">
                        <i data-lucide="search-x" class="w-16 h-16 text-gray-200 dark:text-slate-700 mb-4"></i>
                        <p class="text-lg font-bold text-gray-400 dark:text-slate-500"><asp:Literal ID="litNoResults" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-300 dark:text-slate-600 mt-1"><asp:Literal ID="litNoResultsSub" runat="server"></asp:Literal></p>
                    </div>

                    <%-- ===== 검색 결과 Repeater ===== --%>
                    <%--
                        rptResults: 검색 결과 목록 Repeater.
                        코드비하인드에서 rptResults.DataSource 에 검색 결과 DataTable(또는 List)을 바인딩합니다.
                        바인딩되는 주요 컬럼:
                          - Url: 검색 결과 원본 URL (링크 href와 표시 URL 모두 사용)
                          - Title: 검색 결과 페이지 제목 (링크 텍스트로 표시)
                          - Description: 검색 결과 요약 설명
                        각 결과 클릭 시 saveClick(this) JavaScript 함수를 호출해 클릭 기록을 서버에 저장합니다.
                    --%>
                    <div id="resultsContainer">
                    <asp:Repeater ID="rptResults" runat="server">
                        <ItemTemplate>
                            <div class="group mb-8">
                                <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-slate-400 mb-1">
                                    <%-- data-lucide="globe": 지구본 아이콘. URL 표시 줄 앞에 표시됩니다. --%>
                                    <i data-lucide="globe" class="w-4 h-4"></i>
                                    <%-- Eval("Url"): 검색 결과의 원본 URL을 텍스트로 표시합니다. --%>
                                    <span><%# Eval("Url") %></span>
                                </div>
                                <%--
                                    검색 결과 제목 링크.
                                    onclick='saveClick(this); return false;': 기본 링크 동작을 막고
                                    saveClick() JavaScript 함수를 먼저 호출합니다.
                                    saveClick()은 클릭 기록을 AJAX로 서버에 저장한 후 새 탭으로 URL을 엽니다.
                                    data-title: 검색 결과 제목을 saveClick()이 읽어 서버에 전송하는 데이터 속성입니다.
                                --%>
                                <a href='<%# Eval("Url") %>'
                                   data-title='<%# Eval("Title") %>'
                                   onclick='saveClick(this); return false;'
                                   class="text-2xl text-blue-600 dark:text-blue-400 hover:underline cursor-pointer">
                                    <%-- Eval("Title"): 검색 결과 페이지 제목을 링크 텍스트로 출력합니다. --%>
                                    <%# Eval("Title") %>
                                </a>
                                <%-- Eval("Description"): 검색 결과 요약 설명을 출력합니다. --%>
                                <p class="mt-2 text-gray-700 dark:text-slate-300"><%# Eval("Description") %></p>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    </div>
                </div>

                <%-- ===== 사이드바 ===== --%>
                <div class="lg:w-80 space-y-6">

                    <%-- ===== 인기 검색어 사이드바 ===== --%>
                    <div class="bg-gray-50 dark:bg-slate-800 border dark:border-slate-700 rounded-2xl p-6">
                        <h3 class="font-semibold mb-4 dark:text-white flex items-center gap-2">
                            <%-- data-lucide="trending-up": 인기 상승 화살표 아이콘. 인기 검색어 섹션 제목 앞에 표시됩니다. --%>
                            <i data-lucide="trending-up" class="w-4 h-4 text-blue-500"></i>
                            <%-- litPopularLabel: "인기 검색어" 섹션 제목 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                            <asp:Literal ID="litPopularLabel" runat="server"></asp:Literal>
                        </h3>
                        <%--
                            rptPopular: 인기 검색어 목록 Repeater.
                            코드비하인드에서 rptPopular.DataSource 에 인기 검색어 DataTable(또는 List)을 바인딩합니다.
                            바인딩되는 주요 컬럼:
                              - Query: 검색어 텍스트 (링크 텍스트 및 URL 파라미터로 사용)
                              - SearchCount: 해당 검색어의 검색 횟수 (오른쪽에 숫자로 표시)
                            Container.ItemIndex: 현재 항목의 0부터 시작하는 순서 인덱스입니다.
                        --%>
                        <asp:Repeater ID="rptPopular" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                                    <div class="flex items-center gap-2">
                                        <%--
                                            순위 번호 표시.
                                            Container.ItemIndex + 1: 1부터 시작하는 순위 번호입니다.
                                            Container.ItemIndex < 3 이면 파란색(text-blue-500)으로 1~3위를 강조합니다.
                                            그 외 순위는 회색(text-gray-400)으로 표시됩니다.
                                        --%>
                                        <span class='text-xs font-bold w-5 text-center <%# Container.ItemIndex < 3 ? "text-blue-500" : "text-gray-400 dark:text-slate-500" %>'>
                                            <%# Container.ItemIndex + 1 %>
                                        </span>
                                        <%--
                                            Eval("Query"): 인기 검색어를 링크 텍스트로 표시합니다.
                                            링크 클릭 시 해당 검색어로 SearchResults.aspx 검색을 실행합니다.
                                            Server.UrlEncode(): URL에 특수문자/한글이 포함된 경우 안전하게 인코딩합니다.
                                        --%>
                                        <a href='SearchResults.aspx?q=<%# Server.UrlEncode(Eval("Query").ToString()) %>'
                                           class="text-sm text-gray-700 dark:text-slate-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors">
                                            <%# Eval("Query") %>
                                        </a>
                                    </div>
                                    <span class="text-xs text-gray-400 dark:text-slate-500">
                                        <%--
                                            Eval("SearchCount"): 해당 검색어의 검색 횟수를 표시합니다.
                                            litTimesInner: "회" 단위 텍스트. JavaScript의 window.onload에서
                                            hdnTimesLabel 값을 읽어 동적으로 채웁니다. (다국어 대응)
                                        --%>
                                        <%# Eval("SearchCount") %><asp:Literal ID="litTimesInner" runat="server"></asp:Literal>
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <%-- ===== 관련 검색어 사이드바 (추후 실제 데이터로 교체) ===== --%>
                    <%-- 현재는 정적 텍스트로 하드코딩되어 있습니다. 추후 Repeater로 동적 처리 예정입니다. --%>
                    <div class="bg-gray-50 dark:bg-slate-800 border dark:border-slate-700 rounded-2xl p-6">
                        <%-- litRelatedLabel: "관련 검색어" 섹션 제목 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                        <h3 class="font-semibold mb-4 dark:text-white"><asp:Literal ID="litRelatedLabel" runat="server"></asp:Literal></h3>
                        <div class="space-y-2">
                            <p class="text-blue-600 dark:text-blue-400 cursor-pointer hover:underline text-sm">ASP.NET Web Forms</p>
                            <p class="text-blue-600 dark:text-blue-400 cursor-pointer hover:underline text-sm">Tailwind CSS v4</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- ===== 다국어 텍스트 전달용 HiddenField 모음 ===== --%>
    <%--
        아래 5개의 HiddenField는 서버→클라이언트로 다국어 문자열 값을 전달하기 위해 사용됩니다.
        렌더링된 HTML에는 보이지 않지만, JavaScript에서 .value 로 값을 읽어 UI에 적용합니다.
    --%>
    <%--
        hdnRecentLabel: "최근 검색어" 드롭다운 헤더 텍스트.
        JavaScript에서 document.getElementById('litRecentLabel').textContent 에 설정합니다.
    --%>
    <asp:HiddenField ID="hdnRecentLabel"  runat="server" />
    <%--
        hdnNoHistLabel: 검색 기록이 없을 때 드롭다운에 표시할 안내 텍스트 (예: "최근 검색 기록이 없습니다").
        JavaScript에서 historyEmpty 요소의 textContent 에 설정합니다.
    --%>
    <asp:HiddenField ID="hdnNoHistLabel"  runat="server" />
    <%--
        hdnSearchPh: 검색 입력 필드의 placeholder 텍스트 (예: "검색어를 입력하세요").
        JavaScript에서 searchInput.placeholder 에 설정합니다.
    --%>
    <asp:HiddenField ID="hdnSearchPh"     runat="server" />
    <%--
        hdnTimesLabel: 인기 검색어 횟수 뒤에 붙는 단위 텍스트 (예: "회").
        JavaScript의 window.onload 에서 Repeater 내 litTimesInner 요소들에 설정합니다.
    --%>
    <asp:HiddenField ID="hdnTimesLabel"   runat="server" />
    <%--
        hdnCurrentQuery: 현재 검색어 문자열.
        JavaScript에서 currentQuery 변수로 읽어 saveClick() 함수와 결과 없음 UI 표시 판단에 사용합니다.
    --%>
    <asp:HiddenField ID="hdnCurrentQuery" runat="server" />

    <script>
        /* =====================================================
           다국어 변수 초기화
           서버에서 HiddenField에 설정한 다국어 문자열을 JavaScript 변수로 읽어옵니다.
           ClientID: 서버 컨트롤의 실제 HTML id를 반환합니다 (마스터페이지 중첩 시 id가 달라질 수 있으므로 서버 표현식 사용).
        ===================================================== */
        var recentLabel  = document.getElementById('<%= hdnRecentLabel.ClientID %>').value;   // "최근 검색어"
        var noHistLabel  = document.getElementById('<%= hdnNoHistLabel.ClientID %>').value;   // "기록 없음" 안내 텍스트
        var searchPh     = document.getElementById('<%= hdnSearchPh.ClientID %>').value;      // 검색창 placeholder 텍스트
        var timesLabel   = document.getElementById('<%= hdnTimesLabel.ClientID %>').value;    // 횟수 단위 텍스트 ("회")
        var currentQuery = document.getElementById('<%= hdnCurrentQuery.ClientID %>').value;  // 현재 검색어

        /* 다국어 텍스트를 드롭다운 헤더와 "기록 없음" 안내 영역에 초기 설정합니다. */
        document.getElementById('litRecentLabel').textContent = recentLabel;
        document.getElementById('historyEmpty').textContent   = noHistLabel;

        /* 자주 사용하는 DOM 요소를 변수에 저장합니다. */
        var searchInput  = document.getElementById('<%= txtSearch.ClientID %>');  // 검색 입력 필드
        var dropdown     = document.getElementById('historyDropdown');             // 드롭다운 패널
        var historyList  = document.getElementById('historyList');                 // 기록 목록 ul
        var historyEmpty = document.getElementById('historyEmpty');                // "기록 없음" div
        var noResultsDiv = document.getElementById('noResults');                   // "결과 없음" div

        /* 검색창 placeholder 텍스트를 서버에서 받은 다국어 값으로 설정합니다. */
        searchInput.placeholder = searchPh;

        /* =====================================================
           페이지 로드 완료 시 실행
        ===================================================== */
        window.addEventListener('load', function() {
            // 검색어가 있는데 결과가 없으면 "결과 없음" UI를 표시합니다.
            var items = document.querySelectorAll('#resultsContainer .group');
            if (currentQuery && items.length === 0) {
                noResultsDiv.style.display = 'flex';
            }

            // rptPopular Repeater 내부의 litTimesInner 서버 컨트롤들에
            // "회" 단위 텍스트를 동적으로 설정합니다.
            // [id$="litTimesInner"]: id가 "litTimesInner"로 끝나는 모든 요소를 선택합니다.
            document.querySelectorAll('[id$="litTimesInner"]').forEach(function(el) {
                el.textContent = timesLabel;
            });
        });

        /* =====================================================
           검색창 이벤트 처리
        ===================================================== */

        /* 포커스: 검색창에 커서가 올 때마다 최근 검색 기록을 드롭다운으로 표시합니다. */
        searchInput.addEventListener('focus', function () {
            loadHistory();
        });

        /*
           입력 중 자동완성 제안 표시 (300ms 디바운스).
           디바운스: 타이핑 중 매번 요청하지 않고, 300ms 동안 추가 입력이 없을 때 한 번만 요청합니다.
        */
        var suggestTimer = null;
        searchInput.addEventListener('input', function () {
            clearTimeout(suggestTimer);
            var val = searchInput.value.trim();
            // 입력값이 비어있으면 자동완성 대신 최근 기록을 표시합니다.
            if (val.length === 0) {
                loadHistory();
                return;
            }
            // 300ms 후 자동완성 제안을 요청합니다.
            suggestTimer = setTimeout(function () {
                loadSuggest(val);
            }, 300);
        });

        /* 검색창 외부 클릭 시 드롭다운을 닫습니다. */
        document.addEventListener('click', function (e) {
            if (!document.getElementById('searchBox').contains(e.target)) {
                dropdown.classList.add('hidden');
            }
        });

        /* 엔터키 입력 시 검색을 실행하고 결과 페이지로 이동합니다. */
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();  // 폼 기본 제출 방지
                var q = searchInput.value.trim();
                if (q) {
                    showLoading();   // 로딩 스피너 표시
                    location.href = 'SearchResults.aspx?q=' + encodeURIComponent(q);
                }
            }
        });

        /* =====================================================
           showLoading(): 로딩 스피너 표시 함수
           검색 요청이 시작될 때 화면에 로딩 애니메이션을 표시합니다.
        ===================================================== */
        function showLoading() {
            document.getElementById('loadingSpinner').classList.remove('hidden');
        }

        /* =====================================================
           loadHistory(): 최근 검색 기록 드롭다운 로드 함수
           서버에서 현재 사용자의 최근 검색 기록을 가져와 드롭다운으로 표시합니다.
           요청: GET SearchResults.aspx?action=getHistory
           응답 JSON: [{ query: "검색어1" }, { query: "검색어2" }, ...]
        ===================================================== */
        function loadHistory() {
            fetch('SearchResults.aspx?action=getHistory')
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    // 'history' 타입으로 렌더링 → X 삭제 버튼 포함
                    renderDropdown(data, 'history');
                }).catch(function () { dropdown.classList.add('hidden'); }); // 오류 시 드롭다운 닫기
        }

        /* =====================================================
           loadSuggest(keyword): 자동완성 제안 로드 함수
           입력한 키워드로 시작하는 자동완성 검색어 목록을 서버에서 가져옵니다.
           요청: GET SearchResults.aspx?action=getSuggest&q={keyword}
           응답 JSON: ["제안어1", "제안어2", ...] (문자열 배열)
           - 제안어가 없으면 최근 검색 기록을 대신 표시합니다.
        ===================================================== */
        function loadSuggest(keyword) {
            fetch('SearchResults.aspx?action=getSuggest&q=' + encodeURIComponent(keyword))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (!data || data.length === 0) {
                        loadHistory(); // 자동완성 결과 없으면 최근 기록으로 대체
                        return;
                    }
                    // 서버 응답(문자열 배열)을 { query: string } 객체 배열로 변환합니다.
                    // renderDropdown이 기대하는 형식과 맞추기 위해서입니다.
                    var converted = data.map(function(q) { return { query: q }; });
                    // 'suggest' 타입으로 렌더링 → X 삭제 버튼 없음
                    renderDropdown(converted, 'suggest');
                }).catch(function () { dropdown.classList.add('hidden'); }); // 오류 시 드롭다운 닫기
        }

        /* =====================================================
           renderDropdown(data, type): 드롭다운 목록 렌더링 공통 함수
           최근 검색 기록과 자동완성 제안어를 동일한 드롭다운 UI로 표시합니다.
           매개변수:
             data: [{ query: "검색어" }, ...] 형태의 배열
             type: 'history' → X 삭제 버튼 포함 / 'suggest' → X 버튼 없음
        ===================================================== */
        function renderDropdown(data, type) {
            historyList.innerHTML = '';  // 기존 목록 초기화
            document.getElementById('litRecentLabel').textContent = recentLabel;

            if (!data || data.length === 0) {
                // 데이터가 없으면 "기록 없음" 안내를 표시하고 목록을 숨깁니다.
                historyEmpty.classList.remove('hidden');
                historyList.classList.add('hidden');
            } else {
                historyEmpty.classList.add('hidden');
                historyList.classList.remove('hidden');
                data.forEach(function (item) {
                    var li = document.createElement('li');
                    li.className = 'flex items-center justify-between px-4 py-2 hover:bg-gray-50 dark:hover:bg-slate-700 cursor-pointer group';

                    // history 타입일 때만 X(삭제) 버튼 HTML을 생성합니다.
                    var deleteBtn = type === 'history'
                        ? '<button class="delete-btn ml-2 text-gray-300 hover:text-red-400 dark:text-slate-600 dark:hover:text-red-400 flex-shrink-0"><svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>'
                        : '';

                    // 검색어 텍스트(돋보기 아이콘 + 검색어)와 삭제 버튼을 li 내부에 삽입합니다.
                    li.innerHTML =
                        '<span class="flex items-center gap-2 text-sm text-gray-700 dark:text-slate-300 flex-1 truncate search-query">' +
                        '<svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">' +
                        '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>' +
                        escapeHtml(item.query) + '</span>' + deleteBtn;

                    // 검색어 텍스트 클릭 시 해당 검색어로 검색 페이지로 이동합니다.
                    li.querySelector('.search-query').addEventListener('click', function () {
                        showLoading();
                        location.href = 'SearchResults.aspx?q=' + encodeURIComponent(item.query);
                    });

                    // history 타입일 때만 X 버튼에 삭제 이벤트를 연결합니다.
                    if (type === 'history') {
                        li.querySelector('.delete-btn').addEventListener('click', function (e) {
                            e.stopPropagation(); // 부모 li 클릭 이벤트 전파 방지
                            deleteHistory(item.query, li);
                        });
                    }

                    historyList.appendChild(li);
                });
            }
            dropdown.classList.remove('hidden'); // 드롭다운 표시
        }

        /* =====================================================
           deleteHistory(query, liElement): 검색 기록 개별 삭제 함수
           X 버튼 클릭 시 해당 검색어 기록을 서버에서 삭제하고 DOM에서도 제거합니다.
           요청: GET SearchResults.aspx?action=deleteHistory&q={query}
           응답 JSON: { result: 'ok' }
           매개변수:
             query: 삭제할 검색어 문자열
             liElement: 제거할 li DOM 요소
        ===================================================== */
        function deleteHistory(query, liElement) {
            fetch('SearchResults.aspx?action=deleteHistory&q=' + encodeURIComponent(query))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.result === 'ok') {
                        liElement.remove(); // DOM에서 해당 항목 제거
                        // 목록이 비었으면 "기록 없음" 안내를 표시합니다.
                        if (historyList.children.length === 0) {
                            historyEmpty.classList.remove('hidden');
                            historyList.classList.add('hidden');
                        }
                    }
                });
        }

        /* =====================================================
           saveClick(anchor): 검색 결과 클릭 기록 저장 함수
           사용자가 검색 결과 링크를 클릭할 때 해당 정보를 서버에 AJAX로 저장합니다.
           저장 성공/실패 여부와 관계없이 새 탭에서 원본 URL을 엽니다(.finally 사용).
           요청: GET SearchResults.aspx?action=saveClick&q={query}&url={url}&title={title}&cat={카테고리}
           매개변수:
             anchor: 클릭된 <a> DOM 요소. href와 data-title 속성을 읽습니다.
        ===================================================== */
        function saveClick(anchor) {
            var url   = anchor.href;                                  // 클릭된 링크의 URL
            var title = anchor.getAttribute('data-title') || '';      // 결과 페이지 제목
            var query = currentQuery;                                  // 현재 검색어
            fetch('SearchResults.aspx?action=saveClick&q=' + encodeURIComponent(query) +
                  '&url=' + encodeURIComponent(url) +
                  '&title=' + encodeURIComponent(title) +
                  '&cat=' + encodeURIComponent('일반'))
                .finally(function () {
                    // 저장 성공/실패 무관하게 새 탭에서 원본 URL을 엽니다.
                    window.open(url, '_blank');
                });
        }

        /* =====================================================
           escapeHtml(str): HTML 특수문자 이스케이프 함수 (XSS 방지)
           검색어를 innerHTML에 삽입할 때 스크립트 주입(XSS) 공격을 방지합니다.
           변환: & → &amp;  < → &lt;  > → &gt;  " → &quot;
        ===================================================== */
        function escapeHtml(str) {
            return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
        }
    </script>
</asp:Content>
