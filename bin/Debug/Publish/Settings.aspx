<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-2xl mx-auto py-12 px-6 min-h-screen">
        <h2 class="text-3xl font-extrabold mb-10 dark:text-white">환경 설정</h2>
        <div class="space-y-6">
            <div class="flex justify-between items-center p-6 bg-white dark:bg-slate-800 rounded-3xl border border-gray-200 dark:border-slate-700 shadow-sm">
                <div class="flex items-center gap-5">
                    <div class="p-4 bg-indigo-50 dark:bg-indigo-900/40 text-indigo-600 dark:text-indigo-300 rounded-2xl"><i data-lucide="moon"></i></div>
                    <div>
                        <p class="font-bold text-lg dark:text-white">다크 모드</p>
                        <p class="text-sm text-gray-500 dark:text-gray-400">어두운 테마를 적용합니다.</p>
                    </div>
                </div>
                <asp:CheckBox ID="chkDarkMode" runat="server" AutoPostBack="true" OnCheckedChanged="SaveSettings" CssClass="scale-150 cursor-pointer" />
            </div>
            <div class="flex justify-between items-center p-6 bg-white dark:bg-slate-800 rounded-3xl border border-gray-200 dark:border-slate-700 shadow-sm">
                <div class="flex items-center gap-5">
                    <div class="p-4 bg-emerald-50 dark:bg-emerald-900/40 text-emerald-600 dark:text-emerald-300 rounded-2xl"><i data-lucide="languages"></i></div>
                    <div>
                        <p class="font-bold text-lg dark:text-white">언어 설정</p>
                        <p class="text-sm text-gray-500 dark:text-gray-400">표시 언어를 선택하세요.</p>
                    </div>
                </div>
                <asp:DropDownList ID="ddlLanguage" runat="server" AutoPostBack="true" OnSelectedIndexChanged="SaveSettings" CssClass="bg-gray-50 dark:bg-slate-700 border-none dark:text-white rounded-lg p-2 font-bold cursor-pointer">
                    <asp:ListItem Value="ko">한국어</asp:ListItem>
                    <asp:ListItem Value="en">English</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
    </div>
</asp:Content>