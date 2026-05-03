<%@ Page Title="View" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CommunityView.aspx.cs" Inherits="CommunityView" %>
<%--
    [CommunityView.aspx]
    커뮤니티 게시글 상세 보기 페이지입니다.
    코드비하인드: CommunityView.aspx.cs / Inherits="CommunityView"
    - URL 쿼리스트링 ?no=N 으로 게시글 번호를 받아 상세 내용을 출력합니다.
    - 좋아요 토글, 댓글 추가/삭제 기능을 JavaScript fetch(AJAX)로 처리합니다.
    - 게시글 작성자에게만 수정/삭제 버튼(phOwnerActions)이 표시됩니다.
    - 로그인한 사용자에게만 댓글 작성 폼(phCommentForm)이 표시됩니다.
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12 min-h-screen">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl overflow-hidden shadow-sm">

            <%-- ===== 게시글 헤더 (게시판 레이블, 제목, 작성자, 날짜, 조회수) ===== --%>
            <div class="p-8 border-b border-gray-100 dark:border-slate-700 bg-gray-50/50 dark:bg-slate-800/50">
                <div class="flex items-center gap-2 mb-4">
                    <span class="px-3 py-1 bg-blue-100 dark:bg-blue-950 text-blue-600 dark:text-blue-300 text-xs font-bold rounded-full">
                        <%-- litBoardLabel: "커뮤니티" 같은 게시판 분류 레이블 텍스트. 코드비하인드에서 주입합니다. --%>
                        <asp:Literal ID="litBoardLabel" runat="server"></asp:Literal>
                    </span>
                </div>
                <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">
                    <%-- litTitle: 게시글 제목. 코드비하인드에서 DB의 Title 컬럼 값을 주입합니다. --%>
                    <asp:Literal ID="litTitle" runat="server"></asp:Literal>
                </h1>
                <div class="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
                    <div class="flex items-center gap-4">
                        <span class="font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2.5">
                            <%-- litAvatar: 작성자 아바타 이니셜 또는 프로필 이미지 HTML. 코드비하인드에서 GetAvatarHtml() 등으로 생성합니다. --%>
                            <div class="w-8 h-8 rounded-full overflow-hidden bg-blue-500 dark:bg-blue-600 flex items-center justify-center text-white font-bold text-xs flex-shrink-0">
                                <asp:Literal ID="litAvatar" runat="server"></asp:Literal>
                            </div>
                            <%-- litAuthor: 작성자 닉네임. 코드비하인드에서 DB의 AuthorName(또는 Nick) 컬럼 값을 주입합니다. --%>
                            <asp:Literal ID="litAuthor" runat="server"></asp:Literal>
                        </span>
                        <span class="flex items-center gap-1">
                            <%-- data-lucide="calendar": 달력 아이콘. 작성 날짜 앞에 표시됩니다. --%>
                            <i data-lucide="calendar" class="w-4 h-4"></i>
                            <%-- litDate: 게시글 작성 날짜 문자열. 코드비하인드에서 포맷팅하여 주입합니다. --%>
                            <asp:Literal ID="litDate" runat="server"></asp:Literal>
                        </span>
                        <span class="flex items-center gap-1">
                            <%-- data-lucide="eye": 눈 모양 아이콘. 조회수 앞에 표시됩니다. --%>
                            <i data-lucide="eye" class="w-4 h-4"></i>
                            <%-- litHits: 조회수. 코드비하인드에서 DB의 Hits 컬럼 값을 주입합니다. --%>
                            <asp:Literal ID="litHits" runat="server"></asp:Literal>
                        </span>
                    </div>
                </div>
            </div>

            <%-- ===== 게시글 본문 ===== --%>
            <div class="p-8 min-h-[300px]">
                <div class="text-gray-800 dark:text-gray-100 leading-relaxed text-lg whitespace-pre-wrap break-words font-medium">
                    <%--
                        litContents: 게시글 본문 내용.
                        코드비하인드에서 DB의 Contents 컬럼 값을 주입합니다.
                        whitespace-pre-wrap 클래스로 줄바꿈을 그대로 유지합니다.
                    --%>
                    <asp:Literal ID="litContents" runat="server"></asp:Literal>
                </div>
            </div>

            <%-- ===== 첨부파일 영역 ===== --%>
            <%--
                phFile: 첨부파일이 있을 때만 표시되는 컨테이너(PlaceHolder).
                기본값은 Visible="false" 이며, 코드비하인드에서 첨부파일이 존재할 경우
                phFile.Visible = true 로 설정하여 이 영역 전체를 표시합니다.
            --%>
            <asp:PlaceHolder ID="phFile" runat="server" Visible="false">
                <div class="mx-8 mb-8 p-5 bg-blue-50 dark:bg-blue-950 rounded-2xl border border-blue-100 dark:border-blue-900 flex items-center justify-between">
                    <div class="flex items-center gap-3">
                        <%-- data-lucide="paperclip": 클립 아이콘. 첨부파일 섹션 앞에 표시됩니다. --%>
                        <i data-lucide="paperclip" class="text-blue-500 dark:text-blue-400"></i>
                        <div>
                            <%-- litFileName: 첨부파일 원본 이름(예: report.pdf). 코드비하인드에서 DB의 FileName 컬럼 값을 주입합니다. --%>
                            <p class="text-sm font-bold text-gray-800 dark:text-gray-100"><asp:Literal ID="litFileName" runat="server"></asp:Literal></p>
                            <%-- litFileSize: 파일 크기(KB 단위). 코드비하인드에서 계산 후 주입합니다. --%>
                            <p class="text-xs text-blue-500 dark:text-blue-400"><asp:Literal ID="litFileSize" runat="server"></asp:Literal> KB</p>
                        </div>
                    </div>
                    <%--
                        hlDownload: 파일 다운로드 하이퍼링크.
                        코드비하인드에서 hlDownload.NavigateUrl 에 다운로드 핸들러 경로를 설정합니다.
                        (예: Download.ashx?no=123)
                        litDownloadBtn: "다운로드" 버튼 텍스트. 코드비하인드에서 다국어 값을 주입합니다.
                    --%>
                    <asp:HyperLink ID="hlDownload" runat="server"
                        CssClass="px-4 py-2 bg-white dark:bg-slate-700 text-blue-600 dark:text-blue-300 rounded-xl text-sm font-bold shadow-sm hover:bg-blue-600 dark:hover:bg-blue-600 hover:text-white dark:hover:text-white transition-all">
                        <asp:Literal ID="litDownloadBtn" runat="server"></asp:Literal>
                    </asp:HyperLink>
                </div>
            </asp:PlaceHolder>

            <%-- ===== 좋아요 버튼 및 숨겨진 상태값 ===== --%>
            <div class="px-8 pb-6 flex items-center gap-4">
                <%--
                    btnLike: 좋아요 토글 버튼 (일반 HTML button, asp 서버 컨트롤 아님).
                    onclick="toggleLike()" → 아래 JavaScript toggleLike() 함수를 호출합니다.
                    data-lucide="heart": 하트 아이콘. 좋아요 버튼 내부에 표시됩니다.
                    likeCount: 현재 좋아요 수를 표시하는 span. JavaScript에서 동적으로 업데이트됩니다.
                --%>
                <button type="button" id="btnLike" onclick="toggleLike()"
                    class="flex items-center gap-2 px-6 py-3 rounded-xl border border-gray-200 dark:border-slate-600 text-gray-500 dark:text-slate-400 hover:border-pink-300 hover:text-pink-500 dark:hover:border-pink-700 dark:hover:text-pink-400 transition-all font-bold text-sm">
                    <i data-lucide="heart" class="w-5 h-5"></i>
                    <span id="likeCount">0</span>
                </button>
                <%--
                    hdnPostNo: 현재 게시글 번호를 서버→클라이언트로 숨겨서 전달하는 HiddenField.
                    코드비하인드에서 hdnPostNo.Value = postNo.ToString() 으로 값을 설정합니다.
                    JavaScript에서 postNo 변수로 읽어 fetch URL에 사용합니다.
                --%>
                <asp:HiddenField ID="hdnPostNo"   runat="server" />
                <%--
                    hdnIsLiked: 현재 로그인한 사용자가 이 게시글에 이미 좋아요를 눌렀는지 여부를 전달합니다.
                    코드비하인드에서 "true" 또는 "false" 문자열로 값을 설정합니다.
                    JavaScript에서 isLiked 변수로 읽어 좋아요 버튼 스타일 초기화에 사용합니다.
                --%>
                <asp:HiddenField ID="hdnIsLiked"  runat="server" Value="false" />
                <%--
                    hdnIsLogin: 현재 사용자의 로그인 여부를 전달합니다.
                    코드비하인드에서 로그인 상태이면 "true", 비로그인이면 "false" 로 설정합니다.
                    JavaScript에서 isLogin 변수로 읽어 미로그인 시 로그인 팝업(openLogin())을 호출합니다.
                --%>
                <asp:HiddenField ID="hdnIsLogin"  runat="server" Value="false" />
            </div>

            <%-- ===== 하단 버튼 (목록으로 / 수정·삭제) ===== --%>
            <div class="p-8 border-t border-gray-100 dark:border-slate-700 flex justify-between bg-gray-50/30 dark:bg-slate-800/30">
                <%-- "목록으로" 버튼: Community.aspx 게시글 목록 페이지로 돌아갑니다. --%>
                <a href="Community.aspx" class="px-6 py-3 bg-white dark:bg-slate-700 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-600 transition-all">
                    <%-- litBackBtn: "목록으로" 버튼 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <asp:Literal ID="litBackBtn" runat="server"></asp:Literal>
                </a>
                <%--
                    phOwnerActions: 수정/삭제 버튼 컨테이너.
                    기본값은 Visible="false" 이며, 코드비하인드에서 현재 로그인 사용자가
                    게시글 작성자와 동일한 경우에만 phOwnerActions.Visible = true 로 설정합니다.
                    즉, 본인 게시글에만 수정·삭제 버튼이 보입니다.
                --%>
                <asp:PlaceHolder ID="phOwnerActions" runat="server" Visible="false">
                    <div class="flex gap-2">
                        <%--
                            hlEdit: 게시글 수정 페이지(CommunityEdit.aspx)로 이동하는 링크.
                            코드비하인드에서 hlEdit.NavigateUrl = "CommunityEdit.aspx?no=" + postNo 로 설정합니다.
                            hlEdit.Text 에 "수정" 텍스트를 다국어로 주입합니다.
                        --%>
                        <asp:HyperLink ID="hlEdit" runat="server"
                            CssClass="px-6 py-3 bg-blue-50 dark:bg-blue-950 text-blue-600 dark:text-blue-400 rounded-xl font-bold hover:bg-blue-100 dark:hover:bg-blue-900 transition-all">
                        </asp:HyperLink>
                        <%--
                            btnDelete: 게시글 삭제 버튼.
                            OnClick="btnDelete_Click" → 코드비하인드의 btnDelete_Click 이벤트 핸들러를 호출합니다.
                            서버에서 DB의 게시글 레코드를 삭제하고 Community.aspx로 리디렉션합니다.
                            버튼 텍스트(예: "삭제")는 코드비하인드에서 Text 속성에 설정합니다.
                        --%>
                        <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click"
                            CssClass="px-6 py-3 bg-red-50 dark:bg-red-950 text-red-600 dark:text-red-400 rounded-xl font-bold hover:bg-red-100 dark:hover:bg-red-900 cursor-pointer transition-all" />
                    </div>
                </asp:PlaceHolder>
            </div>

            <%-- ===== 댓글 섹션 ===== --%>
            <div class="p-8 border-t border-gray-100 dark:border-slate-700">
                <h3 class="font-bold text-lg dark:text-white mb-6 flex items-center gap-2">
                    <%-- data-lucide="message-circle": 말풍선 아이콘. 댓글 섹션 제목 앞에 표시됩니다. --%>
                    <i data-lucide="message-circle" class="w-5 h-5 text-blue-500"></i>
                    <%-- litCommentsLabel: "댓글" 섹션 제목 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <asp:Literal ID="litCommentsLabel" runat="server"></asp:Literal>
                    <%--
                        commentCount: 댓글 수를 표시하는 span. JavaScript에서 동적으로 업데이트됩니다.
                        litCommentCount: 초기 댓글 수를 서버에서 렌더링합니다. 코드비하인드에서 DB 조회 결과로 설정합니다.
                    --%>
                    <span id="commentCount" class="text-blue-500 text-base">(<asp:Literal ID="litCommentCount" runat="server">0</asp:Literal>)</span>
                </h3>

                <%-- ===== 댓글 목록 ===== --%>
                <div id="commentList">
                    <%--
                        rptComments: 댓글 목록 Repeater.
                        코드비하인드에서 rptComments.DataSource 에 댓글 DataTable(또는 List)을 바인딩합니다.
                        바인딩되는 주요 컬럼:
                          - CommentID: 댓글 고유 ID (삭제 시 사용)
                          - AuthorNick: 댓글 작성자 닉네임
                          - Author: 댓글 작성자 아이디 (삭제 권한 확인용)
                          - ProfileImg: 프로필 이미지 경로 (GetAvatarHtml에 사용)
                          - Content: 댓글 내용
                          - CreatedAt: 댓글 작성 시각
                    --%>
                    <asp:Repeater ID="rptComments" runat="server">
                        <ItemTemplate>
                            <%--
                                id="cmt_<%# Eval("CommentID") %>": 댓글 고유 ID를 HTML id로 설정합니다.
                                JavaScript의 deleteComment() 함수에서 이 id로 DOM 요소를 찾아 삭제합니다.
                            --%>
                            <div class="flex gap-3 py-4 border-b dark:border-slate-700 last:border-0 comment-item" id="cmt_<%# Eval("CommentID") %>">
                                <div class="w-8 h-8 rounded-full bg-gray-200 dark:bg-slate-600 flex items-center justify-center text-sm font-bold text-gray-600 dark:text-gray-300 flex-shrink-0">
                                    <%--
                                        GetAvatarHtml(): 코드비하인드에 정의된 헬퍼 메서드.
                                        AuthorNick(닉네임)과 ProfileImg(프로필 이미지 경로)를 받아
                                        프로필 이미지 태그 또는 이니셜 텍스트 HTML을 반환합니다.
                                    --%>
                                    <%# GetAvatarHtml(Eval("AuthorNick").ToString(), Eval("ProfileImg").ToString()) %>
                                </div>
                                <div class="flex-1 min-w-0">
                                    <div class="flex items-center justify-between mb-1">
                                        <div class="flex items-center gap-2">
                                            <%-- Eval("AuthorNick"): 댓글 작성자 닉네임을 출력합니다. --%>
                                            <span class="font-semibold text-sm dark:text-white"><%# Eval("AuthorNick") %></span>
                                            <%-- Eval("CreatedAt"): 댓글 작성 시각을 "yyyy.MM.dd HH:mm" 형식으로 출력합니다. --%>
                                            <span class="text-xs text-gray-400 dark:text-slate-500"><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("yyyy.MM.dd HH:mm") %></span>
                                        </div>
                                        <%--
                                            GetDeleteBtn(): 코드비하인드에 정의된 헬퍼 메서드.
                                            Eval("Author")(작성자 아이디)와 Eval("CommentID")를 받아,
                                            현재 로그인 사용자가 해당 댓글의 작성자일 경우에만
                                            "삭제" 버튼 HTML(onclick="deleteComment(id, this)")을 반환합니다.
                                            본인 댓글이 아니면 빈 문자열을 반환해 버튼을 숨깁니다.
                                        --%>
                                        <%# GetDeleteBtn(Eval("Author").ToString(), Eval("CommentID").ToString()) %>
                                    </div>
                                    <%-- Eval("Content"): 댓글 본문 내용을 출력합니다. --%>
                                    <p class="text-sm dark:text-gray-200 break-words leading-relaxed"><%# Eval("Content") %></p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <%--
                        noComments: 댓글이 0개일 때 표시되는 "첫 댓글을 남겨보세요" 안내 영역.
                        서버에서 rptComments.Items.Count > 0 이면 hidden 클래스를 추가해 숨깁니다.
                        JavaScript의 addComment/deleteComment 함수에서도 동적으로 show/hide 합니다.
                    --%>
                    <div id="noComments" class="<%= rptComments.Items.Count > 0 ? "hidden" : "" %> py-8 text-center text-gray-400 dark:text-slate-500 text-sm">
                        <%-- litNoComments: "댓글이 없습니다" 안내 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                        <asp:Literal ID="litNoComments" runat="server"></asp:Literal>
                    </div>
                </div>

                <%-- ===== 댓글 작성 폼 (로그인 사용자만 표시) ===== --%>
                <%--
                    phCommentForm: 댓글 입력 폼 컨테이너.
                    기본값은 Visible="false" 이며, 코드비하인드에서 로그인 상태인 경우에만
                    phCommentForm.Visible = true 로 설정해 폼을 표시합니다.
                    비로그인 사용자에게는 이 영역이 완전히 렌더링되지 않습니다.
                --%>
                <asp:PlaceHolder ID="phCommentForm" runat="server" Visible="false">
                    <div class="flex gap-3 mt-6">
                        <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
                            <%-- litMyAvatar: 현재 로그인한 사용자의 아바타 이니셜. 코드비하인드에서 주입합니다. --%>
                            <asp:Literal ID="litMyAvatar" runat="server"></asp:Literal>
                        </div>
                        <div class="flex-1 flex gap-2">
                            <%--
                                txtCommentInput: 댓글 내용을 입력하는 일반 HTML input 필드 (서버 컨트롤 아님).
                                maxlength="500": 최대 500자 제한.
                                onkeydown: Enter 키 입력 시 addComment() 함수를 호출합니다.
                                placeholder 내부의 asp:Literal(litCommentPh)은 서버에서 다국어 placeholder 텍스트를 렌더링합니다.
                            --%>
                            <input type="text" id="txtCommentInput" maxlength="500"
                                class="flex-1 p-3 border dark:border-slate-600 rounded-xl bg-white dark:bg-slate-700 dark:text-white text-sm outline-none focus:ring-2 focus:ring-blue-500 transition-all"
                                placeholder="<asp:Literal ID="litCommentPh" runat="server" />" onkeydown="if(event.key==='Enter')addComment()" />
                            <%--
                                댓글 등록 버튼 (일반 HTML button).
                                onclick="addComment()" → 아래 JavaScript addComment() 함수를 호출합니다.
                                litCommentBtn: "등록" 버튼 텍스트. 코드비하인드에서 다국어 값을 주입합니다.
                            --%>
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
        /* =====================================================
           서버에서 클라이언트로 전달된 상태 변수
           HiddenField의 서버 렌더링 값을 JavaScript 변수로 초기화합니다.
        ===================================================== */

        // postNo: 현재 게시글 번호. hdnPostNo HiddenField에서 읽습니다. fetch URL에 사용됩니다.
        var postNo   = '<%= hdnPostNo.Value %>';

        // isLiked: 현재 사용자가 이미 좋아요를 눌렀는지 여부. hdnIsLiked HiddenField에서 읽습니다.
        var isLiked  = '<%= hdnIsLiked.Value %>' === 'true';

        // isLogin: 현재 사용자가 로그인 상태인지 여부. hdnIsLogin HiddenField에서 읽습니다.
        var isLogin  = '<%= hdnIsLogin.Value %>' === 'true';

        // delMsg: 댓글 삭제 확인 메시지 (다국어). Lang.Get()으로 서버에서 문자열을 가져옵니다.
        var delMsg   = '<%= Lang.Get("comm.confirmDelCmt").Replace("'","\\'"  ) %>';

        /* =====================================================
           초기 좋아요 수 로드
           페이지가 열릴 때 서버에서 최신 좋아요 수와 내 좋아요 여부를 가져옵니다.
           요청: GET CommunityView.aspx?action=getLike&no={postNo}
           응답 JSON: { count: 숫자, liked: true/false }
        ===================================================== */
        fetch('CommunityView.aspx?action=getLike&no=' + postNo)
            .then(function(r){ return r.json(); })
            .then(function(d){
                // likeCount span에 좋아요 수를 표시합니다.
                document.getElementById('likeCount').textContent = d.count;
                // 현재 사용자의 좋아요 여부에 따라 버튼 스타일을 설정합니다.
                setLikeStyle(d.liked);
            });

        /* =====================================================
           toggleLike(): 좋아요 토글 함수
           - 비로그인 상태이면 로그인 팝업(openLogin())을 엽니다.
           - 로그인 상태이면 서버에 좋아요 토글 요청을 보내고 UI를 업데이트합니다.
           요청: GET CommunityView.aspx?action=toggleLike&no={postNo}
           응답 JSON: { count: 숫자, liked: true/false }
        ===================================================== */
        function toggleLike() {
            if (!isLogin) { openLogin(); return; }
            fetch('CommunityView.aspx?action=toggleLike&no=' + postNo)
                .then(function(r){ return r.json(); })
                .then(function(d){
                    // 좋아요 수 업데이트
                    document.getElementById('likeCount').textContent = d.count;
                    // 전역 isLiked 상태 업데이트
                    isLiked = d.liked;
                    // 버튼 스타일 업데이트
                    setLikeStyle(d.liked);
                });
        }

        /* =====================================================
           setLikeStyle(liked): 좋아요 버튼 스타일 전환 함수
           - liked가 true이면: 버튼을 분홍색(활성화) 스타일로 변경합니다.
           - liked가 false이면: 버튼을 기본(회색) 스타일로 복원합니다.
        ===================================================== */
        function setLikeStyle(liked) {
            var btn = document.getElementById('btnLike');
            if (liked) {
                // 좋아요 활성화 상태: 분홍색 배경·테두리·텍스트
                btn.classList.add('bg-pink-50','border-pink-300','text-pink-600','dark:bg-pink-950','dark:border-pink-700','dark:text-pink-400');
                btn.classList.remove('border-gray-200','dark:border-slate-600','text-gray-500','dark:text-slate-400');
            } else {
                // 좋아요 비활성화 상태: 기본 회색 테두리·텍스트
                btn.classList.remove('bg-pink-50','border-pink-300','text-pink-600','dark:bg-pink-950','dark:border-pink-700','dark:text-pink-400');
                btn.classList.add('border-gray-200','dark:border-slate-600','text-gray-500','dark:text-slate-400');
            }
        }

        /* =====================================================
           addComment(): 댓글 추가 함수
           - 비로그인 상태이면 로그인 팝업(openLogin())을 엽니다.
           - txtCommentInput 입력값이 비어있으면 아무것도 하지 않습니다.
           - 서버에 댓글 추가 요청을 보내고, 성공 시 댓글 목록 맨 위에 새 댓글 HTML을 삽입합니다.
           요청: GET CommunityView.aspx?action=addComment&no={postNo}&content={encodeURIComponent(내용)}
           응답 JSON: {
             result: 'ok',
             commentID: 새 댓글 ID,
             nick: 작성자 닉네임,
             avatarHtml: 아바타 HTML,
             createdAt: 작성 시각 문자열,
             content: 댓글 내용,
             delBtnText: 삭제 버튼 텍스트,
             totalCount: 전체 댓글 수
           }
        ===================================================== */
        function addComment() {
            if (!isLogin) { openLogin(); return; }
            var input = document.getElementById('txtCommentInput');
            var content = input.value.trim();
            // 빈 내용이면 서버 요청 없이 종료
            if (!content) return;
            fetch('CommunityView.aspx?action=addComment&no=' + postNo + '&content=' + encodeURIComponent(content))
                .then(function(r){ return r.json(); })
                .then(function(d){
                    if (d.result !== 'ok') return;
                    // 입력 필드 초기화
                    input.value = '';
                    // "댓글이 없습니다" 안내 문구 숨기기
                    var noEl = document.getElementById('noComments');
                    if (noEl) noEl.classList.add('hidden');
                    // 새 댓글 HTML을 동적으로 생성하여 목록 맨 위에 삽입합니다.
                    var html = '<div class="flex gap-3 py-4 border-b dark:border-slate-700 comment-item" id="cmt_' + d.commentID + '">'
                        + '<div class="w-8 h-8 rounded-full bg-gray-200 dark:bg-slate-600 flex items-center justify-center text-sm font-bold text-gray-600 dark:text-gray-300 flex-shrink-0">' + d.avatarHtml + '</div>'
                        + '<div class="flex-1 min-w-0">'
                        + '<div class="flex items-center justify-between mb-1">'
                        + '<div class="flex items-center gap-2"><span class="font-semibold text-sm dark:text-white">' + d.nick + '</span>'
                        + '<span class="text-xs text-gray-400 dark:text-slate-500">' + d.createdAt + '</span></div>'
                        + '<button type="button" onclick="deleteComment(' + d.commentID + ',this)" class="text-xs text-red-400 hover:text-red-600 transition-colors px-2 py-1">' + d.delBtnText + '</button>'
                        + '</div><p class="text-sm dark:text-gray-200 break-words leading-relaxed">' + d.content + '</p></div></div>';
                    document.getElementById('commentList').insertAdjacentHTML('afterbegin', html);
                    // 댓글 수 카운터 업데이트
                    var cnt = document.getElementById('commentCount');
                    cnt.textContent = '(' + d.totalCount + ')';
                });
        }

        /* =====================================================
           deleteComment(commentID, btn): 댓글 삭제 함수
           - 삭제 확인 대화상자(confirm)를 표시하고, 취소 시 아무것도 하지 않습니다.
           - 서버에 댓글 삭제 요청을 보내고, 성공 시 해당 댓글 DOM을 제거합니다.
           - 댓글이 0개가 되면 "댓글이 없습니다" 안내 문구를 다시 표시합니다.
           요청: GET CommunityView.aspx?action=deleteComment&commentID={id}&no={postNo}
           응답 JSON: { result: 'ok', totalCount: 남은 댓글 수 }
           매개변수:
             commentID: 삭제할 댓글의 ID (숫자)
             btn: 삭제 버튼 DOM 요소 (현재 코드에서는 직접 사용하지 않음)
        ===================================================== */
        function deleteComment(commentID, btn) {
            // 삭제 전 사용자 확인
            if (!confirm(delMsg)) return;
            fetch('CommunityView.aspx?action=deleteComment&commentID=' + commentID + '&no=' + postNo)
                .then(function(r){ return r.json(); })
                .then(function(d){
                    if (d.result !== 'ok') return;
                    // 해당 댓글 DOM 요소를 찾아 제거합니다.
                    var el = document.getElementById('cmt_' + commentID);
                    if (el) el.remove();
                    // 댓글 수 카운터 업데이트
                    var cnt = document.getElementById('commentCount');
                    cnt.textContent = '(' + d.totalCount + ')';
                    // 댓글이 0개이면 "댓글이 없습니다" 안내 문구를 표시합니다.
                    if (d.totalCount === 0) {
                        var noEl = document.getElementById('noComments');
                        if (noEl) noEl.classList.remove('hidden');
                    }
                });
        }

        /* =====================================================
           알림 드롭다운 외부 클릭 닫기
           헤더의 알림 드롭다운이 열려있을 때 다른 영역을 클릭하면 닫힙니다.
           notifWrap: 알림 아이콘 + 드롭다운을 감싸는 컨테이너
           notifDrop: 알림 드롭다운 패널
        ===================================================== */
        document.addEventListener('click', function(e) {
            var w = document.getElementById('notifWrap');
            var d = document.getElementById('notifDrop');
            if (w && d && !w.contains(e.target)) d.classList.add('hidden');
        });
    </script>
</asp:Content>
