using System.Web;

/// <summary>
/// 다국어(한국어/영어) 텍스트를 제공하는 유틸리티(Utility) 클래스.
/// [유틸 레이어] - 화면에 표시되는 모든 UI 텍스트를 한 곳에서 관리한다.
///
/// 동작 원리:
///   - Session["Lang"] 값이 "en"이면 영어 텍스트를 반환하고,
///     "ko"이거나 세션이 없으면 한국어 텍스트를 반환한다.
///   - Lang.Get("키") 형태로 호출하면 현재 언어에 맞는 텍스트가 반환된다.
///
/// 사용 예:
///   litTitle.Text = Lang.Get("nav.community");  // "커뮤니티" 또는 "Community"
///
/// 관리되는 페이지:
///   Site.Master, Default.aspx, SearchResults.aspx, Community.aspx,
///   CommunityView.aspx, CommunityEdit.aspx, CommunityWrite.aspx,
///   Register.aspx, MyPage.aspx, Settings.aspx
///
/// 모든 멤버가 static이므로 new Lang() 없이 Lang.Get("key") 방식으로 바로 사용한다.
/// </summary>
public static class Lang
{
    /// <summary>
    /// 현재 HTTP 세션(Session)의 언어 설정이 영어("en")인지 확인하는 내부 프로퍼티.
    /// Get() 메서드에서 한국어/영어 분기를 결정하는 데 사용된다.
    /// </summary>
    /// <returns>Session["Lang"]이 "en"이면 true, 그 외(없거나 "ko")면 false</returns>
    private static bool IsEn
    {
        get
        {
            // 현재 요청(Request)의 세션 객체 가져오기
            var session = HttpContext.Current.Session;
            // 세션 또는 Lang 키가 없으면 기본값 한국어(false)
            // Lang 값이 정확히 "en"일 때만 true 반환
            return session != null && session["Lang"] != null && session["Lang"].ToString() == "en";
        }
    }

