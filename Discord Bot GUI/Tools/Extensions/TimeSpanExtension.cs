using System;

namespace Discord_Bot.Tools.Extensions;
public static class TimeSpanExtension
{
    public static string ToTimeString(this TimeSpan timespan)
    {
        return (timespan.Hours != 0 ? $"{timespan.Hours}h:" : null) + $"{timespan.Minutes:00}m:{timespan.Seconds:00}s";
    }
}
