using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Discord_Bot.CommandsService;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Instagram;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace Discord_Bot.Commands
{
    public class ServiceDiscordCommunication : ModuleBase<SocketCommandContext>, IServiceDiscordCommunication
    {
        private readonly IServerService serverService;
        private readonly DiscordSocketClient client;
        private readonly IInstaLoader instaLoader;
        private readonly Logging logger;

        public ServiceDiscordCommunication(IServerService serverService, DiscordSocketClient client, IInstaLoader instaLoader, Logging logger)
        {
            this.serverService = serverService;
            this.client = client;
            this.instaLoader = instaLoader;
            this.logger = logger;
        }

        public async Task SendInstagramPostEmbed(Uri uri, ulong messageId, ulong channelId, ulong? guildId)
        {
            IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
            bool shouldMessageBeSuppressed = false;
            bool hasFileDownloadHappened = false;

            List<FileAttachment> attachments = null;
            string postId = uri.Segments[2].EndsWith('/') ? uri.Segments[2][..^1] : uri.Segments[2];
            try
            {
                instaLoader.DownloadFromInstagram(postId);

                string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"Instagram\\{postId}"));
                string caption = "";
                Node metadata = null;

                attachments = ServiceDiscordCommunicationService.ReadFiles(files, ref caption, ref metadata, false);
                string message = ServiceDiscordCommunicationService.GetCaption(uri, caption, metadata);
                hasFileDownloadHappened = true;

                MessageReference refer = new(messageId, channelId, guildId, false);
                try
                {
                    await channel.SendFilesAsync(attachments, message, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                    shouldMessageBeSuppressed = true;
                }
                catch (HttpException ex)
                {
                    if (ex.Message.Contains("40005"))
                    {
                        logger.Warning("ServiceDiscordCommunication.cs GetImagesFromPost", "Embed too large, only sending images!", LogOnly: true);
                        logger.Warning("ServiceDiscordCommunication.cs GetImagesFromPost", ex.ToString(), LogOnly: true);

                        attachments = ServiceDiscordCommunicationService.ReadFiles(files, ref caption, ref metadata, true);
                        if (!CollectionTools.IsNullOrEmpty(attachments))
                        {
                            await channel.SendFilesAsync(attachments, caption, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                            shouldMessageBeSuppressed = true;
                        }
                        else
                        {
                            await channel.SendMessageAsync("Post content too large to send!");
                        }
                    }
                    else
                    {
                        logger.Error("ServiceDiscordCommunication.cs GetImagesFromPost HttpException", ex.ToString());
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    logger.Warning("ServiceDiscordCommunication.cs GetImagesFromPost", "Embed too large, only sending images!", LogOnly: true);
                    logger.Warning("ServiceDiscordCommunication.cs GetImagesFromPost", ex.ToString(), LogOnly: true);

                    attachments = ServiceDiscordCommunicationService.ReadFiles(files, ref caption, ref metadata, true);
                    if (attachments.Count > 0)
                    {
                        await channel.SendFilesAsync(attachments, caption, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                    }
                    else
                    {
                        await channel.SendMessageAsync("Post content too large to send!");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("ServiceDiscordCommunication.cs GetImagesFromPost", ex.ToString());
                await channel.SendMessageAsync("Unexpected exception occured.");
            }

            if (hasFileDownloadHappened)
            {
                attachments.ForEach(x => x.Dispose());
                attachments.Clear();
                Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), $"Instagram/{postId}"), true);
            }

            if (shouldMessageBeSuppressed)
            {
                IUserMessage discordMessage = (await channel.GetMessageAsync(messageId)) as IUserMessage;
                await discordMessage.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
            }
        }

        public async Task SendReminder(ReminderResource reminder)
        {
            //Modify message
            reminder.Message = reminder.Message.Insert(0, $"You told me to remind you at `{reminder.Date}` with the following message:\n\n");

            //Try getting user
            IUser user = await client.GetUserAsync(reminder.UserDiscordId);

            //If user exists send a direct message to the user
            if (user != null)
            {
                await UserExtensions.SendMessageAsync(user, reminder.Message);
            }
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
    }
}
