using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication
{
    public class Log(DateTime time, LogType type, string completeLog)
    {
        public DateTime TimeStamp { get; private set; } = time;

        public LogType Type { get; private set; } = type;

        public string Content { get; set; } = completeLog;
    }
}
