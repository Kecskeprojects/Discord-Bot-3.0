using System;

namespace Discord_Bot.Tools
{
    public static class DateTimeTools
    {
        public static string CurrentTime(bool utc = false)
        {
            DateTime curr = utc ? DateTime.UtcNow : DateTime.Now;

            string hour = curr.Hour < 10 ? "0" + curr.Hour.ToString() : curr.Hour.ToString();
            string minute = curr.Minute < 10 ? "0" + curr.Minute.ToString() : curr.Minute.ToString();
            string second = curr.Second < 10 ? "0" + curr.Second.ToString() : curr.Second.ToString();

            return $"{hour}:{minute}:{second}";
        }

        public static string CurrentDate(bool utc = false)
        {
            DateTime curr = utc ? DateTime.UtcNow : DateTime.Now;

            string year = curr.Year.ToString();
            string month = curr.Month < 10 ? "0" + curr.Month.ToString() : curr.Month.ToString();
            string day = curr.Day < 10 ? "0" + curr.Day.ToString() : curr.Day.ToString();

            return $"{year}-{month}-{day}";
        }
    }
}
