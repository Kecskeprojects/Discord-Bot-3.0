using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Interfaces;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Commands.Admin;

[Name("Mute")]
[Remarks("Admin")]
[Summary("Mute functionality using specified role")]
public class AdminMuteCommands(
    IServerMutedUserService serverMutedUserService,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseCommand(logger, config, serverService)
{

    [Command("change mute role")]
    [Alias(["tomr"])]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Setting the server specific mute role that will be used by the mute commands")]
    public async Task ChangeServerMuteRole([Remainder][Name("role name")] IRole role)
    {
        try
        {
            DbProcessResultEnum result = await serverService.ChangeServerMuteRoleAsync(serverId: Context.Guild.Id, roleName: role.Name.ToLower(), roleId: role.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"Mute role successfully changed to: {role.Name}.",
                DbProcessResultEnum.UpdatedExisting => "Overridden previous role.",
                DbProcessResultEnum.NotFound => "Server not found.",
                _ => "Role could not be changed!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs ChangeServerMuteRole", ex);
        }
    }

    [Command("mute")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Muting user with server specific mute role, also removes all role from user that are lower in the hierarchy than the mute role\n*if left empty, user will be muted permanently, amount of time in years to minutes (e.g.: '15h 5year6day' is a valid amount)")]
    public async Task MuteUser([Remainder][Name("username>amount*")] string parameters)
    {
        try
        {
            //The user (ID or name) and, after a >, how long the mute lasts; without a duration it is practically permanent
            string[] parts = parameters.Split('>');
            string userIdOrName = parts[0].Trim().ToLower();
            string timeData = parts.Length == 2 ? parts[1].Trim().ToLower() : "5000y";

            IGuildUser? user = null;
            if (ulong.TryParse(userIdOrName, out ulong id))
            {
                user = await Context.Guild.GetUserAsync(id);
            }
            else
            {
                await Context.Guild.DownloadUsersAsync();
                IReadOnlyCollection<IGuildUser> users = await Context.Guild.SearchUsersAsync(userIdOrName, 1);
                if (users.Count > 0)
                {
                    user = users.First();
                }
            }

            if (user == null)
            {
                _ = await ReplyAsync("No user was found with that ID!");
                return;
            }

            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                _ = await ReplyAsync("Mute role not set on server.");
                return;
            }
            ulong muteRoleId = server.MuteRoleDiscordId.Value;

            //Read before any role changes, so a mistyped duration leaves the user as they were
            List<string> amounts = StringTools.GetTimeMeasurements(timeData);
            DateTime now = timeProvider.GetUtcNow().UtcDateTime;
            if (amounts.Count % 2 != 0 || !DateTimeTools.TryAddValuesToDate(now, amounts, out DateTime mutedUntil))
            {
                _ = await ReplyAsync("The duration could not be read!");
                return;
            }

            //The bot can only take away roles below its own highest one. The mute role is never one to give back,
            //or unmuting would put it back on
            IGuildUser botUser = await Context.Guild.GetCurrentUserAsync(CacheMode.CacheOnly);
            int highestRolePosition = Context.Guild.Roles.Where(gr => botUser.RoleIds.Contains(gr.Id)).Max(gr => gr.Position);

            List<IRole> roleList = Context.Guild.Roles
                .Where(gr => gr.Id != Context.Guild.EveryoneRole.Id && gr.Id != muteRoleId && gr.Position < highestRolePosition && user.RoleIds.Contains(gr.Id))
                .ToList();

            //Saved before the roles change: a user who is already muted is left as they are,
            //and roles are only taken once they can be given back
            DbProcessResultEnum result = await serverMutedUserService.AddMutedUserAsync(Context.Guild.Id, user.Id, now, mutedUntil, string.Join(";", roleList.Select(x => x.Id)));
            if (result != DbProcessResultEnum.Success)
            {
                _ = await ReplyAsync(result == DbProcessResultEnum.AlreadyExists ? "User Already muted." : "User can't be muted because of a database error!");
                return;
            }

            if (!await TryMuteRolesAsync(user, roleList, muteRoleId))
            {
                _ = await serverMutedUserService.RemoveMutedUserAsync(Context.Guild.Id, user.Id);
                _ = await ReplyAsync("The user's roles could not be changed. The bot needs the Manage Roles permission, and its role has to be above the mute role.");
                return;
            }

            _ = await ReplyAsync($"User has been muted until {DiscordTools.UtcTimestamp(mutedUntil, TimestampTagStyles.ShortDateTime)}");
            await TellUserAsync(user, $"You have been muted in '**{Context.Guild.Name}**' until {DiscordTools.UtcTimestamp(mutedUntil, TimestampTagStyles.ShortDateTime)}.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs MuteUser", ex);
        }
    }

    //False when Discord refused the change; the roles taken by then are given back
    private async Task<bool> TryMuteRolesAsync(IGuildUser user, List<IRole> roleList, ulong muteRoleId)
    {
        try
        {
            await user.RemoveRolesAsync(roleList);
            await user.AddRoleAsync(muteRoleId);
            return true;
        }
        catch (Exception ex)
        {
            logger.Warning("AdminMuteCommands.cs TryMuteRolesAsync", ex);
        }

        try
        {
            await user.AddRolesAsync(roleList);
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs TryMuteRolesAsync", ex);
        }
        return false;
    }

    //The roles have changed by then, so a user with closed direct messages only costs a warning
    private async Task TellUserAsync(IGuildUser user, string message)
    {
        try
        {
            _ = await user.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            logger.Warning("AdminMuteCommands.cs TellUserAsync", ex);
        }
    }

    [Command("unmute")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Unmuting user by removing server specific mute role, also adds all other roles that were removed upon muting")]
    public async Task UnmuteUser([Remainder][Name("user name")] IUser user)
    {
        try
        {
            IGuildUser guildUser = (IGuildUser) user;
            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                _ = await ReplyAsync("Mute role not set on server.");
                return;
            }

            ServerMutedUserResource? mutedUser = await serverMutedUserService.GetMutedUserByUsernameAsync(Context.Guild.Id, guildUser.Id);

            if (mutedUser == null)
            {
                _ = await ReplyAsync("User is not currently muted.");
                return;
            }

            //Empty when the user had no roles below the bot's at the time of the mute
            if (!string.IsNullOrEmpty(mutedUser.RemovedRoleDiscordIds))
            {
                await guildUser.AddRolesAsync(mutedUser.RemovedRoleDiscordIds.Split(";").Select(ulong.Parse));
            }
            await guildUser.RemoveRoleAsync(server.MuteRoleDiscordId.Value);

            DbProcessResultEnum result = await serverMutedUserService.RemoveMutedUserAsync(Context.Guild.Id, guildUser.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"User has been unmuted.",
                _ => "User can't be unmuted because of a database error!"
            };
            _ = await ReplyAsync(resultMessage);
            await TellUserAsync(guildUser, $"You have now been unmuted in '**{Context.Guild.Name}**'.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs UnmuteUser", ex);
        }
    }
}
