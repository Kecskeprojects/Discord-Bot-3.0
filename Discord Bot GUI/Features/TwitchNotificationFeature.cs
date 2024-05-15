using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
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
                        Embed[] embed = TwitchNotificationEmbedProcessor.CreateEmbed(twitchChannel, thumbnailUrl, title);

                        //If there is no notification role set on the server, we just send a message without the role ping
                        string notifRole = !NumberTools.IsNullOrZero(twitchChannel.NotificationRoleDiscordId) ? $"<@&{twitchChannel.NotificationRoleDiscordId}>" : "";

                        await channel.SendMessageAsync(notifRole, embeds: embed);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("TwitchNotificationFeature.cs Run", ex);
            }
        }
    }
}
