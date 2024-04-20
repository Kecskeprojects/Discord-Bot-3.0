using System;

namespace Discord_Bot.Tools
{
    public static class TimeSpanTools
    {
        public static string ToTimeString(this TimeSpan timespan)
        {
            return (timespan.Hours != 0 ? $"{timespan.Hours}:" : null) + $"{timespan.Minutes}:{timespan.Seconds}";
        }
    }
}
