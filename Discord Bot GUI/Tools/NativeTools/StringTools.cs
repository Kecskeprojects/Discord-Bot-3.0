using System.Collections.Generic;

namespace Discord_Bot.Tools.NativeTools;

public static class StringTools
{
    public static string AddNumberPositionIdentifier(string position)
    {
        return string.IsNullOrEmpty(position)
            ? null
            : position.Length >= 2 && position[^2..] is "11" or "12" or "13"
            ? $"{position}th"
            : position[^1] switch
            {
                '1' => $"{position}st",
                '2' => $"{position}nd",
                '3' => $"{position}rd",
                _ => $"{position}th"
            };
    }

    public static List<string> GetTimeMeasurements(string amountstring)
    {
        //Spaces are eliminated for a unified solution
        amountstring = amountstring.Replace(" ", "");
        List<string> amounts = [];

        //-1 conveniently solves the issue of the first Add logic, as that will come out to 0 in the logic
        int lastDigit = -1;
        int lastChar = -1;
        bool lastWasDigit = false;
        bool lastWasChar = false;
        for (int i = 0; i < amountstring.Length; i++)
        {
            if (char.IsNumber(amountstring[i]))
            {
                //Time indicatiors are added to the list here
                if (lastWasChar)
                {
                    amounts.Add(amountstring[(lastDigit + 1)..i]);
                }

                lastDigit = i;
                lastWasDigit = true;
                lastWasChar = false;
            }
            else
            {
                //Numbers are added to the list here
                //The first entry is expected to be a number
                if (lastWasDigit)
                {
                    amounts.Add(amountstring[(lastChar + 1)..i]);
                }

                lastChar = i;
                lastWasDigit = false;
                lastWasChar = true;
            }
        }
        //The last entry will not get added during the loop, so it is added afterwards
        //It is expected to be non-numeric
        amounts.Add(amountstring[(lastDigit + 1)..]);

        return amounts;
    }
}
