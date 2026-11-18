# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. General workflow rules (no implicit branches or worktrees, the 300-line limit for this file) are in the owner's personal `~/.claude/CLAUDE.md`.

## Overview

A self-hosted Discord bot that runs as a **Windows WPF desktop app** (the window shows the live log). It is single-instance, Windows-only (`net11.0-windows`, x64), and talks to SQL Server through EF Core. Most features are aimed at K-pop communities: bias lists and the bias game, Last.fm, music playback, Twitch notifications, birthdays, reminders, weekly polls, self-roles and custom commands.

## Solution layout

- `Discord Bot GUI/`: the bot itself (WPF app, root namespace `Discord_Bot`, assembly `Discord Bot`).
- `DatabaseORM/`: the database layer as a plain `net11.0` class library (no WPF or Discord.Net), referenced by the bot and modeled on the project of the same name in KecskeProjectsSite. Its namespaces are `DatabaseORM.*`. Keep its .NET version in step with the bot's.
- `LastFmApi/`: a small RestSharp/Newtonsoft client for `ws.audioscrobbler.com`, referenced by the bot. Keep its target framework in step with the bot's.
- `MainDB/`: a legacy (non-SDK) SSDT `.sqlproj`, the **source of truth for the database schema**. It builds and publishes only from Visual Studio, not `dotnet build`.
- `Latest_Release.sql`: a full deployment script, but git-ignored and local only. The dev PC's copy dates from 2023, so the source of truth is `MainDB/`.
- `diagram.drawio` / `diagram.png`: the DB diagram.

There is no test project. Linting is done by analyzers during the build (see Linting).

## Commands

```bash
dotnet build "Discord Bot GUI/Discord Bot GUI.csproj"
```

```bash
dotnet run --project "Discord Bot GUI/Discord Bot GUI.csproj"
```

```bash
dotnet format "Discord Bot GUI/Discord Bot GUI.csproj" --verify-no-changes
```

Check every tracked text file against the BOM rule (Git Bash; no output means all match). Keep its patterns in step with the `charset` sections in `.editorconfig`:

```bash
git ls-files --eol | grep -v 'i/-text' | cut -f2 | while IFS= read -r f; do case "$f" in DatabaseORM/Model/*|DatabaseORM/Context/MainDbContext.cs) want=none;; *.cs|*.csproj|*.sln|*.sqlproj|*.xaml|*.resx|*.sql|*.props|*.refactorlog) want=bom;; *) want=none;; esac; [ "$(head -c3 "$f" | od -An -tx1 | tr -d ' \n')" = efbbbf ] && has=bom || has=none; [ "$want" = "$has" ] || echo "expected $want, has $has: $f"; done
```

Build the projects rather than the `.sln`, because the solution includes `MainDB.sqlproj`, which needs Visual Studio/SSDT. Warnings fail the build (`TreatWarningsAsErrors`), so fix them rather than suppressing them. `dotnet format` without `--verify-no-changes` applies every fix it has (whitespace, style and analyzer code fixes such as CA1310), so review its diff. It covers one project at a time (run it for `DatabaseORM/DatabaseORM.csproj` and `LastFmApi/LastFmApi.csproj` too) and also reports IDE0001 and IDE0002, which the build doesn't.

## Linting

