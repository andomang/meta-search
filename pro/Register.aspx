<%@ Page Title="회원가입" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="flex items-center justify-center min-h-[calc(100vh-80px)] px-4 py-12">
        <div class="w-full max-w-md bg-white border border-gray-200 rounded-3xl p-8 shadow-sm">
            <div class="text-center mb-8">
                <h1 class="text-3xl font-bold mb-2">회원가입</h1>
                <p class="text-gray-500">새로운 계정을 만들어보세요</p>
            </div>

            <div class="space-y-4">
                <div>
                    <label class="block text-sm font-medium mb-1 ml-1">이름</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="w-full px-4 py-3 border rounded-xl outline-none focus:ring-2 focus:ring-blue-500" placeholder="이름을 입력하세요"></asp:TextBox>
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1 ml-1">이메일</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full px-4 py-3 border rounded-xl outline-none focus:ring-2 focus:ring-blue-500" placeholder="example@email.com"></asp:TextBox>
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1 ml-1">비밀번호</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full px-4 py-3 border rounded-xl outline-none focus:ring-2 focus:ring-blue-500" placeholder="••••••••"></asp:TextBox>
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1 ml-1">비밀번호 확인</label>
                    <asp:TextBox ID="txtPasswordConfirm" runat="server" TextMode="Password" CssClass="w-full px-4 py-3 border rounded-xl outline-none focus:ring-2 focus:ring-blue-500" placeholder="••••••••"></asp:TextBox>
                </div>

                <div class="pt-4">
                    <asp:Button ID="btnRegister" runat="server" Text="가입하기" OnClick="btnRegister_Click" 
                        CssClass="w-full py-4 bg-blue-500 text-white rounded-xl font-bold hover:bg-blue-600 transition-colors cursor-pointer" />
                </div>

                <p class="text-center text-sm text-gray-500 mt-4">
                    이미 계정이 있으신가요? <a href="javascript:openLogin();" class="text-blue-500 font-semibold hover:underline">로그인</a>
                </p>
            </div>
        </div>
    </div>
</asp:Content>