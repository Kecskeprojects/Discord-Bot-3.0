using Discord;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Wotd;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.CommandsService
{
    public class ChatService
    {
        #region CustomList
        public static EmbedBuilder BuildCustomListEmbed(List<CustomCommandResource> list)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Custom commands:");
            string commands = "";

            foreach (CustomCommandResource command in list)
            {
                if (commands == "")
                {
                    commands += "!" + command.Command;
                }
                else
                {
                    commands += " , !" + command.Command;
                }
            }
            builder.WithDescription(commands);
            builder.WithColor(Color.Teal);
            return builder;
        }
        #endregion

        #region Help
        public static void ReadCommandsFile(Dictionary<string, string> commands)
        {
            using (StreamReader reader = new("Assets\\Commands\\Commands.txt"))
            {
                string curr = "";
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line == "") { continue; }

                    if (line.StartsWith('!') || line.StartsWith('.'))
                    {
                        commands[curr] += "`" + line.Split('\t')[0] + " ` " + line.Split('\t')[1] + "\n";
                    }
                    else if (line.StartsWith('*'))
                    {
                        commands[curr] += line + "\n";
                    }
                    else
                    {
                        commands.Add(line, "");
                        curr = line;
                    }
                }
            };
        }

        public static EmbedBuilder BuildHelpEmbed(Dictionary<string, string> commands, string imageUrl)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("List of Commands");

            foreach (KeyValuePair<string, string> item in commands)
            {
                builder.AddField(item.Key, item.Value, false);
            }
            builder.WithThumbnailUrl(imageUrl);
            builder.WithColor(Color.Orange);
            return builder;
        }
        #endregion

        #region Wotd
        public static EmbedBuilder BuildWotdEmbed(WotdBase result)
        {
            EmbedBuilder embed = new();

            embed.WithTitle($"{result.Words.Langname} word of the day:");

            embed.AddField("Word:", $"{result.Words.Word} ([Audio]({result.Words.Wordsound}))\n\n**Translation:**\n{result.Words.Translation}", true);
            embed.AddField("Example:", $"{result.Words.Fnphrase} ([Audio]({result.Words.Phrasesound}))\n\n**Translation:**\n{result.Words.Enphrase}", true);

            embed.WithColor(Color.Gold);
            embed.WithCurrentTimestamp();
            return embed;
        }
        #endregion
    }
}
