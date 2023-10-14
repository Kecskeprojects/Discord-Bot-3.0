using Discord;
using Discord.WebSocket;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.CommandsService
{
    public class AdminService
    {
        public static EmbedBuilder CreateServerSettingEmbed(ServerResource server, Config config, IReadOnlyCollection<SocketTextChannel> textChannels)
        {
            EmbedBuilder embed = new();

            embed.WithTitle("The server's settings are the following:");
            foreach (KeyValuePair<ChannelTypeEnum, string> item in ChannelTypeNameCollections.EnumName)
            {
                if (server.SettingsChannels.ContainsKey(item.Key))
                {
                    IEnumerable<string> channels = server.SettingsChannels[item.Key]
                                                    .Select(x => textChannels.FirstOrDefault(n => n.Id == x))
                                                    .Where(x => x != null)
                                                    .Select(x => $"`{x.Name}`");

                    embed.AddField($"{item.Value}:", $"`{string.Join(", ", channels)}`");
                }
                else
                {
                    embed.AddField($"{item.Value}:", "`none`");
                }
            }

            if (server.TwitchChannels.Count > 0)
            {
                embed.AddField("Notification role:", $"`{(server.TwitchChannels[0].RoleName != null ? server.TwitchChannels[0].RoleName : "none")}`");
                embed.AddField("Notified Twitch Channel IDs:", $"`{string.Join(",", server.TwitchChannels.Select(x => x.TwitchId))}`");
                embed.AddField("Notified Twitch channel URLs:", $"`{string.Join(",", server.TwitchChannels.Select(x => x.TwitchLink))}`");
            }
            else
            {
                embed.AddField("Notification role:", $"`none`");
                embed.AddField("Notified Twitch Channel IDs:", $"`none`");
                embed.AddField("Notified Twitch channel URLs:", $"`none`");
            }

            embed.WithThumbnailUrl(config.Img);
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Color.Teal);
            return embed;
        }
    }
}
