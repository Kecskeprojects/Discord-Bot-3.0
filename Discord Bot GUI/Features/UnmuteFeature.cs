using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.WebSocket;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Features;

public class UnmuteFeature(
    IServerMutedUserService serverMutedUserService,
    DiscordSocketClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IServerMutedUserService serverMutedUserService = serverMutedUserService;
    private readonly DiscordSocketClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of mutes that ended before or exactly at this minute
            DateTime now = timeProvider.GetUtcNow().UtcDateTime;
            DateTime dateTime = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
            List<ServerMutedUserResource> result = await serverMutedUserService.GetExpiredMutedUsersAsync(dateTime);
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                foreach (ServerMutedUserResource mutedUser in result)
                {
                    ulong serverId = ulong.Parse(mutedUser.ServerDiscordId);
                    ulong userId = ulong.Parse(mutedUser.UserDiscordId);

                    //The bot may have left the server, or the user may have left it (which also drops their roles)
                    SocketGuild? server = client.GetGuild(serverId);
                    SocketGuildUser? user = null;
                    if (server != null)
                    {
                        await server.DownloadUsersAsync();
                        user = server.GetUser(userId);
                    }

                    if (server != null && user != null)
                    {
                        ServerResource? serverData = await serverService.GetByDiscordIdAsync(server.Id);

                        if (serverData?.MuteRoleDiscordId == null)
                        {
                            logger.Log($"Server {server.Id} does not have a mute role, or it could not be loaded.");
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

                        _ = await user.SendMessageAsync($"You have now been unmuted in '**{server.Name}**'.");
                    }
                    else
                    {
                        //Removing the entry anyway, otherwise this user would fail every minute and stop the users after them from being unmuted
                        logger.Log($"Server {serverId} or user {userId} is not available anymore, only the mute entry is removed.");
                    }

                    DbProcessResultEnum removeResult = await serverMutedUserService.RemoveMutedUserAsync(serverId, userId);
                    if (removeResult == DbProcessResultEnum.Failure)
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
