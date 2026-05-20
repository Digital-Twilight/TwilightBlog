using TwilightBlog_Core.Models;

namespace TwilightBlog_Core.Interfaces;

public interface ISteamService
{
    Task<List<SteamGame>> GetOwnedGamesAsync();
}