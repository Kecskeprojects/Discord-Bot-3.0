# Improvement notes

Open work from code-quality investigations, kept so it can be picked up later. Items are removed once they are applied; the git history has the details of earlier work. Last brought up to date 2026-11-24.

Done and removed on 2026-11-24 (the full notes are in `git show bb5b065:docs/improvement-notes.md`, sections 2 and 9 and the minor improvements):
- the test project (described in CLAUDE.md "Tests") and what it needed: the clock, the state stores, `IConfig`, Discord.Net's interfaces, and the seams for Last.fm, the browser, web requests, YouTube and Spotify
- the ten batches of tests (1021 cases), the bugs they found, and the checks that found nothing to change
- the review of the bug fixes from 2026-11-13 to 2026-11-19
- the birthday list names, the RestSharp versions, the culture in interpolated strings, and the four interfaces without a use
- the checks by hand: a test run of the bot, Prettier's line endings and the old Chrome builds (not needed for now), the removed `BaseFeature` checks (confirmed), and the live database (nothing to clean up: the two polls that share a preset's ID got no answers saved to it, there are 45 groups, and no server has a music voice channel)

Done and removed on 2026-11-17 (the investigations are in `git show 483c1fa:docs/improvement-notes.md`, sections 4 and 9–11):
- the move of the database layer into `DatabaseORM`
- CRLF line endings
- implicit usings in the bot, with Windows Forms removed
- the BOM rule
- the direct `System.Management` reference
- `[RequireOwner]` on the extended idol modal

## 1. Linting follow-ups (investigated 2026-11-15 and 2026-11-16)

The analyzer setup is done and described in CLAUDE.md ("Linting"). Left:

- **CA1508 (dead conditional code) is on trial.** Of its first 5 findings, 4 were right and 1 was a false positive (`AudioPlayFeature.WaitAsync`, suppressed in the bot's `Properties/GlobalSuppressions.cs`). Decide whether to keep it once it has run on new code for a while.
- **CA1849 cases worth fixing by hand.** The rule isn't turned on, because half of its 12 findings are false positives (`GetChannel` / `GetRole` are cache lookups in Discord.Net's socket client). The real ones (rechecked 2026-11-24):
  - `CancellationTokenSource.Cancel(false)` runs the cancellation callbacks on the calling thread: `UserVoiceCommands.cs` (3) and `AudioStateStore.cs`. `CancelAsync` avoids that.
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

## 2. Tests: what is left (updated 2026-11-24)

The test project and its conventions are described in CLAUDE.md "Tests". Not covered yet:

- **Saved responses:** `LastFmResponses` builds each Last.fm response by hand. Recording each method once from the real API (`user.gettoptracks`, `user.gettopalbums`, `user.gettopartists`, `user.getrecenttracks`, `artist.getinfo`, `track.getinfo`, plus an error response) would be closer to the real format.
  - Use your own account's data, or replace the usernames, and re-record them when Last.fm changes a format.
  - An optional, opt-in live test (a trait, a real key, the network) could warn when a saved response no longer matches the real format.
- **The scrapers' HTML parsing** (`KpopDbScraper.ParseRow` and the group scraping): split loading a page from parsing it, so the parsing takes the page content and tests can feed it saved pages. Optional.
- **`MusicBrainzAPI` and `WordOfTheDayService`** have no tests of their own; their callers are tested with fakes. They could take their `RestClient`, as `BaseRequests` does (`WordOfTheDayService` uses a static one). No test reaches `SpotifyAPI.SearchItemAsync`'s MusicBrainz lookup yet, since the test artists have no MBID.
- **Randomness:** `new Random()` is still in `CollectionTools`, `StringExtension.ToMockText`, `BirthdayMessageProcessor` and `WhoKnowsImageProcessor` (file names), and `YoutubeKeyStore` picks a key with `Random.Shared`. Their tests check properties (a shuffle keeps every item). Pass the injected `Random` in, as CLAUDE.md "Randomness" describes, if a test needs to decide a roll.
- **Left on the system clock:** the "Days Of Week" poll options (the Mapster mapping passes `DateTime.UtcNow` to `WeeklyPollTools`, which is tested on its own), and the delays in the scrapers, `YoutubeStreamService` and `ProcessTools`.
- **Not worth testing (decided 2026-11-17):** the CLI processes (`yt-dlp`/`ffmpeg`, `twitch.exe`, `instaloader`), the Puppeteer scrapers, `ProcessTools` (performance counters and WMI), the real Discord connection (`BotMain`; a test would need a second bot account and a test server), live external APIs in the normal run, and the WPF parts (`App`, `BotWindow`). Where their callers are tested, the tests replace them.

