using Microsoft.AspNetCore.Mvc;
using TagLib;

namespace TwilightBlog_Web.Controllers;

[ApiController]
[Route("api/music")]
public class MusicController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private static readonly Dictionary<string, TrackMetadata> _cache = [];

    public MusicController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("tracks")]
    public IActionResult GetTracks()
    {
        string musicPath = Path.Combine(_env.WebRootPath, "music");

        if (!Directory.Exists(musicPath))
            return Ok(Array.Empty<TrackMetadata>());

        List<TrackMetadata> files = Directory.GetFiles(musicPath, "*.mp3")
            .OrderBy(f => f)
            .Select(f => GetMetadata(f))
            .ToList();

        return Ok(files);
    }

    [HttpGet("cover/{fileName}")]
    public IActionResult GetCover(string fileName)
    {
        string musicPath = Path.Combine(_env.WebRootPath, "music");
        string filePath = Path.Combine(musicPath, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        try
        {
            using TagLib.File file = TagLib.File.Create(filePath);
            IPicture? picture = file.Tag.Pictures.FirstOrDefault();

            if (picture is null)
                return NotFound();

            return File(picture.Data.Data, picture.MimeType);
        }
        catch
        {
            return NotFound();
        }
    }

    private TrackMetadata GetMetadata(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        if (_cache.TryGetValue(fileName, out TrackMetadata? cached))
            return cached;

        try
        {
            using TagLib.File file = TagLib.File.Create(filePath);
            Tag tag = file.Tag;

            TrackMetadata metadata = new TrackMetadata
            {
                Url = "/music/" + Uri.EscapeDataString(fileName),
                Title = !string.IsNullOrEmpty(tag.Title)
                    ? tag.Title
                    : Path.GetFileNameWithoutExtension(fileName),
                Artist = tag.FirstPerformer ?? "",
                Album = tag.Album ?? "",
                Duration = (int)file.Properties.Duration.TotalSeconds,
                HasCover = file.Tag.Pictures.Length > 0,
                FileName = fileName
            };

            _cache[fileName] = metadata;
            return metadata;
        }
        catch
        {
            return new TrackMetadata
            {
                Url = "/music/" + Uri.EscapeDataString(fileName),
                Title = Path.GetFileNameWithoutExtension(fileName),
                Artist = "",
                Album = "",
                Duration = 0,
                HasCover = false,
                FileName = fileName
            };
        }
    }

    public class TrackMetadata
    {
        public string Url { get; set; } = "";
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public int Duration { get; set; }
        public bool HasCover { get; set; }
        public string FileName { get; set; } = "";
    }
}