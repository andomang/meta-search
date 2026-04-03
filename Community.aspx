<%@ Page Title="Community" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Community.aspx.cs" Inherits="Community" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-6xl mx-auto px-6 py-12 min-h-screen">
        <div class="flex justify-between items-end mb-8 border-b dark:border-slate-700 pb-6">
            <h1 class="text-3xl font-bold dark:text-white">커뮤니티 광장</h1>
            <a href="CommunityWrite.aspx" class="px-6 py-3 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 transition-all shadow-lg shadow-blue-100 dark:shadow-none flex items-center gap-2">
                <i data-lucide="pen-line" class="w-5 h-5"></i> 글쓰기
            </a>
        </div>
        <div class="overflow-hidden border border-gray-200 dark:border-slate-700 rounded-2xl shadow-sm">
            <table class="w-full text-left border-collapse">
                <thead class="bg-gray-50 dark:bg-slate-800 border-b dark:border-slate-700 text-gray-600 dark:text-gray-300">
                    <tr>
                        <th class="px-6 py-4 text-sm font-semibold text-center w-20">번호</th>
                        <th class="px-6 py-4 text-sm font-semibold">제목</th>
                        <th class="px-6 py-4 text-sm font-semibold w-32">작성자</th>
                        <th class="px-6 py-4 text-sm font-semibold w-32 text-center">날짜</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-100 dark:divide-slate-800">
                    <asp:Repeater ID="rptPosts" runat="server">
                        <ItemTemplate>
                            <tr class="hover:bg-blue-50/50 dark:hover:bg-slate-800/50 transition-colors">
                                <td class="px-6 py-4 text-sm text-gray-400 text-center"><%# Eval("No") %></td>
                                <td class="px-6 py-4 font-semibold dark:text-gray-100">
                                    <a href='CommunityView.aspx?no=<%# Eval("No") %>' class="hover:text-blue-600"><%# Eval("Title") %></a>
                                </td>
                                <td class="px-6 py-4 text-sm dark:text-gray-300"><%# Eval("AuthorName") %></td>
                                <td class="px-6 py-4 text-sm text-gray-500 text-center"><%# Convert.ToDateTime(Eval("UploadTime")).ToString("yyyy.MM.dd") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </div>
</asp:Content>