## 3. Known issues, left as they are

Seen while testing (2026-11-19 to 2026-11-24) and not changed. Some were decided; the rest wait until they become a problem.

**Commands:**
- **Names and groups that contain a hyphen (decided 2026-11-23: left until it becomes a problem).** The bias commands split their text on `-` to tell the name from the group, and so does `!ping`'s query, for each name.
  - An idol named `j-hope` or a group named `(g)i-dle` therefore can't be added, removed or edited by name, and `!ping` finds nothing for them. `!ping miyeon-(g)i-dle` works only because three parts make it ignore the group.
  - An alias can't help, because `!bias alias add` splits on `-` too. Only adding the whole group (`!bias add bts`) reaches such an idol.
  - A fix was tried and dropped. It took as the separator the `-` before an existing group's name, and looked up `!ping`'s whole text first and then each `-` in turn. For `!biaslist add` it fell back to the first `-`, since that command may create the group. It worked for all the cases above on LocalDB. The live database had no idol or group name with a hyphen (checked 2026-11-24).
- **Kept as they are (decided 2026-11-23):**
  - `!command add` accepts a name a built-in command or alias already has (`help`, `customlist`). Such a command never runs, since `CommandHandler` falls back to custom commands only when no built-in one matches.
  - `!greeting add` takes any text, not only a link.
  - `!self role remove` takes a role that exists on Discord (an `IRole`). A role deleted on Discord answers "Role not found.", and the rest of the self-role logic copes with such a role.
  - The Last.fm top lists' parameter check throws for three or more words (`!lf tal 10 7day extra`), and the command then answers nothing.
- **Every role the bot stores is a self-role, by design (checked on LocalDB 2026-11-19).** The mute, notification and poll ping roles are in the same `Role` table as the self-roles, so `!update role message` lists them and `+name`/`-name` in the role channel finds them. The notification and poll ping roles are opt-in roles. Accepted consequences:
  - A muted user who can still post in the role channel could take the mute role off with `-muted`; that depends on the server's channel permissions.
  - `!self role remove` fails while the role is in use as the mute, notification or poll role (its foreign key), and the admin is told it could not be removed.
- `!remind in tomorrow>...` (no number at all) answers "Incorrect number of inputs!".
- `!birthday a add` by ID accepts any Discord user, not only the server's members.
- `!bonk`'s delay is documented as milliseconds (`username  delay(ms)`, "1-1000"), but a GIF frame's delay is in hundredths of a second. So the default of 10 is 0.1 seconds, and `!bonk 500` holds each frame for 5 seconds.
- The R6 operator lists hold 39 operators each, and a round fails for a voice channel with more users than that. `!decide` with only commas logs an exception.
- `!wotd` says the language isn't supported when the site times out or sends a page it can't read: `WordOfTheDayService` reports only an outage (`HttpRequestException`) and answers `null` for the rest.
- The bias list's group order and the role message's order follow the machine's culture (`SortedDictionary` and `Order()` with the default comparer). This only matters for names with Hungarian digraphs such as "cs" or "ly".

**Last.fm (decided 2026-11-23):**
- Who-knows stops at the first listener Last.fm answers with an error. So a linked member whose Last.fm account was deleted might break it for the whole server. Unverified: it depends on whether Last.fm answers `track.getInfo`/`artist.getInfo` for an unknown username with an error or without a play count.
- `!lf artist` pages through the user's whole library, 1000 items a page, until it has 5 albums and 8 tracks of the artist. For a large library and a rarely played artist that is many requests. Last.fm offers no better way to get one user's plays of one artist's albums and tracks.

