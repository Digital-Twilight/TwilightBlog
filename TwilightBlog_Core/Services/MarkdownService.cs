using Markdig;
using TwilightBlog_Core.Interfaces;
using TwilightBlog_Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TwilightBlog_Core.Services;

public class MarkdownService : IMarkdownService, IDisposable
{
    private readonly string _postsPath;
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;
    private List<PostMeta>? _cache;
    private readonly FileSystemWatcher _watcher;

    public MarkdownService(string postsPath)
    {
        _postsPath = postsPath;

        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        if (Directory.Exists(postsPath))
        {
            _watcher = new FileSystemWatcher(postsPath, "*.md")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            _watcher.Changed += OnPostsChanged;
            _watcher.Created += OnPostsChanged;
            _watcher.Deleted += OnPostsChanged;
            _watcher.Renamed += OnPostsChanged;
        }
    }

    private void OnPostsChanged(object sender, FileSystemEventArgs e)
    {
        _cache = null;
    }

    public async Task<List<PostMeta>> GetAllPostsAsync(bool includeDrafts = false)
    {
        if (_cache is not null)
            return FilterDrafts(_cache, includeDrafts);

        List<PostMeta> posts = [];

        if (!Directory.Exists(_postsPath))
            return posts;

        foreach (string file in Directory.GetFiles(_postsPath, "*.md"))
        {
            PostMeta? post = await ParsePostAsync(file);
            if (post is not null)
                posts.Add(post);
        }

        _cache = posts.OrderByDescending(p => p.Published).ToList();
        return FilterDrafts(_cache, includeDrafts);
    }

    public async Task<PostMeta?> GetPostBySlugAsync(string slug)
    {
        List<PostMeta> all = await GetAllPostsAsync(includeDrafts: true);
        return all.FirstOrDefault(p => p.Slug == slug);
    }

    public async Task<List<PostMeta>> GetPostsByCategoryAsync(string category)
    {
        List<PostMeta> all = await GetAllPostsAsync();
        return all
            .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<PostMeta>> GetPostsByTagAsync(string tag)
    {
        List<PostMeta> all = await GetAllPostsAsync();
        return all
            .Where(p => p.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<List<IGrouping<int, PostMeta>>> GetArchiveAsync()
    {
        List<PostMeta> all = await GetAllPostsAsync();
        return all
            .GroupBy(p => p.Published.Year)
            .OrderByDescending(g => g.Key)
            .ToList<IGrouping<int, PostMeta>>();
    }

    private async Task<PostMeta?> ParsePostAsync(string filePath)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath);

            if (!content.StartsWith("---"))
                return null;

            int endIndex = content.IndexOf("---", 3);
            if (endIndex < 0)
                return null;

            string yaml = content.Substring(3, endIndex - 3).Trim();
            string body = content.Substring(endIndex + 3).Trim();

            FrontMatter frontmatter = _yamlDeserializer.Deserialize<FrontMatter>(yaml);
            string contentHtml = Markdown.ToHtml(body, _pipeline);

            string plainText = System.Text.RegularExpressions.Regex.Replace(contentHtml, "<.*?>", " ");
            plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();
            string excerpt = plainText.Length > 200
                ? plainText.Substring(0, 200).TrimEnd() + "..."
                : plainText;

            return new PostMeta
            {
                Title = frontmatter.Title,
                Published = frontmatter.Published,
                Tags = frontmatter.Tags ?? [],
                Category = frontmatter.Category ?? "",
                Draft = frontmatter.Draft,
                ArchiveType = frontmatter.ArchiveType ?? "post",
                Slug = Path.GetFileNameWithoutExtension(filePath),
                ContentHtml = contentHtml,
                Description = frontmatter.Description ?? ""
            };
        }
        catch
        {
            return null;
        }
    }

    private static List<PostMeta> FilterDrafts(List<PostMeta> posts, bool includeDrafts)
        => includeDrafts ? posts : posts.Where(p => !p.Draft).ToList();

    private class FrontMatter
    {
        public string Title { get; set; } = "";
        public DateTime Published { get; set; }
        public List<string>? Tags { get; set; }
        public string? Category { get; set; }
        public bool Draft { get; set; } = false;
        public string? ArchiveType { get; set; }
        public string? Description { get; set; }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}