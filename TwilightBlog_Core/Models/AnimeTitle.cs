namespace TwilightBlog_Core.Models;

public class AnimeTitle
{
    public int Id { get; set; }
    public string TitleRu { get; set; } = "";
    public string TitleOriginal { get; set; } = "";
    public string? Image { get; set; }
    public string? Description { get; set; }
    public int? Year { get; set; }
    public List<string> Genres { get; set; } = [];
    public AnimeCategory? Category { get; set; }
    public AnimeStatus? Status { get; set; }
    public double? Grade { get; set; }
    public int? EpisodesReleased { get; set; }
    public int? EpisodesTotal { get; set; }
    public string? UserComment { get; set; }
}

public class AnimeCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class AnimeStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}