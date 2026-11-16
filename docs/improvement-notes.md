# Improvement notes

Findings from code-quality investigations, kept so the work can be picked up later. Nothing here has been applied yet unless marked done.

## 1. Linting (investigated 2026-11-15, set up 2026-11-16)

C# has no separate linter. Linting is done by **Roslyn analyzers** that run during `dotnet build` and in Visual Studio, configured through `.editorconfig` and MSBuild properties.

| Piece | What it does | Current state |
|---|---|---|
| .NET SDK analyzers (CA rules) | Correctness, performance, security, design. Strictness via `<AnalysisLevel>` / `<AnalysisMode>` | `latest-recommended` in both projects (`Directory.Build.props`), exceptions in the root `.editorconfig` |
| Code-style rules (IDE rules) | Style and formatting | Enforced by `dotnet build` (`EnforceCodeStyleInBuild`). The whole Style category is a warning, except IDE0010 / IDE0072 (populate switch) |
| `.editorconfig` | Rule severities, formatting, naming | Root file with formatting options matching the owner's VS settings, code style preferences and severities. `Database/.editorconfig` only marks generated code |
| `dotnet format` | Formatter and fixer; `--verify-no-changes` is the check mode | Available (part of the SDK), not run yet |
| Third-party analyzers | Roslynator, Meziantou.Analyzer, StyleCop.Analyzers (strict style), SonarAnalyzer.CSharp, Microsoft.VisualStudio.Threading.Analyzers (async misuse) | None |
| Nullable reference types | Compiler flags possible null dereferences | `<Nullable>disable</Nullable>` in both projects, turned on per file with `#nullable enable`: `Tools/`, `Features/`, `Processors/`, `Commands/` and `Interactions/` since 2026-11-16 |
| `TreatWarningsAsErrors` | Enforces the zero-warning goal | On since 2026-11-16, in every configuration (`Directory.Build.props`). NuGet audit warnings NU1901–NU1904 stay warnings |

### Trial run (2026-11-15)

A trial build with `-p:AnalysisLevel=latest-recommended -p:EnforceCodeStyleInBuild=true` reported 418 distinct warnings:

- About 320 culture warnings (CA1305, CA1304, CA1311, CA1310): `ToString()`, `ToLower()`, `string.Format`, `StartsWith` without a culture or `StringComparison`. Mostly harmless for a bot; can be lowered in `.editorconfig`.
- Naming and design: CA1051 public fields (30), CA1707 underscores in names (15), CA1711 / CA1716 name suffixes and reserved words, CA1725 parameter names differ from the interface, CA1309, CA1805.
- Worth fixing:
  - CA2201, `throw new Exception(...)` (10 left): `SizedDictionary`, `YoutubeAPI`, `LastFmHelper`, `PollTools`. (`CommandHandler` and `AudioPlayFeature` were fixed during the Discord.Net work.)
  - CA1001: `App` (`mainTimer`) and `BotWindow` (`diagnosticsTimer`) own timers but are not disposable.
  - CA5369: `WordOfTheDayService.cs:31` uses an `XmlSerializer.Deserialize` overload that may allow DTD processing on external RSS. Deserialize through an `XmlReader` with safe settings instead.

### Gotcha (done 2026-11-16)

`Discord Bot GUI/Database/.editorconfig` had `root = true`, so the repo-root `.editorconfig` would **not** have applied anywhere under `Database/`. Removed; `root = true` is now only in the root file.

### Recommended setup

1. **Done 2026-11-16.** `Directory.Build.props` at the repo root so both projects share `<AnalysisLevel>latest-recommended</AnalysisLevel>` and `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`.
2. **Done 2026-11-16.** A root `.editorconfig` that silences the unwanted rules and keeps the rest as warnings (details below).
3. Optionally `Microsoft.VisualStudio.Threading.Analyzers` for `async void`, `.Result` / `.Wait()` and unawaited tasks, which are common in Discord.Net handlers. Not added yet.
4. **Done 2026-11-16.** `dotnet format` for formatting. Turn on `TreatWarningsAsErrors` only once at zero warnings.
5. Enable nullable reference types gradually, folder by folder with `#nullable enable`. **In progress:** `Tools/`, `Features/`, `Processors/`, `Commands/` and `Interactions/` done 2026-11-16 (details under "Nullable reference types" below). Left: `Services/`, `Database/`, `Core/`, `Communication/`, `Resources/`, `Windows/`, the root files and LastFmApi.

