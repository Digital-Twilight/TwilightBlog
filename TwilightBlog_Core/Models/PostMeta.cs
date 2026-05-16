namespace TwilightBlog_Core.Models;

public class PostMeta
{
    public string Title { get; set; } = "";
    public DateTime Published { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Category { get; set; } = "";
    public bool Draft { get; set; } = false;
    public string Slug { get; set; } = "";
    public string ContentHtml { get; set; } = "";
    public string Description { get; set; } = "";
    public string ArchiveType { get; set; } = "post";
}