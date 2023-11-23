using Discord;
using Discord.WebSocket;
using Discord_Bot.CommandsService.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Communication
{
    public class ServiceDiscordCommunication(IServerService serverService, DiscordSocketClient client) : IServiceToDiscordCommunication
    {
        private readonly IServerService serverService = serverService;
        private readonly DiscordSocketClient client = client;

        public async Task SendTwitchEmbed(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
        {
            ServerResource server = await serverService.GetByDiscordIdAsync(twitchChannel.ServerDiscordId);

            //Do not send a message if a channel was not set
            if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.TwitchNotificationText, out List<ulong> notificationChannels))
            {
                foreach (ulong channelId in notificationChannels)
                {
                    IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
                    EmbedBuilder builder = ServiceToDiscordService.BuildTwitchEmbed(twitchChannel, thumbnailUrl, title);

                    //If there is no notification role set on the server, we just send a message without the role ping
                    string notifRole = !NumberTools.IsNullOrZero(twitchChannel.NotificationRoleDiscordId) ? $"<@&{twitchChannel.NotificationRoleDiscordId}>" : "";

                    await channel.SendMessageAsync(notifRole, false, builder.Build());
                }
            }
        }

        public async Task<bool> YoutubeAddPlaylistMessage(ulong channelId)
        {
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
    }
}
