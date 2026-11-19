using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Enums;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.Polls;

//The poll's edit message: WeeklyPollEditInteraction and WeeklyPollOptionEditInteraction read the custom IDs and values back
public sealed class PollEditEmbedProcessorTests
{
    private const int PollId = 21;

    [Theory]
    [InlineData("weekly", true, "Edit 'weekly' Poll")]
    [InlineData("", true, "Create new Weekly Poll")]
    [InlineData("weekly", false, "Create new Weekly Poll")]
    public void CreateEmbed_TitleSaysWhetherThePollIsNew(string name, bool isEdit, string expected)
    {
        Assert.Equal(expected, Assert.Single(PollEditEmbedProcessor.CreateEmbed(new WeeklyPollEditResource { Name = name }, isEdit)).Title);
    }

    [Fact]
    public void CreateComponent_ButtonsShowTheCurrentStateAndCarryIt()
    {
        WeeklyPollEditResource poll = Poll();
        poll.IsMultipleAnswer = true;
        poll.IsActive = false;

        List<ButtonComponent> buttons = ComponentParts.Buttons(PollEditEmbedProcessor.CreateComponent(poll, []));

        Assert.Equal(["Edit Poll", "Multi Answer", "Inactive", "Don't Pin"], buttons.Select(button => button.Label));
        Assert.Equal(
            [$"EditPoll_{PollId}", $"Poll_Change_IsMultipleAnswer_{PollId}_True", $"Poll_Change_IsActive_{PollId}_False", $"Poll_Change_IsPinned_{PollId}_False"],
            buttons.Select(button => button.CustomId));
        Assert.Equal(ButtonStyle.Danger, buttons[2].Style);
    }

    [Fact]
    public void CreateComponent_MenusPreselectTheCurrentChoices()
    {
        WeeklyPollEditResource poll = Poll();
        poll.CloseInTimeSpanTicks = PollCloseInEnum.OneDay.ConvertToTimeSpanTicks();
        poll.RepeatOnDayOfWeek = nameof(DayOfWeek.Friday);
        poll.OptionPresetId = 4;

        List<SelectMenuComponent> menus = ComponentParts.SelectMenus(PollEditEmbedProcessor.CreateComponent(poll, [Preset(4, "Yes or No"), Preset(5, "Rating")]));

        Assert.Equal("24 Hours", Chosen(menus, "Poll_SelectChange_CloseInTimeSpanTicks_").Label);
        Assert.Equal("Friday", Chosen(menus, "Poll_SelectChange_RepeatOnDayOfWeek_").Value);
        Assert.Equal("4", Chosen(menus, "Poll_SelectChange_OptionPresetId_").Value);
        //A poll with a preset has no answers of its own to edit
        Assert.DoesNotContain(menus, menu => menu.CustomId.StartsWith("PollOption_", StringComparison.Ordinal));
    }

    [Fact]
    public void CreateComponent_WithoutAPreset_ChoosesCustomAnswersAndListsTheTenSlots()
    {
        WeeklyPollEditResource poll = Poll();
        poll.Options = [new() { WeeklyPollId = PollId, WeeklyPollOptionId = 8, OrderNumber = 1, Title = "No" }, new() { WeeklyPollId = PollId, WeeklyPollOptionId = 7, OrderNumber = 0, Title = "Yes" }];

        List<SelectMenuComponent> menus = ComponentParts.SelectMenus(PollEditEmbedProcessor.CreateComponent(poll, []));

        Assert.Equal("custom", Chosen(menus, "Poll_SelectChange_OptionPresetId_").Value);
        SelectMenuComponent answers = menus.Single(menu => menu.CustomId.StartsWith("PollOption_", StringComparison.Ordinal));
        //False: the poll's own answers, not a preset's (it said True until 2026-11-19, so answers couldn't be added)
        Assert.Equal($"PollOption_Change_CustomOption_False_{PollId}", answers.CustomId);
        Assert.Equal(10, answers.Options.Count);
        Assert.Equal(["1# Yes", "2# No", "3#"], answers.Options.Take(3).Select(option => option.Label));
        //The value is the slot and the answer's ID, 0 for a slot without an answer yet
        Assert.Equal(["0_7", "1_8", "2_0"], answers.Options.Take(3).Select(option => option.Value));
    }

    private static WeeklyPollEditResource Poll()
    {
        return new WeeklyPollEditResource { WeeklyPollId = PollId, Name = "weekly", RepeatOnDayOfWeek = nameof(DayOfWeek.Monday) };
    }

    private static WeeklyPollOptionPresetResource Preset(int id, string name)
    {
        return new WeeklyPollOptionPresetResource { WeeklyPollOptionPresetId = id, Name = name, Description = name };
    }

    private static SelectMenuOption Chosen(List<SelectMenuComponent> menus, string customIdStart)
    {
        return menus.Single(menu => menu.CustomId.StartsWith(customIdStart, StringComparison.Ordinal)).Options.Single(option => option.IsDefault == true);
    }
}
