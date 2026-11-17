using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class TwitchNotificationFeature(
    DiscordSocketClient client,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            TwitchNotificationData data = (TwitchNotificationData) Parameters;

            ServerResource? server = await serverService.GetByDiscordIdAsync(data.TwitchChannel.ServerDiscordId);
            if (server == null)
            {
                logger.Warning("TwitchNotificationFeature.cs ExecuteCoreLogicAsync", $"Server {data.TwitchChannel.ServerDiscordId} could not be loaded, no notification is sent.");
                return false;
            }

            //Do not send a message if a channel was not set
            if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.TwitchNotificationText, out List<ulong>? notificationChannels))
            {
                foreach (ulong channelId in notificationChannels)
                {
                    Embed[] embed = TwitchNotificationEmbedProcessor.CreateEmbed(data.TwitchChannel, data.ThumbnailUrl, data.Title);

                    //If there is no notification role set on the server, we just send a message without the role ping
                    string notifRole = !NumberTools.IsNullOrZero(data.TwitchChannel.NotificationRoleDiscordId) ? $"<@&{data.TwitchChannel.NotificationRoleDiscordId}>" : "";

                    if (client.GetChannel(channelId) is not IMessageChannel channel)
                    {
                        logger.Error("TwitchNotificationFeature.cs Run", $"Channel (ID: {channelId}) was not found to send twitch notification. (Server ID: {server.ServerId}).");
                        return false;
                    }

                    _ = await channel.SendMessageAsync(notifRole, embeds: embed);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitchNotificationFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }
}
