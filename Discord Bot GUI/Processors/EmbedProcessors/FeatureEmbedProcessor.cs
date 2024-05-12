using Discord;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public class FeatureEmbedProcessor
    {
        public static Embed[] CreateEmbed(string imageUrl)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Passive processes");

            int i = 1;
            Dictionary<string, string> commands = ReadFeaturesFile();
            foreach (KeyValuePair<string, string> item in commands)
            {
                builder.AddField(item.Key, item.Value, false);
                i++;
            }
            builder.WithThumbnailUrl(imageUrl);
            builder.WithColor(Color.Orange);
            return [builder.Build()];
        }

        public static Dictionary<string, string> ReadFeaturesFile()
        {
            Dictionary<string, string> commands = [];
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
            return commands;
        }
    }
}
