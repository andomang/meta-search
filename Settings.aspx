<%@ Page Title="설정" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12">
        <h1 class="text-4xl font-bold mb-8 text-gray-900">설정</h1>
        
        <div class="space-y-6">
            <div class="bg-white border border-gray-200 rounded-3xl p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <div class="w-16 h-16 rounded-full bg-gradient-to-tr from-blue-600 to-purple-600 flex items-center justify-center text-white text-2xl font-bold shadow-lg">
                        <asp:Literal ID="litAvatar" runat="server"></asp:Literal>
                    </div>
                    <div>
                        <h2 class="text-2xl font-semibold"><asp:Literal ID="litUserNameTitle" runat="server"></asp:Literal>님, 안녕하세요!</h2>
                        <p class="text-gray-500">졸업 작품 프로젝트 계정 관리</p>
                    </div>
                </div>

                <div class="divide-y divide-gray-100">
                    <div class="flex justify-between items-center py-5">
                        <div>
                            <p class="text-sm font-medium text-gray-400 mb-1">이름</p>
                            <p class="text-lg font-semibold text-gray-800"><asp:Label ID="lblUserName" runat="server"></asp:Label></p>
                        </div>
                        <button type="button" class="px-4 py-2 text-blue-600 font-medium hover:bg-blue-50 rounded-xl transition-all">변경</button>
                    </div>
                    
                    <div class="flex justify-between items-center py-5">
                        <div>
                            <p class="text-sm font-medium text-gray-400 mb-1">이메일 주소</p>
                            <p class="text-lg font-semibold text-gray-800"><asp:Label ID="lblEmail" runat="server"></asp:Label></p>
                        </div>
                        <button type="button" class="px-4 py-2 text-blue-600 font-medium hover:bg-blue-50 rounded-xl transition-all">변경</button>
                    </div>
                </div>
            </div>

            <div class="bg-white border border-gray-200 rounded-3xl p-8 shadow-sm">
                <h3 class="text-lg font-semibold mb-6">시스템 환경</h3>
                <div class="flex justify-between items-center">
                    <div class="flex items-center gap-3">
                        <div class="p-2 bg-purple-100 rounded-xl text-purple-600"><i data-lucide="moon" class="w-5 h-5"></i></div>
                        <p class="font-medium">다크 모드</p>
                    </div>
                    <label class="relative inline-flex items-center cursor-pointer">
                        <input type="checkbox" class="sr-only peer">
                        <div class="w-12 h-6 bg-gray-200 rounded-full peer peer-checked:bg-blue-600 after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:after:translate-x-6"></div>
                    </label>
                </div>
            </div>
        </div>
    </div>
</asp:Content>