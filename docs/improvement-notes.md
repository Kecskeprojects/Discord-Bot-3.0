# Improvement notes

Findings from code-quality investigations, kept so the work can be picked up later. Nothing here has been applied yet unless marked done.

## 1. Linting (investigated 2026-11-15)

C# has no separate linter. Linting is done by **Roslyn analyzers** that run during `dotnet build` and in Visual Studio, configured through `.editorconfig` and MSBuild properties. The project already gets the default analyzers (see the suppressions in `Properties/GlobalSuppressions.cs`), but at a lenient level and with nothing enforced.

| Piece | What it does | Current state |
|---|---|---|
| .NET SDK analyzers (CA rules) | Correctness, performance, security, design. Strictness via `<AnalysisLevel>` / `<AnalysisMode>` | Default level, only a few rules are warnings |
| Code-style rules (IDE rules) | Style and formatting | Shown in VS only. `dotnet build` needs `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` plus a raised severity in `.editorconfig` |
| `.editorconfig` | Rule severities, formatting, naming | Only `Discord Bot GUI/Database/.editorconfig` exists, no root one |
| `dotnet format` | Formatter and fixer; `--verify-no-changes` is the check mode | Unused |
| Third-party analyzers | Roslynator, Meziantou.Analyzer, StyleCop.Analyzers (strict style), SonarAnalyzer.CSharp, Microsoft.VisualStudio.Threading.Analyzers (async misuse) | None |
| Nullable reference types | Compiler flags possible null dereferences | `<Nullable>disable</Nullable>` in both projects |
| `TreatWarningsAsErrors` | Enforces the zero-warning goal | Off |

### Trial run

A trial build with `-p:AnalysisLevel=latest-recommended -p:EnforceCodeStyleInBuild=true` reported 418 distinct warnings:

- About 320 culture warnings (CA1305, CA1304, CA1311, CA1310): `ToString()`, `ToLower()`, `string.Format`, `StartsWith` without a culture or `StringComparison`. Mostly harmless for a bot; can be lowered in `.editorconfig`.
- Naming and design: CA1051 public fields (30), CA1707 underscores in names (15), CA1711 / CA1716 name suffixes and reserved words, CA1725 parameter names differ from the interface, CA1309, CA1805.
- Worth fixing:
  - CA2201, `throw new Exception(...)` (10 left): `SizedDictionary`, `YoutubeAPI`, `LastFmHelper`, `PollTools`. (`CommandHandler` and `AudioPlayFeature` were fixed during the Discord.Net work.)
  - CA1001: `App` (`mainTimer`) and `BotWindow` (`diagnosticsTimer`) own timers but are not disposable.
  - CA5369: `WordOfTheDayService.cs:31` uses an `XmlSerializer.Deserialize` overload that may allow DTD processing on external RSS. Deserialize through an `XmlReader` with safe settings instead.

### Gotcha

`Discord Bot GUI/Database/.editorconfig` has `root = true`, so a future repo-root `.editorconfig` would **not** apply anywhere under `Database/`, including `DBServices/` and `DBRepositories/`. Remove `root = true` there and put it in the root file.

### Recommended setup

1. `Directory.Build.props` at the repo root so both projects share `<AnalysisLevel>latest-recommended</AnalysisLevel>` and `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`.
2. A root `.editorconfig` that silences the unwanted rules (most culture rules, CA1707) and keeps the rest as warnings.
3. Optionally `Microsoft.VisualStudio.Threading.Analyzers` for `async void`, `.Result` / `.Wait()` and unawaited tasks, which are common in Discord.Net handlers.
4. `dotnet format` for formatting. Turn on `TreatWarningsAsErrors` only once at zero warnings.
5. Enable nullable reference types gradually, folder by folder with `#nullable enable`.

## 2. Test project (investigated 2026-11-15)

### Already in good shape

