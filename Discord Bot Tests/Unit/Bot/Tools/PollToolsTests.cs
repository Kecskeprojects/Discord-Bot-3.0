using Discord_Bot.Enums;
using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Tools;

public sealed class PollToolsTests
{
    [Fact]
    public void CreateTitle_IsTheText()
    {
        Assert.Equal("Weekly question", PollTools.CreateTitle("Weekly question").Text);
    }

    //Discord takes a poll's duration in whole hours
    [Theory]
    [InlineData(1, 1)]
    [InlineData(24, 24)]
    [InlineData(168, 168)]
    public void GetDuration_GivesTheHours(int hours, uint expected)
    {
        Assert.Equal(expected, PollTools.GetDuration(TimeSpan.FromHours(hours).Ticks));
    }

    [Theory]
    [InlineData(PollCloseInEnum.OneHour)]
    [InlineData(PollCloseInEnum.FourHour)]
    [InlineData(PollCloseInEnum.EightHour)]
    [InlineData(PollCloseInEnum.OneDay)]
    [InlineData(PollCloseInEnum.ThreeDay)]
    [InlineData(PollCloseInEnum.OneWeek)]
    public void GetEnumFromTicks_ReadsBackEveryChoice(PollCloseInEnum choice)
    {
        Assert.Equal(choice, PollTools.GetEnumFromTicks(choice.ConvertToTimeSpanTicks()));
    }

    //A new poll's draft starts with long.MinValue ticks (WeeklyPollService.GetOrCreateDummyPollAsync)
    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(0)]
    public void GetEnumFromTicks_AnythingElse_IsAWeek(long ticks)
    {
        Assert.Equal(PollCloseInEnum.OneWeek, PollTools.GetEnumFromTicks(ticks));
    }
}
