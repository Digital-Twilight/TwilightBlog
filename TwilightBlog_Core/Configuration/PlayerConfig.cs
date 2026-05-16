namespace TwilightBlog_Core.Configuration;

public class PlayerConfig
{
    public bool Enabled { get; set; } = true;
    public bool Autoplay { get; set; } = false;
    public double Volume { get; set; } = 0.5;
    public bool ShowCover { get; set; } = true;
    public bool Shuffle { get; set; } = false;
}