### Setup decisions (2026-11-16)

- **Formatting** options mirror the owner's Visual Studio settings (read from `CurrentSettings.vssettings`): space after casts, braces of `case` blocks not indented, file-scoped namespaces, no simple `using` statements, expression-bodied lambdas and local functions only on a single line. With the .NET defaults instead, `dotnet format` wanted to reformat 25 files.
- **Silenced, with the reason written in `.editorconfig`:** CA1304 / CA1311 (lowercasing without a culture), CA1051 (visible fields), CA1707 (underscores), CA1711 (`Enum` suffix), CA1716 (keywords in other languages).
- **CA1305 stays on** except for integer `ToString` / `Parse` (`dotnet_code_quality.CA1305.excluded_symbol_names`). Nullable integers (`int?.ToString()`) are not covered by the exclusion.
- **`GenerateDocumentationFile`** is on only so the build reports IDE0005 (unnecessary usings). Side effect: `Discord Bot.xml` / `LastFmApi.xml` next to the binaries. CS1591 (missing XML comments) is off.
- **The Database namespace is exempt from CA1309 and CA1310** (`Properties/GlobalSuppressions.cs`, next to CA1862). DB services and repositories pass lambdas that EF translates to SQL, and EF can't translate the `StringComparison` overloads. Visual Studio's code fix for these rules adds one anyway: Code Cleanup did it to 7 queries (reverted 2026-11-16).
- The build went from 0 to 87 warnings (the trial with `latest-recommended` alone had 417). After the partial Code Cleanup (`35fb3ba`) and the Database exemption, 55 were left; after the CA2201 fixes, 45; after CA1310 and CA5369, 27 (all CA1305); after CA1305, **0 warnings in Debug and Release**. `dotnet format --verify-no-changes` passes on both projects.

### Warnings from the setup (all cleared 2026-11-16)

Kept as a record of what changed and why. File and line references are from `bdc988d`, before the fixes.

**Formatting and style (9): done 2026-11-16** in `35fb3ba` and `bdc988d`. The extra IDE0001 that only `dotnet format` reported is gone too.

**String comparison: done 2026-11-16**
- CA1310 (17): **done 2026-11-16** with `StringComparison.Ordinal`. `StartsWith` / `EndsWith` / `IndexOf` / `LastIndexOf` with a string argument were culture-sensitive. Every call looks for a fixed ASCII marker (URL prefix, file extension, "tags:", "Token: ", "getInfo", "i am"), so the results are the same. None of these are in EF queries. `EasterEggFeature.cs:48,54,56`, `BiasGameImageProcessor.cs:47`, `InstagramMessageProcessor.cs:24,64,69,73`, `InstaScraper.cs:103`, `KpopDbScraper.cs:87,91`, `TwitchCLI.cs:36`, `UrlTools.cs:29,72`, `LastFmApi/Communication/LastFmRequestDetails.cs:16`.
- CA1309 (7): **no longer reported** because of the Database exemption, not a code change. These were `.ToLower().Equals(x)` inside EF queries in `CustomCommandService`, `RoleService` and `ServerService`; they stay as they are.