**Handlers and background work:**
- **The Twitch monitor's channel list.** The monitor reloads its channel list every tick, so `!twitch add` applies without a restart. It skips an empty list, though, so after the last channel anywhere is removed it keeps checking that one; its notification then finds no server and sends nothing. And when no server follows any Twitch channel at startup, `TwitchAPI.Check` logs "No channels to monitor on twitch, service stopped!" and never starts the monitor, so a channel added after that needs a restart (found 2026-11-24).
- **Unmutes right after connecting.** A server Discord hasn't sent yet (or one in an outage) has no members, so a mute ending in that minute would count its user as gone. `IGuild.Available` can't tell, since `SocketGuild` always answers true (checked 2026-11-19); only the socket type's `IsConnected` could. The window is seconds long, and the next tick is a minute later.
- The music commands change the queue list while the play loop reads it, without a lock. A clash is unlikely and depends on timing, so no test covers it.
- **Discord.Net answers twice for a custom ID it can't read (accepted 2026-11-24).** For a component or modal whose `*` part doesn't convert (`BiasGame_Setup_Gender_nobody_someone`), `ComponentCommandInfo.ParseArgumentsAsync` raises `InteractionExecuted`, and `CommandInfo.ExecuteInternalAsync` raises it again with the same result. `InteractionHandler` then logs two warnings and answers twice: once as the reply, then as a follow-up. Only a custom ID the bot built wrong can reach it.
  - A fix was tried and reverted on 2026-11-24: `InteractionHandler` remembered the interactions it had answered in a `ConditionalWeakTable` and skipped the second report. The harm is a repeated error message and log line, so it wasn't worth adding state to the singleton.

**Database services:**
- `IdolService.ChangeGroupAsync` loads the idol without its group, so an empty group name would set the group to null. Its caller shouldn't pass one.
- `WeeklyPollService.GetPollByIdAsync` includes neither the server nor the preset, so its result has `ServerDiscordId` 0 and no answers for a preset poll. Its only caller, the edit interaction, reads just the name, title, channel and role, so it's harmless today.

## 4. Discord.Net: optional modernization (investigated 2026-11-15)

When checked, Discord.Net 3.20.2 was the latest release, no `[Obsolete]` Discord.Net APIs were in use, and Discord.Commands was not deprecated. The docs note that text commands need the privileged MessageContent intent and recommend slash commands. The fixes from that review are done.

- **Slash commands.** MessageContent only needs Discord approval once the bot is in 100+ servers, so text commands are fine for a self-hosted bot. Slash commands would bring built-in argument parsing, autocomplete and permissions, but would be a large migration.
- **Components V2** (stable since 3.18) could replace the embed + button layouts for help paging, bias lists and poll editing. Cosmetic, not required.
- **Modal improvements** (3.19–3.20): select menus, checkboxes and radio groups in modals could simplify the weekly poll and idol edit modals.

Decided against:
- Adding guilds on `JoinedGuild` instead of on every message: the bot can be added while it isn't running, so the per-message check stays.
- `DownloadUsersAsync` per command: fine as it is.
- `RunMode.Async`: tried in the past and dropped (see CLAUDE.md).

## 5. Cleaning up the external services (not investigated yet)

The `Services/` folder (API wrappers, scrapers and CLI tools, most of them behind `Interfaces/Services/`) has grown over time. Investigate whether these services can be cleaned up or split, and whether one or more base classes would help, e.g. a shared base for REST API clients and one for the Puppeteer scrapers. The tests replace many of them through their interfaces, so a cleanup should keep those. The database layer already moved into `DatabaseORM` (2026-11-17); these services stay in the bot.

## 6. Last.fm: past periods and daily listens (not started)

