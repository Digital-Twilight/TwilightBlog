namespace TwilightBlog_Core.Configuration;

public class SakuraConfig
{
    public bool Enabled { get; set; } = true;
    public int Count { get; set; } = 30;
    public double MinSpeed { get; set; } = 1.0;
    public double MaxSpeed { get; set; } = 3.0;
    public double MinSize { get; set; } = 8.0;
    public double MaxSize { get; set; } = 18.0;
    public double Opacity { get; set; } = 0.7;
    public string Color { get; set; } = "#ffb7c5";
}