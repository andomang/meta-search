<%@ Page Title="Edit Post" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CommunityEdit.aspx.cs" Inherits="CommunityEdit" %>
<%--
    [CommunityEdit.aspx]
    커뮤니티 게시글 수정 페이지입니다.
    코드비하인드: CommunityEdit.aspx.cs / Inherits="CommunityEdit"
    - URL 쿼리스트링 ?no=N 으로 수정할 게시글 번호를 받습니다.
    - 페이지 로드(Page_Load)에서 해당 게시글의 기존 제목/본문을 DB에서 읽어
      txtTitle.Text, txtContent.Text 에 채워 넣어 수정 전 내용이 표시되게 합니다.
    - 저장 버튼(btnSave) 클릭 시 btnSave_Click 이벤트 핸들러가 실행되어
      DB의 게시글을 UPDATE하고 CommunityView.aspx?no=N 으로 리디렉션합니다.
    - 취소 링크를 클릭하면 Community.aspx (목록 페이지)로 돌아갑니다.
    - 현재 로그인 사용자가 게시글 작성자가 아니면 접근이 차단됩니다(코드비하인드에서 처리).
    - 파일 업로드 필드는 이 페이지에 없습니다 (수정 시 파일 변경 불가 또는 별도 처리).
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl p-8 shadow-sm">

            <%-- ===== 페이지 제목 ===== --%>
            <%-- litPageTitle: "게시글 수정" 등 페이지 제목 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
            <h2 class="text-2xl font-bold mb-8 text-gray-900 dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h2>

            <div class="space-y-6">

                <%-- ===== 제목 수정 필드 ===== --%>
                <div>
                    <%-- litTitleLbl: 제목 입력 필드의 레이블 텍스트 (예: "제목"). 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litTitleLbl" runat="server"></asp:Literal></label>
                    <%--
                        txtTitle: 게시글 제목 수정 텍스트 박스.
                        페이지 최초 로드(IsPostBack == false)일 때 코드비하인드에서
                        txtTitle.Text = 기존 제목 값으로 미리 채워 넣어줍니다.
                        저장 시 코드비하인드에서 txtTitle.Text 를 읽어 DB를 UPDATE합니다.
                    --%>
                    <asp:TextBox ID="txtTitle" runat="server"
                        CssClass="w-full px-4 py-3 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>

                <%-- ===== 본문 수정 필드 ===== --%>
                <div>
                    <%-- litContentLbl: 본문 입력 필드의 레이블 텍스트 (예: "내용"). 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litContentLbl" runat="server"></asp:Literal></label>
                    <%--
                        txtContent: 게시글 본문 수정 텍스트 박스.
                        TextMode="MultiLine": 여러 줄 입력 가능한 textarea로 렌더링됩니다.
                        Rows="12": 기본으로 12줄 높이를 표시합니다.
                        페이지 최초 로드 시 코드비하인드에서 txtContent.Text = 기존 본문 값으로
                        미리 채워 넣어 기존 내용을 보여주고 사용자가 이어서 수정할 수 있게 합니다.
                        저장 시 코드비하인드에서 txtContent.Text 를 읽어 DB를 UPDATE합니다.
                    --%>
                    <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="12"
                        CssClass="w-full px-4 py-3 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 resize-none bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>

                <%-- ===== 하단 액션 버튼 (취소 / 저장) ===== --%>
                <div class="flex justify-end gap-4 pt-6 border-t dark:border-slate-700">
                    <%--
                        취소 링크: Community.aspx (게시글 목록 페이지)로 이동합니다.
                        litCancelBtn: "취소" 버튼 텍스트. 코드비하인드에서 다국어 값을 주입합니다.
                    --%>
                    <a href="Community.aspx"
                        class="px-6 py-3 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-200 dark:hover:bg-slate-600 transition-all text-center">
                        <asp:Literal ID="litCancelBtn" runat="server"></asp:Literal>
                    </a>
                    <%--
                        btnSave: 수정 내용 저장 버튼.
                        OnClick="btnSave_Click" → 코드비하인드의 btnSave_Click 이벤트 핸들러를 호출합니다.
                        핸들러에서 txtTitle.Text, txtContent.Text 를 읽어 DB의 해당 게시글 레코드를
                        UPDATE하고, 완료 후 CommunityView.aspx?no=N 으로 리디렉션합니다.
                        버튼 텍스트(예: "저장")는 코드비하인드에서 Text 속성에 설정합니다.
                    --%>
                    <asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click"
                        CssClass="px-10 py-3 bg-blue-600 dark:bg-blue-500 text-white rounded-xl font-bold hover:bg-blue-700 dark:hover:bg-blue-600 transition-all shadow-lg shadow-blue-100 dark:shadow-blue-950 cursor-pointer" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