- Every DB service, repository and external API service is behind an interface (`Interfaces/`) and registered through DI in `Startup.cs`, so DB services can be unit-tested now by mocking their repositories.
- `IMapper` can be built for real in tests from `MapperConfig`, the same way `Startup` does.
- A large share of logic sits in static, mostly pure helpers that need no changes to test: `Tools/` (`StringTools`, `CollectionTools`, `NumberTools`, `UrlTools`, `ChannelTypeEnumTools`, `LastFmTools`, `DiscordTools.IsTypeOfChannel`), `SizedDictionary` / `ServerCache`, and the static embed and message processors (assert on the returned `Embed`'s fields).
- `BotLogger` writing to the window is harmless in tests: `Application.Current?` is null, so nothing happens.

### Blockers, in order of value

1. **The clock.** About 40 `DateTime.UtcNow` / `DateTime.Now` calls in 23 files (DB services, features, processors, `DateTimeTools`, `PollTools`). Reminder, birthday, poll and unmute logic can't be tested deterministically. Fix: inject `TimeProvider` (built into .NET; `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` in tests), or pass `DateTime now` into the static helpers.
2. **`Global` static state** (`ServerAudioResources`, `BiasGames`, `YoutubeApiKeys`). It is shared between tests and can't be reset cleanly. Fix: move it into DI singletons (e.g. `BiasGameStore`, `AudioStateStore`, `YoutubeKeyStore`).
3. **`Config`** is a concrete class whose constructor reads and writes `Assets/config.json` in the current directory, and it's injected into ~45 classes. Fix: extract an `IConfig` interface (smallest change), or move to `IOptionsMonitor<BotOptions>`, which keeps reload-on-change.
4. **`BotLogger`** is concrete and calls `BotWindow.LogToWindow` directly, so the core depends on the WPF window. Fix: an `IBotLogger` interface (lets tests assert an error was logged), and have the window subscribe to a logger event instead of the logger knowing the window. This also matters for step 4 (moving the DB layer into its own library).
5. **Concrete Discord.Net socket types.** `BaseFeature`, `BaseCommand` and `DiscordTools` use `SocketCommandContext` / `SocketInteractionContext`, and several features inject `DiscordSocketClient`. Socket entities have internal constructors and can't be mocked. Fix: use Discord.Net's own interfaces: `ICommandContext`, `IUser`, `IGuild`, `IMessageChannel`, and `IDiscordClient` (or a narrow wrapper like `IDiscordMessenger.SendDmAsync`) instead of `DiscordSocketClient`. `BaseCommand` could derive from `ModuleBase<ICommandContext>`.
6. **Logic inside command bodies.** Commands parse, validate and branch (e.g. `UserReminderCommands.RemindIn`). Commands are the hardest thing to unit-test, so move that logic into features, processors or tools and keep commands as thin adapters.

### Not worth unit-testing

- CLI wrappers (`yt-dlp`/`ffmpeg`, `twitch.exe`), Puppeteer scrapers, live HTTP APIs. They are already behind interfaces; mock them where consumers are tested.
- Image processors: at most smoke tests ("produces a non-empty image of size X").

### Repositories and the database

Don't use EF InMemory or SQLite: the schema uses the `SQL_Latin1_General_CP1_CI_AS` collation (case-insensitive comparisons that queries rely on, see the CA1862 suppression) and `getutcdate()` defaults, so results would differ. If repository tests are wanted, run them against **SQL Server LocalDB** (ships with Visual Studio) or Testcontainers, create the schema from the model or `Latest_Release.sql`, and reset between tests (e.g. Respawn).

### Project setup

- New project `Discord Bot GUI.Tests` targeting `net11.0-windows10.0.19041.0`, x64, `win-x64`, with a `ProjectReference` to the bot. Referencing a WinExe/WPF project from a test project works; the test project may need `<UseWPF>true</UseWPF>`.
- Packages: xUnit v3 (or MSTest / NUnit), NSubstitute (or Moq), `Microsoft.Extensions.TimeProvider.Testing`.
- Suggested order: (1) static tools + Mapster mapping tests, no code changes; (2) DB services with mocked repositories; (3) `TimeProvider` + `Global` → DI, then features; (4) Discord interfaces; (5) optional LocalDB repository tests.

## 3. Discord.Net review (investigated and fixed 2026-11-15)

Discord.Net 3.20.2 is the latest release, all NuGet packages are current, and no `[Obsolete]` Discord.Net APIs are in use. Discord.Commands is not deprecated, but the docs note that text commands need the privileged MessageContent intent and recommend slash commands.

### Fixed

- **Reconnect timer stacking event handlers.** `BotMain.RunBotAsync` registers handlers and modules only once, and a semaphore stops the startup call and the timer from logging in at the same time.
- **Bias game clicks without acknowledgement.** The `IsProcessing` early return now calls `DeferAsync()`. Failure replies across all interactions use `RespondOrFollowupAsync` (`DiscordTools` / `BaseInteraction`), which picks `RespondAsync` or `FollowupAsync` depending on `HasResponded`. Also fixed: the debut step continued after "Not enough idols" and then failed.
- **Lost stack traces.** `CommandExecuted` / `InteractionExecuted` log `ExecuteResult.Exception`. `commands.Log` and `interactions.Log` are now hooked into `BotLogger`. They skip `CommandException` / `InteractionException`, which Discord.Net also sends there, to avoid logging the same error twice.
- **Interaction failures not reported to the user.** `InteractionExecuted` replies ephemerally on any error result.
- **Unobserved fire-and-forget tasks.** `MessageReceived` runs the whole message handling in one `Task.Run` with a try/catch. That also moves the server DB lookup off the gateway task and adds the `is not SocketUserMessage` check. Commands still run with `RunMode.Sync` inside it.
- **Intents** reduced to `Guilds`, `GuildMembers`, `GuildVoiceStates`, `GuildMessages`, `DirectMessages`, `MessageContent`.
- **Shutdown** stops audio playback, calls `StopAsync` / `LogoutAsync`, then closes the browser, waiting up to 3 seconds and running only once.
- **Voice:** the cached `Guild.CurrentUser` replaces `GetUserAsync` polling. `leave` no longer throws when the bot isn't in a channel. Errors are logged before disconnecting. `AudioVariables.Dispose` is null-safe (it ran from the finalizer with a null `AudioClient`, which can crash the process). `throw new Exception` became `InvalidOperationException` in `AudioPlayFeature` and `CommandHandler`.
- **Module discovery** uses `typeof(...).Assembly` instead of `Assembly.GetEntryAssembly()`.

### Accepted as is

- **`DownloadUsersAsync` per command:** fine as it is.
- **Adding guilds on `JoinedGuild` instead of on every message:** rejected. The bot can be added while it isn't running, so the per-message check stays.
- **`RunMode.Async`:** tried in the past and dropped because of its own problems. Sync mode plus background dispatch from the handlers stays.

### Remaining: optional modernization

- **Slash commands.** MessageContent only needs Discord approval once the bot is in 100+ servers, so text commands are fine for a self-hosted bot. Slash commands would bring built-in argument parsing, autocomplete and permissions, but would be a large migration.
- **Components V2** (stable since 3.18) could replace the embed + button layouts for help paging, bias lists and poll editing. Cosmetic, not required.
- **Modal improvements** (3.19–3.20): select menus, checkboxes and radio groups in modals could simplify the weekly poll and idol edit modals.

## 4. Moving the database layer into its own class library (investigated 2026-11-15)

**Verdict: very feasible, moderate effort.** The layering is already mostly clean. Nothing outside `Database/` uses repositories directly, the DB services have no Discord.Net imports, and the bot talks to them only through `I*Service` and `*Resource`. The work is mostly moving files, plus untangling the handful of couplings below.

### What would move

| From the bot | Notes |
|---|---|
| `Database/` (DbContext, 19 models, 20 repositories, 20 services, `.editorconfig`) | As is |
| `Interfaces/DBServices/`, `Interfaces/DBRepositories/` | As is |
| `Resources/` (22 DTOs) | As is, after the `IdolGroupResource` fix below |
| `Core/Caching/` (`ServerCache`, `SizedDictionary`) | Only the DB services and `Startup` use it |
| Enums the DB layer uses (`DbProcessResultEnum`, `ChannelTypeEnum`, `GenderEnum`) | See the `Constant` coupling below |
| `Communication/ListWithDbResult`, `Communication/Bias/ExtendedBiasData`, `AdditionalIdolData` | Plain DTOs, no Discord dependency |
| The model ↔ resource half of `MapperConfig` | As an `IRegister` in the library |
| EF packages (`SqlServer`, `Design`, `Tools`) | Scaffolding then targets the library |

Keeping the existing `Discord_Bot.Database.*` / `Discord_Bot.Resources` namespaces in the new assembly means no `using` changes in the bot.

### Couplings to untangle first

1. **`BotLogger` (the real blocker).** All 20 services take `BotLogger` through `BaseService`, and `BotLogger` calls `BotWindow` (WPF). The library can't reference the bot. Fix: an `IBotLogger` interface in the library (the DB layer only uses `Log` and `Error`) implemented by the existing `BotLogger`. This is the same change as test-project blocker 4. The alternative is the standard `ILogger<T>` (`AddLogging()` is already registered) with a small provider forwarding to `BotLogger`, but that changes ~156 call sites.
2. **Modals in service signatures.** `IIdolService` / `IIdolGroupService` / weekly poll services take 8 Discord `IModal` types (`EditIdolModal`, `EditGroupModal`, `EditWeeklyPollModal`, …), which carry `Discord.Interactions` attributes. Fix: plain DTOs (e.g. `IdolEditData`) in the library, and have the 4 interaction classes map the modal into them (by hand or with a Mapster map). The other option, referencing Discord.Net from the library for these, works but defeats much of the point.
3. **EF models leaking out.**
   - `IdolGroupResource.Idols` is a `List<Idol>` (an EF entity). Change it to `List<IdolResource>`; this also fixes a "commands use Resources" violation.
   - `IIdolGroupService.UpdateOrCreateGroupAsync` takes and returns `IdolGroup`. It's only called from `IdolService`, so make it internal or move it into a repository.
4. **`MapperConfig` mixes layers.** Model ↔ resource and DTO → model mappings go to the library. The `WeeklyPollResource → PollProperties` (Discord) mapping stays in the bot. Register both `IRegister`s at startup.
5. **Helpers in `Tools/`.**
   - `PollTools.GetAnswerOptions(WeeklyPoll)` (used by a model mapping) and `UserIdolStatisticTools.AddRanking` move to the library.
   - `PollTools.CreateTitle` / `GetDuration` (Discord types) stay.
   - `CollectionTools.IsNullOrEmpty` is needed on both sides; split `CollectionTools` or duplicate the one-liner.
6. **`Constant`.**
   - `TwitchChannelService` uses `Constant.TwitchBaseUrl` (+ `UrlCombine`).
   - `ChannelTypeEnum`'s extension uses `Constant.RestrictedChannelTypes`.
   - Move these constants into the library, or keep the extension method in the bot.
7. **`GlobalSuppressions.cs`.** The `Discord_Bot.Database` entries (CA1862, IDE0290 on `MainDbContext`) have to be copied to the new project.

### Shape of the result

- New `DiscordBot.Data` (name open) class library targeting plain **`net11.0`**, with no Windows, WPF or Discord.Net dependency. The bot references it.
- An `AddDatabase(this IServiceCollection, string connectionString)` extension in the library registers the DbContext, repositories, services and cache, which removes ~45 lines from `Startup.cs`.
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
- **YouTube playlist start point:** let the user choose which song or index a playlist import starts from (`YoutubeAPI.PlaylistSearch`). It currently always takes the first page; `MaxResults` is 50, although the old comment said 25.
- **YouTube duplicate on playlist import:** when a video link that also carries a playlist (`&list=`) is imported as a playlist (`YoutubeAPI.AddPlaylistAsync`), the video itself can be queued twice if it is in the imported part of the playlist. Skip it.
- **YouTube search matching:** look into a similarity or points-based ranking of search results, beyond the current channel and filter-word filtering in `YoutubeAPI.FilteredResults`. It first came up searching for "Withcha" by Apink.

## Feature ideas

- Halloween game
- Blackjack game
- Christmas game
- A command that counts a user's messages or files
