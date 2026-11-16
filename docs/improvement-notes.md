# Improvement notes

Open work from code-quality investigations, kept so it can be picked up later. Items are removed once they are applied; the git history has the details of earlier work. Last brought up to date 2026-11-16.

## To check in the running bot

Changes from 2026-11-16 that built cleanly but have not been tried live:

- **Shutdown:** close the window. The log should end with "Application closing..." and no error, and the bot should show offline in Discord right away.
- **Twitch token refresh:** after the bot has run for more than a day (the refresh runs about every 24 hours), stream notifications should keep arriving.
- **Last.fm who-knows:** `!lf wk` with no song (uses what you are playing now), with `artist>track` and with an artist only.
- **Weekly poll buttons:** the toggle buttons in the poll and preset edit messages (active, pinned, multiple answers, special preset), and the preset select's "Custom Options".

## 1. Linting follow-ups (investigated 2026-11-15 and 2026-11-16)

The analyzer setup is done and described in CLAUDE.md ("Linting"). Left:

- **CA1508 (dead conditional code) is on trial.** Of its first 5 findings, 4 were right and 1 was a false positive (`AudioPlayFeature.WaitAsync`, suppressed in GlobalSuppressions). Decide whether to keep it once it has run on new code for a while.
- **CA1849 cases worth fixing by hand.** The rule isn't turned on, because half of its 12 findings are false positives (`GetChannel` / `GetRole` are cache lookups in Discord.Net's socket client). The real ones:
  - `CancellationTokenSource.Cancel(false)` runs the cancellation callbacks on the calling thread: `App.xaml.cs`, `BotMain.cs`, `UserVoiceCommands.cs` (3). `CancelAsync` avoids that.
  - `YoutubeStreamService.cs:51` blocks a thread for up to a minute in `Process.WaitForExit(TimeSpan)`. `WaitForExitAsync` with a timeout token avoids that.
- **Optional, explicitness only (no behavior change):**
  - CA1307 (38): spell out `StringComparison` on `Contains` / `Replace` / `EndsWith(char)`, which are already ordinal. None are in the Database namespace.
  - CA1820 (15): `== ""` → `string.IsNullOrEmpty`. Not a pure rename, because `IsNullOrEmpty` also accepts null.

