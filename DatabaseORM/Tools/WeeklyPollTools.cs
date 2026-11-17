using DatabaseORM.Model;
using System.Globalization;

namespace DatabaseORM.Tools;

internal static class WeeklyPollTools
{
    public static List<string> GetAnswerOptions(WeeklyPoll poll)
    {
        ICollection<WeeklyPollOption> tempList;
        if (poll.OptionPreset != null)
        {
            if (poll.OptionPreset.IsSpecialPreset)
            {
                return GetSpecialAnswers(poll.OptionPreset.Name);
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

    private static List<string> GetSpecialAnswers(string name)
    {
        return name switch
        {
            "Days Of Week" => GetNext7Days("en-US"),
            "Days Of Week (HUN)" => GetNext7Days("hu-HU"),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Special answer values do not exist!")
        };
    }

    private static List<string> GetNext7Days(string cultureCode)
    {
        List<string> days = [];
        for (int i = 0; i < 7; i++)
        {
            days.Add(DateTime.UtcNow.AddDays(i).ToString("MMMM dd. (dddd)", CultureInfo.GetCultureInfo(cultureCode)));
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
