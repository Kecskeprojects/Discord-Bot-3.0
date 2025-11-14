using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class GreetingListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<GreetingResource> greetings)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle("Greetings:");

        foreach (GreetingResource greeting in greetings)
        {
            _ = builder.AddField($"ID:{greeting.GreetingId}", greeting.Url);
        }

        return [builder.Build()];
    }
}
