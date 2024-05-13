using Discord;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public static class HelpAdminEmbedProcessor
    {
        public static Embed[] CreateEmbed(string imageUrl)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Admin Commands");

            int i = 1;
            Dictionary<string, string> commands = ReadCommandsFile();
            foreach (KeyValuePair<string, string> item in commands)
            {
                builder.AddField(item.Key, item.Value, false);
                i++;
            }
            builder.WithThumbnailUrl(imageUrl);
            builder.WithColor(Color.Orange);
            return [builder.Build()];
        }

        private static Dictionary<string, string> ReadCommandsFile()
        {
            Dictionary<string, string> commands = [];
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
            return commands;
        }
    }
}
