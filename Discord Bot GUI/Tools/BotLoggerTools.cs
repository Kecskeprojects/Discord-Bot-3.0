using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Tools.NativeTools;
using System;

namespace Discord_Bot.Tools;
public static class BotLoggerTools
{
    public static Log BaseLog(LogTypeEnum type)
    {
        return new(DateTime.Now, type, $"[{DateTimeTools.CurrentTime()}][{type.ToFriendlyString()}]:\t");
    }

    public static string PutTabsOnNewLines(string message)
    {
        return message.Replace("\n", "\n\t\t\t");
    }
}
