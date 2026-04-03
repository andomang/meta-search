<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12">
        <h1 class="text-4xl font-semibold mb-8 text-gray-900 dark:text-white">설정</h1>
        
        <div class="space-y-6">
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 shadow-sm transition-colors">
                <div class="flex items-center gap-3 mb-6">
                    <i data-lucide="user" class="text-blue-500 w-6 h-6"></i>
                    <h2 class="text-xl font-semibold dark:text-white">계정 설정</h2>
                </div>
                
                <div class="flex justify-between items-center py-4 border-b dark:border-slate-700">
                    <div class="flex-1 mr-4">
                        <p class="font-medium text-gray-900 dark:text-slate-200">이름</p>
                        <asp:TextBox ID="editName" runat="server" CssClass="mt-1 w-full bg-transparent border-none p-0 text-sm text-gray-600 dark:text-slate-300 focus:ring-0 outline-none font-semibold"></asp:TextBox>
                    </div>
                    <asp:LinkButton ID="btnUpdateName" runat="server" OnClick="btnUpdate_Click" CssClass="text-blue-600 dark:text-blue-400 text-sm font-bold hover:underline">변경</asp:LinkButton>
                </div>

                <div class="flex justify-between items-center py-4">
                    <div class="flex-1 mr-4">
                        <p class="font-medium text-gray-900 dark:text-slate-200">이메일</p>
                        <asp:TextBox ID="editEmail" runat="server" CssClass="mt-1 w-full bg-transparent border-none p-0 text-sm text-gray-600 dark:text-slate-300 focus:ring-0 outline-none font-semibold"></asp:TextBox>
                    </div>
                    <asp:LinkButton ID="btnUpdateEmail" runat="server" OnClick="btnUpdate_Click" CssClass="text-blue-600 dark:text-blue-400 text-sm font-bold hover:underline">변경</asp:LinkButton>
                </div>
            </div>

            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 shadow-sm flex items-center justify-between transition-colors">
                <div class="flex items-center gap-3">
                    <i data-lucide="moon" class="text-purple-500 w-6 h-6"></i>
                    <div>
                        <p class="font-medium dark:text-slate-200">다크 모드</p>
                        <p class="text-sm text-gray-500 dark:text-slate-400">상태: <asp:Label ID="lblDarkStatus" runat="server"></asp:Label></p>
                    </div>
                </div>
                <asp:LinkButton ID="btnToggleDark" runat="server" OnClick="btnToggleDark_Click" 
                    CssClass="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold text-sm transition-all shadow-md active:scale-95">
                    테마 전환
                </asp:LinkButton>
            </div>
        </div>
    </div>
    <script>if (typeof lucide !== 'undefined') lucide.createIcons();</script>
</asp:Content>