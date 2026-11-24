# Discord Bot 3.0

A self-hosted Discord bot that runs as a Windows desktop (WPF) app, whose window shows the live log. Most of its features are aimed at K-pop communities: bias lists and the bias game, Last.fm stats, music playback, Twitch notifications, birthdays, reminders, weekly polls, self-roles and custom commands.

## Requirements

- Windows x64 with the .NET 11 Desktop Runtime (the .NET 11 SDK to build it).
- SQL Server with the schema from `MainDB/`, an SSDT project that is published from Visual Studio.
- The Visual C++ 2015–2022 runtime (x64), which `libsodium.dll` needs.
- The fonts the images use, installed on the machine: "Bot Font" for the Last.fm who-knows image, and Comic Sans MS for the bias game's images.
- `Assets/config.json` in the working directory. `Discord Bot GUI/Core/Configuration/config.json` lists its keys. If the file is missing, the bot writes an empty one and doesn't log in until `token` is set; edits apply without a restart.
- Chrome, which PuppeteerSharp downloads on first use for the scrapers.

Start the bot from its own folder: the config and the tools under `Dependencies/` are found from the working directory.

## External files

Everything in `Discord Bot GUI/External Files/` is copied next to the executable, with its folders:

| File | Used for |
|---|---|
| `Dependencies/Audio/yt-dlp.exe`, `ffmpeg.exe` (with `ffprobe.exe` and `ffplay.exe`) | Music: yt-dlp's audio is piped into ffmpeg |
| `Dependencies/twitch.exe` | The Twitch CLI: tokens and user lookups |
| `Dependencies/Instagram/instaloader.exe` | Instagram downloads (the Instagram embed is currently disabled) |
| `opus.dll`, `libsodium.dll`, `libdave.dll` | Voice. `libdave.dll` has to stay at v1.1.1 for now, see `docs/decisions.md` |
| `R6_Attackers.txt`, `R6_Defenders.txt` | The Rainbow Six operator randomizer |

yt-dlp needs replacing with a newer release from time to time, when YouTube changes break the old one.

## Build, run and test

```powershell
dotnet build "Discord Bot GUI/Discord Bot GUI.csproj"
dotnet run --project "Discord Bot GUI/Discord Bot GUI.csproj"
dotnet test --project "Discord Bot Tests/Discord Bot Tests.csproj"
```

Build the projects rather than the solution: the solution includes `MainDB.sqlproj`, which only builds in Visual Studio. The database tests need LocalDB (`(localdb)\MSSQLLocalDB`); nothing else in the tests reaches the network or a database.

## More

- `CLAUDE.md`: how the code is organized, its conventions and the external services it uses
- `docs/improvement-notes.md`: open work
- `docs/known-issues.md`: what is known not to work quite right, and is left as it is
- `docs/decisions.md`: ideas decided against, and why some code looks the way it does
