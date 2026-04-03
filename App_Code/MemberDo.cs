/// <summary>
/// MemberDo의 요약 설명입니다.
/// </summary>
public class MemberDo
{
    //데이터베이스의 members 테이블의 필드구조와 동일하게 선언
    //필즈 추가 후 [오른 마우스]-[리펙토링]-[필드 캡슐화]
    private string userid;

    public string Userid
    {
        get { return userid; }
        set { userid = value; }
    }
    private string passwd;

    public string Passwd
    {
        get { return passwd; }
        set { passwd = value; }
    }
    private string name;

    public string Name
    {
        get { return name; }
        set { name = value; }
    }
    private string nickname;

    public string Nickname
    {
        get { return nickname; }
        set { nickname = value; }
    }
    private string email;

    public string Email
    {
        get { return email; }
        set { email = value; }
    }
    private bool status;

    public bool Status
    {
        get { return status; }
        set { status = value; }
    }
    private int ugrade;

    public int Ugrade
    {
        get { return ugrade; }
        set { ugrade = value; }
    }

    //기본 생성자
    public MemberDo()
    {
    }

    //인수 전달 생성자 --> 생성자 오버로딩
    //회원가입 및 정보수정 기능을 구현할 때 사용
    public MemberDo(string uid, string pwd, string name, string nickname, string email)
    {
        this.userid = uid;
        this.passwd = pwd;
        this.name = name;
        this.nickname = nickname;
        this.email = email;
    }
}