**Decided against (2026-11-16)**, so these don't need another look:
- `Microsoft.VisualStudio.Threading.Analyzers`: 180 warnings in a trial build, mostly VSTHRD200 (`Async` suffix on command methods, which Discord.Net doesn't care about); nothing the current rules miss.
- From `AnalysisLevel=latest-all`:
  - CA2007 `ConfigureAwait`: the handlers run on the thread pool, and in the WPF UI code `ConfigureAwait(false)` would be wrong.
  - CA1515 internal types: Discord.Net only discovers public modules.
  - CA1031 catching `Exception`: the project's convention (handlers log and carry on).
  - CA1062: nullable reference types cover it.
  - CA1002 / CA2227 / CA1819 / CA1034 / CA1815: library API design; Mapster, JSON and Discord.Net (`Embed[]`) work with settable collections and arrays.
  - CA1056 / CA1054 / CA1055: URLs come from the database, JSON and the config as strings.
  - CA1724: type names that are also namespace names (`Log`, `User`, `Owner`, `Data`); renaming churn only.
  - CA5394: `Random` is for games and easter eggs.
  - CA1008: the enums start at 1 on purpose (database IDs, command levels).

## 2. Test project (investigated 2026-11-15)

### Already in good shape

- Every DB service, repository and external API service is behind an interface (`Interfaces/`) and registered through DI in `Startup.cs`, so DB services can be unit-tested now by mocking their repositories.
- `IMapper` can be built for real in tests from `MapperConfig`, the same way `Startup` does.
- A large share of logic sits in static, mostly pure helpers that need no changes to test: `Tools/` (`StringTools`, `CollectionTools`, `NumberTools`, `UrlTools`, `ChannelTypeEnumTools`, `LastFmTools`, `DiscordTools.IsTypeOfChannel`), `SizedDictionary` / `ServerCache`, and the static embed and message processors (assert on the returned `Embed`'s fields).
- `BotLogger` writing to the window is harmless in tests: `Application.Current?` is null, so nothing happens.

### Blockers, in order of value

1. **The clock.** 45 `DateTime.UtcNow` / `DateTime.Now` calls in 23 files (DB services, features, processors, `DateTimeTools`, `PollTools`). Reminder, birthday, poll and unmute logic can't be tested deterministically. Fix: inject `TimeProvider` (built into .NET; `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` in tests), or pass `DateTime now` into the static helpers.
2. **`Global` static state** (`ServerAudioResources`, `BiasGames`, `YoutubeApiKeys`). It is shared between tests and can't be reset cleanly. Fix: move it into DI singletons (e.g. `BiasGameStore`, `AudioStateStore`, `YoutubeKeyStore`).
3. **`Config`** is a concrete class whose constructor reads and writes `Assets/config.json` in the current directory, and about 47 classes inject it. Fix: extract an `IConfig` interface (smallest change), or move to `IOptionsMonitor<BotOptions>`, which keeps reload-on-change.
4. **`BotLogger`** is concrete and calls `BotWindow.LogToWindow` directly, so the core depends on the WPF window. Fix: an `IBotLogger` interface (lets tests assert an error was logged), and have the window subscribe to a logger event instead of the logger knowing the window. Section 4 needs the same change; the design is written down there.
5. **Concrete Discord.Net socket types.** `BaseFeature`, `BaseCommand` and `DiscordTools` use `SocketCommandContext` / `SocketInteractionContext`, and several features inject `DiscordSocketClient`. Socket entities have internal constructors and can't be mocked. Fix: use Discord.Net's own interfaces: `ICommandContext`, `IUser`, `IGuild`, `IMessageChannel`, and `IDiscordClient` (or a narrow wrapper like `IDiscordMessenger.SendDmAsync`) instead of `DiscordSocketClient`. `BaseCommand` could derive from `ModuleBase<ICommandContext>`.
6. **Logic inside command bodies.** Commands parse, validate and branch (e.g. `UserReminderCommands.RemindIn`). Commands are the hardest thing to unit-test, so move that logic into features, processors or tools and keep commands as thin adapters.

### Not worth unit-testing

- CLI wrappers (`yt-dlp`/`ffmpeg`, `twitch.exe`), Puppeteer scrapers, live HTTP APIs. They are already behind interfaces; mock them where consumers are tested.
- Image processors: at most smoke tests ("produces a non-empty image of size X").

### Repositories and the database

Don't use EF InMemory or SQLite: the schema uses the `SQL_Latin1_General_CP1_CI_AS` collation (case-insensitive comparisons that queries rely on, see the CA1862 suppression) and `getutcdate()` defaults, so results would differ. If repository tests are wanted, run them against **SQL Server LocalDB** (ships with Visual Studio) or Testcontainers, create the schema from the model or `Latest_Release.sql`, and reset between tests (e.g. Respawn).

### Project setup

- New project `Discord Bot GUI.Tests` targeting `net11.0-windows10.0.19041.0`, x64, `win-x64`, with a `ProjectReference` to the bot. Referencing a WinExe/WPF project from a test project works; the test project may need `<UseWPF>true</UseWPF>`.
- `Directory.Build.props` applies to it automatically (analyzers, warnings as errors, banned APIs). It needs its own `<Nullable>enable</Nullable>`, like the other two projects.
- Packages: xUnit v3 (or MSTest / NUnit), NSubstitute (or Moq), `Microsoft.Extensions.TimeProvider.Testing`.
- Suggested order: (1) static tools + Mapster mapping tests, no code changes; (2) DB services with mocked repositories; (3) `TimeProvider` + `Global` → DI, then features; (4) Discord interfaces; (5) optional LocalDB repository tests.

## 3. Discord.Net: optional modernization (investigated 2026-11-15)

When checked, Discord.Net 3.20.2 was the latest release, no `[Obsolete]` Discord.Net APIs were in use, and Discord.Commands was not deprecated. The docs note that text commands need the privileged MessageContent intent and recommend slash commands. The fixes from that review are done.

- **Slash commands.** MessageContent only needs Discord approval once the bot is in 100+ servers, so text commands are fine for a self-hosted bot. Slash commands would bring built-in argument parsing, autocomplete and permissions, but would be a large migration.
- **Components V2** (stable since 3.18) could replace the embed + button layouts for help paging, bias lists and poll editing. Cosmetic, not required.
- **Modal improvements** (3.19–3.20): select menus, checkboxes and radio groups in modals could simplify the weekly poll and idol edit modals.

Decided against:
- Adding guilds on `JoinedGuild` instead of on every message: the bot can be added while it isn't running, so the per-message check stays.
- `DownloadUsersAsync` per command: fine as it is.
- `RunMode.Async`: tried in the past and dropped (see CLAUDE.md).

## 4. Moving the database layer into its own class library (investigated 2026-11-15)

**Verdict: very feasible, moderate effort.** The layering is already mostly clean. Nothing outside `Database/` uses repositories directly, the DB services have no Discord.Net imports, and the bot talks to them only through `I*Service` and `*Resource`. The work is mostly moving files, plus untangling the handful of couplings below.

### What would move

| From the bot | Notes |
|---|---|
| `Database/` (DbContext, 19 models, 20 repositories, 20 services, `.editorconfig`) | As is. The `.editorconfig` keeps the models and `MainDbContext` marked as generated code |
| `Interfaces/DBServices/`, `Interfaces/DBRepositories/` | As is |
| `Resources/` (22 DTOs) | As is, after the `IdolGroupResource` fix below |
| `Core/Caching/` (`ServerCache`, `SizedDictionary`) | Only the DB services and `Startup` use it |
| Enums the DB layer uses (`DbProcessResultEnum`, `ChannelTypeEnum`, `GenderEnum`) | See the `Constant` coupling below |
| `Communication/ListWithDbResult`, `Communication/Bias/ExtendedBiasData`, `AdditionalIdolData` | Plain DTOs, no Discord dependency |
| The model ↔ resource half of `MapperConfig` | As an `IRegister` in the library |
| EF packages (`SqlServer`, `Design`, `Tools`) | Scaffolding then targets the library |

Keeping the existing `Discord_Bot.Database.*` / `Discord_Bot.Resources` namespaces in the new assembly means no `using` changes in the bot.

### Couplings to untangle first

1. **`BotLogger` (the real blocker).** All 20 services take `BotLogger` through `BaseService`, and `BotLogger` calls `BotWindow` (WPF). The library can't reference the bot. Fix: an `IBotLogger` interface in the library (the DB layer only uses `Log` and `Error`) implemented by the existing `BotLogger`. This is the same change as test-project blocker 4. The alternative is the standard `ILogger<T>` (`AddLogging()` is already registered) with a small provider forwarding to `BotLogger`, but that changes the 156 logger calls in `Database/`.
   - Design worked out 2026-11-16: `Log` already carries its `Type`, so `BotLogger` can raise a `LogAdded` event, and `BotWindow` subscribes in its constructor (unsubscribes in `OnClosed`) and maps the type to the color it uses today (Log white, Query dark cyan, Client dark gray, Audio dark orange, Error red, Warning yellow). `BotLogger` then loses its `System.Windows` dependency. `BotLogger` appears in 94 files. Other places to change: `Startup.cs` registers `new BotLogger()`, and `App.xaml.cs` resolves it with `GetRequiredService<BotLogger>()`. Put the interface in `Interfaces/Core/`, following the existing `Interfaces/<folder>/` layout.
   - Do this together with: `BotLogger.Logs` is a plain public `List<Log>`, written from many threads and read and cleared by `LogToFile` on the timer thread. A concurrent `Add` can lose entries, and `LogToFile` can hit "Collection was modified". A private `ConcurrentQueue<Log>` that `LogToFile` drains fixes it. Nothing outside `BotLogger` reads `Logs`.
2. **Modals in service signatures.** The idol, idol group and weekly poll service interfaces take 8 Discord `IModal` types (`EditIdolModal`, `EditIdolExtendedModal`, `ChangeIdolGroupModal`, `ChangeIdolProfileLinkModal`, `OverrideImageModal`, `EditGroupModal`, `EditWeeklyPollModal`, `EditWeeklyPollOptionPresetModal`), which carry `Discord.Interactions` attributes. Fix: plain DTOs (e.g. `IdolEditData`) in the library, and have the interaction classes map the modal into them (by hand or with a Mapster map). Referencing Discord.Net from the library for these would work, but defeats much of the point.
3. **EF models leaking out.**
   - `IdolGroupResource.Idols` is a `List<Idol>` (an EF entity). Change it to `List<IdolResource>`; this also fixes a "commands use Resources" violation.
   - `IIdolGroupService.UpdateOrCreateGroupAsync` takes and returns `IdolGroup`. It's only called from `IdolService`, so make it internal or move it into a repository.
4. **`MapperConfig` mixes layers.** Model ↔ resource and DTO → model mappings go to the library. The `WeeklyPollResource → PollProperties` (Discord) mapping stays in the bot. Register both `IRegister`s at startup.
5. **Helpers in `Tools/`.**
   - `PollTools.GetAnswerOptions(WeeklyPoll)` (used by a model mapping) and `UserIdolStatisticTools.AddRanking` move to the library.
   - `PollTools.CreateTitle` / `GetDuration` (Discord types) stay.
   - `CollectionTools.IsNullOrEmpty` is needed on both sides; split `CollectionTools` or duplicate the one-liner (with its `[NotNullWhen(false)]`).
6. **`Constant`.**
   - `TwitchChannelService` uses `Constant.TwitchBaseUrl` (+ `UrlCombine`).
   - `ChannelTypeEnum`'s extension uses `Constant.RestrictedChannelTypes`.
   - Move these constants into the library, or keep the extension method in the bot.
7. **`GlobalSuppressions.cs`.** The Database entries have to move to the new project: CA1309, CA1310 and CA1862 for the `Discord_Bot.Database` namespace, IDE0028 for `Database.Models`, and IDE0290 for `MainDbContext`.

### Shape of the result

- New `DiscordBot.Data` (name open) class library targeting plain **`net11.0`**, with no Windows, WPF or Discord.Net dependency. The bot references it.
- It gets the analyzer setup from `Directory.Build.props` automatically, and needs `<Nullable>enable</Nullable>` in its csproj.
- An `AddDatabase(this IServiceCollection, string connectionString)` extension in the library registers the DbContext, repositories, services and cache, which takes about 45 lines out of `Startup.cs`.
- Repository interfaces could become `internal`, with `InternalsVisibleTo` for tests, so the bot physically can't bypass the services.
- `MainDB/` (SSDT) is unaffected. Scaffolding commands point at the new project (use the bot as `--startup-project` if EF tooling asks for one).

### Benefits

- The boundary rule ("commands work with `*Resource` types, not EF models") becomes enforced by the compiler instead of by convention.
- The DB library can be unit-tested from a plain `net11.0` test project with no WPF or Windows target (see section 2).
- It can be reused later, e.g. by a web dashboard or a separate scraper process.

### Suggested order

1. `IBotLogger`.
2. Fix the `IdolGroupResource` / `UpdateOrCreateGroupAsync` leaks.
3. Modal → DTO for the 8 modals.
4. Split `MapperConfig` and the `Tools` helpers.
5. Create the project and move the files (keeping namespaces).
6. Add the `AddDatabase` extension.
7. Build, then a quick run against the dev database.

Steps 1–4 are worth doing even inside the current project, and they make the move itself mechanical.

Doing this before the test project (section 2) is a good order: DB-service tests can then live in a plain `net11.0` test project.

## 5. Cleaning up the external services (not investigated yet)

The `Services/` folder (API wrappers, scrapers and CLI tools behind `Interfaces/Services/`) has grown over time. Investigate whether these services can be cleaned up or split, and whether one or more base classes would help, e.g. a shared base for REST API clients and one for the Puppeteer scrapers. This fits well with section 2 (testability) and section 4 (library split).

## 6. Last.fm: past periods and daily listens (not started)

- **Past periods in all list commands.** Let users ask for a specific past year, and possibly a half year, quarter, month or week, instead of only the rolling periods Last.fm supports (`7day`, `1month`, `3month`, `6month`, `12month`, `overall`; see `LastFmHelper.LastfmTimePeriod`). The `user.getWeekly*Chart` endpoints return older data for a time range given as Unix timestamps:
  - `?method=user.getweeklychartlist&user=[username]&api_key=[API_KEY]&format=json` lists the available ranges.
  - `?method=user.getweeklyartistchart&user=[username]&api_key=[API_KEY]&from=1704067200&to=1735689599&format=json` returns one range (this example is the whole of 2024).
- **Daily listens.** Last.fm has no `day` period. Add a method in `LastFmApi` (`BaseRequests`) that builds daily stats from the recent tracks endpoint when the requested period is `day` (or anything that converts to it), and use it from the list commands.

## Minor improvements

- **Generic `BaseService`:** turn it into a generic class so simple DB calls need less boilerplate. Each service would still declare its own methods. `GenericRepository<T>` already exists on the repository side.
- **Last.fm top tags:** a new command for `user.getTopTags`. That endpoint has no period or pagination.
- **Last.fm artist stats image (found 2026-11-16):** `LastFmAPI.GetArtistDataAsync` picks the MBID from the albums when there are any, but always passes `tracks[0]`'s artist and track name to the Spotify search. An artist with album plays but no track plays would throw there (rare, since album plays come from track plays). Passing a track name also makes the search return that track's album cover instead of the artist's image, as `GetTopArtistsAsync` does. Decide which image the stats should show, and take both values from the same list.
- **Hand-written results in `Services/Models` (found 2026-11-16):** `Services/Models/LastFm` (`ArtistStats`, `LastFmListResult`, `NowPlayingResult`, `WhoKnows`) and `Services/Models/SpotifyAPI/SpotifyImageSearchResult` are the bot's own result DTOs, not JSON models. Because the folder is marked as generated code, they get no nullable checking and no analyzer warnings. Moving them to `Communication/` (where data types belong by convention) would bring both back. Their nullable fields are `Message`, the image URLs and the optional now-playing data.
- **Culture in interpolated strings (found 2026-11-16):** CA1305 doesn't cover them. For example `LastFmListResultTools` writes `{percentage}%`, which shows "12,5%" under hu-HU and "12.5%" under en-US. Leave it as is if the server's culture is the intended display format.
- **YouTube playlist start point:** let the user choose which song or index a playlist import starts from (`YoutubeAPI.PlaylistSearch`). It currently always takes the first page of 50 (`MaxResults`).
- **YouTube duplicate on playlist import:** when a video link that also carries a playlist (`&list=`) is imported as a playlist (`YoutubeAPI.AddPlaylistAsync`), the video itself can be queued twice if it is in the imported part of the playlist. Skip it.
- **YouTube search matching:** look into a similarity or points-based ranking of search results, beyond the current channel and filter-word filtering in `YoutubeAPI.FilteredResults`. It first came up searching for "Withcha" by Apink.

## Feature ideas

- Halloween game
- Blackjack game
- Christmas game
- A command that counts a user's messages or files
