using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TwilightBlog_Core.Configuration;
using TwilightBlog_Core.Interfaces;
using TwilightBlog_Core.Models;

namespace TwilightBlog_Core.Services;

public class SteamService : ISteamService
{
    private readonly HttpClient _http;
    private readonly SteamConfig _config;

    private List<SteamGame>? _cache;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SteamService(HttpClient http, SteamConfig config)
    {
        _http = http;
        _config = config;
    }

    public async Task<List<SteamGame>> GetOwnedGamesAsync()
    {
        if (_cache is not null && DateTime.UtcNow < _cacheExpiry)
            return _cache;

        await _lock.WaitAsync();
        try
        {
            if (_cache is not null && DateTime.UtcNow < _cacheExpiry)
                return _cache;

            string url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/" +
                      $"?key={_config.ApiKey}" +
                      $"&steamid={_config.SteamId}" +
                      $"&include_appinfo=true" +
                      $"&include_played_free_games=true";

            HttpResponseMessage response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return [];

            SteamResponse? json = await response.Content.ReadFromJsonAsync<SteamResponse>();
            if (json?.Response?.Games is null)
                return [];

            List<SteamGame> games = json.Response.Games
                .Where(g => g.PlaytimeMinutes > 0)
                .Where(g => !_config.HiddenAppIds.Contains(g.AppId))
                .OrderByDescending(g => g.PlaytimeMinutes)
                .Select(g =>
                {
                    SteamGame game = new SteamGame
                    {
                        AppId = g.AppId,
                        Name = g.Name ?? "",
                        PlaytimeMinutes = g.PlaytimeMinutes,
                        IconUrl = !string.IsNullOrEmpty(g.ImgIconUrl)
                            ? $"https://media.steampowered.com/steamcommunity/public/images/apps/{g.AppId}/{g.ImgIconUrl}.jpg"
                            : null,
                        HeaderUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{g.AppId}/header.jpg"
                    };

                    if (_config.Comments.TryGetValue(g.AppId.ToString(), out string? comment))
                        game.UserComment = comment;

                    return game;
                })
                .ToList();

            _cache = games;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(_config.CacheMinutes);

            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }

    // ===== JSON модели =====

    private class SteamResponse
    {
        [JsonPropertyName("response")]
        public SteamResponseInner? Response { get; set; }
    }

    private class SteamResponseInner
    {
        [JsonPropertyName("games")]
        public List<SteamGameItem>? Games { get; set; }
    }

    private class SteamGameItem
    {
        [JsonPropertyName("appid")]
        public int AppId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("playtime_forever")]
        public int PlaytimeMinutes { get; set; }

        [JsonPropertyName("img_icon_url")]
        public string? ImgIconUrl { get; set; }
    }
}