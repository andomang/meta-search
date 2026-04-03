<%@ Page Title="Community" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Community.aspx.cs" Inherits="Community" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-6xl mx-auto px-6 py-12">
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div class="lg:col-span-2">
                <h1 class="text-4xl font-semibold mb-8">커뮤니티</h1>
                <div class="bg-white border rounded-2xl p-6 shadow-sm mb-6">
                    <asp:TextBox ID="txtNewPost" runat="server" TextMode="MultiLine" Rows="3" CssClass="w-full p-4 border rounded-xl outline-none focus:ring-2 focus:ring-blue-500" placeholder="무엇을 공유하고 싶으신가요?"></asp:TextBox>
                    <div class="flex justify-end mt-3">
                        <asp:Button ID="btnPost" runat="server" Text="게시" OnClick="btnPost_Click" CssClass="px-6 py-2 bg-blue-500 text-white rounded-lg cursor-pointer" />
                    </div>
                </div>
                <asp:Repeater ID="rptPosts" runat="server">
                    <ItemTemplate>
                        <div class="bg-white border rounded-2xl p-6 shadow-sm mb-4">
                            <div class="flex items-start gap-4">
                                <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold"><%# Eval("Avatar") %></div>
                                <div class="flex-1">
                                    <div class="flex items-center gap-2 mb-1">
                                        <span class="font-semibold"><%# Eval("Author") %></span>
                                        <span class="text-sm text-gray-500"><%# Eval("Time") %></span>
                                    </div>
                                    <p class="text-gray-800"><%# Eval("Content") %></p>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="bg-white border rounded-2xl p-6 shadow-sm h-fit">
                <h2 class="text-xl font-semibold mb-4">커뮤니티 통계</h2>
                <div class="space-y-2 text-gray-600">
                    <div class="flex justify-between"><span>전체 회원</span><span class="font-bold">12,543</span></div>
                    <div class="flex justify-between"><span>오늘 방문자</span><span class="font-bold">1,284</span></div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>