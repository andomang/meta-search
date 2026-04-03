<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12">
        <h1 class="text-4xl font-semibold mb-8">설정</h1>
        
        <div class="space-y-6">
            <div class="bg-white border rounded-2xl p-6 shadow-sm">
                <div class="flex items-center gap-3 mb-6">
                    <i data-lucide="user" class="text-blue-500 w-6 h-6"></i>
                    <h2 class="text-xl font-semibold">계정 설정</h2>
                </div>
                <div class="flex justify-between items-center py-4 border-b">
                    <div>
                        <p class="font-medium">이름</p>
                        <p class="text-sm text-gray-500">An (사용자)</p>
                    </div>
                    <button type="button" class="text-blue-500 px-4 py-2 hover:bg-blue-50 rounded-lg">변경</button>
                </div>
                <div class="flex justify-between items-center py-4">
                    <div>
                        <p class="font-medium">이메일</p>
                        <p class="text-sm text-gray-500">user@example.com</p>
                    </div>
                    <button type="button" class="text-blue-500 px-4 py-2 hover:bg-blue-50 rounded-lg">변경</button>
                </div>
            </div>

            <div class="bg-white border rounded-2xl p-6 shadow-sm flex justify-between items-center">
                <div class="flex items-center gap-3">
                    <i data-lucide="moon" class="text-purple-500 w-6 h-6"></i>
                    <div>
                        <p class="font-medium">다크 모드</p>
                        <p class="text-sm text-gray-500">어두운 테마를 사용합니다.</p>
                    </div>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" class="sr-only peer">
                    <div class="w-14 h-8 bg-gray-200 rounded-full peer peer-checked:bg-blue-500 after:content-[''] after:absolute after:top-1 after:left-1 after:bg-white after:border-gray-300 after:border after:rounded-full after:h-6 after:w-6 after:transition-all peer-checked:after:translate-x-6"></div>
                </label>
            </div>

            <div class="bg-white border rounded-2xl p-6 shadow-sm">
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3">
                        <i data-lucide="globe" class="text-green-500 w-6 h-6"></i>
                        <p class="font-medium">언어 설정</p>
                    </div>
                    <asp:DropDownList ID="ddlLanguage" runat="server" CssClass="border rounded-lg px-4 py-2 outline-none">
                        <asp:ListItem Value="ko">한국어</asp:ListItem>
                        <asp:ListItem Value="en">English</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
        </div>
    </div>
</asp:Content>