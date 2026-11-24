using System.Globalization;
using Discord_Bot.Enums;
using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Tools;

//What App's one-minute tick runs at set times (UTC); 2026-11-23 is a Monday
public sealed class ScheduleToolsTests
{
    [Theory]
    [InlineData("2026-11-24 00:00:12", ScheduledTaskEnum.ClearWindowLog)]
    [InlineData("2026-11-24 06:00:00", ScheduledTaskEnum.BirthdaysAndWeeklyPolls)]
    [InlineData("2026-11-23 06:00:59", ScheduledTaskEnum.BirthdaysAndWeeklyPolls | ScheduledTaskEnum.BiasScraping)]
    [InlineData("2026-11-24 08:00:30", ScheduledTaskEnum.YoutubeKeyRefill)]
    public void DueAt_SetMinute_GivesItsTasks(string utcNow, ScheduledTaskEnum due)
    {
        Assert.Equal(due, ScheduleTools.DueAt(Utc(utcNow)));
    }

    [Theory]
    [InlineData("2026-11-24 23:59:59")]
    [InlineData("2026-11-24 00:01:00")]
    [InlineData("2026-11-23 05:59:59")]
    [InlineData("2026-11-23 06:01:00")]
    [InlineData("2026-11-24 07:00:00")]
    [InlineData("2026-11-24 12:00:00")]
    public void DueAt_OtherMinutes_GivesNothing(string utcNow)
    {
        Assert.Equal(ScheduledTaskEnum.None, ScheduleTools.DueAt(Utc(utcNow)));
    }

    //Each comes up once in a day of ticks, the scraping once in a week
    [Fact]
    public void DueAt_AWeekOfMinutes_GivesEachTaskItsCount()
    {
        DateTime monday = Utc("2026-11-23 00:00:00");
        List<ScheduledTaskEnum> week = [.. Enumerable.Range(0, 7 * 24 * 60).Select(minute => ScheduleTools.DueAt(monday.AddMinutes(minute)))];

        Assert.Equal(7, week.Count(due => due.HasFlag(ScheduledTaskEnum.ClearWindowLog)));
        Assert.Equal(7, week.Count(due => due.HasFlag(ScheduledTaskEnum.BirthdaysAndWeeklyPolls)));
        Assert.Equal(7, week.Count(due => due.HasFlag(ScheduledTaskEnum.YoutubeKeyRefill)));
        Assert.Equal(1, week.Count(due => due.HasFlag(ScheduledTaskEnum.BiasScraping)));
    }

    private static DateTime Utc(string time)
    {
        return DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
