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
public class AdminMuteCommands(
    IServerMutedUserService serverMutedUserService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{

    [Command("mute role")]
    [RequireUserPermission(ChannelPermission.MuteMembers)]
    [RequireContext(ContextType.Guild)]
    [Summary("Changing server mute role")]
    public async Task ChangeServerMuteRole([Remainder] string name)
    {
        try
        {
            //Check if role with that name exists
            IRole role = Context.Guild.Roles.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

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
    [Summary("Adding mute role to user")]
    public async Task MuteUser([Remainder] string data)
    {
        try
        {
            string username = "";
            string timeData = "";
            if (data.Split(">").Length == 1)
            {
                username = data;
                timeData = "10000y";
            }
            else if (data.Split(">").Length == 2)
            {
                username = data.Split(">")[0];
                timeData = data.Split(">")[1];
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

            if(server.MuteRoleDiscordId == null)
            {
                await ReplyAsync("Mute role not set on server.");
                return;
            }

            List<ulong> roles = [.. user.RoleIds];
            await user.RemoveRolesAsync(roles);
            await user.AddRoleAsync(server.MuteRoleDiscordId.Value);

            List<string> amounts = StringTools.GetTimeMeasurements(timeData);

            if (amounts.Count % 2 == 0)
            {
                //Check what lengths of time we need to deal with and add it to the current date
                if (!DateTimeTools.TryAddValuesToDate(amounts, out DateTime mutedUntil))
                {
                    return;
                }

                DbProcessResultEnum result = await serverMutedUserService.AddMutedUserAsync(Context.Guild.Id, user.Id, mutedUntil, string.Join(";", roles));
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"User has been muted until {TimestampTag.FromDateTime(mutedUntil, TimestampTagStyles.ShortDateTime)}",
                    DbProcessResultEnum.AlreadyExists => "User Already muted.",
                    _ => "User can't be unmuted because of a database error!"
                };

                if (result != DbProcessResultEnum.Success)
                {
                    await user.AddRolesAsync(roles);
                }

                await ReplyAsync(resultMessage);
                await user.SendMessageAsync($"You have been muted in '{Context.Guild.Name}' until {TimestampTag.FromDateTime(mutedUntil, TimestampTagStyles.ShortDateTime)}.");
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
    [Summary("Removing mute role from user")]
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
            await user.SendMessageAsync($"You have now been unmuted in '{Context.Guild.Name}'.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminMuteCommands.cs UnmuteUser", ex);
        }
    }
}
