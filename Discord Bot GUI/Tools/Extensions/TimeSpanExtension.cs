using System;

namespace Discord_Bot.Tools.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToTimeString(this TimeSpan timespan)
        {
            return (timespan.Hours != 0 ? $"{timespan.Hours}:" : null) + $"{timespan.Minutes:00}:{timespan.Seconds:00}";
        }
    }
}
