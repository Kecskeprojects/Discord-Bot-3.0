# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Keep this file under 300 lines. When you add to it, tighten or remove outdated parts so it stays under that limit.

## Workflow

The owner is the only contributor and rarely works on two things at once. Work directly in the main checkout on the current branch, and create a new branch or git worktree only when explicitly asked.

## Overview

A self-hosted Discord bot that runs as a **Windows WPF desktop app** (the window shows the live log). It is single-instance, Windows-only (`net11.0-windows`, x64), and talks to SQL Server through EF Core. Most features are aimed at K-pop communities: bias lists and the bias game, Last.fm, music playback, Twitch notifications, birthdays, reminders, weekly polls, self-roles and custom commands.

## Solution layout

- `Discord Bot GUI/`: the bot itself (WPF app, root namespace `Discord_Bot`, assembly `Discord Bot`).
- `LastFmApi/`: a small RestSharp/Newtonsoft client for `ws.audioscrobbler.com`, referenced by the bot. Keep its target framework in step with the bot's.
- `MainDB/`: a legacy (non-SDK) SSDT `.sqlproj`, the **source of truth for the database schema**. It builds and publishes only from Visual Studio, not `dotnet build`.
- `Latest_Release.sql`: the generated full deployment script for the current schema.
- `diagram.drawio` / `diagram.png`: the DB diagram.

There is no test project and no linter configured.

## Commands

```bash
dotnet build "Discord Bot GUI/Discord Bot GUI.csproj"
```

```bash
dotnet run --project "Discord Bot GUI/Discord Bot GUI.csproj"
```

Build the projects rather than the `.sln`, because the solution includes `MainDB.sqlproj`, which needs Visual Studio/SSDT. The goal is a build with no warnings, so fix new warnings rather than ignoring them.

## Runtime requirements

- **Config:** read from `Assets/config.json` **in the current working directory** (with `reloadOnChange`, so edits apply without a restart; `Config` is registered transient on purpose). `Discord Bot GUI/Core/Configuration/config.json` is only a key template and is **not** copied to the output. Never put real values in the tracked template. If the file is missing, `Config`'s constructor writes an empty one, and the bot refuses to log in until `token` is set.
- **Native/CLI dependencies** in `External Files/` are copied to the output root and invoked through `cmd.exe /C`: `yt-dlp.exe` piped into `ffmpeg.exe` for audio, `twitch.exe` (Twitch CLI) for tokens and user lookup, and `instaloader.exe` (Instagram, currently disabled). `opus.dll`, `libsodium.dll` and `libdave.dll` are needed for voice. Discord's DAVE E2EE is enabled (`EnableVoiceDaveEncryption`).
- **Headless Chrome:** PuppeteerSharp downloads Chrome on first use (`BrowserService`, a singleton browser shared by the scrapers). `headless_browser` in the config toggles it.

## External services

| Service | Where | Notes |
|---|---|---|
| Discord | Discord.Net (`Startup.cs`, `BotMain.cs`) | Only the gateway intents in use (each is commented in `Startup.cs`); add one only when handling its events |
| SQL Server | EF Core, `sql_connection_string` | Schema from `MainDB/` |
| Last.fm | `LastFmApi` project + `Services/LastFmAPI.cs` | `lastfm_artist_input_replacement` in the config rewrites artist names |
| Spotify | SpotifyAPI.Web | Client credentials from the config |
| YouTube Data API | Google.Apis.YouTube.v3 | **Several keys rotated** by quota through `Global.YoutubeApiKeys`; reset daily by `YoutubeAPI.KeyReset` |
| Twitch | TwitchLib `LiveStreamMonitor` (on its own thread from `App.xaml.cs`) + `twitch.exe` CLI | |
| MusicBrainz | REST | Artist lookups |
| dbkpop.com / kprofiles.com | Puppeteer scraping (`KpopDbScraper`, `BiasScrapingProcessor`) | Weekly idol data refresh |
| Twitter/X, Instagram | Puppeteer scraping / GraphQL | **Both embeds are currently disabled.** `UserTwitterScraperCommands.cs` is fully commented out, and the Instagram embed call is commented out in `CommandHandler`. The `TwitterScraper`/`InstaScraper` services, their DI registrations and the `enable_*_embed` config keys remain |
| Word of the Day | `wotd.transparent.com` RSS | |

Base URLs live in `Core/Constant.cs`.

## Architecture

**Startup:** `App.OnStartup` → `Startup.Run()` (builds the whole DI container by hand; **every new service, feature, processor or repository must be registered there**) → shows `BotWindow` → starts the Twitch monitor thread → `BotMain.RunBotAsync()` logs in and wires up `CommandHandler` and `InteractionHandler`.

**Scheduler:** a 1-minute `System.Timers.Timer` in `App.xaml.cs` reconnects the client if it is logged out, runs `ReminderFeature` and `UnmuteFeature` every tick, runs `BirthdayFeature`, `WeeklyPollFeature` and the YouTube key reset at 06:00 UTC, scrapes bias data on Mondays, and clears the window log at midnight UTC. Times are **UTC**.

**Two command systems coexist:**
1. **Text prefix commands** (`Commands/`, Discord.Commands `ModuleBase` via `BaseCommand`) are triggered by `!` or `.`. This is where nearly all commands are. `CommandHandler.ProcessMessageAsync` also:
   - auto-inserts any guild it sees into the DB,
   - treats messages in a `RoleText` channel as `+role`/`-role` self-role requests and **deletes the message**,
   - otherwise runs `EasterEggFeature`,
   - on `UnknownCommand`, falls back to DB-defined custom commands (`CustomCommandFeature`).
