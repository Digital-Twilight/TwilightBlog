using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TwilightBlog_Core.Configuration;
using TwilightBlog_Core.Interfaces;
using TwilightBlog_Core.Models;

namespace TwilightBlog_Core.Services;

public class AnixartService : IAnixartService
{
    private readonly HttpClient _http;
    private readonly AnixartConfig _config;
    private List<AnimeTitle>? _cache;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AnixartService(HttpClient http, AnixartConfig config)
    {
        _http = http;
        _config = config;
    }

    public async Task<List<AnimeTitle>> GetCompletedAsync()
    {
        if (_cache is not null && DateTime.UtcNow < _cacheExpiry)
            return _cache;

        await _lock.WaitAsync();
        try
        {
            if (_cache is not null && DateTime.UtcNow < _cacheExpiry)
                return _cache;

            List<AnimeTitle> result = new List<AnimeTitle>();
            int page = 0;
            AnixartListResponse? json = null;

            do
            {
                string url = $"https://api-s.anixsekai.com/profile/list/all/3/{page}?sort=1&token={_config.Token}";

                HttpResponseMessage response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    break;

                json = await response.Content.ReadFromJsonAsync<AnixartListResponse>(JsonOptions);

                if (json is null || json.Code != 0 || json.Content is null)
                    break;

                foreach (AnixartReleaseItem item in json.Content)
                {
                    AnimeTitle title = MapToAnimeTitle(item);
                    if (_config.Comments.TryGetValue(item.Id.ToString(), out string? comment))
                        title.UserComment = comment;
                    result.Add(title);
                }

                page++;

            } while (json.CurrentPage < json.TotalPageCount);

            _cache = result;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(_config.CacheMinutes);

            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static AnimeTitle MapToAnimeTitle(AnixartReleaseItem item) => new()
    {
        Id = item.Id,
        TitleRu = item.TitleRu ?? "",
        TitleOriginal = item.TitleOriginal ?? "",
        Image = item.Image,
        Description = item.Description,
        Year = int.TryParse(item.Year, out int y) ? y : null,
        Genres = item.Genres?.Split(',').Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g)).ToList() ?? [],
        Category = item.Category is null ? null : new AnimeCategory
        {
            Id = item.Category.Id,
            Name = item.Category.Name ?? ""
        },
        Status = item.Status is null ? null : new AnimeStatus
        {
            Id = item.Status.Id,
            Name = item.Status.Name ?? ""
        },
        Grade = item.Grade,
        EpisodesReleased = item.EpisodesReleased,
        EpisodesTotal = item.EpisodesTotal
    };

    // ===== JSON модели =====

    private class AnixartListResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("content")]
        public List<AnixartReleaseItem>? Content { get; set; }

        [JsonPropertyName("total_page_count")]
        public int TotalPageCount { get; set; }

        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }
    }

    private class AnixartReleaseItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title_ru")]
        public string? TitleRu { get; set; }

        [JsonPropertyName("title_original")]
        public string? TitleOriginal { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("year")]
        public string? Year { get; set; }

        [JsonPropertyName("genres")]
        public string? Genres { get; set; }

        [JsonPropertyName("category")]
        public AnixartCategory? Category { get; set; }

        [JsonPropertyName("status")]
        public AnixartStatus? Status { get; set; }

        [JsonPropertyName("grade")]
        public double? Grade { get; set; }

        [JsonPropertyName("episodes_released")]
        public int? EpisodesReleased { get; set; }

        [JsonPropertyName("episodes_total")]
        public int? EpisodesTotal { get; set; }
    }

    private class AnixartCategory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private class AnixartStatus
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}