using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Bits;
using TwitchLib.Api.Helix;
using Discord.Net;
using Microsoft.Extensions.Logging;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Commands
{
    public class ServiceDiscordCommunication : ModuleBase<SocketCommandContext>, IServiceDiscordCommunication
    {
        private readonly IServerService serverService;
        private readonly DiscordSocketClient client;
        private readonly Logging logger;

        public ServiceDiscordCommunication(IServerService serverService, DiscordSocketClient client, Logging logger)
        {
            this.serverService = serverService;
            this.client = client;
            this.logger = logger;
        }

        public async Task SendTwitchEmbed(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
        {
            ServerResource server = await serverService.GetByDiscordIdAsync(twitchChannel.ServerDiscordId);

            //Do not send a message if a channel was not set
            if (server.SettingsChannels.ContainsKey(ChannelTypeEnum.TwitchNotificationText))
            {
                foreach (ulong channelId in server.SettingsChannels[ChannelTypeEnum.TwitchNotificationText])
                {
                    IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;

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
            IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
            IUserMessage message = await channel.SendMessageAsync("You requested a song from a playlist!\n Do you want to me to add the playlist to the queue?");
            await message.AddReactionAsync(new Emoji("\U00002705"));

            //Wait 15 seconds for user to react to message, and then delete it, also delete it if they react, but add playlist
            int timer = 0;
            while (timer <= 15)
            {
                IEnumerable<IUser> result = await message.GetReactionUsersAsync(new Emoji("\U00002705"), 5).FlattenAsync();

                if (result.Count() > 1) break;

                await Task.Delay(1000);
                timer++;
            }
            await message.DeleteAsync();

            return timer <= 15;
        }

        public async Task<string> SendTwitterMessage(List<Uri> videos, List<Uri> images, ulong channelId, MessageReference messageReference, string messages)
        {
            IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
            try
            {
                List<FileAttachment> result = AllContentInRegularMessage(videos, images);
                if (result.Count > 0)
                {
                    await channel.SendFilesAsync(result, messageReference: messageReference);

                    return "";
                }

                if (messages.Length > 0)
                {
                    return messages;
                }

                return "No image/videos in tweet.";
            }
            catch (HttpException ex)
            {
                if (ex.Message.Contains("40005"))
                {
                    logger.Warning("ServiceDiscordCommunication.cs SendTwitterMessage", "Embed too large, only sending images!", LogOnly: true);
                    logger.Warning("ServiceDiscordCommunication.cs SendTwitterMessage", ex.ToString(), LogOnly: true);

                    List<FileAttachment> result = AllContentInRegularMessage(videos, images, false);
                    if (result.Count > 0) await channel.SendFilesAsync(result, messageReference: messageReference);
                    else return "Post content too large to send!";

                    return "";
                }
            }

            return "Unexpected error occured!";
        }

        private static List<FileAttachment> AllContentInRegularMessage(List<Uri> videos, List<Uri> images, bool sendVideos = true)
        {
            List<FileAttachment> Embeds = new();
            string commonFileName = $"twitter_{DateTime.Now:yyMMdd}_{DateTime.Now:HHmmss}";

            for (int i = 0; i < (images.Count < 10 ? images.Count : 10) && Embeds.Count < 10; i++)
            {
                images[i] = new Uri(images[i].OriginalString.Split("?")[0] + "?format=jpg");
                Embeds.Add(new FileAttachment(Global.GetStream(images[i].OriginalString), $"{commonFileName}_image_{i + 1}.png"));
            }

            for (int i = 0; sendVideos && i < (videos.Count < 10 ? videos.Count : 10) && Embeds.Count < 10; i++)
            {
                Embeds.Add(new FileAttachment(Global.GetStream(videos[i].OriginalString), $"{commonFileName}_video_{i + 1}.mp4"));
            }

            return Embeds;
        }
    }
}
