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

    //The seconds of the date are dropped, the scheduler checks once a minute.
    //Returns false for anything it can't read, rather than throwing (until 2026-11-23 these threw: a unit before its number,
    //a number too big for an int, and an amount past the year 9999). Weeks are counted in days as a double, as an int could wrap around
    public static bool TryAddValuesToDate(DateTime date, List<string> amounts, out DateTime modifiedDate)
    {
        modifiedDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, date.Kind);
        //Go through the list 2 elements at a time
        for (int i = 0; i < amounts.Count; i += 2)
        {
            //The first element is the number, the second is the type, meaning day, month, year...
            if (!int.TryParse(amounts[i], out int amount))
            {
                return false;
            }
            string type = amounts[i + 1];

            //Add the appropriate amount of time
            try
            {
                modifiedDate = type switch
                {
                    "year" or "years" or "yr" or "y" => modifiedDate.AddYears(amount),
                    "month" or "months" or "mon" or "M" => modifiedDate.AddMonths(amount),
                    "week" or "weeks" or "w" => modifiedDate.AddDays(amount * 7.0),
                    "day" or "days" or "d" => modifiedDate.AddDays(amount),
                    "hour" or "hours" or "hr" or "h" => modifiedDate.AddHours(amount),
                    "minute" or "minutes" or "min" or "m" => modifiedDate.AddMinutes(amount),
                    _ => DateTime.MinValue
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            if (modifiedDate == DateTime.MinValue)
            {
                return false;
            }
        }
        return true;
    }
}
