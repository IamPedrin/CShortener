public class Url
{
    public long Id { get; private set; }
    public string OriginalUrl { get; private set; }
    public string ShortCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int AccessCount { get; private set; }


    public Url(long id, string originalUrl, string shortCode)
    {
        Id = id;
        OriginalUrl = originalUrl;
        ShortCode = shortCode;
        CreatedAt = DateTime.UtcNow;
        AccessCount = 0;
    }

    public void NewAccess()
    {
        AccessCount++;
    }

}

public record CreateUrlRequest
{
    public required string OriginalUrl { get; set; }
}