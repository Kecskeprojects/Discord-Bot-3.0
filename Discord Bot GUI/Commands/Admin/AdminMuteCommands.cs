using Discord;
using Discord.Commands;
using Discord.Rest;
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
    [RequireUserPermission(ChannelPermission.MuteMembers)]
    [RequireContext(ContextType.Guild)]
    [Summary("Setting the server specific mute role that will be used by the other commands")]
    public async Task ChangeServerMuteRole([Remainder] string rolename)
    {
        try
        {
            //Check if role with that name exists
            IRole role = Context.Guild.Roles.Where(x => x.Name.Equals(rolename, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (role != null)
            {
                DbProcessResultEnum result = await serverService.ChangeServerMuteRoleAsync(serverId: Context.Guild.Id, roleName: role.Name.ToLower(), roleId: role.Id);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"Mute role successfully changed to: {role.Name}.",
                    DbProcessResultEnum.UpdatedExisting => "Overridden previous role.",
                    DbProcessResultEnum.NotFound => "Server not found.",
                    _ => "Role could not be changed!"
                };
                await ReplyAsync(resultMessage);
            }
            else
            {
                await ReplyAsync("Role not found!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs ChangeServerMuteRole", ex);
        }
    }

    [Command("mute")]
    [RequireUserPermission(ChannelPermission.MuteMembers)]
    [RequireContext(ContextType.Guild)]
    [Summary("Muting user with server specific mute role, also removes all other roles\n*if left empty, user will be muted permanently, amount of time in years to minutes (e.g.: '15h 5year6day' is a valid amount)")]
    public async Task MuteUser([Name("username>amount*")][Remainder] string parameters)
    {
        try
        {
            string username = "";
            string timeData = "";
            if (parameters.Split(">").Length == 1)
            {
                username = parameters;
                timeData = "5000y";
            }
            else if (parameters.Split(">").Length == 2)
            {
                username = parameters.Split(">")[0];
                timeData = parameters.Split(">")[1];
            }
            else
            {
                return;
            }

            IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(username, 1);
            if (users.Count == 0)
            {
                return;
            }
            RestGuildUser user = users.First();

            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                await ReplyAsync("Mute role not set on server.");
                return;
            }

            int highestRolePosition = Context.Guild.CurrentUser.Roles.Max(x => x.Position);

            List<ulong> userRoles = [.. user.RoleIds];

            List<ulong> roleIdList = Context.Guild.Roles.Where(x =>
                userRoles.Contains(x.Id) &&
                !x.IsEveryone &&
                x.Position < highestRolePosition).Select(x => x.Id).ToList();

            await user.RemoveRolesAsync(roleIdList);
            await user.AddRoleAsync(server.MuteRoleDiscordId.Value);

            List<string> amounts = StringTools.GetTimeMeasurements(timeData);

            if (amounts.Count % 2 == 0)
            {
                //Check what lengths of time we need to deal with and add it to the current date
                if (!DateTimeTools.TryAddValuesToDate(amounts, out DateTime mutedUntil))
                {
                    return;
                }

                DbProcessResultEnum result = await serverMutedUserService.AddMutedUserAsync(Context.Guild.Id, user.Id, mutedUntil, string.Join(";", roleIdList));
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"User has been muted until {TimestampTag.FromDateTime(mutedUntil, TimestampTagStyles.ShortDateTime)}",
                    DbProcessResultEnum.AlreadyExists => "User Already muted.",
                    _ => "User can't be unmuted because of a database error!"
                };

                if (result != DbProcessResultEnum.Success)
                {
                    await user.RemoveRoleAsync(server.MuteRoleDiscordId.Value);
                    await user.AddRolesAsync(roleIdList);
                }

                await ReplyAsync(resultMessage);
                await user.SendMessageAsync($"You have been muted in '**{Context.Guild.Name}**' until {TimestampTag.FromDateTime(mutedUntil, TimestampTagStyles.ShortDateTime)}.");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs MuteUser", ex);
        }
    }

    [Command("unmute")]
    [RequireUserPermission(ChannelPermission.MuteMembers)]
    [RequireContext(ContextType.Guild)]
    [Summary("Unmuting user by removing server specific mute role, also adds all other roles that were removed upon muting")]
    public async Task UnmuteUser([Remainder] string username)
    {
        try
        {
            IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(username, 1);
            if (users.Count == 0)
            {
                return;
            }
            RestGuildUser user = users.First();

            ServerResource server = await GetCurrentServerAsync();

            if (server.MuteRoleDiscordId == null)
            {
                await ReplyAsync("Mute role not set on server.");
                return;
            }

            ServerMutedUserResource mutedUser = await serverMutedUserService.GetMutedUserByUsernameAsync(Context.Guild.Id, user.Id);

            if (string.IsNullOrEmpty(mutedUser.RemovedRoleDiscordIds))
            {
                await ReplyAsync("User is not currently muted.");
                return;
            }

            await user.AddRolesAsync(mutedUser.RemovedRoleDiscordIds.Split(";").Select(ulong.Parse));
            await user.RemoveRoleAsync(server.MuteRoleDiscordId.Value);

            DbProcessResultEnum result = await serverMutedUserService.RemoveMutedUserAsync(Context.Guild.Id, user.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"User has been unmuted.",
                _ => "User can't be unmuted because of a database error!"
            };
            await ReplyAsync(resultMessage);
            await user.SendMessageAsync($"You have now been unmuted in '**{Context.Guild.Name}**'.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs UnmuteUser", ex);
        }
    }
}
