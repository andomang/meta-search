using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

/// <summary>
/// 설정 페이지 코드비하인드 (Settings.aspx.cs)
/// URL: Settings.aspx
/// 주요 기능:
///   - 로그인하지 않은 사용자는 메인 페이지(Default.aspx)로 리다이렉트한다.
///   - 다크 모드 토글: 쿠키·세션·DB(members.DarkMode)를 동기화한다.
///   - 언어 변경(한국어/영어): 쿠키·세션·DB(members.Language)를 동기화한다.
///   - 내 정보 수정: members 테이블의 Name, Nickname, Email 을 UPDATE 한다.
///   - 프로필 사진 업로드: ~/uploads/ 폴더에 파일을 저장하고 DB를 업데이트한다.
///   - 통계 조회: SearchHistory, SearchClickHistory 테이블에서 검색·클릭 건수를 집계한다.
///   - AJAX 처리: action 쿼리스트링으로 비동기 요청(삭제/비밀번호 변경/회원 탈퇴)을 처리한다.
/// </summary>
public partial class Settings : System.Web.UI.Page
{
    /// <summary>
    /// 페이지가 로드될 때마다 실행되는 이벤트 핸들러.
    /// 로그인 여부 확인, AJAX 요청 분기, 다크 모드/언어 동기화,
    /// 사용자 정보 로드, 통계 로드, 다국어 텍스트 바인딩을 처리한다.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // 로그인 세션이 없으면 메인 페이지로 리다이렉트 (비로그인 접근 차단)
        if (Session["UserID"] == null) { Response.Redirect("Default.aspx"); return; }

        // URL 쿼리스트링에 action 파라미터가 있으면 AJAX 요청으로 처리
        string action = Request.QueryString["action"];
        if (!string.IsNullOrEmpty(action)) { HandleAjax(action); return; }

        // DarkMode 쿠키가 있으면 세션의 IsDark 값을 쿠키 값으로 동기화
        if (Request.Cookies["DarkMode"] != null)
        {
            string dmVal = Request.Cookies["DarkMode"].Value;
            // "1" 또는 "on" 이면 다크 모드 활성 상태로 세션에 저장
            Session["IsDark"] = (dmVal == "1" || dmVal == "on");
        }

        // 최초 페이지 로드(PostBack 이 아닌 경우)에만 DB에서 데이터를 읽어옴
        if (!IsPostBack) { LoadUserData(); LoadStats(); SetLangButtons(); }

        // 현재 다크 모드 상태를 세션에서 읽어 ON/OFF 표시 텍스트 결정
        bool isDark = Session["IsDark"] != null && (bool)Session["IsDark"];
        string on = "<span class='text-blue-500 font-bold uppercase'>" + Lang.Get("set.themeOn") + "</span>";
        string off = "<span class='text-gray-400 font-bold uppercase'>" + Lang.Get("set.themeOff") + "</span>";
        // 다크 모드 상태에 따라 ON 또는 OFF 문구를 litThemeStatus 에 표시
        litThemeStatus.Text = isDark ? on : off;

        // 프로필 사진 섹션의 레이블·부제목·버튼 문구를 현재 언어에 맞게 설정
        litProfilePhotoLabel.Text = Lang.Get("set.profilePhoto");
        litProfilePhotoSub.Text   = Lang.Get("set.profilePhotoSub");
        btnUploadPhoto.Text       = Lang.Get("set.photoBtn");

        // 현재 프로필 사진 또는 닉네임 첫글자 표시
        try
        {
            // members 테이블에서 현재 로그인 사용자의 닉네임과 프로필 이미지 파일명을 조회
            SqlDataReader rp = DbMan.ExecuteReader(
                string.Format("SELECT Nickname, ProfileImg FROM members WHERE userid='{0}'", Session["UserID"]));
            if (rp.Read())
            {
                string pImg = rp["ProfileImg"] != DBNull.Value ? rp["ProfileImg"].ToString() : "";
                string nick = rp["Nickname"].ToString().Trim();

                // 프로필 이미지가 있으면 <img> 태그로, 없으면 닉네임 첫 글자(대문자)로 아바타 표시
                litCurrentAvatar.Text = !string.IsNullOrEmpty(pImg)
                    ? string.Format("<img src='uploads/{0}' class='w-full h-full object-cover' />", pImg)
                    : (nick.Length > 0 ? nick.Substring(0, 1).ToUpper() : "?");
            }
            // 리더 닫기 및 DB 연결 해제
            rp.Close(); DbMan.Close();
        }
        catch { DbMan.Close(); }

