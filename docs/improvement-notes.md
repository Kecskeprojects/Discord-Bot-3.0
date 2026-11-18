# Improvement notes

Open work from code-quality investigations, kept so it can be picked up later. Items are removed once they are applied; the git history has the details of earlier work. Last brought up to date 2026-11-17.

Done and removed on 2026-11-17 (the investigations are in `git show 483c1fa:docs/improvement-notes.md`, sections 4 and 9–11):
- the move of the database layer into `DatabaseORM`
- CRLF line endings
- implicit usings in the bot, with Windows Forms removed
- the BOM rule
- the direct `System.Management` reference
- `[RequireOwner]` on the extended idol modal

## To check by hand

- **Run the bot once against the dev database and Discord.** Since the last real run, the database layer moved into `DatabaseORM`, implicit usings were turned on, and Windows Forms was removed (2026-11-17). Since then, `Startup.Run()` was split, `IConfig` added, `TimeProvider` injected, `Global` replaced by the state stores and `LastFmApi` made instance-based (2026-11-18). Worth a try: a Last.fm top list and who-knows, a reminder (`!remind in 1m>test`), a short mute, a self-role request, a bias game (start, stop, and one played to the end), and music (a YouTube search, a playlist link, and waiting until the inactivity disconnect).
  - What has been checked: builds, `dotnet format` and throwaway programs. Those resolved every DB service through the real `Startup.Run()`, ran `ProcessTools.GetStatistics()` (RAM figure, WMI child process count) and loaded a resx bitmap.
  - Worth a look in the window: the diagnostics line.
- **Correct the scraped idol genders on the dev and the live database.**
  - Until 2026-11-17, an idol first filled from dbkpop was saved with `Male`/`Female`. The bias game's gender filter (`IdolRepository.GetListForGameAsync`) and the edit modal use `M`/`F`, so the filtered game left those idols out. The mapping is fixed (`DatabaseMapperConfig` uses `ToDatabaseFriendlyString()`); the existing rows aren't.
  - Run `UPDATE Idol SET Gender = 'M' WHERE Gender = 'Male'` and `UPDATE Idol SET Gender = 'F' WHERE Gender = 'Female'`, then check with `SELECT Gender, COUNT(*) FROM Idol GROUP BY Gender`. This is a one-off data fix, not an SSDT patch.
- **VS Code's Prettier and CRLF.** Prettier writes LF by default. It should take `end_of_line = crlf` from the root `.editorconfig`, but that is untested in the VS Code extension.
  - To check: save a Markdown file in VS Code, then run `git ls-files --eol -- <file>`. It should say `w/crlf`.
  - If it doesn't, add a `.prettierrc` with `"endOfLine": "crlf"`.
- **Old Chrome builds.** `BrowserService` downloads PuppeteerSharp's default Chrome build on first use after every PuppeteerSharp upgrade, and never removes the old ones. On 2026-11-17 the old `net10.0` output folder alone held 7 Chrome and 6 ChromeHeadlessShell builds (117 to 150). Delete all but the current build, in the bot's folder on the live server too.

## 1. Linting follow-ups (investigated 2026-11-15 and 2026-11-16)

The analyzer setup is done and described in CLAUDE.md ("Linting"). Left:

- **CA1508 (dead conditional code) is on trial.** Of its first 5 findings, 4 were right and 1 was a false positive (`AudioPlayFeature.WaitAsync`, suppressed in the bot's `Properties/GlobalSuppressions.cs`). Decide whether to keep it once it has run on new code for a while.
- **CA1849 cases worth fixing by hand.** The rule isn't turned on, because half of its 12 findings are false positives (`GetChannel` / `GetRole` are cache lookups in Discord.Net's socket client). The real ones:
  - `CancellationTokenSource.Cancel(false)` runs the cancellation callbacks on the calling thread: `App.xaml.cs`, `BotMain.cs`, `UserVoiceCommands.cs` (3). `CancelAsync` avoids that.
  - `YoutubeStreamService` blocks a thread for up to a minute in `Process.WaitForExit(TimeSpan)`. `WaitForExitAsync` with a timeout token avoids that.
- **Optional, explicitness only (no behavior change):**
  - CA1307 (38): spell out `StringComparison` on `Contains` / `Replace` / `EndsWith(char)`, which are already ordinal. None are in `DatabaseORM`.
  - CA1820 (15): `== ""` → `string.IsNullOrEmpty`. Not a pure rename, because `IsNullOrEmpty` also accepts null.

