<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12 min-h-screen">
        <h1 class="text-3xl font-bold mb-10 dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h1>

        <div class="flex gap-2 mb-8 border-b dark:border-slate-700">
            <button type="button" onclick="switchTab('general')" id="tabGeneral"
                class="tab-btn px-6 py-3 font-bold text-sm border-b-2 border-blue-500 text-blue-500 dark:text-blue-400 transition-all">
                <asp:Literal ID="litTabGeneral" runat="server"></asp:Literal>
            </button>
            <button type="button" onclick="switchTab('privacy')" id="tabPrivacy"
                class="tab-btn px-6 py-3 font-bold text-sm border-b-2 border-transparent text-gray-400 hover:text-gray-600 dark:hover:text-slate-300 transition-all">
                <asp:Literal ID="litTabPrivacy" runat="server"></asp:Literal>
            </button>
        </div>

        <%-- 일반 탭 --%>
        <div id="panelGeneral" class="space-y-6">
            <%-- 프로필 사진 --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litProfilePhotoLabel" runat="server"></asp:Literal></h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litProfilePhotoSub" runat="server"></asp:Literal></p>
                    </div>
                    <div class="flex items-center gap-4">
                        <div class="w-14 h-14 rounded-full overflow-hidden bg-blue-500 flex items-center justify-center text-white text-xl font-bold flex-shrink-0">
                            <asp:Literal ID="litCurrentAvatar" runat="server"></asp:Literal>
                        </div>
                        <div class="flex flex-col gap-2">
                            <asp:FileUpload ID="fuPhoto" runat="server" CssClass="text-xs text-gray-500 dark:text-slate-400" />
                            <asp:Button ID="btnUploadPhoto" runat="server" OnClick="btnUploadPhoto_Click"
                                CssClass="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold text-sm cursor-pointer transition-all" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold dark:text-white flex items-center gap-2">
                            <asp:Literal ID="litThemeLabel" runat="server"></asp:Literal>: <asp:Literal ID="litThemeStatus" runat="server"></asp:Literal>
                        </h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litThemeSub" runat="server"></asp:Literal></p>
                    </div>
                    <asp:Button ID="btnToggleDark" runat="server" OnClick="btnToggleDark_Click"
                        CssClass="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95 shadow-lg shadow-blue-200 dark:shadow-none" />
                </div>
            </div>

            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litLangLabel" runat="server"></asp:Literal></h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litLangSub" runat="server"></asp:Literal></p>
                    </div>
                    <div class="flex gap-3">
                        <asp:Button ID="btnLangKo" runat="server" Text="한국어" OnClick="btnLangKo_Click"
                            CssClass="px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer" />
                        <asp:Button ID="btnLangEn" runat="server" Text="English" OnClick="btnLangEn_Click"
                            CssClass="px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer" />
                    </div>
                </div>
            </div>

            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <h2 class="text-xl font-bold mb-6 dark:text-white"><asp:Literal ID="litMyInfoLabel" runat="server"></asp:Literal></h2>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litNameLbl" runat="server"></asp:Literal></label>
                        <asp:TextBox ID="editName" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>
                    <div>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litNickLbl" runat="server"></asp:Literal></label>
                        <asp:TextBox ID="editNickname" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>
                    <div>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litEmailLbl" runat="server"></asp:Literal></label>
                        <asp:TextBox ID="editEmail" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>
                </div>
                <div class="mt-8 flex justify-between items-center">
                    <button type="button" onclick="openPwModal()" class="text-sm text-blue-500 hover:underline font-semibold">
                        <asp:Literal ID="litChangePwBtn" runat="server"></asp:Literal>
                    </button>
                    <asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click"
                        CssClass="px-8 py-3 bg-gray-900 dark:bg-white dark:text-black text-white rounded-xl font-bold cursor-pointer hover:opacity-90 transition-all active:scale-95" />
                </div>
            </div>
        </div>

        <%-- 개인정보 탭 --%>
        <div id="panelPrivacy" class="space-y-6 hidden">
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <h2 class="text-xl font-bold mb-6 dark:text-white"><asp:Literal ID="litStatsLabel" runat="server"></asp:Literal></h2>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                    <div class="bg-gray-50 dark:bg-slate-700 rounded-2xl p-5 text-center">
                        <p class="text-3xl font-black text-blue-500"><asp:Literal ID="litTotalSearch" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litTotalSearchLbl" runat="server"></asp:Literal></p>
                    </div>
                    <div class="bg-gray-50 dark:bg-slate-700 rounded-2xl p-5 text-center">
                        <p class="text-3xl font-black text-purple-500"><asp:Literal ID="litTotalClick" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litTotalClickLbl" runat="server"></asp:Literal></p>
                    </div>
                    <div class="bg-gray-50 dark:bg-slate-700 rounded-2xl p-5 text-center">
                        <p class="text-3xl font-black text-pink-500"><asp:Literal ID="litTopCategory" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litTopCategoryLbl" runat="server"></asp:Literal></p>
                    </div>
                </div>
                <h3 class="font-bold text-sm text-gray-500 dark:text-slate-400 mb-3"><asp:Literal ID="litTop5Lbl" runat="server"></asp:Literal></h3>
                <asp:Repeater ID="rptTopKeywords" runat="server">
                    <ItemTemplate>
                        <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                            <span class="text-sm font-medium dark:text-white"><%# Container.ItemIndex + 1 %>. <%# Eval("Query") %></span>
                            <span class="text-xs text-gray-400 dark:text-slate-500"><%# Eval("SearchCount") %>회</span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litDelSearchLabel" runat="server"></asp:Literal></h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litDelSearchSub" runat="server"></asp:Literal></p>
                    </div>
                    <button type="button" onclick="openDeleteModal('search')"
                        class="px-6 py-3 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95">
                        <asp:Literal ID="litDelSearchBtn" runat="server"></asp:Literal>
                    </button>
                </div>
            </div>

            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litDelClickLabel" runat="server"></asp:Literal></h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litDelClickSub" runat="server"></asp:Literal></p>
                    </div>
                    <button type="button" onclick="openDeleteModal('click')"
                        class="px-6 py-3 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95">
                        <asp:Literal ID="litDelClickBtn" runat="server"></asp:Literal>
                    </button>
                </div>
            </div>

            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border border-red-200 dark:border-red-900/50 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <h2 class="text-xl font-bold text-red-500"><asp:Literal ID="litWithdrawLabel" runat="server"></asp:Literal></h2>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litWithdrawSub" runat="server"></asp:Literal></p>
                    </div>
                    <button type="button" onclick="openWithdrawModal()"
                        class="px-6 py-3 bg-white dark:bg-slate-700 border border-red-300 dark:border-red-700 text-red-500 hover:bg-red-500 hover:text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95">
                        <asp:Literal ID="litWithdrawBtn" runat="server"></asp:Literal>
                    </button>
                </div>
            </div>
        </div>
    </div>

    <%-- 기록 삭제 모달 --%>
    <div id="deleteModal" class="hidden fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4">
        <div class="bg-white dark:bg-slate-800 p-8 rounded-2xl w-full max-w-sm shadow-2xl">
            <h2 class="text-xl font-bold mb-2 dark:text-white" id="deleteModalTitle"></h2>
            <p class="text-sm text-gray-500 dark:text-slate-400 mb-6"><asp:Literal ID="litDelModalDesc" runat="server"></asp:Literal></p>
            <div class="space-y-3 mb-8">
                <button type="button" onclick="confirmDelete('1hour')"  class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit1Hour" runat="server"></asp:Literal></button>
                <button type="button" onclick="confirmDelete('12hour')" class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit12Hour" runat="server"></asp:Literal></button>
                <button type="button" onclick="confirmDelete('1day')"   class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit1Day" runat="server"></asp:Literal></button>
                <button type="button" onclick="confirmDelete('7day')"   class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit7Day" runat="server"></asp:Literal></button>
                <button type="button" onclick="confirmDelete('30day')"  class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit30Day" runat="server"></asp:Literal></button>
                <button type="button" onclick="confirmDelete('all')"    class="del-btn w-full py-3 rounded-xl border border-red-300 dark:border-red-700 text-red-500 dark:text-red-400 hover:bg-red-500 hover:text-white font-bold transition-all text-left px-5"><asp:Literal ID="litDelAll" runat="server"></asp:Literal></button>
            </div>
            <button type="button" onclick="closeDeleteModal()" class="w-full text-gray-400 dark:text-slate-500 text-sm hover:text-gray-600 transition-all"><asp:Literal ID="litDelCancel" runat="server"></asp:Literal></button>
        </div>
    </div>

    <%-- 비밀번호 변경 모달 --%>
    <div id="pwModal" class="hidden fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4">
        <div class="bg-white dark:bg-slate-800 p-8 rounded-2xl w-full max-w-sm shadow-2xl">
            <h2 class="text-xl font-bold mb-6 dark:text-white"><asp:Literal ID="litPwModalTitle" runat="server"></asp:Literal></h2>
            <div class="space-y-4 mb-6">
                <div>
                    <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litPwCurLbl" runat="server"></asp:Literal></label>
                    <input type="password" id="pwCurrent" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                    <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litPwNewLbl" runat="server"></asp:Literal></label>
                    <input type="password" id="pwNew" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                    <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litPwConfirmLbl" runat="server"></asp:Literal></label>
                    <input type="password" id="pwConfirm" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <p id="pwError" class="text-red-500 text-xs hidden"></p>
            </div>
            <button type="button" onclick="submitPwChange()" class="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold transition-all mb-3"><asp:Literal ID="litPwSubmitBtn" runat="server"></asp:Literal></button>
            <button type="button" onclick="closePwModal()" class="w-full text-gray-400 text-sm hover:text-gray-600 transition-all"><asp:Literal ID="litPwCancelBtn" runat="server"></asp:Literal></button>
        </div>
    </div>

    <%-- 회원 탈퇴 모달 --%>
    <div id="withdrawModal" class="hidden fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4">
        <div class="bg-white dark:bg-slate-800 p-8 rounded-2xl w-full max-w-sm shadow-2xl">
            <h2 class="text-xl font-bold mb-2 text-red-500"><asp:Literal ID="litWdModalTitle" runat="server"></asp:Literal></h2>
            <p class="text-sm text-gray-500 dark:text-slate-400 mb-6"><asp:Literal ID="litWdDesc" runat="server"></asp:Literal></p>
            <div class="space-y-4 mb-6">
                <input type="password" id="withdrawPw" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-red-500" />
                <p id="withdrawError" class="text-red-500 text-xs hidden"></p>
            </div>
            <button type="button" onclick="submitWithdraw()" class="w-full py-3 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold transition-all mb-3"><asp:Literal ID="litWdSubmitBtn" runat="server"></asp:Literal></button>
            <button type="button" onclick="closeWithdrawModal()" class="w-full text-gray-400 text-sm hover:text-gray-600 transition-all"><asp:Literal ID="litWdCancelBtn" runat="server"></asp:Literal></button>
        </div>
    </div>

    <script>
        var deleteTarget = 'search';
        var msgDeleted = '<%= Lang.Get("set.delBtn") == "Delete" ? "Records deleted." : "기록이 삭제되었습니다." %>';
        var msgError = '<%= Lang.Get("set.delBtn") == "Delete" ? "An error occurred." : "오류가 발생했습니다." %>';
        var msgPwMismatch = '<%= Lang.Get("set.delBtn") == "Delete" ? "Passwords do not match." : "새 비밀번호가 일치하지 않습니다." %>';
        var msgPwShort = '<%= Lang.Get("set.delBtn") == "Delete" ? "Password must be at least 4 characters." : "비밀번호는 4자 이상이어야 합니다." %>';
        var msgPwRequired = '<%= Lang.Get("set.delBtn") == "Delete" ? "Please fill in all fields." : "모든 항목을 입력해주세요." %>';
        var msgPwWrong      = '<%= Lang.Get("set.delBtn") == "Delete" ? "Current password is incorrect." : "현재 비밀번호가 틀렸습니다." %>';
        var msgWdRequired   = '<%= Lang.Get("set.delBtn") == "Delete" ? "Please enter your password." : "비밀번호를 입력해주세요." %>';
        var msgWdWrong      = '<%= Lang.Get("set.delBtn") == "Delete" ? "Password is incorrect." : "비밀번호가 틀렸습니다." %>';
        var delSearchTitle  = '<%= Lang.Get("set.delSearch") %>';
        var delClickTitle   = '<%= Lang.Get("set.delClick") %>';

        function switchTab(tab) {
            document.getElementById('panelGeneral').classList.toggle('hidden', tab !== 'general');
            document.getElementById('panelPrivacy').classList.toggle('hidden', tab !== 'privacy');
            document.getElementById('tabGeneral').className = tab === 'general'
                ? 'tab-btn px-6 py-3 font-bold text-sm border-b-2 border-blue-500 text-blue-500 dark:text-blue-400 transition-all'
                : 'tab-btn px-6 py-3 font-bold text-sm border-b-2 border-transparent text-gray-400 hover:text-gray-600 dark:hover:text-slate-300 transition-all';
            document.getElementById('tabPrivacy').className = tab === 'privacy'
                ? 'tab-btn px-6 py-3 font-bold text-sm border-b-2 border-blue-500 text-blue-500 dark:text-blue-400 transition-all'
                : 'tab-btn px-6 py-3 font-bold text-sm border-b-2 border-transparent text-gray-400 hover:text-gray-600 dark:hover:text-slate-300 transition-all';
        }

        function openDeleteModal(target) {
            deleteTarget = target;
            document.getElementById('deleteModalTitle').textContent = target === 'search' ? delSearchTitle : delClickTitle;
            document.getElementById('deleteModal').classList.remove('hidden');
        }
        function closeDeleteModal() { document.getElementById('deleteModal').classList.add('hidden'); }
        function confirmDelete(range) {
            fetch('Settings.aspx?action=deleteHistory&target=' + deleteTarget + '&range=' + range)
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    closeDeleteModal();
                    alert(d.result === 'ok' ? msgDeleted : msgError);
                    if (d.result === 'ok') location.reload();
                });
        }
        document.getElementById('deleteModal').addEventListener('click', function (e) { if (e.target === this) closeDeleteModal(); });

        function openPwModal() { document.getElementById('pwModal').classList.remove('hidden'); }
        function closePwModal() {
            document.getElementById('pwModal').classList.add('hidden');
            document.getElementById('pwCurrent').value = '';
            document.getElementById('pwNew').value = '';
            document.getElementById('pwConfirm').value = '';
            document.getElementById('pwError').classList.add('hidden');
        }
        function submitPwChange() {
            var cur = document.getElementById('pwCurrent').value;
            var nw  = document.getElementById('pwNew').value;
            var cf  = document.getElementById('pwConfirm').value;
            var err = document.getElementById('pwError');
            if (!cur || !nw || !cf) { err.textContent = msgPwRequired; err.classList.remove('hidden'); return; }
            if (nw !== cf)          { err.textContent = msgPwMismatch; err.classList.remove('hidden'); return; }
            if (nw.length < 4)      { err.textContent = msgPwShort;    err.classList.remove('hidden'); return; }
            fetch('Settings.aspx?action=changePw&cur=' + encodeURIComponent(cur) + '&nw=' + encodeURIComponent(nw))
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    if (d.result === 'ok')    { closePwModal(); alert('<%= Lang.Get("set.delBtn") == "Delete" ? "Password changed." : "비밀번호가 변경되었습니다." %>'); }
                    else if (d.result === 'wrong') { err.textContent = msgPwWrong; err.classList.remove('hidden'); }
                    else { err.textContent = msgError; err.classList.remove('hidden'); }
                });
        }
        document.getElementById('pwModal').addEventListener('click', function (e) { if (e.target === this) closePwModal(); });

        function openWithdrawModal() { document.getElementById('withdrawModal').classList.remove('hidden'); }
        function closeWithdrawModal() {
            document.getElementById('withdrawModal').classList.add('hidden');
            document.getElementById('withdrawPw').value = '';
            document.getElementById('withdrawError').classList.add('hidden');
        }
        function submitWithdraw() {
            var pw  = document.getElementById('withdrawPw').value;
            var err = document.getElementById('withdrawError');
            if (!pw) { err.textContent = msgWdRequired; err.classList.remove('hidden'); return; }
            fetch('Settings.aspx?action=withdraw&pw=' + encodeURIComponent(pw))
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    if (d.result === 'ok')         { alert('<%= Lang.Get("set.delBtn") == "Delete" ? "Account deleted." : "탈퇴가 완료되었습니다." %>'); location.href = 'Default.aspx'; }
                    else if (d.result === 'wrong') { err.textContent = msgWdWrong; err.classList.remove('hidden'); }
                    else { err.textContent = msgError; err.classList.remove('hidden'); }
                });
        }
        document.getElementById('withdrawModal').addEventListener('click', function (e) { if (e.target === this) closeWithdrawModal(); });
    </script>
</asp:Content>