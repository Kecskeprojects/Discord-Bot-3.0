using System;

namespace Discord_Bot.Enums;

public enum PollCloseInEnum
{
    OneHour = 1,
    FourHour = 2,
    EightHour = 3,
    OneDay = 4,
    ThreeDay = 5,
    OneWeek = 6
}

public static class PollCloseInEnumExtended
{
    public static long ConvertToTimeSpanTicks(this PollCloseInEnum pollCloseInEnum)
    {
        return (pollCloseInEnum switch
        {
            PollCloseInEnum.OneHour => new TimeSpan(1, 0, 0),
            PollCloseInEnum.FourHour => new TimeSpan(4, 0, 0),
            PollCloseInEnum.EightHour => new TimeSpan(8, 0, 0),
            PollCloseInEnum.OneDay => new TimeSpan(1, 0, 0, 0),
            PollCloseInEnum.ThreeDay => new TimeSpan(3, 0, 0, 0),
            PollCloseInEnum.OneWeek => new TimeSpan(7, 0, 0, 0),
            _ => TimeSpan.Zero
        }).Ticks;
    }

    public static string ToFriendlyString(this PollCloseInEnum pollCloseInEnum)
    {
        return pollCloseInEnum switch
        {
            PollCloseInEnum.OneHour => "1 Hour",
            PollCloseInEnum.FourHour => "4 Hours",
            PollCloseInEnum.EightHour => "8 Hours",
            PollCloseInEnum.OneDay => "24 Hours",
            PollCloseInEnum.ThreeDay => "3 Days",
            PollCloseInEnum.OneWeek => "1 Week",
            _ => ""
        };
    }
}
