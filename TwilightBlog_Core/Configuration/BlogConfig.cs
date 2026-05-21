namespace TwilightBlog_Core.Configuration;

public class BlogConfig
{
    public string Title { get; set; } = "TwilightBlog";
    public string Subtitle { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public SocialLinks Social { get; set; } = new();
    public string Language { get; set; } = "ru";
}

public class SocialLinks
{
    public string? GitHub { get; set; }
    public string? Telegram { get; set; }
    public string? Twitter { get; set; }
    public string? Email { get; set; }
}