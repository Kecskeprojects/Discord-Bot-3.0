using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Features;
public class UnmuteFeature(IServerMutedUserService serverMutedUserService, DiscordSocketClient client, IServerService serverService, BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IServerMutedUserService serverMutedUserService = serverMutedUserService;
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of reminders that are before or exactly set to this minute
            DateTime dateTime = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
            List<ServerMutedUserResource> result = await serverMutedUserService.GetExpiredMutedUsersAsync(dateTime);
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                foreach (ServerMutedUserResource mutedUser in result)
                {
                    SocketGuild server = client.GetGuild(ulong.Parse(mutedUser.ServerDiscordId));

                    await server.DownloadUsersAsync();
                    SocketGuildUser user = server.GetUser(ulong.Parse(mutedUser.UserDiscordId));

                    ServerResource serverData = await serverService.GetByDiscordIdAsync(server.Id);

                    if (serverData.MuteRoleDiscordId == null)
                    {
                        logger.Log($"Server {serverData.DiscordId} does not have a mute role.");
                    }
                    else
                    {
                        await user.RemoveRoleAsync(serverData.MuteRoleDiscordId.Value);
                    }

                    if (string.IsNullOrEmpty(mutedUser.RemovedRoleDiscordIds))
                    {
                        logger.Log($"User {mutedUser.UserDiscordId} does not have roles that need to be reassigned.");
                    }
                    else
                    {
                        await user.AddRolesAsync(mutedUser.RemovedRoleDiscordIds.Split(";").Select(ulong.Parse));
                    }

                    //If user exists send a direct message to the user
                    if (user != null)
                    {
                        await user.SendMessageAsync($"You have now been unmuted in '**{server.Name}**'.");
                    }
                    DbProcessResultEnum reminderResult = await serverMutedUserService.RemoveMutedUserAsync(server.Id, user.Id);
                    if (reminderResult == DbProcessResultEnum.Failure)
                    {
                        logger.Error("UnmuteFeature.cs ExecuteCoreLogicAsync", "Failure during automatic unmuting process!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UnmuteFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }
}
