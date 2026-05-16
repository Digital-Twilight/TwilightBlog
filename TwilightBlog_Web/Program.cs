using TwilightBlog_Core.Configuration;
using TwilightBlog_Core.Interfaces;
using TwilightBlog_Core.Services;
using TwilightBlog_Web.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Подключаем все json-конфиги из папки configs/
string configsPath = Path.Combine(builder.Environment.ContentRootPath, "..", "configs");

builder.Configuration
    .AddJsonFile(Path.Combine(configsPath, "blog.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "pages.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "sakura.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "wallpaper.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "player.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "soundcloud.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "steam.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "anixart.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configsPath, "devices.json"), optional: false, reloadOnChange: true);

// Регистрируем конфиги через IOptionsMonitor<T>
builder.Services.Configure<BlogConfig>(
    builder.Configuration.GetSection(nameof(BlogConfig)));
builder.Services.Configure<PagesConfig>(
    builder.Configuration.GetSection(nameof(PagesConfig)));
builder.Services.Configure<SakuraConfig>(
    builder.Configuration.GetSection(nameof(SakuraConfig)));
builder.Services.Configure<WallpaperConfig>(
    builder.Configuration.GetSection(nameof(WallpaperConfig)));
builder.Services.Configure<PlayerConfig>(
    builder.Configuration.GetSection(nameof(PlayerConfig)));
builder.Services.Configure<SoundCloudConfig>(
    builder.Configuration.GetSection(nameof(SoundCloudConfig)));
builder.Services.Configure<SteamConfig>(
    builder.Configuration.GetSection(nameof(SteamConfig)));
builder.Services.Configure<AnixartConfig>(
    builder.Configuration.GetSection(nameof(AnixartConfig)));
builder.Services.Configure<DevicesConfig>(
    builder.Configuration.GetSection(nameof(DevicesConfig)));

// Markdown сервис
string postsPath = Path.Combine(builder.Environment.ContentRootPath, "posts");
builder.Services.AddSingleton<IMarkdownService, MarkdownService>(sp => new MarkdownService(postsPath));

// HTTP клиенты
builder.Services.AddHttpClient("Steam", client =>
{
    client.BaseAddress = new Uri("https://api.steampowered.com/");
});
builder.Services.AddHttpClient("SoundCloud", client =>
{
    client.BaseAddress = new Uri("https://api.soundcloud.com/");
});

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();