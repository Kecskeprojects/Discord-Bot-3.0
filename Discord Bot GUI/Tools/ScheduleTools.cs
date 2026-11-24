using Discord_Bot.Enums;

namespace Discord_Bot.Tools;

public static class ScheduleTools
{
    //The tasks due in the minute of utcNow. The tick comes once a minute, so each is due once a day, the scraping once a week
    public static ScheduledTaskEnum DueAt(DateTime utcNow)
    {
        ScheduledTaskEnum due = ScheduledTaskEnum.None;
        if (utcNow is { Hour: 0, Minute: 0 })
        {
            due |= ScheduledTaskEnum.ClearWindowLog;
        }

        if (utcNow is { Hour: 6, Minute: 0 })
        {
            due |= ScheduledTaskEnum.BirthdaysAndWeeklyPolls;
            if (utcNow.DayOfWeek == DayOfWeek.Monday)
            {
                due |= ScheduledTaskEnum.BiasScraping;
            }
        }

        if (utcNow is { Hour: 8, Minute: 0 })
        {
            due |= ScheduledTaskEnum.YoutubeKeyRefill;
        }

        return due;
    }
}
