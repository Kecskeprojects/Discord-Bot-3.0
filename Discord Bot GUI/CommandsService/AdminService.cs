using Discord;
using Discord.WebSocket;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
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
                if (server.SettingsChannels.TryGetValue(item.Key, out List<ulong> settingsChannels))
                {
                    IEnumerable<string> channels = settingsChannels.Select(x => textChannels.FirstOrDefault(n => n.Id == x))
                                                    .Where(x => x != null)
                                                    .Select(x => $"`{x.Name}`");

                    embed.AddField($"{item.Value}:", $"`{string.Join(", ", channels).Replace("`, `", ", ")}`");
                }
                else
                {
                    embed.AddField($"{item.Value}:", "`none`");
                }
            }

            if (server.TwitchChannels.Count > 0)
            {
                embed.AddField("Notification role:", $"`{server.NotificationRoleName ?? "none"}`");
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
            embed.WithTimestamp(DateTime.UtcNow);
            embed.WithColor(Color.Teal);
            return embed;
        }

        #region Help
        public static void ReadCommandsFile(Dictionary<string, string> commands)
        {
            using (StreamReader reader = new("Assets\\Commands\\Admin_Commands.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.StartsWith('!') || line.StartsWith('.'))
                    {
                        string[] parts = line.Split("\t\t");
                        commands.Add(parts[0], parts[1]);
                    }
                }
            };
        }

        public static EmbedBuilder BuildHelpEmbed(Dictionary<string, string> commands, string imageUrl)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Admin Commands");

            int i = 1;
            foreach (KeyValuePair<string, string> item in commands)
            {
                builder.AddField(item.Key, item.Value, false);
                i++;
            }
            builder.WithThumbnailUrl(imageUrl);
            builder.WithColor(Color.Orange);
            return builder;
        }

        public static void ReadFeaturesFile(Dictionary<string, string> commands)
        {
            using (StreamReader reader = new("Assets\\Commands\\Features.txt"))
            {
                string curr = "";
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.StartsWith('-'))
                    {
                        commands[curr] += $"{line}\n";
                    }
                    else if (line != "")
                    {
                        commands.Add(line, "");
                        curr = line;
                    }
                }
            };
        }

        public static EmbedBuilder BuildFeaturesEmbed(Dictionary<string, string> commands, string imageUrl)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Passive processes");

            int i = 1;
            foreach (KeyValuePair<string, string> item in commands)
            {
                builder.AddField(item.Key, item.Value, false);
                i++;
            }
            builder.WithThumbnailUrl(imageUrl);
            builder.WithColor(Color.Orange);
            return builder;
        }
        #endregion
    }
}
