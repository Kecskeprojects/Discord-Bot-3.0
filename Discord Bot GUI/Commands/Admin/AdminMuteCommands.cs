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

            timeData = timeData.ToLower();
            IGuildUser guildUser = user;
            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                _ = await ReplyAsync("Mute role not set on server.");
                return;
            }

            //The bot can only take away roles below its own highest one
            IGuildUser botUser = await Context.Guild.GetCurrentUserAsync(CacheMode.CacheOnly);
            int highestRolePosition = Context.Guild.Roles.Where(gr => botUser.RoleIds.Contains(gr.Id)).Max(gr => gr.Position);

            List<IRole> roleList = Context.Guild.Roles
                .Where(gr => gr.Id != Context.Guild.EveryoneRole.Id && gr.Position < highestRolePosition && guildUser.RoleIds.Contains(gr.Id))
                .ToList();

            await guildUser.RemoveRolesAsync(roleList);
            await guildUser.AddRoleAsync(server.MuteRoleDiscordId.Value);

            List<string> amounts = StringTools.GetTimeMeasurements(timeData);

            if (amounts.Count % 2 == 0)
            {
                //Check what lengths of time we need to deal with and add it to the current date
                DateTime now = timeProvider.GetUtcNow().UtcDateTime;
                if (!DateTimeTools.TryAddValuesToDate(now, amounts, out DateTime mutedUntil))
                {
                    return;
                }

                DbProcessResultEnum result = await serverMutedUserService.AddMutedUserAsync(Context.Guild.Id, user.Id, now, mutedUntil, string.Join(";", roleList.Select(x => x.Id)));
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"User has been muted until {DiscordTools.UtcTimestamp(mutedUntil, TimestampTagStyles.ShortDateTime)}",
                    DbProcessResultEnum.AlreadyExists => "User Already muted.",
                    _ => "User can't be unmuted because of a database error!"
                };

                if (result != DbProcessResultEnum.Success)
                {
                    await guildUser.RemoveRoleAsync(server.MuteRoleDiscordId.Value);
                    await guildUser.AddRolesAsync(roleList);
                }

                _ = await ReplyAsync(resultMessage);
                _ = await guildUser.SendMessageAsync($"You have been muted in '**{Context.Guild.Name}**' until {DiscordTools.UtcTimestamp(mutedUntil, TimestampTagStyles.ShortDateTime)}.");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs MuteUser", ex);
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

            if (mutedUser == null || string.IsNullOrEmpty(mutedUser.RemovedRoleDiscordIds))
            {
                _ = await ReplyAsync("User is not currently muted.");
                return;
            }

            await guildUser.AddRolesAsync(mutedUser.RemovedRoleDiscordIds.Split(";").Select(ulong.Parse));
            await guildUser.RemoveRoleAsync(server.MuteRoleDiscordId.Value);

            DbProcessResultEnum result = await serverMutedUserService.RemoveMutedUserAsync(Context.Guild.Id, guildUser.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"User has been unmuted.",
                _ => "User can't be unmuted because of a database error!"
            };
            _ = await ReplyAsync(resultMessage);
            _ = await guildUser.SendMessageAsync($"You have now been unmuted in '**{Context.Guild.Name}**'.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs UnmuteUser", ex);
        }
    }
}
