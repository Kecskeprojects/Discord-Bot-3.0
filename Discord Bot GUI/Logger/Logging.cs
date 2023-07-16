using System;

namespace Discord_Bot.Logger
{
    public static class Logging
    {
        #region Bot logging
        public static void Log(string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Log);

            log.Content += message;
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(log.Content);
            }

            if (!ConsoleOnly)
            {
                Global.Logs.Add(log);
            }
        }

        public static void Query(string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Query);

            log.Content += message;
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(log.Content);
            }

            if (!ConsoleOnly)
            {
                Global.Logs.Add(log);
            }
        }

        public static void Client(string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Client);

            log.Content += message;
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(log.Content);
            }

            if (!ConsoleOnly)
            {
                Global.Logs.Add(log);
            }
        }
        #endregion

        #region Message logging
        public static void Mes_User(string message, string server = "DM")
        {
            Log log = BaseLog(LogType.Mes_User);

            log.Content += $"Server: {server}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            Global.Logs.Add(log);
        }

        public static void Mes_Other(string message, string server = "DM")
        {
            Log log = BaseLog(LogType.Mes_Other);

            log.Content += $"Server: {server}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            Global.Logs.Add(log);
        }
        #endregion

        #region Error logging
        public static void Error(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Error);

            log.Content += $"Location: {location}\n{message}";
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(log.Content);
            }

            if (!ConsoleOnly)
            {
                Global.Logs.Add(log);
            }
        }

        public static void Warning(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Warning);

            log.Content += $"Location: {location}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(log.Content);
            }

            if (!ConsoleOnly)
            {
                Global.Logs.Add(log);
            }
        }
        #endregion

        #region Helper methods
        private static Log BaseLog(LogType type)
        {
            return new Log(DateTime.Now, type, $"[{CurrentTime()}][{type.Value}]:\t");
        }

        private static string PutTabsOnNewLines(string message)
        {
            return message.Replace("\n", "\n\t\t\t");
        }

        private static string CurrentTime() 
        {
            string hour = DateTime.Now.Hour < 10 ? "0" + DateTime.Now.Hour.ToString() : DateTime.Now.Hour.ToString();
            string minute = DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString();
            string second = DateTime.Now.Second < 10 ? "0" + DateTime.Now.Second.ToString() : DateTime.Now.Second.ToString();

            return $"{hour}:{minute}:{second}"; 
        }
        #endregion
    }
}