    /// <summary>
    /// 주어진 키(key)에 해당하는 현재 언어의 UI 텍스트를 반환한다.
    /// 세션의 Lang 값이 "en"이면 영어를, 그 외에는 한국어를 반환한다.
    /// 등록되지 않은 키를 요청하면 키 문자열 자체를 반환하여 디버깅을 돕는다.
    /// </summary>
    /// <param name="key">
    /// 텍스트 키 문자열. 형식: "페이지약어.항목명"
    /// 예: "nav.community", "login.btn", "reg.id"
    /// </param>
    /// <returns>현재 언어 설정에 맞는 텍스트 문자열</returns>
    public static string Get(string key)
    {
        // IsEn 프로퍼티로 현재 언어가 영어인지 확인 (true = 영어)
        bool en = IsEn;
        switch (key)
        {
            // =====================
            // Site.Master - 상단 내비게이션 바 텍스트
            // =====================
            case "nav.community":   return en ? "Community"   : "커뮤니티";      // 게시판 링크
            case "nav.settings":    return en ? "Settings"    : "설정";          // 설정 페이지 링크
            case "nav.mypage":      return en ? "My Page"     : "마이페이지";    // 마이페이지 링크
            case "nav.login":       return en ? "Login"       : "로그인";        // 로그인 버튼
            case "nav.logout":      return en ? "Logout"      : "로그아웃";      // 로그아웃 버튼
            case "nav.greeting":    return en ? ""            : "님";            // "OOO 님" 뒤에 붙는 호칭 (영어는 공백)

            // 로그인 모달 팝업 내 텍스트
            case "login.title":     return en ? "Login"       : "로그인";        // 모달 제목
            case "login.id":        return en ? "Username"    : "아이디";        // 아이디 입력 레이블
            case "login.pw":        return en ? "Password"    : "비밀번호";      // 비밀번호 입력 레이블
            case "login.btn":       return en ? "Sign In"     : "로그인";        // 로그인 버튼 텍스트
            case "login.noAccount": return en ? "Don't have an account?" : "계정이 없으신가요?"; // 회원가입 안내
            case "login.register":  return en ? "Sign up"     : "회원가입";      // 회원가입 링크
            case "login.close":     return en ? "Close"       : "닫기";          // 모달 닫기 버튼

            // =====================
            // Default.aspx - 메인(홈) 페이지 텍스트
            // =====================
            case "home.subtitle":   return en ? "Search everything"       : "모든 정보를 검색하세요";    // 검색창 아래 부제
            case "home.community":  return en ? "Community"               : "자유게시판";                // 커뮤니티 카드 제목
            case "home.commSub":    return en ? "Share and discuss"       : "함께 이야기 나누는 공간";   // 커뮤니티 카드 설명
            case "home.settings":   return en ? "Settings"                : "개인 설정";                 // 설정 카드 제목
            case "home.settSub":    return en ? "Dark mode & preferences" : "다크모드 및 환경 설정";     // 설정 카드 설명

            // =====================
            // SearchResults.aspx - 검색 결과 페이지 텍스트
            // =====================
            case "search.results":      return en ? "Search results"      : "검색 결과 리스트입니다.";   // 결과 목록 제목
            case "search.related":      return en ? "Related searches"    : "관련 검색어";               // 관련 검색어 섹션 제목
            case "search.popular":      return en ? "Popular searches"    : "인기 검색어";               // 인기 검색어 섹션 제목
            case "search.recent":       return en ? "Recent searches"     : "최근 검색어";               // 최근 검색어 섹션 제목
            case "search.noHistory":    return en ? "No search history."  : "검색 기록이 없습니다.";     // 기록 없을 때
            case "search.placeholder":  return en ? "Search anything..."  : "검색어를 입력하세요";       // 검색 입력창 placeholder
            case "search.noResults":    return en ? "No results found."   : "검색 결과가 없습니다.";     // 결과 없을 때 메인 문구
            case "search.noResultsSub": return en ? "Try a different keyword." : "다른 검색어를 입력해 보세요."; // 결과 없을 때 보조 문구
            case "search.loading":      return en ? "Searching..."        : "검색 중...";                // 로딩 중 텍스트
            case "search.times":        return en ? "times"               : "회";                        // "5회 검색" 처럼 단위 뒤에 붙는 텍스트

            // =====================
            // Community.aspx - 게시판 목록 페이지 텍스트
            // =====================
            case "comm.title":      return en ? "Community"   : "커뮤니티 광장";   // 페이지 제목
            case "comm.write":      return en ? "Write"       : "글쓰기";          // 글쓰기 버튼
            case "comm.no":         return en ? "No."         : "번호";            // 게시글 번호 컬럼 헤더
            case "comm.titleCol":   return en ? "Title"       : "제목";            // 제목 컬럼 헤더
            case "comm.author":     return en ? "Author"      : "작성자";          // 작성자 컬럼 헤더
            case "comm.date":       return en ? "Date"        : "날짜";            // 날짜 컬럼 헤더
            case "comm.prev":       return en ? "Prev"        : "이전";            // 페이지네이션 이전 버튼
            case "comm.next":       return en ? "Next"        : "다음";            // 페이지네이션 다음 버튼
            case "comm.empty":      return en ? "No posts yet." : "게시글이 없습니다."; // 게시글 없을 때

            // =====================
            // CommunityView.aspx - 게시글 상세 보기 페이지 텍스트
            // =====================
            case "view.board":      return en ? "Free Board"   : "자유게시판";     // 게시판 이름 표시
            case "view.download":   return en ? "Download"     : "다운로드";       // 파일 다운로드 버튼
            case "view.back":       return en ? "Back to list" : "목록으로";       // 목록 돌아가기 버튼
            case "view.delete":     return en ? "Delete"       : "삭제";           // 게시글 삭제 버튼
            case "view.edit":       return en ? "Edit"         : "수정";           // 게시글 수정 버튼
            case "view.confirmDel": return en ? "Are you sure you want to delete this post?" : "정말 삭제하시겠습니까?"; // 삭제 확인 메시지

            // =====================
            // CommunityEdit.aspx - 게시글 수정 페이지 텍스트
            // =====================
            case "edit.title":      return en ? "Edit Post"    : "게시글 수정";    // 페이지 제목
            case "edit.titleLbl":   return en ? "Title"        : "제목";           // 제목 입력 레이블
            case "edit.content":    return en ? "Content"      : "내용";           // 내용 입력 레이블
            case "edit.cancel":     return en ? "Cancel"       : "취소";           // 취소 버튼
            case "edit.submit":     return en ? "Save"         : "저장하기";       // 저장 버튼

            // =====================
            // CommunityWrite.aspx - 게시글 작성 페이지 텍스트
            // =====================
            case "write.title":     return en ? "New Post"      : "새 게시글 작성";  // 페이지 제목
            case "write.titleLbl":  return en ? "Title"         : "제목";            // 제목 입력 레이블
            case "write.titlePh":   return en ? "Enter title"   : "제목을 입력하세요"; // 제목 입력창 placeholder
            case "write.content":   return en ? "Content"       : "내용";            // 내용 입력 레이블
            case "write.contentPh": return en ? "Enter content" : "내용을 입력하세요"; // 내용 입력창 placeholder
            case "write.file":      return en ? "Attachment"    : "파일 첨부";       // 파일 첨부 레이블
            case "write.cancel":    return en ? "Cancel"        : "취소";            // 취소 버튼
            case "write.submit":    return en ? "Post"          : "등록하기";        // 등록 버튼

            // =====================
            // Register.aspx - 회원가입 페이지 텍스트
            // =====================
            case "reg.title":   return en ? "Sign Up"           : "회원가입";             // 페이지 제목
            case "reg.id":      return en ? "Username"          : "아이디";               // 아이디 입력 레이블
            case "reg.idPh":    return en ? "6~15 alphanumeric" : "6~15자 영문/숫자";    // 아이디 입력창 placeholder
            case "reg.pw":      return en ? "Password"          : "비밀번호";             // 비밀번호 입력 레이블
            case "reg.pwPh":    return en ? "Enter password"    : "비밀번호를 입력하세요"; // 비밀번호 placeholder
            case "reg.name":    return en ? "Name"              : "이름";                 // 이름 입력 레이블
            case "reg.nick":    return en ? "Nickname"          : "닉네임";               // 닉네임 입력 레이블
            case "reg.email":   return en ? "Email"             : "이메일";               // 이메일 입력 레이블
            case "reg.cancel":  return en ? "Cancel"            : "취소";                 // 취소 버튼
            case "reg.submit":  return en ? "Create Account"    : "가입 완료";            // 가입 완료 버튼

            // =====================
            // MyPage.aspx - 마이페이지 텍스트
            // =====================
            case "my.title":        return en ? "My Page"           : "마이페이지";                // 페이지 제목
            case "my.welcome":      return en ? "Welcome"           : "안녕하세요";               // 인사말
            case "my.recentSearch": return en ? "Recent Searches"   : "최근 검색어";              // 최근 검색어 카드 제목
            case "my.myPosts":      return en ? "My Posts"          : "내 게시글";               // 내 게시글 카드 제목
            case "my.noSearch":     return en ? "No search history.": "검색 기록이 없습니다.";   // 검색 기록 없을 때
            case "my.noPosts":      return en ? "No posts yet."     : "작성한 게시글이 없습니다."; // 게시글 없을 때
            case "my.viewAll":      return en ? "View all"          : "전체보기";                // 전체보기 링크
            case "my.totalSearch":  return en ? "Total Searches"    : "총 검색 횟수";            // 통계 카드 항목
            case "my.totalClick":   return en ? "Total Clicks"      : "총 클릭 횟수";            // 통계 카드 항목
            case "my.topKeyword":   return en ? "Top Keyword"       : "최다 검색어";             // 통계 카드 항목

            // =====================
            // Settings.aspx - 설정 페이지 텍스트
            // =====================
            case "set.title":       return en ? "Settings"       : "설정";                          // 페이지 제목
            case "set.tabGeneral":  return en ? "General"        : "일반";                          // 일반 탭 이름
            case "set.tabPrivacy":  return en ? "Privacy"        : "개인정보";                      // 개인정보 탭 이름
            case "set.theme":       return en ? "Display Theme"  : "화면 테마";                    // 테마 설정 제목
            case "set.themeSub":    return en ? "Toggle current theme." : "현재 테마 상태를 전환합니다."; // 테마 설명
            case "set.themeBtn":    return en ? "Toggle"         : "테마 변경";                    // 테마 변경 버튼
            case "set.themeOn":     return en ? "On"             : "On";                            // 다크모드 켜짐 표시
            case "set.themeOff":    return en ? "Off"            : "Off";                           // 다크모드 꺼짐 표시
            case "set.lang":        return en ? "Language"       : "언어 설정";                    // 언어 설정 제목
            case "set.langSub":     return en ? "Select service language." : "서비스 언어를 선택합니다."; // 언어 설명
            case "set.myInfo":      return en ? "Edit Profile"   : "내 정보 수정";                 // 정보 수정 섹션 제목
            case "set.name":        return en ? "Name"           : "이름";                          // 이름 입력 레이블
            case "set.nick":        return en ? "Nickname"       : "닉네임";                       // 닉네임 입력 레이블
            case "set.email":       return en ? "Email"          : "이메일";                       // 이메일 입력 레이블
            case "set.changePw":    return en ? "Change Password": "비밀번호 변경";                // 비밀번호 변경 링크
            case "set.updateBtn":   return en ? "Update"         : "정보 업데이트";                // 정보 업데이트 버튼
            case "set.stats":       return en ? "My Search Stats": "내 검색 통계";                 // 검색 통계 섹션 제목
            case "set.totalSearch": return en ? "Total Searches" : "총 검색 횟수";                 // 통계 항목
            case "set.totalClick":  return en ? "Total Clicks"   : "총 클릭 횟수";                 // 통계 항목
            case "set.topCategory": return en ? "Top Category"   : "가장 많은 카테고리";           // 통계 항목
            case "set.top5":        return en ? "Top 5 Keywords" : "자주 검색한 키워드 TOP 5";     // 통계 항목
            case "set.delSearch":   return en ? "Delete Search History"  : "검색 기록 삭제";       // 검색 기록 삭제 섹션
            case "set.delSearchSub":return en ? "Delete search records by period." : "특정 기간의 검색 기록을 삭제합니다."; // 설명
            case "set.delClick":    return en ? "Delete Click History"   : "클릭 기록 삭제";       // 클릭 기록 삭제 섹션
            case "set.delClickSub": return en ? "Delete clicked link records." : "검색 결과에서 클릭한 링크 기록을 삭제합니다."; // 설명
            case "set.delBtn":      return en ? "Delete"         : "기록 삭제";                    // 기록 삭제 버튼
            case "set.withdraw":    return en ? "Delete Account" : "회원 탈퇴";                    // 회원 탈퇴 섹션 제목
            case "set.withdrawSub": return en ? "Your account and all data will be permanently deleted." : "계정과 모든 데이터가 영구적으로 삭제됩니다."; // 탈퇴 설명
            case "set.withdrawBtn": return en ? "Delete Account" : "탈퇴하기";                    // 탈퇴 버튼
            // 기간별 삭제 모달 옵션 (드롭다운)
            case "set.delModal1h":  return en ? "Last 1 hour"    : "최근 1시간";
            case "set.delModal12h": return en ? "Last 12 hours"  : "최근 12시간";
            case "set.delModal1d":  return en ? "Last 1 day"     : "최근 1일";
            case "set.delModal7d":  return en ? "Last 7 days"    : "최근 7일";
            case "set.delModal30d": return en ? "Last 30 days"   : "최근 30일";
            case "set.delModalAll": return en ? "Delete All"     : "전체 삭제";
            case "set.cancel":      return en ? "Cancel"         : "취소";
            // 비밀번호 변경 모달
            case "set.pwModal":     return en ? "Change Password"    : "비밀번호 변경";            // 모달 제목
            case "set.pwCur":       return en ? "Current Password"   : "현재 비밀번호";            // 현재 비밀번호 레이블
            case "set.pwNew":       return en ? "New Password"       : "새 비밀번호";              // 새 비밀번호 레이블
            case "set.pwConfirm":   return en ? "Confirm Password"   : "새 비밀번호 확인";         // 비밀번호 확인 레이블
            case "set.pwBtn":       return en ? "Change"         : "변경하기";                     // 변경 버튼
            // 회원 탈퇴 확인 모달
            case "set.wdModal":     return en ? "Delete Account"     : "회원 탈퇴";               // 모달 제목
            case "set.wdDesc":      return en ? "Enter your password to permanently delete your account and all data." : "비밀번호를 입력하면 계정과 모든 데이터가 영구 삭제됩니다."; // 설명
            case "set.wdPh":        return en ? "Confirm password"   : "비밀번호 확인";           // 비밀번호 입력 placeholder
            case "set.wdBtn":       return en ? "Confirm Delete"     : "탈퇴 확인";               // 탈퇴 확인 버튼

            // =====================
            // 댓글 관련 텍스트 (CommunityView.aspx)
            // =====================
            case "comm.comments":      return en ? "Comments"               : "댓글";                       // 댓글 섹션 제목
            case "comm.noComments":    return en ? "No comments yet."       : "첫 댓글을 작성해보세요.";   // 댓글 없을 때
            case "comm.commentPh":     return en ? "Write a comment..."     : "댓글을 입력하세요...";      // 댓글 입력창 placeholder
            case "comm.commentBtn":    return en ? "Post"                   : "댓글 작성";                 // 댓글 작성 버튼
            case "comm.delComment":    return en ? "Delete"                 : "삭제";                      // 댓글 삭제 버튼
            case "comm.confirmDelCmt": return en ? "Delete this comment?"   : "댓글을 삭제하시겠습니까?"; // 댓글 삭제 확인 메시지

            // =====================
            // 좋아요 기능 텍스트
            // =====================
            case "comm.like":          return en ? "Like"                   : "좋아요";        // 좋아요 버튼
            case "comm.unlike":        return en ? "Unlike"                 : "좋아요 취소";   // 좋아요 취소 버튼

            // =====================
            // 커뮤니티 게시글 검색 텍스트
            // =====================
            case "comm.searchPh":      return en ? "Search posts..."        : "게시물 검색..."; // 검색 입력창 placeholder
            case "comm.searchBtn":     return en ? "Search"                 : "검색";           // 검색 버튼
            case "comm.searchReset":   return en ? "All posts"              : "전체보기";       // 전체보기(검색 초기화) 버튼

            // =====================
            // Settings.aspx - 프로필 사진 업로드 텍스트
            // =====================
            case "set.profilePhoto":    return en ? "Profile Photo"         : "프로필 사진";                           // 섹션 제목
            case "set.profilePhotoSub": return en ? "Upload a profile picture (jpg/png)." : "프로필 사진을 업로드합니다 (jpg/png)."; // 설명
            case "set.photoBtn":        return en ? "Upload"                : "업로드";                                // 업로드 버튼
            case "set.photoUpdated":    return en ? "Photo updated."        : "사진이 업데이트되었습니다.";           // 업로드 성공 메시지
            case "set.photoError":      return en ? "Only jpg/png allowed." : "jpg 또는 png 파일만 업로드 가능합니다."; // 업로드 오류 메시지

            // =====================
            // 알림(Notification) 관련 텍스트
            // =====================
            case "notif.title":        return en ? "Notifications"          : "알림";                                         // 알림 섹션 제목
            case "notif.noNotif":      return en ? "No notifications."      : "알림이 없습니다.";                            // 알림 없을 때
            case "notif.markRead":     return en ? "Mark all read"          : "모두 읽음";                                   // 전체 읽음 처리 버튼
            case "notif.comment":      return en ? "commented on your post" : "님이 내 게시글에 댓글을 달았습니다";        // 댓글 알림 문구
            case "notif.like":         return en ? "liked your post"        : "님이 내 게시글에 좋아요를 눌렀습니다";      // 좋아요 알림 문구

            // 등록되지 않은 키는 키 자체를 반환 (개발 중 누락된 키를 쉽게 발견하기 위함)
            default: return key;
        }
    }
}
