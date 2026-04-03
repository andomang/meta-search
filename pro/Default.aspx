<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="flex flex-col items-center justify-center min-h-[calc(100vh-80px)] px-4">
        <div class="w-full max-w-2xl text-center">
            <h1 class="text-6xl font-semibold mb-2 bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500 bg-clip-text text-transparent">Meta Search Engine</h1>
            <p class="text-gray-600 mb-12">모든 정보를 검색하세요</p>
            <div class="relative mb-8">
                <i data-lucide="search" class="absolute left-5 top-1/2 -translate-y-1/2 text-gray-400"></i>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full pl-14 pr-6 py-5 text-lg border rounded-full shadow-sm focus:outline-none focus:shadow-md transition-shadow" placeholder="검색어를 입력하세요"></asp:TextBox>
            </div>
            <div class="flex justify-center gap-4">
                <asp:Button ID="btnSearch" runat="server" Text="검색" OnClick="btnSearch_Click" CssClass="px-8 py-3 bg-gray-100 rounded-lg hover:bg-gray-200 cursor-pointer" />
            </div>
        </div>
    </div>
</asp:Content>