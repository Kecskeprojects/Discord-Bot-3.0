using Discord;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Tests.Common.Builders;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors;

//!wotd's embed: the word and an example sentence side by side, each with its audio link and translation
public sealed class WordOfTheDayEmbedProcessorTests
{
    [Fact]
    public void CreateEmbed_GivesTheWordAndTheExampleWithAudioAndTranslation()
    {
        Embed embed = Assert.Single(WordOfTheDayEmbedProcessor.CreateEmbed(ServiceModels.WordOfTheDay("Korean")));

        Assert.Equal("Korean word of the day:", embed.Title);
        Assert.Equal(["Word:", "Example:"], embed.Fields.Select(field => field.Name));
        Assert.Equal("사랑 ([Audio](https://example.com/word.mp3))\n\n**Translation:**\nlove", embed.Fields[0].Value);
        Assert.Equal("사랑해요 ([Audio](https://example.com/phrase.mp3))\n\n**Translation:**\nI love you", embed.Fields[1].Value);
        Assert.All(embed.Fields, field => Assert.True(field.Inline));
    }
}
