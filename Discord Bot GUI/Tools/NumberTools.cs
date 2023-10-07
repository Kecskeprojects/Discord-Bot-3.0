namespace Discord_Bot.Tools
{
    public class NumberTools
    {
        public static bool IsNullOrZero(int? value) => value == null || value == 0;
        public static bool IsNullOrZero(ulong? value) => value == null || value == 0;
    }
}
