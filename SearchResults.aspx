<%@ Page Title="Search Results" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SearchResults.aspx.cs" Inherits="SearchResults" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-white dark:bg-slate-900 transition-colors">

        <%-- 상단 고정 검색바 --%>
        <div class="border-b border-gray-200 dark:border-slate-800 bg-white dark:bg-slate-900 sticky top-0 z-10">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 py-4">
                <div class="flex flex-col sm:flex-row sm:items-center gap-4">
                    <a href="Default.aspx" class="text-2xl font-semibold bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent flex-shrink-0">Search</a>

                    <%-- 검색창 + 버튼 + 드롭다운 --%>
                    <div class="flex-1 max-w-2xl flex gap-2 relative" id="searchBox">
                        <div class="relative flex-1">
                            <i data-lucide="search" class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5 z-10"></i>
                            <asp:TextBox ID="txtSearch" runat="server"
                                CssClass="w-full pl-12 pr-4 py-3 border border-gray-300 dark:border-slate-600 rounded-full shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-slate-800 dark:text-white"
                                autocomplete="off" />

                            <%-- 최근 검색어 / 자동완성 통합 드롭다운 --%>
                            <div id="historyDropdown"
                                 class="hidden absolute left-0 right-0 top-full mt-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 rounded-2xl shadow-lg z-50 overflow-hidden">
                                <div class="flex items-center px-4 py-3 border-b border-gray-100 dark:border-slate-700">
                                    <span id="litRecentLabel" class="text-xs font-semibold text-gray-500 dark:text-slate-400"></span>
                                </div>
                                <ul id="historyList" class="py-1 max-h-64 overflow-y-auto"></ul>
                                <div id="historyEmpty" class="hidden px-4 py-4 text-sm text-gray-400 dark:text-slate-500 text-center"></div>
                            </div>
                        </div>
                        <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click"
                            CssClass="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-full transition-all flex-shrink-0 cursor-pointer" />
                </div>
            </div>
        </div>

        <div class="max-w-7xl mx-auto px-4 sm:px-6 py-8">
            <div class="flex flex-col lg:flex-row gap-8">

                <%-- 검색 결과 영역 --%>
                <div class="flex-1">
                    <p class="text-sm text-gray-600 dark:text-slate-400 mb-6"><asp:Literal ID="litResultsLabel" runat="server"></asp:Literal></p>

                    <%-- 로딩 스피너 (검색 버튼 클릭 시 표시, 결과 로드 후 숨김) --%>
                    <div id="loadingSpinner" class="hidden flex flex-col items-center justify-center py-20">
                        <div class="w-10 h-10 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mb-4"></div>
                        <p class="text-gray-400 dark:text-slate-500 text-sm"><asp:Literal ID="litLoadingLabel" runat="server"></asp:Literal></p>
                    </div>

                    <%-- 검색 결과 없음 UI --%>
                    <div id="noResults" style="display:none" class="flex-col items-center justify-center py-20 text-center">
                        <i data-lucide="search-x" class="w-16 h-16 text-gray-200 dark:text-slate-700 mb-4"></i>
                        <p class="text-lg font-bold text-gray-400 dark:text-slate-500"><asp:Literal ID="litNoResults" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-300 dark:text-slate-600 mt-1"><asp:Literal ID="litNoResultsSub" runat="server"></asp:Literal></p>
                    </div>

                    <%-- 검색 결과 Repeater --%>
                    <div id="resultsContainer">
                    <asp:Repeater ID="rptResults" runat="server">
                        <ItemTemplate>
                            <div class="group mb-8">
                                <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-slate-400 mb-1">
                                    <i data-lucide="globe" class="w-4 h-4"></i>
                                    <span><%# Eval("Url") %></span>
                                </div>
                                <%-- 클릭 시 saveClick AJAX 호출 후 원본 URL로 이동 --%>
                                <a href='<%# Eval("Url") %>'
                                   data-title='<%# Eval("Title") %>'
                                   onclick='saveClick(this); return false;'
                                   class="text-2xl text-blue-600 dark:text-blue-400 hover:underline cursor-pointer">
                                    <%# Eval("Title") %>
                                </a>
                                <p class="mt-2 text-gray-700 dark:text-slate-300"><%# Eval("Description") %></p>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    </div>
                </div>

                <%-- 사이드바 --%>
                <div class="lg:w-80 space-y-6">

                    <%-- 인기 검색어 사이드바 --%>
                    <div class="bg-gray-50 dark:bg-slate-800 border dark:border-slate-700 rounded-2xl p-6">
                        <h3 class="font-semibold mb-4 dark:text-white flex items-center gap-2">
                            <i data-lucide="trending-up" class="w-4 h-4 text-blue-500"></i>
                            <asp:Literal ID="litPopularLabel" runat="server"></asp:Literal>
                        </h3>
                        <asp:Repeater ID="rptPopular" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                                    <div class="flex items-center gap-2">
                                        <%-- 순위 번호 (1~3위는 파란색 강조) --%>
                                        <span class='text-xs font-bold w-5 text-center <%# Container.ItemIndex < 3 ? "text-blue-500" : "text-gray-400 dark:text-slate-500" %>'>
                                            <%# Container.ItemIndex + 1 %>
                                        </span>
                                        <a href='SearchResults.aspx?q=<%# Server.UrlEncode(Eval("Query").ToString()) %>'
                                           class="text-sm text-gray-700 dark:text-slate-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors">
                                            <%# Eval("Query") %>
                                        </a>
                                    </div>
                                    <span class="text-xs text-gray-400 dark:text-slate-500">
                                        <%# Eval("SearchCount") %><asp:Literal ID="litTimesInner" runat="server"></asp:Literal>
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <%-- 관련 검색어 (추후 실제 데이터로 교체) --%>
                    <div class="bg-gray-50 dark:bg-slate-800 border dark:border-slate-700 rounded-2xl p-6">
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

    <asp:HiddenField ID="hdnRecentLabel"  runat="server" />
    <asp:HiddenField ID="hdnNoHistLabel"  runat="server" />
    <asp:HiddenField ID="hdnSearchPh"     runat="server" />
    <asp:HiddenField ID="hdnTimesLabel"   runat="server" />
    <asp:HiddenField ID="hdnCurrentQuery" runat="server" />

    <script>
        // =====================
        // 다국어 변수 (서버에서 렌더링)
        // =====================
        var recentLabel  = document.getElementById('<%= hdnRecentLabel.ClientID %>').value;
        var noHistLabel  = document.getElementById('<%= hdnNoHistLabel.ClientID %>').value;
        var searchPh     = document.getElementById('<%= hdnSearchPh.ClientID %>').value;
        var timesLabel   = document.getElementById('<%= hdnTimesLabel.ClientID %>').value;
        var currentQuery = document.getElementById('<%= hdnCurrentQuery.ClientID %>').value;

        // 다국어 텍스트 DOM 초기화
        document.getElementById('litRecentLabel').textContent = recentLabel;
        document.getElementById('historyEmpty').textContent   = noHistLabel;

        var searchInput  = document.getElementById('<%= txtSearch.ClientID %>');
        var dropdown     = document.getElementById('historyDropdown');
        var historyList  = document.getElementById('historyList');
        var historyEmpty = document.getElementById('historyEmpty');
        var noResultsDiv = document.getElementById('noResults');

        // placeholder 설정
        searchInput.placeholder = searchPh;

        // 결과가 없으면 noResults UI 표시 (rptResults 비어있는 경우)
        window.addEventListener('load', function() {
            var items = document.querySelectorAll('#resultsContainer .group');
            if (currentQuery && items.length === 0) {
                noResultsDiv.style.display = 'flex';
            }

            // Repeater 내부 "회" 텍스트 설정
            document.querySelectorAll('[id$="litTimesInner"]').forEach(function(el) {
                el.textContent = timesLabel;
            });
        });

        // =====================
        // 검색창 이벤트
        // =====================

        // 포커스: 항상 최근 검색 기록 표시 (X 버튼 포함)
        searchInput.addEventListener('focus', function () {
            loadHistory();
        });

        // 입력 중: 자동완성 제안 표시 (300ms 디바운스)
        var suggestTimer = null;
        searchInput.addEventListener('input', function () {
            clearTimeout(suggestTimer);
            var val = searchInput.value.trim();
            if (val.length === 0) {
                loadHistory();
                return;
            }
            suggestTimer = setTimeout(function () {
                loadSuggest(val);
            }, 300);
        });

        // 외부 클릭 시 드롭다운 닫기
        document.addEventListener('click', function (e) {
            if (!document.getElementById('searchBox').contains(e.target)) {
                dropdown.classList.add('hidden');
            }
        });

        // 엔터키 검색
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                var q = searchInput.value.trim();
                if (q) {
                    showLoading();
                    location.href = 'SearchResults.aspx?q=' + encodeURIComponent(q);
                }
            }
        });

        // =====================
        // 로딩 스피너
        // =====================
        function showLoading() {
            document.getElementById('loadingSpinner').classList.remove('hidden');
        }

        // =====================
        // 최근 검색 기록 드롭다운
        // =====================
        function loadHistory() {
            fetch('SearchResults.aspx?action=getHistory')
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    renderDropdown(data, 'history');
                }).catch(function () { dropdown.classList.add('hidden'); });
        }

        // =====================
        // 자동완성 제안
        // =====================
        function loadSuggest(keyword) {
            fetch('SearchResults.aspx?action=getSuggest&q=' + encodeURIComponent(keyword))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (!data || data.length === 0) {
                        loadHistory(); // 제안 없으면 최근 기록 표시
                        return;
                    }
                    // 자동완성 결과를 history 드롭다운 형식으로 변환
                    var converted = data.map(function(q) { return { query: q }; });
                    renderDropdown(converted, 'suggest');
                }).catch(function () { dropdown.classList.add('hidden'); });
        }

        // =====================
        // 드롭다운 렌더링 공통 함수
        // type: 'history' (X 버튼 포함) | 'suggest' (X 버튼 없음)
        // =====================
        function renderDropdown(data, type) {
            historyList.innerHTML = '';
            document.getElementById('litRecentLabel').textContent = recentLabel;

            if (!data || data.length === 0) {
                historyEmpty.classList.remove('hidden');
                historyList.classList.add('hidden');
            } else {
                historyEmpty.classList.add('hidden');
                historyList.classList.remove('hidden');
                data.forEach(function (item) {
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

                    // 검색어 클릭 시 검색 실행
                    li.querySelector('.search-query').addEventListener('click', function () {
                        showLoading();
                        location.href = 'SearchResults.aspx?q=' + encodeURIComponent(item.query);
                    });

                    // X 버튼 클릭 시 기록 삭제 (history 타입만)
                    if (type === 'history') {
                        li.querySelector('.delete-btn').addEventListener('click', function (e) {
                            e.stopPropagation();
                            deleteHistory(item.query, li);
                        });
                    }

                    historyList.appendChild(li);
                });
            }
            dropdown.classList.remove('hidden');
        }

        // =====================
        // 검색 기록 삭제 (X 버튼)
        // =====================
        function deleteHistory(query, liElement) {
            fetch('SearchResults.aspx?action=deleteHistory&q=' + encodeURIComponent(query))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.result === 'ok') {
                        liElement.remove();
                        if (historyList.children.length === 0) {
                            historyEmpty.classList.remove('hidden');
                            historyList.classList.add('hidden');
                        }
                    }
                });
        }

        // =====================
        // 검색 결과 클릭 기록 저장
        // 클릭 시 AJAX로 saveClick 호출 후 원본 URL로 이동
        // =====================
        function saveClick(anchor) {
            var url   = anchor.href;
            var title = anchor.getAttribute('data-title') || '';
            var query = currentQuery;
            fetch('SearchResults.aspx?action=saveClick&q=' + encodeURIComponent(query) +
                  '&url=' + encodeURIComponent(url) +
                  '&title=' + encodeURIComponent(title) +
                  '&cat=' + encodeURIComponent('일반'))
                .finally(function () {
                    // 저장 성공/실패 무관하게 원본 URL로 이동
                    window.open(url, '_blank');
                });
        }

        // HTML 특수문자 이스케이프 (XSS 방지)
        function escapeHtml(str) {
            return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
        }
    </script>
</asp:Content>
