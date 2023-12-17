using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Discord_Bot.Core.Logger
{
    public class Logging
    {
        //List of logs, before they are cleared
        public readonly List<Log> Logs = [];

        public static void ClearWindowLog()
        {
            Application.Current?.Dispatcher.Invoke(DispatcherPriority.DataBind, () =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        MainWindow main = Application.Current.MainWindow as MainWindow;
                        IEnumerable<Inline> range = main.MainLogText.Inlines.TakeLast(20);
                        main.MainLogText.Inlines.Clear();
                        main.MainLogText.Inlines.AddRange(range);
                    }
                });
        }

        #region Bot Logging
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

        #region Message Logging
        public void MesUser(string message, string server = "DM")
        {
            Log log = BaseLog(LogType.Mes_User);

            log.Content += $"Server: {server}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            Logs.Add(log);
        }

        public void MesOther(string message, string server = "DM")
        {
            Log log = BaseLog(LogType.Mes_Other);

            log.Content += $"Server: {server}, Content: {message}";
            log.Content = PutTabsOnNewLines(log.Content);

            Logs.Add(log);
        }
        #endregion

        #region Error Logging
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

        #region Helper Methods
        private static void LogToWindow(Log log, Brush color)
        {
            Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            string mess = log.Content.Replace(":\t", ":    \t");
            Application.Current?.Dispatcher.Invoke(DispatcherPriority.DataBind, () =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        MainWindow main = Application.Current.MainWindow as MainWindow;
                        Run run = new(mess + "\n")
                        {
                            Foreground = color
                        };
                        main.MainLogText.Inlines.Add(run);
                    }
                });
        }

        private static Log BaseLog(LogType type)
        {
            return new(DateTime.Now, type, $"[{DateTimeTools.CurrentTime()}][{type.Value}]:\t");
        }

        private static string PutTabsOnNewLines(string message)
        {
            return message.Replace("\n", "\n\t\t\t");
        }
        #endregion
    }
}
