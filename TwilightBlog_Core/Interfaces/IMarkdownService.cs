using TwilightBlog_Core.Models;

namespace TwilightBlog_Core.Interfaces;

public interface IMarkdownService
{
    Task<List<PostMeta>> GetAllPostsAsync(bool includeDrafts = false);
    Task<PostMeta?> GetPostBySlugAsync(string slug);
    Task<List<PostMeta>> GetPostsByCategoryAsync(string category);
    Task<List<PostMeta>> GetPostsByTagAsync(string tag);
    Task<List<IGrouping<int, PostMeta>>> GetArchiveAsync();
}