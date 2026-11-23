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

- **Run the bot once against the dev database and Discord.** Since the last real run, the database layer moved into `DatabaseORM`, implicit usings were turned on, and Windows Forms was removed (2026-11-17). Since then, `Startup.Run()` was split, `IConfig` added, `TimeProvider` injected, `Global` replaced by the state stores `LastFmApi` made instance-based, `IBrowserService` and `IWebService` added, and the commands, interactions and features moved onto Discord.Net's interfaces, and YouTube and Spotify put behind `IYoutubeDataClient` and `ISpotifyClientFactory` (2026-11-18). Worth a try: a Spotify track and playlist link, a mute (by name and by ID) and an unmute, the R6 randomizer from a voice channel, a weekly poll edit, `!leave` while playing, a Last.fm top list and who-knows (with its image), a bonk, a reminder (`!remind in 1m>test`), a short mute, a self-role request, a bias game (start, stop, and one played to the end), and music (a YouTube search, a playlist link, and waiting until the inactivity disconnect).
  - What has been checked: builds, `dotnet format` and throwaway programs. Those resolved every DB service through the real `Startup.Run()`, ran `ProcessTools.GetStatistics()` (RAM figure, WMI child process count) and loaded a resx bitmap.
  - Worth a look in the window: the diagnostics line.
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

## 2. Test project (investigated 2026-11-15 to 2026-11-17, the project exists since 2026-11-19)

`Discord Bot Tests/` is set up (see "Project setup" below and CLAUDE.md "Tests"). The rest of this section is the investigation behind it and what is still to test.

### Already in good shape

- **The DB layer is a plain `net11.0` library.** `DatabaseORM` has no WPF or Discord.Net dependency.
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

Blockers 1 to 4 and the interfaces below are done (2026-11-18). Blocker 5 is handled per command, as each one gets tests.

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
     - Keys edited in `config.json` apply at the next refill (08:00 UTC since 2026-11-19, or a restart). A duplicate key counts once instead of throwing at startup.
3. **`Config` (done 2026-11-18).** The 47 classes inject `IConfig` (`Interfaces/IConfig.cs`) instead, so tests mock it. `DatabaseStartup` takes the connection string as a `Func<IServiceProvider, string>` and `Startup` reads it through `IConfig`, so replacing `IConfig` in a test also replaces the database. Checked with a throwaway program: `ValidateOnBuild` passes on the real registrations, and a fake `IConfig` reached the DbContext.
4. **Concrete Discord.Net socket types (done 2026-11-18, the rule is in CLAUDE.md "Command conventions").**
   - `BaseCommand` derives from `ModuleBase<ICommandContext>` and `BaseInteraction` from `InteractionModuleBase<IInteractionContext>`. `BaseFeature.Context` is an `ICommandContext`, and `DiscordTools` takes the interface contexts.
   - The seven features that injected `DiscordSocketClient` take `IDiscordClient`, which `Startup` registers as the same client.
   - Commands, features, tools and processors use `IGuild`, `IGuildUser`, `IRole`, `IVoiceChannel` and `IComponentInteraction` instead of the socket types. Only `CommandHandler`, `InteractionHandler`, `BotMain`, `App` and `Startup` still use the socket types, since they are where the gateway connects.
   - **The same behavior:** socket lookups that read the cache synchronously (`Guild.GetUser`, `Guild.CurrentUser`, `client.GetGuild`, `client.GetChannel`) became the interfaces' async ones with `CacheMode.CacheOnly`, so nothing calls the REST API where it didn't before. `RainbowRandomizerTools` finds the voice channel's users from the member cache, the way `SocketVoiceChannel.ConnectedUsers` does.
   - **Fixed along the way:**
     - Muting had failed since 2025-02-01 (`34c869b`, which made the command look the user up by name or ID itself). It still cast the user to `SocketGuildUser`, but a user found by name is a `RestGuildUser` and one found by ID was a global user, so both threw. It now looks up a server member (by ID with `Guild.GetUserAsync`) and works with `IGuildUser`; someone not on the server gets "No user was found".
     - `CommandHandler.CheckCustomCommandAsync` only ran custom commands for a `SocketCommandContext`; it now takes any context.
     - A birthday of a user who left the server threw on `user.Mention`, which stopped the birthday messages of every server after it. That user is now skipped with a warning.
   - Checked with a throwaway program: real text commands (`!bias game`, `!bias game stop`) ran through `CommandService` with a fake, non-socket context and sent their replies, and the gender button ran through `InteractionService` with a fake `IInteractionContext`.
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
2. **`BrowserService` → `IBrowserService` (done 2026-11-18).** The interface has the two members its users call, `NewPage` and `CloseBrowser`; `OpenBrowser` became private, since only `NewPage` uses it.
   - `BiasScrapingProcessor` (136 lines: matching scraped idols to the database, the group check, image changes) only uses it to close the browser at the end. It can now be tested with mocked `IBrowserService`, `IKpopDbScraper` and `IIdolService`.
   - Optional: to test the scrapers' HTML parsing (`KpopDbScraper.ParseRow` and the group scraping), split loading a page from parsing it, so the parsing takes the page content and tests can feed it saved pages.
3. **`WebTools` → `IWebService` (done 2026-11-18).** `GetStream`, `GetBody` and `TestConnection` moved to `Services/WebService.cs`, a singleton behind `Interfaces/Services/IWebService.cs`. It's named a service because `Tools/` holds only static helpers.
   - The old class created a new `HttpClient` for every call (not a static one, as this note said before). `WebService` keeps one, with connections renewed every 15 minutes so DNS changes still apply, and disposes each response.
   - `LastFmWhoKnowsEmbedProcessor`, `BiasGameImageProcessor`, `InstaScraper`, `InstagramEmbedFeature`, `UserBonkCommands` and `BotMain` inject it. The static `SocialMessageProcessor.GetAttachments` takes it as a parameter.
   - `WhoKnowsImageProcessor` is behind `Interfaces/Processors/IWhoKnowsImageProcessor.cs`, so `LastFmWhoKnowsEmbedProcessor` (download, draw, fall back to a plain embed) can be tested without a download or Magick.NET.
   - Checked with a throwaway program:
     - Against a local HTTP server, `WebService` returned a PNG byte for byte, converted an AVIF to PNG, and returned page text, including five calls at once.
     - With a fake `IWebService` and `IWhoKnowsImageProcessor`, the who-knows embed fell back to the plain embed when the image failed and attached it when it worked.
