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
    }
}