- `Directory.Build.props` (repo root) gives every project `AnalysisLevel=latest-recommended` and `EnforceCodeStyleInBuild`, so Visual Studio and `dotnet build` report the same warnings, and `TreatWarningsAsErrors` in every configuration (NuGet audit warnings NU1901–NU1904 stay warnings). `GenerateDocumentationFile` is on only so the build reports IDE0005 (unnecessary usings); CS1591 is off.
- The root `.editorconfig` holds the formatting options (matching the owner's VS settings: a space after casts, `case` block braces not indented), the code style preferences (file-scoped namespaces, no simple `using` statements) and every severity change. All IDE style and code-quality rules are warnings. Private fields are camelCase with no prefix; private constants are PascalCase.
- Beyond `latest-recommended`, CA2000 (dispose objects), CA1063 (dispose pattern), CA1508 (dead conditions, on trial), CA1052 (static holder classes), CA1851 (multiple enumeration) and CA2234 (`Uri` overloads) are on. Disposable types in `Communication/` are sealed and have no finalizers, so whoever creates one disposes it. An internal class that nothing derives from must be sealed (CA1852).
- `BannedSymbols.txt` (repo root, read by `Microsoft.CodeAnalysis.BannedApiAnalyzers` from `Directory.Build.props`) turns project rules into warning RS0030: no `Console`, no `RunMode.Async`, no `Assembly.GetEntryAssembly`, no ImageSharp. Add new project-wide bans there.
- `ImplicitUsings` is on in every project, so don't write `using System;`, `System.Collections.Generic`, `System.Linq`, `System.Threading` or `System.Threading.Tasks` (IDE0005 rejects them).
  - The bot is a WPF project, so its global set lacks `System.IO` and `System.Net.Http`; those stay explicit there.
  - With `System.Threading` global, write `System.Timers.Timer` in full, and alias `System.Diagnostics.ThreadState` (as `Core/Constant.cs` does).
  - The bot doesn't use Windows Forms (`UseWindowsForms` is off). Adding it back would also make `System.Windows.Forms` and `System.Drawing` global and clash with WPF names such as `Application`.
- Nullable reference types are on in every project (`<Nullable>enable</Nullable>`). The JSON models (`Services/Models`, `LastFmApi/Models`) and `MainDbContext.cs` are marked `generated_code` in the root `.editorconfig`, which keeps them nullable-oblivious and outside the analyzers. The scaffolded `DatabaseORM/Model` files are checked for nullable on purpose (see "Database changes"); only IDE0005 and IDE0028 are off there. Check for null where a value comes from, and give callees non-nullable parameters unless null is a real option. A caller that has checked a value the compiler lost track of (a nested property, a combined condition) passes it with `!`. A `*Resource` property is nullable when its database column is nullable or the mapping can produce null; otherwise it gets `= ""` / `= []`.
- Ternaries (`?:`) are for simple value choices. IDE0045 and IDE0046 are off (the `dotnet_style_prefer_conditional_expression_over_*` options), so an `if`/`else` that only assigns or returns can stay one. Use `if`/`else` when the branches perform actions, contain an `await` of different operations, throw, or would nest another `?:`. Mapster mappings are the exception: they are expression trees, which don't allow `?.`, so their null checks stay ternaries.
- Silencing a rule needs a written reason, either as a comment in `.editorconfig` or in the project's `GlobalSuppressions.cs` (`Properties/` in the bot, the project root in `DatabaseORM`). CA1305 excludes integer `ToString`/`Parse` only; use `CultureInfo.InvariantCulture` for doubles, dates and `string.Format`.
- The `DatabaseORM.Repository` and `DatabaseORM.Service` namespaces are exempt from CA1309, CA1310 and CA1862 (`DatabaseORM/GlobalSuppressions.cs`), because EF translates the lambdas passed to repositories into SQL and can't translate `StringComparison` overloads. Never apply those code fixes there (Code Cleanup will offer them if the suppressions go).
- The path-specific sections (generated code, the model folders) are in the root `.editorconfig` too. A nested `.editorconfig` must not get `root = true`, or the root file stops applying below it.
- **Line endings are CRLF** in the working copy (`.gitattributes` `* text=auto eol=crlf`, `.editorconfig` `end_of_line = crlf`); the repository stores LF. An ending-only change shows as modified with an empty diff, LF next to comments fails IDE0055, and `dotnet format --verify-no-changes` reports any LF line (ENDOFLINE). Claude's Write tool and Git Bash `sed -i`, `awk` and redirects write LF: use Edit for existing files and `sed -b` in Bash, run `unix2dos -q <file>` on anything written another way, and check with `git ls-files --eol` (Git Bash's `grep $'\r'` is unreliable). The investigation (which tool writes which ending) is in `git show 483c1fa:docs/improvement-notes.md`, section 9.
- **The UTF-8 BOM follows the tool that creates each kind of file** (`charset` in `.editorconfig`; the investigation is in section 11 of the same old version of the notes):
  - With a BOM: C#, project, solution, XAML, resx and SQL files, and `Directory.Build.props`, as Visual Studio, `dotnet new` and SSDT write them.
  - Without one: the EF-scaffolded `DatabaseORM/Model/` files and `MainDbContext.cs` (EF Core 11 writes none), and everything else (Markdown, JSON, txt, config files).
  - `dotnet format` enforces it for C# (CHARSET); check the other files with the command under Commands.
  - Claude's Edit and Write tools keep an existing BOM, but a new file from Write gets one only if its content starts with U+FEFF.
  - Never write repository files with PowerShell `Set-Content` or `Out-File` without `-Encoding`: Windows PowerShell 5.1 writes ANSI (windows-1250 here), which corrupts non-ASCII text.

## Runtime requirements

