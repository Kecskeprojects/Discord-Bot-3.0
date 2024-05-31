using Discord;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Processors.EmbedProcessors.Help
{
    public static class HelpUserEmbedProcessor
    {
        public static Embed[] CreateEmbed(string imageUrl)
        {
            EmbedBuilder builder = new();
            Dictionary<string, string> commands = ReadCommandsFile();
            builder.WithTitle("List of Commands");

            foreach (KeyValuePair<string, string> item in commands)
            {
                builder.AddField(item.Key, item.Value, false);
            }
            builder.WithThumbnailUrl(imageUrl);
            builder.WithColor(Color.Orange);
            return [builder.Build()];
        }

        private static Dictionary<string, string> ReadCommandsFile()
        {
            Dictionary<string, string> commands = [];
            using (StreamReader reader = new("Assets\\Commands\\Commands.txt"))
            {
                string curr = "";
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line == "")
                    {
                        continue;
                    }

                    if (line.StartsWith('!') || line.StartsWith('.'))
                    {
                        string[] parts = line.Split("\t\t");
                        commands[curr] += $"`{parts[0]}` {parts[1]}\n";
                    }
                    else if (line.StartsWith('*'))
                    {
                        commands[curr] += $"{line}\n";
                    }
                    else
                    {
                        commands.Add(line, "");
                        curr = line;
                    }
                }
            };
            return commands;
        }
    }
}
