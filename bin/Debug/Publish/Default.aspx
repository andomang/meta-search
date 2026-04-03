<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <%-- 전체 컨테이너: 다크모드 배경 지원 --%>
    <div class="flex flex-col items-center justify-center min-h-[calc(100vh-80px)] px-4 bg-white dark:bg-slate-950 transition-colors duration-300">
        
        <div class="w-full max-w-3xl text-center">
<%-- 🌈 그라데이션 타이틀 (pb-4 추가로 g 잘림 해결) --%>
<h1 class="text-6xl font-black mb-2 pb-4 bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent tracking-tighter leading-tight">
    Meta Search Engine
</h1>
            <p class="text-gray-500 dark:text-gray-400 text-xl mb-12 font-medium">모든 정보를 검색하세요</p>
            
            <%-- 🔍 중앙 검색창 섹션 --%>
            <div class="relative mb-16 group">
                <div class="absolute left-6 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-blue-500 transition-colors">
                    <i data-lucide="search" class="w-6 h-6"></i>
                </div>
                <asp:TextBox ID="txtSearch" runat="server" 
                    CssClass="w-full pl-16 pr-36 py-6 text-xl border border-gray-100 dark:border-slate-700 rounded-full shadow-xl shadow-blue-100/20 dark:shadow-none focus:outline-none focus:ring-4 focus:ring-blue-500/10 dark:focus:ring-blue-500/20 bg-white dark:bg-slate-800 dark:text-white transition-all placeholder:text-gray-300 dark:placeholder:text-gray-500" 
                    placeholder="무엇을 도와드릴까요?"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="검색" OnClick="btnSearch_Click" 
                    CssClass="absolute right-3 top-3 bottom-3 px-8 bg-slate-900 dark:bg-blue-600 text-white rounded-full font-bold hover:bg-blue-800 dark:hover:bg-blue-500 transition-all cursor-pointer active:scale-95 shadow-lg shadow-blue-200 dark:shadow-none" />
            </div>

            <%-- 🗂️ 하단 바로가기 카드 버튼 (좋다고 하셨던 디자인) --%>
            <div class="grid md:grid-cols-2 gap-6 max-w-2xl mx-auto text-left">
                <a href="Community.aspx" class="p-6 bg-gray-50 dark:bg-slate-800/50 rounded-3xl border border-transparent hover:border-blue-500/50 dark:hover:border-blue-500/50 transition-all group flex items-center gap-5 shadow-sm">
                    <div class="p-4 bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded-2xl group-hover:scale-110 transition-transform">
                        <i data-lucide="message-circle" class="w-6 h-6"></i>
                    </div>
                    <div>
                        <h3 class="font-bold text-gray-900 dark:text-white text-lg">자유게시판</h3>
                        <p class="text-sm text-gray-500 dark:text-gray-400">함께 이야기 나누는 공간</p>
                    </div>
                </a>

                <a href="Settings.aspx" class="p-6 bg-gray-50 dark:bg-slate-800/50 rounded-3xl border border-transparent hover:border-purple-500/50 dark:hover:border-purple-500/50 transition-all group flex items-center gap-5 shadow-sm">
                    <div class="p-4 bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400 rounded-2xl group-hover:scale-110 transition-transform">
                        <i data-lucide="sliders" class="w-6 h-6"></i>
                    </div>
                    <div>
                        <h3 class="font-bold text-gray-900 dark:text-white text-lg">개인 설정</h3>
                        <p class="text-sm text-gray-500 dark:text-gray-400">다크모드 및 환경 설정</p>
                    </div>
                </a>
            </div>
        </div>

    </div>
</asp:Content>