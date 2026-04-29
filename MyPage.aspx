<%@ Page Title="My Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-5xl mx-auto px-6 py-12 min-h-screen">

        <%-- 페이지 헤더 --%>
        <div class="mb-10">
            <h1 class="text-3xl font-bold dark:text-white mb-1"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h1>
            <p class="text-gray-500 dark:text-slate-400 text-sm">
                <asp:Literal ID="litWelcome" runat="server"></asp:Literal>,
                <span class="font-semibold text-blue-500"><%= Session["UserName"] %></span>
            </p>
        </div>

        <%-- 통계 카드 3개 --%>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-10">
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 text-center shadow-sm">
                <p class="text-xs font-semibold text-gray-400 dark:text-slate-500 mb-1"><asp:Literal ID="litTotalSearchLbl" runat="server"></asp:Literal></p>
                <p class="text-4xl font-extrabold text-blue-500"><asp:Literal ID="litTotalSearch" runat="server"></asp:Literal></p>
            </div>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 text-center shadow-sm">
                <p class="text-xs font-semibold text-gray-400 dark:text-slate-500 mb-1"><asp:Literal ID="litTotalClickLbl" runat="server"></asp:Literal></p>
                <p class="text-4xl font-extrabold text-purple-500"><asp:Literal ID="litTotalClick" runat="server"></asp:Literal></p>
            </div>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 text-center shadow-sm">
                <p class="text-xs font-semibold text-gray-400 dark:text-slate-500 mb-1"><asp:Literal ID="litTopKeywordLbl" runat="server"></asp:Literal></p>
                <p class="text-2xl font-extrabold text-pink-500 truncate"><asp:Literal ID="litTopKeyword" runat="server"></asp:Literal></p>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">

            <%-- 최근 검색어 카드 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 shadow-sm">
                <div class="flex items-center justify-between mb-5">
                    <h2 class="font-semibold dark:text-white flex items-center gap-2">
                        <i data-lucide="clock" class="w-4 h-4 text-blue-500"></i>
                        <asp:Literal ID="litRecentSearchLbl" runat="server"></asp:Literal>
                    </h2>
                    <a href="SearchResults.aspx" class="text-xs text-blue-500 hover:underline font-semibold">
                        <asp:Literal ID="litViewAllSearch" runat="server"></asp:Literal>
                    </a>
                </div>
                <asp:Literal ID="litNoSearch" runat="server" Visible="false"></asp:Literal>
                <asp:Repeater ID="rptRecentSearch" runat="server">
                    <ItemTemplate>
                        <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                            <a href='SearchResults.aspx?q=<%# Server.UrlEncode(Eval("Query").ToString()) %>'
                               class="text-sm text-gray-700 dark:text-slate-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors flex items-center gap-2">
                                <i data-lucide="search" class="w-3.5 h-3.5 text-gray-400 flex-shrink-0"></i>
                                <%# Eval("Query") %>
                            </a>
                            <span class="text-xs text-gray-400 dark:text-slate-500 flex-shrink-0">
                                <%# Convert.ToDateTime(Eval("SearchTime")).ToString("MM.dd") %>
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- 내 게시글 카드 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 shadow-sm">
                <div class="flex items-center justify-between mb-5">
                    <h2 class="font-semibold dark:text-white flex items-center gap-2">
                        <i data-lucide="file-text" class="w-4 h-4 text-purple-500"></i>
                        <asp:Literal ID="litMyPostsLbl" runat="server"></asp:Literal>
                    </h2>
                    <a href="Community.aspx" class="text-xs text-blue-500 hover:underline font-semibold">
                        <asp:Literal ID="litViewAllPosts" runat="server"></asp:Literal>
                    </a>
                </div>
                <asp:Literal ID="litNoPosts" runat="server" Visible="false"></asp:Literal>
                <asp:Repeater ID="rptMyPosts" runat="server">
                    <ItemTemplate>
                        <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                            <a href='CommunityView.aspx?no=<%# Eval("No") %>'
                               class="text-sm font-semibold text-gray-800 dark:text-gray-100 hover:text-blue-500 dark:hover:text-blue-400 transition-colors truncate max-w-[70%]">
                                <%# Eval("Title") %>
                            </a>
                            <span class="text-xs text-gray-400 dark:text-slate-500 flex-shrink-0">
                                <%# Convert.ToDateTime(Eval("UploadTime")).ToString("MM.dd") %>
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Content>
