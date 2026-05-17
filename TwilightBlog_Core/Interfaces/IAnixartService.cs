using TwilightBlog_Core.Models;

namespace TwilightBlog_Core.Interfaces;

public interface IAnixartService
{
    Task<List<AnimeTitle>> GetCompletedAsync();
}