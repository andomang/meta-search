<%@ Page Title="Community" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Community.aspx.cs" Inherits="Community" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-6xl mx-auto px-6 py-12 min-h-screen">
        <div class="flex justify-between items-end mb-8 border-b dark:border-slate-700 pb-6">
            <h1 class="text-3xl font-bold dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h1>
            <a href="CommunityWrite.aspx" class="px-6 py-3 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 transition-all shadow-lg shadow-blue-100 dark:shadow-none flex items-center gap-2">
                <i data-lucide="pen-line" class="w-5 h-5"></i>
                <asp:Literal ID="litWriteBtn" runat="server"></asp:Literal>
            </a>
        </div>

        <%-- 검색 박스 --%>
        <div class="flex gap-2 mb-6">
            <div class="relative flex-1 max-w-md">
                <i data-lucide="search" class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400"></i>
                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="w-full pl-9 pr-4 py-2.5 border dark:border-slate-600 rounded-xl bg-white dark:bg-slate-800 dark:text-white text-sm outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
            </div>
            <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click"
                CssClass="px-5 py-2.5 bg-blue-600 text-white rounded-xl font-bold text-sm hover:bg-blue-700 transition-all cursor-pointer" />
            <asp:HyperLink ID="hlReset" runat="server" NavigateUrl="Community.aspx"
                CssClass="px-5 py-2.5 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-slate-300 rounded-xl font-bold text-sm hover:bg-gray-200 dark:hover:bg-slate-600 transition-all flex items-center">
                <asp:Literal ID="litResetBtn" runat="server"></asp:Literal>
            </asp:HyperLink>
        </div>

        <%-- 게시글 목록 테이블 --%>
        <div class="overflow-hidden border border-gray-200 dark:border-slate-700 rounded-2xl shadow-sm mb-6">
            <table class="w-full text-left border-collapse">
                <thead class="bg-gray-50 dark:bg-slate-800 border-b dark:border-slate-700 text-gray-600 dark:text-gray-300">
                    <tr>
                        <th class="px-6 py-4 text-sm font-semibold text-center w-16"><asp:Literal ID="litColNo" runat="server"></asp:Literal></th>
                        <th class="px-6 py-4 text-sm font-semibold"><asp:Literal ID="litColTitle" runat="server"></asp:Literal></th>
                        <th class="px-6 py-4 text-sm font-semibold w-28"><asp:Literal ID="litColAuthor" runat="server"></asp:Literal></th>
                        <th class="px-6 py-4 text-sm font-semibold w-28 text-center"><asp:Literal ID="litColDate" runat="server"></asp:Literal></th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-100 dark:divide-slate-800">
                    <asp:Repeater ID="rptPosts" runat="server">
                        <ItemTemplate>
                            <tr class="hover:bg-blue-50/50 dark:hover:bg-slate-800/50 transition-colors">
                                <td class="px-6 py-4 text-sm text-gray-400 text-center"><%# Eval("No") %></td>
                                <td class="px-6 py-4 font-semibold dark:text-gray-100">
                                    <a href='CommunityView.aspx?no=<%# Eval("No") %>' class="hover:text-blue-600"><%# Eval("Title") %></a>
                                    <%# Convert.ToInt32(Eval("LikeCount")) > 0 ? "<span class='ml-2 text-xs text-pink-500 font-bold'>♥ " + Eval("LikeCount") + "</span>" : "" %>
                                    <%# Convert.ToInt32(Eval("CommentCount")) > 0 ? "<span class='ml-1 text-xs text-blue-400 font-bold'>[" + Eval("CommentCount") + "]</span>" : "" %>
                                </td>
                                <td class="px-6 py-4 text-sm dark:text-gray-300">
                                    <%# Eval("AuthorName") %>
                                    <span class="ml-2 text-xs text-gray-400 dark:text-slate-500 whitespace-nowrap">
                                        <i data-lucide="eye" class="inline w-3 h-3 mr-0.5 align-middle"></i><%# Eval("Hits") %>
                                    </span>
                                </td>
                                <td class="px-6 py-4 text-sm text-gray-500 text-center"><%# Convert.ToDateTime(Eval("UploadTime")).ToString("yyyy.MM.dd") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%-- 페이지네이션 버튼 --%>
        <div class="flex items-center justify-center gap-4">
            <asp:Button ID="btnPrev" runat="server" OnClick="btnPage_Click"
                CssClass="px-5 py-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-700 transition-all disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer" />
            <span class="text-sm text-gray-500 dark:text-slate-400 font-medium">
                <asp:Literal ID="litPageInfo" runat="server"></asp:Literal>
            </span>
            <asp:Button ID="btnNext" runat="server" OnClick="btnPage_Click"
                CssClass="px-5 py-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-700 transition-all disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer" />
        </div>
    </div>
</asp:Content>
