namespace Discord_Bot.Tools;

public static class NumberTools
{
    public static bool IsNullOrZero(int? value)
    {
        return value == null || value == 0;
    }

    public static bool IsNullOrZero(ulong? value)
    {
        return value == null || value == 0;
    }
}
