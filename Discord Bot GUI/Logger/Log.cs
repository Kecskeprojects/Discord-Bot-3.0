using System;

namespace Discord_Bot.Logger
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
        private LogType(string value) { Value = value; }

        public string Value { get; private set; }

        public static LogType Log { get { return new LogType("LOG"); } }
        public static LogType Query { get { return new LogType("QUERY"); } }
        public static LogType Client { get { return new LogType("CLIENT"); } }
        public static LogType Mes_User { get { return new LogType("MES_USER"); } }
        public static LogType Mes_Other { get { return new LogType("MES_OTHER"); } }
        public static LogType Error { get { return new LogType("ERROR"); } }
        public static LogType Warning { get { return new LogType("WARNING"); } }
    }
}
