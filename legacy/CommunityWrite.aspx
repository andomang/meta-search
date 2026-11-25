<%@ Page Title="Write" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CommunityWrite.aspx.cs" Inherits="CommunityWrite" %>
<%--
    [CommunityWrite.aspx]
    커뮤니티 게시글 새 글 작성 페이지입니다.
    코드비하인드: CommunityWrite.aspx.cs / Inherits="CommunityWrite"
    - 제목(txtTitle), 본문(txtContent), 첨부파일(fileUpload) 입력 폼을 제공합니다.
    - 저장 버튼(btnSave) 클릭 시 btnSave_Click 이벤트 핸들러가 실행되어
      DB에 게시글을 INSERT하고 CommunityView.aspx 로 리디렉션합니다.
    - 취소 링크를 클릭하면 Community.aspx (목록 페이지)로 돌아갑니다.
    - 다국어 처리를 위해 레이블 텍스트를 asp:Literal로 서버에서 주입합니다.
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto px-6 py-12">
        <div class="bg-white dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-3xl p-8 shadow-sm">

            <%-- ===== 페이지 제목 ===== --%>
            <%-- litPageTitle: "새 글 작성" 등 페이지 제목 텍스트. 코드비하인드에서 다국어 값을 주입합니다. --%>
            <h2 class="text-2xl font-bold mb-8 text-gray-900 dark:text-white"><asp:Literal ID="litPageTitle" runat="server"></asp:Literal></h2>

            <div class="space-y-6">

                <%-- ===== 제목 입력 필드 ===== --%>
                <div>
                    <%-- litTitleLbl: 제목 입력 필드의 레이블 텍스트 (예: "제목"). 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litTitleLbl" runat="server"></asp:Literal></label>
                    <%--
                        txtTitle: 게시글 제목 입력 텍스트 박스.
                        코드비하인드에서 txtTitle.Text 로 입력된 제목을 읽어 DB에 저장합니다.
                        단일 줄(TextMode 기본값: SingleLine) 입력 필드입니다.
                    --%>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="w-full px-4 py-3 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>

                <%-- ===== 본문 입력 필드 ===== --%>
                <div>
                    <%-- litContentLbl: 본문 입력 필드의 레이블 텍스트 (예: "내용"). 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litContentLbl" runat="server"></asp:Literal></label>
                    <%--
                        txtContent: 게시글 본문 입력 텍스트 박스.
                        TextMode="MultiLine": 여러 줄 입력 가능한 textarea로 렌더링됩니다.
                        Rows="12": 기본으로 12줄 높이를 표시합니다.
                        코드비하인드에서 txtContent.Text 로 입력된 본문을 읽어 DB에 저장합니다.
                    --%>
                    <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="12" CssClass="w-full px-4 py-3 border border-gray-200 dark:border-slate-600 rounded-xl outline-none focus:ring-2 focus:ring-blue-500 resize-none bg-white dark:bg-slate-700 dark:text-white transition-all placeholder:text-gray-400 dark:placeholder:text-gray-500"></asp:TextBox>
                </div>

                <%-- ===== 첨부파일 업로드 필드 ===== --%>
                <div>
                    <%-- litFileLbl: 파일 업로드 필드의 레이블 텍스트 (예: "첨부파일"). 코드비하인드에서 다국어 값을 주입합니다. --%>
                    <label class="block text-sm font-semibold mb-2 ml-1 text-gray-700 dark:text-gray-300"><asp:Literal ID="litFileLbl" runat="server"></asp:Literal></label>
                    <div class="relative group">
                        <%--
                            fileUpload: 파일 선택 컨트롤 (asp:FileUpload).
                            HTML의 <input type="file"> 로 렌더링됩니다.
                            코드비하인드에서 fileUpload.HasFile 로 파일 첨부 여부를 확인하고,
                            fileUpload.SaveAs(경로) 로 서버에 파일을 저장합니다.
                            파일 이름은 fileUpload.FileName 으로 읽습니다.
                            파일 크기(바이트)는 fileUpload.PostedFile.ContentLength 로 읽습니다.
                        --%>
                        <asp:FileUpload ID="fileUpload" runat="server" CssClass="w-full px-4 py-3 border-2 border-dashed border-gray-200 dark:border-slate-600 rounded-xl hover:border-blue-400 dark:hover:border-blue-500 transition-all cursor-pointer bg-gray-50 dark:bg-slate-700 text-sm text-gray-500 dark:text-gray-400 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 dark:file:bg-blue-950 file:text-blue-700 dark:file:text-blue-300 hover:file:bg-blue-100 dark:hover:file:bg-blue-900" />
                    </div>
                </div>

                <%-- ===== 하단 액션 버튼 (취소 / 저장) ===== --%>
                <div class="flex justify-end gap-4 pt-6 border-t dark:border-slate-700">
                    <%--
                        취소 링크: Community.aspx (게시글 목록 페이지)로 이동합니다.
                        litCancelBtn: "취소" 버튼 텍스트. 코드비하인드에서 다국어 값을 주입합니다.
                    --%>
                    <a href="Community.aspx" class="px-6 py-3 bg-gray-100 dark:bg-slate-700 text-gray-600 dark:text-gray-300 rounded-xl font-bold hover:bg-gray-200 dark:hover:bg-slate-600 transition-all text-center"><asp:Literal ID="litCancelBtn" runat="server"></asp:Literal></a>
                    <%--
                        btnSave: 게시글 저장(등록) 버튼.
                        OnClick="btnSave_Click" → 코드비하인드의 btnSave_Click 이벤트 핸들러를 호출합니다.
                        핸들러에서 txtTitle.Text, txtContent.Text, fileUpload 값을 읽어
                        DB에 게시글을 INSERT하고, 완료 후 CommunityView.aspx?no=N 으로 리디렉션합니다.
                        버튼 텍스트(예: "저장" 또는 "등록")는 코드비하인드에서 Text 속성에 설정합니다.
                    --%>
                    <asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click" CssClass="px-10 py-3 bg-blue-600 dark:bg-blue-500 text-white rounded-xl font-bold hover:bg-blue-700 dark:hover:bg-blue-600 transition-all shadow-lg shadow-blue-100 dark:shadow-blue-950 cursor-pointer" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
