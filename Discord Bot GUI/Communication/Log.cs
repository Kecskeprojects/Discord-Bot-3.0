using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication;
public class Log(DateTime time, LogTypeEnum type, string completeLog)
{
    public DateTime TimeStamp { get; private set; } = time;
    public LogTypeEnum Type { get; private set; } = type;
    public string Content { get; set; } = completeLog;
}
