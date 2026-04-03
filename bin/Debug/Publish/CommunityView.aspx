<%@ Page Title="상세보기" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CommunityView.aspx.cs" Inherits="CommunityView" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12 min-h-screen bg-white dark:bg-slate-900">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl overflow-hidden shadow-sm hover:border-blue-100 dark:hover:border-blue-900 transition-all">
            <div class="p-8 border-b border-gray-100 dark:border-slate-700 bg-gray-50/50 dark:bg-slate-800/50 relative">
                <div class="flex items-center gap-2 mb-4">
                    <span class="px-3 py-1 bg-blue-100 dark:bg-blue-950 text-blue-600 dark:text-blue-300 text-xs font-bold rounded-full">자유게시판</span>
                </div>
                <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6"><asp:Literal ID="litTitle" runat="server"></asp:Literal></h1>
                
                <div class="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
                    <div class="flex items-center gap-4">
                        <span class="font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2.5">
                            <div class="w-8 h-8 rounded-full bg-blue-500 dark:bg-blue-600 flex items-center justify-center text-white font-bold text-xs"><asp:Literal ID="litAvatar" runat="server"></asp:Literal></div>
                            <asp:Literal ID="litAuthor" runat="server"></asp:Literal>
                        </span>
                        <span class="flex items-center gap-1"><i data-lucide="calendar" class="w-4 h-4"></i> <asp:Literal ID="litDate" runat="server"></asp:Literal></span>
                        <span class="flex items-center gap-1"><i data-lucide="eye" class="w-4 h-4"></i> <asp:Literal ID="litHits" runat="server"></asp:Literal></span>
                    </div>
                </div>
            </div>

            <div class="p-8 min-h-[300px]">
                <div class="text-gray-800 dark:text-gray-100 leading-relaxed text-lg whitespace-pre-wrap break-words font-medium"><asp:Literal ID="litContents" runat="server"></asp:Literal></div>
            </div>

            <asp:PlaceHolder ID="phFile" runat="server" Visible="false">
                <div class="mx-8 mb-8 p-5 bg-blue-50 dark:bg-blue-950 rounded-2xl border border-blue-100 dark:border-blue-900 flex items-center justify-between">
                    <div class="flex items-center gap-3">
                        <i data-lucide="paperclip" class="text-blue-500 dark:text-blue-400"></i>
                        <div>
                            <p class="text-sm font-bold text-gray-800 dark:text-gray-100"><asp:Literal ID="litFileName" runat="server"></asp:Literal></p>
                            <p class="text-xs text-blue-500 dark:text-blue-400"><asp:Literal ID="litFileSize" runat="server"></asp:Literal> KB</p>
                        </div>
                    </div>
                    <asp:HyperLink ID="hlDownload" runat="server" CssClass="px-4 py-2 bg-white dark:bg-slate-700 text-blue-600 dark:text-blue-300 rounded-xl text-sm font-bold shadow-sm hover:bg-blue-600 dark:hover:bg-blue-600 hover:text-white dark:hover:text-white transition-all">다운로드</asp:HyperLink>
                </div>
            </asp:PlaceHolder>

            <div class="p-8 border-t border-gray-100 dark:border-slate-700 flex justify-between bg-gray-50/30 dark:bg-slate-800/30">
                <a href="Community.aspx" class="px-6 py-3 bg-white dark:bg-slate-700 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-600 transition-all">목록으로</a>
                <asp:PlaceHolder ID="phOwnerActions" runat="server" Visible="false">
                    <div class="flex gap-2">
                        <asp:Button ID="btnDelete" runat="server" Text="삭제" OnClick="btnDelete_Click" CssClass="px-6 py-3 bg-red-50 dark:bg-red-950 text-red-600 dark:text-red-400 rounded-xl font-bold hover:bg-red-100 dark:hover:bg-red-900cursor-pointer" OnClientClick="return confirm('정말 삭제하시겠습니까?');" />
                    </div>
                </asp:PlaceHolder>
            </div>
        </div>
    </div>
</asp:Content>