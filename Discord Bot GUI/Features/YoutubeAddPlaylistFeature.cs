using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using Discord;

namespace Discord_Bot.Features;

public class YoutubeAddPlaylistFeature(
    IDiscordClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IDiscordClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            ulong channelId = (ulong) Parameters;
            if (await client.GetChannelAsync(channelId, CacheMode.CacheOnly) is not IMessageChannel channel)
            {
                logger.Warning("YoutubeAddPlaylistFeature.cs ExecuteCoreLogicAsync", $"Channel {channelId} is not available, the playlist is not offered.");
                return false;
            }

            IUserMessage message = await channel.SendMessageAsync("You requested a song from a playlist!\n Do you want to me to add the playlist to the queue?");
            await message.AddReactionAsync(new Emoji("\U00002705"));

            //Wait 15 seconds for user to react to message, and then delete it, also delete it if they react, but add playlist
            int timer = 0;
            while (timer <= 15)
            {
                IEnumerable<IUser> result = await message.GetReactionUsersAsync(new Emoji("\U00002705"), 5).FlattenAsync();

                if (result.Count() > 1)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), timeProvider);
                timer++;
            }
            await message.DeleteAsync();

            return timer <= 15;
        }
        catch (Exception ex)
        {
            logger.Error("YoutubeAddPlaylistFeature.cs ExecuteCoreLogicAsync", ex);
        }
        return false;
    }
}
