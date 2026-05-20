namespace TwilightBlog_Core.Configuration;

public class SteamConfig
{
    public string ApiKey { get; set; } = "";
    public string SteamId { get; set; } = "";
    public int CacheMinutes { get; set; } = 60;
    public List<int> HiddenAppIds { get; set; } = [];
    public Dictionary<string, string> Comments { get; set; } = [];
}