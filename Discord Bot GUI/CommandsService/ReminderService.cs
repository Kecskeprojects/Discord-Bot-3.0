using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.CommandsService
{
    public class ReminderService
    {

        public static List<string> GetAmountsList(string amountstring)
        {
            //Split amounts into a string list, accounting for accidental spaces
            List<string> amounts = amountstring.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

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

        public static bool TryAddValuesToDate(List<string> amounts, out DateTime modifiedDate)
        {
            DateTime temp = DateTime.UtcNow; 
            modifiedDate = new DateTime(temp.Year, temp.Month, temp.Day, temp.Hour, temp.Minute, 0, temp.Kind);
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
}
