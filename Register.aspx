<%-- ===== 페이지 지시문 =====
     Title       : 브라우저 탭에 표시될 페이지 제목 ("Register")
     Language    : 코드비하인드 언어 - C# 사용
     MasterPageFile: ~/Site.Master 를 레이아웃 틀로 사용한다.
                    Site.Master 가 공통 헤더/푸터/CSS 를 포함하며,
                    이 페이지의 콘텐츠는 Master 의 ContentPlaceHolder 에 삽입된다.
     AutoEventWireup: true → Page_Load, 버튼 클릭 등의 이벤트 메서드를 자동으로 연결한다.
     CodeFile    : 이 aspx 파일과 짝을 이루는 C# 코드비하인드 파일 경로 (Register.aspx.cs)
     Inherits    : 코드비하인드의 클래스 이름 (Register) --%>
<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>

<%-- ===== 메인 콘텐츠 영역 =====
     Site.Master 의 "MainContent" ContentPlaceHolder 자리에 이 블록이 삽입된다.
     runat="server" → 서버에서 처리하는 ASP.NET 컨트롤임을 의미한다. --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ===== 페이지 전체 래퍼 =====
         max-w-2xl : 최대 너비 42rem(672px) 제한
         mx-auto   : 가운데 정렬
         px-6 py-16: 좌우 패딩 24px, 위아래 패딩 64px
         min-h-screen : 최소 화면 전체 높이 확보 --%>
    <div class="max-w-2xl mx-auto px-6 py-16 min-h-screen">

        <%-- ===== 회원가입 카드 =====
             bg-white dark:bg-slate-800 : 라이트/다크 모드 배경색
             border border-gray-200 dark:border-slate-700 : 테두리
             rounded-3xl : 크게 둥근 모서리
             p-10 : 내부 패딩 40px
             shadow-sm : 가벼운 그림자 --%>
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl p-10 shadow-sm">

            <%-- 페이지 제목: 코드비하인드에서 litPageTitle.Text 로 "회원가입" 또는 "Register" 를 설정한다.
                 text-3xl font-extrabold : 크고 굵은 폰트
                 tracking-tight : 자간 좁힘 --%>
            <h2 class="text-3xl font-extrabold mb-10 text-gray-900 dark:text-white tracking-tight"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h2>

            <%-- ===== 입력 필드 그룹 =====
                 space-y-6 : 자식 요소 사이 세로 간격 24px --%>
            <div class="space-y-6">

                <%-- ===== 아이디 입력 필드 ===== --%>
                <div>
                    <%-- 아이디 레이블: 코드비하인드에서 litIdLbl.Text 로 "아이디" 또는 "ID" 를 설정한다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litIdLbl" runat="server"></asp:Literal></label>
                    <%-- 아이디 입력 텍스트박스
                         ID="txtUserID" : 코드비하인드에서 txtUserID.Text 로 입력된 아이디를 읽는다.
                         runat="server" → 서버 컨트롤 (C# 에서 제어 가능)
                         w-full : 부모 너비 100%
                         px-4 py-3.5 : 내부 패딩
                         border ... rounded-xl : 테두리와 둥근 모서리
                         focus:ring-2 focus:ring-blue-500 : 포커스 시 파란 링 표시
                         placeholder:text-gray-400 : placeholder 텍스트 색상 --%>
                    <asp:TextBox ID="txtUserID" runat="server" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>

                <%-- ===== 비밀번호 입력 필드 ===== --%>
                <div>
                    <%-- 비밀번호 레이블: 코드비하인드에서 litPwLbl.Text 로 "비밀번호" 또는 "Password" 를 설정한다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litPwLbl" runat="server"></asp:Literal></label>
                    <%-- 비밀번호 입력 텍스트박스
                         ID="txtPassword" : 코드비하인드에서 txtPassword.Text 로 입력값을 읽는다.
                         TextMode="Password" → 입력 내용을 ●●● 으로 감추는 비밀번호 모드
                         runat="server" → 서버 컨트롤 --%>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <%-- ===== 이름 입력 필드 ===== --%>
                <div>
                    <%-- 이름 레이블: 코드비하인드에서 litNameLbl.Text 로 "이름" 또는 "Name" 를 설정한다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litNameLbl" runat="server"></asp:Literal></label>
                    <%-- 이름 입력 텍스트박스
                         ID="txtName" : 코드비하인드에서 txtName.Text 로 입력값을 읽는다.
                         runat="server" → 서버 컨트롤 --%>
                    <asp:TextBox ID="txtName" runat="server" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <%-- ===== 닉네임 입력 필드 ===== --%>
                <div>
                    <%-- 닉네임 레이블: 코드비하인드에서 litNickLbl.Text 로 "닉네임" 또는 "Nickname" 를 설정한다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litNickLbl" runat="server"></asp:Literal></label>
                    <%-- 닉네임 입력 텍스트박스
                         ID="txtNickname" : 코드비하인드에서 txtNickname.Text 로 입력값을 읽는다.
                         runat="server" → 서버 컨트롤 --%>
                    <asp:TextBox ID="txtNickname" runat="server" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <%-- ===== 이메일 입력 필드 ===== --%>
                <div>
                    <%-- 이메일 레이블: 코드비하인드에서 litEmailLbl.Text 로 "이메일" 또는 "Email" 를 설정한다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litEmailLbl" runat="server"></asp:Literal></label>
                    <%-- 이메일 입력 텍스트박스
                         ID="txtEmail" : 코드비하인드에서 txtEmail.Text 로 입력값을 읽는다.
                         TextMode="Email" → 브라우저가 이메일 형식 유효성 검사를 도와주는 email 타입 입력창
                         runat="server" → 서버 컨트롤 --%>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full px-4 py-3.5 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all"></asp:TextBox>
                </div>

                <%-- ===== 버튼 그룹 (취소 / 가입) =====
                     pt-8 border-t : 위쪽 패딩 32px + 구분선
                     flex justify-end gap-4 : 오른쪽 정렬, 버튼 간 간격 16px --%>
                <div class="pt-8 border-t dark:border-slate-700 flex justify-end gap-4">

                    <%-- 취소 버튼 (일반 HTML 링크)
                         href="Default.aspx" : 클릭 시 메인 페이지로 이동
                         bg-gray-100 : 회색 배경의 비강조 버튼 스타일
                         litCancelBtn : 코드비하인드에서 litCancelBtn.Text 로 "취소" 또는 "Cancel" 을 설정한다. --%>
                    <a href="Default.aspx" class="px-6 py-3.5 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-200 dark:hover:bg-slate-600 transition-all"><asp:Literal ID="litCancelBtn" runat="server"></asp:Literal></a>

                    <%-- 회원가입 제출 버튼
                         ID="btnRegister" : 코드비하인드의 btnRegister_Click 이벤트와 연결된다.
                         OnClick="btnRegister_Click" : 클릭 시 서버에서 btnRegister_Click 메서드가 실행된다.
                                                       이 메서드에서 입력값 검증 및 DB 저장 처리를 수행한다.
                         runat="server" → 서버 컨트롤
                         bg-blue-600 : 파란색 강조 버튼
                         shadow-lg shadow-blue-100 : 파란색 그림자 효과 --%>
                    <asp:Button ID="btnRegister" runat="server" OnClick="btnRegister_Click" CssClass="px-10 py-3.5 bg-blue-600 dark:bg-blue-500 text-white rounded-xl font-bold hover:bg-blue-700 dark:hover:bg-blue-600 transition-all shadow-lg shadow-blue-100 dark:shadow-blue-950 cursor-pointer" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
