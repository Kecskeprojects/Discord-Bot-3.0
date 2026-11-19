using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.Polls;

//A preset's edit message: WeeklyPollOptionPresetEditInteraction and WeeklyPollOptionEditInteraction read the custom IDs and values back
public sealed class PollPresetEditEmbedProcessorTests
{
    private const int PresetId = 4;

    [Fact]
    public void CreateComponent_ButtonsShowTheCurrentStateAndCarryIt()
    {
        List<ButtonComponent> buttons = ComponentParts.Buttons(PollPresetEditEmbedProcessor.CreateComponent(Preset(isSpecial: false)));

        Assert.Equal(["Edit Preset", "Normal Options", "Active"], buttons.Select(button => button.Label));
        Assert.Equal(
            [$"EditPollPreset_{PresetId}", $"PollPreset_Change_IsSpecialPreset_{PresetId}_False", $"PollPreset_Change_IsActive_{PresetId}_True"],
            buttons.Select(button => button.CustomId));
    }

    [Fact]
    public void CreateComponent_AnswersMenu_GivesEachSlotAndItsAnswersId()
    {
        SelectMenuComponent answers = Assert.Single(ComponentParts.SelectMenus(PollPresetEditEmbedProcessor.CreateComponent(Preset(isSpecial: false))));

        Assert.Equal($"PollOption_Change_CustomOption_True_{PresetId}", answers.CustomId);
        Assert.Equal(["1# Yes", "2#"], answers.Options.Take(2).Select(option => option.Label));
        //The answer's own ID, not the preset's (it gave the preset's until 2026-11-19, which opened the wrong answer)
        Assert.Equal(["0_17", "1_0"], answers.Options.Take(2).Select(option => option.Value));
    }

    //A special preset's answers are made by the bot, such as the coming week's days
    [Fact]
    public void CreateComponent_SpecialPreset_HasNoAnswersToEdit()
    {
        Assert.Empty(ComponentParts.SelectMenus(PollPresetEditEmbedProcessor.CreateComponent(Preset(isSpecial: true))));
    }

    [Theory]
    [InlineData("Yes or No", true, "Edit 'Yes or No' Preset")]
    [InlineData("", true, "Create new Weekly Poll Option Preset")]
    public void CreateEmbed_TitleSaysWhetherThePresetIsNew(string name, bool isEdit, string expected)
    {
        Assert.Equal(expected, Assert.Single(PollPresetEditEmbedProcessor.CreateEmbed(new WeeklyPollOptionPresetResource { Name = name }, isEdit)).Title);
    }

    private static WeeklyPollOptionPresetResource Preset(bool isSpecial)
    {
        return new WeeklyPollOptionPresetResource
        {
            WeeklyPollOptionPresetId = PresetId,
            Name = "Yes or No",
            IsActive = true,
            IsSpecialPreset = isSpecial,
            Options = [new() { WeeklyPollOptionPresetId = PresetId, WeeklyPollOptionId = 17, OrderNumber = 0, Title = "Yes" }]
        };
    }
}
