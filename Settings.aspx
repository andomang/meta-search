<%-- ===== 페이지 지시문 =====
     Title       : 브라우저 탭에 표시될 페이지 제목 ("Settings")
     Language    : 코드비하인드 언어 - C# 사용
     MasterPageFile: ~/Site.Master 를 레이아웃 틀로 사용한다.
                    Site.Master 가 공통 헤더/푸터/CSS 를 포함하며,
                    이 페이지의 콘텐츠는 Master 의 ContentPlaceHolder 에 삽입된다.
     AutoEventWireup: true → Page_Load 등 이벤트 메서드를 자동으로 연결한다.
     CodeFile    : 짝을 이루는 C# 코드비하인드 파일 경로 (Settings.aspx.cs)
     Inherits    : 코드비하인드의 클래스 이름 (Settings) --%>
<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="Settings" %>

<%-- ===== 메인 콘텐츠 영역 =====
     Site.Master 의 "MainContent" ContentPlaceHolder 자리에 이 블록이 삽입된다.
     runat="server" → 서버에서 처리하는 ASP.NET 컨트롤임을 의미한다. --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ===== 페이지 전체 래퍼 =====
         max-w-4xl : 최대 너비 56rem(896px) 제한
         mx-auto   : 가운데 정렬
         px-6 py-12: 좌우 패딩 24px, 위아래 패딩 48px
         min-h-screen : 최소 화면 전체 높이 확보 --%>
    <div class="max-w-4xl mx-auto px-6 py-12 min-h-screen">

        <%-- 페이지 제목: 코드비하인드에서 litPageTitle.Text 로 "설정" 또는 "Settings" 를 설정한다.
             text-3xl font-bold : 크고 굵은 폰트 --%>
        <h1 class="text-3xl font-bold mb-10 dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h1>

        <%-- ===== 탭 네비게이션 =====
             flex gap-2 mb-8 border-b : 탭 버튼을 가로로 나열하고 아래 구분선을 표시한다.
             JavaScript 의 switchTab() 함수가 탭 전환을 처리한다. --%>
        <div class="flex gap-2 mb-8 border-b dark:border-slate-700">

            <%-- [일반] 탭 버튼
                 onclick="switchTab('general')" : 클릭 시 일반 탭 패널을 표시하고 개인정보 패널을 숨긴다.
                 id="tabGeneral" : JavaScript 에서 활성/비활성 클래스를 동적으로 교체할 때 사용한다.
                 border-b-2 border-blue-500 : 기본적으로 활성 상태(파란 밑줄) 로 시작한다.
                 litTabGeneral : 코드비하인드에서 "일반" 또는 "General" 텍스트를 설정한다. --%>
            <button type="button" onclick="switchTab('general')" id="tabGeneral"
                class="tab-btn px-6 py-3 font-bold text-sm border-b-2 border-blue-500 text-blue-500 dark:text-blue-400 transition-all">
                <asp:Literal ID="litTabGeneral" runat="server"></asp:Literal>
            </button>

            <%-- [개인정보] 탭 버튼
                 onclick="switchTab('privacy')" : 클릭 시 개인정보 탭 패널을 표시하고 일반 패널을 숨긴다.
                 id="tabPrivacy" : JavaScript 에서 활성/비활성 클래스를 동적으로 교체할 때 사용한다.
                 border-transparent : 비활성 상태이므로 밑줄이 투명하다.
                 litTabPrivacy : 코드비하인드에서 "개인정보" 또는 "Privacy" 텍스트를 설정한다. --%>
            <button type="button" onclick="switchTab('privacy')" id="tabPrivacy"
                class="tab-btn px-6 py-3 font-bold text-sm border-b-2 border-transparent text-gray-400 hover:text-gray-600 dark:hover:text-slate-300 transition-all">
                <asp:Literal ID="litTabPrivacy" runat="server"></asp:Literal>
            </button>
        </div>

        <%-- ===== 일반 탭 패널 =====
             id="panelGeneral" : JavaScript switchTab() 에서 이 id 로 표시/숨김 처리
             space-y-6 : 자식 카드들 사이 세로 간격 24px --%>
        <%-- 일반 탭 --%>
        <div id="panelGeneral" class="space-y-6">

            <%-- ===== 프로필 사진 카드 ===== --%>
            <%-- 프로필 사진 --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <%-- 프로필 사진 섹션 제목: 코드비하인드에서 litProfilePhotoLabel.Text 로 설정 --%>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litProfilePhotoLabel" runat="server"></asp:Literal></h2>
                        <%-- 프로필 사진 부제목(설명): 코드비하인드에서 litProfilePhotoSub.Text 로 설정 --%>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litProfilePhotoSub" runat="server"></asp:Literal></p>
                    </div>
                    <div class="flex items-center gap-4">
                        <%-- 현재 아바타 표시 영역
                             w-14 h-14 rounded-full : 56px × 56px 원형
                             bg-blue-500 : 기본 아바타 배경색(사진 없을 때 이니셜 표시)
                             litCurrentAvatar : 코드비하인드에서 현재 아바타 이미지 또는 이니셜을 출력한다. --%>
                        <div class="w-14 h-14 rounded-full overflow-hidden bg-blue-500 flex items-center justify-center text-white text-xl font-bold flex-shrink-0">
                            <asp:Literal ID="litCurrentAvatar" runat="server"></asp:Literal>
                        </div>
                        <div class="flex flex-col gap-2">
                            <%-- 파일 업로드 컨트롤
                                 ID="fuPhoto" : 코드비하인드에서 fuPhoto.HasFile 로 파일 선택 여부를 확인하고,
                                                fuPhoto.SaveAs() 또는 fuPhoto.PostedFile 로 파일을 처리한다.
                                 runat="server" → 서버 컨트롤 --%>
                            <asp:FileUpload ID="fuPhoto" runat="server" CssClass="text-xs text-gray-500 dark:text-slate-400" />
                            <%-- 사진 업로드 버튼
                                 ID="btnUploadPhoto" : 코드비하인드의 btnUploadPhoto_Click 이벤트와 연결된다.
                                 OnClick="btnUploadPhoto_Click" : 클릭 시 서버에서 파일 저장 처리를 수행한다.
                                 runat="server" → 서버 컨트롤 --%>
                            <asp:Button ID="btnUploadPhoto" runat="server" OnClick="btnUploadPhoto_Click"
                                CssClass="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold text-sm cursor-pointer transition-all" />
                        </div>
                    </div>
                </div>
            </div>

            <%-- ===== 테마(다크모드) 전환 카드 ===== --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <%-- 테마 레이블: litThemeLabel 에 "테마" 또는 "Theme" 를 출력한다.
                             litThemeStatus: 현재 테마 상태 ("다크" / "라이트" 등)를 코드비하인드에서 설정한다. --%>
                        <h2 class="text-xl font-bold dark:text-white flex items-center gap-2">
                            <asp:Literal ID="litThemeLabel" runat="server"></asp:Literal>: <asp:Literal ID="litThemeStatus" runat="server"></asp:Literal>
                        </h2>
                        <%-- 테마 부제목(설명): 코드비하인드에서 litThemeSub.Text 로 설정 --%>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litThemeSub" runat="server"></asp:Literal></p>
                    </div>
                    <%-- 다크모드 전환 버튼
                         ID="btnToggleDark" : 코드비하인드의 btnToggleDark_Click 이벤트와 연결된다.
                         OnClick="btnToggleDark_Click" : 클릭 시 서버에서 다크/라이트 모드를 전환한다.
                         runat="server" → 서버 컨트롤
                         active:scale-95 : 클릭 시 살짝 줄어드는 눌림 효과 --%>
                    <asp:Button ID="btnToggleDark" runat="server" OnClick="btnToggleDark_Click"
                        CssClass="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95 shadow-lg shadow-blue-200 dark:shadow-none" />
                </div>
            </div>

            <%-- ===== 언어 설정 카드 ===== --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <%-- 언어 설정 제목: 코드비하인드에서 litLangLabel.Text 로 설정 --%>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litLangLabel" runat="server"></asp:Literal></h2>
                        <%-- 언어 설정 부제목: 코드비하인드에서 litLangSub.Text 로 설정 --%>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litLangSub" runat="server"></asp:Literal></p>
                    </div>
                    <div class="flex gap-3">
                        <%-- 한국어 선택 버튼
                             ID="btnLangKo" : 코드비하인드의 btnLangKo_Click 이벤트와 연결된다.
                             OnClick="btnLangKo_Click" : 클릭 시 언어를 한국어로 전환한다.
                             runat="server" → 서버 컨트롤 --%>
                        <asp:Button ID="btnLangKo" runat="server" Text="한국어" OnClick="btnLangKo_Click"
                            CssClass="px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer" />
                        <%-- 영어 선택 버튼
                             ID="btnLangEn" : 코드비하인드의 btnLangEn_Click 이벤트와 연결된다.
                             OnClick="btnLangEn_Click" : 클릭 시 언어를 영어로 전환한다.
                             runat="server" → 서버 컨트롤 --%>
                        <asp:Button ID="btnLangEn" runat="server" Text="English" OnClick="btnLangEn_Click"
                            CssClass="px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer" />
                    </div>
                </div>
            </div>

            <%-- ===== 내 정보 수정 카드 ===== --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <%-- 내 정보 섹션 제목: 코드비하인드에서 litMyInfoLabel.Text 로 설정 --%>
                <h2 class="text-xl font-bold mb-6 dark:text-white"><asp:Literal ID="litMyInfoLabel" runat="server"></asp:Literal></h2>

                <%-- grid grid-cols-1 md:grid-cols-2 gap-6 : 모바일은 1열, 768px 이상은 2열 그리드 --%>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">

                    <%-- 이름 수정 필드 --%>
                    <div>
                        <%-- 이름 레이블: 코드비하인드에서 litNameLbl.Text 로 설정 --%>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litNameLbl" runat="server"></asp:Literal></label>
                        <%-- 이름 수정 입력창
                             ID="editName" : 코드비하인드의 Page_Load 에서 editName.Text 에 현재 이름을 미리 채우고,
                                             btnUpdate_Click 에서 editName.Text 로 수정된 이름을 읽는다.
                             runat="server" → 서버 컨트롤 --%>
                        <asp:TextBox ID="editName" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>

                    <%-- 닉네임 수정 필드 --%>
                    <div>
                        <%-- 닉네임 레이블: 코드비하인드에서 litNickLbl.Text 로 설정 --%>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litNickLbl" runat="server"></asp:Literal></label>
                        <%-- 닉네임 수정 입력창
                             ID="editNickname" : 코드비하인드의 Page_Load 에서 editNickname.Text 에 현재 닉네임을 미리 채우고,
                                                  btnUpdate_Click 에서 editNickname.Text 로 수정된 닉네임을 읽는다.
                             runat="server" → 서버 컨트롤 --%>
                        <asp:TextBox ID="editNickname" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>

                    <%-- 이메일 수정 필드 --%>
                    <div>
                        <%-- 이메일 레이블: 코드비하인드에서 litEmailLbl.Text 로 설정 --%>
                        <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litEmailLbl" runat="server"></asp:Literal></label>
                        <%-- 이메일 수정 입력창
                             ID="editEmail" : 코드비하인드의 Page_Load 에서 editEmail.Text 에 현재 이메일을 미리 채우고,
                                              btnUpdate_Click 에서 editEmail.Text 로 수정된 이메일을 읽는다.
                             runat="server" → 서버 컨트롤 --%>
                        <asp:TextBox ID="editEmail" runat="server" CssClass="w-full p-4 rounded-xl border dark:bg-slate-700 dark:text-white dark:border-slate-600 outline-none focus:ring-2 focus:ring-blue-500 transition-all"></asp:TextBox>
                    </div>
                </div>

                <%-- ===== 하단 버튼 영역 ===== --%>
                <div class="mt-8 flex justify-between items-center">
                    <%-- 비밀번호 변경 링크 (일반 HTML 버튼)
                         onclick="openPwModal()" : 클릭 시 JavaScript 의 openPwModal() 함수를 호출하여
                                                   비밀번호 변경 모달 다이얼로그를 열어준다.
                         litChangePwBtn : 코드비하인드에서 "비밀번호 변경" 또는 "Change Password" 텍스트를 설정한다. --%>
                    <button type="button" onclick="openPwModal()" class="text-sm text-blue-500 hover:underline font-semibold">
                        <asp:Literal ID="litChangePwBtn" runat="server"></asp:Literal>
                    </button>
                    <%-- 정보 저장 버튼
                         ID="btnUpdate" : 코드비하인드의 btnUpdate_Click 이벤트와 연결된다.
                         OnClick="btnUpdate_Click" : 클릭 시 서버에서 이름/닉네임/이메일을 DB에 저장한다.
                         runat="server" → 서버 컨트롤
                         bg-gray-900 dark:bg-white dark:text-black : 라이트는 검정, 다크는 흰색 버튼 --%>
                    <asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click"
                        CssClass="px-8 py-3 bg-gray-900 dark:bg-white dark:text-black text-white rounded-xl font-bold cursor-pointer hover:opacity-90 transition-all active:scale-95" />
                </div>
            </div>
        </div>

        <%-- ===== 개인정보 탭 패널 =====
             id="panelPrivacy" : JavaScript switchTab() 에서 이 id 로 표시/숨김 처리
             hidden : 기본적으로 숨겨져 있으며, [개인정보] 탭 클릭 시 표시된다. --%>
        <%-- 개인정보 탭 --%>
        <div id="panelPrivacy" class="space-y-6 hidden">

            <%-- ===== 이용 통계 카드 ===== --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <%-- 통계 섹션 제목: 코드비하인드에서 litStatsLabel.Text 로 설정 --%>
                <h2 class="text-xl font-bold mb-6 dark:text-white"><asp:Literal ID="litStatsLabel" runat="server"></asp:Literal></h2>

                <%-- 통계 숫자 카드 3개: grid-cols-3 으로 3열 배치
                     sm:grid-cols-3 : 640px 이상에서 3열 배치, 그 미만은 1열 --%>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">

                    <%-- 총 검색 횟수 카드
                         litTotalSearch : 코드비하인드에서 검색 횟수 숫자를 설정한다.
                         litTotalSearchLbl : "총 검색 횟수" 레이블 텍스트를 설정한다. --%>
                    <div class="bg-gray-50 dark:bg-slate-700 rounded-2xl p-5 text-center">
                        <p class="text-3xl font-black text-blue-500"><asp:Literal ID="litTotalSearch" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litTotalSearchLbl" runat="server"></asp:Literal></p>
                    </div>

                    <%-- 총 클릭 횟수 카드
                         litTotalClick : 코드비하인드에서 클릭 횟수 숫자를 설정한다.
                         litTotalClickLbl : "총 클릭 횟수" 레이블 텍스트를 설정한다. --%>
                    <div class="bg-gray-50 dark:bg-slate-700 rounded-2xl p-5 text-center">
                        <p class="text-3xl font-black text-purple-500"><asp:Literal ID="litTotalClick" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litTotalClickLbl" runat="server"></asp:Literal></p>
                    </div>

                    <%-- 최다 검색 카테고리 카드
                         litTopCategory : 코드비하인드에서 가장 많이 검색한 카테고리명을 설정한다.
                         litTopCategoryLbl : "최다 카테고리" 레이블 텍스트를 설정한다. --%>
                    <div class="bg-gray-50 dark:bg-slate-700 rounded-2xl p-5 text-center">
                        <p class="text-3xl font-black text-pink-500"><asp:Literal ID="litTopCategory" runat="server"></asp:Literal></p>
                        <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litTopCategoryLbl" runat="server"></asp:Literal></p>
                    </div>
                </div>

                <%-- 상위 5개 키워드 레이블: 코드비하인드에서 litTop5Lbl.Text 로 설정 --%>
                <h3 class="font-bold text-sm text-gray-500 dark:text-slate-400 mb-3"><asp:Literal ID="litTop5Lbl" runat="server"></asp:Literal></h3>

                <%-- ===== 상위 키워드 반복 목록 =====
                     ID="rptTopKeywords" : 코드비하인드에서 rptTopKeywords.DataSource 에 키워드 목록을 바인딩하고
                                           rptTopKeywords.DataBind() 를 호출하면 ItemTemplate 이 반복 출력된다.
                     runat="server" → 서버 컨트롤 --%>
                <asp:Repeater ID="rptTopKeywords" runat="server">
                    <ItemTemplate>
                        <%-- 각 키워드 항목 행 --%>
                        <div class="flex items-center justify-between py-2 border-b dark:border-slate-700 last:border-0">
                            <span class="text-sm font-medium dark:text-white">
                                <%-- Container.ItemIndex + 1 : 현재 반복 인덱스(0부터 시작)에 +1 하여 순위 번호를 표시한다. --%>
                                <%# Container.ItemIndex + 1 %>.
                                <%-- Eval("Query") : 데이터 소스의 "Query" 컬럼 값(검색어 문자열)을 출력한다. --%>
                                <%# Eval("Query") %>
                            </span>
                            <span class="text-xs text-gray-400 dark:text-slate-500">
                                <%-- Eval("SearchCount") : 데이터 소스의 "SearchCount" 컬럼 값(검색 횟수)을 출력하고 "회" 단위를 붙인다. --%>
                                <%# Eval("SearchCount") %>회
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- ===== 검색 기록 삭제 카드 ===== --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <%-- 검색 기록 삭제 제목: 코드비하인드에서 litDelSearchLabel.Text 로 설정 --%>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litDelSearchLabel" runat="server"></asp:Literal></h2>
                        <%-- 검색 기록 삭제 부제목: 코드비하인드에서 litDelSearchSub.Text 로 설정 --%>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litDelSearchSub" runat="server"></asp:Literal></p>
                    </div>
                    <%-- 검색 기록 삭제 버튼 (일반 HTML 버튼)
                         onclick="openDeleteModal('search')" : JavaScript 의 openDeleteModal() 에 'search' 인자를 전달하여
                                                               검색 기록 삭제 모달을 열어준다.
                         litDelSearchBtn : 코드비하인드에서 버튼 텍스트를 설정한다. --%>
                    <button type="button" onclick="openDeleteModal('search')"
                        class="px-6 py-3 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95">
                        <asp:Literal ID="litDelSearchBtn" runat="server"></asp:Literal>
                    </button>
                </div>
            </div>

            <%-- ===== 클릭 기록 삭제 카드 ===== --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border dark:border-slate-700 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <%-- 클릭 기록 삭제 제목: 코드비하인드에서 litDelClickLabel.Text 로 설정 --%>
                        <h2 class="text-xl font-bold dark:text-white"><asp:Literal ID="litDelClickLabel" runat="server"></asp:Literal></h2>
                        <%-- 클릭 기록 삭제 부제목: 코드비하인드에서 litDelClickSub.Text 로 설정 --%>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litDelClickSub" runat="server"></asp:Literal></p>
                    </div>
                    <%-- 클릭 기록 삭제 버튼 (일반 HTML 버튼)
                         onclick="openDeleteModal('click')" : JavaScript 의 openDeleteModal() 에 'click' 인자를 전달하여
                                                              클릭 기록 삭제 모달을 열어준다.
                         litDelClickBtn : 코드비하인드에서 버튼 텍스트를 설정한다. --%>
                    <button type="button" onclick="openDeleteModal('click')"
                        class="px-6 py-3 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95">
                        <asp:Literal ID="litDelClickBtn" runat="server"></asp:Literal>
                    </button>
                </div>
            </div>

            <%-- ===== 회원 탈퇴 카드 =====
                 border-red-200 dark:border-red-900/50 : 위험을 나타내는 빨간 테두리 --%>
            <div class="bg-white dark:bg-slate-800 p-8 rounded-3xl border border-red-200 dark:border-red-900/50 shadow-sm">
                <div class="flex items-center justify-between">
                    <div>
                        <%-- 회원 탈퇴 제목: 코드비하인드에서 litWithdrawLabel.Text 로 설정
                             text-red-500 : 위험 경고를 나타내는 빨간색 텍스트 --%>
                        <h2 class="text-xl font-bold text-red-500"><asp:Literal ID="litWithdrawLabel" runat="server"></asp:Literal></h2>
                        <%-- 회원 탈퇴 부제목(경고 메시지): 코드비하인드에서 litWithdrawSub.Text 로 설정 --%>
                        <p class="text-gray-500 dark:text-slate-400 text-sm mt-1"><asp:Literal ID="litWithdrawSub" runat="server"></asp:Literal></p>
                    </div>
                    <%-- 회원 탈퇴 버튼 (일반 HTML 버튼)
                         onclick="openWithdrawModal()" : JavaScript 의 openWithdrawModal() 을 호출하여
                                                         비밀번호 확인 탈퇴 모달을 열어준다.
                         litWithdrawBtn : 코드비하인드에서 버튼 텍스트를 설정한다. --%>
                    <button type="button" onclick="openWithdrawModal()"
                        class="px-6 py-3 bg-white dark:bg-slate-700 border border-red-300 dark:border-red-700 text-red-500 hover:bg-red-500 hover:text-white rounded-xl font-bold cursor-pointer transition-all active:scale-95">
                        <asp:Literal ID="litWithdrawBtn" runat="server"></asp:Literal>
                    </button>
                </div>
            </div>
        </div>
    </div>

    <%-- ===== 기록 삭제 모달 =====
         id="deleteModal" : JavaScript 의 openDeleteModal() / closeDeleteModal() 에서 제어한다.
         fixed inset-0 bg-black/50 : 화면 전체를 반투명 검정으로 덮는 오버레이
         z-[100] : 다른 모든 요소 위에 표시
         hidden : 기본적으로 숨겨져 있다. --%>
    <%-- 기록 삭제 모달 --%>
    <div id="deleteModal" class="hidden fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4">
        <div class="bg-white dark:bg-slate-800 p-8 rounded-2xl w-full max-w-sm shadow-2xl">
            <%-- 모달 제목: JavaScript 의 openDeleteModal() 에서 검색/클릭 기록 구분에 맞게 텍스트를 동적으로 설정한다. --%>
            <h2 class="text-xl font-bold mb-2 dark:text-white" id="deleteModalTitle"></h2>
            <%-- 모달 설명 텍스트: 코드비하인드에서 litDelModalDesc.Text 로 설정 --%>
            <p class="text-sm text-gray-500 dark:text-slate-400 mb-6"><asp:Literal ID="litDelModalDesc" runat="server"></asp:Literal></p>

            <%-- ===== 삭제 범위 선택 버튼 목록 =====
                 각 버튼의 onclick="confirmDelete('범위')" 는 JavaScript 의 confirmDelete() 함수를 호출한다.
                 'range' 인자로 서버 API 에 삭제 기간 범위를 전달한다. --%>
            <div class="space-y-3 mb-8">
                <%-- 1시간 이내 기록 삭제 --%>
                <button type="button" onclick="confirmDelete('1hour')"  class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit1Hour" runat="server"></asp:Literal></button>
                <%-- 12시간 이내 기록 삭제 --%>
                <button type="button" onclick="confirmDelete('12hour')" class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit12Hour" runat="server"></asp:Literal></button>
                <%-- 1일 이내 기록 삭제 --%>
                <button type="button" onclick="confirmDelete('1day')"   class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit1Day" runat="server"></asp:Literal></button>
                <%-- 7일 이내 기록 삭제 --%>
                <button type="button" onclick="confirmDelete('7day')"   class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit7Day" runat="server"></asp:Literal></button>
                <%-- 30일 이내 기록 삭제 --%>
                <button type="button" onclick="confirmDelete('30day')"  class="del-btn w-full py-3 rounded-xl border dark:border-slate-600 text-gray-700 dark:text-slate-300 hover:bg-red-50 dark:hover:bg-red-900/20 hover:border-red-300 hover:text-red-600 dark:hover:text-red-400 font-medium transition-all text-left px-5"><asp:Literal ID="lit30Day" runat="server"></asp:Literal></button>
                <%-- 전체 기록 삭제 (강조 스타일: 빨간 테두리/텍스트, 마우스 오버 시 빨간 배경) --%>
                <button type="button" onclick="confirmDelete('all')"    class="del-btn w-full py-3 rounded-xl border border-red-300 dark:border-red-700 text-red-500 dark:text-red-400 hover:bg-red-500 hover:text-white font-bold transition-all text-left px-5"><asp:Literal ID="litDelAll" runat="server"></asp:Literal></button>
            </div>
            <%-- 모달 닫기 버튼
                 onclick="closeDeleteModal()" : 모달을 숨기고 닫는다.
                 litDelCancel : 코드비하인드에서 "취소" 또는 "Cancel" 텍스트를 설정한다. --%>
            <button type="button" onclick="closeDeleteModal()" class="w-full text-gray-400 dark:text-slate-500 text-sm hover:text-gray-600 transition-all"><asp:Literal ID="litDelCancel" runat="server"></asp:Literal></button>
        </div>
    </div>

    <%-- ===== 비밀번호 변경 모달 =====
         id="pwModal" : JavaScript 의 openPwModal() / closePwModal() 에서 제어한다.
         hidden : 기본적으로 숨겨져 있다. --%>
    <%-- 비밀번호 변경 모달 --%>
    <div id="pwModal" class="hidden fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4">
        <div class="bg-white dark:bg-slate-800 p-8 rounded-2xl w-full max-w-sm shadow-2xl">
            <%-- 모달 제목: 코드비하인드에서 litPwModalTitle.Text 로 "비밀번호 변경" 텍스트를 설정 --%>
            <h2 class="text-xl font-bold mb-6 dark:text-white"><asp:Literal ID="litPwModalTitle" runat="server"></asp:Literal></h2>

            <div class="space-y-4 mb-6">
                <%-- 현재 비밀번호 입력 --%>
                <div>
                    <%-- 현재 비밀번호 레이블: 코드비하인드에서 litPwCurLbl.Text 로 설정 --%>
                    <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litPwCurLbl" runat="server"></asp:Literal></label>
                    <%-- 현재 비밀번호 입력창 (순수 HTML input 태그)
                         id="pwCurrent" : JavaScript 의 submitPwChange() 에서 이 id 로 값을 읽는다.
                         type="password" : 입력 내용이 ●●● 으로 가려진다. --%>
                    <input type="password" id="pwCurrent" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <%-- 새 비밀번호 입력 --%>
                <div>
                    <%-- 새 비밀번호 레이블: 코드비하인드에서 litPwNewLbl.Text 로 설정 --%>
                    <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litPwNewLbl" runat="server"></asp:Literal></label>
                    <%-- 새 비밀번호 입력창
                         id="pwNew" : JavaScript 의 submitPwChange() 에서 이 id 로 값을 읽는다. --%>
                    <input type="password" id="pwNew" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <%-- 새 비밀번호 확인 입력 --%>
                <div>
                    <%-- 새 비밀번호 확인 레이블: 코드비하인드에서 litPwConfirmLbl.Text 로 설정 --%>
                    <label class="block text-sm font-semibold mb-2 dark:text-gray-300"><asp:Literal ID="litPwConfirmLbl" runat="server"></asp:Literal></label>
                    <%-- 새 비밀번호 확인 입력창
                         id="pwConfirm" : JavaScript 에서 pwNew 와 값이 일치하는지 비교한다. --%>
                    <input type="password" id="pwConfirm" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <%-- 오류 메시지 표시 영역
                     id="pwError" : JavaScript 에서 검증 실패 메시지를 이 요소에 표시한다.
                     hidden : 기본적으로 숨겨져 있고, 오류 발생 시 hidden 클래스가 제거된다. --%>
                <p id="pwError" class="text-red-500 text-xs hidden"></p>
            </div>

            <%-- 비밀번호 변경 제출 버튼 (일반 HTML 버튼)
                 onclick="submitPwChange()" : 입력값을 검증한 뒤 서버 API 로 비밀번호 변경을 요청한다.
                 litPwSubmitBtn : 코드비하인드에서 "변경" 또는 "Change" 텍스트를 설정한다. --%>
            <button type="button" onclick="submitPwChange()" class="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold transition-all mb-3"><asp:Literal ID="litPwSubmitBtn" runat="server"></asp:Literal></button>
            <%-- 모달 닫기 버튼
                 onclick="closePwModal()" : 모달을 닫고 입력값을 초기화한다.
                 litPwCancelBtn : 코드비하인드에서 "취소" 또는 "Cancel" 텍스트를 설정한다. --%>
            <button type="button" onclick="closePwModal()" class="w-full text-gray-400 text-sm hover:text-gray-600 transition-all"><asp:Literal ID="litPwCancelBtn" runat="server"></asp:Literal></button>
        </div>
    </div>

    <%-- ===== 회원 탈퇴 모달 =====
         id="withdrawModal" : JavaScript 의 openWithdrawModal() / closeWithdrawModal() 에서 제어한다.
         hidden : 기본적으로 숨겨져 있다. --%>
    <%-- 회원 탈퇴 모달 --%>
    <div id="withdrawModal" class="hidden fixed inset-0 bg-black/50 z-[100] flex items-center justify-center p-4">
        <div class="bg-white dark:bg-slate-800 p-8 rounded-2xl w-full max-w-sm shadow-2xl">
            <%-- 탈퇴 모달 제목: 코드비하인드에서 litWdModalTitle.Text 로 설정
                 text-red-500 : 위험한 동작임을 빨간색으로 강조 --%>
            <h2 class="text-xl font-bold mb-2 text-red-500"><asp:Literal ID="litWdModalTitle" runat="server"></asp:Literal></h2>
            <%-- 탈퇴 안내 메시지: 코드비하인드에서 litWdDesc.Text 로 설정 --%>
            <p class="text-sm text-gray-500 dark:text-slate-400 mb-6"><asp:Literal ID="litWdDesc" runat="server"></asp:Literal></p>

            <div class="space-y-4 mb-6">
                <%-- 탈퇴 확인용 비밀번호 입력창
                     id="withdrawPw" : JavaScript 의 submitWithdraw() 에서 이 id 로 값을 읽어 서버로 전송한다. --%>
                <input type="password" id="withdrawPw" class="w-full p-3 border rounded-xl dark:bg-slate-700 dark:border-slate-600 dark:text-white outline-none focus:ring-2 focus:ring-red-500" />
                <%-- 탈퇴 오류 메시지 표시 영역
                     id="withdrawError" : JavaScript 에서 비밀번호 불일치 등 오류 메시지를 표시한다. --%>
                <p id="withdrawError" class="text-red-500 text-xs hidden"></p>
            </div>

            <%-- 탈퇴 확인 버튼 (일반 HTML 버튼)
                 onclick="submitWithdraw()" : 비밀번호를 서버로 전송하여 탈퇴를 처리한다.
                                              성공 시 Default.aspx 로 이동한다.
                 litWdSubmitBtn : 코드비하인드에서 "탈퇴" 또는 "Withdraw" 텍스트를 설정한다. --%>
            <button type="button" onclick="submitWithdraw()" class="w-full py-3 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold transition-all mb-3"><asp:Literal ID="litWdSubmitBtn" runat="server"></asp:Literal></button>
            <%-- 모달 닫기 버튼
                 onclick="closeWithdrawModal()" : 모달을 닫고 입력값을 초기화한다.
                 litWdCancelBtn : 코드비하인드에서 "취소" 또는 "Cancel" 텍스트를 설정한다. --%>
            <button type="button" onclick="closeWithdrawModal()" class="w-full text-gray-400 text-sm hover:text-gray-600 transition-all"><asp:Literal ID="litWdCancelBtn" runat="server"></asp:Literal></button>
        </div>
    </div>

    <%-- ===== 인라인 JavaScript =====
         설정 페이지의 탭 전환, 기록 삭제 모달, 비밀번호 변경 모달, 회원 탈퇴 모달을 제어한다. --%>
    <script>
        // ===== 서버에서 주입되는 다국어 메시지 =====
        // Lang.Get("...") : 현재 언어 설정에 맞는 문자열 반환 (<= ... %> 는 서버 출력 표현식)
        var deleteTarget    = 'search'; // 현재 삭제 대상: 'search'(검색 기록) 또는 'click'(클릭 기록)

        // 삭제 성공/오류 알림 메시지
        var msgDeleted      = '<%= Lang.Get("set.delBtn") == "Delete" ? "Records deleted." : "기록이 삭제되었습니다." %>';
        var msgError        = '<%= Lang.Get("set.delBtn") == "Delete" ? "An error occurred." : "오류가 발생했습니다." %>';

        // 비밀번호 변경 유효성 메시지
        var msgPwMismatch   = '<%= Lang.Get("set.delBtn") == "Delete" ? "Passwords do not match." : "새 비밀번호가 일치하지 않습니다." %>';
        var msgPwShort      = '<%= Lang.Get("set.delBtn") == "Delete" ? "Password must be at least 4 characters." : "비밀번호는 4자 이상이어야 합니다." %>';
        var msgPwRequired   = '<%= Lang.Get("set.delBtn") == "Delete" ? "Please fill in all fields." : "모든 항목을 입력해주세요." %>';
        var msgPwWrong      = '<%= Lang.Get("set.delBtn") == "Delete" ? "Current password is incorrect." : "현재 비밀번호가 틀렸습니다." %>';

        // 회원 탈퇴 유효성 메시지
        var msgWdRequired   = '<%= Lang.Get("set.delBtn") == "Delete" ? "Please enter your password." : "비밀번호를 입력해주세요." %>';
        var msgWdWrong      = '<%= Lang.Get("set.delBtn") == "Delete" ? "Password is incorrect." : "비밀번호가 틀렸습니다." %>';

        // 삭제 모달 제목 텍스트 (검색 기록 vs 클릭 기록)
        var delSearchTitle  = '<%= Lang.Get("set.delSearch") %>';
        var delClickTitle   = '<%= Lang.Get("set.delClick") %>';

        // ===== switchTab(tab) =====
        // 탭 버튼 클릭 시 호출된다.
        // tab: 'general'(일반) 또는 'privacy'(개인정보)
        // 선택된 탭의 패널을 표시하고 나머지를 숨긴다.
        // 탭 버튼의 활성/비활성 스타일(파란 밑줄 등)도 함께 전환한다.
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

        // ===== openDeleteModal(target) =====
        // target: 'search' 또는 'click' — 삭제할 기록 종류를 지정한다.
        // 모달 제목을 target 에 맞게 설정하고 모달을 화면에 표시한다.
        function openDeleteModal(target) {
            deleteTarget = target;
            document.getElementById('deleteModalTitle').textContent = target === 'search' ? delSearchTitle : delClickTitle;
            document.getElementById('deleteModal').classList.remove('hidden');
        }

        // ===== closeDeleteModal() =====
        // 기록 삭제 모달을 숨긴다.
        function closeDeleteModal() { document.getElementById('deleteModal').classList.add('hidden'); }

        // ===== confirmDelete(range) =====
        // range: '1hour' | '12hour' | '1day' | '7day' | '30day' | 'all'
        // 서버의 Settings.aspx?action=deleteHistory API 에 삭제 대상과 기간을 전달하여 기록을 삭제한다.
        // 성공 시 페이지를 새로고침하여 통계를 갱신한다.
        function confirmDelete(range) {
            fetch('Settings.aspx?action=deleteHistory&target=' + deleteTarget + '&range=' + range)
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    closeDeleteModal();
                    alert(d.result === 'ok' ? msgDeleted : msgError);
                    if (d.result === 'ok') location.reload(); // 통계 수치 갱신을 위해 페이지 새로고침
                });
        }

        // 모달 배경(오버레이) 클릭 시 모달을 닫는다
        document.getElementById('deleteModal').addEventListener('click', function (e) { if (e.target === this) closeDeleteModal(); });

        // ===== openPwModal() / closePwModal() =====
        // 비밀번호 변경 모달 열기/닫기
        // closePwModal 은 입력창 값과 오류 메시지를 초기화한다.
        function openPwModal() { document.getElementById('pwModal').classList.remove('hidden'); }
        function closePwModal() {
            document.getElementById('pwModal').classList.add('hidden');
            document.getElementById('pwCurrent').value = '';
            document.getElementById('pwNew').value = '';
            document.getElementById('pwConfirm').value = '';
            document.getElementById('pwError').classList.add('hidden');
        }

        // ===== submitPwChange() =====
        // 현재 비밀번호(cur), 새 비밀번호(nw), 확인(cf) 입력값을 검증한 뒤
        // Settings.aspx?action=changePw API 로 전송하여 비밀번호를 변경한다.
        function submitPwChange() {
            var cur = document.getElementById('pwCurrent').value; // 현재 비밀번호
            var nw  = document.getElementById('pwNew').value;     // 새 비밀번호
            var cf  = document.getElementById('pwConfirm').value; // 새 비밀번호 확인
            var err = document.getElementById('pwError');         // 오류 메시지 표시 요소

            // 모든 칸이 채워졌는지 검사
            if (!cur || !nw || !cf) { err.textContent = msgPwRequired; err.classList.remove('hidden'); return; }
            // 새 비밀번호와 확인이 일치하는지 검사
            if (nw !== cf)          { err.textContent = msgPwMismatch; err.classList.remove('hidden'); return; }
            // 새 비밀번호 최소 길이 검사 (4자 이상)
            if (nw.length < 4)      { err.textContent = msgPwShort;    err.classList.remove('hidden'); return; }

            // 서버 API 호출: 현재/새 비밀번호를 URL 인코딩하여 전달
            fetch('Settings.aspx?action=changePw&cur=' + encodeURIComponent(cur) + '&nw=' + encodeURIComponent(nw))
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    if (d.result === 'ok')        { closePwModal(); alert('<%= Lang.Get("set.delBtn") == "Delete" ? "Password changed." : "비밀번호가 변경되었습니다." %>'); }
                    else if (d.result === 'wrong') { err.textContent = msgPwWrong; err.classList.remove('hidden'); } // 현재 비밀번호 불일치
                    else { err.textContent = msgError; err.classList.remove('hidden'); }                            // 알 수 없는 오류
                });
        }

        // 모달 배경 클릭 시 비밀번호 변경 모달을 닫는다
        document.getElementById('pwModal').addEventListener('click', function (e) { if (e.target === this) closePwModal(); });

        // ===== openWithdrawModal() / closeWithdrawModal() =====
        // 회원 탈퇴 모달 열기/닫기
        // closeWithdrawModal 은 입력창 값과 오류 메시지를 초기화한다.
        function openWithdrawModal() { document.getElementById('withdrawModal').classList.remove('hidden'); }
        function closeWithdrawModal() {
            document.getElementById('withdrawModal').classList.add('hidden');
            document.getElementById('withdrawPw').value = '';
            document.getElementById('withdrawError').classList.add('hidden');
        }

        // ===== submitWithdraw() =====
        // 탈퇴 확인용 비밀번호(pw)를 Settings.aspx?action=withdraw API 로 전송한다.
        // 성공 시 Default.aspx 로 이동한다.
        function submitWithdraw() {
            var pw  = document.getElementById('withdrawPw').value;    // 탈퇴 확인용 비밀번호
            var err = document.getElementById('withdrawError');        // 오류 메시지 표시 요소

            // 비밀번호가 입력됐는지 검사
            if (!pw) { err.textContent = msgWdRequired; err.classList.remove('hidden'); return; }

            // 서버 API 호출: 비밀번호를 URL 인코딩하여 전달
            fetch('Settings.aspx?action=withdraw&pw=' + encodeURIComponent(pw))
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    if (d.result === 'ok')         { alert('<%= Lang.Get("set.delBtn") == "Delete" ? "Account deleted." : "탈퇴가 완료되었습니다." %>'); location.href = 'Default.aspx'; }
                    else if (d.result === 'wrong') { err.textContent = msgWdWrong; err.classList.remove('hidden'); } // 비밀번호 불일치
                    else { err.textContent = msgError; err.classList.remove('hidden'); }                            // 알 수 없는 오류
                });
        }

        // 모달 배경 클릭 시 탈퇴 모달을 닫는다
        document.getElementById('withdrawModal').addEventListener('click', function (e) { if (e.target === this) closeWithdrawModal(); });
    </script>
</asp:Content>
