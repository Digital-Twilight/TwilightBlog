namespace TwilightBlog_Core.Configuration;

public class DevicesConfig
{
    public List<DeviceItem> Items { get; set; } = [];
}

public class DeviceItem
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? Link { get; set; }
}