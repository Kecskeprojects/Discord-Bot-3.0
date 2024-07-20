using Discord;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors.Help;

public static class FeaturesEmbedProcessor
{
    public static Embed[] CreateEmbed(string imageUrl)
    {
        EmbedBuilder builder = new();
        builder.WithTitle("Passive processes");

        int i = 1;
        Dictionary<string, string> commands = ReadFeatures();
        foreach (KeyValuePair<string, string> item in commands)
        {
            builder.AddField(item.Key, item.Value, false);
            i++;
        }
        builder.WithThumbnailUrl(imageUrl);
        builder.WithColor(Color.Orange);
        return [builder.Build()];
    }

    public static Dictionary<string, string> ReadFeatures()
    {
        Dictionary<string, string> commands = [];
        string[] lines = Properties.Resource.Bot_Features.Split("\r\n");
        string curr = "";
        foreach (string line in lines)
        {
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
        return commands;
    }
}
