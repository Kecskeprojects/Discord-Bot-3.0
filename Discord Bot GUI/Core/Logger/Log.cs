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

    public class LogType
    {
        private LogType(string value) => Value = value;

        public string Value { get; private set; }

        public static LogType Log => new("LOG");
        public static LogType Query => new("QUERY");
        public static LogType Client => new("CLIENT");
        public static LogType Mes_User => new("MES_USER");
        public static LogType Mes_Other => new("MES_OTHER");
        public static LogType Error => new("ERROR");
        public static LogType Warning => new("WARNING");
    }
}
