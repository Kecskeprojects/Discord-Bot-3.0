# Improvement notes

Open work from code-quality investigations, kept so it can be picked up later. Items are removed once they are applied; the git history has the details of earlier work. Last brought up to date 2026-11-16.

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

- Every DB service, repository and external service class is behind an interface (`Interfaces/`) and registered through DI in `Startup.cs`, so DB services can be unit-tested now by mocking their repositories, and code that uses an external service can mock it. What is still missing for the services' own logic is listed under "Interfaces for mocking" below.
- `IMapper` can be built for real in tests from `MapperConfig`, the same way `Startup` does.
- A large share of logic sits in static, mostly pure helpers that need no changes to test: `Tools/` (`StringTools`, `CollectionTools`, `NumberTools`, `UrlTools`, `ChannelTypeEnumTools`, `LastFmTools`, `DiscordTools.IsTypeOfChannel`), `SizedDictionary` / `ServerCache`, and the static embed and message processors (assert on the returned `Embed`'s fields).
- Everything logs through `IBotLogger`, so tests can pass a mock (and assert that an error was logged). The concrete `BotLogger` has no WPF dependency either; the window subscribes to its `LogAdded` event.

### Blockers, in order of value

1. **The clock.** 45 `DateTime.UtcNow` / `DateTime.Now` calls in 23 files (DB services, features, processors, `DateTimeTools`, `PollTools`). Reminder, birthday, poll and unmute logic can't be tested deterministically. Fix: inject `TimeProvider` (built into .NET; `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` in tests), or pass `DateTime now` into the static helpers.
2. **`Global` static state** (`ServerAudioResources`, `BiasGames`, `YoutubeApiKeys`). It is shared between tests and can't be reset cleanly. Fix: move it into DI singletons (e.g. `BiasGameStore`, `AudioStateStore`, `YoutubeKeyStore`).
3. **`Config`** is a concrete class whose constructor reads and writes `Assets/config.json` in the current directory, and about 47 classes inject it. Fix: extract an `IConfig` interface (smallest change), or move to `IOptionsMonitor<BotOptions>`, which keeps reload-on-change.
4. **Concrete Discord.Net socket types.** `BaseFeature`, `BaseCommand` and `DiscordTools` use `SocketCommandContext` / `SocketInteractionContext`, and several features inject `DiscordSocketClient`. Socket entities have internal constructors and can't be mocked. Fix: use Discord.Net's own interfaces: `ICommandContext`, `IUser`, `IGuild`, `IMessageChannel`, and `IDiscordClient` (or a narrow wrapper like `IDiscordMessenger.SendDmAsync`) instead of `DiscordSocketClient`. `BaseCommand` could derive from `ModuleBase<ICommandContext>`.
5. **Logic inside command bodies.** Commands parse, validate and branch (e.g. `UserReminderCommands.RemindIn`). Commands are the hardest thing to unit-test, so move that logic into features, processors or tools and keep commands as thin adapters.

### Interfaces for mocking (investigated 2026-11-16)

What has to be mockable so tests can leave out the database, the external APIs and the scraping. These classes have no interface yet, in order of how much code each one makes testable:

1. **The Last.fm requests (`LastFmApi`).** `UserBasedRequests`, `InfoBasedRequests`, `UtilityRequests` and `WhoKnowsRequests` are static methods over one static `RestClient` in `BaseRequests`. `LastFmAPI` calls them directly (465 lines: the list results, now playing, artist stats, who-knows), and so do `UtilityRequests` (215 lines: total plays, the monthly ranking, every album or track of an artist) and `WhoKnowsRequests` (195 lines). Fix: instance classes behind `IUserBasedRequests`, `IInfoBasedRequests`, `IUtilityRequests` and `IWhoKnowsRequests`, registered in DI; the last two take the first two. For the library's own JSON handling, `BaseRequests` can take the `RestClient` from outside: RestSharp builds one on an `HttpClient`, so a test passes a fake `HttpMessageHandler` that returns saved responses. Optional at the same time: give the API key to the constructor instead of every call.
2. **`BrowserService`** (Puppeteer, a concrete singleton) → `IBrowserService` (`OpenBrowser`, `CloseBrowser`, `NewPage`). `BiasScrapingProcessor` (136 lines: matching scraped idols to the database, the group check, image changes) only uses it to close the browser at the end, so with the interface it can be tested with mocked `IKpopDbScraper` and `IIdolService`. Optional: to test the scrapers' HTML parsing (`KpopDbScraper`, `ExtendedBiasData`), split loading a page from parsing it, so the parsing takes the page content and tests can feed it saved pages.
3. **`WebTools`** (static `HttpClient`: `GetStream`, `GetBody`, `TestConnection`) → an instance service behind an interface (e.g. `IWebTools`). `LastFmWhoKnowsEmbedProcessor`, `BiasGameImageProcessor`, `SocialMessageProcessor`, `InstaScraper`, `UserBonkCommands` and `BotMain` use it; with the interface, the image processors get a local test image instead of a download. `WhoKnowsImageProcessor` needs an interface too, so `LastFmWhoKnowsEmbedProcessor` (88 lines: download, draw, fall back to a plain embed) can be tested without Magick.NET.
4. **YouTube.** `YoutubeAPI` (320 lines: watch, playlist and youtu.be links, keyword search with filtering, the playlist import, key rotation and quota counting) creates a `YouTubeService` for every search. Fix: a small `IYoutubeDataClient` wrapping the three calls it makes (videos by ID, search, playlist items), and an interface for `YoutubeAddPlaylistFeature`, which waits for the user's answer in Discord. The key quotas and the music queue are in `Global` (blocker 2).
5. **Spotify.** `SpotifyAPI` (201 lines: link handling, playlist or album → search terms, the image search fallback) builds a `SpotifyClient` on every call. SpotifyAPI.Web already has `ISpotifyClient`, so no interface of our own is needed: inject that, through a small factory if credential edits in `config.json` should still apply without a restart.
6. **`DiscordSocketClient`**, injected into `BotMain`, `CommandHandler`, `InteractionHandler`, `DiscordTools` and the features `BirthdayFeature`, `ReminderFeature`, `UnmuteFeature`, `WeeklyPollFeature`, `TwitchNotificationFeature`, `EasterEggFeature` and `YoutubeAddPlaylistFeature`: blocker 4. The birthday, reminder, unmute and poll logic sits in those features, so this matters most for feature tests.

No interface needed:
- `Config` (42 classes): blocker 3 already covers it (`IConfig`).
- `ServerCache` (every DB service): it lives in memory, so tests use a real one.
- `MainDbContext`: only the repositories use it. Mock the repository interfaces, or test the repositories against LocalDB (below).
- `IMapper` is already an interface; tests build the real one from `MapperConfig`.
- Features and processors that only commands and interactions inject (`AudioPlayFeature`, `AudioRequestFeature`, `WeeklyPollFeature`, `BiasScrapingProcessor`, the image processors): commands aren't test targets (blocker 5), so nothing has to mock them.
- `MusicBrainzAPI` and `WordOfTheDayService`: thin wrappers, already behind interfaces, so their consumers can mock them. Their own parsing could be tested by passing in the `RestClient`, as for `BaseRequests` (optional).
- `TwitchCLI`, `YoutubeStreamService`, `InstaLoader` (CLI processes) and `ProcessTools` / `ProcessExtension` (performance counters and WMI for the window's diagnostics): not worth testing.
- `TwitchAPI`: TwitchLib's monitor raises the events. To test the live / offline decisions, move them into a plain class that the event handlers call, rather than wrapping the monitor.
- `CommandService` / `InteractionService` (Discord.Net): only the handlers and the help command use them.

Randomness and files (no interface, but tests have to account for them):
- `new Random()` in `CollectionTools`, `StringExtension.ToMockText`, `RainbowRandomizerTools`, `BirthdayMessageProcessor`, `EasterEggFeature`, `WhoKnowsImageProcessor` (file names), `YoutubeAPI` (key choice) and several commands. Tests can check properties (a shuffle keeps every item), or the helpers take a `Random` so a test passes a seeded one.
- `RainbowRandomizerTools.GetOperatorList` reads `R6_Attackers.txt` / `R6_Defenders.txt` from the working directory: copy them to the test output, or pass the list in.

### Not worth unit-testing

- CLI wrappers (`yt-dlp`/`ffmpeg`, `twitch.exe`), Puppeteer scrapers, live HTTP APIs. They are already behind interfaces; mock them where consumers are tested.
- Image processors: at most smoke tests ("produces a non-empty image of size X").

### Repositories and the database

Don't use EF InMemory or SQLite: the schema uses the `SQL_Latin1_General_CP1_CI_AS` collation (case-insensitive comparisons that queries rely on, see the CA1862 suppression) and `getutcdate()` defaults, so results would differ. If repository tests are wanted, run them against **SQL Server LocalDB** (ships with Visual Studio) or Testcontainers, create the schema from the model or `Latest_Release.sql`, and reset between tests (e.g. Respawn).

### Project setup

- New project `Discord Bot GUI.Tests` targeting `net11.0-windows10.0.19041.0`, x64, `win-x64`, with a `ProjectReference` to the bot. Referencing a WinExe/WPF project from a test project works; the test project may need `<UseWPF>true</UseWPF>`.
- `Directory.Build.props` applies to it automatically (analyzers, warnings as errors, banned APIs). It needs its own `<Nullable>enable</Nullable>`, like the other two projects.
- Packages: xUnit v3 (or MSTest / NUnit), NSubstitute (or Moq), `Microsoft.Extensions.TimeProvider.Testing`.
- Suggested order: (1) static tools + Mapster mapping tests, no code changes; (2) DB services with mocked repositories; (3) the Last.fm request interfaces, `IBrowserService` and `IWebTools`, then `LastFmAPI`, `BiasScrapingProcessor` and the who-knows embed; (4) `TimeProvider` + `Global` → DI, then features; (5) Discord interfaces; (6) YouTube and Spotify; (7) optional LocalDB repository tests.

## 3. Discord.Net: optional modernization (investigated 2026-11-15)

When checked, Discord.Net 3.20.2 was the latest release, no `[Obsolete]` Discord.Net APIs were in use, and Discord.Commands was not deprecated. The docs note that text commands need the privileged MessageContent intent and recommend slash commands. The fixes from that review are done.

- **Slash commands.** MessageContent only needs Discord approval once the bot is in 100+ servers, so text commands are fine for a self-hosted bot. Slash commands would bring built-in argument parsing, autocomplete and permissions, but would be a large migration.
- **Components V2** (stable since 3.18) could replace the embed + button layouts for help paging, bias lists and poll editing. Cosmetic, not required.
- **Modal improvements** (3.19–3.20): select menus, checkboxes and radio groups in modals could simplify the weekly poll and idol edit modals.

Decided against:
- Adding guilds on `JoinedGuild` instead of on every message: the bot can be added while it isn't running, so the per-message check stays.
- `DownloadUsersAsync` per command: fine as it is.
- `RunMode.Async`: tried in the past and dropped (see CLAUDE.md).

## 4. Moving the database layer into its own class library (investigated 2026-11-15, planned and done 2026-11-17)

**Verdict: very feasible, moderate effort.** The layering is already mostly clean. Nothing outside `Database/` uses repositories directly, the DB services have no Discord.Net imports, and the bot talks to them only through `I*Service` and `*Resource`. The work is mostly moving files, plus untangling the handful of couplings below.

### What would move

The target is the `DatabaseORM/` project (created 2026-11-17), modeled on the project of the same name in KecskeProjectsSite.

| From the bot | To `DatabaseORM/` | Notes |
|---|---|---|
| `Database/MainDbContext.cs` | `Context/` | Generated code; its root `.editorconfig` section needs the new path |
| `Database/Models/` (19) | `Model/` | Its root `.editorconfig` section (IDE0005 / IDE0028 off) needs the new path |
| `Database/DBRepositories/` (20), `Database/DBServices/` (20) | `Repository/`, `Service/` | As is |
| `Interfaces/DBRepositories/`, `Interfaces/DBServices/` | `Interface/Repository/`, `Interface/Service/` | As is |
| `Interfaces/Core/IBotLogger.cs` | `Interface/` | The services log through it; `BotLogger` stays in the bot and implements it |
| `Resources/` (21 DTOs) | `Resources/` | As is |
| `Core/Caching/` (`ServerCache`, `SizedDictionary`) | `Caching/` | Only the DB services and `Startup` use it |
| Enums the DB layer uses (`DbProcessResultEnum`, `ChannelTypeEnum`, `GenderEnum`) | `Enums/` | See the `Constant` coupling below |
| `ListWithDbResult`, `Bias/ExtendedBiasData`, `AdditionalIdolData`, the three `*EditData` DTOs | `Communication/` | Plain DTOs, no Discord dependency |
| `Database/Mapping/DatabaseMapperConfig.cs` | `Mapping/` | The DB half of the old `MapperConfig`, an `IRegister` |
| `Database/Tools/` (`WeeklyPollTools`, `UserIdolStatisticTools`, `Extensions/EntityFrameworkExtension`) | `Tools/` | Helpers only the DB layer uses |
| The DB registrations in `Startup.cs` | `DatabaseStartup.cs` | As in KecskeProjectsSite, but taking the connection string from the bot's `Config` |
| EF packages (`SqlServer`, `Design`, `Tools`) | csproj | Scaffolding then targets the library; the command is kept locally in the git-ignored `Context/update-scaffold.txt` |

**Namespaces become `DatabaseORM.*`.** The original idea of keeping `Discord_Bot.Database.*` doesn't fit the new folder names, because IDE0130 (namespace must match folder) is a build error like every other style rule. That means `using` updates in about 110 bot files, plus the enum users. It's mechanical work.

**Differences from KecskeProjectsSite's DatabaseORM, kept on purpose:**
- No nested `.editorconfig` with `root = true`; the path sections go in the root file.
- `Model/` is not `generated_code`, so its nullable annotations count.
- Services keep returning `DbProcessResultEnum` instead of throwing.
- The `I*` interfaces stay.
- `IMapper` stays injected.

**In the new csproj (done 2026-11-17):**
- `Microsoft.Extensions.Identity.Core` (KecskeProjectsSite's password hasher) was removed.
- `ImplicitUsings` is on, unlike in the bot. The moved files' redundant `using System;` and similar lines were removed with `dotnet format --diagnostics IDE0005`. The scaffolded `Model/` files keep theirs, because IDE0005 is off there and a re-scaffold writes them again.

### Couplings to untangle first

1. **Modals in service signatures.** **Done 2026-11-17.**
   - The idol, idol group, image and weekly poll services took 8 Discord `IModal` types.
   - The three multi-field modals now map themselves into `IdolEditData`, `IdolExtendedEditData` and `IdolGroupEditData` (in `Communication/Bias/`) with `ToEditData()`.
   - The single- and two-field ones pass plain strings, as `EditWeeklyPollOptionModal` already did. The idol overloads became `UpdateProfileUrlAsync` and `ChangeGroupAsync`.
   - Parsing and trimming stayed in the services, so behavior is unchanged.
2. **EF models leaking out.** **Done 2026-11-17.**
   - `IdolGroupResource` (whose `Idols` was a `List<Idol>`) was deleted with its mapping; nothing used it.
   - `UpdateOrCreateGroupAsync` left `IIdolGroupService`. It is now the private `IdolService.GetOrCreateGroupAsync`, which uses the repository `IdolService` already had, and `IdolService` no longer depends on `IIdolGroupService`.
3. **`MapperConfig` mixes layers.** **Done 2026-11-17.**
   - The model → resource and scraped data → model mappings, and the `ShallowCopyForSameType` default, are now in `Database/Mapping/DatabaseMapperConfig.cs`.
   - `Core/MapperConfig.cs` keeps only `WeeklyPollResource → PollProperties` (Discord).
   - `Startup` registers both before `Compile()`.
4. **Helpers in `Tools/`.** **Done 2026-11-17.**
   - `PollTools.GetAnswerOptions` (and its private helpers) became `Database/Tools/WeeklyPollTools`. `PollTools` keeps `CreateTitle`, `GetDuration` and `GetEnumFromTicks`.
   - `UserIdolStatisticTools` moved to `Database/Tools/`, and `EntityFrameworkExtension` (only `GenericRepository` uses it) to `Database/Tools/Extensions/`.
   - The DB layer's five `CollectionTools.IsNullOrEmpty` calls became `Count` checks, because those lists are never null. `CollectionTools` stays in the bot.
5. **`Constant`.** **Done 2026-11-17.**
   - `AddTwitchChannelAsync` now takes the link. The command builds it from `Constant.TwitchBaseUrl`, so base URLs stay in `Constant.cs`.
   - `RestrictedChannelTypes` became a private field of `ChannelTypeEnumExtension`, its only user.
6. **AngleSharp in `ExtendedBiasData` (found and done 2026-11-17).** Its constructor parsed a dbkpop table row (`IElement`). It is now a plain DTO, and the parsing moved to `KpopDbScraper.ParseRow`.
7. **`GlobalSuppressions.cs`.** **Done 2026-11-17.**
   - `DatabaseORM/GlobalSuppressions.cs` now holds CA1309, CA1310 and CA1862 for the `DatabaseORM.Repository` and `DatabaseORM.Service` namespaces. That is where the EF query lambdas are. The old `Discord_Bot.Database` scope also covered the models, the mapping and the DB tools, none of which need it.
   - It also holds IDE0290 for `MainDbContext`, and IDE0305 for the whole library, with the bot's justification.
   - The bot's commented-out IDE0028 entry for the models was dropped; the `.editorconfig` model section covers it.

### Shape of the result

- The `DatabaseORM` class library targets plain **`net11.0`**, with no Windows, WPF or Discord.Net dependency. The bot references it.
- It gets the analyzer setup from `Directory.Build.props` automatically; its csproj already has `<Nullable>enable</Nullable>`.
- `DatabaseStartup.ConfigureDatabaseServices(services, connectionString)` in the library registers the DbContext, the cache, the services and the repositories. That took about 45 lines out of `Startup.cs`.
  - The connection string is passed as a `Func<string>` that is read once per scope, so an edited `sql_connection_string` still applies without a restart, as before.
  - The Mapster registers and `IBotLogger` stay registered in the bot's `Startup`, because the bot's own code shares them.
- The repositories and their interfaces, the concrete services, `ServerCache` and the DB tools are `internal`, so the bot can't bypass the services. CA1852 then required the leaf classes to be `sealed`.
  - A future test project needs `InternalsVisibleTo` to reach them.
  - The scaffolded models and `MainDbContext` stay public, as generated.
- `MainDB/` (SSDT) is unaffected. Scaffolding points at the new project (`-Project` and `-StartupProject DatabaseORM`).

### Benefits

- The boundary rule ("commands work with `*Resource` types, not EF models") becomes enforced by the compiler instead of by convention.
- The DB library can be unit-tested from a plain `net11.0` test project with no WPF or Windows target (see section 2).
- It can be reused later, e.g. by a web dashboard or a separate scraper process.

### Slices (planned 2026-11-17)

Each slice ends in a green build.

1. **Done 2026-11-17.** Boundary leaks and modal → DTO, inside the bot (couplings 1 and 2).
2. **Done 2026-11-17.** Split `MapperConfig`, the `Tools` helpers, the `Constant` uses and `ExtendedBiasData`'s parsing, still inside the bot (couplings 3–6). A grep confirms that the files to move (the table above) only use each other, `System`, EF Core and Mapster.
3. **Done 2026-11-17.** The move:
   - `git mv` of 136 files into the `DatabaseORM/` folders, with `DatabaseORM.*` namespaces. The repositories needed `using DatabaseORM.Context;`, because they no longer sit below the context's namespace.
   - Stripped the implicit usings, and updated and sorted the bot's usings.
   - The bot references `DatabaseORM` and no longer has the EF packages; `SqlServer` comes through the project reference. `Identity.Core` was dropped.
   - The suppressions moved and the `.editorconfig` paths were fixed.
   - The scaffold command is in `Context/update-scaffold.txt`, which is git-ignored and only exists locally, because it names the dev machine. It passes `-StartupProject DatabaseORM`, so the bot needs no `EntityFrameworkCore.Design`.
4. **Done 2026-11-17.** `DatabaseStartup`, the `internal` repositories and services, and the CLAUDE.md updates (layout, layers, scaffold step, linting paths).
   - A throwaway program ran the bot's real `Startup.Run()` with a dummy connection string. All 19 DB services and three features resolved, and an edited connection string reached the next scope.
   - Still to do: a run of the bot itself against the dev database.

Slices 1 and 2 are worth doing even inside the current project, and they make the move itself mechanical.

Doing this before the test project (section 2) is a good order: DB-service tests can then live in a plain `net11.0` test project.

## 5. Cleaning up the external services (not investigated yet)

The `Services/` folder (API wrappers, scrapers and CLI tools behind `Interfaces/Services/`) has grown over time. Investigate whether these services can be cleaned up or split, and whether one or more base classes would help, e.g. a shared base for REST API clients and one for the Puppeteer scrapers. This fits well with section 2 (testability) and section 4 (library split).

## 6. Last.fm: past periods and daily listens (not started)

- **Past periods in all list commands.** Let users ask for a specific past year, and possibly a half year, quarter, month or week, instead of only the rolling periods Last.fm supports (`7day`, `1month`, `3month`, `6month`, `12month`, `overall`; see `LastFmHelper.LastfmTimePeriod`). The `user.getWeekly*Chart` endpoints return older data for a time range given as Unix timestamps:
  - `?method=user.getweeklychartlist&user=[username]&api_key=[API_KEY]&format=json` lists the available ranges.
  - `?method=user.getweeklyartistchart&user=[username]&api_key=[API_KEY]&from=1704067200&to=1735689599&format=json` returns one range (this example is the whole of 2024).
- **Daily listens.** Last.fm has no `day` period. Add a method in `LastFmApi` (`BaseRequests`) that builds daily stats from the recent tracks endpoint when the requested period is `day` (or anything that converts to it), and use it from the list commands.

## 7. Conditional (`?:`) expressions (investigated 2026-11-16)

The rule change, the bias list paging fix and the rewrites of the complex and the unneeded ternaries are done (2026-11-17; see CLAUDE.md "Linting"). 154 conditional expressions are left, and only one is nested: `LastFmAPI.cs:278`, which goes with the "Last.fm artist stats image" item under Minor improvements.

**Borderline, left as they are** (fine as ternaries, `if`/`else` by taste):
- A value from one of two awaited calls: `UserBiasCommands.cs:72, 128`, `UserLastfmCommands.cs:353`, `AudioRequestFeature.cs:69`. A guard and a call: `YoutubeAPI.cs:110, 119`, where an early return would read better now that IDE0046 is off.
- The same longer condition twice: `StaticRoundData.cs:15, 24`. A small `IsRightSide(position)` method would make both simple.
- An `out` variable or a negation in the condition: `LastFmAPI.cs:183`, `DiscordTools.cs:17` (the `!` goes away if the branches are swapped), `DiscordTools.cs:87`, and `TwitchNotificationFeature.cs:44` (swap the branches).
- `LastFmListResultTools.cs:78`: the long date-parsing branch could go into a local.

## 8. Instagram and Twitter embeds (investigated 2026-11-17)

Can the disabled embeds be turned back on as they are, with adjustments and without logging in?

**How they work today:**
- **Instagram (`InstaScraper`):** a plain `HttpClient` GET (no User-Agent) to `instagram.com/graphql/query/?doc_id=8845758582119845&variables={"shortcode":…}`. The call in `CommandHandler` is commented out, and so is the Instaloader path in `InstagramEmbedFeature`; `instaloader.exe` is no longer in `External Files`.
- **Twitter (`TwitterScraper`):** headless Chrome opens the tweet page logged out and reads the page's own `TweetResultByRestId` GraphQL response. Only the commented-out `!twt` command calls it. The Chrome build doesn't matter any more: the comment in `BrowserService` about Chrome 117 is from an older approach that captured video responses, while the scraper now reads the video URLs from the JSON.

**Test (one request, logged out, a well-known public post):** Instagram answers that GraphQL GET with `401` `{"message":"Please wait a few minutes before you try again.","require_login":true}`. So the Instagram scraper doesn't work as it is. Instagram's own logged-out web page sends this query as a POST with tokens taken from the page (`lsd`, the web app ID). Copying that wasn't tested: it means imitating Instagram's internal web client, the `doc_id` changes every few months, and logged-out requests are rate-limited per IP. Twitter wasn't tested live for the same reason.

**Terms of use:** both sites forbid automated collection without their permission (X's terms forbid scraping "in any form, for any purpose" without written consent, and Instagram's forbid collecting data "in an automated way" without permission). This holds whether or not an account is logged in.

**Lower-risk ways to get embeds back:**
- **Link rewriting:** reply with the link rewritten to an embed-fix service (for X: `fxtwitter.com` / `fixupx.com`; for Instagram, services like `kkinstagram.com` come and go), and suppress the original embed. Discord then builds the embed itself, and the bot fetches nothing. This is a few lines in `InstagramEmbedFeature` / a revived `!twt`, and needs no scraper or browser. The catch is relying on a third-party service.
- **Official APIs:** the X API's read access is paid (no reasonable tier for a hobby bot). Instagram's oEmbed needs a registered Meta app with an app access token, and returns embed HTML, not media files. Neither fits the "download and re-upload" design.

**If the scrapers are revived anyway, fix these first:**
- `InstaScraper.ExtractFromUrl` cuts the shortcode at the first `/` of the last segment, so a link without a trailing slash (`/p/ABC`, which `UrlTools.LinkSearch` produces when it strips a `?igsh=…` query) throws `ArgumentOutOfRangeException`.
- `InstaScraper.GetMediaUris` sends every carousel item as an image, so videos inside a carousel arrive as thumbnails (the video branch is commented out).
- `TwitterScraper.GetMediaUris` takes `VideoInfo.Variants[^1]`, which isn't necessarily the best MP4 (the variants include an `.m3u8` playlist). Pick the `video/mp4` variant with the highest bitrate that fits Discord's upload limit. It also returns a list that is always empty, which no caller uses.
- `TwitterScraper.ModifyImageUrl`: `url.Split(".").Length > 1` is always true for `https://pbs.twimg.com/...`, so the size and format logic after it never runs.
- `TwitterScraper.ExtractFromUrl` calls `DeleteCookieAsync()` without arguments, which deletes nothing.
- Posts X marks as sensitive (`NsfwLoggedOut`) can't be seen logged out at all.

**Found on the way:** `BrowserService` downloads PuppeteerSharp's default Chrome build on first use after every PuppeteerSharp upgrade and never removes the old ones. The old `net10.0` output folder alone holds 7 Chrome and 6 ChromeHeadlessShell builds (117 to 150). Check the bot's folder on the live server too, and delete all but the current build.

## 9. Line endings (noted, investigated and applied 2026-11-17)

Settle on one line ending for every file in the repository and enforce it.

### What happens today

- **The repository is fine.** `.gitattributes` has `* text=auto`, so every commit stores LF (all 525 text files are `i/lf`). The back and forth only happens in the working copy.
- **Nothing declares a working copy ending.** Each tool writes its own default, and the file keeps that ending until git rewrites it.
- **Git writes CRLF.** It does so on checkout, discard, reset, stash, merge and pull, but only for the files that operation touches.
- **An ending-only change looks like an edit.** git status, GitHub Desktop and Visual Studio list the file as modified, with an empty diff ("LF will be replaced by CRLF the next time Git touches it"). Committing it changes nothing.
- **LF in a `.cs` file can break the build.** On Windows the Roslyn formatter expects CRLF by default (a maintainer: it "uses the line endings of the platform it is running on", [roslyn#73122](https://github.com/dotnet/roslyn/issues/73122)). Without `end_of_line`, IDE0055 flags only the LF line breaks next to comments.
- **Fixing those spots makes mixed files.** `dotnet format`, or Code Cleanup's formatting, rewrites only the flagged line breaks as CRLF, which leaves the file mixed. That happened twice during the section 4 move.

### Who writes what (checked on this machine 2026-11-17)

| Source | Writes | Evidence |
|---|---|---|
| Git for Windows 2.47 (`autocrlf=false`, system config) | CRLF on checkout | `* text=auto` with the default `core.eol=native`. Confirmed in a scratch repo, and on the 170 files restored during section 4 |
| GitHub Desktop's own git 2.56 (`autocrlf=true` in its bundled config) | CRLF on checkout | Same result through a different setting. It commits LF like the others |
| Visual Studio 18's own git 2.56 | CRLF on checkout | Its bundled config says `autocrlf=true`, but the Git for Windows config overrides it to `false`. The result is CRLF either way |
| Visual Studio editor | Keeps a file's ending; templates are CRLF | `.sln`, new `.csproj` and the resx designer file are CRLF. "Check for consistent line endings on load" is on, so it offers to fix mixed files |
| EF `Scaffold-DbContext` (Package Manager Console) | CRLF | KecskeProjectsSite's scaffolded files share the scaffold run's timestamp (2026-11-16 21:11, no checkout since) and are CRLF |
| SSDT publish scripts (`MainDB/DB Patch`) | CRLF | Generation timestamps, CRLF |
| VS Code | New files CRLF (`files.eol` default `auto`); keeps existing endings | No EditorConfig extension is installed, so VS Code ignores `.editorconfig` |
| VS Code + Prettier (global default formatter, format on save) | LF for files Prettier formats (Markdown, JSON, YAML, JS/TS, CSS, HTML) | Prettier's default `endOfLine` is `lf` since 2.0. It reads `end_of_line` from `.editorconfig`, but none is set. It also reformats the content (for example Markdown tables). It doesn't touch `.cs` |
| Windows Notepad | Keeps a file's ending; new files CRLF | Since Windows 10 1809 ([announcement](https://devblogs.microsoft.com/commandline/extended-eol-in-notepad/)) |
| draw.io | LF | `diagram.drawio` |
| Claude Code's Edit tool | Keeps the file's ending, including on added lines | Tested on a CRLF and an LF file |
| Claude Code's Write tool | Always LF, even over a CRLF file | Tested. The files created during section 4 are LF this way |
| Git Bash `sed -i`, `awk`, `>` redirects, heredocs | LF (`sed -i` turns CRLF files into LF) | Tested. `sed -b -i` keeps CRLF. This is what turned about 220 files into LF during section 4 |
| PowerShell 5.1 `Get-Content \| Set-Content`, `Out-File` | CRLF (LF files become CRLF) | Tested. `[IO.File]::ReadAllText` / `WriteAllText` keeps the ending |
| `dotnet format` / Code Cleanup | Without `end_of_line`: CRLF only at the flagged spots (mixed files). With `end_of_line = crlf`: every line becomes CRLF | Tested on a scratch project ("ENDOFLINE: Fix end of line marker") |

**Origin of the mix:** every text file last written before the Claude Code sessions (up to 2026-11-13) is CRLF, except `.gitattributes` and `LICENSE` from the repository's creation in 2023 and `diagram.drawio`. The LF files come from Claude Code sessions since 2026-11-15 (the Write tool and Git Bash text tools, e.g. `Directory.Build.props` and `BannedSymbols.txt` from 2026-11-16), plus Prettier for anything saved in VS Code. Visual Studio and git then turn some of them back into CRLF.

**Checking line endings:** `git ls-files --eol`, `od -c` or `cat -A` are reliable. `grep -c $'\r$'` in Git Bash is not: it reported CR on files that had none.

### Recommendation: CRLF in the working copy, LF in the repository

- **Why CRLF:** it is what nearly every Windows tool here writes (Visual Studio, all three gits, EF scaffolding, SSDT, PowerShell, Notepad) and what the formatter expects. The LF writers are few and controllable: Claude Code's tools, Prettier and draw.io.
  - Going LF instead would mean fighting Visual Studio's templates, the scaffolder, SSDT and the formatter's platform default.
- **`.gitattributes`:** `* text=auto eol=crlf`. Every git client then checks out CRLF whatever its own `autocrlf` says, and the repository stays LF. No file needs an LF exception today (no shell scripts).
  - If EF's `.t4` templates are added later (the "Code Cleanup edits the scaffolded models" item), they must stay CRLF. LF templates are reported to produce an extra blank line per line.
- **`.editorconfig`:** `end_of_line = crlf` under `[*]`. Then:
  - `dotnet format whitespace` turns every line of a `.cs` file into CRLF, and `--verify-no-changes` reports any LF line.
  - Visual Studio uses CRLF for new lines.
  - Prettier writes CRLF when it reads the `.editorconfig`. Check that VS Code's Prettier extension does so by saving a Markdown file once and looking at `git ls-files --eol`.
  - The Roslyn maintainer's advice against setting `end_of_line` is for repos built on both Windows and Linux. Here both settings say CRLF and everything runs on Windows, so the mismatch that issue describes can't happen.
- **CLAUDE.md:** a short rule for Claude sessions.
  - Files are CRLF. Use Edit for existing files.
  - After Write or Git Bash text tools, convert to CRLF with `sed -b -i 's/\r*$/\r/'`, or run `dotnet format whitespace`, and check with `git ls-files --eol`.
  - The same rule could go in the global `~/.claude/CLAUDE.md`, since KecskeProjectsSite has the same tools.

### Applying it (done 2026-11-17)

On a clean working tree:
1. Change `.gitattributes` and `.editorconfig`, then commit. The index is already LF, so `git add --renormalize .` should report nothing.
2. Rewrite every working file in CRLF. git doesn't rewrite files it considers unchanged, so: `git rm -r --cached -q .` then `git reset --hard`.
3. Check that `git ls-files --eol` shows only `w/crlf` (and `-text` for binaries), that the three projects build, and that `dotnet format --verify-no-changes` passes for each.
4. Add the CLAUDE.md rule.

**Result:**
- **Settings:** commit `6176c49` holds `.gitattributes`, `.editorconfig` and this investigation. `git add --renormalize .` found nothing to change.
- **Working copy:** `git rm -r --cached -q .` and `git reset --hard` rewrote all 526 text files as CRLF. The 16 binaries stay `-text`, and `LICENSE` is no longer mixed.
- **Checks:** the three projects build without warnings, and `dotnet format --verify-no-changes` passes for each.
- **Safety net:** a file turned into LF with `sed -i` got all 14 of its lines reported as ENDOFLINE, and `git checkout` restored it as CRLF.
- **CLAUDE.md:** the line endings rule is in its Linting section.
- **Still to check by hand:** that VS Code's Prettier now writes CRLF. Save a Markdown file there, then run `git ls-files --eol -- <file>`.
- **Local only:** the ignored `Context/update-scaffold.txt` was converted with `unix2dos`.

## 10. Implicit usings in the bot (noted 2026-11-17, not investigated yet)

`DatabaseORM` and `LastFmApi` already have `<ImplicitUsings>enable</ImplicitUsings>`; only `Discord Bot GUI` doesn't. Investigate whether to turn it on there too.

**Points to check:**
- **WinForms usings.** The bot has `UseWindowsForms` as well as `UseWPF`. With implicit usings, the Windows Desktop SDK then also imports `System.Drawing` and `System.Windows.Forms`, which clash with WPF names (`Application`, `Color`, `MessageBox`...).
  - A quick grep found no `System.Windows.Forms` use in the code, so first check whether `UseWindowsForms` is still needed.
  - Otherwise, drop those two with `<Using Remove="..." />`.
- **`System.Threading` becomes global.** A bare `Timer` is then ambiguous in any file that also imports `System.Timers`. `App.xaml.cs` already writes `System.Timers.Timer` in full, and the build will point out any others.
- **`System.IO` is not implicit in WPF projects.** A WPF test project on 2026-11-17 had no `File` or `Path` without it, so files that use them keep their `using System.IO;`.
- **The redundant usings.** Remove them with `dotnet format --diagnostics IDE0005`, then review the diff.

## Minor improvements

- **Generic `BaseService`:** turn it into a generic class so simple DB calls need less boilerplate. Each service would still declare its own methods. `GenericRepository<T>` already exists on the repository side.
- **Last.fm top tags:** a new command for `user.getTopTags`. That endpoint has no period or pagination.
- **Last.fm artist stats image (found 2026-11-16):** `LastFmAPI.GetArtistDataAsync` picks the MBID from the albums when there are any, but always passes `tracks[0]`'s artist and track name to the Spotify search. An artist with album plays but no track plays would throw there (rare, since album plays come from track plays). Passing a track name also makes the search return that track's album cover instead of the artist's image, as `GetTopArtistsAsync` does. Decide which image the stats should show, and take both values from the same list.
- **Hand-written results in `Services/Models` (found 2026-11-16):** `Services/Models/LastFm` (`ArtistStats`, `LastFmListResult`, `NowPlayingResult`, `WhoKnows`) and `Services/Models/SpotifyAPI/SpotifyImageSearchResult` are the bot's own result DTOs, not JSON models. Because the folder is marked as generated code, they get no nullable checking and no analyzer warnings. Moving them to `Communication/` (where data types belong by convention) would bring both back. Their nullable fields are `Message`, the image URLs and the optional now-playing data.
- **Code Cleanup edits the scaffolded models (found 2026-11-16):** `DatabaseORM/Model` (formerly `Database/Models`) is not marked as generated code (so its `?` annotations count, see CLAUDE.md "Database changes"), so Visual Studio's Code Cleanup changes those files, e.g. it removes unused usings even with IDE0005 off for the folder. Until this is fixed, leave the folder out of Code Cleanup, or re-scaffold afterwards. Fix: mark the folder `generated_code = true` again, and have the scaffold write `#nullable enable` at the top of each model file, which turns nullable back on in generated code. To do that, install EF's templates (`dotnet new install Microsoft.EntityFrameworkCore.Templates`, from nuget.org), run `dotnet new ef-templates` in the `DatabaseORM` project folder, add the directive to `CodeTemplates/EFCore/EntityType.t4`, and re-scaffold once. `Scaffold-DbContext` and `dotnet ef dbcontext scaffold` use those templates automatically. The cost: the templates become the project's own copy, so after an EF upgrade, refresh them and re-add the line.
- **Scraped idol gender is stored as `Male`/`Female` (found 2026-11-17, code fixed 2026-11-17, existing rows still to correct):**
  - The `ExtendedBiasData → Idol` mapping (`DatabaseMapperConfig`) writes `Gender.Value.ToString()`, so an idol first filled from dbkpop gets `Male`/`Female`.
  - Everything else uses `M`/`F` (`ToDatabaseFriendlyString`): the edit modal, and the bias game's gender filter in `IdolRepository.GetListForGameAsync`. The game filtered by gender therefore leaves those idols out.
  - The bug predates the Mapster port (AutoMapper's enum → string did the same).
  - Fix: the mapping now uses `ToDatabaseFriendlyString()`.
  - Still to do: correct the existing rows on the dev and the live database (`UPDATE Idol SET Gender = 'M' WHERE Gender = 'Male'`, and `'F'` for `'Female'`). This is a one-off data fix, so it isn't an SSDT patch. Check with `SELECT Gender, COUNT(*) FROM Idol GROUP BY Gender`.
- **`[RequireOwner]` on the extended idol modal (found 2026-11-17, done 2026-11-17):** `EditIdolExtendedModalSubmit` in `EditIdolModalInteraction` was the only edit handler without `[RequireOwner]`. It only mattered for consistency, because the modal opens only from the owner-only `EditIdolData` menu. The attribute is now added.
- **Culture in interpolated strings (found 2026-11-16):** CA1305 doesn't cover them. For example `LastFmListResultTools` writes `{percentage}%`, which shows "12,5%" under hu-HU and "12.5%" under en-US. Leave it as is if the server's culture is the intended display format.
- **YouTube playlist start point:** let the user choose which song or index a playlist import starts from (`YoutubeAPI.PlaylistSearch`). It currently always takes the first page of 50 (`MaxResults`).
- **YouTube duplicate on playlist import:** when a video link that also carries a playlist (`&list=`) is imported as a playlist (`YoutubeAPI.AddPlaylistAsync`), the video itself can be queued twice if it is in the imported part of the playlist. Skip it.
- **YouTube search matching:** look into a similarity or points-based ranking of search results, beyond the current channel and filter-word filtering in `YoutubeAPI.FilteredResults`. It first came up searching for "Withcha" by Apink.

## Feature ideas

- Halloween game
- Blackjack game
- Christmas game
- A command that counts a user's messages or files
