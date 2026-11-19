using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Features;

public class UnmuteFeature(
    IServerMutedUserService serverMutedUserService,
    IDiscordClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IServerMutedUserService serverMutedUserService = serverMutedUserService;
    private readonly IDiscordClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Until the gateway is connected no server is cached (a start without internet, or the minute that logs back in),
            //and a missing server would look like one the bot left: its mutes would be dropped without giving the roles back
            if (client.ConnectionState != ConnectionState.Connected)
            {
                return false;
            }

            //Get the list of mutes that ended before or exactly at this minute
            DateTime now = timeProvider.GetUtcNow().UtcDateTime;
            DateTime dateTime = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
            List<ServerMutedUserResource> result = await serverMutedUserService.GetExpiredMutedUsersAsync(dateTime);
            bool allUnmuted = true;
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                //Each user on their own, so one that fails doesn't stop the users after them from being unmuted
                foreach (ServerMutedUserResource mutedUser in result)
                {
                    //Not "allUnmuted = allUnmuted && ...": that would skip everyone after the first failure
                    if (!await TryUnmuteAsync(mutedUser))
                    {
                        allUnmuted = false;
                    }
                }
            }
            return allUnmuted;
        }
        catch (Exception ex)
        {
            logger.Error("UnmuteFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
    }

    //False when the roles could not be changed: the mute entry then stays, and the next minute tries again
    private async Task<bool> TryUnmuteAsync(ServerMutedUserResource mutedUser)
    {
        ulong serverId;
        ulong userId;
        try
        {
            serverId = ulong.Parse(mutedUser.ServerDiscordId);
            userId = ulong.Parse(mutedUser.UserDiscordId);

            //The bot may have left the server, or the user may have left it (which also drops their roles)
            IGuild? server = await client.GetGuildAsync(serverId, CacheMode.CacheOnly);
            IGuildUser? user = null;
            if (server != null)
            {
                await server.DownloadUsersAsync();
                user = await server.GetUserAsync(userId, CacheMode.CacheOnly);
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

                await TellUserAsync(user, server.Name);
            }
            else
            {
                //Removing the entry anyway, otherwise this user would be tried again every minute
                logger.Log($"Server {serverId} or user {userId} is not available anymore, only the mute entry is removed.");
            }
        }
        catch (Exception ex)
        {
            logger.Error("UnmuteFeature.cs TryUnmuteAsync", ex);
            return false;
        }

        DbProcessResultEnum removeResult = await serverMutedUserService.RemoveMutedUserAsync(serverId, userId);
        if (removeResult == DbProcessResultEnum.Failure)
        {
            logger.Error("UnmuteFeature.cs TryUnmuteAsync", "Failure during automatic unmuting process!");
            return false;
        }
        return true;
    }

    //The roles are back by now, so a refused message doesn't keep the mute entry. Kept, it would run again next minute and
    //take the mute role off a user who was muted again in the meantime.
    private async Task TellUserAsync(IGuildUser user, string serverName)
    {
        try
        {
            _ = await user.SendMessageAsync($"You have now been unmuted in '**{serverName}**'.");
        }
        catch (Exception ex)
        {
            logger.Warning("UnmuteFeature.cs TellUserAsync", ex);
        }
    }
}
