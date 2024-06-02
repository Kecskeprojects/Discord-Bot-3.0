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

namespace Discord_Bot.Features;

public class TwitchNotificationFeature(DiscordSocketClient client, IServerService serverService, BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            TwitchChannelResource twitchChannel = Parameters.TwitchChannel;
            string thumbnailUrl = Parameters.ThumbnailUrl;
            string title = Parameters.Title;

            ServerResource server = await serverService.GetByDiscordIdAsync(twitchChannel.ServerDiscordId);

            //Do not send a message if a channel was not set
            if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.TwitchNotificationText, out List<ulong> notificationChannels))
            {
                foreach (ulong channelId in notificationChannels)
                {
                    Embed[] embed = TwitchNotificationEmbedProcessor.CreateEmbed(twitchChannel, thumbnailUrl, title);

                    //If there is no notification role set on the server, we just send a message without the role ping
                    string notifRole = !NumberTools.IsNullOrZero(twitchChannel.NotificationRoleDiscordId) ? $"<@&{twitchChannel.NotificationRoleDiscordId}>" : "";

                    if (client.GetChannel(channelId) is not IMessageChannel channel)
                    {
                        logger.Error("TwitchNotificationFeature.cs Run", $"Channel (ID: {channelId}) was not found to send twitch notification. (Server ID: {server.ServerId}).");
                        return false;
                    }

                    await channel.SendMessageAsync(notifRole, embeds: embed);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitchNotificationFeature.cs Run", ex);
            return false;
        }
        return true;
    }
}
