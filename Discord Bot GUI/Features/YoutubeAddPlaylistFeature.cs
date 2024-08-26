using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class YoutubeAddPlaylistFeature(DiscordSocketClient client, IServerService serverService, BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            ulong channelId = (ulong) Parameters;
            IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
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

                await Task.Delay(1000);
                timer++;
            }
            await message.DeleteAsync();

            return timer <= 15;
        }
        catch (Exception ex)
        {
            logger.Error("YoutubeAddPlaylistFeature.cs Run", ex);
        }
        return false;
    }
}