- **Past periods in all list commands.** Let users ask for a specific past year, and possibly a half year, quarter, month or week. Today only the rolling periods Last.fm supports work (`7day`, `1month`, `3month`, `6month`, `12month`, `overall`; see `LastFmHelper.LastfmTimePeriod`). The `user.getWeekly*Chart` endpoints return older data for a time range given as Unix timestamps:
  - `?method=user.getweeklychartlist&user=[username]&api_key=[API_KEY]&format=json` lists the available ranges.
  - `?method=user.getweeklyartistchart&user=[username]&api_key=[API_KEY]&from=1704067200&to=1735689599&format=json` returns one range (this example is the whole of 2024).
- **Daily listens.** Last.fm has no `day` period. Add a method in `LastFmApi` (`BaseRequests`) that builds daily stats from the recent tracks endpoint when the requested period is `day` (or anything that converts to it), and use it from the list commands.

## 7. Conditional (`?:`) expressions (investigated 2026-11-16)

154 conditional expressions were left after the 2026-11-17 cleanup (see CLAUDE.md "Linting"). The borderline ones (fine as ternaries, `if`/`else` by taste) were left as they are. Their list is in `git show bb5b065:docs/improvement-notes.md`, section 6, with line numbers from 2026-11-17 that have since moved. The nested one it found is the image fallback in `LastFmAPI.GetArtistDataAsync`, which goes with "Last.fm artist stats image" under Minor improvements.

## 8. Instagram and Twitter embeds (investigated 2026-11-17)

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

## 9. MainDB as an SDK-style SQL project (investigated 2026-11-19, not started)

SQL projects have a newer format: SDK-style projects built on the `Microsoft.Build.Sql` SDK, which do for `.sqlproj` what SDK-style projects did for `.csproj`. The SDK is generally available (latest stable 2.3.0, 2026-09-17, needs the .NET 8 SDK or later). Microsoft calls it "the format that will be supported in the future" ([overview](https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/sql-database-projects)). `MainDB` is in the original SSDT format.

**What it would bring here:**
- `dotnet build` builds `MainDB.dacpac`, so the whole solution would build from the command line. CLAUDE.md's "build the projects rather than the `.sln`" would no longer be needed.
- The LocalDB tests could create the real schema from that dacpac instead of from the scaffolded model (CLAUDE.md "Tests"). Today that needs a Visual Studio build first.
- `.sql` files in the project folder are included by default, so a new table needs no project-file edit.
- Probably ends Test Explorer's warning "No test is available in `MainDB\obj\Debug\MainDB.dll`" (investigated 2026-11-19). Unverified.
  - The warning is harmless, and no project setting stops it. Visual Studio chooses which projects to scan by its own rules, which the vstest maintainers say they can't change ([microsoft/vstest#2130](https://github.com/microsoft/vstest/issues/2130)). A 2023 comment there says `<IsTestProject>false</IsTestProject>` and `<TestProject>false</TestProject>` don't stop the scan.
  - The `TestContainer` capability decides this only for SDK-style projects, which is why the conversion would probably end it.

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

## Minor improvements

- **Reminders to closed direct messages repeat forever (found 2026-11-19).** `ReminderFeature` keeps a reminder whose DM fails, by design, in case Discord's problem was temporary. Discord's 50007 (closed DMs, or no shared server) is usually permanent, so that reminder is retried, and a warning logged, every minute. Treat 50007 as handled, or give up after a day.
- **`Config` as a singleton (investigated 2026-11-18):** `Config` is registered transient. Each instance builds its own configuration with `reloadOnChange`, which watches `Assets/` and is never disposed.
  - Measured with a throwaway program: 500 instances held about 500 extra OS handles until a full garbage collection released them. So it isn't a leak, but each `Config` a command resolves keeps a handle open until the next collection.
  - One long-lived `Config` picked up an edit to `config.json` within 1.5 seconds, so a singleton would still apply edits without a restart.
  - The change: `AddSingleton<IConfig, Config>()` in `Startup`, and the "transient" note in CLAUDE.md.
