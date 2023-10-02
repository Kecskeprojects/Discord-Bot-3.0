namespace Discord_Bot.Core.Logger
{
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
