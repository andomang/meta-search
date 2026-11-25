/// <summary>
/// 회원 한 명의 정보를 담는 데이터 전달 객체 (DO: Domain Object / DTO: Data Transfer Object).
/// [도메인 레이어] - DB의 members 테이블 구조와 동일하게 필드를 선언하여,
/// UI 페이지 ↔ MemberDao ↔ DB 사이에서 데이터를 편리하게 주고받기 위해 사용된다.
///
/// 사용 예:
///   MemberDo m = new MemberDo("hong", "pass1234", "홍길동", "길동이", "hong@test.com");
///   new MemberDao().RegisterUser(m);
///
/// 각 필드는 private으로 선언하고 public 프로퍼티(getter/setter)로 접근하는
/// 캡슐화(Encapsulation) 패턴을 따른다.
/// </summary>
public class MemberDo
{
    // ──────────────────────────────────────────────────────────────
    // DB members 테이블의 컬럼 구조와 동일하게 private 필드 선언
    // 필드 추가 후 [오른 마우스] - [리팩토링] - [필드 캡슐화] 로 프로퍼티 자동 생성 가능
    // ──────────────────────────────────────────────────────────────

    // 회원 아이디 (DB: userid, Char(15))
    private string userid;

    /// <summary>
    /// 회원 아이디 프로퍼티.
    /// DB의 userid 컬럼에 해당하며, 최대 15자 영문/숫자로 구성된다.
    /// </summary>
    public string Userid
    {
        get { return userid; }
        set { userid = value; }
    }

    // 비밀번호 (DB: passwd, Char(32) - MD5 해시값 저장용)
    private string passwd;

    /// <summary>
    /// 비밀번호 프로퍼티.
    /// 이 객체에는 평문(Plain Text) 비밀번호를 담고,
    /// MemberDao.RegisterUser()에서 MD5 해시로 변환하여 DB에 저장한다.
    /// </summary>
    public string Passwd
    {
        get { return passwd; }
        set { passwd = value; }
    }

    // 실명 (DB: name, NChar(10))
    private string name;

    /// <summary>
    /// 회원 실명 프로퍼티.
    /// DB의 name 컬럼에 해당하며, 한글 이름 등 유니코드 문자를 지원한다.
    /// </summary>
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    // 닉네임 (DB: nickname, NChar(10)) - 화면에 표시되는 이름
    private string nickname;

    /// <summary>
    /// 닉네임 프로퍼티.
    /// 로그인 후 상단 네비게이션 바 등에 "OOO 님" 형태로 표시된다.
    /// DB의 nickname 컬럼에 해당한다.
    /// </summary>
    public string Nickname
    {
        get { return nickname; }
        set { nickname = value; }
    }

    // 이메일 주소 (DB: email, Char(20))
    private string email;

    /// <summary>
    /// 이메일 주소 프로퍼티.
    /// DB의 email 컬럼에 해당하며, 회원가입 시 입력받는다.
    /// </summary>
    public string Email
    {
        get { return email; }
        set { email = value; }
    }

    // 계정 활성화 여부 (DB: status, 'true'/'1' = 활성, 그 외 = 비활성)
    private bool status;

    /// <summary>
    /// 계정 활성화 상태 프로퍼티.
    /// true이면 로그인 가능한 활성 계정, false이면 비활성(탈퇴/정지) 계정.
    /// Authenticate() 메서드에서 status 값을 확인하여 로그인 허용 여부를 결정한다.
    /// </summary>
    public bool Status
    {
        get { return status; }
        set { status = value; }
    }

    // 회원 등급 (DB: ugrade, int) - 0: 일반, 1: 관리자 등으로 활용 가능
    private int ugrade;

    /// <summary>
    /// 회원 등급 프로퍼티.
    /// 숫자로 등급을 구분하며 (예: 0 = 일반 회원, 1 = 관리자),
    /// 관리자 기능 구현 시 이 값으로 권한을 판단할 수 있다.
    /// </summary>
    public int Ugrade
    {
        get { return ugrade; }
        set { ugrade = value; }
    }

    /// <summary>
    /// 기본 생성자.
    /// 빈 MemberDo 객체를 생성한 뒤 각 프로퍼티를 개별 설정할 때 사용.
    /// 예: MemberDo m = new MemberDo(); m.Userid = "hong"; ...
    /// </summary>
    public MemberDo()
    {
    }

    /// <summary>
    /// 회원가입 및 정보 수정 시 필요한 핵심 5개 필드를 한 번에 초기화하는 생성자.
    /// (생성자 오버로딩 - 기본 생성자와 이름은 같지만 파라미터가 다름)
    /// 예: MemberDo m = new MemberDo("hong", "pass1234", "홍길동", "길동이", "hong@test.com");
    /// </summary>
    /// <param name="uid">회원 아이디</param>
    /// <param name="pwd">평문 비밀번호 (MemberDao에서 MD5로 변환됨)</param>
    /// <param name="name">실명</param>
    /// <param name="nickname">닉네임 (화면 표시용)</param>
    /// <param name="email">이메일 주소</param>
    public MemberDo(string uid, string pwd, string name, string nickname, string email)
    {
        // this 키워드: 현재 객체의 필드를 가리킴 (파라미터 name과 필드 name을 구분하기 위해 사용)
        this.userid = uid;
        this.passwd = pwd;
        this.name = name;
        this.nickname = nickname;
        this.email = email;
    }
}
