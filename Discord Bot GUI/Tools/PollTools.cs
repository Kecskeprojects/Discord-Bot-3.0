using Discord;
using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Tools;

public static class PollTools
{
    public static PollMediaProperties CreateTitle(string text)
    {
        return new PollMediaProperties()
        {
            Text = text
        };
    }

    public static uint GetDuration(long closeInTimeSpanTicks)
    {
        return (uint) TimeSpan.FromTicks(closeInTimeSpanTicks).TotalHours;
    }

    public static PollCloseInEnum GetEnumFromTicks(long ticks)
    {
        TimeSpan span = new(ticks);
        return (int) span.TotalHours switch
        {
            1 => PollCloseInEnum.OneHour,
            4 => PollCloseInEnum.FourHour,
            8 => PollCloseInEnum.EightHour,
            24 => PollCloseInEnum.OneDay,
            72 => PollCloseInEnum.ThreeDay,
            168 => PollCloseInEnum.OneWeek,
            _ => PollCloseInEnum.OneWeek
        };
    }
}
