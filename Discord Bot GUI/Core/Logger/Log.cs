using System;

namespace Discord_Bot.Core.Logger
{
    public class Log
    {
        public DateTime TimeStamp { get; private set; }

        public LogType Type { get; private set; }

        public string Content { get; set; }

        public Log(DateTime time, LogType type, string completeLog)
        {
            TimeStamp = time;
            Type = type;
            Content = completeLog;
        }
    }
}
