using DatabaseORM.Resources;
using Discord;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class GreetingListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<GreetingResource> greetings)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle("Greetings:");

        //Discord allows 25 fields in an embed
        foreach (GreetingResource greeting in greetings.Take(EmbedBuilder.MaxFieldCount))
        {
            _ = builder.AddField($"ID:{greeting.GreetingId}", greeting.Url);
        }

        if (greetings.Count > EmbedBuilder.MaxFieldCount)
        {
            _ = builder.WithFooter($"{greetings.Count - EmbedBuilder.MaxFieldCount} more not shown");
        }

        return [builder.Build()];
    }
}
