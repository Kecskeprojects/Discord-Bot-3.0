using System.Globalization;

namespace Discord_Bot.Tools.NativeTools;

public static class DateTimeTools
{
    public static string CurrentTime(bool utc = false)
    {
        DateTime curr = utc ? DateTime.UtcNow : DateTime.Now;
        return curr.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public static string CurrentDate(bool utc = false)
    {
        DateTime curr = utc ? DateTime.UtcNow : DateTime.Now;
        return curr.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    //The seconds of the date are dropped, the scheduler checks once a minute
    public static bool TryAddValuesToDate(DateTime date, List<string> amounts, out DateTime modifiedDate)
    {
        modifiedDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, date.Kind);
        //Go through the list 2 elements at a time
        for (int i = 0; i < amounts.Count; i += 2)
        {
            //The first element is the number, the second is the type, meaning day, month, year...
            int amount = int.Parse(amounts[i]);
            string type = amounts[i + 1];

            //Add the appropriate amount of time
            modifiedDate = type switch
            {
                "year" or "years" or "yr" or "y" => modifiedDate.AddYears(amount),
                "month" or "months" or "mon" or "M" => modifiedDate.AddMonths(amount),
                "week" or "weeks" or "w" => modifiedDate.AddDays(amount * 7),
                "day" or "days" or "d" => modifiedDate.AddDays(amount),
                "hour" or "hours" or "hr" or "h" => modifiedDate.AddHours(amount),
                "minute" or "minutes" or "min" or "m" => modifiedDate.AddMinutes(amount),
                _ => DateTime.MinValue
            };

            if (modifiedDate == DateTime.MinValue)
            {
                return false;
            }
        }
        return true;
    }
}
