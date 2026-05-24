using Microsoft.Extensions.Options;
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

builder.Services.AddHttpClient("Anixart", client =>
{
    client.BaseAddress = new Uri("https://api-s.anixsekai.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "Anixart/8.7.2 (Android)");
    client.DefaultRequestHeaders.Add("Sign", "");
});

builder.Services.AddSingleton<IAnixartService>(sp =>
{
    IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();
    HttpClient http = factory.CreateClient("Anixart");
    AnixartConfig config = sp.GetRequiredService<IOptionsMonitor<AnixartConfig>>().CurrentValue;
    return new AnixartService(http, config);
});

builder.Services.AddSingleton<ISteamService>(sp =>
{
    IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();
    HttpClient http = factory.CreateClient("Steam");
    SteamConfig config = sp.GetRequiredService<IOptionsMonitor<SteamConfig>>().CurrentValue;
    return new SteamService(http, config);
});

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLocalization();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

BlogConfig blogConfig = builder.Configuration
    .GetSection(nameof(BlogConfig))
    .Get<BlogConfig>() ?? new BlogConfig();

System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(blogConfig.Language);

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture),
    SupportedCultures = [culture],
    SupportedUICultures = [culture],
    RequestCultureProviders = []
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();