**Decided against (2026-11-16)**, so these don't need another look:
- `Microsoft.VisualStudio.Threading.Analyzers`: 180 warnings in a trial build, mostly VSTHRD200 (`Async` suffix on command methods, which Discord.Net doesn't care about). It found nothing the current rules miss.
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

## 2. Test project (investigated 2026-11-15 and 2026-11-16, updated 2026-11-17)

### Already in good shape

- **The DB layer is a plain `net11.0` library.** `DatabaseORM` has no WPF or Discord.Net dependency, so its tests can live in a plain `net11.0` test project.
  - Its services are behind `I*Service` interfaces. Its repositories are behind interfaces too, but those are `internal`, as are the concrete services, `ServerCache` and the DB tools.
  - Everything is registered by `DatabaseStartup.ConfigureDatabaseServices`, so DB services can be unit-tested by mocking their repositories (see "Project setup" for reaching the internals).
- **External services are behind interfaces** (`Interfaces/Services/` in the bot), so code that uses one can mock it. What the services' own logic still needs is under "Interfaces for mocking" below.
- **`IMapper` can be built for real in tests:** register `DatabaseMapperConfig` and `MapperConfig` on a `TypeAdapterConfig` and compile it, as `Startup` does.
- **Many static, mostly pure helpers need no changes to test:**
  - `Tools/` in the bot: `StringTools`, `CollectionTools`, `NumberTools`, `UrlTools`, `ChannelTypeEnumTools`, `LastFmTools`, `DiscordTools.IsTypeOfChannel`.
  - `DatabaseORM/Tools` and `SizedDictionary` / `ServerCache`.
  - The static embed and message processors (assert on the returned `Embed`'s fields).
- **Everything logs through `IBotLogger`,** so tests can pass a mock and assert that an error was logged. The concrete `BotLogger` has no WPF dependency either; the window subscribes to its `LogAdded` event.

### Blockers, in order of value

1. **The clock (done 2026-11-18, the rule is in CLAUDE.md "The clock").**
   - `TimeProvider` is now injected into the reminder, unmute, weekly poll and birthday features, the mute and reminder commands, and `BiasGameInteraction`.
   - These take the time as a parameter: `DateTimeTools.TryAddValuesToDate`, `BiasGameDebutEmbedProcessor`, `BirthdayMessageProcessor`, `WeeklyPollTools.GetAnswerOptions`, `IBirthdayService.GetBirthdaysByDateAsync` and `IServerMutedUserService.AddMutedUserAsync`.
   - Checked with a throwaway program: the container validates with a fake clock, all 34 command and interaction modules construct, and the date helpers give the old results for a fixed time.
   - The birthday and "already muted" queries used to compare against `GETUTCDATE()` on the SQL server; they now send the bot's own UTC time as a parameter. That only differs if the two machines' clocks disagree.
   - **Left on the system clock:**
     - The scheduler's decisions in `App.OnTimedEvent` (06:00, midnight, Mondays). `App` is the WPF application and can't be tested; move those decisions into a plain class with `TimeProvider` if they need tests.
     - The "Days Of Week" poll options: the Mapster mapping passes `DateTime.UtcNow` to `WeeklyPollTools`, which is testable on its own.
     - The delays in the scrapers, `YoutubeStreamService` and `ProcessTools`, which are in code that isn't unit-tested.
   - The waits in the features use `Task.Delay(delay, timeProvider)` (done 2026-11-18): both in `AudioPlayFeature` (the wait for new requests and the pause for too-short videos), `YoutubeAddPlaylistFeature`'s polling for the user's answer, and `SelfRoleFeature`'s 1.5 seconds before deleting its reply. A fake clock skips them; with `TimeProvider.System` they wait as before.
   - Tests use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`.
2. **`Global` static state (done 2026-11-18, described in CLAUDE.md "Shared state").** `Core/Global.cs` is replaced by three singletons in `Core/`: `AudioStateStore`, `BiasGameStore` and `YoutubeKeyStore`.
   - `GetCurrentAudioResource` left `BaseCommand` and `BaseFeature`. `UserVoiceCommands` and the two audio features inject `AudioStateStore` instead, so the other commands' constructors are unchanged.
   - `BiasGameStore.TryStart` owns the 30-minute expiry, with `TimeProvider`. `BiasGameData` takes its start time in the constructor.
   - Checked with a throwaway program: the container validates and all 34 modules construct. The expiry holds at 29 minutes and lets a new game start at 31. The rotation uses each key once, the audio store hands out one instance per server, and `CancelAll` cancels the playback.
   - **Behavior changes:**
     - Abandoned games (replaced after 30 minutes, or ended by an error) are now disposed; before, they were left to the garbage collector.
     - Two "bias game" commands at the same moment now give one game and "You already have a game going!", instead of two games.
     - The audio state of a server is created atomically (`GetOrAdd`). Before, a lost race returned an instance that wasn't stored.
     - The key rotation keeps the key string itself. Before, it kept an index into the config's key list, but picked it from the number of keys left, so it could choose a used-up key again or one past the end.
     - Keys edited in `config.json` apply at the next refill (06:00 UTC or a restart). A duplicate key counts once instead of throwing at startup.
3. **`Config` (done 2026-11-18).** The 47 classes inject `IConfig` (`Interfaces/IConfig.cs`) instead, so tests mock it. `DatabaseStartup` takes the connection string as a `Func<IServiceProvider, string>` and `Startup` reads it through `IConfig`, so replacing `IConfig` in a test also replaces the database. Checked with a throwaway program: `ValidateOnBuild` passes on the real registrations, and a fake `IConfig` reached the DbContext.
4. **Concrete Discord.Net socket types.** `BaseFeature`, `BaseCommand` and `DiscordTools` use `SocketCommandContext` / `SocketInteractionContext`, and several features inject `DiscordSocketClient`. Socket entities have internal constructors and can't be mocked.
   - Fix: use Discord.Net's own interfaces: `ICommandContext`, `IUser`, `IGuild`, `IMessageChannel`, and `IDiscordClient` (or a narrow wrapper like `IDiscordMessenger.SendDmAsync`) instead of `DiscordSocketClient`. `BaseCommand` could derive from `ModuleBase<ICommandContext>`.
5. **Logic inside command bodies.** Commands parse, validate and branch (e.g. `UserReminderCommands.RemindIn`). Commands are the hardest thing to unit-test.
   - Fix: move that logic into features, processors or tools, and keep commands as thin adapters.

### Interfaces for mocking (investigated 2026-11-16)

What has to be mockable so tests can leave out the database, the external APIs and the scraping. These classes have no interface yet, in order of how much code each one makes testable:

1. **Last.fm: fake only the HTTP layer (the seam is done, 2026-11-18).**
   - **Why this one place is enough:** all of `LastFmApi` (858 lines) sends through one `RestClient` in `BaseRequests`, in two methods: `UserBasedRequestHandler` and `InfoBasedRequestHandler`, each a single `GetAsync`. The request classes on top of it do no other I/O.
     - The bot's `LastFmAPI` (456 lines) calls 14 of their methods. It also uses `ISpotifyAPI` (already an interface), `IBotLogger` and `IConfig`, which it reads only for `Lastfm_API_Key`. It uses no clock, `WebTools`, shared state, files or randomness.
     - Faking only the HTTP responses therefore tests the whole chain:
       - query building (escaping, `limit` / `page` / `period`)
       - success and error handling
       - JSON deserialization
       - the paging and aggregation in `UtilityRequests` and `WhoKnowsRequests`
       - `LastFmAPI`'s list results, now playing, artist stats and who-knows
   - **The seam (done 2026-11-18):** `LastFmApi` is instance-based, with the client injected.
     - `BaseRequests` takes its `RestClient` in the constructor, and its two handlers are `internal`.
     - `UserBasedRequests` and `InfoBasedRequests` take the `BaseRequests` instead of inheriting from it. `UtilityRequests` and `WhoKnowsRequests` take the request classes they call.
     - `Startup` registers one `BaseRequests`, with one long-lived `RestClient` as RestSharp recommends, and the four classes as singletons. `LastFmAPI` takes them in its constructor.
     - The API key stays a parameter of each call, read from `IConfig` per call. `LastFmHelper` stays static.
     - Checked with a throwaway program, with a fake HTTP layer behind the real container:
       - Top tracks sent the same query strings as before, and summed the play count over two pages.
       - A Last.fm error came back as the result message.
       - Two containers with different fakes ran in parallel without mixing their results.
   - **In a test:** register `new BaseRequests(new RestClient(new RestClientOptions(LastFmApi.Constant.LastFmApiBaseUri) { ConfigureMessageHandler = _ => fakeHandler }))` after `Startup.ConfigureServices`, or build the request classes directly over it. The fake `HttpMessageHandler` returns saved JSON and can assert on the request URL.
     - Tests against saved responses take milliseconds each, so running them in parallel saves little at first. The bigger gain is isolation: no shared static client to reset between tests.
   - **Saved responses:** record each Last.fm method once from the real API (`user.gettoptracks`, `user.gettopalbums`, `user.gettopartists`, `user.getrecenttracks`, `artist.getinfo`, `track.getinfo`, plus an error response). Keep them as JSON files in the test project.
     - The fake handler picks the file by the `method=` parameter (and the page, for the paging helpers).
     - Use your own account's data, or replace the usernames.
     - Saved responses can drift from the live API, so re-record them when Last.fm changes a format.
   - **Config:** a `LastFmAPI` test mocks `IConfig`; only `Lastfm_API_Key` is read.
   - **Spotify:** `LastFmAPI` asks it for the image in the top lists, recents, now playing, artist stats and who-knows (corrected 2026-11-18; it isn't only the artist stats). Mock `ISpotifyAPI` in all of them; returning null falls back to Last.fm's own image.
2. **`BrowserService`** (Puppeteer, a concrete singleton) → `IBrowserService` (`OpenBrowser`, `CloseBrowser`, `NewPage`).
   - `BiasScrapingProcessor` (136 lines: matching scraped idols to the database, the group check, image changes) only uses it to close the browser at the end. With the interface it can be tested with mocked `IKpopDbScraper` and `IIdolService`.
   - Optional: to test the scrapers' HTML parsing (`KpopDbScraper.ParseRow` and the group scraping), split loading a page from parsing it, so the parsing takes the page content and tests can feed it saved pages.
3. **`WebTools`** (static `HttpClient`: `GetStream`, `GetBody`, `TestConnection`) → an instance service behind an interface (e.g. `IWebTools`).
   - `LastFmWhoKnowsEmbedProcessor`, `BiasGameImageProcessor`, `SocialMessageProcessor`, `InstaScraper`, `UserBonkCommands` and `BotMain` use it. With the interface, the image processors get a local test image instead of a download.
   - `WhoKnowsImageProcessor` needs an interface too, so `LastFmWhoKnowsEmbedProcessor` (88 lines: download, draw, fall back to a plain embed) can be tested without Magick.NET.
4. **YouTube.** `YoutubeAPI` (320 lines: watch, playlist and youtu.be links, keyword search with filtering, the playlist import, key rotation and quota counting) creates a `YouTubeService` for every search.
   - Fix: a small `IYoutubeDataClient` wrapping the three calls it makes (videos by ID, search, playlist items), and an interface for `YoutubeAddPlaylistFeature`, which waits for the user's answer in Discord.
   - The key quotas and the music queue are in `YoutubeKeyStore` and `AudioStateStore`; tests use real ones.
   - **The key rotation never ran (found and fixed 2026-11-18).**
     - The cause: `VideoSearch`, `KeywordSearch` and `PlaylistSearch` caught every exception from `ExecuteAsync`, logged it and returned "not found", so the quota error never reached the key switch in `Searching`. Until the 06:00 UTC refill, every search on a used-up key failed, even when other keys had quota.
     - It dates from `b33c2ae` (2023-08-27, "YoutubeAPI reimplemented"), which added the catch blocks and the key switch together; no later change touched them. The earlier version, which `KeyReset` comes from, isn't in this repository.
     - The fix: the three searches let quota errors through (`catch ... when (!IsQuotaExceeded(ex))`, the same string match as before). `RunWithKeyRotationAsync` repeats the request with each remaining key until one works, for searches and for the playlist import.
     - Repeating must not queue anything twice. So the "add the playlist?" offer for a watch link with `&list=` now starts after the video search. A playlist import that runs out of quota after queuing songs keeps what it queued, drops the key and stops, instead of starting over.
     - Checked with a throwaway program against the Google library, with a fake HTTP layer that answers YouTube's 403 `quotaExceeded`. The committed version swallowed the error and returned "not found". With the fix, the error reaches the rotation, the keys are tried in turn until one works, the song is queued once, and the search stops cleanly when no key is left. Other errors are still logged and return "not found".
   - The quota units counted per key (`YoutubeKeyStore.AddUsage`) are never read. They could decide which key to use next, or show in the diagnostics; otherwise drop them.
5. **Spotify.** `SpotifyAPI` (201 lines: link handling, playlist or album → search terms, the image search fallback) builds a `SpotifyClient` on every call.
   - SpotifyAPI.Web already has `ISpotifyClient`, so no interface of our own is needed. Inject that, through a small factory if credential edits in `config.json` should still apply without a restart.
6. **`DiscordSocketClient`**, injected into `BotMain`, `CommandHandler`, `InteractionHandler`, `DiscordTools` and seven features: `BirthdayFeature`, `ReminderFeature`, `UnmuteFeature`, `WeeklyPollFeature`, `TwitchNotificationFeature`, `EasterEggFeature`, `YoutubeAddPlaylistFeature`.
   - This is blocker 4. The birthday, reminder, unmute and poll logic sits in those features, so it matters most for feature tests.

No interface needed:
- `Config`: done, behind `IConfig` (blocker 3).
- `ServerCache` (every DB service): it lives in memory, so tests use a real one.
- `MainDbContext`: only the repositories use it. Mock the repository interfaces, or test the repositories against LocalDB (below).
- `IMapper` is already an interface; tests build the real one (see above).
- Features and processors that only commands and interactions inject (`AudioPlayFeature`, `AudioRequestFeature`, `WeeklyPollFeature`, `BiasScrapingProcessor`, the image processors): commands aren't test targets (blocker 5), so nothing has to mock them.
- `MusicBrainzAPI` and `WordOfTheDayService`: thin wrappers, already behind interfaces, so their consumers can mock them. Their own parsing could be tested by passing in the `RestClient`, as for `BaseRequests` (optional).
- `TwitchCLI`, `YoutubeStreamService`, `InstaLoader` (CLI processes) and `ProcessTools` / `ProcessExtension` (performance counters and WMI for the window's diagnostics): not worth testing.
- `TwitchAPI`: TwitchLib's monitor raises the events. To test the live / offline decisions, move them into a plain class that the event handlers call, rather than wrapping the monitor.
- `CommandService` / `InteractionService` (Discord.Net): only the handlers and the help command use them.

Randomness and files (no interface, but tests have to account for them):
- **Randomness:** `new Random()` appears in `CollectionTools`, `StringExtension.ToMockText`, `RainbowRandomizerTools`, `BirthdayMessageProcessor`, `EasterEggFeature`, `WhoKnowsImageProcessor` (file names), `YoutubeAPI` (key choice) and several commands. Tests can check properties (a shuffle keeps every item), or the helpers take a `Random` so a test passes a seeded one.
- **Files:** `RainbowRandomizerTools.GetOperatorList` reads `R6_Attackers.txt` / `R6_Defenders.txt` from the working directory. Copy them to the test output, or pass the list in.

### Not worth unit-testing

- CLI wrappers (`yt-dlp`/`ffmpeg`, `twitch.exe`), Puppeteer scrapers, live HTTP APIs. They are already behind interfaces; mock them where consumers are tested.
- Image processors: at most smoke tests ("produces a non-empty image of size X").

### Repositories and the database

Don't use EF InMemory or SQLite. The schema uses the `SQL_Latin1_General_CP1_CI_AS` collation (case-insensitive comparisons that queries rely on, see the CA1862 suppression) and `getutcdate()` defaults, so results would differ.

If repository tests are wanted (checked 2026-11-17):
- **Run them against SQL Server LocalDB.** `MSSQLLocalDB` is installed on the dev PC (SQL Server 2025, `Microsoft SQL Server/170`).
- **Create the schema with `Database.EnsureCreated()` from the scaffolded model.**
  - It carries `UseCollation("SQL_Latin1_General_CP1_CI_AS")` and all 26 `getutcdate()` defaults, so the result matches the SSDT schema closely enough.
  - Then run `MainDB/Post-deployment/ChannelType_Inserts.sql` for the 6 lookup rows.
- **Why not the other two schema sources:**
  - `Latest_Release.sql` is git-ignored, and the dev PC's copy is from 2023.
  - Publishing `MainDB.dacpac` (DacFx or SqlPackage) would be exact, but it needs a Visual Studio build of `MainDB` first.
- **Isolation:** give each test collection its own database (a unique name, created at the start and dropped at the end), so collections run in parallel. Within one collection, reset between tests (e.g. Respawn).

### Integration tests (investigated 2026-11-17)

Yes, the bot's test project can run integration tests: real services wired together in-process, with only the outside world faked. In order of value for effort:

1. **DI composition.** Build the real container and resolve every registered service.
   - A throwaway program already did this through `Startup.Run()` on 2026-11-17, with a dummy connection string. No SQL Server or Discord connection is made.
   - Cheap, and it catches a missing registration or a broken Mapster mapping before the bot starts.
2. **The DB layer against LocalDB.** Real DB services and repositories on the schema above.
   - This tests the actual queries, the collation-dependent comparisons, the cache invalidation, and the results the services return.
3. **Last.fm through the bot.** `LastFmAPI` and `LastFmApi` on the injected fake HTTP client with the saved responses (item 1 above). `ISpotifyAPI` is replaced by a mock.
4. **Whole text commands, without Discord.**
   - Discord.Net's `CommandService.ExecuteAsync(ICommandContext, …)` runs a command from its text. Give it a fake context that records what the command replies, and a real container with LocalDB and the fake Last.fm client.
   - A test can then send `!lf…` or a bias command and check the reply embed.
   - This needs blocker 4 first. `BaseCommand` derives from `ModuleBase<SocketCommandContext>`, so a non-socket context fails; deriving from `ModuleBase<ICommandContext>` fixes that.
   - Interactions (buttons, modals) work the same way through `InteractionService`, with the same prerequisite.

**The one enabler they share (done 2026-11-18):** `Startup.ConfigureServices(IServiceCollection)` registers everything, and `Startup.Run()` only builds the provider from it. A test calls `ConfigureServices` on its own collection and replaces only the edges:
- the Last.fm `RestClient`
- `ISpotifyAPI`, and other external services as needed
- the connection string, by replacing `IConfig` (see blocker 3)

**Not worth it:**
- A real Discord gateway: it would need a second bot account and a test server, and would be slow and flaky.
- Live external APIs as part of the normal run. An optional, opt-in live Last.fm test (a trait, a real key, network) could warn when a saved response no longer matches the real format.

**Where:**
- Keep them in `Discord Bot GUI.Tests` under `Integration/` (see "Project setup"), marked `[Trait("Category", "Integration")]`. Then `dotnet test --filter "Category!=Integration"` stays the quick run.
- Move them to their own project if the LocalDB setup grows.
- DB-only integration tests can also go in `DatabaseORM.Tests`.

### Project setup

- **Two test projects fit the split:**
  - `DatabaseORM.Tests`, plain `net11.0`: DB services, the Mapster mappings, `ServerCache`, the DB tools.
  - `Discord Bot GUI.Tests`, `net11.0-windows10.0.19041.0`, x64, `win-x64`, referencing the bot: tools, processors, features. Referencing a WinExe/WPF project from a test project works; it may need `<UseWPF>true</UseWPF>`.
- **Folder layout (decided 2026-11-18),** in each test project:
  - `Unit/` mirrors the folders of the project under test (`Unit/Tools/` for `Tools/`, and so on).
  - `Integration/` is grouped by theme instead (e.g. `Music/`, `LastFm/`, `Bias/`).
  - `Mocks/` holds the fakes and mocks, organized by what they stand in for.
  - Shared constants and helpers keep repetition down, so the tests stay readable and maintainable by hand. Where they live is still open.
- **Reaching `DatabaseORM`'s internals:** add `[assembly: InternalsVisibleTo("DatabaseORM.Tests")]` to `DatabaseORM` (the repositories, the concrete services, `ServerCache` and the DB tools are `internal`).
  - Mocking an internal interface with NSubstitute or Moq also needs `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`.
- **Shared setup:** `Directory.Build.props` and the root `.editorconfig` apply automatically (analyzers, warnings as errors, banned APIs, CRLF, the BOM rule). Each test project needs its own `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`, like the other projects.
- **Packages:** xUnit v3 (or MSTest / NUnit), NSubstitute (or Moq), `Microsoft.Extensions.TimeProvider.Testing`.
- **Suggested order:**
  1. Static tools and Mapster mapping tests, with no code changes. Then the DI composition test on `Startup.ConfigureServices`.
  2. DB services with mocked repositories.
  3. Tests for `LastFmApi` and the bot's `LastFmAPI` against saved responses, run in parallel (the injected client is done). They go in `Discord Bot GUI.Tests`, which reaches `LastFmApi` through the bot.
  4. Integration tests: the DB layer on LocalDB, and Last.fm through the real container.
  5. `IBrowserService` and `IWebTools`; then `BiasScrapingProcessor` and the who-knows embed.
  6. The features (`TimeProvider` and the state stores are done).
  7. The Discord interfaces, then whole-command integration tests.
  8. YouTube and Spotify.

## 3. Discord.Net: optional modernization (investigated 2026-11-15)

When checked, Discord.Net 3.20.2 was the latest release, no `[Obsolete]` Discord.Net APIs were in use, and Discord.Commands was not deprecated. The docs note that text commands need the privileged MessageContent intent and recommend slash commands. The fixes from that review are done.

- **Slash commands.** MessageContent only needs Discord approval once the bot is in 100+ servers, so text commands are fine for a self-hosted bot. Slash commands would bring built-in argument parsing, autocomplete and permissions, but would be a large migration.
- **Components V2** (stable since 3.18) could replace the embed + button layouts for help paging, bias lists and poll editing. Cosmetic, not required.
- **Modal improvements** (3.19–3.20): select menus, checkboxes and radio groups in modals could simplify the weekly poll and idol edit modals.

Decided against:
- Adding guilds on `JoinedGuild` instead of on every message: the bot can be added while it isn't running, so the per-message check stays.
- `DownloadUsersAsync` per command: fine as it is.
- `RunMode.Async`: tried in the past and dropped (see CLAUDE.md).

## 4. Cleaning up the external services (not investigated yet)

The `Services/` folder (API wrappers, scrapers and CLI tools behind `Interfaces/Services/`) has grown over time. Investigate whether these services can be cleaned up or split, and whether one or more base classes would help, e.g. a shared base for REST API clients and one for the Puppeteer scrapers. This fits well with section 2 (testability). The database layer already moved into `DatabaseORM` (2026-11-17); these services stay in the bot.

## 5. Last.fm: past periods and daily listens (not started)

- **Past periods in all list commands.** Let users ask for a specific past year, and possibly a half year, quarter, month or week. Today only the rolling periods Last.fm supports work (`7day`, `1month`, `3month`, `6month`, `12month`, `overall`; see `LastFmHelper.LastfmTimePeriod`). The `user.getWeekly*Chart` endpoints return older data for a time range given as Unix timestamps:
  - `?method=user.getweeklychartlist&user=[username]&api_key=[API_KEY]&format=json` lists the available ranges.
  - `?method=user.getweeklyartistchart&user=[username]&api_key=[API_KEY]&from=1704067200&to=1735689599&format=json` returns one range (this example is the whole of 2024).
- **Daily listens.** Last.fm has no `day` period. Add a method in `LastFmApi` (`BaseRequests`) that builds daily stats from the recent tracks endpoint when the requested period is `day` (or anything that converts to it), and use it from the list commands.

## 6. Conditional (`?:`) expressions (investigated 2026-11-16, line numbers updated 2026-11-17)

154 conditional expressions were left after the 2026-11-17 cleanup (see CLAUDE.md "Linting"), and only one is nested: `LastFmAPI.cs:275`, which goes with the "Last.fm artist stats image" item under Minor improvements.

**Borderline, left as they are** (fine as ternaries, `if`/`else` by taste):
- **A value from one of two awaited calls:** `UserBiasCommands.cs:68, 124`, `UserLastfmCommands.cs:349`, `AudioRequestFeature.cs:66`.
- **A guard and a call:** `YoutubeAPI.cs:106, 115`, where an early return would read better now that IDE0046 is off.
- **The same longer condition twice:** `StaticRoundData.cs:15, 24`. A small `IsRightSide(position)` method would make both simple.
- **An `out` variable or a negation in the condition:**
  - `LastFmAPI.cs:180`
  - `DiscordTools.cs:15` (the `!` goes away if the branches are swapped)
  - `DiscordTools.cs:85`
  - `TwitchNotificationFeature.cs:41` (swap the branches)
- **`LastFmListResultTools.cs:76`:** the long date-parsing branch could go into a local.

## 7. Instagram and Twitter embeds (investigated 2026-11-17)

Can the disabled embeds be turned back on as they are, with adjustments and without logging in?

**How they work today:**
- **Instagram (`InstaScraper`):** a plain `HttpClient` GET (no User-Agent) to `instagram.com/graphql/query/?doc_id=8845758582119845&variables={"shortcode":…}`.
  - The call in `CommandHandler` is commented out, and so is the Instaloader path in `InstagramEmbedFeature`.
  - `instaloader.exe` is no longer in `External Files`.
- **Twitter (`TwitterScraper`):** headless Chrome opens the tweet page logged out and reads the page's own `TweetResultByRestId` GraphQL response. Only the commented-out `!twt` command calls it.
  - The Chrome build doesn't matter any more. The comment in `BrowserService` about Chrome 117 is from an older approach that captured video responses; the scraper now reads the video URLs from the JSON.

**Test (one request, logged out, a well-known public post):**
- Instagram answers that GraphQL GET with `401` `{"message":"Please wait a few minutes before you try again.","require_login":true}`. So the Instagram scraper doesn't work as it is.
- Instagram's own logged-out web page sends this query as a POST with tokens taken from the page (`lsd`, the web app ID). Copying that wasn't tested: it means imitating Instagram's internal web client, the `doc_id` changes every few months, and logged-out requests are rate-limited per IP.
- Twitter wasn't tested live for the same reason.

**Terms of use:** both sites forbid automated collection without their permission (X's terms forbid scraping "in any form, for any purpose" without written consent, and Instagram's forbid collecting data "in an automated way" without permission). This holds whether or not an account is logged in.

**Lower-risk ways to get embeds back:**
- **Link rewriting:** reply with the link rewritten to an embed-fix service, and suppress the original embed. For X: `fxtwitter.com` / `fixupx.com`; for Instagram, services like `kkinstagram.com` come and go.
  - Discord then builds the embed itself, and the bot fetches nothing.
  - This is a few lines in `InstagramEmbedFeature` / a revived `!twt`, and needs no scraper or browser. The catch is relying on a third-party service.
- **Official APIs:** neither fits the "download and re-upload" design.
  - The X API's read access is paid, with no reasonable tier for a hobby bot.
  - Instagram's oEmbed needs a registered Meta app with an app access token, and returns embed HTML, not media files.

**If the scrapers are revived anyway, fix these first:**
- `InstaScraper.ExtractFromUrl` cuts the shortcode at the first `/` of the last segment. A link without a trailing slash (`/p/ABC`, which `UrlTools.LinkSearch` produces when it strips a `?igsh=…` query) therefore throws `ArgumentOutOfRangeException`.
- `InstaScraper.GetMediaUris` sends every carousel item as an image, so videos inside a carousel arrive as thumbnails (the video branch is commented out).
- `TwitterScraper.GetMediaUris` takes `VideoInfo.Variants[^1]`, which isn't necessarily the best MP4 (the variants include an `.m3u8` playlist).
  - Pick the `video/mp4` variant with the highest bitrate that fits Discord's upload limit.
  - It also returns a list that is always empty, which no caller uses.
- `TwitterScraper.ModifyImageUrl`: `url.Split(".").Length > 1` is always true for `https://pbs.twimg.com/...`, so the size and format logic after it never runs.
- `TwitterScraper.ExtractFromUrl` calls `DeleteCookieAsync()` without arguments, which deletes nothing.
- Posts X marks as sensitive (`NsfwLoggedOut`) can't be seen logged out at all.

## Minor improvements

- **RestSharp versions aligned (found and done 2026-11-18):** `LastFmApi` referenced 114.0.0 while the bot used 114.1.0, which NuGet then used for both. It now references 114.1.0 too, so the library builds against what it runs with.
- **`Config` as a singleton (investigated 2026-11-18):** `Config` is registered transient. Each instance builds its own configuration with `reloadOnChange`, which watches `Assets/` and is never disposed.
  - Measured with a throwaway program: 500 instances held about 500 extra OS handles until a full garbage collection released them. So it isn't a leak, but each `Config` a command resolves keeps a handle open until the next collection.
  - One long-lived `Config` picked up an edit to `config.json` within 1.5 seconds, so a singleton would still apply edits without a restart.
  - The change: `AddSingleton<IConfig, Config>()` in `Startup`, and the "transient" note in CLAUDE.md.
- **Generic `BaseService`** (`DatabaseORM/Service`): turn it into a generic class so simple DB calls need less boilerplate. Each service would still declare its own methods. `GenericRepository<T>` already exists on the repository side.
- **Last.fm top tags:** a new command for `user.getTopTags`. That endpoint has no period or pagination.
- **Last.fm artist stats image (found 2026-11-16):** `LastFmAPI.GetArtistDataAsync` picks the MBID from the albums when there are any, but always passes `tracks[0]`'s artist and track name to the Spotify search.
  - An artist with album plays but no track plays would throw there (rare, since album plays come from track plays).
  - Passing a track name also makes the search return that track's album cover instead of the artist's image, as `GetTopArtistsAsync` does.
  - Decide which image the stats should show, and take both values from the same list.
- **Hand-written results in `Services/Models` (found 2026-11-16):** `Services/Models/LastFm` (`ArtistStats`, `LastFmListResult`, `NowPlayingResult`, `WhoKnows`) and `Services/Models/SpotifyAPI/SpotifyImageSearchResult` are the bot's own result DTOs, not JSON models.
  - Because the folder is marked as generated code, they get no nullable checking and no analyzer warnings.
  - Moving them to `Communication/` (where data types belong by convention) would bring both back. Their nullable fields are `Message`, the image URLs and the optional now-playing data.
- **Code Cleanup edits the scaffolded models (found 2026-11-16):** `DatabaseORM/Model` isn't marked as generated code, so that its `?` annotations count (see CLAUDE.md "Database changes"). Visual Studio's Code Cleanup therefore changes those files; for example, it removes unused usings even with IDE0005 off for the folder.
  - Until this is fixed, leave the folder out of Code Cleanup, or re-scaffold afterwards.
  - Fix: mark the folder `generated_code = true` again, and have the scaffold write `#nullable enable` at the top of each model file, which turns nullable back on in generated code. The folder's `charset = utf-8` (no BOM, as EF Core 11 scaffolds) stays as it is.
  - To do that, install EF's templates (`dotnet new install Microsoft.EntityFrameworkCore.Templates`, from nuget.org), run `dotnet new ef-templates` in the `DatabaseORM` folder, add the directive to `CodeTemplates/EFCore/EntityType.t4`, and re-scaffold once. `Scaffold-DbContext` and `dotnet ef dbcontext scaffold` use those templates automatically.
  - The templates must stay CRLF; LF templates are reported to produce an extra blank line per line. `.gitattributes` already checks them out that way.
  - The cost: the templates become the project's own copy, so after an EF upgrade, refresh them and re-add the line.
- **Culture in interpolated strings (found 2026-11-16, done 2026-11-18):** CA1305 doesn't cover interpolated strings, so their numbers and dates followed the culture of the Windows account that runs the bot (the dev PC is hu-HU). Only two reached Discord, and both are fixed:
  - **The Last.fm top lists:** the percentage now uses `CultureInfo.InvariantCulture` ("33.33%"; before, "33,33%" under hu-HU).
  - **The reminder DM:** the time is now a Discord timestamp, which Discord shows in each reader's own time zone and language. Before, it was the UTC time in the machine's date format, without saying it was UTC.
  - **Found along the way, the same fix:** the reminder list already used a Discord timestamp, but on the date read from the database. SQL Server returns it without a `DateTimeKind`, and `TimestampTag` then reads it as the machine's local time. On a machine set to Central European time, the list showed every reminder one hour early (two in summer); on a server set to UTC, it was right.
    - `DiscordTools.UtcTimestamp` marks the time as UTC first. The reminder DM, the reminder list, the "I will remind you" reply and the two mute messages all use it now.
  - Checked with a throwaway program on the hu-HU, Central European dev PC: the top list shows "33.33%", and a database date of 14:00 UTC becomes `<t:1795010400:f>`, which is 14:00 UTC.
- **YouTube playlist start point:** let the user choose which song or index a playlist import starts from (`YoutubeAPI.PlaylistSearch`). It currently always takes the first page of 50 (`MaxResults`).
- **YouTube duplicate on playlist import:** when a video link that also carries a playlist (`&list=`) is imported as a playlist (`YoutubeAPI.AddPlaylistAsync`), the video itself can be queued twice if it is in the imported part of the playlist. Skip it.
- **YouTube search matching:** look into a similarity or points-based ranking of search results, beyond the current channel and filter-word filtering in `YoutubeAPI.FilteredResults`. It first came up searching for "Withcha" by Apink.

## Feature ideas

- Halloween game
- Blackjack game
- Christmas game
- A command that counts a user's messages or files
