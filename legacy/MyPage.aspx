<%-- ===== 페이지 지시문 =====
     Title       : 브라우저 탭에 표시될 페이지 제목 ("My Page")
     Language    : 코드비하인드 언어 - C# 사용
     MasterPageFile: ~/Site.Master 를 레이아웃 틀로 사용한다.
                    Site.Master 가 공통 헤더/푸터/CSS 를 포함하며,
                    이 페이지의 콘텐츠는 Master 의 ContentPlaceHolder 에 삽입된다.
     AutoEventWireup: true → Page_Load 등 이벤트 메서드를 자동으로 연결한다.
     CodeFile    : 짝을 이루는 C# 코드비하인드 파일 경로 (MyPage.aspx.cs)
     Inherits    : 코드비하인드의 클래스 이름 (MyPage) --%>
<%@ Page Title="My Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<%-- ===== 메인 콘텐츠 영역 =====
     Site.Master 의 "MainContent" ContentPlaceHolder 자리에 이 블록이 삽입된다.
     runat="server" → 서버에서 처리하는 ASP.NET 컨트롤임을 의미한다. --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ===== 페이지 전체 래퍼 =====
         max-w-5xl : 최대 너비 64rem(1024px) 제한
         mx-auto   : 가운데 정렬
         px-6 py-12: 좌우 패딩 24px, 위아래 패딩 48px
         min-h-screen : 최소 화면 전체 높이 확보 --%>
    <div class="max-w-5xl mx-auto px-6 py-12 min-h-screen">

        <%-- ===== 페이지 헤더 ===== --%>
        <%-- 페이지 헤더 --%>
        <div class="mb-10">
            <%-- 페이지 제목: 코드비하인드에서 litPageTitle.Text 로 "마이 페이지" 또는 "My Page" 를 설정한다.
                 text-3xl font-bold : 크고 굵은 폰트 --%>
            <h1 class="text-3xl font-bold dark:text-white mb-1"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h1>
            <p class="text-gray-500 dark:text-slate-400 text-sm">
                <%-- 환영 메시지 앞부분: 코드비하인드에서 litWelcome.Text 로 "안녕하세요" 등의 문구를 설정한다. --%>
                <asp:Literal ID="litWelcome" runat="server"></asp:Literal>,
                <%-- 현재 로그인한 사용자 이름 출력
                     Session["UserName"] : 로그인 시 세션에 저장된 사용자 이름을 직접 출력한다.
                     <%= ... %> : 서버 인라인 출력 표현식 (서버에서 값을 HTML 에 직접 삽입) --%>
                <span class="font-semibold text-blue-500"><%= Session["UserName"] %></span>
            </p>
        </div>

        <%-- ===== 통계 카드 3개 =====
             grid grid-cols-1 sm:grid-cols-3 : 모바일은 1열, 640px 이상에서 3열 배치
             gap-4 mb-10 : 카드 간 간격 16px, 아래 여백 40px --%>
        <%-- 통계 카드 3개 --%>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-10">

            <%-- 총 검색 횟수 카드
                 bg-white dark:bg-slate-800 : 라이트/다크 배경
                 border ... rounded-2xl : 테두리와 둥근 모서리
                 shadow-sm : 가벼운 그림자
                 text-center : 텍스트 가운데 정렬 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 text-center shadow-sm">
                <%-- 카드 레이블: 코드비하인드에서 litTotalSearchLbl.Text 로 "총 검색 횟수" 레이블을 설정한다. --%>
                <p class="text-xs font-semibold text-gray-400 dark:text-slate-500 mb-1"><asp:Literal ID="litTotalSearchLbl" runat="server"></asp:Literal></p>
                <%-- 검색 횟수 숫자: 코드비하인드에서 litTotalSearch.Text 로 숫자값을 설정한다.
                     text-4xl font-extrabold text-blue-500 : 파란색 초대형 굵은 숫자 --%>
                <p class="text-4xl font-extrabold text-blue-500"><asp:Literal ID="litTotalSearch" runat="server"></asp:Literal></p>
            </div>

            <%-- 총 클릭 횟수 카드 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 text-center shadow-sm">
                <%-- 카드 레이블: 코드비하인드에서 litTotalClickLbl.Text 로 "총 클릭 횟수" 레이블을 설정한다. --%>
                <p class="text-xs font-semibold text-gray-400 dark:text-slate-500 mb-1"><asp:Literal ID="litTotalClickLbl" runat="server"></asp:Literal></p>
                <%-- 클릭 횟수 숫자: 코드비하인드에서 litTotalClick.Text 로 숫자값을 설정한다.
                     text-purple-500 : 보라색으로 구분 --%>
                <p class="text-4xl font-extrabold text-purple-500"><asp:Literal ID="litTotalClick" runat="server"></asp:Literal></p>
            </div>

            <%-- 최다 검색 키워드 카드 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 text-center shadow-sm">
                <%-- 카드 레이블: 코드비하인드에서 litTopKeywordLbl.Text 로 "최다 검색 키워드" 레이블을 설정한다. --%>
                <p class="text-xs font-semibold text-gray-400 dark:text-slate-500 mb-1"><asp:Literal ID="litTopKeywordLbl" runat="server"></asp:Literal></p>
                <%-- 최다 키워드 텍스트: 코드비하인드에서 litTopKeyword.Text 로 키워드를 설정한다.
                     text-2xl : 검색 횟수보다 작은 폰트 (단어 길이가 다양하므로)
                     truncate : 넘치는 텍스트를 ... 으로 줄임
                     text-pink-500 : 분홍색으로 구분 --%>
                <p class="text-2xl font-extrabold text-pink-500 truncate"><asp:Literal ID="litTopKeyword" runat="server"></asp:Literal></p>
            </div>
        </div>

        <%-- ===== 하단 2열 그리드 (최근 검색어 + 내 게시글) =====
             grid grid-cols-1 lg:grid-cols-2 : 1024px 이상에서 2열 배치
             gap-8 : 카드 간 간격 32px --%>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">

            <%-- ===== 최근 검색어 카드 ===== --%>
            <%-- 최근 검색어 카드 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 shadow-sm">
                <div class="flex items-center justify-between mb-5">
                    <h2 class="font-semibold dark:text-white flex items-center gap-2">
                        <%-- data-lucide="clock" : Lucide 아이콘 - 시계(최근/시간) 아이콘
                             text-blue-500 : 파란색 아이콘 --%>
                        <i data-lucide="clock" class="w-4 h-4 text-blue-500"></i>
                        <%-- 최근 검색어 카드 제목: 코드비하인드에서 litRecentSearchLbl.Text 로 설정 --%>
                        <asp:Literal ID="litRecentSearchLbl" runat="server"></asp:Literal>
                    </h2>
                    <%-- "전체 보기" 링크 → SearchResults.aspx 로 이동
                         litViewAllSearch : 코드비하인드에서 "전체 보기" 또는 "View All" 텍스트를 설정한다. --%>
                    <a href="SearchResults.aspx" class="text-xs text-blue-500 hover:underline font-semibold">
                        <asp:Literal ID="litViewAllSearch" runat="server"></asp:Literal>
                    </a>
                </div>

                <%-- 검색어가 없을 때 표시하는 메시지
                     ID="litNoSearch" : 코드비하인드에서 데이터가 없으면 litNoSearch.Visible = true 로 표시한다.
                     Visible="false" : 기본적으로 숨겨져 있다. --%>
                <asp:Literal ID="litNoSearch" runat="server" Visible="false"></asp:Literal>

                <%-- ===== 최근 검색어 반복 목록 =====
                     ID="rptRecentSearch" : 코드비하인드에서 rptRecentSearch.DataSource 에 검색 기록 목록을 바인딩하고
                                            rptRecentSearch.DataBind() 를 호출하면 ItemTemplate 이 반복 출력된다.
                     runat="server" → 서버 컨트롤 --%>
                <asp:Repeater ID="rptRecentSearch" runat="server">
                    <ItemTemplate>
                        <%-- 각 검색어 항목 행 --%>
                        <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                            <%-- 검색어 링크: 클릭 시 해당 검색어로 SearchResults.aspx 를 열어준다.
                                 Server.UrlEncode(Eval("Query").ToString()) : 검색어를 URL 안전 문자로 인코딩한다.
                                 Eval("Query") : 데이터 소스의 "Query" 컬럼(검색어 문자열)을 출력한다. --%>
                            <a href='SearchResults.aspx?q=<%# Server.UrlEncode(Eval("Query").ToString()) %>'
                               class="text-sm text-gray-700 dark:text-slate-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors flex items-center gap-2">
                                <%-- data-lucide="search" : Lucide 아이콘 - 돋보기(검색) 아이콘 --%>
                                <i data-lucide="search" class="w-3.5 h-3.5 text-gray-400 flex-shrink-0"></i>
                                <%-- Eval("Query") : 검색어 텍스트를 출력한다. --%>
                                <%# Eval("Query") %>
                            </a>
                            <%-- 검색 날짜: Eval("SearchTime") 데이터 컬럼을 DateTime 으로 변환 후 "MM.dd" 형식으로 표시
                                 예: 2026.04.30 → "04.30" --%>
                            <span class="text-xs text-gray-400 dark:text-slate-500 flex-shrink-0">
                                <%# Convert.ToDateTime(Eval("SearchTime")).ToString("MM.dd") %>
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- ===== 내 게시글 카드 ===== --%>
            <%-- 내 게시글 카드 --%>
            <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-2xl p-6 shadow-sm">
                <div class="flex items-center justify-between mb-5">
                    <h2 class="font-semibold dark:text-white flex items-center gap-2">
                        <%-- data-lucide="file-text" : Lucide 아이콘 - 문서(게시글) 아이콘
                             text-purple-500 : 보라색 아이콘 --%>
                        <i data-lucide="file-text" class="w-4 h-4 text-purple-500"></i>
                        <%-- 내 게시글 카드 제목: 코드비하인드에서 litMyPostsLbl.Text 로 설정 --%>
                        <asp:Literal ID="litMyPostsLbl" runat="server"></asp:Literal>
                    </h2>
                    <%-- "전체 보기" 링크 → Community.aspx 로 이동
                         litViewAllPosts : 코드비하인드에서 "전체 보기" 또는 "View All" 텍스트를 설정한다. --%>
                    <a href="Community.aspx" class="text-xs text-blue-500 hover:underline font-semibold">
                        <asp:Literal ID="litViewAllPosts" runat="server"></asp:Literal>
                    </a>
                </div>

                <%-- 게시글이 없을 때 표시하는 메시지
                     ID="litNoPosts" : 코드비하인드에서 데이터가 없으면 litNoPosts.Visible = true 로 표시한다.
                     Visible="false" : 기본적으로 숨겨져 있다. --%>
                <asp:Literal ID="litNoPosts" runat="server" Visible="false"></asp:Literal>

                <%-- ===== 내 게시글 반복 목록 =====
                     ID="rptMyPosts" : 코드비하인드에서 rptMyPosts.DataSource 에 게시글 목록을 바인딩하고
                                       rptMyPosts.DataBind() 를 호출하면 ItemTemplate 이 반복 출력된다.
                     runat="server" → 서버 컨트롤 --%>
                <asp:Repeater ID="rptMyPosts" runat="server">
                    <ItemTemplate>
                        <%-- 각 게시글 항목 행 --%>
                        <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                            <%-- 게시글 제목 링크: 클릭 시 해당 게시글 상세 페이지로 이동
                                 Eval("No") : 데이터 소스의 "No" 컬럼(게시글 고유 번호)을 URL 파라미터로 사용한다.
                                 truncate max-w-[70%] : 긴 제목이 넘치면 ... 으로 줄임 --%>
                            <a href='CommunityView.aspx?no=<%# Eval("No") %>'
                               class="text-sm font-semibold text-gray-800 dark:text-gray-100 hover:text-blue-500 dark:hover:text-blue-400 transition-colors truncate max-w-[70%]">
                                <%-- Eval("Title") : 데이터 소스의 "Title" 컬럼(게시글 제목)을 출력한다. --%>
                                <%# Eval("Title") %>
                            </a>
                            <%-- 작성 날짜: Eval("UploadTime") 데이터 컬럼을 DateTime 으로 변환 후 "MM.dd" 형식으로 표시
                                 예: 2026-04-30 14:23:00 → "04.30" --%>
                            <span class="text-xs text-gray-400 dark:text-slate-500 flex-shrink-0">
                                <%# Convert.ToDateTime(Eval("UploadTime")).ToString("MM.dd") %>
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Content>
