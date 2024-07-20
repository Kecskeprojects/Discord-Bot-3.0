using Discord;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors.Help;

public static class HelpOwnerEmbedProcessor
{
    public static Embed[] CreateEmbed(string imageUrl)
    {
        EmbedBuilder builder = new();
        Dictionary<string, string> commands = ReadCommandsFile();
        builder.WithTitle("Owner Commands");

        int i = 1;
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
        string[] lines = Properties.Resource.Owner_Commands.Split("\r\n");
        foreach (string line in lines)
        {
            if (line.StartsWith('!') || line.StartsWith('.'))
            {
                string[] parts = line.Split("\t\t");
                commands.Add(parts[0], parts[1]);
            }
        }
        return commands;
    }
}
