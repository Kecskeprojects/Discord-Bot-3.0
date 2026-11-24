# Known issues

What is known not to work quite right but is left as it is, so it doesn't come up again as a new finding. This file covers every project in the repository. Open work is in `improvement-notes.md`, and decisions and code facts are in `decisions.md`. New known issues go here, and an item leaves this file when it is fixed or picked up again as open work. Last brought up to date 2026-11-24.

Mostly seen while testing (2026-11-19 to 2026-11-24). Some were decided; the rest wait until they become a problem.

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
- **Every role the bot stores is a self-role** (by design, see `decisions.md`), with these accepted consequences:
  - A muted user who can still post in the role channel could take the mute role off with `-muted`; that depends on the server's channel permissions.
  - `!self role remove` fails while the role is in use as the mute, notification or poll role (its foreign key), and the admin is told it could not be removed.
- **R6 rounds for more than 39 players (decided 2026-11-24).** Each operator list holds 39, and a 40th player in the voice channel makes the round fail; a voice channel that full won't happen.

**Last.fm (decided 2026-11-23):**
- `!lf artist` pages through the user's whole library, 1000 items a page, until it has 5 albums and 8 tracks of the artist. For a large library and a rarely played artist that is many requests. Last.fm offers no better way to get one user's plays of one artist's albums and tracks.

**Handlers and background work:**
- **The Twitch monitor's channel list (accepted 2026-11-24: unlikely, and a restart fixes it).** The monitor reloads its channel list every tick, so `!twitch add` applies without a restart. It skips an empty list, though, so after the last channel anywhere is removed it keeps checking that one; its notification then finds no server and sends nothing. And when no server follows any Twitch channel at startup, `TwitchAPI.Check` logs "No channels to monitor on twitch, service stopped!" and never starts the monitor, so a channel added after that needs a restart.
- **Unmutes right after connecting.** A server Discord hasn't sent yet (or one in an outage) has no members, so a mute ending in that minute would count its user as gone. `IGuild.Available` can't tell, since `SocketGuild` always answers true (checked 2026-11-19); only the socket type's `IsConnected` could. The window is seconds long, and the next tick is a minute later.
- The music commands change the queue list while the play loop reads it, without a lock. A clash is unlikely and depends on timing, so no test covers it.
- **Discord.Net answers twice for a custom ID it can't read (accepted 2026-11-24).** For a component or modal whose `*` part doesn't convert (`BiasGame_Setup_Gender_nobody_someone`), `ComponentCommandInfo.ParseArgumentsAsync` raises `InteractionExecuted`, and `CommandInfo.ExecuteInternalAsync` raises it again with the same result. `InteractionHandler` then logs two warnings and answers twice: once as the reply, then as a follow-up. Only a custom ID the bot built wrong can reach it.
  - A fix was tried and reverted on 2026-11-24: `InteractionHandler` remembered the interactions it had answered in a `ConditionalWeakTable` and skipped the second report. The harm is a repeated error message and log line, so it wasn't worth adding state to the singleton.

**External services:**
- **dbkpop.com's table isn't read.** Reading it in the weekly idol refresh is commented out on purpose, because the site blocked the bot or timed out, so only idols with a profile link are refreshed.
- **Who-knows by track and a deleted Last.fm account (checked 2026-11-24).** `artist.getInfo` for a username that doesn't exist answers the artist without the user's play count, which counts as not listened, so a deleted account doesn't break who-knows by artist. `track.getInfo` wasn't checked, and who-knows stops at the first member Last.fm answers with an error.
