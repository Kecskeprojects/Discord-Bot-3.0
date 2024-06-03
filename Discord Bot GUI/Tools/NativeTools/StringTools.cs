using System;
using System.Collections.Generic;

namespace Discord_Bot.Tools.NativeTools;

public static class StringTools
{
    public static string AddNumberPositionIdentifier(string position)
    {
        if (string.IsNullOrEmpty(position))
        {
            return null;
        }

        if (position.Length >= 2 && position[^2..^1] is "11" or "12" or "13")
        {
            return $"{position}th";
        }

        return position[^1] switch
        {
            '1' => $"{position}st",
            '2' => $"{position}nd",
            '3' => $"{position}rd",
            _ => $"{position}th"
        };
    }

    public static List<string> GetTimeMeasurements(string amountstring)
    {
        //Split amounts into a string list, accounting for accidental spaces
        List<string> amounts = [.. amountstring.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

        //Go through every element in the list and check
        //if the user missed a space between the number and the corresponding type
        for (int i = 0; i < amounts.Count; i++)
        {
            //Don't check if the element is a number in itself
            if (!int.TryParse(amounts[i], out _))
            {
                char[] chars = amounts[i].ToCharArray();
                int lastValid = -1;

                //Go through the array of chars one by one
                //until a non number is found, in which case we exit the loop
                for (int j = 0; j < chars.Length; j++)
                {
                    if (char.IsDigit(chars[j]))
                    {
                        lastValid = j;
                    }
                    else
                    {
                        break;
                    }
                }

                //If any numbers were found in the array,
                //we rewrite original string and put the number part of it into the list
                if (lastValid >= 0)
                {
                    amounts[i] = amounts[i][(lastValid + 1)..];
                    amounts.Insert(i, new string(chars, 0, lastValid + 1));
                }
            }
        }

        return amounts;
    }
}
