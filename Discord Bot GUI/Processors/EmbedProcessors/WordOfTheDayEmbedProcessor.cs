using Discord;
using Discord_Bot.Services.Models.Wotd;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public class WordOfTheDayEmbedProcessor
    {
        public static Embed[] CreateEmbed(WotdBase result)
        {
            EmbedBuilder embed = new();

            embed.WithTitle($"{result.Words.Langname} word of the day:");

            embed.AddField("Word:", $"{result.Words.Word} ([Audio]({result.Words.Wordsound}))\n\n**Translation:**\n{result.Words.Translation}", true);
            embed.AddField("Example:", $"{result.Words.Fnphrase} ([Audio]({result.Words.Phrasesound}))\n\n**Translation:**\n{result.Words.Enphrase}", true);

            embed.WithColor(Color.Gold);
            embed.WithCurrentTimestamp();
            return [embed.Build()];
        }
    }
}
