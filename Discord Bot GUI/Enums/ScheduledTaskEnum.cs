namespace Discord_Bot.Enums;

//What App's one-minute tick runs at set times; the reconnect, reminders and unmutes run on every tick. Times are UTC
[Flags]
public enum ScheduledTaskEnum
{
    None = 0,
    //Midnight
    ClearWindowLog = 1,
    //06:00
    BirthdaysAndWeeklyPolls = 2,
    //06:00 on Mondays
    BiasScraping = 4,
    //08:00, after YouTube's quota reset at midnight Pacific time
    YoutubeKeyRefill = 8
}
