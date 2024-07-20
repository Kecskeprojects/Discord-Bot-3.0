namespace Discord_Bot.Tools.NativeTools;

public static class NumberTools
{
    public static bool IsNullOrZero(int? value)
    {
        return value is null or 0;
    }

    public static bool IsNullOrZero(ulong? value)
    {
        return value is null or 0;
    }
}
