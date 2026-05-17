namespace TwilightBlog_Core.Configuration;

public class AnixartConfig
{
    public string Token { get; set; } = "";
    public string UserId { get; set; } = "";
    public int CacheMinutes { get; set; } = 60;
    public Dictionary<string, string> Comments { get; set; } = [];
}