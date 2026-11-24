# Decisions and code facts

Choices made on purpose, and facts about the code that explain why something looks the way it does, so neither comes back as an idea or a "fix". This file covers every project in the repository. Known issues are in `known-issues.md`, open work in `improvement-notes.md`. New decisions and code facts go here. Last brought up to date 2026-11-24.

## Code facts

- **`libdave.dll` stays at v1.1.1 (January 2026, tested 2026-11-15).** libdave v1.2 changed its C API under the same export names: `daveSessionProcessProposals` and `daveSessionProcessWelcome` lost their `recognizedUserIds` arguments in favour of `daveSessionAddRecognizedUser`. Discord.Net.Dave 3.20.2 still calls the old signatures, so v1.3.0 loaded but voice failed with "MLS welcome lists unrecognized user ID". Update libdave only together with a Discord.Net release whose Dave binding calls `daveSessionAddRecognizedUser`.
- **The other external files:** the ffmpeg 9 essentials build's executables are over GitHub's 100 MB file limit, opus has no official Windows binaries, and libsodium 1.0.22 needs the Visual C++ 2015–2022 runtime on the machine running the bot.
- **The Chrome 117 comment in `BrowserService` is out of date (checked 2026-11-17).** It is from an older approach that captured video responses. The Twitter scraper now reads the video URLs from the page's JSON, so the Chrome build no longer matters.
- **Every role the bot stores is a self-role, by design (checked on LocalDB 2026-11-19).** The mute, notification and poll ping roles are in the same `Role` table as the self-roles, so `!update role message` lists them and `+name`/`-name` in the role channel finds them. The notification and poll ping roles are opt-in roles. Its consequences are in `known-issues.md`.
- **Magick.NET instead of ImageSharp (2026-11-15).** SixLabors ImageSharp was removed because its fixed versions need a paid license, and `BannedSymbols.txt` bans it. The four image processors (polaroid, winner bracket, bonk GIF, who-knows) were ported, and these differences from the ImageSharp output were reviewed and accepted, so they aren't to be "fixed" back:
  - Text sits about 0.22 × the font size lower, because `DrawText` places text by the font's full ascent.
  - The who-knows header lines are spaced by line height, not ink height, and long names lose one more character.
  - The polaroid's name limit comes from the canvas width; the old 372 was wider than the image.
  - The bonk GIF keeps a white background, chosen over a transparent one. The old `Rgba32(1, 1, 1, 0.5f)` meant half-transparent white (the float constructor), and GIF has no partial transparency.
  - There is no ImageSharp baseline to compare with any more: the snapshot harness and its "before" renders were deleted after the owner checked the output.
- **Mapster instead of AutoMapper.** The project moved off AutoMapper; CLAUDE.md ("Mapping") has what Mapster needs written out by hand.

## Decided against

**Code and design (2026-11-24):**
- A generic `BaseService` like `GenericRepository<T>`: the owner removed such generic service methods from the other projects, because simple queries weren't worth it.
- Dropping the 13 empty repository interfaces (`IServerRepository : IGenericRepository<Server> { }` and the like): they stay.
- EF's own templates for the scaffold, to mark `DatabaseORM/Model` as generated code with `#nullable enable`: they would need refreshing after every EF upgrade. CLAUDE.md ("Database changes") has the workaround for Code Cleanup.
- `!lf artist`'s image (the top track's album cover from Spotify, or the top album's from Last.fm) works as intended, and so its nested ternary fallback stays.

**Discord.Net (2026-11-15):**
- Adding guilds on `JoinedGuild` instead of on every message: the bot can be added while it isn't running, so the per-message check stays.
- `DownloadUsersAsync` per command: fine as it is.
- `RunMode.Async`: tried in the past and dropped (see CLAUDE.md).

**Tests:**
- Not worth testing (2026-11-17): the CLI processes (`yt-dlp`/`ffmpeg`, `twitch.exe`, `instaloader`), the Puppeteer scrapers, `ProcessTools` (performance counters and WMI), the real Discord connection (`BotMain`; a test would need a second bot account and a test server), live external APIs in the normal run, and the WPF parts (`App`, `BotWindow`). Where their callers are tested, the tests replace them.
- No tests over responses recorded from the real APIs or saved scraper pages, and none of `MusicBrainzAPI` and `WordOfTheDayService` themselves; their callers are tested with fakes, and the tests keep building their responses by hand (2026-11-24).
- The `new Random()` left in the shuffle, the mocking easter egg, the birthday greeting and the who-knows image's file name, and the system clock left in the "Days Of Week" answers and the waits in `YoutubeStreamService`, the scrapers and `ProcessTools`: their tests check properties, or they aren't tested by decision (2026-11-24).

**Conditional (`?:`) expressions (2026-11-16):** after the 2026-11-17 cleanup, the 154 left (see CLAUDE.md "Linting") include borderline ones, fine as ternaries or as `if`/`else` by taste, which stay. Their list is in `git show bb5b065:docs/improvement-notes.md`, section 6, with line numbers that have since moved.

**Linting rules** (the analyzer setup is in CLAUDE.md "Linting"):
- 2026-11-24:
  - CA1307 (36 places): spelling out `StringComparison` on `Contains` / `Replace` / `EndsWith(char)`, which are already ordinal. Explicitness only; the culture-sensitive calls are covered by CA1310, which is on.
  - CA1820 (14 places): `== ""` → `string.IsNullOrEmpty`. Marginally faster, and not a pure rename, because `IsNullOrEmpty` also accepts null.
  - CA1849 hand fixes: `CancelAsync` and `WaitForExitAsync` would change nothing that matters here, and the current calls have caused no problems.
- 2026-11-16:
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
