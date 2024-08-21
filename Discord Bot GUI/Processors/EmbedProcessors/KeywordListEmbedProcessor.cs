using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors;
internal class KeywordListEmbedProcessor
{
    public static Embed[] CreateEmbed(string serverName, List<KeywordResource> keywords)
    {
        EmbedBuilder builder = new();
        builder.WithTitle($"{serverName} trigger words/sentences:");

        foreach (KeywordResource keyword in keywords)
        {
            builder.AddField(keyword.Trigger, keyword.Response);
        }

        builder.WithColor(Color.DarkMagenta);
        builder.WithCurrentTimestamp();
        return [builder.Build()];
    }
}
