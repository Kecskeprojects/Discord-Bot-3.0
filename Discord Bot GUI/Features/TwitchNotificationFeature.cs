using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features
{
    public class TwitchNotificationFeature(IServerService serverService, DiscordSocketClient client, Logging logger)
    {
        private readonly IServerService serverService = serverService;
        private readonly DiscordSocketClient client = client;
        private readonly Logging logger = logger;

        public async Task Run(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
        {
            try
            {
                ServerResource server = await serverService.GetByDiscordIdAsync(twitchChannel.ServerDiscordId);

                //Do not send a message if a channel was not set
                if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.TwitchNotificationText, out List<ulong> notificationChannels))
                {
                    foreach (ulong channelId in notificationChannels)
                    {
                        IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
                        EmbedBuilder builder = BuildTwitchEmbed(twitchChannel, thumbnailUrl, title);

                        //If there is no notification role set on the server, we just send a message without the role ping
                        string notifRole = !NumberTools.IsNullOrZero(twitchChannel.NotificationRoleDiscordId) ? $"<@&{twitchChannel.NotificationRoleDiscordId}>" : "";

                        await channel.SendMessageAsync(notifRole, false, builder.Build());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("TwitchNotificationFeature.cs Run", ex);
            }
        }

        private static EmbedBuilder BuildTwitchEmbed(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
        {
            string thumbnail = thumbnailUrl.Replace("{width}", "1024").Replace("{height}", "576");

            EmbedBuilder builder = new();
            builder.WithTitle("Stream is now online!");
            builder.AddField(title != "" ? title : "No Title", twitchChannel.TwitchLink, false);
            builder.WithImageUrl(thumbnail);
            builder.WithCurrentTimestamp();

            builder.WithColor(Color.Purple);
            return builder;
        }
    }
}
