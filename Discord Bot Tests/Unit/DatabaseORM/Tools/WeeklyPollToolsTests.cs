using DatabaseORM.Model;
using DatabaseORM.Tools;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Tools;

public sealed class WeeklyPollToolsTests
{
    //A Wednesday
    private static readonly DateTime today = TestTimes.Now.UtcDateTime;

    [Fact]
    public void GetAnswerOptions_DaysOfWeek_ListsTheNextSevenDaysAndNoneOfTheAbove()
    {
        List<string> options = WeeklyPollTools.GetAnswerOptions(SpecialPoll("Days Of Week"), today);

        string[] expected =
        [
            "November 18. (Wednesday)", "November 19. (Thursday)", "November 20. (Friday)", "November 21. (Saturday)",
            "November 22. (Sunday)", "November 23. (Monday)", "November 24. (Tuesday)", "None of the above"
        ];
        Assert.Equal(expected, options);
    }

    [Fact]
    public void GetAnswerOptions_DaysOfWeekInHungarian_UsesHungarianNames()
    {
        List<string> options = WeeklyPollTools.GetAnswerOptions(SpecialPoll("Days Of Week (HUN)"), today);

        Assert.Equal(8, options.Count);
        Assert.Equal("november 18. (szerda)", options[0]);
        Assert.Equal("Egyik sem", options[^1]);
    }

    [Fact]
    public void GetAnswerOptions_OwnOptions_AreInTheirOrder()
    {
        WeeklyPoll poll = new() { WeeklyPollOptions = [Option(2, "Second"), Option(1, "First"), Option(3, "Third")] };

        string[] expected = ["First", "Second", "Third"];
        Assert.Equal(expected, WeeklyPollTools.GetAnswerOptions(poll, today));
    }

    [Fact]
    public void GetAnswerOptions_UnknownSpecialPreset_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WeeklyPollTools.GetAnswerOptions(SpecialPoll("Moon Phases"), today));
    }

    private static WeeklyPoll SpecialPoll(string presetName)
    {
        return new WeeklyPoll { OptionPreset = new WeeklyPollOptionPreset { Name = presetName, IsSpecialPreset = true } };
    }

    private static WeeklyPollOption Option(byte orderNumber, string title)
    {
        return new WeeklyPollOption { OrderNumber = orderNumber, Title = title };
    }
}
