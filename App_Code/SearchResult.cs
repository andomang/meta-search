// App_Code/SearchResult.cs
public class SearchResult
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }   // "naver", "daum", "google"
    public int Score { get; set; }       // 점수제 계산용
}