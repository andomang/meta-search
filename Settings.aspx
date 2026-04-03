<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12 min-h-screen">
        <h1 class="text-3xl font-bold mb-10 dark:text-white">설정</h1>
        
        <div class="space-y-6">
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm transition-all">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold dark:text-white flex items-center gap-2">
                            화면 테마: <asp:Literal ID="litThemeStatus" runat="server"></asp:Literal>
                        </h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm">현재 테마 상태를 전환합니다.</p>
                    </div>
                    <asp:Button ID="btnToggleDark" runat="server" OnClick="btnToggleDark_Click" 
                        Text="테마 변경"
                        CssClass="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95 shadow-lg shadow-blue-200 dark:shadow-none" />
                </div>
            </div>

            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <h2 class="text-xl font-bold mb-6 dark:text-white">내 정보 수정</h2>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300">이름</label>
                        <asp:TextBox ID="editName" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>
                    <div>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300">이메일</label>
                        <asp:TextBox ID="editEmail" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>
                </div>
                <div class="mt-8 flex justify-end">
                    <asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" Text="정보 업데이트" 
                        CssClass="px-8 py-3 bg-gray-900 dark:bg-white dark:text-black text-white rounded-xl font-bold cursor-pointer hover:opacity-90 transition-all active:scale-95" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>