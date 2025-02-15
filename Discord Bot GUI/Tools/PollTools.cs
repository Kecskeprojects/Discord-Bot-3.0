using Discord;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            _ => throw new Exception("Special answer values do not exist!")
        };
    }

    private static List<string> GetNext7Days(string cultureCode)
    {
        List<string> days = [];
        for (int i = 1; i < 8; i++)
        {
            days.Add(DateTime.UtcNow.AddDays(i).ToString("MMMM dd. (dddd)", CultureInfo.GetCultureInfo(cultureCode)));
        }
        return days;
    }

    public static PollCloseInEnum GetEnumFromTicks(long ticks)
    {
        TimeSpan span = new(ticks);
        return (int)span.TotalHours switch
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