4. **YouTube (done 2026-11-18).** `YoutubeAPI` (watch, playlist and youtu.be links, keyword search with filtering, the playlist import, key rotation and quota counting) calls YouTube through `IYoutubeDataClient` (`Interfaces/Services/`).
   - It has the three calls the bot makes (video by ID, keyword search, playlist items), each taking the API key, so the rotation stays in `YoutubeAPI`. A test returns Google's own response classes (`VideoListResponse` and so on, plain data classes), or throws a `Google.GoogleApiException` with "quotaExceeded" in it.
   - `YoutubeDataClient` keeps one `YouTubeService` per key for the bot's lifetime. Before, `YoutubeAPI` created one, with its own HTTP client, for every search.
   - `YoutubeAddPlaylistFeature` got no interface after all. Since blocker 4 it takes only `IDiscordClient` and `TimeProvider`, so a test builds the real feature over fakes (a channel that isn't found means "no").
   - The key quotas and the music queue are in `YoutubeKeyStore` and `AudioStateStore`; tests use real ones.
   - Checked with a throwaway program, with a fake `IYoutubeDataClient` behind the real container:
     - A keyword search queued the first result, and a youtu.be link made a single video call.
     - A playlist whose third video hit the quota kept the two queued songs and didn't repeat them.
     - A used-up first key switched to the next one and queued the song once.
   - **The key rotation never ran (found and fixed 2026-11-18).**
     - The cause: `VideoSearch`, `KeywordSearch` and `PlaylistSearch` caught every exception from `ExecuteAsync`, logged it and returned "not found", so the quota error never reached the key switch in `Searching`. Until YouTube's own reset (midnight Pacific time), every search on a used-up key failed, even when other keys had quota.
     - It dates from `b33c2ae` (2023-08-27, "YoutubeAPI reimplemented"), which added the catch blocks and the key switch together; no later change touched them. The earlier version, which `KeyReset` comes from, isn't in this repository.
     - The fix: the three searches let quota errors through (`catch ... when (!IsQuotaExceeded(ex))`, the same string match as before). `RunWithKeyRotationAsync` repeats the request with each remaining key until one works, for searches and for the playlist import.
     - Repeating must not queue anything twice. So the "add the playlist?" offer for a watch link with `&list=` now starts after the video search. A playlist import that runs out of quota after queuing songs keeps what it queued, drops the key and stops, instead of starting over.
     - Checked with a throwaway program against the Google library, with a fake HTTP layer that answers YouTube's 403 `quotaExceeded`. The committed version swallowed the error and returned "not found". With the fix, the error reaches the rotation, the keys are tried in turn until one works, the song is queued once, and the search stops cleanly when no key is left. Other errors are still logged and return "not found".
   - The quota units counted per key (`YoutubeKeyStore.AddUsage`) are never read. They could decide which key to use next, or show in the diagnostics; otherwise drop them.
5. **Spotify (done 2026-11-18).** `SpotifyAPI` (link handling, playlist or album → search terms, the image search fallback) gets SpotifyAPI.Web's own `ISpotifyClient` from `ISpotifyClientFactory`.
   - `SpotifyClientFactory` (a singleton) keeps one client while the credentials in `config.json` stay the same, so an edit still applies without a restart. Before, every call built a new client, which also asked Spotify for a new token each time.
   - A test mocks the factory to return a mocked `ISpotifyClient` (`Tracks`, `Playlists`, `Albums`, `Artists` and `Search` are interfaces too). The throwaway program checked a track link becoming the YouTube search "name artist", and the factory reusing its client until the secret changed.
6. **`DiscordSocketClient` (done 2026-11-18, blocker 4).** The seven features and `DiscordTools` take `IDiscordClient`; only the gateway classes keep the socket client.

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
- **Randomness:** `new Random()` appears in `CollectionTools`, `StringExtension.ToMockText`, `BirthdayMessageProcessor` and `WhoKnowsImageProcessor` (file names); `YoutubeKeyStore` picks a key with `Random.Shared` (checked 2026-11-23). Tests can check properties (a shuffle keeps every item), or the code takes the injected `Random`, as `EasterEggFeature` does since 2026-11-19 and `!my biases` and the randomizer commands (with `RainbowRandomizerTools`) since 2026-11-23 (described in CLAUDE.md "Randomness").
- **Files:** `RainbowRandomizerTools.GetOperatorList` reads `R6_Attackers.txt` / `R6_Defenders.txt` from the working directory. Copy them to the test output, or pass the list in.

### Not worth unit-testing

- CLI wrappers (`yt-dlp`/`ffmpeg`, `twitch.exe`), Puppeteer scrapers, live HTTP APIs. They are already behind interfaces; mock them where consumers are tested.
- Image processors: at most smoke tests ("produces a non-empty image of size X").

### Repositories and the database

Don't use EF InMemory or SQLite. The schema uses the `SQL_Latin1_General_CP1_CI_AS` collation (case-insensitive comparisons that queries rely on, see the CA1862 suppression) and `getutcdate()` defaults, so results would differ.

The repository tests, planned 2026-11-17 and built on 2026-11-19 as `Integration/Database/` (described in CLAUDE.md "Tests"):
- **Run them against SQL Server LocalDB.** `MSSQLLocalDB` is installed on the dev PC (SQL Server 2025, `Microsoft SQL Server/170`).
  - **The default instance doesn't start (found 2026-11-19).** It was created by LocalDB 2019 (v15), and only the 2025 version (v17) is installed now. The Application event log says "LocalDB parent instance version is invalid: MSSQL15E.LOCALDB".
  - **Fixed 2026-11-19** with `sqllocaldb delete MSSQLLocalDB`, then `sqllocaldb create MSSQLLocalDB`. It is now version 17.0.4025.3, starts on first connection, and its server collation is `SQL_Latin1_General_CP1_CI_AS`, the same as the schema's.
  - The 2026-11-19 checks below used an instance of their own instead (`sqllocaldb create <name> -s`, then `sqllocaldb stop` and `sqllocaldb delete`). `delete` leaves the instance's log folder under `%LOCALAPPDATA%\Microsoft\Microsoft SQL Server Local DB\Instances\`.
  - Those checks also confirmed the approach: `EnsureCreatedAsync()` on a unique database name, seeded rows with fixed `ChannelTypeId`s, the real services and repositories with a fresh `MainDbContext` per call, then `EnsureDeletedAsync()`.
- **Create the schema with `Database.EnsureCreated()` from the scaffolded model.**
  - It carries `UseCollation("SQL_Latin1_General_CP1_CI_AS")` and all 26 `getutcdate()` defaults, so the result matches the SSDT schema closely enough.
  - Then run `MainDB/Post-deployment/ChannelType_Inserts.sql` for the 6 lookup rows. The test project links that file into its output (`Database/`), and `LocalDatabase` runs it without its `USE [$(DatabaseName)]` line.
- **Why not the other two schema sources:**
  - `Latest_Release.sql` is git-ignored, and the dev PC's copy is from 2023.
  - Publishing `MainDB.dacpac` (DacFx or SqlPackage) would be exact, but it needs a Visual Studio build of `MainDB` first.
- **Isolation:** give each test collection its own database (a unique name, created at the start and dropped at the end), so collections run in parallel. Within one collection, reset between tests.
  - **Done 2026-11-19 without Respawn.** Each test class is its own collection and gets a `LocalDatabase` class fixture.
  - `ResetAsync` empties every table except `ChannelType` before each test. It lists the tables from the EF model, join tables included, and switches the foreign keys off meanwhile.
  - The six classes run in parallel; the whole suite still takes about 4 seconds.
  - **A run that is killed** leaves its `DiscordBotTests_<guid>` databases behind. To list them:
    ```powershell
    sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name LIKE 'DiscordBotTests[_]%'"
    ```
    Drop each with `DROP DATABASE [name]`.

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
   - Possible since blocker 4 (2026-11-18), and tried with a throwaway program:
     - For a command, `new CommandContext(fakeClient, fakeMessage)` (Discord.Net's own non-socket context) is enough, after `commands.AddModulesAsync(...)` on the real container. Pass `argPos: 1` for the `!` prefix.
     - For a button or modal, the fake context must implement `IRouteMatchContainer` as well as `IInteractionContext`. Without it, the `*` parts of custom IDs such as `BiasGame_Setup_Gender_*_*` can't be read.
     - Fakes of the Discord interfaces (message, channel, user, interaction) can be NSubstitute substitutes. The throwaway program used a small `DispatchProxy` that answers from a table.

**The one enabler they share (done 2026-11-18):** `Startup.ConfigureServices(IServiceCollection)` registers everything, and `Startup.Run()` only builds the provider from it. A test calls `ConfigureServices` on its own collection and replaces only the edges:
- the Last.fm `RestClient`
- `ISpotifyAPI`, and other external services as needed
- the connection string, by replacing `IConfig` (see blocker 3)

**Not worth it:**
- A real Discord gateway: it would need a second bot account and a test server, and would be slow and flaky.
- Live external APIs as part of the normal run. An optional, opt-in live Last.fm test (a trait, a real key, network) could warn when a saved response no longer matches the real format.

**Where:** in `Discord Bot Tests/Integration/<theme>/`, without a trait. The whole run takes about 2 seconds, so there's no quick run to split off yet; the tests could be filtered by namespace if LocalDB makes the run slow.

### Project setup (done 2026-11-19, described in CLAUDE.md "Tests")

- **Decided 2026-11-19:** xUnit v3, NSubstitute, and one test project for all three projects (`Discord Bot Tests`, Windows/WPF like the bot), with `Unit/` mirroring each project, `Integration/` by theme, `Mocks/`, and `Common/Constants/` and `Common/Tools/` for the shared values and helpers.
- **Packages (2026-11-19):** xunit.v3 4.0.3, xunit.runner.visualstudio 4.0.1, Microsoft.NET.Test.Sdk 18.12.0, NSubstitute 6.2.0, Microsoft.Extensions.TimeProvider.Testing 11.0.0.
- **Setup:**
  - DatabaseORM lets the tests and NSubstitute's proxies see its internals (`InternalsVisibleTo` in its project file).
  - The root `global.json` makes `dotnet test` use Microsoft.Testing.Platform. Without it, the .NET 11 SDK refuses to run xUnit v3 ("Testing with VSTest target is no longer supported").
  - The solution lists the test project with only its own Debug/Release lines: `dotnet sln add` also added x64 and x86 platforms to every project, so those were left out.
  - **Test Explorer warns "No test is available in `MainDB\obj\Debug\MainDB.dll`" (investigated 2026-11-19).** It is harmless, and no project setting stops it:
    - Visual Studio chooses which projects to scan by its own rules. The vstest maintainers say they can't change those rules ([microsoft/vstest#2130](https://github.com/microsoft/vstest/issues/2130), closed with "There isn't unfortunately a way to avoid this"). A 2023 comment there says `<IsTestProject>false</IsTestProject>` and `<TestProject>false</TestProject>` don't stop the scan.
    - The `TestContainer` capability decides this only for SDK-style projects. `MainDB` is a legacy SSDT project, so it doesn't apply.
    - The only route found is turning `MainDB` into an SDK-style SQL project (section 8). Untested.
- **The test cases (theories count once per case):**
  - Unit:
    - `DateTimeTools`; the top-list percentage under hu-HU; `DiscordTools` (UTC timestamps, channel types)
    - `BiasGameStore` and `YoutubeKeyStore`; the debut-year menu
    - `YoutubeAPI` (search, links, key rotation, a playlist that runs out of quota)
    - `WeeklyPollTools`; `LastFmHelper`
    - The Mapster mappings (added 2026-11-19, 48 more cases, 140 in all; see "Next" item 1)
    - Seven DB services over substitute repositories (added 2026-11-19, 79 more cases, 219 in all; see "Next" item 2)
    - The other twelve DB services and `UserIdolStatisticTools` (added 2026-11-19, 147 more cases, 404 in all with the LocalDB ones below)
    - The text, number, collection, URL and channel-type helpers, the list and server-settings embeds, and `TwitchNotificationFeature` (added 2026-11-23, 67 more cases, 597 in all; see "Next" items 3 and 9)
  - Integration on LocalDB (added 2026-11-19, 38 more cases, 257 in all; see "Next" item 5):
    - Settings channels, the server's row, roles and Twitch channels, and the cached server across scopes.
    - The bias list, reminders, birthdays and mutes.
    - The bias game's data (added 2026-11-19 with `!bias game stop` and the bias game embeds, 22 more cases, 476 in all).
    - Weekly polls and presets (added 2026-11-19 with the answer menus through the interaction service, the poll embeds and `PollTools`, 41 more cases, 528 in all).
  - Integration:
    - Every registration and every command and interaction module.
    - Last.fm top tracks through the real container with a fake HTTP layer.
    - The bias game in a direct message, from `!bias game` to the gender button.
    - A Spotify track link through the real `SpotifyAPI` and `YoutubeAPI`.
    - `!remind`, `!birthday` (and the admin's `!birthday a`), and `!help` with its category menu, plus `BaseCommand` and the message processors as unit tests (added 2026-11-23, 79 more cases, 674 in all; the container test lost two cases when the base classes became abstract; see "Next" item 7)
    - The replies for input the reminder, birthday and mute commands couldn't read (added 2026-11-23, 16 more cases, 690 in all)
    - The server setup and owner commands, and the LocalDB queries for custom commands, greetings and users (added 2026-11-23, 58 more cases, 748 in all; see "Next" items 5 and 7)
    - The bias commands, `!ping` and its query on LocalDB, and the idol edit menu with its modals (added 2026-11-23, 74 more cases, 822 in all; see "Next" item 7)
    - The weekly poll and preset text commands, the randomizer commands and `!wotd` (added 2026-11-23, 45 more cases, 867 in all; see "Next" item 7)
    - Who may manage weekly polls and presets: refusals for a member who can create polls and for an administrator who isn't the owner (added 2026-11-23, 2 more cases, 869 in all)
- **Next, roughly in order of value:**
  1. The Mapster mappings (`DatabaseMapperConfig`, `MapperConfig`). **Done 2026-11-19:**
     - The tests cover every hand-written mapping, including navigations a query didn't load. One theory compiles each resource mapping with `RequireDestinationMemberSource`, so a new resource member without a mapping fails it.
     - They found that a missing group, server or user gave `null` in `IdolResource.GroupName`, `IdolExtendedResource.GroupName` and the two `ServerMutedUserResource` IDs, which are non-nullable. These now give `""`.
     - They found three members that are filled after the mapping. `SettingsChannels`, `IdolExtendedResource.LatestImageUrl` and `UserResource.Username` are now `.Ignore`d, with a comment saying who fills them.
     - `TwitchChannelResource.TwitchDiscordId` was never set or read, so it's removed.
     - Not covered: the "Days Of Week" options in the weekly poll mapping read the system clock (see "Left on the system clock" above); `WeeklyPollToolsTests` covers them.
  2. DB services with mocked repositories. **Done 2026-11-19, all 19 services:**
     - First batch: `ServerService`, `ChannelService` and `TwitchChannelService` (the three that change cached server settings, so each write is checked to drop the cached server), `ReminderService`, `BirthdayService`, `ServerMutedUserService` (the dates and times) and `UserIdolService` (the bias list results).
     - Second batch: `IdolService` (including what the weekly bias scraping does to each idol), the three weekly poll services (including the field updates from the edit message's buttons), `UserService`, `RoleService`, `CustomCommandService`, `GreetingService`, `IdolAliasService`, `IdolGroupService`, `IdolImageService` and `UserIdolStatisticService`.
     - **Fixed in the second batch:**
       - The idol and group edit modals show dates as `yyyy. MM. dd.`, but `IdolService` and `IdolGroupService` parsed them in the machine's culture. Under German (`de-DE`), that format can't be read, and the edit silently kept the old date.
       - Checked 2026-11-19: Hungarian, both English cultures and the invariant culture read it, as well as `2000-01-02` and `2000.01.02`. The three parses now use `CultureInfo.InvariantCulture`, as CLAUDE.md asks, and tests run them under `de-DE`.
       - `RoleService.ChangeServerMuteRoleAsync` only threw `NotImplementedException` and wasn't on `IRoleService`. The mute command uses `ServerService`'s method, so the dead one is removed.
     - The repositories are substitutes answering from rows (`HasRows`, described in CLAUDE.md "Tests"), so these tests can't see a missing `Include`.
     - Three bugs were found while reading the services, confirmed on a throwaway LocalDB database, and fixed (2026-11-19):
       - **Removing one settings channel always answered "not found",** and adding a type a channel already had failed with a primary-key violation instead of "already exists". `ChannelService`'s single-channel lookups included `Server` instead of `ChannelTypes`, so the channel's types were never loaded. The cause was the generic-repository refactor in `28b13de` (2024-03-31); the old `GetChannelByDiscordIdAsync` included `ChannelTypes`.
       - **Removing a bias by `name>group` threw a null reference,** which is the form the bot asks for when two biases share a name. `UserIdolService.RemoveUserIdolAsync` filtered the user's idols on `Group.Name`, but the groups weren't loaded with them. The database now finds the matching idols, as `RemoveUserIdolGroupAsync` already did. That also makes the name comparison case-insensitive, like the other lookups.
       - **`!remind remove` with a number past the end logged an error** and answered "Reminder could not be removed!" instead of "Reminder doesn't exist with that ID". `ReminderRepository.GetByIndexAsync` used `FirstAsync`, and 0 or below sent a negative `OFFSET` to SQL Server. It now returns null for both.
     - Seen, not changed:
       - `IdolService.ChangeGroupAsync` loads the idol without its group, so an empty group name would set the group to null. Its caller shouldn't pass one.
       - `UpdateIdolDetailsAsync` only skips a scraped image that someone replaced. That is enough, because `BiasScrapingProcessor.UpdateBiasAsync` doesn't call it when the idol's newest image already has the scraped URL. A first reading of the service alone wrongly suggested the weekly scrape re-adds images; the live table's 767 rows agree that it doesn't (checked 2026-11-19).
       - `WeeklyPollService.GetPollByIdAsync` includes neither the server nor the preset, so its result has `ServerDiscordId` 0 and no answers for a preset poll. Its only caller, the edit interaction, reads just the name, title, channel and role, so it's harmless today.
       - `RoleService.RemoveSelfRoleAsync` on a role that is also the server's notification or mute role would probably fail on the foreign key, because the server row points at it. Unverified; a LocalDB test could check.
  3. The features: reminder, unmute, birthday and weekly poll with the fake clock and an `IDiscordClient` substitute. **Done 2026-11-19** (`Unit/Bot/Features/`, 29 cases, 433 in all):
     - Reminders: what's due by the start of the minute, the DM text, and removing them.
     - Unmutes: the mute role off, the removed roles back, the DM, a user or server that's gone.
     - Birthdays: the greeting in the birthday channel, and the server, channel or user that's missing.
     - Weekly polls: the poll and role mention in its channel, pinning, and a poll without answers or channel.
     - New mocks for this:
       - `DiscordMocks.Guild` and `GuildUser`.
       - `RecordingChannel.Text`, and `DirectMessagesOf` (the user's DMs, which Discord.Net's `SendMessageAsync` extension opens).
       - `DiscordErrors`: the `HttpException`s Discord.Net throws for closed DMs (50007) and missing permissions (50013).
       - `SentMessage` now also records a sent poll and the message the channel returned.
     - The birthday greeting is picked at random and only some greetings give the age, so the test checks only the mention.
     - **One failure stopped the rest of the run (found and fixed 2026-11-19).**
       - Each feature had one try/catch around its whole loop, so the first exception Discord threw ended the run. A throwaway test confirmed it, failing the first of two items as Discord does.
       - **Reminders:** nothing after the failing one was sent, and none was removed. The scheduler gets the same list every minute, so a failing reminder blocked those behind it indefinitely, and any sent before it were sent again every minute.
       - **Unmutes:** the failing user's roles came back but their mute entry stayed, so it was retried every minute, and users after them were never unmuted.
       - **Birthdays and weekly polls:** the servers after the failing one got nothing that day.
       - Not seen on the live bot. The log would show `ReminderFeature.cs ExecuteCoreLogicAsync` or `UnmuteFeature.cs ExecuteCoreLogicAsync` errors every minute.
     - **The fix:** each item now runs on its own, and a failure is logged without stopping the others. With the pre-fix code, five of the six new failure tests fail, the weekly poll one included.
       - **Reminders (decided 2026-11-19):** a reminder whose DM fails stays for the next minute, in case Discord's problem was temporary. Only the sent ones are removed, along with those whose user Discord doesn't know anymore. A user with closed DMs is therefore retried, and a warning logged, every minute. If that gets noisy, give up after some time.
       - **Unmutes:** a failed DM is only a warning, and the entry is still removed once the roles are back. Kept, the old entry would run again and take the mute role off a user who was muted again meanwhile. If the role change itself fails, the entry stays for the next run.
       - **Birthdays and weekly polls:** a failure is logged and the next server's message is sent. A message that failed isn't retried, since they run once a day.
       - `Run()` now returns false when any item failed. The scheduler doesn't read it.
       - **Corrected 2026-11-19 after commit `1b0aa13`:** in that commit the unmute loop reads `allUnmuted = allUnmuted && await TryUnmuteAsync(...)`. The line was rewritten outside the session before the commit, possibly by an IDE quick fix. `&&` stops calling `TryUnmuteAsync` after the first failure, which brings back the bug the fix removed. `Run_RoleChangeRefused_KeepsThatMuteForTheNextRunAndUnmutesTheOthers` caught it. The loop now uses an `if`, which no short-circuit suggestion applies to.
     - `SelfRoleFeature` too (2026-11-19, 6 cases, 439 in all): `+role` and `-role`, the reply deleted after 1.5 seconds on the fake clock, a role that isn't on the list, one deleted on Discord, and Discord refusing the change.
     - `CustomCommandFeature` and `EasterEggFeature` too (2026-11-19, 15 cases, 454 in all):
       - Custom commands: the server's link for `!name`, an unknown name, and a channel that refuses.
       - Easter eggs: the greeting for a mention, the 1-in-5000 message, and the 1-in-20 replies to "I think" and "I am".
       - `EasterEggFeature` now takes the injected `Random`, so the tests decide each roll.
     - **Fixed with them (confirmed by tests first):**
       - **Mentions in the older `<@!id>` form weren't recognized.** The second check was `CurrentUser.Mention.Remove(2, 1)`. Discord.Net 3.20 gives `<@id>`, so that drops the first digit of the ID and can never match. A probe showed `<@123456789012345678>` becoming `<@23456789012345678>`. Both forms are now checked by ID. Current Discord clients send `<@id>`, so the impact was small.
       - **In a direct message, the 1-in-20 reply logged an error instead of replying.** `CommandHandler` runs this feature for DMs too, where `Context.Guild` is null, and the reply's `MessageReference` read `Context.Guild.Id`. It now passes `Context.Guild?.Id`.
     - **Every role the bot stores is a self-role, by design (checked on LocalDB 2026-11-19).**
       - The mute, notification and poll ping roles are kept in the same `Role` table as the self-roles. So they are listed by `!update role message` and found for `+name`/`-name` in the role channel.
       - That is intended: the notification and poll ping roles are opt-in roles. A user who gives themselves the mute role has only themselves to blame.
       - Known consequences, accepted:
         - A muted user who can still post in the role channel could take the mute role off with `-muted`; that depends on the server's channel permissions.
         - `!self role remove` fails while the role is in use as the mute, notification or poll role (its foreign key), and the admin is told it could not be removed.
     - `TwitchNotificationFeature` too (2026-11-23, 5 cases): the post in the notification channel with the role ping and the stream's embed (the thumbnail sized 1024x576), no role or title, no notification channel, a deleted channel, and a server that didn't load. Nothing new turned up. The notification channel is one per server, so a failure can't stop other channels.
  4. More Last.fm (recents, now playing, who-knows, artist stats). Saved responses recorded from the real API would replace the hand-built JSON in `LastFmResponses`.
  5. The DB layer on LocalDB (see "Repositories and the database"). **First batch done 2026-11-19:**
     - Six classes in `Integration/Database/` cover the real services, repositories and cache:
       - `SettingsChannelTests` and `ServerSettingsTests`: the settings, with the cached server read before and after each change
       - `BiasListTests`: the name, alias and group lookups (in another letter case too), the user-idol links, clearing the list
       - `ReminderTests`, `BirthdayTests` and `MuteTests`: positions, due reminders, the day-of-year query and list order, and the "still muted" check
     - They guard the three bugs fixed under item 2: with the pre-fix code, exactly those five cases fail.
     - **Second batch done 2026-11-19:** `BiasGameDataTests` (10 cases).
       - The idol picker (`GetListForGameAsync`): the gender, the debut years at both ends, no debut or no image left out, at most 16 different idols, each with its newest image and "Soloist" for a group without a full name.
       - The top-10 statistics: the weights (a win counts 2, a second place 0.5, a quarter-final 0.125), the order, the heart for biases, the gender filter and the cut at 10. An idol without images gets no image: SQL Server answers the projection's `.First()` with NULL, where LINQ to objects would throw.
       - Two whole games through `UpdateUserStatisticsAsync`, read back by `!bias stats`'s query.
       - The newest image (`GetLatestByIdolIdAsync`), and an override recording the image it replaces.
       - Nothing new turned up.
     - **Third batch done 2026-11-19:** `WeeklyPollTests` (13 cases).
       - Polls are built the way the edit message builds them: a draft, then its name, then each answer created empty and given its title.
       - Covered: the day and active filters, the preset's answers through the nested include, the "Days Of Week" preset, the draft left out of the list, and the same channel reused.
     - `Integration/Polls/AnswerEditTests` drives the answer menus through Discord.Net's interaction service on LocalDB: a choice from the real menu, then the rows written and the modal opened.
     - **Fixed in this batch (each confirmed on LocalDB first, 2026-11-19):**
       - **Removing a poll with its own answers failed** on `FK_WeeklyPollOption_WeeklyPollId`: the poll was deleted without its answer rows. Its answers are now removed with it, in the same save.
       - **Removing a preset left its answers behind** with both keys null; they are now removed with it.
       - **A poll's own answers couldn't be added through its edit message.** The poll's answer menu said `PollOption_Change_CustomOption_True_<pollId>`, so `WeeklyPollOptionEditInteraction` saved the new answer as a preset's. With no preset of that ID it failed on `FK_WeeklyPollOption_WeeklyPollOptionPresetId` ("The poll option could not be found."); with one, the answer went to that preset. It now says `False`.
       - **A preset's existing answers couldn't be edited.** The preset's answer menu gave each answer the preset's ID instead of its own, so the bot looked up the wrong answer. It answered "could not be found", or opened a different answer whose ID happened to match.
       - Both menu slips came with `d07284f` (2025-03-01, "added logic for preset editing"), which added the true/false part to the custom ID.
     - **Removing a preset that a poll still uses is refused (decided and done 2026-11-19).** It used to fail on `FK_WeeklyPoll_WeeklyPollOptionPresetId` with "Preset could not be removed!".
       - `RemovePresetByNameAsync` now returns the new `DbProcessResultEnum.InUse`, and `!weekly poll preset remove` says the preset is still used by a weekly poll and to switch that poll to another preset or its own answers first.
       - No preset had been removed on the live bot before, so there are no orphaned answers to clean up.
     - **To check on the live database:** poll answers that went to a preset with the same ID through the old menu. They show as two answers in one slot of a preset:
         ```sql
         SELECT WeeklyPollOptionPresetId, OrderNumber, COUNT(*) FROM WeeklyPollOption WHERE WeeklyPollOptionPresetId IS NOT NULL GROUP BY WeeklyPollOptionPresetId, OrderNumber HAVING COUNT(*) > 1
         ```
     - **Fourth batch done 2026-11-23:** `CustomCommandTests` (names per server and in any letter case, the list by name, removal), `GreetingTests` (removal by the listed ID) and `UserTests` (the Last.fm name: a new user gets a row, an existing name is kept, the list leaves out empty names, and clearing it). Nothing new turned up.
  6. `BiasScrapingProcessor` and the who-knows embed. The other bias game embeds are tested (2026-11-19): the match and its arrows (picking one idol sends the other's ID), the result, the setup's gender buttons, and the stats list. A test also checks that the gender buttons' numbers are `GenderEnum`'s values.
  7. More whole commands. Guild commands need an `IGuild` substitute and a `ServerResource` from a substituted `IServerService`. **First batch done 2026-11-23:**
     - `Integration/Reminders`, `Integration/Birthdays` and `Integration/Help` run the commands through Discord.Net's command service:
       - `!remind in` and its aliases, in a server and in DMs, with the command channel restriction, the length limit, the list and the removal results.
       - `!birthday add/remove/list` and the admin's `a add` (by ID and by name) and `a remove`, with the guild-only and Manage Roles preconditions.
       - `!help` at each level and its category menu.
     - `Unit/Bot/Commands/BaseCommandTests`: the channel check (DMs, a type the server has or hasn't set) and the argument splitting.
     - The message processors (`Unit/Bot/Processors/MessageProcessor/`): the bias list and its group menus, a group's members, the birthday greetings, `!bias edit`'s menu read back by `BiasEditData`, and the role message. `SocialMessageProcessor` is left out, since only the disabled embeds use it.
     - Two convention tests over the real modules:
       - Every module's `[Remarks]` is a `CommandLevelEnum` name, and every module and command has a `[Summary]`.
       - Every help page and category page builds within Discord's limits (fields, empty values, 6000 characters).
       - Both passed for the bot's modules.
     - **Fixed (each confirmed by its test failing first):**
       - **`!remind in` dropped the part of a message after a `>`:** `1h>check if a > b` saved "check if a". Only the first `>` splits it now.
       - **`!birthday list` showed a user who left the server under the name of whoever asked.** `GetNickName(context, null)` falls back to the command's author. They are left out now, as the daily greeting skips them, and a list of only such users says there are none.
       - **`!bias list` and `!my biases` without a group failed past 125 groups.** The groups go into select menus of 25, and a message holds 5 rows; Discord.Net threw "Value must be less than 5". The first 125 are offered and the message says how many more there are, which can still be named in the command.
         - **To check on the live database:** `SELECT COUNT(DISTINCT GroupId) FROM Idol` above 125 means `!bias list` has been failing (logged as `UserBiasCommands.cs BiasList`).
       - `BaseCommand` and `BaseInteraction` are abstract now. Discord.Net loaded both as modules without commands, like `BaseFeature`, which already was.
     - **Commands that answered nothing (confirmed with a probe 2026-11-23, replies added the same day):** each of these returned without a reply, so the user couldn't tell what went wrong.
       - `!remind in` with an unknown unit (`1x`, or `1H`: units are case-sensitive because `M` is months and `m` minutes). It now says the amount couldn't be read, with the units and an example.
       - `!remind in` with a unit before its number (`h1`), a number too big for `int`, or a date past year 9999 (`10000y`). These three also logged an exception. `DateTimeTools.TryAddValuesToDate` now returns false for them, so they get the same reply.
         - `!mute` uses the same parser, so these durations now get its "The duration could not be read!" instead of an exception. That follows from the shared parser, and the mute tests cover it.
         - The parser also counted weeks in days as an `int`, which could wrap around: 613566757 weeks gave 3 days. They are a `double` now.
       - `!remind in` without a `>`: it now says to separate the amount and the message with `>`, with an example.
       - `!remind list` with no reminders: "You do not have any reminders set!"
       - `!birthday a add` without a `>` (it logged an `IndexOutOfRangeException`): "Incorrect input parameters", as for a date it can't split.
       - Left as it was: `!remind in tomorrow>...` (no number at all) still answers "Incorrect number of inputs!".
     - Seen, not changed:
       - `!remind at` took a date until the 2024-08-28 cleanup removed it (`890b7de`); it has been an alias of `!remind in` since.
       - The birthday date is parsed in the machine's culture (`DateTime.TryParse` without a provider), but year-month-day input reads the same under hu-HU, en-US, en-GB, de-DE, fr-FR and ja-JP. A test covers them.
       - `!birthday a add` by ID accepts any Discord user, not only the server's members.
       - `BiasListMessageProcessor`'s "unsorted" branch never runs, since every idol has a group (`Idol.GroupId` is `NOT NULL`, and both list queries load it).
       - The bias list's group order and the role message's order follow the machine's culture (`SortedDictionary` and `Order()` with the default comparer). This only matters for names with Hungarian digraphs such as "cs" or "ly".
       - No command passes `allowLackOfType: false`.
     - **Second batch done 2026-11-23:** the server setup and owner commands, through the command service.
       - `Integration/ServerSetup`:
         - `!server settings`
         - `!channel add/remove`: the type list, an unknown type, a channel by name, and the one-per-server types
         - `!twitch add/remove` by name or link, with the Twitch CLI as a substitute
         - `!twitch role add/remove`
         - `!self role add/remove` and `!update role message`
         - `!command add/remove` and `!custom list`
       - `Integration/Owner`: `!greeting add/list/remove` and `!say`, with `[RequireOwner]` asking the client for the application's owner.
       - **Changed (each confirmed by its test failing first):**
         - **Replies for what answered nothing**, as decided for the reminder commands. Channels are given by name, as the commands are used:
           - `!channel add <type>` without a channel: "Name the channel too: `!channel add music [channel name]`".
           - `!update role message` without a role channel: "This server has no role channel, set one with `!channel add role [channel name]`".
           - `!greeting list` with no greetings: "There are no greetings yet!".
         - `!greeting remove` no longer says "or it is not yours" (copied from the reminders), since greetings are the bot's.
       - **Considered and kept as they are (decided 2026-11-23):**
         - `!self role remove` takes a role that exists on Discord (an `IRole`). A role deleted on Discord answers "Role not found.", and the owner confirmed the rest of the self-role logic copes with such a role.
         - `!command add` accepts a name a built-in command or alias already has (`help`, `customlist`). Such a command never runs, since `CommandHandler` falls back to custom commands only when no built-in one matches; no check is needed.
         - `!greeting add` takes any text, not only a link: greetings were never meant to be links only.
       - Seen, not changed: the Twitch monitor reloads its channel list every tick, so `!twitch add` applies without a restart. It skips an empty list, though, so after the last channel anywhere is removed it keeps checking that one. Its notification then finds no server and sends nothing.
     - **Third batch done 2026-11-23:** the bias commands.
       - `Integration/Biases/BiasCommandTests`:
         - `!bias add/remove` by name, by `name-group` and by a whole group, with each result message
         - `!bias clear`, `!my biases` (the menu, one group's members, the remark) and `!bias list`
         - `!ping`: who is pinged, the sender and users who left the server left out, both "nobody else" replies, and the command deleted
       - `Integration/Biases/OwnerBiasCommandTests`: `!biaslist add/remove`, `!bias alias add/remove` and `!edit biasdata`, owner-only.
       - `Integration/Biases/BiasEditTests`:
         - `!edit biasdata`'s menu through the interaction service: each action opens its modal for the idol, filled with its data, and an unknown idol or group is answered.
         - Each modal submitted, with the values reaching the service. `DiscordMocks.ModalSubmit` is the new fake for this.
       - `Integration/Database/BiasPingTests`: `!ping`'s query (`GetUsersByIdolsAsync`) by name, name-group, alias and group, each fan once over several names.
       - The edit modals' maximum lengths match or exceed the columns they edit, so a saved value always fits its modal (checked 2026-11-23).
       - **Fixed (each confirmed by its test failing first):**
         - **`!ping` failed for more fans than one message holds.** Discord takes 2000 characters per message, about 90 mentions. All of them went into one message, which Discord.Net refuses. The command had been deleted by then, so nobody was pinged and only an error was logged. The mentions are now sent in as many messages as they need.
         - **Replies for the owner commands that answered nothing** when a part was missing (`!biaslist add/remove`, `!edit biasdata` without a group, `!bias alias add/remove` without all three parts). They now give the format.
         - `!my biases` takes its 1-in-10 remark roll from the injected `Random`, so tests decide it.
       - **Names and groups that contain a hyphen (confirmed on LocalDB 2026-11-23; left as it is until it becomes a problem, decided the same day).**
         - The bias commands split their text on `-` to tell the name from the group. So does `!ping`'s query, for each name.
         - An idol named `j-hope` or a group named `(g)i-dle` therefore can't be added, removed or edited by name. `!bias add j-hope`, `j-hope-bts`, `(g)i-dle` and `miyeon-(g)i-dle` all answer "Bias not in database." `!ping j-hope` and `!ping (g)i-dle` find nothing.
         - `!ping miyeon-(g)i-dle` works only because three parts make it ignore the group.
         - An alias can't help either, because `!bias alias add` splits on `-` too. Only adding the whole group (`!bias add bts`) reaches such an idol.
         - **To check on the live database** whether any names are affected:
           ```sql
           SELECT Name FROM Idol WHERE Name LIKE '%-%' UNION ALL SELECT Name FROM IdolGroup WHERE Name LIKE '%-%'
           ```
         - A fix was tried and dropped on 2026-11-23. It took as the separator the `-` before an existing group's name, and looked up `!ping`'s whole text first and then each `-` in turn. For `!biaslist add` it fell back to the first `-`, since that command may create the group. It worked for all the cases above on LocalDB, if this is ever needed.
     - **Fourth batch done 2026-11-23:** the weekly polls' text commands, the randomizers and `!wotd`.
       - `Integration/Polls`:
         - `WeeklyPollCommandTests`: `!weekly poll create/edit/remove/list/resend` and the short forms (`!wp c`), with the edit message, "Poll not found!", each removal result and a resend posting the poll in its channel.
         - `PresetCommandTests`: `!weekly poll preset create/edit/remove/list` (`!wpp`), including the "still used by a weekly poll" reply.
       - `Integration/Randomizer`:
         - `!8ball`, `!coin flip` (both sides, the edge at 50 and 51, two given choices) and `!decide`
         - the three R6 randomizers: the sender alone or everyone in their voice channel, a different operator each, the rounds' sides, a named channel
       - `Integration/WordOfTheDay` and `WordOfTheDayEmbedProcessorTests`: `!wotd` with the feed replaced (a language, an unknown one, the site down, DMs) and its embed.
       - **Changed (confirmed by its test failing first):** `!weekly poll resend` of a poll without answers sent nothing and answered nothing. It now says the poll has no answers yet.
       - The randomizer commands and `RainbowRandomizerTools.GetResponseForRound` take the injected `Random`, so the tests decide each roll.
       - **Who may manage weekly polls and presets (decided and changed 2026-11-23).**
         - Until then, every weekly poll and preset command, and every button and modal of their edit messages, required only `ChannelPermission.SendPolls`, Discord's "Create Polls". That had been so since the feature was added (`d400f59`, 2025-02-02).
         - Discord gives "Create Polls" to @everyone unless a server takes it away, as far as I know. So any member could create, edit, resend or remove a server's weekly polls; a test confirmed that a member with only that permission got through.
         - The presets are shared by every server, and their commands, though in the owner module, had no `[RequireOwner]`. So a member of any server the bot is in could edit or remove the presets other servers' polls use.
         - **Now:** the weekly poll commands and the poll edit message's buttons, menus and modals require `GuildPermission.Administrator`, like the other admin commands. The preset commands and the preset edit message's require `[RequireOwner]`.
         - The answer menu was one handler for both (`PollOption_Change_CustomOption_*_*`, `True` for a preset). It is now two routes, `..._False_*` for administrators and `..._True_*` for the owner, sharing one method.
         - `WeeklyPollCommandTests`, `PresetCommandTests` and `AnswerEditTests` run as an administrator and as the owner. They check that a member who can create polls but isn't an administrator is refused, and so is an administrator who isn't the owner, for the commands and the edit messages alike.
       - Seen, not changed:
         - `WordOfTheDayService` reports a site outage (`HttpRequestException`) but answers `null` for a timeout or a page it can't read. `!wotd` then says the language isn't supported.
         - The service itself has no tests: it calls the real site through a static `RestClient`.
         - The R6 operator lists hold 39 operators each, and a round fails for a voice channel with more users than that. `!decide` with only commas logs an exception.
  8. Commands with a random branch: `UserBiasGameCommands.BiasGameStop` answers with an easter egg 1 time in 100. **Done 2026-11-19:** it takes the injected `Random`, and `Integration/BiasGame/BiasGameStopTests` (5 cases) covers:
     - stopping a game: the reply, the game's message deleted, the game disposed, and all three aliases
     - no game to stop: nothing is said
     - the easter egg: the game keeps going
  9. The static helpers and the simple embeds. **Done 2026-11-23** (62 cases):
     - Helpers (`Unit/Bot/Tools/`): `StringTools` (the 1st/2nd/3rd/11th endings, reading "1d 2h 30m"), `NumberTools`, `CollectionTools` (the shuffle keeps the playing song first and every song once), `UrlTools`, `ChannelTypeEnumTools`, `StringExtension` (`ToMockText` never gives three capitals in a row) and `TimeSpanExtension`.
     - Embeds: the birthday, greeting, custom command and reminder lists, and `!server settings`.
     - **Fixed (each confirmed by its test failing first):** Discord.Net refuses an embed with more than 25 fields or an empty field value, so these lists failed whole.
       - **`!birthday list` failed at 16, 31 or 46 birthdays, and at more than 60.** A field opens after every 15th birthday (the first field holds 16), so a list ending there left a new, empty field. The loop also stops when the fifth field opens, which left that field empty for any longer list. Empty fields are now skipped. The list shows the first 61, with "N more not shown" in the footer.
       - **`!greeting list` and `!remind list` failed past 25 entries** (one field each). They now show 25 and the same footer. The reminders are by date, so those are the soonest, and `!remind remove` still takes any position.

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
- `UrlTools.SanitizeText`, which puts the links in a caption in angle brackets so Discord doesn't embed them, brackets a link twice (`<<https://x.com/p>>`) when it's already bracketed or appears twice: `Replace` changes every copy, and the next pass finds the copy again with the `>` attached (found with a probe 2026-11-23). Only these two embeds use it.

## 8. MainDB as an SDK-style SQL project (investigated 2026-11-19, not started)

SQL projects have a newer format: SDK-style projects built on the `Microsoft.Build.Sql` SDK, which do for `.sqlproj` what SDK-style projects did for `.csproj`. The SDK is generally available (latest stable 2.3.0, 2026-09-17, needs the .NET 8 SDK or later). Microsoft calls it "the format that will be supported in the future" ([overview](https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/sql-database-projects)). `MainDB` is in the original SSDT format.

**What it would bring here:**
- `dotnet build` builds `MainDB.dacpac`, so the whole solution would build from the command line. CLAUDE.md's "build the projects rather than the `.sln`" would no longer be needed.
- The DB tests could put the real schema on LocalDB from that dacpac (section 2, "Repositories and the database"). Today that needs a Visual Studio build first.
- `.sql` files in the project folder are included by default, so a new table needs no project-file edit.
- Probably ends the Test Explorer warning about `MainDB.dll` (section 2, "Project setup"), since Visual Studio's `TestContainer` rule covers SDK-style projects. Unverified.

**Which tools can open it (checked 2026-11-19; the dev PC has Visual Studio Community 2026 18.12 and SSMS 22.12):**
- **Visual Studio 2026 can't.** It only supports the original format ([SSDT SDK-style page](https://learn.microsoft.com/en-us/sql/ssdt/sql-server-data-tools-sdk-style)).
  - Microsoft's [March 2026 SSMS post](https://techcommunity.microsoft.com/blog/azuresqlblog/database-devops-preview-in-ssms-22-4-1/4503858) says support in 2026 "is not yet on the roadmap". A July 2026 Microsoft reply under it says the Visual Studio 2022 preview work was paused because of conflicting changes in 2026, and that the item is under review. The roadmap is at aka.ms/sqlprojects-roadmap.
  - After a conversion, Visual Studio 2026 would probably show `MainDB` as unloaded in the solution (untested). The bot projects aren't affected.
- **SSMS 22.4 and later can,** through the Database DevOps workload (preview) ([SSMS docs](https://learn.microsoft.com/en-us/ssms/database-devops)). It opens SDK-style projects only, with `Microsoft.Build.Sql` 2.1.0 or later.
  - The dev PC's SSMS doesn't have that workload. To add it: Visual Studio Installer → Modify on SSMS 22 → Workloads.
  - It can build, publish to a server or generate a script, and it handles the post-deployment script, SQLCMD variables and code analysis.
  - It has no table designer and no rename/refactoring, and it can't create publish profiles. The docs don't say whether it reads existing ones.
  - Schema compare: Microsoft's [tools table](https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/sql-projects-tools) says yes, the March post says it's coming.
- **VS Code can** (SQL Database Projects extension). It opens both formats, creates publish profiles and has schema compare, but has no table designer.
- **Visual Studio 2022 can,** with the SDK-style preview component. That would mean a second Visual Studio install just for this.

**What `MainDB` uses, and how it would carry over (checked 2026-11-19):**
- The 21 table files are `<Build Include>` items. They would be removed, because the default glob picks the files up.
- The insert scripts under `Post-deployment/` and the archived `DB Patch/Patched/` scripts are `<None>` items. The default glob would compile them into the model, so they need `<Build Remove="…" />` (check with the first build).
- Keep as they are, per the convert guide: `PostDeploy`, the three publish profiles, `MainDB.refactorlog` and the `BuildConfiguration` SQLCMD variable.
- No SQLCLR code and no database references, so the format's known gaps don't apply.
- Patch scripts come from SSDT publish in Visual Studio today. Without it, `SqlPackage /Action:Script /SourceFile:MainDB.dacpac /Profile:Main_Release.publish.xml` should produce them from the same profiles. Untested. SSMS's Publish → Generate Script does the same from its dialog.

**How:** Microsoft's [convert guide](https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/howto/convert-original-sql-project) has these steps:
1. Back up the `.sqlproj`, and keep a dacpac built from the original.
2. Add `<Sdk Name="Microsoft.Build.Sql" Version="…" />`.
3. Remove the `<Import>` lines, the `Properties` folder entry and the `<Build Include>` items.
4. Change the project type GUID in the `.sln` to `{42EA0DBD-9CF1-443E-919E-BE9C484E4577}`.
5. Build, then compare the two dacpacs with `DacpacVerify` (`dotnet tool install --global Microsoft.DacpacVerify --prerelease`). That check covers the schema model, not the post-deployment script, refactorlog or project settings, so compare those by eye.

**When to revisit:** when Visual Studio 2026 supports SDK-style SQL projects. It could happen sooner if SSMS's Database DevOps workload covers the schema work: try it on a converted copy of `MainDB` first. Check that it builds the same dacpac, that Generate Script makes a patch from the existing profiles, and that editing tables as scripts without the designer is acceptable. A community SDK, `MSBuild.Sdk.SqlProj`, also builds dacpacs with `dotnet build`. Not looked at.

## 9. Review of the bug fixes since 2026-11-13 (investigated and fixed 2026-11-19)

Every bug fix in the commits from `6a8e309` (2026-11-13) to `1d03ec2` (2026-11-19) was checked against its callers. They all fixed real bugs. Three of them caused or uncovered a problem, fixed here:
- **Expired mutes dropped while the bot is offline (done):** since `62498fa`, `UnmuteFeature` took a server missing from the cache for one the bot had left, and deleted the mute without giving the roles back. Before the gateway connects the cache is empty: at a start without internet, and in the minute that logs back in (`StartAsync` returns before READY). It now waits for `ConnectionState.Connected`. Discord.Net fills the server list before it sets that state.
  - Left as is: right after connecting, a server Discord hasn't sent yet (or one in an outage) has no members, so a mute ending in that minute would count its user as gone. `IGuild.Available` can't tell, since `SocketGuild` always answers true (checked 2026-11-19); only the socket type's `IsConnected` could. The window is seconds long, and the next tick is a minute later.
- **YouTube keys refilled too early (done):** YouTube resets the quotas at midnight Pacific time (08:00 UTC in winter, 07:00 in summer). The refill ran at 06:00 UTC, and since the rotation works (`b93a12c`), a search before YouTube's reset dropped the used-up keys again, for almost a day. The refill is now at 08:00 UTC, on its own in `App.OnTimedEvent`.
- **The mute commands after muting was fixed (done):** `7b3cf08` made `!mute` work again (broken since `34c869b`, 2025-02-01). That also brought older bugs back into use:
  - `!unmute` answered "User is not currently muted." for a user who had no roles to give back, and left the mute role on.
  - A refused mute ("already muted") or a failed one still sent the user "You have been muted until …". For "already muted", the roles were also taken and put back.
  - A duration that couldn't be read left the user muted, with their roles gone and no saved mute.
  - A user already wearing the mute role got it saved as a role to give back, so unmuting put it back on.
  - `!mute` now reads the duration first, saves the mute, and only then changes the roles. If Discord refuses the roles (no Manage Roles, or the mute role above the bot's), it gives back what it took, drops the saved mute and says why. The direct messages only cost a warning when refused. `Integration/Mute/MuteCommandTests` covers both commands.
- **Not a problem:** `af1d5ea`'s gender mapping only reaches idols filled from dbkpop from then on, but the database has no `Male`/`Female` rows (only `M`, `F` and 9 nulls), so the earlier "correct the genders" check is dropped.
- **Still open: a reminder to closed direct messages repeats forever.** `ReminderFeature` keeps a reminder whose DM fails, by design (the problem may be temporary). Discord's 50007 (closed DMs, or no shared server) is usually permanent, so that reminder is retried and logged every minute. Treat 50007 as handled, or give up after a day.

## Minor improvements

- **Birthday list names (found 2026-11-18):** `UserBirthdayCommands` shows the command sender's name for a user who left the server. `GetUserNickname(null)` falls back to `Context.User`. Show "unknown user" instead, or leave those birthdays out.
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