- **Config:** read from `Assets/config.json` **in the current working directory** (with `reloadOnChange`, so edits apply without a restart). Classes inject `IConfig` (`Interfaces/`), registered transient as `Config`, so a new setting needs a property on both. The DbContext reads its connection string through `IConfig` too, so replacing `IConfig` in a test also replaces the database. `Discord Bot GUI/Core/Configuration/config.json` is only a key template and is **not** copied to the output. Never put real values in the tracked template. If the file is missing, `Config`'s constructor writes an empty one, and the bot refuses to log in until `token` is set. A missing value reads as `""`, `[]` or an empty dictionary, never null (the binder turns an empty JSON object into null, so `Config` covers that too).
- **Native/CLI dependencies** in `External Files/` are copied to the output root and invoked through `cmd.exe /C`: `yt-dlp.exe` piped into `ffmpeg.exe` for audio, `twitch.exe` (Twitch CLI) for tokens and user lookup, and `instaloader.exe` (Instagram, currently disabled). `opus.dll`, `libsodium.dll` and `libdave.dll` are needed for voice. Discord's DAVE E2EE is enabled (`EnableVoiceDaveEncryption`).
- **Headless Chrome:** PuppeteerSharp downloads Chrome on first use (`BrowserService`, a singleton browser shared by the scrapers). `headless_browser` in the config toggles it.

## External services

| Service | Where | Notes |
|---|---|---|
| Discord | Discord.Net (`Startup.cs`, `BotMain.cs`) | Only the gateway intents in use (each is commented in `Startup.cs`); add one only when handling its events |
| SQL Server | EF Core, `sql_connection_string` | Schema from `MainDB/` |
| Last.fm | `LastFmApi` project + `Services/LastFmAPI.cs` | `lastfm_artist_input_replacement` in the config rewrites artist names. Check `IsSuccess` before reading a `GenericResponseItem`'s `Response` |
| Spotify | SpotifyAPI.Web | Client credentials from the config |
| YouTube Data API | Google.Apis.YouTube.v3 | **Several keys rotated** by quota through `Global.YoutubeApiKeys`; reset daily by `YoutubeAPI.KeyReset` |
| Twitch | TwitchLib `LiveStreamMonitor` (on its own thread from `App.xaml.cs`) + `twitch.exe` CLI | |
| MusicBrainz | REST | Artist lookups |
| dbkpop.com / kprofiles.com | Puppeteer scraping (`KpopDbScraper`, `BiasScrapingProcessor`) | Weekly idol data refresh |
| Twitter/X, Instagram | Puppeteer scraping / GraphQL | **Both embeds are currently disabled.** `UserTwitterScraperCommands.cs` is fully commented out, and the Instagram embed call is commented out in `CommandHandler`. The `TwitterScraper`/`InstaScraper` services, their DI registrations and the `enable_*_embed` config keys remain |
| Word of the Day | `wotd.transparent.com` RSS | |

Base URLs live in `Core/Constant.cs`.

## Architecture

**Startup:** `App.OnStartup` → `Startup.Run()` (builds the DI container by hand from `Startup.ConfigureServices`, which tests reuse; **every new feature, processor or external service must be registered there**, and every new DB service or repository in `DatabaseStartup.ConfigureDatabaseServices` in `DatabaseORM`, which `ConfigureServices` calls) → shows `BotWindow` → starts the Twitch monitor thread → `BotMain.RunBotAsync()` logs in and wires up `CommandHandler` and `InteractionHandler`.

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

**Layers (inside `Discord Bot GUI/`, then `DatabaseORM/`):**
- `Commands/` and `Interactions/` → Discord entry points; they call DB services, features, and processors.
- `Features/`: reusable units of behavior derived from `BaseFeature`; call them with `Run(...)` and implement `ExecuteCoreLogicAsync()`, which returns `bool`. Registered transient.
- `Processors/`: build output (Discord embeds, Magick.NET images and GIFs, message text). `EmbedProcessors` are mostly static.
- `Services/`: wrappers for external APIs, scrapers and CLI tools behind `Interfaces/Services/`.
- `DatabaseORM/Service/` (transient; `BaseService` gives `IMapper`, `IBotLogger`, `ServerCache`) → `DatabaseORM/Repository/` (scoped; `GenericRepository<T>` plus entity-specific queries) → `Context/MainDbContext`. The bot sees only the `I*Service` interfaces (`Interface/Service/`), resources, enums, `Communication/` DTOs, `DatabaseMapperConfig`, `DatabaseStartup` and `IBotLogger`. The repositories, concrete services, `ServerCache` and the DB tools are `internal`, so the bot can only go through the services. DB services catch exceptions, log them, and return `DbProcessResultEnum`, `null` (declared `Task<T?>`) for a single result, or an empty list for a list, instead of throwing. Callers check single results for null; `GetCurrentServerAsync()` in the base classes throws instead, because every guild gets a row (`CommandHandler`), so a missing one means the database failed.
- `DatabaseORM/Model/` are EF entities, and `DatabaseORM/Resources/` are the DTOs that services return. **Commands should work with `*Resource` types, not EF models.**
- `Communication/`: in-memory DTOs, including modal definitions. The DTOs the DB services take are in `DatabaseORM/Communication/`, such as the `*EditData` classes that the edit modals map to with `ToEditData()`.

