using DatabaseORM.Model;
using System.Globalization;

namespace DatabaseORM.Tools;

internal static class WeeklyPollTools
{
    //today is only used by the special presets, which list the days from today on
    public static List<string> GetAnswerOptions(WeeklyPoll poll, DateTime today)
    {
        ICollection<WeeklyPollOption> tempList;
        if (poll.OptionPreset != null)
        {
            if (poll.OptionPreset.IsSpecialPreset)
            {
                return GetSpecialAnswers(poll.OptionPreset.Name, today);
            }
            tempList = poll.OptionPreset.WeeklyPollOptions;
        }
        else
        {
            tempList = poll.WeeklyPollOptions;
        }

        return tempList
            .OrderBy(x => x.OrderNumber)
            .Select(x => x.Title)
            .ToList();
    }

    private static List<string> GetSpecialAnswers(string name, DateTime today)
    {
        return name switch
        {
            "Days Of Week" => GetNext7Days(today, "en-US"),
            "Days Of Week (HUN)" => GetNext7Days(today, "hu-HU"),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Special answer values do not exist!")
        };
    }

    private static List<string> GetNext7Days(DateTime today, string cultureCode)
    {
        List<string> days = [];
        for (int i = 0; i < 7; i++)
        {
            days.Add(today.AddDays(i).ToString("MMMM dd. (dddd)", CultureInfo.GetCultureInfo(cultureCode)));
        }

        if (cultureCode == "hu-HU")
        {
            days.Add("Egyik sem");
        }
        else
        {
            days.Add("None of the above");
        }

        return days;
    }
}
