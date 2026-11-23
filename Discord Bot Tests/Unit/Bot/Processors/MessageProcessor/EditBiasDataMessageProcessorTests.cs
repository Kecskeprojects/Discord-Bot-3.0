using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Enums;
using Discord_Bot.Processors.MessageProcessor;

namespace Discord_Bot.Tests.Unit.Bot.Processors.MessageProcessor;

//!bias edit's menu; BiasEditComponentInteraction reads each choice back with BiasEditData
public sealed class EditBiasDataMessageProcessorTests
{
    [Fact]
    public void CreateComponent_OffersEveryActionForTheIdol()
    {
        MessageComponent component = EditBiasDataMessageProcessor.CreateComponent("jisoo", "blackpink");

        SelectMenuComponent menu = Assert.IsType<SelectMenuComponent>(Assert.Single(Assert.IsType<ActionRowComponent>(Assert.Single(component.Components)).Components));
        Assert.Equal("EditIdolData", menu.CustomId);
        List<BiasEditData> choices = [.. menu.Options.Select(option => new BiasEditData(option.Value))];
        Assert.Equal(Enum.GetValues<BiasEditActionTypeEnum>(), choices.Select(choice => choice.Action));
        Assert.All(choices, choice => Assert.Equal(("jisoo", "blackpink"), (choice.BiasName, choice.BiasGroupName)));
    }
}
