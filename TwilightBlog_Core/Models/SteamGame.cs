namespace TwilightBlog_Core.Models;

public class SteamGame
{
    public int AppId { get; set; }
    public string Name { get; set; } = "";
    public string? IconUrl { get; set; }
    public string? HeaderUrl { get; set; }
    public int PlaytimeMinutes { get; set; }
    public string? UserComment { get; set; }

    public string PlaytimeFormatted => PlaytimeMinutes switch
    {
        < 60 => $"{PlaytimeMinutes} мин",
        _ => $"{PlaytimeMinutes / 60} ч {PlaytimeMinutes % 60} мин"
    };
}