        // 설정 페이지 전반의 탭·섹션·버튼·모달 문구를 현재 언어에 맞게 바인딩
        litPageTitle.Text = Lang.Get("set.title");
        litTabGeneral.Text = Lang.Get("set.tabGeneral");
        litTabPrivacy.Text = Lang.Get("set.tabPrivacy");
        litThemeLabel.Text = Lang.Get("set.theme");
        litThemeSub.Text = Lang.Get("set.themeSub");
        btnToggleDark.Text = Lang.Get("set.themeBtn");
        litLangLabel.Text = Lang.Get("set.lang");
        litLangSub.Text = Lang.Get("set.langSub");
        litMyInfoLabel.Text = Lang.Get("set.myInfo");
        litNameLbl.Text = Lang.Get("set.name");
        litNickLbl.Text = Lang.Get("set.nick");
        litEmailLbl.Text = Lang.Get("set.email");
        litChangePwBtn.Text = Lang.Get("set.changePw");
        btnUpdate.Text = Lang.Get("set.updateBtn");
        litStatsLabel.Text = Lang.Get("set.stats");
        litTotalSearchLbl.Text = Lang.Get("set.totalSearch");
        litTotalClickLbl.Text = Lang.Get("set.totalClick");
        litTopCategoryLbl.Text = Lang.Get("set.topCategory");
        litTop5Lbl.Text = Lang.Get("set.top5");

        // 검색 기록 삭제 섹션 문구 바인딩
        litDelSearchLabel.Text = Lang.Get("set.delSearch");
        litDelSearchSub.Text = Lang.Get("set.delSearchSub");
        litDelSearchBtn.Text = Lang.Get("set.delBtn");

        // 클릭 기록 삭제 섹션 문구 바인딩
        litDelClickLabel.Text = Lang.Get("set.delClick");
        litDelClickSub.Text = Lang.Get("set.delClickSub");
        litDelClickBtn.Text = Lang.Get("set.delBtn");

        // 회원 탈퇴 섹션 문구 바인딩
        litWithdrawLabel.Text = Lang.Get("set.withdraw");
        litWithdrawSub.Text = Lang.Get("set.withdrawSub");
        litWithdrawBtn.Text = Lang.Get("set.withdrawBtn");

        // 삭제 기간 선택 모달의 안내 문구 (영어/한국어 분기)
        litDelModalDesc.Text = Lang.Get("set.cancel") == "Cancel" ? "Select period to delete." : "삭제할 기간을 선택하세요.";

        // 삭제 기간 선택 모달의 기간 옵션 문구 바인딩
        lit1Hour.Text = Lang.Get("set.delModal1h");
        lit12Hour.Text = Lang.Get("set.delModal12h");
        lit1Day.Text = Lang.Get("set.delModal1d");
        lit7Day.Text = Lang.Get("set.delModal7d");
        lit30Day.Text = Lang.Get("set.delModal30d");
        litDelAll.Text = Lang.Get("set.delModalAll");
        litDelCancel.Text = Lang.Get("set.cancel");

        // 비밀번호 변경 모달 문구 바인딩
        litPwModalTitle.Text = Lang.Get("set.pwModal");
        litPwCurLbl.Text = Lang.Get("set.pwCur");
        litPwNewLbl.Text = Lang.Get("set.pwNew");
        litPwConfirmLbl.Text = Lang.Get("set.pwConfirm");
        litPwSubmitBtn.Text = Lang.Get("set.pwBtn");
        litPwCancelBtn.Text = Lang.Get("set.cancel");

