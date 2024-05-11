using System;
using System.Text.RegularExpressions;

namespace Discord_Bot.Tools
{
    public static partial class StringExtension
    {
        public static string RemoveSpecialCharacters(this string str)
        {
            return NormalCharacterRegex().Replace(str, "");
        }

        [GeneratedRegex("[^a-zA-Z0-9\\s]+", RegexOptions.Compiled)]
        private static partial Regex NormalCharacterRegex();

        public static string ToMockText(this string str)
        {
            Random r = new();
            string newString = "";
            for (int i = 0; i < str.Length; i++)
            {
                newString += r.Next(0, 2) == 0 ? char.ToUpper(str[i]) : str[i];
            }

            str = newString;
            newString = "";
            for (int i = 0; i < str.Length; i++)
            {
                if(i > 1 && char.IsUpper(str[i]) && char.IsUpper(str[i - 1]) && char.IsUpper(newString[i - 1]))
                {
                    newString += char.ToLower(str[i]);
                    continue;
                }

                newString += str[i];
            }
            return newString;
        }
    }
}
