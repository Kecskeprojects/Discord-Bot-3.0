using Discord;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.CommandsService
{
    public class OwnerService
    {
        public static void ReadCommandsFile(Dictionary<string, string> commands)
        {
            using (StreamReader reader = new("Assets\\Commands\\Owner_Commands.txt"))
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
            builder.WithTitle("Owner Commands");

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
    }
}