**Format provider, CA1305 (27): done 2026-11-16.** None of these could be reproduced as a bug: under both hu-HU and en-US on .NET 11, `DateTime.Parse` accepts Last.fm's English dates ("16 Nov 2026, 14:05"), and the bias modal dates parse back correctly. The risk was that output and parsing depend on the server's culture. Fixed with `CultureInfo.InvariantCulture` unless noted:
- `string.Format`: `UserBiasCommands`, `BirthdayMessageProcessor`, `WhoKnowsImageProcessor` (2), `ImageTools`.
- Parsing: `LastFmListResultTools` (`double.Parse` of play counts ×3, `DateTime.Parse` of Last.fm dates), `ProcessExtension` (`Convert.ToInt32` ×2), `Convert.ChangeType` in `WeeklyPollService` and `WeeklyPollOptionPresetService` (the edited fields are `string`, `bool`, `long` and `int?`, so invariant is safe).
- `ReminderFeature` and `UnmuteFeature` sent `DateTime.UtcNow` through a string and back to drop the seconds. Now `new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind)`, as `DateTimeTools.TryAddValuesToDate` already does. The value is only compared in EF queries, so its `Kind` (now `Utc` instead of `Unspecified`) doesn't matter.
- `InstagramMessageProcessor`, `InstaScraper`: `ToString("yyyy\\.MM\\.dd")`.
- `BiasEditComponentInteraction` (3): the edit modals are prefilled with `?.ToString("yyyy. MM. dd.", CultureInfo.InvariantCulture) ?? ""`. This differs from the earlier ISO proposal, because the modals' placeholders say "YYYY. MM. DD. format". The output is exactly what hu-HU produced before ("2001. 05. 12."), and `DateOnly.TryParse` reads it back under hu-HU, en-US, en-GB and invariant (tested on .NET 11). Before, an en-US server would have prefilled "5/12/2001".
- Nullable integers: `WeeklyPollService.UpdateAsync` now looks the role up only inside `if (roleId.HasValue)`, with `roleId.Value.ToString()` in a local (before, a null `roleId` queried `DiscordId == ""`). `LastFmAPI.GetNowPlayingAsync`: `userplaycount?.ToString() ?? ""`. `LastFmRequestDetails`: `Limit` / `Page` use `?.ToString()`; empty and null are both skipped when the URL is built.
- Not covered by CA1305: interpolated strings. For example `LastFmListResultTools` writes `{percentage}%`, which shows "12,5%" under hu-HU and "12.5%" under en-US. Leave it as is if the server's culture is the intended display format.

**Exceptions, CA2201 (10): done 2026-11-16**
- `SizedDictionary`: `throw new()` on a null key → `ArgumentNullException.ThrowIfNull(key)`.
- `YoutubeAPI` (3) → `InvalidOperationException`.
- `PollTools.GetSpecialAnswers` → `ArgumentOutOfRangeException(nameof(name), name, ...)`.
- `LastFmApi/LastFmHelper` (3) → `ArgumentException(..., nameof(parameters))`. This adds " (Parameter 'parameters')" to the message, which only appears in the log.
- Nothing catches these types or checks their messages; every caller catches `Exception` and logs it, so behavior is unchanged.

**Design (16): done 2026-11-16, with follow-ups for CA1001**
- CA1725 (8): **done 2026-11-16** in `35fb3ba` and `bdc988d`. Code Cleanup renamed the implementations to the interface names, the opposite of the proposal here (which was renaming the interfaces to the clearer implementation names, e.g. `reminderOrderId`, `foreignId`, `artistMbid`).
- CA1805 (5): **done 2026-11-16** in `35fb3ba`.
- CA1001 (3): **done 2026-11-16** in `bdc988d`. `App.Dispose` disposes `BotService` and `mainTimer`, `BotMain.Dispose` disposes `startLock` and the client, and `BotWindow.Dispose` disposes `diagnosticsTimer`. Follow-ups found on review (2026-11-16):
  - **Done 2026-11-16.** Nothing called these methods. Now `Closing()` stops `mainTimer` first, so the reconnect timer can't log the bot back in during shutdown. `CloseAsync` calls `App.Dispose` last in its `finally`, after `StopBotAsync` and the log flush, on the thread pool like the rest of the shutdown (the client's `Dispose` waits on its own `StopAsync`, which risks the UI-thread deadlock that `Closing()` already avoids). `BotWindow` disposes its timer in `OnClosed`.
  - **Done 2026-11-16.** Null cases: `BotWindow.diagnosticsTimer` is null when `ShowDiagnostics` is off, and `App.BotService` is null if startup failed before it was assigned. Both use `?.Dispose()`.
  - `BotMain` keeps disposing the `DiscordSocketClient`, although the DI container created it, because the root service provider is never disposed. This is the only place it gets disposed (a second dispose would be harmless).
  - Not checked by running the bot. To check by hand: start the bot, close the window, and confirm the log ends with "Application closing..." and no error, and that the bot shows offline in Discord right away.

