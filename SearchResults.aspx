<%@ Page Title="Search Results" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SearchResults.aspx.cs" Inherits="SearchResults" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-white dark:bg-slate-900 transition-colors">
        <div class="border-b border-gray-200 dark:border-slate-800 bg-white dark:bg-slate-900 sticky top-0 z-10">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 py-4">
                <div class="flex flex-col sm:flex-row sm:items-center gap-4">
                    <a href="Default.aspx" class="text-2xl font-semibold bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent flex-shrink-0">Search</a>
                    <div class="flex-1 max-w-2xl relative">
                        <i data-lucide="search" class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5"></i>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full pl-12 pr-4 py-3 border border-gray-300 dark:border-slate-600 rounded-full shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-slate-800 dark:text-white" placeholder="검색어를 입력하세요"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnSearch" runat="server" Text="검색" OnClick="btnSearch_Click" CssClass="hidden" />
                </div>
            </div>
        </div>

        <div class="max-w-7xl mx-auto px-4 sm:px-6 py-8">
            <div class="flex flex-col lg:flex-row gap-8">
                <div class="flex-1">
                    <p class="text-sm text-gray-600 dark:text-slate-400 mb-6">검색 결과 리스트입니다.</p>
                    <asp:Repeater ID="rptResults" runat="server">
                        <ItemTemplate>
                            <div class="group mb-8">
                                <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-slate-400 mb-1">
                                    <i data-lucide="globe" class="w-4 h-4"></i><span><%# Eval("Url") %></span>
                                </div>
                                <a href='<%# Eval("Url") %>' class="text-2xl text-blue-600 dark:text-blue-400 hover:underline"><%# Eval("Title") %></a>
                                <p class="mt-2 text-gray-700 dark:text-slate-300"><%# Eval("Description") %></p>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <div class="lg:w-80 space-y-6">
                    <div class="bg-gray-50 dark:bg-slate-800 border dark:border-slate-700 rounded-2xl p-6">
                        <h3 class="font-semibold mb-4 dark:text-white">관련 검색어</h3>
                        <div class="space-y-2">
                            <p class="text-blue-600 dark:text-blue-400 cursor-pointer hover:underline text-sm">ASP.NET Web Forms 강좌</p>
                            <p class="text-blue-600 dark:text-blue-400 cursor-pointer hover:underline text-sm">Tailwind CSS v4 사용법</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>