        // 회원 탈퇴 확인 모달 문구 바인딩
        litWdModalTitle.Text = Lang.Get("set.wdModal");
        litWdDesc.Text = Lang.Get("set.wdDesc");
        litWdSubmitBtn.Text = Lang.Get("set.wdBtn");
        litWdCancelBtn.Text = Lang.Get("set.cancel");
    }

    /// <summary>
    /// URL 쿼리스트링 action 값에 따라 AJAX 요청을 처리하는 메서드.
    /// 응답은 JSON 형식으로 반환한다.
    /// 처리 가능한 action:
    ///   - deleteHistory: SearchHistory 또는 SearchClickHistory 에서 기간별 레코드 삭제
    ///   - changePw:      현재 비밀번호 확인 후 members 테이블의 passwd 를 UPDATE
    ///   - withdraw:      비밀번호 확인 후 회원 관련 데이터 전체 삭제 및 세션 초기화
    /// </summary>
    private void HandleAjax(string action)
    {
        // 응답 Content-Type 을 JSON 으로 설정
        Response.ContentType = "application/json";
        string userID = Session["UserID"].ToString();
        try
        {
            if (action == "deleteHistory")
            {
                // 삭제 대상 테이블(검색 기록 or 클릭 기록)과 기간 파라미터를 쿼리스트링에서 가져옴
                string target = Request.QueryString["target"];
                string range = Request.QueryString["range"];

                // target 값에 따라 삭제할 테이블과 날짜 컬럼명을 결정
                string table = target == "click" ? "SearchClickHistory" : "SearchHistory";
                string timeCol = target == "click" ? "ClickTime" : "SearchTime";

                // 기간 문자열을 SQL DATEADD 조건식으로 변환
                string condition = GetDateCondition(range);

                // 기간 조건이 있으면 조건부 DELETE, 없으면 전체 DELETE
                string sql = condition != null
                    ? string.Format("DELETE FROM {0} WHERE UserID='{1}' AND {2} >= {3}", table, userID, timeCol, condition)
                    : string.Format("DELETE FROM {0} WHERE UserID='{1}'", table, userID);

                // DELETE 쿼리 실행 후 DB 연결 해제
                DbMan.ExecuteNonQuery(sql); DbMan.Close();

                // 성공 JSON 응답 반환
                Response.Write("{\"result\":\"ok\"}");
            }
            else if (action == "changePw")
            {
                // 쿼리스트링에서 현재 비밀번호와 새 비밀번호를 가져옴
                string cur = Request.QueryString["cur"];
                string nw = Request.QueryString["nw"];

                // 두 비밀번호를 MD5 해시로 암호화
                string encCur = GetMD5(cur);
                string encNew = GetMD5(nw);

                // members 테이블에서 현재 아이디와 현재 비밀번호(해시)가 일치하는지 확인
                SqlDataReader r = DbMan.ExecuteReader(string.Format(
                    "SELECT COUNT(*) FROM members WHERE userid='{0}' AND passwd='{1}'", userID, encCur));
                bool valid = r.Read() && Convert.ToInt32(r[0]) > 0;
                r.Close(); DbMan.Close();

                if (!valid)
                {
                    // 현재 비밀번호가 틀린 경우 "wrong" 결과 반환
                    Response.Write("{\"result\":\"wrong\"}");
                }
                else
                {
                    // 현재 비밀번호 확인 성공 시 members 테이블의 passwd 를 새 해시값으로 UPDATE
                    DbMan.ExecuteNonQuery(string.Format(
                        "UPDATE members SET passwd='{0}' WHERE userid='{1}'", encNew, userID));
                    DbMan.Close();

                    // 성공 JSON 응답 반환
                    Response.Write("{\"result\":\"ok\"}");
                }
            }
            else if (action == "withdraw")
            {
                // 쿼리스트링에서 탈퇴 확인용 비밀번호를 가져와 MD5 해시로 암호화
                string pw = Request.QueryString["pw"];
                string encPw = GetMD5(pw);

                // members 테이블에서 아이디와 비밀번호가 일치하는지 확인
                SqlDataReader r = DbMan.ExecuteReader(string.Format(
                    "SELECT COUNT(*) FROM members WHERE userid='{0}' AND passwd='{1}'", userID, encPw));
                bool valid = r.Read() && Convert.ToInt32(r[0]) > 0;
                r.Close(); DbMan.Close();

                if (!valid)
                {
                    // 비밀번호가 틀린 경우 "wrong" 결과 반환
                    Response.Write("{\"result\":\"wrong\"}");
                }
                else
                {
                    // 비밀번호 확인 성공 시 해당 사용자의 모든 데이터를 순서대로 삭제

                    // SearchHistory 테이블에서 해당 사용자의 검색 기록 삭제
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM SearchHistory WHERE UserID='{0}'", userID)); DbMan.Close();

                    // SearchClickHistory 테이블에서 해당 사용자의 클릭 기록 삭제
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM SearchClickHistory WHERE UserID='{0}'", userID)); DbMan.Close();

                    // Bbs 테이블에서 해당 사용자가 작성한 게시글 삭제
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM Bbs WHERE Author='{0}'", userID)); DbMan.Close();

                    // members 테이블에서 회원 레코드 자체를 삭제
                    DbMan.ExecuteNonQuery(string.Format("DELETE FROM members WHERE userid='{0}'", userID)); DbMan.Close();

                    // 서버 세션 초기화 (로그아웃 처리)
                    Session.Clear();

                    // 성공 JSON 응답 반환
                    Response.Write("{\"result\":\"ok\"}");
                }
            }
        }
        catch { DbMan.Close(); Response.Write("{\"result\":\"error\"}"); }

        // 응답 전송 완료 후 요청 처리 종료
        Response.End();
    }

    /// <summary>
    /// 삭제 기간 문자열(range)을 SQL Server 의 DATEADD 조건식으로 변환하는 메서드.
    /// "all" 또는 알 수 없는 값이면 null 을 반환하여 전체 삭제로 처리된다.
    /// </summary>
    /// <param name="range">삭제 기간 키 (예: "1hour", "12hour", "1day", "7day", "30day")</param>
    /// <returns>SQL DATEADD 함수 문자열 또는 null (전체 삭제)</returns>
    private string GetDateCondition(string range)
    {
        switch (range)
        {
            case "1hour":  return "DATEADD(HOUR, -1, GETDATE())";   // 최근 1시간 이내
            case "12hour": return "DATEADD(HOUR, -12, GETDATE())";  // 최근 12시간 이내
            case "1day":   return "DATEADD(DAY, -1, GETDATE())";    // 최근 1일 이내
            case "7day":   return "DATEADD(DAY, -7, GETDATE())";    // 최근 7일 이내
            case "30day":  return "DATEADD(DAY, -30, GETDATE())";   // 최근 30일 이내
            default:       return null;                              // 전체 삭제 (기간 제한 없음)
        }
    }

    /// <summary>
    /// members 테이블에서 현재 로그인 사용자의 기본 정보를 조회하여 폼에 채우는 메서드.
    /// 조회 항목: Name(이름), Nickname(닉네임), Email(이메일), Language(언어 설정)
    /// IsPostBack 이 false 일 때만 호출된다.
    /// </summary>
    private void LoadUserData()
    {
        try
        {
            // members 테이블에서 현재 로그인 사용자의 정보를 SELECT
            string sql = string.Format("SELECT Name, Nickname, Email, Language FROM members WHERE UserID='{0}'", Session["UserID"]);
            using (SqlDataReader reader = DbMan.ExecuteReader(sql))
            {
                if (reader.Read())
                {
                    // 조회된 값을 각 편집 폼 컨트롤에 바인딩
                    editName.Text = reader["Name"].ToString().Trim();
                    editNickname.Text = reader["Nickname"].ToString().Trim();
                    editEmail.Text = reader["Email"].ToString().Trim();

                    // Language 값이 NULL 이면 기본값 "ko"(한국어)로 세션에 저장
                    Session["Lang"] = reader["Language"] != DBNull.Value ? reader["Language"].ToString().Trim() : "ko";
                }
                DbMan.Close();
            }
        }
        catch { DbMan.Close(); }
    }

    /// <summary>
    /// 현재 로그인 사용자의 활동 통계를 DB에서 조회하여 화면에 표시하는 메서드.
    /// 조회 항목:
    ///   - SearchHistory: 총 검색 횟수
    ///   - SearchClickHistory: 총 클릭 횟수
    ///   - SearchHistory: 가장 많이 검색한 카테고리 (TOP 1)
    ///   - SearchHistory: 검색 키워드 TOP 5 (Repeater 바인딩)
    /// IsPostBack 이 false 일 때만 호출된다.
    /// </summary>
    private void LoadStats()
    {
        try
        {
            string uid = Session["UserID"].ToString();

            // SearchHistory 테이블에서 총 검색 횟수 조회
            SqlDataReader r1 = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM SearchHistory WHERE UserID='{0}'", uid));
            litTotalSearch.Text = r1.Read() ? r1[0].ToString() : "0"; r1.Close(); DbMan.Close();

            // SearchClickHistory 테이블에서 총 클릭 횟수 조회
            SqlDataReader r2 = DbMan.ExecuteReader(string.Format("SELECT COUNT(*) FROM SearchClickHistory WHERE UserID='{0}'", uid));
            litTotalClick.Text = r2.Read() ? r2[0].ToString() : "0"; r2.Close(); DbMan.Close();

            // SearchHistory 테이블에서 가장 많이 검색한 카테고리를 TOP 1 으로 조회
            SqlDataReader r3 = DbMan.ExecuteReader(string.Format(
                "SELECT TOP 1 Category FROM SearchHistory WHERE UserID='{0}' GROUP BY Category ORDER BY COUNT(*) DESC", uid));
            litTopCategory.Text = r3.Read() ? r3[0].ToString() : "-"; r3.Close(); DbMan.Close();

            // SearchHistory 테이블에서 검색어별 검색 횟수 TOP 5 를 DataSet 으로 조회하여 Repeater 에 바인딩
            DataSet ds = DbMan.DataAdapterFill(string.Format(
                "SELECT TOP 5 Query, COUNT(*) AS SearchCount FROM SearchHistory WHERE UserID='{0}' GROUP BY Query ORDER BY SearchCount DESC", uid), "Top");
            rptTopKeywords.DataSource = ds.Tables[0];
            rptTopKeywords.DataBind();
        }
        catch { DbMan.Close(); }
    }

    /// <summary>
    /// 언어 선택 버튼(한국어/영어)의 CSS 클래스를 현재 언어에 따라 활성/비활성으로 설정하는 메서드.
    /// 현재 선택된 언어 버튼은 파란색 강조 스타일, 나머지는 기본 스타일이 적용된다.
    /// IsPostBack 이 false 일 때만 호출된다.
    /// </summary>
    private void SetLangButtons()
    {
        // 세션에서 현재 언어 값을 읽어옴 (없으면 기본값 "ko")
        string lang = Session["Lang"] != null ? Session["Lang"].ToString() : "ko";

        // 활성 버튼 CSS 클래스 (파란색 배경, 흰색 텍스트)
        string activeClass = "px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer bg-blue-600 text-white border-blue-600";

        // 비활성 버튼 CSS 클래스 (다크 모드 대응 포함)
        string inactClass = "px-5 py-2 rounded-xl font-bold text-sm border transition-all cursor-pointer dark:bg-slate-700 dark:text-white dark:border-slate-600 border-gray-300 text-gray-700 hover:border-blue-400";

        // 현재 언어에 맞는 버튼에 활성 클래스, 나머지에 비활성 클래스를 적용
        btnLangKo.CssClass = lang == "ko" ? activeClass : inactClass;
        btnLangEn.CssClass = lang == "en" ? activeClass : inactClass;
    }

    /// <summary>
    /// 한국어 언어 선택 버튼(btnLangKo)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 언어를 "ko"(한국어)로 변경한다.
    /// </summary>
    protected void btnLangKo_Click(object sender, EventArgs e) { UpdateLang("ko"); }

    /// <summary>
    /// 영어 언어 선택 버튼(btnLangEn)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 언어를 "en"(영어)로 변경한다.
    /// </summary>
    protected void btnLangEn_Click(object sender, EventArgs e) { UpdateLang("en"); }

    /// <summary>
    /// 선택된 언어 값을 DB, 세션, 쿠키에 모두 저장하고 페이지를 새로고침하는 메서드.
    /// members 테이블의 Language 컬럼을 UPDATE 한다.
    /// Language 쿠키 만료 기간은 1년으로 설정된다.
    /// </summary>
    /// <param name="lang">변경할 언어 코드 ("ko" 또는 "en")</param>
    private void UpdateLang(string lang)
    {
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                // members 테이블의 Language 컬럼을 선택한 언어 코드로 UPDATE
                SqlCommand cmd = new SqlCommand("UPDATE members SET [Language]=@lang WHERE userid=@id", conn);
                cmd.Parameters.AddWithValue("@lang", lang);
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery(); DbMan.Close();
            }
        }
        catch { DbMan.Close(); }

        // 세션의 언어 값을 새 언어로 업데이트
        Session["Lang"] = lang;

        // Language 쿠키를 새 언어 값으로 설정하고 만료 기간을 1년으로 지정
        Response.Cookies["Language"].Value = lang;
        Response.Cookies["Language"].Expires = DateTime.Now.AddYears(1);

        // 언어 변경 사항이 즉시 반영되도록 설정 페이지를 새로고침
        Response.Redirect("Settings.aspx");
    }

    /// <summary>
    /// 다크 모드 토글 버튼(btnToggleDark)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 현재 다크 모드 상태를 반전시키고 DB, 세션, 쿠키에 동기화한 뒤 페이지를 새로고침한다.
    /// members 테이블의 DarkMode 컬럼(1=다크, 0=라이트)을 UPDATE 한다.
    /// DarkMode 쿠키 만료 기간은 1년으로 설정된다.
    /// </summary>
    protected void btnToggleDark_Click(object sender, EventArgs e)
    {
        // 현재 다크 모드 상태를 반전 (true → false, false → true)
        bool next = !(Session["IsDark"] != null && (bool)Session["IsDark"]);
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                // members 테이블의 DarkMode 컬럼을 새 상태값(1 또는 0)으로 UPDATE
                SqlCommand cmd = new SqlCommand("UPDATE members SET DarkMode=@dark WHERE UserID=@id", conn);
                cmd.Parameters.AddWithValue("@dark", next ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery();
            }
        }
        catch { DbMan.Close(); }

        // 세션의 IsDark 값을 새 상태로 업데이트
        Session["IsDark"] = next;

        // DarkMode 쿠키를 새 상태 값("1" 또는 "0")으로 설정하고 만료 기간을 1년으로 지정
        Response.Cookies["DarkMode"].Value = next ? "1" : "0";
        Response.Cookies["DarkMode"].Expires = DateTime.Now.AddYears(1);

        // 테마 변경 사항이 즉시 반영되도록 설정 페이지를 새로고침
        Response.Redirect("Settings.aspx");
    }

    /// <summary>
    /// 내 정보 업데이트 버튼(btnUpdate)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// members 테이블의 Name(이름), Nickname(닉네임), Email(이메일)을 UPDATE 한다.
    /// 성공 시 세션의 UserName 도 새 닉네임으로 갱신하고 설정 페이지를 새로고침한다.
    /// </summary>
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                // members 테이블의 Name, Nickname, Email 컬럼을 입력값으로 UPDATE
                SqlCommand cmd = new SqlCommand("UPDATE members SET Name=@name, Nickname=@nick, Email=@email WHERE UserID=@id", conn);
                cmd.Parameters.AddWithValue("@name", editName.Text.Trim());
                cmd.Parameters.AddWithValue("@nick", editNickname.Text.Trim());
                cmd.Parameters.AddWithValue("@email", editEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@id", Session["UserID"].ToString());
                cmd.ExecuteNonQuery(); DbMan.Close();

                // 세션의 UserName 을 새 닉네임으로 갱신 (내비게이션 표시 이름 즉시 반영)
                Session["UserName"] = editNickname.Text.Trim();

                // 현재 언어에 맞는 성공 메시지 결정 (영어/한국어 분기)
                string msg = Lang.Get("set.cancel") == "Cancel" ? "Updated successfully." : "정보가 업데이트되었습니다.";

                // 성공 메시지 alert 후 설정 페이지를 새로고침
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ok",
                    string.Format("alert('{0}'); location.href='Settings.aspx';", msg), true);
            }
        }
        catch { DbMan.Close(); }
    }

    /// <summary>
    /// 프로필 사진 업로드 버튼(btnUploadPhoto)을 클릭했을 때 호출되는 이벤트 핸들러.
    /// 선택한 이미지 파일(.jpg/.jpeg/.png)을 ~/uploads/ 폴더에 저장하고
    /// members 테이블의 ProfileImg 컬럼을 저장된 파일명으로 UPDATE 한다.
    /// 파일이 없거나 허용되지 않는 확장자면 오류 메시지를 표시한다.
    /// </summary>
    protected void btnUploadPhoto_Click(object sender, EventArgs e)
    {
        // 파일이 선택되지 않은 경우 처리를 중단
        if (!fuPhoto.HasFile) return;

        // 업로드된 파일의 확장자를 소문자로 추출
        string ext = System.IO.Path.GetExtension(fuPhoto.FileName).ToLower();

        // .jpg, .jpeg, .png 이외의 파일 형식은 허용하지 않음
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "photoErr",
                string.Format("alert('{0}');", Lang.Get("set.photoError")), true);
            return;
        }

        // 저장 파일명을 "사용자아이디 + 확장자" 형식으로 결정 (예: user01.jpg)
        string uid      = Session["UserID"].ToString();
        string fileName = uid.Trim() + ext;

        // 서버의 ~/uploads/ 폴더 실제 경로를 계산
        string savePath = Server.MapPath("~/uploads/") + fileName;

        // ~/uploads/ 폴더가 없으면 새로 생성
        if (!System.IO.Directory.Exists(Server.MapPath("~/uploads/")))
            System.IO.Directory.CreateDirectory(Server.MapPath("~/uploads/"));

        // 업로드된 파일을 지정된 경로에 저장
        fuPhoto.SaveAs(savePath);

        try
        {
            using (SqlConnection conn = DbMan.Open())
            {
                // members 테이블의 ProfileImg 컬럼을 새 파일명으로 UPDATE
                SqlCommand cmd = new SqlCommand("UPDATE members SET ProfileImg=@img WHERE userid=@id", conn);
                cmd.Parameters.AddWithValue("@img", fileName);
                cmd.Parameters.AddWithValue("@id",  uid);
                cmd.ExecuteNonQuery(); DbMan.Close();
            }
        }
        catch { DbMan.Close(); }

        // 사진 업데이트 성공 메시지 alert 후 설정 페이지를 새로고침
        ScriptManager.RegisterStartupScript(this, GetType(), "photoOk",
            string.Format("alert('{0}'); location.href='Settings.aspx';", Lang.Get("set.photoUpdated")), true);
    }

    /// <summary>
    /// 입력된 문자열을 MD5 해시값(16진수 대문자 문자열)으로 변환하는 유틸리티 메서드.
    /// 비밀번호 확인(changePw, withdraw) 처리 시 평문 비밀번호를 암호화하는 데 사용된다.
    /// </summary>
    /// <param name="input">해시 처리할 평문 문자열 (예: 비밀번호)</param>
    /// <returns>MD5 해시값 (32자리 16진수 대문자 문자열)</returns>
    private string GetMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            // 입력 문자열을 ASCII 바이트 배열로 변환한 뒤 MD5 해시 계산
            byte[] b = md5.ComputeHash(Encoding.ASCII.GetBytes(input));

            // 바이트 배열을 16진수 대문자 문자열로 변환하여 반환
            StringBuilder sb = new StringBuilder();
            foreach (byte x in b) sb.Append(x.ToString("X2"));
            return sb.ToString();
        }
    }
}
