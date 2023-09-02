using Discord;
using Discord.Commands;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class ServiceDiscordCommunication : ModuleBase<SocketCommandContext>, IServiceDiscordCommunication
    {
        private readonly IServerService serverService;
        public ServiceDiscordCommunication(IServerService serverService) => this.serverService = serverService;

        public async Task SendTwitchEmbed(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
        {
            ServerResource server = await serverService.GetByDiscordIdAsync(twitchChannel.ServerDiscordId);

            //Do not send a message if a channel was not set
            if (server.SettingsChannels.ContainsKey(ChannelTypeEnum.TwitchNotificationText))
            {
                foreach (ulong channelId in server.SettingsChannels[ChannelTypeEnum.TwitchNotificationText])
                {
                    IMessageChannel channel = Context.Client.GetChannel(channelId) as IMessageChannel;

                    string thumbnail = thumbnailUrl.Replace("{width}", "1024").Replace("{height}", "576");

                    EmbedBuilder builder = new();
                    builder.WithTitle("Stream is now online!");
                    builder.AddField(title != "" ? title : "No Title", twitchChannel.TwitchLink, false);
                    builder.WithImageUrl(thumbnail);
                    builder.WithCurrentTimestamp();

                    builder.WithColor(Color.Purple);

                    //If there is no notification role set on the server, we just send a message without the role ping
                    string notifRole = IntTools.IsNullOrZero(twitchChannel.RoleId) ? $"<@&{twitchChannel.RoleId}>" : "";

                    await channel.SendMessageAsync(notifRole, false, builder.Build());
                }
            }
        }

        public async Task<bool> YoutubeAddPlaylistMessage(ulong channelId)
        {
            IMessageChannel channel = Context.Client.GetChannel(channelId) as IMessageChannel;
            IUserMessage message = await channel.SendMessageAsync("You requested a song from a playlist!\n Do you want to me to add the playlist to the queue?"); //Todo: Was RestUserMessage, does it affect the logic?
            await message.AddReactionAsync(new Emoji("\U00002705"));

            //Wait 15 seconds for user to react to message, and then delete it, also delete it if they react, but add playlist
            int timer = 0;
            while (timer <= 15)
            {
                IEnumerable<IUser> result = await message.GetReactionUsersAsync(new Emoji("\U00002705"), 5).FlattenAsync();

                if (result.Count() > 1) return true;

                await Task.Delay(1000);
                timer++;
            }
            await message.DeleteAsync();
            return false;
        }
    }
}
