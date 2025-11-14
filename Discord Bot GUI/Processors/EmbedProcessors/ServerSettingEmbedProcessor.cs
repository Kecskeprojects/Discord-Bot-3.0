using Discord;
using Discord.WebSocket;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class ServerSettingEmbedProcessor
{
    public static Embed[] CreateEmbed(ServerResource server, IReadOnlyCollection<SocketTextChannel> textChannels, string thumbnailUrl)
    {
        EmbedBuilder embed = new();

        _ = embed.WithTitle("The server's settings are the following:");
        foreach (KeyValuePair<ChannelTypeEnum, string> item in ChannelTypeEnumTools.GetNameDictionary())
        {
            if (server.SettingsChannels.TryGetValue(item.Key, out List<ulong> settingsChannels))
            {
                IEnumerable<string> channels = settingsChannels.Select(x => textChannels.FirstOrDefault(n => n.Id == x))
                                                .Where(x => x != null)
                                                .Select(x => $"`{x.Name}`");

                _ = embed.AddField($"{item.Value}:", $"`{string.Join(", ", channels).Replace("`, `", ", ")}`");
            }
            else
            {
                _ = embed.AddField($"{item.Value}:", "`none`");
            }
        }

        _ = embed.AddField("Mute role:", $"`{server.MuteRoleName ?? "none"}`");
        _ = embed.AddField("Notification role:", $"`{server.NotificationRoleName ?? "none"}`");

        if (server.TwitchChannels.Count > 0)
        {
            _ = embed.AddField("Notified Twitch Channel IDs:", $"`{string.Join(",", server.TwitchChannels.Select(x => x.TwitchId))}`");
            _ = embed.AddField("Notified Twitch channel URLs:", $"`{string.Join(",", server.TwitchChannels.Select(x => x.TwitchLink))}`");
        }
        else
        {
            _ = embed.AddField("Notified Twitch Channel IDs:", $"`none`");
            _ = embed.AddField("Notified Twitch channel URLs:", $"`none`");
        }

        _ = embed.WithThumbnailUrl(thumbnailUrl);
        _ = embed.WithTimestamp(DateTime.UtcNow);
        _ = embed.WithColor(Color.Teal);
        return [embed.Build()];
    }
}
