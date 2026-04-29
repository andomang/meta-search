<%@ Page Title="Edit Post" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CommunityEdit.aspx.cs" Inherits="CommunityEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl p-8 shadow-sm">
            <h2 class="text-2xl font-bold mb-8 text-gray-900 dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h2>
            <div class="space-y-6">
                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litTitleLbl" runat="server"></asp:Literal></label>
                    <asp:TextBox ID="txtTitle" runat="server"
                        CssClass="w-full px-4 py-3 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>
                <div>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litContentLbl" runat="server"></asp:Literal></label>
                    <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="12"
                        CssClass="w-full px-4 py-3 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 resize-none bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>
                <div class="flex justify-end gap-4 pt-6 border-t dark:border-slate-700">
                    <a href="Community.aspx"
                        class="px-6 py-3 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-200 dark:hover:bg-slate-600 transition-all text-center">
                        <asp:Literal ID="litCancelBtn" runat="server"></asp:Literal>
                    </a>
                    <asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click"
                        CssClass="px-10 py-3 bg-blue-600 dark:bg-blue-500 text-white rounded-xl font-bold hover:bg-blue-700 dark:hover:bg-blue-600 transition-all shadow-lg shadow-blue-100 dark:shadow-blue-950 cursor-pointer" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
