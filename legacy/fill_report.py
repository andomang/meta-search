# -*- coding: utf-8 -*-
"""
캡스톤디자인 결과보고서 자동 작성 스크립트
Hancom Office HWP COM API를 이용해 템플릿에 각 섹션 내용을 입력한다.
"""
import win32com.client
import time
import os

TEMPLATE = r"c:\2026\meta\캡스톤디자인결과보고서_양식_2026.hwp"
OUTPUT   = r"c:\2026\meta\캡스톤디자인결과보고서_완성본_2026.hwp"

def main():
    hwp = win32com.client.Dispatch("HWPFrame.HwpObject")
    hwp.XHwpWindows.Item(0).Visible = False

    hwp.Open(TEMPLATE, "HWP", "forceopen:1")
    time.sleep(1)

    # ── 헬퍼: 텍스트 검색 ──────────────────────────────────────────
    def find(text):
        hwp.Run("MoveDocBegin")
        hwp.HAction.GetDefault("FindReplace", hwp.HParameterSet.HFindReplace.HSet)
        hwp.HParameterSet.HFindReplace.FindString    = text
        hwp.HParameterSet.HFindReplace.ReplaceString = ""
        hwp.HParameterSet.HFindReplace.IgnoreMessage = 1
        hwp.HParameterSet.HFindReplace.FindType      = 0
        hwp.HParameterSet.HFindReplace.Direction     = 0
        hwp.HParameterSet.HFindReplace.WholeWordOnly = 0
        hwp.HParameterSet.HFindReplace.IgnoreCase    = 0
        return hwp.HAction.Execute("FindReplace", hwp.HParameterSet.HFindReplace.HSet)

    # ── 헬퍼: 텍스트 입력 ──────────────────────────────────────────
    def write(text):
        hwp.HAction.GetDefault("InsertText", hwp.HParameterSet.HInsertText.HSet)
        hwp.HParameterSet.HInsertText.Text = text
        hwp.HAction.Execute("InsertText", hwp.HParameterSet.HInsertText.HSet)

    def nl():
        hwp.Run("BreakPara")

    def section(heading, lines):
        if find(heading):
            hwp.Run("MoveLineEnd")
            nl()
            for line in lines:
                write(line)
                nl()
        else:
            print("[WARN] not found:", heading)

    # ── 1. 프로젝트의 배경 ─────────────────────────────────────────
    section("1. 프로젝트의 배경", [
        "- 기존 단일 검색 엔진(네이버, 구글)의 편향된 검색 결과 및 광고 과다 노출 문제",
        "- 원하는 결과를 얻기 위해 여러 검색 엔진을 개별 방문해야 하는 불편",
        "- AI 기술 발전으로 검색어의 지능적 분류·정렬 가능성 대두",
        "- 사용자 간 정보 공유를 위한 커뮤니티 공간의 필요성",
    ])

    # ── 2. 프로젝트의 목표 ─────────────────────────────────────────
    section("2. 프로젝트의 목표", [
        "- 네이버·구글·다음 등 복수 검색 엔진 결과를 통합하는 메타 검색 엔진 구축",
        "- Anthropic Claude AI API를 활용한 검색어 카테고리 자동 분류 (13개 카테고리)",
        "- ASP.NET Web Forms (.NET 4.8) 기반 회원 커뮤니티 구현 (게시판·댓글·좋아요·알림)",
        "- 한국어/영어 다국어 지원 및 다크 모드 적용으로 접근성 향상",
        "- 마이페이지·설정을 통한 개인화 서비스 제공",
    ])

    # ── 3. 사례 연구 ───────────────────────────────────────────────
    section("3. 사례 연구", [
        "[야후(Yahoo)]",
        "- 내용: Bing 검색엔진에 검색 결과를 전량 의존하는 구조 채택",
        "- 분석: 구글 등장 후 검색 정확도·성능에서 밀려 시장 점유율 급락",
        "- 시사점: 자체 알고리즘 없이 외부 엔진 의존 시 경쟁력 약화",
        "",
        "[코리아닷컴(Korea.com)]",
        "- 내용: 메가서치(MegaSearch) AI 검색엔진 도입, 2000년대 초 AI 활용 시도",
        "- 분석: 초기 AI 기술의 낮은 정확도·광고 과다로 사용자 신뢰 확보 실패",
        "- 시사점: 검색 정확도와 사용자 경험이 핵심 경쟁력",
        "",
        "[네이트(Nate)]",
        "- 내용: 2013년 이후 다음 검색엔진 채택, 커뮤니티(판) 중심 서비스로 전환",
        "- 분석: 검색 차별화 포기 후 커뮤니티 콘텐츠로 사용자 유입",
        "- 시사점: 커뮤니티 기능이 사용자 체류 시간·충성도 향상에 기여",
    ])

    # ── 4. 프로젝트의 개요, 범위, 내용, 방법 ─────────────────────
    section("4. 프로젝트의 개요, 범위, 내용, 방법", [
        "[프로젝트 개요]",
        "복수 검색 플랫폼의 결과를 통합·AI 분류하는 ASP.NET 기반 메타 검색 엔진 및",
        "회원 커뮤니티 포털 개발",
        "",
        "[개발 범위]",
        "- Back-end : ASP.NET Web Forms (.NET 4.8), C#, SQL Server",
        "- Front-end: Tailwind CSS, Lucide Icons, Vanilla JavaScript (fetch API)",
        "- AI 연동  : Anthropic Claude Haiku 4.5 API – 검색어 카테고리 분류",
        "- DB       : SQL Server 7개 테이블",
        "",
        "[주요 기능 및 방법]",
        "- 검색어 입력 → Claude AI로 카테고리 분류(13종) → SearchHistory 테이블 저장",
        "- 회원 가입/로그인 (MD5 암호화), 다크 모드·언어 쿠키 기반 전환",
        "- 커뮤니티 CRUD, 파일 첨부, 댓글·좋아요 AJAX (페이지 새로고침 없음)",
        "- 실시간 알림: 댓글/좋아요 발생 시 Notifications 테이블 INSERT → 벨 뱃지 표시",
        "- 마이페이지: 검색 횟수·클릭 수·최다 검색어 통계",
    ])

    # ── 5. 팀 구성 및 역할 분담 ────────────────────────────────────
    section("5. 팀 구성 및 역할 분담", [
        "팀명: 서치 믹스 (Search Mix)   지도교수: 김영우",
        "",
        "이름          학번        역할",
        "안도영(팀장)  42225017    전체 설계, 백엔드 개발, Claude AI API 연동, DB 설계",
        "안민지        42225019    UI/UX 설계, 프론트엔드, Tailwind CSS 반응형 레이아웃",
        "윤서준        42225020    검색 기능 구현, 커뮤니티 기능, 통합 테스트",
    ])

    # ── 6. 시스템 구성도 (Sitemap) ─────────────────────────────────
    section("6. 시스템 구성도", [
        "주 메뉴             부 메뉴                 내용",
        "홈 (Default)        검색창                  메인 검색 인터페이스",
        "                    검색 버튼               쿼리를 SearchResults.aspx 로 전달",
        "검색 결과           결과 리스트             검색 결과 카드 출력 (URL 이동)",
        "                    사이드바                인기 검색어 TOP 10",
        "커뮤니티            게시글 목록             Repeater 동적 렌더링",
        "                    게시글 작성/수정/상세   CRUD + 파일 첨부 + 댓글·좋아요",
        "설정                계정 / 화면 / 지역      프로필·다크 모드·언어·기록 삭제",
        "회원 관리           로그인 (모달)           Site.Master 공통 모달",
        "                    회원가입                Register.aspx",
        "                    마이페이지              통계·최근 검색어·내 게시글",
    ])

    # ── 7. 데이터베이스 설계 ───────────────────────────────────────
    section("7. 데이터베이스 설계", [
        "테이블명              주요 컬럼",
        "members               userid(PK), passwd, name, nickname, email,",
        "                      darkmode, language, profileImg",
        "Bbs                   No(PK), Title, Contents, Author(FK), UploadTime,",
        "                      Hits, FileName, FileSize, FilePath",
        "BbsComments           CommentID(PK), BbsNo(FK), Author(FK), Content, CreatedAt",
        "BbsLikes              LikeID(PK), BbsNo(FK), UserID(FK)",
        "SearchHistory         HistoryID(PK), UserID(FK), Query, Category, SearchedAt",
        "SearchClickHistory    ClickID(PK), UserID(FK), Query, Category,",
        "                      ClickedUrl, ClickedTitle, ClickedAt",
        "Notifications         NotifID(PK), UserID(FK), Type, BbsNo(FK),",
        "                      CommentID, IsRead, ActorID(FK)",
    ])

    # ── 8. 구현 결과 ───────────────────────────────────────────────
    section("8. 구현 결과", [
        "[Default.aspx – 메인 검색 페이지]",
        "- 중앙 검색창, 최근·인기 검색어 드롭다운, 커뮤니티 바로가기 카드",
        "- 다크 모드 및 한/영 전환 즉시 반영",
        "",
        "[SearchResults.aspx – 검색 결과 페이지]",
        "- 검색 결과 카드 리스트 (제목, URL, 설명, 출처)",
        "- 우측 사이드바: 인기 검색어 TOP 10 (1~3위 색상 강조)",
        "- Claude AI API 호출로 검색어 카테고리 자동 분류 및 DB 저장",
        "",
        "[Community.aspx – 게시판 목록]",
        "- 게시글 목록 테이블: 번호·제목·작성자·조회수·좋아요·댓글 수·날짜",
        "- 키워드 검색 및 서버 사이드 페이지네이션",
        "",
        "[CommunityView.aspx – 게시글 상세]",
        "- 게시글 본문, 첨부파일 다운로드",
        "- 좋아요 토글 버튼 (AJAX), 댓글 추가·삭제 실시간 처리",
        "",
        "[Settings.aspx – 설정]",
        "- 탭 UI (일반 / 개인정보보호)",
        "- 프로필 사진 업로드, 비밀번호 변경, 회원 탈퇴, 검색 통계 대시보드",
        "",
        "[Site.Master – 공통 레이아웃]",
        "- 상단 내비게이션, 알림 벨 아이콘 (읽지 않은 알림 수 뱃지)",
        "- 알림 드롭다운: 댓글·좋아요 알림, 개별 읽음·삭제 지원",
    ])

    # ── 9. 기대효과 및 활용방안 ────────────────────────────────────
    section("9. 기대효과 및 활용방안", [
        "- 다중 검색 엔진 통합으로 단일 엔진 대비 다양하고 균형 잡힌 결과 제공",
        "- AI 자동 분류(13개 카테고리)로 사용자 검색 패턴 수집·맞춤 서비스 기반 마련",
        "- 커뮤니티 기능을 통해 검색 외 사용자 유입 및 체류 시간 증가",
        "- ASP.NET, Claude AI API, SQL Server 역량을 포트폴리오로 증명 가능",
    ])

    # ── 10. 결론 및 향후 계획 ──────────────────────────────────────
    section("10. 결론 및 향후 계획", [
        "[결론]",
        "- ASP.NET Web Forms (.NET 4.8) 기반 메타 검색 엔진 핵심 구조 완성",
        "- Claude AI API 연동으로 검색어 카테고리 자동 분류 기능 구현",
        "- 회원 관리·커뮤니티·알림·설정·마이페이지 등 포털 핵심 기능 구현",
        "- Tailwind CSS 다크 모드·다국어 지원으로 접근성 향상",
        "",
        "[향후 계획]",
        "- 실제 크롤러 모듈 개발: 네이버 검색 API, 구글 Custom Search API 연동",
        "- ClaudeApi.RankResults() 활용 AI 기반 검색 결과 재정렬 적용",
        "- 모바일 반응형 레이아웃 강화",
        "- 장애인 접근성 기능 추가 (고대비 모드, 텍스트 크기 조절)",
    ])

    # ── 저장 ────────────────────────────────────────────────────────
    hwp.SaveAs(OUTPUT, "HWP", "")
    hwp.Quit()
    print("저장 완료:", OUTPUT)

if __name__ == "__main__":
    main()
