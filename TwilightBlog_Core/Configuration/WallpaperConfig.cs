namespace TwilightBlog_Core.Configuration;

public class WallpaperConfig
{
    public bool Enabled { get; set; } = false;
    public WallpaperThemeSet Light { get; set; } = new();
    public WallpaperThemeSet Dark { get; set; } = new();
    public double Blur { get; set; } = 0.0;
    public double Dimming { get; set; } = 0.3;
    public string Size { get; set; } = "cover";
    public string Position { get; set; } = "center";
    public int IntervalSeconds { get; set; } = 30;
    public string Transition { get; set; } = "fade"; // fade | none
    public double TransitionDuration { get; set; } = 1.0;
}

public class WallpaperThemeSet
{
    public List<string> Images { get; set; } = [];
}