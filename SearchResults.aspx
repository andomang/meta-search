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
                <%-- min-w-0: flex 아이템이 extraPanel 가로 스크롤 콘텐츠에 밀려 사이드바를 찌부시키는 문제 방지 --%>
                <div class="flex-1 min-w-0">
                    <%-- litResultsLabel: 결과 개수 안내 텍스트 + AI 분류 키워드 배지 --%>
                    <div class="flex items-center gap-2 mb-6">
                        <p class="text-sm text-gray-600 dark:text-slate-400"><asp:Literal ID="litResultsLabel" runat="server"></asp:Literal></p>
                        <%-- keywordBadge: JavaScript에서 hdnKeyword 값을 읽어 동적으로 채움 --%>
                        <span id="keywordBadge" class="hidden px-2.5 py-0.5 bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300 text-xs font-semibold rounded-full border border-blue-200 dark:border-blue-700/50"></span>
                    </div>

                    <%-- extraPanel: AI 분류 키워드에 따라 카카오 장소/네이버 쇼핑·뉴스·책/Wikipedia/환율 등 추가 정보 표시 --%>
                    <div id="extraPanel" class="hidden mb-6"></div>

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

                    <%-- 전체화면 로딩 오버레이: 검색 네비게이션 시 표시 --%>
                    <div id="pageLoadingOverlay" class="hidden fixed inset-0 z-[9999] bg-white dark:bg-slate-950 flex flex-col items-center justify-center gap-6">
                        <div class="w-10 h-10 border-4 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
                        <div class="w-full max-w-xl px-6 space-y-3">
                            <div class="h-5 bg-gray-100 dark:bg-slate-800 rounded-full animate-pulse"></div>
                            <div class="h-4 bg-gray-100 dark:bg-slate-800 rounded-full w-3/4 animate-pulse"></div>
                            <div class="h-4 bg-gray-100 dark:bg-slate-800 rounded-full w-1/2 animate-pulse"></div>
                        </div>
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

                    <%-- ===== 엔진 탭 (전체/구글/네이버/다음) ===== --%>
                    <%-- type="button" 필수: 없으면 기본값이 submit이라 폼 PostBack이 발생한다. --%>
                    <div class="flex gap-0 mb-6 border-b border-gray-200 dark:border-slate-700">
                        <button type="button" class="engine-tab px-4 py-2 text-sm font-medium border-b-2 border-blue-500 text-blue-600 dark:text-blue-400 -mb-px" data-tab="all">전체</button>
                        <button type="button" class="engine-tab px-4 py-2 text-sm font-medium text-gray-500 dark:text-slate-400 hover:text-gray-700 dark:hover:text-slate-300" data-tab="google">구글</button>
                        <button type="button" class="engine-tab px-4 py-2 text-sm font-medium text-gray-500 dark:text-slate-400 hover:text-gray-700 dark:hover:text-slate-300" data-tab="naver">네이버</button>
                        <button type="button" class="engine-tab px-4 py-2 text-sm font-medium text-gray-500 dark:text-slate-400 hover:text-gray-700 dark:hover:text-slate-300" data-tab="daum">다음</button>
                    </div>

                    <%-- resultsContainer: JavaScript의 renderResultsPage()가 동적으로 결과 HTML을 채운다. --%>
                    <div id="resultsContainer"></div>
                    <%-- paginationContainer: 페이지 번호 버튼 영역 (이전/숫자/다음). JavaScript에서 동적 생성. --%>
                    <div id="paginationContainer" class="mt-6"></div>
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

                    <%-- ===== 관련 검색어 사이드바 (NIM AI 생성) ===== --%>
                    <%-- 결과가 있을 때만 표시. JavaScript의 window.load에서 hidden 제거 --%>
                    <div id="relatedPanel" class="hidden bg-gray-50 dark:bg-slate-800 border dark:border-slate-700 rounded-2xl p-6">
                        <h3 class="font-semibold mb-4 dark:text-white flex items-center gap-2">
                            <i data-lucide="search" class="w-4 h-4 text-blue-500"></i>
                            <asp:Literal ID="litRelatedLabel" runat="server"></asp:Literal>
                        </h3>
                        <%-- JavaScript에서 NIM 생성 관련 검색어 a 태그를 동적으로 채운다 --%>
                        <div id="relatedList" class="space-y-2"></div>
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
    <%-- 엔진별 검색 결과 JSON (코드비하인드에서 직렬화하여 JavaScript로 전달) --%>
    <asp:HiddenField ID="hdnGoogleJson" runat="server" />
    <asp:HiddenField ID="hdnNaverJson"  runat="server" />
    <asp:HiddenField ID="hdnDaumJson"   runat="server" />
    <%-- 현재 검색어 키워드의 엔진 점수 배열 [구글, 네이버, 다음] JSON --%>
    <asp:HiddenField ID="hdnScores"     runat="server" />
    <%-- Claude AI 분류 키워드 (예: 국내맛집, IT기술) - 검색결과 상단 배지에 표시 --%>
    <asp:HiddenField ID="hdnKeyword"    runat="server" />
    <%-- 외부 API 추가 결과 JSON (카카오 장소, 네이버 쇼핑/뉴스/책, Wikipedia, 환율) --%>
    <asp:HiddenField ID="hdnExtraJson"   runat="server" />
    <%-- NIM AI 생성 관련 검색어 JSON 배열 (사이드바 관련 검색어 패널) --%>
    <asp:HiddenField ID="hdnRelatedJson" runat="server" />

    <script>
        /* =====================================================
           다국어 변수 초기화: HiddenField에서 서버가 주입한 텍스트 읽기
        ===================================================== */
        var recentLabel  = document.getElementById('<%= hdnRecentLabel.ClientID %>').value;
        var noHistLabel  = document.getElementById('<%= hdnNoHistLabel.ClientID %>').value;
        var searchPh     = document.getElementById('<%= hdnSearchPh.ClientID %>').value;
        var timesLabel   = document.getElementById('<%= hdnTimesLabel.ClientID %>').value;
        var currentQuery = document.getElementById('<%= hdnCurrentQuery.ClientID %>').value;

        document.getElementById('litRecentLabel').textContent = recentLabel;
        document.getElementById('historyEmpty').textContent   = noHistLabel;

        var searchInput  = document.getElementById('<%= txtSearch.ClientID %>');
        var dropdown     = document.getElementById('historyDropdown');
        var historyList  = document.getElementById('historyList');
        var historyEmpty = document.getElementById('historyEmpty');
        searchInput.placeholder = searchPh;

        /* =====================================================
           엔진별 검색 결과 JSON 파싱: 코드비하인드에서 직렬화하여 HiddenField에 저장한 값 읽기
           각 배열 원소 형식: { title, url, desc, src }
           engineScores: [구글점수, 네이버점수, 다음점수] — 가중치 믹스에 사용
        ===================================================== */
        var googleResults = [], naverResults = [], daumResults = [], engineScores = [1, 1, 1];
        try { googleResults = JSON.parse(document.getElementById('<%= hdnGoogleJson.ClientID %>').value || '[]'); } catch(e) {}
        try { naverResults  = JSON.parse(document.getElementById('<%= hdnNaverJson.ClientID  %>').value || '[]'); } catch(e) {}
        try { daumResults   = JSON.parse(document.getElementById('<%= hdnDaumJson.ClientID   %>').value || '[]'); } catch(e) {}
        try { engineScores  = JSON.parse(document.getElementById('<%= hdnScores.ClientID     %>').value || '[1,1,1]'); } catch(e) {}

        /* AI 분류 키워드 배지 표시: hdnKeyword 값이 있으면 결과 카운트 옆 배지에 렌더링 */
        var aiKeyword = document.getElementById('<%= hdnKeyword.ClientID %>').value;
        if (aiKeyword) {
            var kwBadge = document.getElementById('keywordBadge');
            kwBadge.textContent = aiKeyword;
            kwBadge.classList.remove('hidden');
        }

        /* 외부 API 추가 결과 파싱 (카카오/네이버/Wikipedia/환율) - window.load에서 렌더링 */
        var extraData = {};
        try { extraData = JSON.parse(document.getElementById('<%= hdnExtraJson.ClientID %>').value || '{}'); } catch(e) {}

        /* NIM AI 생성 관련 검색어 파싱 - window.load에서 사이드바에 렌더링 */
        var relatedData = [];
        try { relatedData = JSON.parse(document.getElementById('<%= hdnRelatedJson.ClientID %>').value || '[]'); } catch(e) {}

        /* =====================================================
           페이지네이션 상태 변수
           PAGE_SIZE     : 페이지당 결과 수
           activeItems   : 현재 배치의 결과 목록 (최대 100개)
           currentPage   : 전체 기준 현재 페이지 번호 (배치 2에서는 11~20)
           batchStartPage: 현재 배치의 시작 전역 페이지 번호 (배치 1=1, 배치 2=11, ...)
           batchNum      : 현재 배치 번호
           activeTab     : 현재 선택 탭 이름 ('all'/'google'/'naver'/'daum')
           isLoadingBatch: 배치 로딩 중 중복 요청 방지 플래그
        ===================================================== */
        var PAGE_SIZE      = 10;
        var activeItems    = [];
        var currentPage    = 1;
        var batchStartPage = 1;
        var batchNum       = 1;
        var activeTab      = 'all';
        var isLoadingBatch = false;

        /* =====================================================
           엔진 탭 클릭 이벤트: 탭 전환 시 해당 엔진 결과로 switchTab 호출
        ===================================================== */
        document.querySelectorAll('.engine-tab').forEach(function(tab) {
            tab.addEventListener('click', function() {
                // 활성 탭 스타일 전환
                document.querySelectorAll('.engine-tab').forEach(function(t) {
                    t.classList.remove('border-b-2', 'border-blue-500', 'text-blue-600', 'dark:text-blue-400', '-mb-px');
                    t.classList.add('text-gray-500', 'dark:text-slate-400');
                });
                this.classList.add('border-b-2', 'border-blue-500', 'text-blue-600', 'dark:text-blue-400', '-mb-px');
                this.classList.remove('text-gray-500', 'dark:text-slate-400');

                // 선택된 탭에 따라 결과 목록 결정 후 현재 배치 첫 페이지부터 렌더링
                var tabName = this.getAttribute('data-tab');
                activeTab = tabName;
                var items;
                if      (tabName === 'all')    items = weightedMix(googleResults, naverResults, daumResults, engineScores);
                else if (tabName === 'google') items = googleResults;
                else if (tabName === 'naver')  items = naverResults;
                else                           items = daumResults;
                switchTab(items);
            });
        });

        /* =====================================================
           switchTab(items): 탭 전환 시 결과 교체. 현재 배치의 시작 페이지로 초기화.
           batchStartPage 기준으로 전역 페이지 번호를 유지한다.
        ===================================================== */
        function switchTab(items) {
            activeItems = items || [];
            currentPage = batchStartPage;  // 배치 2라면 11페이지부터 시작
            renderResultsPage();
        }

        /* =====================================================
           weightedMix(g, n, d, s): 가중치 점수 기반 결과 교차 배치
           엔진을 점수 내림차순으로 먼저 정렬한 뒤 라운드마다 점수만큼 추가.
           예) 국내뉴스 [구글1, 네이버3, 다음2] → 정렬 후 네이버3→다음2→구글1 순
        ===================================================== */
        function weightedMix(g, n, d, s) {
            // 점수 내림차순 정렬: 높은 점수 엔진이 각 라운드의 앞자리를 차지
            var engines = [
                { items: g, score: s[0] },
                { items: n, score: s[1] },
                { items: d, score: s[2] }
            ];
            engines.sort(function(a, b) { return b.score - a.score; });

            var result = [], idx = [0, 0, 0];
            while (idx[0] < engines[0].items.length || idx[1] < engines[1].items.length || idx[2] < engines[2].items.length) {
                var added = 0;
                for (var e = 0; e < 3; e++) {
                    for (var i = 0; i < engines[e].score && idx[e] < engines[e].items.length; i++, idx[e]++) {
                        result.push(engines[e].items[idx[e]]);
                        added++;
                    }
                }
                if (added === 0) break; // 모든 엔진 소진 시 루프 종료
            }
            return result;
        }

        /* =====================================================
           renderResultsPage(): 현재 페이지에 해당하는 결과만 렌더링.
           currentPage는 전역 페이지 번호이므로 batchStartPage 기준 로컬 인덱스로 변환.
           예) batchStartPage=11, currentPage=13 → localPage=3 → slice(20, 30)
        ===================================================== */
        function renderResultsPage() {
            var container    = document.getElementById('resultsContainer');
            var noResultsDiv = document.getElementById('noResults');

            if (!activeItems || activeItems.length === 0) {
                container.innerHTML = '';
                document.getElementById('paginationContainer').innerHTML = '';
                if (currentQuery) noResultsDiv.style.display = 'flex';
                return;
            }
            noResultsDiv.style.display = 'none';

            // 배치 내 로컬 페이지 번호로 변환하여 슬라이스
            var localPage = currentPage - batchStartPage + 1;
            var start     = (localPage - 1) * PAGE_SIZE;
            var pageItems = activeItems.slice(start, start + PAGE_SIZE);

            var html = '';
            pageItems.forEach(function(item) {
                // 출처 엔진에 따라 배지 색상과 레이블 결정
                var src      = item.src  || '';
                var srcLabel = src === 'google' ? 'DDG' : src === 'naver' ? '네이버' : '다음';
                var srcCss   = src === 'google'
                    ? 'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-300'
                    : src === 'naver'
                    ? 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300'
                    : 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300';

                // 썸네일 이미지가 있는 경우 오른쪽에 표시 (네이버 뉴스 스타일)
                var imgHtml = '';
                if (item.img) {
                    imgHtml = '<img src="' + escapeHtml(item.img) + '" alt="" ' +
                              'class="w-28 h-20 object-cover rounded-lg flex-shrink-0 ml-4 bg-gray-100 dark:bg-slate-700" ' +
                              'onerror="this.style.display=\'none\'">';
                }

                html +=
                    '<div class="group mb-8 flex items-start" data-url="' + escapeHtml(item.url || '') + '">' +
                    '  <div class="flex-1 min-w-0">' +
                    '    <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-slate-400 mb-1">' +
                    '      <span class="text-xs px-2 py-0.5 rounded-full font-medium ' + srcCss + '">' + srcLabel + '</span>' +
                    '      <i data-lucide="globe" class="w-4 h-4 flex-shrink-0"></i>' +
                    '      <span class="truncate max-w-md">' + escapeHtml(item.url || '') + '</span>' +
                    '    </div>' +
                    '    <a href="' + escapeHtml(item.url || '') + '" target="_blank" ' +
                    '       class="text-xl text-blue-600 dark:text-blue-400 hover:underline cursor-pointer font-medium">' +
                    escapeHtml(item.title || '') +
                    '    </a>' +
                    '    <p class="mt-1 text-sm text-gray-700 dark:text-slate-300 line-clamp-2">' + escapeHtml(item.desc || '') + '</p>' +
                    '  </div>' +
                    imgHtml +
                    '</div>';
            });
            container.innerHTML = html;

            // 동적으로 삽입된 <i data-lucide="globe"> 아이콘 재초기화
            if (typeof lucide !== 'undefined') lucide.createIcons();

            // 페이지네이션 버튼 렌더링
            renderPagination(activeItems.length);

            // 이미지 없는 결과(네이버/DDG)에 og:image 비동기 보완 (Daum은 이미 img 있어 스킵됨)
            enrichImages(pageItems);
        }

        /* =====================================================
           renderPagination(totalCount): 전역 페이지 번호 기반 페이지네이션 버튼 렌더링.
           현재 배치의 페이지 범위(batchStartPage ~ batchLastPage)만 표시하며,
           마지막 페이지에서 다음 버튼 클릭 시 자동으로 다음 배치를 로드한다.
        ===================================================== */
        function renderPagination(totalCount) {
            var pgContainer  = document.getElementById('paginationContainer');
            var batchPages   = Math.ceil(totalCount / PAGE_SIZE);
            // 전역 마지막 페이지: 이전 배치 페이지 수 + 현재 배치 페이지 수
            var batchLastPage = batchStartPage - 1 + batchPages;

            // 배치 1이고 1페이지뿐이면 페이지네이션 불필요
            if (batchPages <= 1 && batchStartPage === 1) { pgContainer.innerHTML = ''; return; }

            // type='button' 필수: 미지정 시 기본값 submit → 폼 PostBack 발생
            var btnBase = 'inline-flex items-center justify-center w-9 h-9 rounded-lg text-sm font-medium transition-colors ';
            var btnNorm = btnBase + 'text-gray-600 dark:text-slate-400 hover:bg-gray-100 dark:hover:bg-slate-700';
            var btnAct  = btnBase + 'bg-blue-600 text-white cursor-default';
            var btnDis  = btnBase + 'text-gray-300 dark:text-slate-600 cursor-not-allowed';

            var html = '<div class="flex items-center justify-center gap-1 py-4 border-t border-gray-200 dark:border-slate-700">';

            // 이전 버튼: 배치 시작 페이지 미만으로는 이동 불가 (이전 배치 결과 미보유)
            if (currentPage > batchStartPage) {
                html += '<button type="button" class="' + btnNorm + '" onclick="goToPage(' + (currentPage - 1) + ')">' +
                        '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">' +
                        '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>' +
                        '</button>';
            } else {
                html += '<button type="button" class="' + btnDis + '" disabled>' +
                        '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">' +
                        '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>' +
                        '</button>';
            }

            // 페이지 번호: 현재 배치 범위 내에서 앞뒤 2개씩 표시
            var startPage = Math.max(batchStartPage, currentPage - 2);
            var endPage   = Math.min(batchLastPage,  currentPage + 2);

            // 배치 첫 페이지가 표시 범위 밖이면 첫 페이지 + ... 표시
            if (startPage > batchStartPage) {
                html += '<button type="button" class="' + btnNorm + '" onclick="goToPage(' + batchStartPage + ')">' + batchStartPage + '</button>';
                if (startPage > batchStartPage + 1) html += '<span class="px-1 text-gray-400">…</span>';
            }
            for (var p = startPage; p <= endPage; p++) {
                if (p === currentPage)
                    html += '<button type="button" class="' + btnAct + '">' + p + '</button>';
                else
                    html += '<button type="button" class="' + btnNorm + '" onclick="goToPage(' + p + ')">' + p + '</button>';
            }
            // 배치 마지막 페이지가 표시 범위 밖이면 ... + 마지막 페이지 표시
            if (endPage < batchLastPage) {
                if (endPage < batchLastPage - 1) html += '<span class="px-1 text-gray-400">…</span>';
                html += '<button type="button" class="' + btnNorm + '" onclick="goToPage(' + batchLastPage + ')">' + batchLastPage + '</button>';
            }

            // 다음 버튼: 배치 마지막 페이지 초과 시 다음 배치 로드 트리거
            html += '<button type="button" class="' + btnNorm + '" onclick="goToPage(' + (currentPage + 1) + ')">' +
                    '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">' +
                    '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/></svg>' +
                    '</button>';

            html += '</div>';
            pgContainer.innerHTML = html;
        }

        /* =====================================================
           goToPage(page): 지정 페이지로 이동.
           현재 배치 마지막 페이지를 초과하면 loadNextBatch() 호출.
        ===================================================== */
        function goToPage(page) {
            if (page < batchStartPage) return;  // 이전 배치는 접근 불가

            var batchLastPage = batchStartPage - 1 + Math.ceil(activeItems.length / PAGE_SIZE);
            if (page > batchLastPage) {
                // 배치 경계 초과 → 다음 배치 AJAX 로드
                loadNextBatch(page);
                return;
            }
            currentPage = page;
            renderResultsPage();
            document.getElementById('resultsContainer').scrollIntoView({ behavior: 'smooth', block: 'start' });
        }

        /* =====================================================
           loadNextBatch(targetPage): 서버에 다음 배치 크롤링을 AJAX로 요청.
           Naver/Daum offset 파라미터와 함께 crawlMore 액션 호출.
           응답 받으면 activeItems 교체 후 targetPage 렌더링.
        ===================================================== */
        function loadNextBatch(targetPage) {
            if (isLoadingBatch) return;
            isLoadingBatch = true;

            // 로딩 스피너 표시
            document.getElementById('resultsContainer').innerHTML =
                '<div class="flex flex-col items-center justify-center py-20">' +
                '<div class="w-10 h-10 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mb-4"></div>' +
                '<p class="text-gray-400 dark:text-slate-500 text-sm">다음 검색 결과를 가져오는 중...</p>' +
                '</div>';
            document.getElementById('paginationContainer').innerHTML = '';

            // targetPage 기반으로 배치 번호 계산 (11페이지 → 배치 2, 21페이지 → 배치 3)
            var newBatchNum = Math.ceil(targetPage / 10);

            fetch('SearchResults.aspx?action=crawlMore&q=' + encodeURIComponent(currentQuery) + '&batch=' + newBatchNum)
                .then(function(res) { return res.json(); })
                .then(function(data) {
                    googleResults = data.google || [];
                    naverResults  = data.naver  || [];
                    daumResults   = data.daum   || [];

                    // 현재 탭에 맞는 결과 혼합
                    var items;
                    if      (activeTab === 'all')    items = weightedMix(googleResults, naverResults, daumResults, engineScores);
                    else if (activeTab === 'google') items = googleResults;
                    else if (activeTab === 'naver')  items = naverResults;
                    else                             items = daumResults;

                    if (!items || items.length === 0) {
                        isLoadingBatch = false;
                        document.getElementById('resultsContainer').innerHTML =
                            '<p class="text-center text-gray-400 dark:text-slate-500 py-10">더 이상 검색 결과가 없습니다.</p>';
                        return;
                    }

                    // 배치 상태 갱신 후 렌더링
                    batchNum       = newBatchNum;
                    batchStartPage = (newBatchNum - 1) * 10 + 1;  // 배치2→11, 배치3→21
                    activeItems    = items;
                    currentPage    = batchStartPage;               // 새 배치의 첫 페이지
                    isLoadingBatch = false;

                    renderResultsPage();
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                })
                .catch(function() {
                    isLoadingBatch = false;
                    document.getElementById('resultsContainer').innerHTML =
                        '<p class="text-center text-red-400 dark:text-red-500 py-10">결과를 가져오지 못했습니다. 잠시 후 다시 시도해주세요.</p>';
                });
        }

        /* =====================================================
           페이지 로드 완료 시: 결과 초기 렌더링 + 인기검색어 단위 텍스트 설정
        ===================================================== */
        window.addEventListener('load', function() {
            // 배치 1 초기 상태 설정 후 전체 탭 렌더링
            var total = googleResults.length + naverResults.length + daumResults.length;
            if (total > 0) {
                activeTab      = 'all';
                batchNum       = 1;
                batchStartPage = 1;
                switchTab(weightedMix(googleResults, naverResults, daumResults, engineScores));
            } else if (currentQuery) {
                // 검색어는 있지만 모든 크롤러가 결과를 못 가져온 경우
                document.getElementById('noResults').style.display = 'flex';
            }

            // 인기 검색어 Repeater 내 "회" 단위 텍스트 동적 설정
            document.querySelectorAll('[id$="litTimesInner"]').forEach(function(el) {
                el.textContent = timesLabel;
            });

            // 외부 API 추가 결과 패널 렌더링 (lucide 아이콘 사용하므로 load 이후 실행)
            if (extraData && extraData.type) renderExtraPanel(extraData);

            // NIM 관련 검색어 사이드바 렌더링
            if (relatedData && relatedData.length > 0) {
                var relList  = document.getElementById('relatedList');
                var relPanel = document.getElementById('relatedPanel');
                if (relList && relPanel) {
                    relatedData.forEach(function(term) {
                        var a = document.createElement('a');
                        a.href        = 'SearchResults.aspx?q=' + encodeURIComponent(term);
                        a.className   = 'block text-blue-600 dark:text-blue-400 hover:underline text-sm py-0.5 truncate';
                        a.textContent = term;
                        relList.appendChild(a);
                    });
                    relPanel.classList.remove('hidden');
                    if (typeof lucide !== 'undefined') lucide.createIcons();
                }
            }
        });

        /* =====================================================
           검색창 이벤트: 포커스 → 기록 드롭다운, 입력 → 자동완성, 엔터 → 검색
        ===================================================== */
        searchInput.addEventListener('focus', function() { loadHistory(); });

        var suggestTimer = null;
        searchInput.addEventListener('input', function() {
            clearTimeout(suggestTimer);
            var val = searchInput.value.trim();
            if (val.length === 0) { loadHistory(); return; }
            suggestTimer = setTimeout(function() { loadSuggest(val); }, 300);
        });

        document.addEventListener('click', function(e) {
            if (!document.getElementById('searchBox').contains(e.target))
                dropdown.classList.add('hidden');
        });

        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                var q = searchInput.value.trim();
                if (q) { showLoading(); location.href = 'SearchResults.aspx?q=' + encodeURIComponent(q); }
            }
        });

        /* =====================================================
           showLoading(): 로딩 스피너 표시
        ===================================================== */
        function showLoading() {
            // 결과 영역 인라인 스피너 + 전체화면 오버레이 동시 표시
            document.getElementById('loadingSpinner').classList.remove('hidden');
            document.getElementById('pageLoadingOverlay').classList.remove('hidden');
        }

        /* =====================================================
           loadHistory(): 최근 검색 기록 드롭다운 로드
        ===================================================== */
        function loadHistory() {
            fetch('SearchResults.aspx?action=getHistory')
                .then(function(res) { return res.json(); })
                .then(function(data) { renderDropdown(data, 'history'); })
                .catch(function() { dropdown.classList.add('hidden'); });
        }

        /* =====================================================
           loadSuggest(keyword): 자동완성 제안어 로드
        ===================================================== */
        function loadSuggest(keyword) {
            fetch('SearchResults.aspx?action=getSuggest&q=' + encodeURIComponent(keyword))
                .then(function(res) { return res.json(); })
                .then(function(data) {
                    if (!data || data.length === 0) { loadHistory(); return; }
                    renderDropdown(data.map(function(q) { return { query: q }; }), 'suggest');
                })
                .catch(function() { dropdown.classList.add('hidden'); });
        }

        /* =====================================================
           renderDropdown(data, type): 드롭다운 목록 렌더링
           type 'history' → X 삭제 버튼 포함, 'suggest' → 없음
        ===================================================== */
        function renderDropdown(data, type) {
            historyList.innerHTML = '';
            document.getElementById('litRecentLabel').textContent = recentLabel;

            if (!data || data.length === 0) {
                historyEmpty.classList.remove('hidden');
                historyList.classList.add('hidden');
            } else {
                historyEmpty.classList.add('hidden');
                historyList.classList.remove('hidden');
                data.forEach(function(item) {
                    var li = document.createElement('li');
                    li.className = 'flex items-center justify-between px-4 py-2 hover:bg-gray-50 dark:hover:bg-slate-700 cursor-pointer group';
                    var deleteBtn = type === 'history'
                        ? '<button class="delete-btn ml-2 text-gray-300 hover:text-red-400 dark:text-slate-600 dark:hover:text-red-400 flex-shrink-0"><svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>'
                        : '';
                    li.innerHTML =
                        '<span class="flex items-center gap-2 text-sm text-gray-700 dark:text-slate-300 flex-1 truncate search-query">' +
                        '<svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">' +
                        '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>' +
                        escapeHtml(item.query) + '</span>' + deleteBtn;

                    // li 전체 클릭 → 스피너 표시 후 이동 (.search-query span 외 패딩 영역도 포함)
                    li.addEventListener('click', function() {
                        showLoading();
                        location.href = 'SearchResults.aspx?q=' + encodeURIComponent(item.query);
                    });
                    if (type === 'history') {
                        li.querySelector('.delete-btn').addEventListener('click', function(e) {
                            e.stopPropagation(); // li 클릭과 중복 방지
                            deleteHistory(item.query, li);
                        });
                    }
                    historyList.appendChild(li);
                });
            }
            dropdown.classList.remove('hidden');
        }

        /* =====================================================
           deleteHistory(query, liElement): 검색 기록 개별 삭제
        ===================================================== */
        function deleteHistory(query, liElement) {
            fetch('SearchResults.aspx?action=deleteHistory&q=' + encodeURIComponent(query))
                .then(function(res) { return res.json(); })
                .then(function(data) {
                    if (data.result === 'ok') {
                        liElement.remove();
                        if (historyList.children.length === 0) {
                            historyEmpty.classList.remove('hidden');
                            historyList.classList.add('hidden');
                        }
                    }
                });
        }

        /* =====================================================
           enrichImages(items): img가 없는 결과에 og:image 비동기 보완
           현재 페이지 항목 URL을 서버에 POST하면, 서버가 각 URL의
           og:image / twitter:image 메타태그를 추출하여 JSON으로 반환.
           응답 후 DOM의 data-url 속성으로 대상 div를 찾아 이미지를 삽입한다.
           실패하거나 이미지가 없으면 결과 레이아웃에 변화 없음.
        ===================================================== */
        function enrichImages(items) {
            // img가 없는 항목의 URL만 수집 (Daum 결과는 이미 img 있어 제외됨)
            var urlsToFetch = [];
            items.forEach(function(item) {
                if (!item.img && item.url) urlsToFetch.push(item.url);
            });
            if (urlsToFetch.length === 0) return;  // 모두 이미지 있으면 조기 종료

            fetch('SearchResults.aspx?action=getImages', {
                method:  'POST',
                headers: { 'Content-Type': 'application/json' },
                body:    JSON.stringify(urlsToFetch)
            })
            .then(function(res) { return res.json(); })
            .then(function(map) {
                // data-url 속성으로 결과 div를 찾아 이미지 삽입
                var container = document.getElementById('resultsContainer');
                if (!container) return;
                var divs = container.querySelectorAll('[data-url]');
                divs.forEach(function(div) {
                    var url    = div.dataset.url;   // 브라우저가 HTML 엔티티 자동 디코딩
                    var imgUrl = map[url];
                    if (!imgUrl) return;
                    if (div.querySelector('img')) return;  // 이미 이미지 있으면 스킵

                    // Daum 썸네일과 동일한 스타일로 이미지 생성·삽입
                    var imgEl       = document.createElement('img');
                    imgEl.src       = imgUrl;
                    imgEl.alt       = '';
                    imgEl.className = 'w-28 h-20 object-cover rounded-lg flex-shrink-0 ml-4 bg-gray-100 dark:bg-slate-700';
                    imgEl.onerror   = function() { this.style.display = 'none'; };
                    div.appendChild(imgEl);

                    // activeItems 캐시 갱신: JS 객체는 참조 타입이므로 items 수정이 activeItems에도 반영됨
                    items.forEach(function(item) {
                        if (item.url === url) item.img = imgUrl;
                    });
                });
            })
            .catch(function() {});  // 네트워크 오류는 UI에 영향 없이 무시
        }

        /* =====================================================
           renderExtraPanel(data): AI 분류 키워드에 따른 외부 API 결과 패널 렌더링
           type별 함수: places/shopping/news/books/wiki/exchange
        ===================================================== */
        function renderExtraPanel(data) {
            var panel = document.getElementById('extraPanel');
            if (!panel || !data || !data.type) return;
            var inner = '';
            if      (data.type === 'places')   inner = renderPlaces(data);
            else if (data.type === 'shopping') inner = renderShopping(data);
            else if (data.type === 'news')     inner = renderNews(data);
            else if (data.type === 'books')    inner = renderBooks(data);
            else if (data.type === 'wiki')     inner = renderWiki(data);
            else if (data.type === 'exchange') inner = renderExchange(data);
            else if (data.type === 'youtube')  inner = renderYoutube(data);
            else if (data.type === 'weather')  inner = renderWeather(data);
            if (!inner) return;
            panel.innerHTML = '<div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-5">' + inner + '</div>';
            panel.classList.remove('hidden');
            // 동적으로 삽입된 lucide 아이콘 초기화
            if (typeof lucide !== 'undefined') lucide.createIcons();
        }

        /* 패널 공통 헤더: 아이콘 + 제목 + 출처 레이블 */
        function panelHeader(icon, title, source) {
            return '<div class="flex items-center justify-between mb-4">' +
                   '  <div class="flex items-center gap-2">' +
                   '    <i data-lucide="' + icon + '" class="w-4 h-4 text-blue-500"></i>' +
                   '    <span class="font-semibold text-sm text-gray-800 dark:text-white">' + escapeHtml(title) + '</span>' +
                   '  </div>' +
                   '  <span class="text-xs text-gray-400 dark:text-slate-500">제공: ' + escapeHtml(source) + '</span>' +
                   '</div>';
        }

        /* 카카오 로컬 장소 카드 (가로 스크롤) */
        function renderPlaces(data) {
            var items = data.items || [];
            if (!items.length) return '';
            var html = panelHeader('map-pin', '장소 정보', '카카오맵');
            html += '<div class="flex gap-3 overflow-x-auto pb-2">';
            items.forEach(function(p) {
                // 카테고리 마지막 세부 항목만 표시 (예: "음식점 > 한식 > 삼겹살" → "삼겹살")
                var cat = p.category ? p.category.split('>').pop().trim() : '';
                html +=
                    '<a href="' + escapeHtml(p.url || '#') + '" target="_blank" rel="noopener" ' +
                    '   class="flex-shrink-0 w-44 bg-gray-50 dark:bg-slate-700/50 rounded-xl p-3 hover:bg-gray-100 dark:hover:bg-slate-700 transition-colors" style="text-decoration:none">' +
                    '  <div class="font-semibold text-sm text-gray-900 dark:text-white truncate mb-1">' + escapeHtml(p.name) + '</div>' +
                    (cat ? '  <div class="text-xs text-blue-500 dark:text-blue-400 mb-1">' + escapeHtml(cat) + '</div>' : '') +
                    '  <div class="text-xs text-gray-500 dark:text-slate-400 mb-1" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden">' + escapeHtml(p.address) + '</div>' +
                    (p.phone ? '  <div class="text-xs text-gray-400 dark:text-slate-500">' + escapeHtml(p.phone) + '</div>' : '') +
                    '</a>';
            });
            // 카카오맵 전체 검색 링크 카드
            html += '<a href="https://map.kakao.com/?q=' + encodeURIComponent(currentQuery || '') + '" target="_blank" rel="noopener" ' +
                    '   class="flex-shrink-0 w-44 bg-blue-50 dark:bg-blue-900/20 rounded-xl p-3 flex flex-col items-center justify-center gap-2 hover:bg-blue-100 dark:hover:bg-blue-900/40 transition-colors" style="text-decoration:none">' +
                    '  <i data-lucide="map" class="w-6 h-6 text-blue-500"></i>' +
                    '  <span class="text-xs text-blue-600 dark:text-blue-400 font-medium text-center">카카오맵에서<br>더 보기</span>' +
                    '</a>';
            html += '</div>';
            return html;
        }

        /* 네이버 쇼핑 상품 카드 (가로 스크롤) */
        function renderShopping(data) {
            var items = data.items || [];
            if (!items.length) return '';
            var html = panelHeader('shopping-bag', '쇼핑', '네이버 쇼핑');
            html += '<div class="flex gap-3 overflow-x-auto pb-2">';
            items.forEach(function(p) {
                var price = p.price ? parseInt(p.price, 10).toLocaleString() + '원' : '';
                html +=
                    '<a href="' + escapeHtml(p.link || '#') + '" target="_blank" rel="noopener" ' +
                    '   class="flex-shrink-0 w-36 bg-gray-50 dark:bg-slate-700/50 rounded-xl overflow-hidden hover:shadow-md transition-shadow" style="text-decoration:none">' +
                    (p.image
                        ? '<img src="' + escapeHtml(p.image) + '" alt="" class="w-full h-28 object-cover" onerror="this.style.display=\'none\'">'
                        : '<div class="w-full h-28 bg-gray-200 dark:bg-slate-600 flex items-center justify-center"><i data-lucide="image" class="w-8 h-8 text-gray-400"></i></div>') +
                    '  <div class="p-2">' +
                    '    <div class="text-xs text-gray-800 dark:text-slate-200 font-medium mb-1" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden">' + escapeHtml(p.title) + '</div>' +
                    '    <div class="text-xs text-gray-500 dark:text-slate-400 truncate mb-0.5">' + escapeHtml(p.mall) + '</div>' +
                    (price ? '    <div class="text-xs font-bold text-blue-600 dark:text-blue-400">' + price + '</div>' : '') +
                    '  </div>' +
                    '</a>';
            });
            html += '</div>';
            return html;
        }

        /* 네이버 뉴스 헤드라인 리스트 */
        function renderNews(data) {
            var items = data.items || [];
            if (!items.length) return '';
            var html = panelHeader('newspaper', '뉴스', '네이버 뉴스');
            html += '<div class="space-y-1">';
            items.forEach(function(n, i) {
                var dateStr = formatNewsDate(n.pub);
                html +=
                    '<a href="' + escapeHtml(n.link || '#') + '" target="_blank" rel="noopener" ' +
                    '   class="flex gap-3 items-start rounded-lg p-2 -mx-2 hover:bg-gray-50 dark:hover:bg-slate-700/50 transition-colors" style="text-decoration:none">' +
                    '  <span class="text-xs font-bold text-blue-500 dark:text-blue-400 w-4 flex-shrink-0 mt-0.5">' + (i + 1) + '</span>' +
                    '  <div class="flex-1 min-w-0">' +
                    '    <div class="text-sm font-medium text-gray-800 dark:text-slate-200 truncate">' + escapeHtml(n.title) + '</div>' +
                    (n.desc ? '    <div class="text-xs text-gray-500 dark:text-slate-400 truncate mt-0.5">' + escapeHtml(n.desc) + '</div>' : '') +
                    '  </div>' +
                    (dateStr ? '<span class="text-xs text-gray-400 dark:text-slate-500 flex-shrink-0">' + escapeHtml(dateStr) + '</span>' : '') +
                    '</a>';
            });
            html += '</div>';
            return html;
        }

        /* 네이버 책 카드 (가로 스크롤) */
        function renderBooks(data) {
            var items = data.items || [];
            if (!items.length) return '';
            var html = panelHeader('book-open', '관련 도서', '네이버 책');
            html += '<div class="flex gap-4 overflow-x-auto pb-2">';
            items.forEach(function(b) {
                var price = b.price ? parseInt(b.price, 10).toLocaleString() + '원' : '';
                html +=
                    '<a href="' + escapeHtml(b.link || '#') + '" target="_blank" rel="noopener" ' +
                    '   class="flex-shrink-0 w-28 hover:opacity-80 transition-opacity" style="text-decoration:none">' +
                    (b.image
                        ? '<img src="' + escapeHtml(b.image) + '" alt="" class="w-full h-40 object-cover rounded-lg shadow mb-2" onerror="this.style.display=\'none\'">'
                        : '<div class="w-full h-40 bg-gray-200 dark:bg-slate-600 rounded-lg mb-2 flex items-center justify-center"><i data-lucide="book" class="w-8 h-8 text-gray-400"></i></div>') +
                    '  <div class="text-xs text-gray-800 dark:text-slate-200 font-medium mb-0.5" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden">' + escapeHtml(b.title) + '</div>' +
                    '  <div class="text-xs text-gray-500 dark:text-slate-400 truncate">' + escapeHtml(b.author) + '</div>' +
                    (price ? '  <div class="text-xs font-bold text-blue-600 dark:text-blue-400 mt-0.5">' + price + '</div>' : '') +
                    '</a>';
            });
            html += '</div>';
            return html;
        }

        /* Wikipedia 요약 박스 */
        function renderWiki(data) {
            if (!data.extract) return '';
            var html = panelHeader('globe', '위키피디아', 'Wikipedia');
            html += '<div class="flex gap-4">';
            if (data.thumbnail) {
                html += '<img src="' + escapeHtml(data.thumbnail) + '" alt="" ' +
                        '     class="w-20 h-20 object-cover rounded-lg flex-shrink-0" onerror="this.style.display=\'none\'">';
            }
            html +=
                '<div class="flex-1 min-w-0">' +
                '  <div class="font-semibold text-gray-900 dark:text-white text-sm mb-1">' + escapeHtml(data.title) + '</div>' +
                '  <p class="text-xs text-gray-600 dark:text-slate-400" style="display:-webkit-box;-webkit-line-clamp:4;-webkit-box-orient:vertical;overflow:hidden">' + escapeHtml(data.extract) + '</p>' +
                (data.url ? '  <a href="' + escapeHtml(data.url) + '" target="_blank" rel="noopener" class="text-xs text-blue-500 hover:underline mt-1 inline-block">자세히 보기 →</a>' : '') +
                '</div>' +
                '</div>';
            return html;
        }

        /* 실시간 환율 카드 */
        function renderExchange(data) {
            if (!data.rates) return '';
            var labels = { USD: '달러', JPY: '엔화', EUR: '유로', CNY: '위안', GBP: '파운드' };
            var flags  = { USD: '🇺🇸', JPY: '🇯🇵', EUR: '🇪🇺', CNY: '🇨🇳', GBP: '🇬🇧' };
            var html = panelHeader('banknote', '실시간 환율', 'ExchangeRate-API');
            html += '<div class="grid grid-cols-2 sm:grid-cols-3 gap-3">';
            ['USD', 'JPY', 'EUR', 'CNY', 'GBP'].forEach(function(code) {
                if (!data.rates[code]) return;
                var rate = parseFloat(data.rates[code]).toLocaleString('ko-KR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                html +=
                    '<div class="bg-gray-50 dark:bg-slate-700/50 rounded-xl p-3">' +
                    '  <div class="flex items-center gap-1.5 mb-1">' +
                    '    <span>' + (flags[code] || '') + '</span>' +
                    '    <span class="text-xs font-bold text-gray-700 dark:text-slate-300">' + code + '</span>' +
                    '    <span class="text-xs text-gray-400 dark:text-slate-500">' + (labels[code] || '') + '</span>' +
                    '  </div>' +
                    '  <div class="text-xs text-gray-500 dark:text-slate-400">1 ' + code + ' =</div>' +
                    '  <div class="text-sm font-bold text-blue-600 dark:text-blue-400">' + rate + ' 원</div>' +
                    '</div>';
            });
            html += '</div>';
            // 업데이트 시각: "Wed, 29 Jul 2026 00:00:01 +0000" → "Wed, 29 Jul 2026" 로 간소화
            if (data.updated) {
                var upd = data.updated.replace(/\s+\d+:\d+:\d+.*$/, '');
                html += '<div class="mt-3 text-xs text-gray-400 dark:text-slate-500">업데이트: ' + escapeHtml(upd) + ' | USD 기준</div>';
            }
            return html;
        }

        /* YouTube 영상 카드 (가로 스크롤) */
        function renderYoutube(data) {
            var items = data.items || [];
            if (!items.length) return '';
            var html = panelHeader('play-circle', 'YouTube 영상', 'YouTube Data API');
            html += '<div class="flex gap-3 overflow-x-auto pb-2">';
            items.forEach(function(v) {
                html +=
                    '<a href="https://www.youtube.com/watch?v=' + encodeURIComponent(v.id) + '" target="_blank" rel="noopener" ' +
                    '   class="flex-shrink-0 w-52 bg-gray-50 dark:bg-slate-700/50 rounded-xl overflow-hidden hover:shadow-md transition-shadow" style="text-decoration:none">' +
                    '  <div class="relative">' +
                    (v.thumb
                        ? '<img src="' + escapeHtml(v.thumb) + '" alt="" class="w-full h-28 object-cover">'
                        : '<div class="w-full h-28 bg-gray-200 dark:bg-slate-600"></div>') +
                    // 재생 버튼 오버레이
                    '    <div class="absolute inset-0 flex items-center justify-center">' +
                    '      <div class="w-10 h-10 bg-red-600 rounded-full flex items-center justify-center bg-opacity-90">' +
                    '        <svg class="w-5 h-5 text-white ml-0.5" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>' +
                    '      </div>' +
                    '    </div>' +
                    '  </div>' +
                    '  <div class="p-2">' +
                    '    <div class="text-xs text-gray-800 dark:text-slate-200 font-medium mb-1" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden">' + escapeHtml(v.title) + '</div>' +
                    '    <div class="text-xs text-gray-500 dark:text-slate-400 truncate">' + escapeHtml(v.channel) + '</div>' +
                    '  </div>' +
                    '</a>';
            });
            html += '</div>';
            return html;
        }

        /* OpenWeatherMap 날씨 카드 */
        function renderWeather(data) {
            if (!data.temp) return '';
            // 날씨 아이콘: OWM 제공 PNG (2x 해상도)
            var iconUrl = data.icon ? 'https://openweathermap.org/img/wn/' + data.icon + '@2x.png' : '';
            var html = panelHeader('cloud-sun', '현재 날씨', 'OpenWeatherMap');
            html +=
                '<div class="flex flex-wrap items-center gap-6">' +
                // 왼쪽: 아이콘 + 기온 + 설명
                '  <div class="flex items-center gap-2">' +
                (iconUrl ? '<img src="' + iconUrl + '" alt="" class="w-16 h-16 flex-shrink-0">' : '') +
                '    <div>' +
                '      <div class="text-3xl font-bold text-gray-900 dark:text-white">' + escapeHtml(data.temp) + '°C</div>' +
                '      <div class="text-sm text-gray-600 dark:text-slate-300 capitalize">' + escapeHtml(data.desc) + '</div>' +
                '      <div class="text-xs text-gray-400 dark:text-slate-500">' +
                         escapeHtml(data.city) + (data.country ? ' (' + escapeHtml(data.country) + ')' : '') +
                '      </div>' +
                '    </div>' +
                '  </div>' +
                // 오른쪽: 세부 날씨 정보 그리드
                '  <div class="grid grid-cols-2 gap-x-8 gap-y-1.5 text-sm">' +
                '    <span class="text-gray-500 dark:text-slate-400">체감온도</span>' +
                '    <span class="font-medium text-gray-800 dark:text-slate-200">' + escapeHtml(data.feels) + '°C</span>' +
                '    <span class="text-gray-500 dark:text-slate-400">습도</span>' +
                '    <span class="font-medium text-gray-800 dark:text-slate-200">' + escapeHtml(data.humidity) + '%</span>' +
                '    <span class="text-gray-500 dark:text-slate-400">바람</span>' +
                '    <span class="font-medium text-gray-800 dark:text-slate-200">' + escapeHtml(data.wind) + ' m/s</span>' +
                '  </div>' +
                '</div>';
            return html;
        }

        /* 뉴스 날짜 포맷: RFC-2822 → "M월 D일" */
        function formatNewsDate(pubStr) {
            if (!pubStr) return '';
            try {
                var d = new Date(pubStr);
                if (isNaN(d.getTime())) return '';
                return (d.getMonth() + 1) + '월 ' + d.getDate() + '일';
            } catch(e) { return ''; }
        }

        /* =====================================================
           escapeHtml(str): XSS 방지용 HTML 특수문자 이스케이프
        ===================================================== */
        function escapeHtml(str) {
            if (!str) return '';
            return String(str).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
        }
    </script>
</asp:Content>
