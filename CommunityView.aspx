<%@ Page Title="View" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CommunityView.aspx.cs" Inherits="CommunityView" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12 min-h-screen">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl overflow-hidden shadow-sm">

            <%-- 게시글 헤더 --%>
            <div class="p-8 border-b border-gray-100 dark:border-slate-700 bg-gray-50/50 dark:bg-slate-800/50">
                <div class="flex items-center gap-2 mb-4">
                    <span class="px-3 py-1 bg-blue-100 dark:bg-blue-950 text-blue-600 dark:text-blue-300 text-xs font-bold rounded-full">
                        <asp:Literal ID="litBoardLabel" runat="server"></asp:Literal>
                    </span>
                </div>
                <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">
                    <asp:Literal ID="litTitle" runat="server"></asp:Literal>
                </h1>
                <div class="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
                    <div class="flex items-center gap-4">
                        <span class="font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2.5">
                            <div class="w-8 h-8 rounded-full overflow-hidden bg-blue-500 dark:bg-blue-600 flex items-center justify-center text-white font-bold text-xs flex-shrink-0">
                                <asp:Literal ID="litAvatar" runat="server"></asp:Literal>
                            </div>
                            <asp:Literal ID="litAuthor" runat="server"></asp:Literal>
                        </span>
                        <span class="flex items-center gap-1">
                            <i data-lucide="calendar" class="w-4 h-4"></i>
                            <asp:Literal ID="litDate" runat="server"></asp:Literal>
                        </span>
                        <span class="flex items-center gap-1">
                            <i data-lucide="eye" class="w-4 h-4"></i>
                            <asp:Literal ID="litHits" runat="server"></asp:Literal>
                        </span>
                    </div>
                </div>
            </div>

            <%-- 게시글 본문 --%>
            <div class="p-8 min-h-[300px]">
                <div class="text-gray-800 dark:text-gray-100 leading-relaxed text-lg whitespace-pre-wrap break-words font-medium">
                    <asp:Literal ID="litContents" runat="server"></asp:Literal>
                </div>
            </div>

            <%-- 첨부파일 --%>
            <asp:PlaceHolder ID="phFile" runat="server" Visible="false">
                <div class="mx-8 mb-8 p-5 bg-blue-50 dark:bg-blue-950 rounded-2xl border border-blue-100 dark:border-blue-900 flex items-center justify-between">
                    <div class="flex items-center gap-3">
                        <i data-lucide="paperclip" class="text-blue-500 dark:text-blue-400"></i>
                        <div>
                            <p class="text-sm font-bold text-gray-800 dark:text-gray-100"><asp:Literal ID="litFileName" runat="server"></asp:Literal></p>
                            <p class="text-xs text-blue-500 dark:text-blue-400"><asp:Literal ID="litFileSize" runat="server"></asp:Literal> KB</p>
                        </div>
                    </div>
                    <asp:HyperLink ID="hlDownload" runat="server"
                        CssClass="px-4 py-2 bg-white dark:bg-slate-700 text-blue-600 dark:text-blue-300 rounded-xl text-sm font-bold shadow-sm hover:bg-blue-600 dark:hover:bg-blue-600 hover:text-white dark:hover:text-white transition-all">
                        <asp:Literal ID="litDownloadBtn" runat="server"></asp:Literal>
                    </asp:HyperLink>
                </div>
            </asp:PlaceHolder>

            <%-- 좋아요 버튼 --%>
            <div class="px-8 pb-6 flex items-center gap-4">
                <button type="button" id="btnLike" onclick="toggleLike()"
                    class="flex items-center gap-2 px-6 py-3 rounded-xl border border-gray-200 dark:border-slate-600 text-gray-500 dark:text-slate-400 hover:border-pink-300 hover:text-pink-500 dark:hover:border-pink-700 dark:hover:text-pink-400 transition-all font-bold text-sm">
                    <i data-lucide="heart" class="w-5 h-5"></i>
                    <span id="likeCount">0</span>
                </button>
                <asp:HiddenField ID="hdnPostNo"   runat="server" />
                <asp:HiddenField ID="hdnIsLiked"  runat="server" Value="false" />
                <asp:HiddenField ID="hdnIsLogin"  runat="server" Value="false" />
            </div>

            <%-- 하단 버튼 --%>
            <div class="p-8 border-t border-gray-100 dark:border-slate-700 flex justify-between bg-gray-50/30 dark:bg-slate-800/30">
                <a href="Community.aspx" class="px-6 py-3 bg-white dark:bg-slate-700 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-600 transition-all">
                    <asp:Literal ID="litBackBtn" runat="server"></asp:Literal>
                </a>
                <asp:PlaceHolder ID="phOwnerActions" runat="server" Visible="false">
                    <div class="flex gap-2">
                        <asp:HyperLink ID="hlEdit" runat="server"
                            CssClass="px-6 py-3 bg-blue-50 dark:bg-blue-950 text-blue-600 dark:text-blue-400 rounded-xl font-bold hover:bg-blue-100 dark:hover:bg-blue-900 transition-all">
                        </asp:HyperLink>
                        <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click"
                            CssClass="px-6 py-3 bg-red-50 dark:bg-red-950 text-red-600 dark:text-red-400 rounded-xl font-bold hover:bg-red-100 dark:hover:bg-red-900 cursor-pointer transition-all" />
                    </div>
                </asp:PlaceHolder>
            </div>

            <%-- 댓글 섹션 --%>
            <div class="p-8 border-t border-gray-100 dark:border-slate-700">
                <h3 class="font-bold text-lg dark:text-white mb-6 flex items-center gap-2">
                    <i data-lucide="message-circle" class="w-5 h-5 text-blue-500"></i>
                    <asp:Literal ID="litCommentsLabel" runat="server"></asp:Literal>
                    <span id="commentCount" class="text-blue-500 text-base">(<asp:Literal ID="litCommentCount" runat="server">0</asp:Literal>)</span>
                </h3>

                <%-- 댓글 목록 --%>
                <div id="commentList">
                    <asp:Repeater ID="rptComments" runat="server">
                        <ItemTemplate>
                            <div class="flex gap-3 py-4 border-b dark:border-slate-700 last:border-0 comment-item" id="cmt_<%# Eval("CommentID") %>">
                                <div class="w-8 h-8 rounded-full bg-gray-200 dark:bg-slate-600 flex items-center justify-center text-sm font-bold text-gray-600 dark:text-gray-300 flex-shrink-0">
                                    <%# GetAvatarHtml(Eval("AuthorNick").ToString(), Eval("ProfileImg").ToString()) %>
                                </div>
                                <div class="flex-1 min-w-0">
                                    <div class="flex items-center justify-between mb-1">
                                        <div class="flex items-center gap-2">
                                            <span class="font-semibold text-sm dark:text-white"><%# Eval("AuthorNick") %></span>
                                            <span class="text-xs text-gray-400 dark:text-slate-500"><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("yyyy.MM.dd HH:mm") %></span>
                                        </div>
                                        <%# GetDeleteBtn(Eval("Author").ToString(), Eval("CommentID").ToString()) %>
                                    </div>
                                    <p class="text-sm dark:text-gray-200 break-words leading-relaxed"><%# Eval("Content") %></p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div id="noComments" class="<%= rptComments.Items.Count > 0 ? "hidden" : "" %> py-8 text-center text-gray-400 dark:text-slate-500 text-sm">
                        <asp:Literal ID="litNoComments" runat="server"></asp:Literal>
                    </div>
                </div>

                <%-- 댓글 작성 폼 (로그인 시) --%>
                <asp:PlaceHolder ID="phCommentForm" runat="server" Visible="false">
                    <div class="flex gap-3 mt-6">
                        <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
                            <asp:Literal ID="litMyAvatar" runat="server"></asp:Literal>
                        </div>
                        <div class="flex-1 flex gap-2">
                            <input type="text" id="txtCommentInput" maxlength="500"
                                class="flex-1 p-3 border dark:border-slate-600 rounded-xl bg-white dark:bg-slate-700 dark:text-white text-sm outline-none focus:ring-2 focus:ring-blue-500 transition-all"
                                placeholder="<asp:Literal ID="litCommentPh" runat="server" />" onkeydown="if(event.key==='Enter')addComment()" />
                            <button type="button" onclick="addComment()"
                                class="px-4 py-2 bg-blue-600 text-white rounded-xl font-bold text-sm hover:bg-blue-700 cursor-pointer transition-all whitespace-nowrap">
                                <asp:Literal ID="litCommentBtn" runat="server"></asp:Literal>
                            </button>
                        </div>
                    </div>
                </asp:PlaceHolder>
            </div>
        </div>
    </div>

    <script>
        var postNo   = '<%= hdnPostNo.Value %>';
        var isLiked  = '<%= hdnIsLiked.Value %>' === 'true';
        var isLogin  = '<%= hdnIsLogin.Value %>' === 'true';
        var delMsg   = '<%= Lang.Get("comm.confirmDelCmt").Replace("'","\\'"  ) %>';

        // 초기 좋아요 수 로드
        fetch('CommunityView.aspx?action=getLike&no=' + postNo)
            .then(function(r){ return r.json(); })
            .then(function(d){
                document.getElementById('likeCount').textContent = d.count;
                setLikeStyle(d.liked);
            });

        function toggleLike() {
            if (!isLogin) { openLogin(); return; }
            fetch('CommunityView.aspx?action=toggleLike&no=' + postNo)
                .then(function(r){ return r.json(); })
                .then(function(d){
                    document.getElementById('likeCount').textContent = d.count;
                    isLiked = d.liked;
                    setLikeStyle(d.liked);
                });
        }

        function setLikeStyle(liked) {
            var btn = document.getElementById('btnLike');
            if (liked) {
                btn.classList.add('bg-pink-50','border-pink-300','text-pink-600','dark:bg-pink-950','dark:border-pink-700','dark:text-pink-400');
                btn.classList.remove('border-gray-200','dark:border-slate-600','text-gray-500','dark:text-slate-400');
            } else {
                btn.classList.remove('bg-pink-50','border-pink-300','text-pink-600','dark:bg-pink-950','dark:border-pink-700','dark:text-pink-400');
                btn.classList.add('border-gray-200','dark:border-slate-600','text-gray-500','dark:text-slate-400');
            }
        }

        function addComment() {
            if (!isLogin) { openLogin(); return; }
            var input = document.getElementById('txtCommentInput');
            var content = input.value.trim();
            if (!content) return;
            fetch('CommunityView.aspx?action=addComment&no=' + postNo + '&content=' + encodeURIComponent(content))
                .then(function(r){ return r.json(); })
                .then(function(d){
                    if (d.result !== 'ok') return;
                    input.value = '';
                    var noEl = document.getElementById('noComments');
                    if (noEl) noEl.classList.add('hidden');
                    var html = '<div class="flex gap-3 py-4 border-b dark:border-slate-700 comment-item" id="cmt_' + d.commentID + '">'
                        + '<div class="w-8 h-8 rounded-full bg-gray-200 dark:bg-slate-600 flex items-center justify-center text-sm font-bold text-gray-600 dark:text-gray-300 flex-shrink-0">' + d.avatarHtml + '</div>'
                        + '<div class="flex-1 min-w-0">'
                        + '<div class="flex items-center justify-between mb-1">'
                        + '<div class="flex items-center gap-2"><span class="font-semibold text-sm dark:text-white">' + d.nick + '</span>'
                        + '<span class="text-xs text-gray-400 dark:text-slate-500">' + d.createdAt + '</span></div>'
                        + '<button type="button" onclick="deleteComment(' + d.commentID + ',this)" class="text-xs text-red-400 hover:text-red-600 transition-colors px-2 py-1">' + d.delBtnText + '</button>'
                        + '</div><p class="text-sm dark:text-gray-200 break-words leading-relaxed">' + d.content + '</p></div></div>';
                    document.getElementById('commentList').insertAdjacentHTML('afterbegin', html);
                    var cnt = document.getElementById('commentCount');
                    cnt.textContent = '(' + d.totalCount + ')';
                });
        }

        function deleteComment(commentID, btn) {
            if (!confirm(delMsg)) return;
            fetch('CommunityView.aspx?action=deleteComment&commentID=' + commentID + '&no=' + postNo)
                .then(function(r){ return r.json(); })
                .then(function(d){
                    if (d.result !== 'ok') return;
                    var el = document.getElementById('cmt_' + commentID);
                    if (el) el.remove();
                    var cnt = document.getElementById('commentCount');
                    cnt.textContent = '(' + d.totalCount + ')';
                    if (d.totalCount === 0) {
                        var noEl = document.getElementById('noComments');
                        if (noEl) noEl.classList.remove('hidden');
                    }
                });
        }

        // 알림 드롭다운 외부 클릭 닫기
        document.addEventListener('click', function(e) {
            var w = document.getElementById('notifWrap');
            var d = document.getElementById('notifDrop');
            if (w && d && !w.contains(e.target)) d.classList.add('hidden');
        });
    </script>
</asp:Content>
