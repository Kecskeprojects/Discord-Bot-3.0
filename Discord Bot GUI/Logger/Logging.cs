using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Discord_Bot.Logger
{
    public class Logging
    {
        //List of logs, before they are cleared
        public readonly List<Log> Logs = new();


        #region Bot logging
        public void Log(string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Log);

            log.Content += message;
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                LogToWindow(log, Brushes.White);
            }

            if (!ConsoleOnly)
            {
                Logs.Add(log);
            }
        }


        public void Query(string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Query);

            log.Content += message;
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                LogToWindow(log, Brushes.DarkCyan);
            }

            if (!ConsoleOnly)
            {
                Logs.Add(log);
            }
        }


        public void Client(string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Client);

            log.Content += message;
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                LogToWindow(log, Brushes.DarkGray);
            }

            if (!ConsoleOnly)
            {
                Logs.Add(log);
            }
        }
        #endregion


        #region Message logging
        public void Mes_User(string message, string server = "DM")
        {
            Log log = BaseLog(LogType.Mes_User);

            log.Content += $"Server: {server}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            Logs.Add(log);
        }

        public void Mes_Other(string message, string server = "DM")
        {
            Log log = BaseLog(LogType.Mes_Other);

            log.Content += $"Server: {server}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            Logs.Add(log);
        }
        #endregion


        #region Error logging
        public void Error(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Error);

            log.Content += $"Location: {location}\n{message}";
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                LogToWindow(log, Brushes.Red);
            }

            if (!ConsoleOnly)
            {
                Logs.Add(log);
            }
        }


        public void Warning(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
        {
            Log log = BaseLog(LogType.Warning);

            log.Content += $"Location: {location}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            if (!LogOnly)
            {
                LogToWindow(log, Brushes.Yellow);
            }

            if (!ConsoleOnly)
            {
                Logs.Add(log);
            }
        }
        #endregion


        #region Helper methods
        private static void LogToWindow(Log log, Brush color)
        {
            if(Application.Current != null)
            {
                string mess = log.Content.Replace(":\t", ":    \t");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if(Application.Current.MainWindow != null)
                    {
                        MainWindow main = Application.Current.MainWindow as MainWindow;
                        main.MainLogText.Foreground = color;
                        main.MainLogText.Text += "\n" + mess;
                    }
                });
            }
        }


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
