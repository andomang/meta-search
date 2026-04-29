<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="flex flex-col items-center justify-center min-h-[calc(100vh-80px)] px-4 bg-white dark:bg-slate-950 transition-colors duration-300">
        <div class="w-full max-w-3xl text-center">
            <h1 class="text-6xl font-black mb-2 pb-4 bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent tracking-tighter leading-tight">
                Meta Search Engine
            </h1>
            <p class="text-gray-500 dark:text-gray-400 text-xl mb-12 font-medium"><asp:Literal ID="litSubtitle" runat="server"></asp:Literal></p>

            <div class="relative mb-16 group" id="searchBox">
                <div class="absolute left-6 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-blue-500 transition-colors z-10">
                    <i data-lucide="search" class="w-6 h-6"></i>
                </div>
                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="w-full pl-16 pr-36 py-6 text-xl border border-gray-100 dark:border-slate-700 rounded-full shadow-xl shadow-blue-100/20 dark:shadow-none focus:outline-none focus:ring-4 focus:ring-blue-500/10 dark:focus:ring-blue-500/20 bg-white dark:bg-slate-800 dark:text-white transition-all placeholder:text-gray-300 dark:placeholder:text-gray-500"
                    autocomplete="off" />
                <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click"
                    CssClass="absolute right-3 top-3 bottom-3 px-8 bg-slate-900 dark:bg-blue-600 text-white rounded-full font-bold hover:bg-blue-800 dark:hover:bg-blue-500 transition-all cursor-pointer active:scale-95 shadow-lg shadow-blue-200 dark:shadow-none" />

                <!-- 최근 검색어 드롭다운 -->
                <div id="historyDropdown"
                     class="hidden absolute left-0 right-0 top-full mt-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 rounded-2xl shadow-lg z-50 overflow-hidden text-left">
                    <div class="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-slate-700">
                        <span id="litRecentLabel" class="text-xs font-semibold text-gray-500 dark:text-slate-400"></span>
                    </div>
                    <ul id="historyList" class="py-1 max-h-64 overflow-y-auto"></ul>
                    <div id="historyEmpty" class="hidden px-4 py-4 text-sm text-gray-400 dark:text-slate-500 text-center"></div>
                </div>
            </div>

            <div class="grid md:grid-cols-2 gap-6 max-w-2xl mx-auto text-left">
                <a href="Community.aspx" class="p-6 bg-gray-50 dark:bg-slate-800/50 rounded-3xl border border-transparent hover:border-blue-500/50 dark:hover:border-blue-500/50 transition-all group flex items-center gap-5 shadow-sm">
                    <div class="p-4 bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded-2xl group-hover:scale-110 transition-transform">
                        <i data-lucide="message-circle" class="w-6 h-6"></i>
                    </div>
                    <div>
                        <h3 class="font-bold text-gray-900 dark:text-white text-lg"><asp:Literal ID="litCommunityTitle" runat="server"></asp:Literal></h3>
                        <p class="text-sm text-gray-500 dark:text-gray-400"><asp:Literal ID="litCommunitySub" runat="server"></asp:Literal></p>
                    </div>
                </a>
                <a href="Settings.aspx" class="p-6 bg-gray-50 dark:bg-slate-800/50 rounded-3xl border border-transparent hover:border-purple-500/50 dark:hover:border-purple-500/50 transition-all group flex items-center gap-5 shadow-sm">
                    <div class="p-4 bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400 rounded-2xl group-hover:scale-110 transition-transform">
                        <i data-lucide="sliders" class="w-6 h-6"></i>
                    </div>
                    <div>
                        <h3 class="font-bold text-gray-900 dark:text-white text-lg"><asp:Literal ID="litSettingsTitle" runat="server"></asp:Literal></h3>
                        <p class="text-sm text-gray-500 dark:text-gray-400"><asp:Literal ID="litSettingsSub" runat="server"></asp:Literal></p>
                    </div>
                </a>
            </div>
        </div>
    </div>

    <script>
        var recentLabel = '<%= Lang.Get("search.recent") %>';
        var noHistoryLabel = '<%= Lang.Get("search.noHistory") %>';
        var searchPlaceholder = '<%= Lang.Get("search.placeholder") %>';
        document.getElementById('litRecentLabel').textContent = recentLabel;
        document.getElementById('historyEmpty').textContent = noHistoryLabel;

        const searchInput = document.getElementById('<%= txtSearch.ClientID %>');
        searchInput.placeholder = searchPlaceholder;
        const dropdown = document.getElementById('historyDropdown');
        const historyList = document.getElementById('historyList');
        const historyEmpty = document.getElementById('historyEmpty');

        searchInput.addEventListener('focus', function () { loadHistory(); });
        document.addEventListener('click', function (e) {
            if (!document.getElementById('searchBox').contains(e.target)) dropdown.classList.add('hidden');
        });
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); var q = searchInput.value.trim(); if (q) location.href = 'SearchResults.aspx?q=' + encodeURIComponent(q); }
        });

        function loadHistory() {
            fetch('SearchResults.aspx?action=getHistory')
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    historyList.innerHTML = '';
                    if (!data || data.length === 0) {
                        historyEmpty.classList.remove('hidden');
                        historyList.classList.add('hidden');
                    } else {
                        historyEmpty.classList.add('hidden');
                        historyList.classList.remove('hidden');
                        data.forEach(function (item) {
                            var li = document.createElement('li');
                            li.className = 'flex items-center justify-between px-4 py-2 hover:bg-gray-50 dark:hover:bg-slate-700 cursor-pointer';
                            li.innerHTML = '<span class="flex items-center gap-2 text-sm text-gray-700 dark:text-slate-300 flex-1 truncate search-query"><svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' + escapeHtml(item.query) + '</span><button class="delete-btn ml-2 text-gray-300 hover:text-red-400 dark:text-slate-600 dark:hover:text-red-400 flex-shrink-0" data-query="' + escapeHtml(item.query) + '"><svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>';
                            li.querySelector('.search-query').addEventListener('click', function () { location.href = 'SearchResults.aspx?q=' + encodeURIComponent(item.query); });
                            li.querySelector('.delete-btn').addEventListener('click', function (e) { e.stopPropagation(); deleteHistory(item.query, li); });
                            historyList.appendChild(li);
                        });
                    }
                    dropdown.classList.remove('hidden');
                }).catch(function () { dropdown.classList.add('hidden'); });
        }
        function deleteHistory(query, liElement) {
            fetch('SearchResults.aspx?action=deleteHistory&q=' + encodeURIComponent(query))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.result === 'ok') { liElement.remove(); if (historyList.children.length === 0) { historyEmpty.classList.remove('hidden'); historyList.classList.add('hidden'); } }
                });
        }
        function escapeHtml(str) { return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }
    </script>
</asp:Content>