using System.Web;

/// <summary>
/// 다국어 지원 헬퍼 클래스 (한국어/영어)
/// Session["Lang"] 값을 기준으로 "ko" = 한국어, "en" = 영어 텍스트 반환
/// 모든 페이지의 Literal, Button.Text, placeholder 등에서 Lang.Get("key") 방식으로 호출
/// </summary>
public static class Lang
{
    /// <summary>
    /// 현재 세션의 언어가 영어인지 확인
    /// </summary>
    private static bool IsEn
    {
        get
        {
            var session = HttpContext.Current.Session;
            return session != null && session["Lang"] != null && session["Lang"].ToString() == "en";
        }
    }

    /// <summary>
    /// 키에 해당하는 현재 언어 텍스트 반환
    /// 키가 없으면 키 자체를 반환 (디버깅 용이)
    /// </summary>
    /// <param name="key">텍스트 키 (예: "nav.community")</param>
    /// <returns>현재 언어에 맞는 텍스트</returns>
    public static string Get(string key)
    {
        bool en = IsEn;
        switch (key)
        {
            // =====================
            // Site.Master - 내비게이션
            // =====================
            case "nav.community":   return en ? "Community"   : "커뮤니티";
            case "nav.settings":    return en ? "Settings"    : "설정";
            case "nav.mypage":      return en ? "My Page"     : "마이페이지";
            case "nav.login":       return en ? "Login"       : "로그인";
            case "nav.logout":      return en ? "Logout"      : "로그아웃";
            case "nav.greeting":    return en ? ""            : "님";

            // 로그인 모달
            case "login.title":     return en ? "Login"       : "로그인";
            case "login.id":        return en ? "Username"    : "아이디";
            case "login.pw":        return en ? "Password"    : "비밀번호";
            case "login.btn":       return en ? "Sign In"     : "로그인";
            case "login.noAccount": return en ? "Don't have an account?" : "계정이 없으신가요?";
            case "login.register":  return en ? "Sign up"     : "회원가입";
            case "login.close":     return en ? "Close"       : "닫기";

            // =====================
            // Default.aspx - 메인 페이지
            // =====================
            case "home.subtitle":   return en ? "Search everything"       : "모든 정보를 검색하세요";
            case "home.community":  return en ? "Community"               : "자유게시판";
            case "home.commSub":    return en ? "Share and discuss"       : "함께 이야기 나누는 공간";
            case "home.settings":   return en ? "Settings"                : "개인 설정";
            case "home.settSub":    return en ? "Dark mode & preferences" : "다크모드 및 환경 설정";

            // =====================
            // SearchResults.aspx - 검색 결과
            // =====================
            case "search.results":      return en ? "Search results"      : "검색 결과 리스트입니다.";
            case "search.related":      return en ? "Related searches"    : "관련 검색어";
            case "search.popular":      return en ? "Popular searches"    : "인기 검색어";
            case "search.recent":       return en ? "Recent searches"     : "최근 검색어";
            case "search.noHistory":    return en ? "No search history."  : "검색 기록이 없습니다.";
            case "search.placeholder":  return en ? "Search anything..."  : "검색어를 입력하세요";
            case "search.noResults":    return en ? "No results found."   : "검색 결과가 없습니다.";
            case "search.noResultsSub": return en ? "Try a different keyword." : "다른 검색어를 입력해 보세요.";
            case "search.loading":      return en ? "Searching..."        : "검색 중...";
            case "search.times":        return en ? "times"               : "회";

            // =====================
            // Community.aspx - 게시판 목록
            // =====================
            case "comm.title":      return en ? "Community"   : "커뮤니티 광장";
            case "comm.write":      return en ? "Write"       : "글쓰기";
            case "comm.no":         return en ? "No."         : "번호";
            case "comm.titleCol":   return en ? "Title"       : "제목";
            case "comm.author":     return en ? "Author"      : "작성자";
            case "comm.date":       return en ? "Date"        : "날짜";
            case "comm.prev":       return en ? "Prev"        : "이전";
            case "comm.next":       return en ? "Next"        : "다음";
            case "comm.empty":      return en ? "No posts yet." : "게시글이 없습니다.";

            // =====================
            // CommunityView.aspx - 게시글 상세
            // =====================
            case "view.board":      return en ? "Free Board"   : "자유게시판";
            case "view.download":   return en ? "Download"     : "다운로드";
            case "view.back":       return en ? "Back to list" : "목록으로";
            case "view.delete":     return en ? "Delete"       : "삭제";
            case "view.edit":       return en ? "Edit"         : "수정";
            case "view.confirmDel": return en ? "Are you sure you want to delete this post?" : "정말 삭제하시겠습니까?";

            // =====================
            // CommunityEdit.aspx - 게시글 수정 (새 페이지)
            // =====================
            case "edit.title":      return en ? "Edit Post"    : "게시글 수정";
            case "edit.titleLbl":   return en ? "Title"        : "제목";
            case "edit.content":    return en ? "Content"      : "내용";
            case "edit.cancel":     return en ? "Cancel"       : "취소";
            case "edit.submit":     return en ? "Save"         : "저장하기";

            // =====================
            // CommunityWrite.aspx - 게시글 작성
            // =====================
            case "write.title":     return en ? "New Post"      : "새 게시글 작성";
            case "write.titleLbl":  return en ? "Title"         : "제목";
            case "write.titlePh":   return en ? "Enter title"   : "제목을 입력하세요";
            case "write.content":   return en ? "Content"       : "내용";
            case "write.contentPh": return en ? "Enter content" : "내용을 입력하세요";
            case "write.file":      return en ? "Attachment"    : "파일 첨부";
            case "write.cancel":    return en ? "Cancel"        : "취소";
            case "write.submit":    return en ? "Post"          : "등록하기";

            // =====================
            // Register.aspx - 회원가입
            // =====================
            case "reg.title":   return en ? "Sign Up"           : "회원가입";
            case "reg.id":      return en ? "Username"          : "아이디";
            case "reg.idPh":    return en ? "6~15 alphanumeric" : "6~15자 영문/숫자";
            case "reg.pw":      return en ? "Password"          : "비밀번호";
            case "reg.pwPh":    return en ? "Enter password"    : "비밀번호를 입력하세요";
            case "reg.name":    return en ? "Name"              : "이름";
            case "reg.nick":    return en ? "Nickname"          : "닉네임";
            case "reg.email":   return en ? "Email"             : "이메일";
            case "reg.cancel":  return en ? "Cancel"            : "취소";
            case "reg.submit":  return en ? "Create Account"    : "가입 완료";

            // =====================
            // MyPage.aspx - 마이페이지 (새 페이지)
            // =====================
            case "my.title":        return en ? "My Page"           : "마이페이지";
            case "my.welcome":      return en ? "Welcome"           : "안녕하세요";
            case "my.recentSearch": return en ? "Recent Searches"   : "최근 검색어";
            case "my.myPosts":      return en ? "My Posts"          : "내 게시글";
            case "my.noSearch":     return en ? "No search history.": "검색 기록이 없습니다.";
            case "my.noPosts":      return en ? "No posts yet."     : "작성한 게시글이 없습니다.";
            case "my.viewAll":      return en ? "View all"          : "전체보기";
            case "my.totalSearch":  return en ? "Total Searches"    : "총 검색 횟수";
            case "my.totalClick":   return en ? "Total Clicks"      : "총 클릭 횟수";
            case "my.topKeyword":   return en ? "Top Keyword"       : "최다 검색어";

            // =====================
            // Settings.aspx - 설정
            // =====================
            case "set.title":       return en ? "Settings"       : "설정";
            case "set.tabGeneral":  return en ? "General"        : "일반";
            case "set.tabPrivacy":  return en ? "Privacy"        : "개인정보";
            case "set.theme":       return en ? "Display Theme"  : "화면 테마";
            case "set.themeSub":    return en ? "Toggle current theme." : "현재 테마 상태를 전환합니다.";
            case "set.themeBtn":    return en ? "Toggle"         : "테마 변경";
            case "set.themeOn":     return en ? "On"             : "On";
            case "set.themeOff":    return en ? "Off"            : "Off";
            case "set.lang":        return en ? "Language"       : "언어 설정";
            case "set.langSub":     return en ? "Select service language." : "서비스 언어를 선택합니다.";
            case "set.myInfo":      return en ? "Edit Profile"   : "내 정보 수정";
            case "set.name":        return en ? "Name"           : "이름";
            case "set.nick":        return en ? "Nickname"       : "닉네임";
            case "set.email":       return en ? "Email"          : "이메일";
            case "set.changePw":    return en ? "Change Password": "비밀번호 변경";
            case "set.updateBtn":   return en ? "Update"         : "정보 업데이트";
            case "set.stats":       return en ? "My Search Stats": "내 검색 통계";
            case "set.totalSearch": return en ? "Total Searches" : "총 검색 횟수";
            case "set.totalClick":  return en ? "Total Clicks"   : "총 클릭 횟수";
            case "set.topCategory": return en ? "Top Category"   : "가장 많은 카테고리";
            case "set.top5":        return en ? "Top 5 Keywords" : "자주 검색한 키워드 TOP 5";
            case "set.delSearch":   return en ? "Delete Search History"  : "검색 기록 삭제";
            case "set.delSearchSub":return en ? "Delete search records by period." : "특정 기간의 검색 기록을 삭제합니다.";
            case "set.delClick":    return en ? "Delete Click History"   : "클릭 기록 삭제";
            case "set.delClickSub": return en ? "Delete clicked link records." : "검색 결과에서 클릭한 링크 기록을 삭제합니다.";
            case "set.delBtn":      return en ? "Delete"         : "기록 삭제";
            case "set.withdraw":    return en ? "Delete Account" : "회원 탈퇴";
            case "set.withdrawSub": return en ? "Your account and all data will be permanently deleted." : "계정과 모든 데이터가 영구적으로 삭제됩니다.";
            case "set.withdrawBtn": return en ? "Delete Account" : "탈퇴하기";
            case "set.delModal1h":  return en ? "Last 1 hour"    : "최근 1시간";
            case "set.delModal12h": return en ? "Last 12 hours"  : "최근 12시간";
            case "set.delModal1d":  return en ? "Last 1 day"     : "최근 1일";
            case "set.delModal7d":  return en ? "Last 7 days"    : "최근 7일";
            case "set.delModal30d": return en ? "Last 30 days"   : "최근 30일";
            case "set.delModalAll": return en ? "Delete All"     : "전체 삭제";
            case "set.cancel":      return en ? "Cancel"         : "취소";
            case "set.pwModal":     return en ? "Change Password"    : "비밀번호 변경";
            case "set.pwCur":       return en ? "Current Password"   : "현재 비밀번호";
            case "set.pwNew":       return en ? "New Password"       : "새 비밀번호";
            case "set.pwConfirm":   return en ? "Confirm Password"   : "새 비밀번호 확인";
            case "set.pwBtn":       return en ? "Change"         : "변경하기";
            case "set.wdModal":     return en ? "Delete Account"     : "회원 탈퇴";
            case "set.wdDesc":      return en ? "Enter your password to permanently delete your account and all data." : "비밀번호를 입력하면 계정과 모든 데이터가 영구 삭제됩니다.";
            case "set.wdPh":        return en ? "Confirm password"   : "비밀번호 확인";
            case "set.wdBtn":       return en ? "Confirm Delete"     : "탈퇴 확인";

            // =====================
            // 댓글
            // =====================
            case "comm.comments":      return en ? "Comments"               : "댓글";
            case "comm.noComments":    return en ? "No comments yet."       : "첫 댓글을 작성해보세요.";
            case "comm.commentPh":     return en ? "Write a comment..."     : "댓글을 입력하세요...";
            case "comm.commentBtn":    return en ? "Post"                   : "댓글 작성";
            case "comm.delComment":    return en ? "Delete"                 : "삭제";
            case "comm.confirmDelCmt": return en ? "Delete this comment?"   : "댓글을 삭제하시겠습니까?";

            // =====================
            // 좋아요
            // =====================
            case "comm.like":          return en ? "Like"                   : "좋아요";
            case "comm.unlike":        return en ? "Unlike"                 : "좋아요 취소";

            // =====================
            // 커뮤니티 검색
            // =====================
            case "comm.searchPh":      return en ? "Search posts..."        : "게시물 검색...";
            case "comm.searchBtn":     return en ? "Search"                 : "검색";
            case "comm.searchReset":   return en ? "All posts"              : "전체보기";

            // =====================
            // 프로필 사진 (Settings)
            // =====================
            case "set.profilePhoto":    return en ? "Profile Photo"         : "프로필 사진";
            case "set.profilePhotoSub": return en ? "Upload a profile picture (jpg/png)." : "프로필 사진을 업로드합니다 (jpg/png).";
            case "set.photoBtn":        return en ? "Upload"                : "업로드";
            case "set.photoUpdated":    return en ? "Photo updated."        : "사진이 업데이트되었습니다.";
            case "set.photoError":      return en ? "Only jpg/png allowed." : "jpg 또는 png 파일만 업로드 가능합니다.";

            // =====================
            // 알림
            // =====================
            case "notif.title":        return en ? "Notifications"          : "알림";
            case "notif.noNotif":      return en ? "No notifications."      : "알림이 없습니다.";
            case "notif.markRead":     return en ? "Mark all read"          : "모두 읽음";
            case "notif.comment":      return en ? "commented on your post" : "님이 내 게시글에 댓글을 달았습니다";
            case "notif.like":         return en ? "liked your post"        : "님이 내 게시글에 좋아요를 눌렀습니다";

            default: return key;
        }
    }
}
