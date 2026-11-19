using DatabaseORM.Enums;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.BiasGame;

//!bias stats; both callers send it only when there is at least one statistic
public sealed class BiasGameStatEmbedProcessorTests
{
    [Fact]
    public void CreateEmbed_ListsTheIdolsInOrderWithAHeartForBiases()
    {
        UserBiasGameStatResource stats = new()
        {
            BiasGameCount = 3,
            Stats =
            [
                new() { IdolStageName = "First", IdolGroupFullName = "Group*Star", IsUserBias = true, LatestImageUrl = "https://example.com/first.png" },
                new() { IdolStageName = "Second", IdolGroupFullName = null }
            ]
        };

        Embed embed = Assert.Single(BiasGameStatEmbedProcessor.CreateEmbed(TestValues.Username, GenderEnum.Female, stats));

        Assert.Equal($"Bias Game Stats for {TestValues.Username}", embed.Title);
        Assert.Equal("https://example.com/first.png", embed.Thumbnail?.Url);
        Assert.Equal("Gender: F", embed.Footer?.Text);
        EmbedField favorites = Assert.Single(embed.Fields);
        Assert.Equal("Your Favorites from *3 games*", favorites.Name);
        //A * in a group name would start Discord's italics, so it is escaped
        Assert.Equal("#1 First - Group\\*Star :heart:\n#2 Second - Soloist\n", favorites.Value);
    }

    //BiasGameStatInteraction reads both genders back from the button
    [Fact]
    public void CreateComponent_ButtonsCarryTheChosenAndTheCurrentGender()
    {
        List<ButtonComponent> buttons = ComponentParts.Buttons(BiasGameStatEmbedProcessor.CreateComponent(GenderEnum.NotSpecified, DiscordIds.User));

        Assert.Equal(
            [$"BiasStats_Gender_1_NotSpecified_{DiscordIds.User}", $"BiasStats_Gender_2_NotSpecified_{DiscordIds.User}", $"BiasStats_Gender_3_NotSpecified_{DiscordIds.User}"],
            buttons.Select(button => button.CustomId));
        Assert.Equal(
            [GenderEnum.Male, GenderEnum.Female, GenderEnum.NotSpecified],
            buttons.Select(button => (GenderEnum) int.Parse(button.CustomId.Split('_')[2])));
    }
}
