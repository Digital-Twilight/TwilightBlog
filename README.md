# TwilightBlog

Personal blog template built with Blazor Server — Markdown posts, anime & games tracking, music player, sakura animation and wallpapers.

## Features

- 📝 Markdown posts with Frontmatter (title, tags, category, draft, description)
- 🗂 Archive page with chronological history of posts and site updates
- 🌸 Animated sakura petals
- 🖼 Wallpaper support with crossfade slideshow, separate sets for light/dark theme
- 🎵 Music player
- 🎌 Anime page via Anixart API (completed list with personal comments)
- 🎮 Games page via Steam API (played games sorted by playtime, with comments)
- 🖥 Devices page (fully config-driven)
- 🌙 Light/dark theme toggle, persisted in localStorage
- ⚙️ Everything configured via separate JSON files in `configs/`
- 🔄 Hot reload for posts and configs without restart

## Tech Stack

- [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/) (.NET 9)
- [Markdig](https://github.com/xoofx/markdig) — Markdown rendering
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) — Frontmatter parsing

## Project Structure
```
TwilightBlog/
├── TwilightBlog.sln
├── configs/                  # All configuration files
│   ├── blog.json             # Blog meta, author, social links
│   ├── pages.json            # Enable/disable pages
│   ├── wallpaper.json        # Wallpaper sets per theme
│   ├── sakura.json           # Sakura animation settings
│   ├── player.json           # Music player settings
│   ├── anixart.json          # Anixart API credentials and comments
│   ├── steam.json            # Steam API credentials and comments
│   └── devices.json          # Devices list
├── TwilightBlog_Web/
│   ├── Components/
│   │   ├── Layout/           # MainLayout, ThemeToggle, WallpaperManager
│   │   └── Pages/            # Home, Post, Archive, Anime, Games, Devices
│   ├── posts/                # Markdown post files
│   └── wwwroot/              # Static assets, CSS, JS
└── TwilightBlog_Core/
    ├── Configuration/        # Config POCOs
    ├── Interfaces/           # Service interfaces
    ├── Models/               # PostMeta, AnimeTitle, SteamGame, DeviceItem
    └── Services/             # MarkdownService, AnixartService, SteamService
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Setup

1. Clone the repository:
```bash
git clone https://github.com/Digital-Twilight/TwilightBlog.git
cd TwilightBlog
```

2. Copy config templates and fill in your credentials:
```bash
copy configs\anixart.example.json configs\anixart.json
copy configs\steam.example.json configs\steam.json
```

3. Edit `configs\blog.json` with your info.

4. Add your posts to `TwilightBlog_Web\posts\` as `.md` files.

5. Run:
```bash
dotnet run --project TwilightBlog_Web\TwilightBlog_Web.csproj
```

## Post Format

```markdown
---
Title: My Post
Published: 2026-05-21
Tags: [Blazor, C#]
Category: Dev
Draft: false
ArchiveType: post
Description: Short description shown on the home page.
---

Post content in Markdown...
```

`ArchiveType` can be `post`, `feature`, `update` or `fix`.

## Configuration

All configs support hot reload — changes apply without restarting the server.

| File | Purpose |
|------|---------|
| `blog.json` | Blog title, author, description, social links |
| `pages.json` | Enable/disable pages (`true`/`false`) |
| `wallpaper.json` | Wallpaper images per theme, interval, blur, dimming |
| `sakura.json` | Petal count, speed, size, color, opacity |
| `player.json` | Volume, autoplay, shuffle |
| `anixart.json` | Anixart token, user ID, per-title comments |
| `steam.json` | Steam API key, Steam ID, hidden apps, per-game comments |
| `devices.json` | Devices list with categories, descriptions and links |

## License

This project is licensed under the GNU General Public License v3.0 — see [LICENSE](LICENSE) for details.