2. **Interactions** (`Interactions/`, `BaseInteraction`) are **only** `[ComponentInteraction]` and `[ModalInteraction]` handlers for buttons, selects and modals sent by text commands (bias editing, bias game, help paging, weekly-poll editing). There are no slash commands, and nothing registers any.

Each command or interaction runs in its own DI scope (`services.CreateScope()`) with `RunMode.Sync`. The handlers dispatch it in the background (`Task.Run` / fire-and-forget), so the gateway isn't blocked. `RunMode.Async` was tried and rejected, so keep this setup. `BotMain` registers handlers once; the reconnect timer only logs in again.

Every interaction must be acknowledged within 3 seconds, including early returns (use `DeferAsync()`). In error paths use `RespondOrFollowupAsync`, because a plain `RespondAsync` throws after a defer. Open findings and ideas are tracked in `docs/improvement-notes.md`.

**Command conventions:**
- Module-level `[Remarks("User" | "Admin" | "Owner")]` must match `CommandLevelEnum`. `HelpCommands` filters by it, so a module with the wrong value disappears from help. `[Summary]` is the help text.
- Permissions use `[RequireOwner]`, `[RequireUserPermission]` and `[RequireContext]` attributes. Most user commands also start with `if (!await IsCommandAllowedAsync(ChannelTypeEnum.X)) return;`. Servers can restrict commands to channels of a given `ChannelTypeEnum`, and if a server has set no channel of that type, the command is allowed (`allowLackOfType`).
- Multiple arguments are usually split on `>` with `GetParametersBySplit` and lowercased by default.

**Layers (inside `Discord Bot GUI/`):**
- `Commands/` and `Interactions/` → Discord entry points; they call DB services, features, and processors.
- `Features/`: reusable units of behavior derived from `BaseFeature`; call them with `Run(...)` and implement `ExecuteCoreLogicAsync()`, which returns `bool`. Registered transient.
- `Processors/`: build output (Discord embeds, Magick.NET images and GIFs, message text). `EmbedProcessors` are mostly static.
- `Services/`: wrappers for external APIs, scrapers and CLI tools behind `Interfaces/Services/`.
- `Database/DBServices/` (transient; `BaseService` gives `IMapper`, `BotLogger`, `ServerCache`) → `Database/DBRepositories/` (scoped; `GenericRepository<T>` plus entity-specific queries) → `MainDbContext`. DB services catch exceptions, log them, and return `DbProcessResultEnum` or `null` instead of throwing.
- `Database/Models/` are EF entities, and `Resources/` are the DTOs that services return. **Commands should work with `*Resource` types, not EF models.**
- `Communication/`: in-memory DTOs, including modal definitions.

**Mapping:** Mapster, configured in `Core/MapperConfig.cs` and compiled at startup, so an invalid mapping fails fast. The project moved off AutoMapper. Mapster does **not** null-guard navigation chains or convert `string` → `ulong` on its own, so write these mappings out explicitly. `ShallowCopyForSameType(true)` is on to avoid deep-cloning EF navigation graphs.

**Discord IDs** are stored as `string` (varchar) in the DB and exposed as `ulong` on resources. Compare against `id.ToString()` in EF queries.

**Shared state:**
- `Core/Global.cs` holds static `ConcurrentDictionary`s for per-guild audio state (`ServerAudioResources`), running bias games (`BiasGames`) and YouTube key quotas. This state is lost on restart.
- `ServerCache` is a 50-entry cache of `ServerResource`. Any DB write that changes server settings **must** call `cache.RemoveCachedEntityManually(serverId)`, or reads go stale.

**Logging:** use `BotLogger` (`Log`, `Error(location, ex)`, `Warning`, `Query`, `Client`, `Audio`) and never `Console`. By convention, `location` is `"FileName.cs MethodName"`. Logs are flushed to `Logs/` in the working directory.

## Database changes

The schema is **database-first**:
1. Edit the table definitions in `MainDB/Tables/*.sql`.
2. Generate the deployment or patch script with SSDT publish (profiles: `Local_Debug`, `Local_Release`, `Main_Release`). Applied patches are archived as `MainDB/DB Patch/Patched/Patch_N.sql`, and `Latest_Release.sql` holds the full script.
3. Update `Database/Models/*` and `MainDbContext.cs` to match. These are scaffolded and marked `generated_code` in `Database/.editorconfig`, so don't hand-style them.
4. Update the matching `Resource`, Mapster mapping, repository and service.

Lookup data such as `ChannelType` rows is seeded from `MainDB/Post-deployment/`. Adding a `ChannelTypeEnum` value needs a matching insert there and its string forms in `ChannelTypeEnumExtension`.

## Gotchas

- Image work uses **Magick.NET** (Q16, so channels are 0–65535) through `Tools/ImageTools.cs`, `Tools/MagickTools.cs`, `Tools/Extensions/MagickImageExtension.cs` and `Communication/TextFont.cs`. SixLabors ImageSharp was removed because its fixed versions need a paid license, so don't add it back. `DrawText` treats (x, y) as the top left of the text, and `TextFont` finds per-user installed fonts such as "Bot Font" through the registry.- The Chrome build used by PuppeteerSharp matters for scraping. Per a comment in `BrowserService`, video responses changed after Chrome 117.