**Security, CA5369 (1): done 2026-11-16**
- `WordOfTheDayService` deserialized the external RSS with `XmlSerializer.Deserialize(TextReader)`, which processes DTDs. It now reads through `XmlReader.Create(..., new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore })` (`Ignore` rather than `Prohibit`, so a DOCTYPE in the feed doesn't throw). Checked against 6 live feeds (zh, fr, ru, hindi, esp, pl): identical results to the old path, and none has a DOCTYPE.

### Further rule candidates (investigated 2026-11-16, recommended and optional ones applied 2026-11-16)

Trial builds on top of the zero-warning setup. Counts are new warnings.

**Outcome (2026-11-16):** all recommended and optional rules below are on, and the build is back at 0 warnings in Debug and Release.
- CA2000: all 14 fixed. `Startup.Run` is suppressed in GlobalSuppressions (DI singletons). `YoutubeAPI` needed more than a `using`: the playlist import runs in the background after `Run` returns and used `Run`'s service. It now creates and disposes its own service once the user confirms. `GetChildProcessCount` also disposes the child `Process` objects it creates every second.
- CA1063: the 5 finalizers are removed and the 8 types sealed. Two bugs went with them:
  - `WhoKnowsEmbedResult.Dispose()` threw on a null `ImageData` (who-knows embeds without an image). `Dispose` stopped before marking itself disposed, so the finalizer ran it again on the finalizer thread, which ends the process.
  - `LastFmWhoKnowsEmbedProcessor` hands an `EditPictureResult`'s stream to the who-knows result without disposing the `EditPictureResult`, so its finalizer could close the stream while the embed was still being sent.
- Naming: 16 fields renamed; private constants keep PascalCase through a more specific rule.
- Banned API list: `BannedSymbols.txt` at the repo root, with `Microsoft.CodeAnalysis.BannedApiAnalyzers` 5.6.0 added in `Directory.Build.props`. Checked with a temporary file that every entry reports RS0030.
- **CA1508, kept on trial: 4 of 5 findings were right.**
  - Real bug: `ChannelTypeEnumTools.TryGetEnumFromCommandText` ran `FirstOrDefault` over a non-nullable enum, so it always returned `true`, with `None` for unknown text. `!channel add/remove <typo>` answered "ChannelType or Server not found." instead of listing the valid types, and "unspecified" was accepted as `None`.
  - Dead code: `BotLogger.LogToFile` (the writer null check) and `TwitterScraper` (`Body?.` after the null check).
  - False positive, suppressed: `AudioPlayFeature.WaitAsync` reads `VoiceChannel` again after a `Task.Delay`, during which the bot can be disconnected.

**Recommended**
1. **IDE code-quality rules** (`dotnet_analyzer_diagnostic.category-CodeQuality.severity = warning`): unused or unread private members, unnecessary suppressions and similar. **0 warnings today**, so it only guards future code.
2. **CA2000, dispose objects before losing scope** (14). Real leaks:
   - `ProcessTools.cs:17-21` creates 5 `PerformanceCounter`s on every diagnostics tick (once a second when `ShowDiagnostics` is on) and never disposes them.
   - `ProcessExtension.cs:14,32` (`ManagementObjectSearcher`), `WebTools.cs:45` (`Ping`, on every login attempt), `YoutubeAPI.cs:85` (`YouTubeService` per search), `TwitchCLI.cs:45`, `UserBiasGameCommands.cs:79` (`BiasGameData` from `TryRemove`).
   - False positives: `Startup.cs:32,47,54`, DI singletons the container owns. Suppress those with a justification.
3. **CA1063, implement IDisposable correctly** (21, in 8 types). `AudioVariables`, `BiasGameData`, `EditPictureResult`, `ServerAudioResource` and `WhoKnowsEmbedResult` have finalizers that call `Dispose()` on managed objects. That is unsafe, because those objects may already be finalized; the `AudioVariables` finalizer crash in section 3 was this pattern. They hold no native resources, so the fix is to remove the finalizers and seal the classes. `App`, `BotMain` and `BotWindow` only need `sealed`.

**Optional**
4. **Private field naming**, camelCase without a prefix, as most of the code already does (16). The exceptions: `_` prefix in `AudioVariables`, `BiasGameData`, `EditPictureResult`, `ServerAudioResource`, `WhoKnowsEmbedResult`, `MusicBrainzAPI`, `WordOfTheDayService`, `LastFmApi/BaseRequests`; PascalCase in `App.BotService`, `TextFont.InstalledFamilies`, `Config.Configuration`, `TwitchAPI` (`Monitor`, `API`, `Token`, `TokenTick`), `BotWindow.AutoScroll`.
5. **Banned API list** (`Microsoft.CodeAnalysis.BannedApiAnalyzers` package + `BannedSymbols.txt`) to turn CLAUDE.md rules into build errors: `Console.*` (use `BotLogger`), `RunMode.Async` (rejected), `Assembly.GetEntryAssembly`, `SixLabors.*` (license). No current violations, so it's a guard only. `DateTime.Now` is better left off the list, because the 3 uses (`BotLoggerTools`, `DateTimeTools`) are deliberate local log times.
6. **CA1508, dead conditional code** (5): `BotLogger.cs:24`, `AudioPlayFeature.cs:178`, `TwitterScraper.cs:96-97`, `ChannelTypeEnumTools.cs:26`. Useful, but this rule is known for false positives.

**Not recommended at first: second look (investigated 2026-11-16, after the rules above were applied)**

`AnalysisLevel=latest-all` now adds 2,145 warnings. Rule by rule:
- Worth adding after all (cheap, no noise). **Added and fixed 2026-11-16, together with CA2234 below:**
  - CA1052 (9): classes with only static members that aren't `static`. The 6 embed processors in `Processors/EmbedProcessors/Help` and `Polls`, plus `LastFmHelper`, `UtilityRequests` and `WhoKnowsRequests` in LastFmApi. All are now `static`.
  - CA1851 (2): `InstagramMessageProcessor.ReadFiles` ended with an empty leftover loop, `for (int i = 0; i < orderedFiles.Count(); i++) { }`, which sorted the whole file list again on every iteration. Removed. (The Instagram embed is disabled.)
- Worth fixing by hand, but not as a rule:
  - CA1849 (12): half are real and half are false positives. Real: `CancellationTokenSource.Cancel` runs the cancellation callbacks on the calling thread (App, BotMain, UserVoiceCommands ×3), and `YoutubeStreamService.cs:51` blocks a thread in `Process.WaitForExit(TimeSpan)`. False positives: `GetChannel` ×4 and `GetRole` ×2 are cache lookups in Discord.Net's socket client.
- Optional, explicitness only (no behavior change):
  - CA1307 (38): spell out `StringComparison` on `Contains` / `Replace` / `EndsWith(char)`, which are already ordinal. None of them are in the Database namespace.
  - CA1820 (15): `== ""` → `string.IsNullOrEmpty`. Not a pure rename: `IsNullOrEmpty` also accepts null.
  - CA2234 (2): `WebTools` passes URLs to `HttpClient` as strings. **Added and fixed 2026-11-16** (`new Uri(url)`).
- Not worth it, for reasons specific to this code:
  - CA2007 ConfigureAwait (881): the Discord.Net handlers already run on the thread pool, where it changes nothing. Only the WPF UI code (`App`, `BotWindow`) has a synchronization context, and there `ConfigureAwait(false)` would be wrong before UI access.
  - CA1515 make types internal (387): Discord.Net only discovers public command and interaction modules, so applying the code fix would make commands disappear.
  - CA1031 catch `Exception` (279): the project's convention; handlers log and carry on.
  - CA1062 validate arguments (182): nullable reference types are the better tool.
  - CA1002 (123), CA2227 (80), CA1819 (4), CA1034 + CA1815 (3, `Config.ConfigModel`): library API design. The settable collections and arrays are what Mapster, JSON and Discord.Net (`Embed[]`) work with.
  - CA1056 / CA1054 / CA1055 (76): URLs come from the database, JSON and the config as strings.
  - CA1724 (24): type names such as `Log`, `User`, `Owner` and `Data` that also exist as namespaces. Renaming churn with no effect on behavior.
  - CA5394 (20): `Random` for games and easter eggs, not security.
  - CA1008 (5): these enums start at 1 on purpose (database IDs, command levels), so a `None = 0` would mean nothing.

`Microsoft.VisualStudio.Threading.Analyzers` 18.12.1, trial build (180 warnings): VSTHRD200 `Async` suffix (153, mostly command methods, whose names Discord.Net doesn't care about), VSTHRD202 return the task directly (9), VSTHRD100 `async void` (8, all event handlers or overrides, all guarded now), VSTHRD103 (6, the same `GetChannel` / `GetRole` false positives), VSTHRD001 (3, Visual Studio extension threading rules), VSTHRD002 (1, the deliberate 3-second shutdown wait). Nothing new; not worth adding.

Nullable reference types for the whole project (992 warnings):
- About 620 are CS8618, uninitialized non-nullable properties, in the JSON / API / DTO models: `Services/Models` (354), LastFmApi (204, mostly models), `Resources` (55), `Communication` (40).
- The logic folders are small: `Tools` 19 (10 files), `Commands` 19, `Processors` 21, `Features` 16, `Interactions` 14, `Core` 16, `Windows` 8, root files 46 (`App`, `BotMain`, the handlers), service code 74, `Database` 92.
- Suggested order: `#nullable enable` in `Tools` first, then `Features`, `Processors`, `Commands` and `Interactions`. Leave the model folders for last, or keep nullable off there.
- **`Tools/` done 2026-11-16:** `#nullable enable` at the top of all 23 files, 18 warnings fixed. With `TreatWarningsAsErrors` on, a folder can only be switched on once all its warnings are fixed. Files outside converted folders stay oblivious, so callers there are not affected.
  - Annotated as nullable because they return or accept null on purpose: `UrlTools.LinkSearch` / `SanitizeText`, `StringTools.AddNumberPositionIdentifier`, the `server` parameter of `DiscordTools.IsTypeOfChannel`, the optional `user` of `GetNickName`, and the `contentType` of `MagickTools.ConvertIfUnsupported`. `StringExtension.UrlCombine` returns null only when both parts are null, which `[NotNullIfNotNull]` expresses, so URLs built from constants stay non-null for callers.
  - Behavior changes: `WebTools.GetStream` threw a NullReferenceException for a response without a Content-Type header, and now copies the data as it is. `DiscordTools.GetNickNameBase` threw when the user in a server channel wasn't a `SocketGuildUser`, and now falls back to the username. `MagickTools.MeasureText` and `ImageTools.GetDominantColor` throw an `InvalidOperationException` with a message where Magick.NET returns null (only when its native call fails), instead of a NullReferenceException in the caller. `ProcessExtension.GetChildProcesses` filters with `OfType<Process>()`.
- **`Features/`, `Processors/`, `Commands/`, `Interactions/` done 2026-11-16:** `#nullable enable` in all 85 files, 68 warnings fixed.
  - Mostly annotations: `out` variables of `TryGetValue` / `TryRemove`, optional `IChannel` / `IUser` command parameters, and `FirstOrDefault` results that are already null-checked. Two helpers in `Tools` got attributes so callers know the result after the check: `CollectionTools.IsNullOrEmpty` (`[NotNullWhen(false)]`) and `ChannelTypeEnumTools.TryGetEnumFromCommandText` (`[NotNullWhen(true)]`).
  - `BaseFeature.Context` / `Parameters` are `= null!`: `Run(...)` sets them before `ExecuteCoreLogicAsync`, and features started without them never read them. A nullable type would have meant a null check in every feature.
  - `as` casts that can't fail became casts that state the assumption: `(SocketGuildUser) Context.User` in guild-only features and `(SocketMessageComponent) Context.Interaction` in component handlers. `AudioRequestFeature` uses `Context.User is not SocketGuildUser { VoiceChannel: not null }`.
  - Bugs fixed:
    - `UnmuteFeature` checked `user != null` only for the DM. When a muted user (or the bot) had left the server, `user.RemoveRoleAsync` / `user.Id` threw every minute, the exception ended the loop for everyone after them, and their row was never removed. Now the roles are only touched when both server and user are found, and the entry is removed either way.
    - `BirthdayFeature`: a deleted birthday channel or a server the bot left threw inside the loop and stopped the day's birthdays for the other servers. Now it is logged and skipped.
    - `!update role message` and the YouTube playlist offer threw on a deleted channel. They now reply / log instead.
  - The image processors that log and return null on failure now say so in their types (`MemoryStream?`, `EditPictureResult?`). `UserBonkCommands` skips sending when the gif failed, instead of throwing in `SendFileAsync`. `BiasGameWinnerBracketImageProcessor.EditImage` branches on `winner == null` instead of the file count. `InstagramMessageProcessor` (disabled embed) throws a clear exception when Instaloader saved no metadata file.
  - Open: `BiasGameStepInteraction` assigns the result of `UpdateWinnerBracket` / `AddFinal` straight to `BiasGameData.WinnerBracket`. When drawing fails it becomes null and the next step of the game throws. The replaced stream is also never disposed. Not flagged yet because `Communication/` is still oblivious; keep the previous bracket and dispose the replaced one when that folder is converted.

**Found on the way**
- **Done 2026-11-16:** `App.StartTwitchMonitor` was `async void` without a try/catch on its own thread. `TwitchAPI.Start()` catches its own exceptions, but creating the scope or constructing `TwitchAPI` and its dependencies (e.g. `Config` with a broken config file) could still throw and end the process. It now logs instead.
- **Done 2026-11-16:** `App.OnStartup` is also `async void`, and nothing caught errors from `await botService.RunBotAsync()` (`RunBotAsync` has only a `finally`). A login failure at startup (e.g. an invalid token, which `LoginAsync` rejects) closed the app without a log entry. It is now logged. A failed login leaves the client `LoggedOut`, so the reconnect timer retries every minute, and a corrected token in `config.json` is picked up without a restart. The other `async void` handlers already caught their exceptions.
- **Done 2026-11-16:** the Twitch monitor keeps one DI scope for its whole life (`Check` ends with `await Task.Delay(-1)`), and its `async void` event handlers, which can overlap, all shared it. Three things were unsafe to share:
  - the scope's `DbContext`, which allows one operation at a time ("A second operation was started on this context");
  - one `TwitchNotificationFeature` instance, whose `Parameters` hold the current run's data, so overlapping announcements could swap their data;
  - a plain `Dictionary` of stream states.
  `TwitchAPI` now takes `IServiceProvider` and creates a scope for each channel lookup and each notification (the channel resources are DTOs, so they outlive the scope). The stream states are a `ConcurrentDictionary`.
- **Done 2026-11-16:** the token refresh in `TwitchAPI.MonitorOnServiceTick` (every 1440 ticks, about 24 hours) stored the new token only in the `token` field, never in `api.Settings.AccessToken`, so the monitor kept using the token from startup. Twitch app access tokens expire after about 60 days, so a long-running bot would have stopped getting stream updates until a restart. It now sets `api.Settings.AccessToken` too; the monitor shares that client. Not checked live: that needs a bot running for over a day.

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
   - Design worked out 2026-11-16 (not applied): `Log` already carries its `Type`, so `BotLogger` can raise a `LogAdded` event, and `BotWindow` subscribes in its constructor (unsubscribes in `OnClosed`) and maps the type to the color it uses today (Log white, Query dark cyan, Client dark gray, Audio dark orange, Error red, Warning yellow). `BotLogger` then loses its `System.Windows` dependency. `BotLogger` appears in 94 files. Other places to change: `Startup.cs` registers `new BotLogger()`, and `App.xaml.cs` resolves it with `GetService<BotLogger>()`. Put the interface in `Interfaces/Core/`, following the existing `Interfaces/<folder>/` layout.
   - Found at the same time: `BotLogger.Logs` is a plain `List<Log>` written from many threads and read and cleared by `LogToFile` on the timer thread. A concurrent `Add` can lose entries, and `LogToFile` can hit "Collection was modified". A private `ConcurrentQueue<Log>` that `LogToFile` drains fixes it; do it together with `IBotLogger`. Nothing outside `BotLogger` reads `Logs`.
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
