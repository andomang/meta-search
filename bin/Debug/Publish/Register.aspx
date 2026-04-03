<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-2xl mx-auto px-6 py-16 min-h-screen">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl p-10 shadow-sm">
            <h2 class="text-3xl font-extrabold mb-10 text-gray-900 dark:text-white tracking-tight">회원가입</h2>
            
            <div class="space-y-6">
                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300">아이디</label>
                    <asp:TextBox ID="txtUserID" runat="server" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500" placeholder="6~15자 영문/숫자"></asp:TextBox>
                </div>

                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300">비밀번호</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all" placeholder="비밀번호를 입력하세요"></asp:TextBox>
                </div>

                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300">이름</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300">닉네임</label>
                    <asp:TextBox ID="txtNickname" runat="server" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300">이메일</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <div class="pt-8 border-t dark:border-slate-700 flex justify-end gap-4">
                    <a href="Default.aspx" class="px-6 py-3.5 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-200 dark:hover:bg-slate-600 transition-all">취소</a>
                    <asp:Button ID="btnRegister" runat="server" Text="가입 완료" OnClick="btnRegister_Click" CssClass="px-10 py-3.5 bg-blue-600 dark:bg-blue-500 text-white rounded-xl font-bold hover:bg-blue-700 dark:hover:bg-blue-600 transition-all shadow-lg shadow-blue-100 dark:shadow-blue-950 cursor-pointer" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>