using Discord;
using Discord_Bot.Services.Models.Wotd;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class WordOfTheDayEmbedProcessor
{
    public static Embed[] CreateEmbed(WotdBase result)
    {
        EmbedBuilder embed = new();

        _ = embed.WithTitle($"{result.Words.Langname} word of the day:");

        _ = embed.AddField("Word:", $"{result.Words.Word} ([Audio]({result.Words.Wordsound}))\n\n**Translation:**\n{result.Words.Translation}", true);
        _ = embed.AddField("Example:", $"{result.Words.Fnphrase} ([Audio]({result.Words.Phrasesound}))\n\n**Translation:**\n{result.Words.Enphrase}", true);

        _ = embed.WithColor(Color.Gold);
        _ = embed.WithCurrentTimestamp();
        return [embed.Build()];
    }
}
