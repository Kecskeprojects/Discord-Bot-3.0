using DatabaseORM.Enums;
using Discord;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.BiasGame;

//The first step of a new game
public sealed class BiasGameGenderEmbedProcessorTests
{
    [Fact]
    public void CreateEmbed_NamesTheTwoSteps()
    {
        Embed embed = Assert.Single(BiasGameGenderEmbedProcessor.CreateEmbed(TestValues.Username, TestValues.AvatarUrl));

        Assert.Equal("Bias Game Setup", embed.Title);
        Assert.Equal(TestValues.Username, embed.Footer?.Text);
        Assert.Equal(["1. Select a gender", "2. Select a debut range"], embed.Fields.Select(field => field.Name));
    }

    //BiasGameInteraction reads the number back as a GenderEnum
    [Fact]
    public void CreateComponent_ManWomanAndBothButtons_CarryTheGendersValues()
    {
        List<ButtonComponent> buttons = ComponentParts.Buttons(BiasGameGenderEmbedProcessor.CreateComponent(DiscordIds.User));

        Assert.Equal(
            [GenderEnum.Male, GenderEnum.Female, GenderEnum.NotSpecified],
            buttons.Select(button => (GenderEnum) int.Parse(button.CustomId.Split('_')[3])));
        Assert.All(buttons, button => Assert.EndsWith($"_{DiscordIds.User}", button.CustomId, StringComparison.Ordinal));
    }
}