- **YouTube key usage (found 2026-11-18):** the quota units counted per key (`YoutubeKeyStore.AddUsage`) are never read. They could decide which key to use next, or show in the diagnostics; otherwise drop them.
- **Generic `BaseService`** (`DatabaseORM/Service`): turn it into a generic class so simple DB calls need less boilerplate. Each service would still declare its own methods. `GenericRepository<T>` already exists on the repository side.
- **Empty repository interfaces (investigated 2026-11-24, kept for now):** 13 of the 19 entity repository interfaces declare nothing beyond `IGenericRepository<T>` (such as `IServerRepository : IGenericRepository<Server> { }`), and each has an empty class behind it. The tests could substitute `IGenericRepository<Server>` just as well. Dropping them means registering the open generic `GenericRepository<>`, and changing the services' constructors and the tests. `DatabaseORM` is modeled on KecskeProjectsSite's, so check that project's layout first.
- **Dead code (found 2026-11-23):** `BiasListMessageProcessor`'s "unsorted" branch never runs, since every idol has a group (`Idol.GroupId` is `NOT NULL`, and both list queries load it). And no command passes `allowLackOfType: false` to `IsCommandAllowedAsync`.
- **Last.fm top tags:** a new command for `user.getTopTags`. That endpoint has no period or pagination.
- **Last.fm artist stats image (found 2026-11-16):** `LastFmAPI.GetArtistDataAsync` takes the MBID from the albums when there are any, but the artist and item it searches Spotify for from the tracks when there are any. The throw for an artist with album plays but no track plays is fixed (2026-11-23).
  - Passing a track name makes the search return that track's album cover instead of the artist's image, as `GetTopArtistsAsync` does.
  - Decide which image the stats should show, and take the values from one list. Its image fallback is a nested ternary.
- **Hand-written results in `Services/Models` (found 2026-11-16):** `Services/Models/LastFm` (`ArtistStats`, `LastFmListResult`, `NowPlayingResult`, `WhoKnows`) and `Services/Models/SpotifyAPI/SpotifyImageSearchResult` are the bot's own result DTOs, not JSON models.
  - Because the folder is marked as generated code, they get no nullable checking and no analyzer warnings.
  - Moving them to `Communication/` (where data types belong by convention) would bring both back. Their nullable fields are `Message`, the image URLs and the optional now-playing data.
- **Code Cleanup edits the scaffolded models (found 2026-11-16):** `DatabaseORM/Model` isn't marked as generated code, so that its `?` annotations count (see CLAUDE.md "Database changes"). Visual Studio's Code Cleanup therefore changes those files; for example, it removes unused usings even with IDE0005 off for the folder.
  - Until this is fixed, leave the folder out of Code Cleanup, or re-scaffold afterwards.
  - Fix: mark the folder `generated_code = true` again, and have the scaffold write `#nullable enable` at the top of each model file, which turns nullable back on in generated code. The folder's `charset = utf-8` (no BOM, as EF Core 11 scaffolds) stays as it is.
  - To do that, install EF's templates (`dotnet new install Microsoft.EntityFrameworkCore.Templates`, from nuget.org), run `dotnet new ef-templates` in the `DatabaseORM` folder, add the directive to `CodeTemplates/EFCore/EntityType.t4`, and re-scaffold once. `Scaffold-DbContext` and `dotnet ef dbcontext scaffold` use those templates automatically.
  - The templates must stay CRLF; LF templates are reported to produce an extra blank line per line. `.gitattributes` already checks them out that way.
  - The cost: the templates become the project's own copy, so after an EF upgrade, refresh them and re-add the line.
- **YouTube playlist start point:** let the user choose which song or index a playlist import starts from (`YoutubeAPI.PlaylistSearch`). It currently always takes the first page of 50 (`MaxResults`).
- **YouTube duplicate on playlist import:** when a video link that also carries a playlist (`&list=`) is imported as a playlist (`YoutubeAPI.AddPlaylistAsync`), the video itself can be queued twice if it is in the imported part of the playlist. Skip it.
- **YouTube search matching:** look into a similarity or points-based ranking of search results, beyond the current channel and filter-word filtering in `YoutubeAPI.FilteredResults`. It first came up searching for "Withcha" by Apink.

## Feature ideas

- Halloween game
- Blackjack game
- Christmas game
- A command that counts a user's messages or files
