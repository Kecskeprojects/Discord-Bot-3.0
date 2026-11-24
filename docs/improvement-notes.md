# Improvement notes

Open work, kept so it can be picked up later. This file covers every project in the repository. Items are removed once they are applied; the git history has the earlier work. Known issues left as they are are in `known-issues.md`, and decisions and code facts in `decisions.md`. Last brought up to date 2026-11-24.

## 1. Discord.Net: optional modernization (investigated 2026-11-15)

When checked, Discord.Net 3.20.2 was the latest release, no `[Obsolete]` Discord.Net APIs were in use, and Discord.Commands was not deprecated. The docs note that text commands need the privileged MessageContent intent and recommend slash commands.

- **Slash commands.** MessageContent only needs Discord approval once the bot is in 100+ servers, so text commands are fine for a self-hosted bot. Slash commands would bring built-in argument parsing, autocomplete and permissions, but would be a large migration.
- **Components V2** (stable since 3.18) could replace the embed + button layouts for help paging, bias lists and poll editing. Cosmetic, not required.
- **Modal improvements** (3.19–3.20): select menus, checkboxes and radio groups in modals could simplify the weekly poll and idol edit modals.

## 2. Cleaning up the external services (not investigated yet)

The `Services/` folder (API wrappers, scrapers and CLI tools, most of them behind `Interfaces/Services/`) has grown over time. Investigate whether these services can be cleaned up or split, and whether one or more base classes would help, e.g. a shared base for REST API clients and one for the Puppeteer scrapers. The tests replace many of them through their interfaces, so a cleanup should keep those. These services stay in the bot, unlike the database layer in `DatabaseORM`.

## 3. Last.fm: past periods and daily listens (not started)

- **Past periods in all list commands.** Let users ask for a specific past year, and possibly a half year, quarter, month or week. Today only the rolling periods Last.fm supports work (`7day`, `1month`, `3month`, `6month`, `12month`, `overall`; see `LastFmHelper.LastfmTimePeriod`). The `user.getWeekly*Chart` endpoints return older data for a time range given as Unix timestamps:
  - `?method=user.getweeklychartlist&user=[username]&api_key=[API_KEY]&format=json` lists the available ranges.
  - `?method=user.getweeklyartistchart&user=[username]&api_key=[API_KEY]&from=1704067200&to=1735689599&format=json` returns one range (this example is the whole of 2024).
- **Daily listens.** Last.fm has no `day` period. Add a method in `LastFmApi` (`BaseRequests`) that builds daily stats from the recent tracks endpoint when the requested period is `day` (or anything that converts to it), and use it from the list commands.

## 4. Instagram and Twitter embeds (investigated 2026-11-17)

Can the disabled embeds be turned back on as they are, with adjustments and without logging in?

**How they work today:**
- **Instagram (`InstaScraper`):** a plain `HttpClient` GET (no User-Agent) to `instagram.com/graphql/query/?doc_id=8845758582119845&variables={"shortcode":…}`.
  - The call in `CommandHandler` is commented out, and so is the Instaloader path in `InstagramEmbedFeature`.
  - `instaloader.exe` is still in `External Files/Dependencies/Instagram`.
- **Twitter (`TwitterScraper`):** headless Chrome opens the tweet page logged out and reads the page's own `TweetResultByRestId` GraphQL response. Only the commented-out `!twt` command calls it.
  - The Chrome build doesn't matter any more (`decisions.md`, the Chrome 117 comment).

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

## 5. MainDB as an SDK-style SQL project (investigated 2026-11-19, not started)

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

- **Last.fm top tags:** a new command for `user.getTopTags`. That endpoint has no period or pagination.
- **YouTube playlist start point:** let the user choose which song or index a playlist import starts from (`YoutubeAPI.PlaylistSearch`). It currently always takes the first page of 50 (`MaxResults`).
- **YouTube search matching:** look into a similarity or points-based ranking of search results, beyond the current channel and filter-word filtering in `YoutubeAPI.FilteredResults`. It first came up searching for "Withcha" by Apink.
- **A fuller README (noted 2026-11-24):** `README.md` covers what the bot is, the requirements, the external files and how to build, run and test it. A rework could add:
  - what each feature does, with the commands (`!help` lists them from the modules' `[Summary]`), and perhaps screenshots of the window and of the bias game, who-knows and bonk images
  - `config.json` key by key: what each one is for, and where to get the token, the Spotify, Last.fm and Twitch credentials and the YouTube keys
  - setting up the database: publishing `MainDB` to a new server, and the `ChannelType` rows its post-deployment script adds
  - a first run on a new machine (the .NET Desktop Runtime, the Visual C++ runtime, the fonts, Chrome's first download) and updating the external files (yt-dlp, and `libdave.dll`'s pin)
  - how the projects fit together, for a reader who doesn't read CLAUDE.md

## Feature ideas

- Halloween game
- Blackjack game
- Christmas game
- A command that counts a user's messages or files