**Mapping:** Mapster, configured in `DatabaseORM/Mapping/DatabaseMapperConfig.cs` (EF models and scraped data) and `Core/MapperConfig.cs` (resources to Discord types). `Startup` registers both and compiles them, so an invalid mapping fails fast. The project moved off AutoMapper. Mapster does **not** null-guard navigation chains or convert `string` → `ulong` on its own, so write these mappings out explicitly. `ShallowCopyForSameType(true)` is on to avoid deep-cloning EF navigation graphs.

**Discord IDs** are stored as `string` (varchar) in the DB and exposed as `ulong` on resources. Compare against `id.ToString()` in EF queries.

**Shared state:**
- `Core/Global.cs` holds static `ConcurrentDictionary`s for per-guild audio state (`ServerAudioResources`), running bias games (`BiasGames`) and YouTube key quotas. This state is lost on restart.
- `ServerCache` (`DatabaseORM/Caching/`) is a 50-entry cache of `ServerResource`. Any DB write that changes server settings **must** call `cache.RemoveCachedEntityManually(serverId)`, or reads go stale.

**Logging:** inject `IBotLogger` (`DatabaseORM/Interface/`, so the DB layer can log through it: `Log`, `Error(location, ex)`, `Warning`, `Query`, `Client`, `Audio`) and never use `Console`. By convention, `location` is `"FileName.cs MethodName"`. One `BotLogger` instance is registered under both types; only `Startup`, `App` (`LogToFile`) and `BotWindow` use the concrete type. The window shows logs by subscribing to `BotLogger.LogAdded`, so the logger has no WPF dependency. Logs are queued and written to `Logs/` in the working directory every minute and at shutdown.

## Database changes

The schema is **database-first**:
1. Edit the table definitions in `MainDB/Tables/*.sql`.
2. Generate the deployment or patch script with SSDT publish (profiles: `Local_Debug`, `Local_Release`, `Main_Release`). Applied patches are archived as `MainDB/DB Patch/Patched/Patch_N.sql`. A full script can be generated too, but `Latest_Release.sql` is git-ignored, so it only exists locally.
3. Re-scaffold `DatabaseORM/Model/*` and `DatabaseORM/Context/MainDbContext.cs` from the Package Manager Console: `Scaffold-DbContext "<connection string>" -Project "DatabaseORM" -StartupProject "DatabaseORM" Microsoft.EntityFrameworkCore.SqlServer -f -NoOnConfiguring -OutputDir Model -ContextDir Context -Context MainDbContext`. The full local command is in the git-ignored `DatabaseORM/Context/update-scaffold.txt`. Don't hand-edit or hand-style the scaffolded files. With nullable on, the scaffold writes `?` for nullable columns and optional navigations instead of `.IsRequired()`, so EF and the compiler read required columns from those annotations. That is why the models are not marked as generated code (there a `?` would be error CS8669). In an `Include` path through an optional navigation, use `!` (`wp => wp.OptionPreset!.WeeklyPollOptions`): EF only reads the path.
4. Update the matching `Resource`, Mapster mapping, repository and service.

Lookup data such as `ChannelType` rows is seeded from `MainDB/Post-deployment/`. Adding a `ChannelTypeEnum` value needs a matching insert there and its string forms in `ChannelTypeEnumExtension` (`DatabaseORM/Enums/ChannelTypeEnum.cs`).

## Gotchas

- Image work uses **Magick.NET** (Q16, so channels are 0–65535) through `Tools/ImageTools.cs`, `Tools/MagickTools.cs`, `Tools/Extensions/MagickImageExtension.cs` and `Communication/TextFont.cs`. SixLabors ImageSharp was removed because its fixed versions need a paid license, so don't add it back. `DrawText` treats (x, y) as the top left of the text, and `TextFont` finds per-user installed fonts such as "Bot Font" through the registry.
- The Chrome build used by PuppeteerSharp matters for scraping. Per a comment in `BrowserService`, video responses changed after Chrome 117.
