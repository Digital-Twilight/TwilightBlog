namespace TwilightBlog_Core.Configuration;

public class SoundCloudConfig
{
    public string ClientId { get; set; } = "";
    public string OAuthToken { get; set; } = "";
    public List<string> PlaylistUrls { get; set; } = [];
    public List<string> TrackUrls { get; set; } = [];
}