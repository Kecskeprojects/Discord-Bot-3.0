using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

[Name("Mute")]
[Remarks("Admin")]
[Summary("Mute functionality using specified role")]
public class AdminMuteCommands(
    IServerMutedUserService serverMutedUserService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
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
            //Get artist's name and the track for search
            string userIdOrName = parameters.Split('>')[0].Trim().ToLower();
            string timeData = parameters.Split('>').Length == 2
                                ? parameters.Split('>')[1].Trim().ToLower()
                                : "5000y";

            IUser user = null;
            if (ulong.TryParse(userIdOrName, out ulong id))
            {
                user = await Context.Client.GetUserAsync(id);
            }
            else
            {
                await Context.Guild.DownloadUsersAsync();
                IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(userIdOrName, 1);
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
            SocketGuildUser guildUser = (SocketGuildUser) user;
            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                _ = await ReplyAsync("Mute role not set on server.");
                return;
            }

            int highestRolePosition = Context.Guild.CurrentUser.Roles.Max(x => x.Position);

            List<SocketRole> roleList = Context.Guild.Roles
                .Where(gr => !gr.IsEveryone && gr.Position < highestRolePosition)
                .Join(guildUser.Roles, gr => gr.Id, gur => gur.Id, (gr, gur) => new { gr, gur })
                .Select(group => group.gr)
                .ToList();

            await guildUser.RemoveRolesAsync(roleList);
            await guildUser.AddRoleAsync(server.MuteRoleDiscordId.Value);

            List<string> amounts = StringTools.GetTimeMeasurements(timeData);

            if (amounts.Count % 2 == 0)
            {
                //Check what lengths of time we need to deal with and add it to the current date
                if (!DateTimeTools.TryAddValuesToDate(amounts, out DateTime mutedUntil))
                {
                    return;
                }

                DbProcessResultEnum result = await serverMutedUserService.AddMutedUserAsync(Context.Guild.Id, user.Id, mutedUntil, string.Join(";", roleList.Select(x => x.Id)));
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"User has been muted until {TimestampTag.FromDateTime(mutedUntil, TimestampTagStyles.ShortDateTime)}",
                    DbProcessResultEnum.AlreadyExists => "User Already muted.",
                    _ => "User can't be unmuted because of a database error!"
                };

                if (result != DbProcessResultEnum.Success)
                {
                    await guildUser.RemoveRoleAsync(server.MuteRoleDiscordId.Value);
                    await guildUser.AddRolesAsync(roleList);
                }

                _ = await ReplyAsync(resultMessage);
                _ = await guildUser.SendMessageAsync($"You have been muted in '**{Context.Guild.Name}**' until {TimestampTag.FromDateTime(mutedUntil, TimestampTagStyles.ShortDateTime)}.");
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
            SocketGuildUser guildUser = (SocketGuildUser) user;
            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                _ = await ReplyAsync("Mute role not set on server.");
                return;
            }

            ServerMutedUserResource mutedUser = await serverMutedUserService.GetMutedUserByUsernameAsync(Context.Guild.Id, guildUser.Id);

            if (string.IsNullOrEmpty(mutedUser.RemovedRoleDiscordIds))
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
