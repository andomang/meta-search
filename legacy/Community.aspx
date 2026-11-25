<%@ Page Title="Community" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Community.aspx.cs" Inherits="Community" %>
<%--
    [Community.aspx]
    커뮤니티 게시판 목록 페이지입니다.
    코드비하인드: Community.aspx.cs / Inherits="Community"
    - 게시글 목록을 테이블 형태로 보여줍니다.
    - 키워드 검색, 페이지네이션(이전/다음) 기능을 제공합니다.
    - 다국어 처리를 위해 텍스트를 asp:Literal로 서버에서 주입합니다.
--%>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-6xl mx-auto px-6 py-12 min-h-screen">

        <%-- ===== 페이지 헤더 (제목 + 글쓰기 버튼) ===== --%>
        <div class="flex justify-between items-end mb-8 border-b dark:border-slate-700 pb-6">
            <%-- litPageTitle: 페이지 제목 텍스트. 코드비하인드에서 Lang.Get() 등으로 다국어 값을 설정합니다. --%>
            <h1 class="text-3xl font-bold dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h1>
            <%-- "글쓰기" 링크 버튼. CommunityWrite.aspx로 이동합니다. --%>
            <a href="CommunityWrite.aspx" class="px-6 py-3 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 transition-all shadow-lg shadow-blue-100 dark:shadow-none flex items-center gap-2">
                <%-- data-lucide="pen-line": Lucide 아이콘 라이브러리의 펜 아이콘. 글쓰기 버튼 앞에 표시됩니다. --%>
                <i data-lucide="pen-line" class="w-5 h-5"></i>
                <%-- litWriteBtn: "글쓰기" 버튼 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                <asp:Literal ID="litWriteBtn" runat="server"></asp:Literal>
            </a>
        </div>

        <%-- ===== 검색 박스 ===== --%>
        <%-- 키워드를 입력하고 검색 버튼을 클릭하면 btnSearch_Click 이벤트가 발생합니다. --%>
        <div class="flex gap-2 mb-6">
            <div class="relative flex-1 max-w-md">
                <%-- data-lucide="search": 돋보기 아이콘. 입력 필드 왼쪽에 장식용으로 표시됩니다. --%>
                <i data-lucide="search" class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400"></i>
                <%--
                    txtSearch: 검색 키워드 입력 필드.
                    코드비하인드에서 txtSearch.Text 로 입력값을 읽어 DB 검색에 사용합니다.
                    검색 실행 후 같은 페이지를 다시 로드할 때 검색어가 유지됩니다.
                --%>
                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="w-full pl-9 pr-4 py-2.5 border dark:border-slate-600 rounded-xl bg-white dark:bg-slate-800 dark:text-white text-sm outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
            </div>
            <%--
                btnSearch: 검색 실행 버튼.
                OnClick="btnSearch_Click" → 코드비하인드의 btnSearch_Click 이벤트 핸들러를 호출합니다.
                클릭 시 txtSearch.Text의 값을 조건으로 게시글을 필터링하고 첫 페이지로 돌아갑니다.
                버튼 텍스트(예: "검색")는 코드비하인드에서 Text 속성에 직접 설정됩니다.
            --%>
            <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click"
                CssClass="px-5 py-2.5 bg-blue-600 text-white rounded-xl font-bold text-sm hover:bg-blue-700 transition-all cursor-pointer" />
            <%--
                hlReset: 검색 초기화(전체보기) 하이퍼링크.
                NavigateUrl="Community.aspx" → 쿼리스트링 없이 목록 첫 페이지로 이동합니다.
                litResetBtn: "초기화" 또는 "전체보기" 버튼 텍스트를 코드비하인드에서 주입합니다.
            --%>
            <asp:HyperLink ID="hlReset" runat="server" NavigateUrl="Community.aspx"
                CssClass="px-5 py-2.5 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-slate-300 rounded-xl font-bold text-sm hover:bg-gray-200 dark:hover:bg-slate-600 transition-all flex items-center">
                <asp:Literal ID="litResetBtn" runat="server"></asp:Literal>
            </asp:HyperLink>
        </div>

        <%-- ===== 게시글 목록 테이블 ===== --%>
        <%-- 게시글 데이터를 테이블 형태로 출력합니다. 내부의 rptPosts Repeater가 행을 동적으로 생성합니다. --%>
        <div class="overflow-hidden border border-gray-200 dark:border-slate-700 rounded-2xl shadow-sm mb-6">
            <table class="w-full text-left border-collapse">
                <%-- 테이블 헤더 행: 번호 / 제목 / 작성자 / 날짜 컬럼 --%>
                <thead class="bg-gray-50 dark:bg-slate-800 border-b dark:border-slate-700 text-gray-600 dark:text-gray-300">
                    <tr>
                        <%-- litColNo: "번호" 컬럼 헤더 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
                        <th class="px-6 py-4 text-sm font-semibold text-center w-16"><asp:Literal ID="litColNo" runat="server"></asp:Literal></th>
                        <%-- litColTitle: "제목" 컬럼 헤더 텍스트 --%>
                        <th class="px-6 py-4 text-sm font-semibold"><asp:Literal ID="litColTitle" runat="server"></asp:Literal></th>
                        <%-- litColAuthor: "작성자" 컬럼 헤더 텍스트 --%>
                        <th class="px-6 py-4 text-sm font-semibold w-28"><asp:Literal ID="litColAuthor" runat="server"></asp:Literal></th>
                        <%-- litColDate: "날짜" 컬럼 헤더 텍스트 --%>
                        <th class="px-6 py-4 text-sm font-semibold w-28 text-center"><asp:Literal ID="litColDate" runat="server"></asp:Literal></th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-100 dark:divide-slate-800">
                    <%--
                        rptPosts: 게시글 목록 Repeater.
                        코드비하인드에서 rptPosts.DataSource 에 DataTable(또는 List) 을 바인딩합니다.
                        바인딩되는 주요 컬럼: No(번호), Title(제목), AuthorName(작성자), UploadTime(작성일),
                                             LikeCount(좋아요 수), CommentCount(댓글 수), Hits(조회수)
                    --%>
                    <asp:Repeater ID="rptPosts" runat="server">
                        <ItemTemplate>
                            <tr class="hover:bg-blue-50/50 dark:hover:bg-slate-800/50 transition-colors">
                                <%-- Eval("No"): 게시글 번호 컬럼 출력 --%>
                                <td class="px-6 py-4 text-sm text-gray-400 text-center"><%# Eval("No") %></td>
                                <td class="px-6 py-4 font-semibold dark:text-gray-100">
                                    <%--
                                        Eval("No"): URL 쿼리스트링에 게시글 번호를 넣어 상세 페이지로 이동합니다.
                                        Eval("Title"): 게시글 제목을 링크 텍스트로 출력합니다.
                                    --%>
                                    <a href='CommunityView.aspx?no=<%# Eval("No") %>' class="hover:text-blue-600"><%# Eval("Title") %></a>
                                    <%--
                                        Eval("LikeCount"): 좋아요 수가 1 이상이면 분홍색 하트(♥)와 숫자를 제목 옆에 표시합니다.
                                        0이면 빈 문자열을 반환해 숨깁니다.
                                    --%>
                                    <%# Convert.ToInt32(Eval("LikeCount")) > 0 ? "<span class='ml-2 text-xs text-pink-500 font-bold'>♥ " + Eval("LikeCount") + "</span>" : "" %>
                                    <%--
                                        Eval("CommentCount"): 댓글 수가 1 이상이면 파란색 대괄호([N]) 형태로 표시합니다.
                                        0이면 빈 문자열을 반환해 숨깁니다.
                                    --%>
                                    <%# Convert.ToInt32(Eval("CommentCount")) > 0 ? "<span class='ml-1 text-xs text-blue-400 font-bold'>[" + Eval("CommentCount") + "]</span>" : "" %>
                                </td>
                                <td class="px-6 py-4 text-sm dark:text-gray-300">
                                    <%-- Eval("AuthorName"): 작성자 닉네임 출력 --%>
                                    <%# Eval("AuthorName") %>
                                    <span class="ml-2 text-xs text-gray-400 dark:text-slate-500 whitespace-nowrap">
                                        <%-- data-lucide="eye": 눈 모양 아이콘. 조회수 앞에 표시됩니다. --%>
                                        <i data-lucide="eye" class="inline w-3 h-3 mr-0.5 align-middle"></i><%# Eval("Hits") %>
                                    </span>
                                </td>
                                <%--
                                    Eval("UploadTime"): 작성일을 DateTime으로 변환한 뒤 "yyyy.MM.dd" 형식으로 출력합니다.
                                    예) 2025.06.01
                                --%>
                                <td class="px-6 py-4 text-sm text-gray-500 text-center"><%# Convert.ToDateTime(Eval("UploadTime")).ToString("yyyy.MM.dd") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%-- ===== 페이지네이션 버튼 ===== --%>
        <%--
            이전 페이지(btnPrev) / 현재 페이지 정보(litPageInfo) / 다음 페이지(btnNext) 로 구성됩니다.
            두 버튼 모두 OnClick="btnPage_Click" 으로 같은 이벤트 핸들러를 공유합니다.
            코드비하인드에서 ((Button)sender).ID 를 확인해 이전/다음을 구분합니다.
        --%>
        <div class="flex items-center justify-center gap-4">
            <%--
                btnPrev: 이전 페이지 이동 버튼.
                코드비하인드에서 현재 페이지가 1이면 Enabled=false 로 비활성화합니다.
                버튼 텍스트(예: "이전")는 코드비하인드에서 Text 속성에 설정합니다.
            --%>
            <asp:Button ID="btnPrev" runat="server" OnClick="btnPage_Click"
                CssClass="px-5 py-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-700 transition-all disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer" />
            <%--
                litPageInfo: "현재 페이지 / 전체 페이지" 형태의 페이지 정보 텍스트.
                예) "1 / 5" — 코드비하인드에서 문자열을 조합해 주입합니다.
            --%>
            <span class="text-sm text-gray-500 dark:text-slate-400 font-medium">
                <asp:Literal ID="litPageInfo" runat="server"></asp:Literal>
            </span>
            <%--
                btnNext: 다음 페이지 이동 버튼.
                코드비하인드에서 현재 페이지가 마지막 페이지이면 Enabled=false 로 비활성화합니다.
                버튼 텍스트(예: "다음")는 코드비하인드에서 Text 속성에 설정합니다.
            --%>
            <asp:Button ID="btnNext" runat="server" OnClick="btnPage_Click"
                CssClass="px-5 py-2 bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-600 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-50 dark:hover:bg-slate-700 transition-all disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer" />
        </div>
    </div>
</asp:Content>
