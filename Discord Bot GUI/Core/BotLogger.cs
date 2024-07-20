using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using Discord_Bot.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace Discord_Bot.Core;
public class BotLogger
{
    //List of logs, before they are cleared
    public readonly List<Log> Logs = [];

    public void LogToFile()
    {
        try
        {
            StreamWriter logFileWriter = null;
            if (Logs.Count != 0 && logFileWriter == null)
            {
                string file_location = $"Logs\\logs[{DateTimeTools.CurrentDate()}].txt";

                using (logFileWriter = File.AppendText(file_location))
                {
                    string[] contents = Logs.Select(n => n.Content).ToArray();
                    foreach (string log in contents)
                    {
                        logFileWriter.WriteLine(log);
                    }
                }

                logFileWriter = null;
                Logs.Clear();
            }
        }
        catch (Exception ex)
        {
            Error("BotLogger.cs LogtoFile", ex);
        }
    }

    #region Internal Logging
    public void Log(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Log);

        log.Content += message;
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.White);
        }

        if (!ConsoleOnly)
        {
            Logs.Add(log);
        }
    }

    public void Query(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Query);

        log.Content += message;
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.DarkCyan);
        }

        if (!ConsoleOnly)
        {
            Logs.Add(log);
        }
    }

    public void Client(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Client);

        log.Content += message;
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.DarkGray);
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
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.MesUser);

        log.Content += $"Server: {server}, Content: {message}";
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        Logs.Add(log);
    }

    public void MesOther(string message, string server = "DM")
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.MesOther);

        log.Content += $"Server: {server}, Content: {message}";
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        Logs.Add(log);
    }
    #endregion

    #region Error Logging
    public void Error(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Error);

        log.Content += $"Location: {location}\n{message}";
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.Red);
        }

        if (!ConsoleOnly)
        {
            Logs.Add(log);
        }
    }

    public void Error(string location, Exception ex, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Error);

        log.Content += $"Location: {location}\n{ex}";
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.Red);
        }

        if (!ConsoleOnly)
        {
            Logs.Add(log);
        }
    }

    public void Warning(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Warning);

        log.Content += $"Location: {location}, Content: {message}";
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.Yellow);
        }

        if (!ConsoleOnly)
        {
            Logs.Add(log);
        }
    }

    public void Warning(string location, Exception ex, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Log log = BotLoggerTools.BaseLog(LogTypeEnum.Warning);

        log.Content += $"Location: {location}, Content: {ex}";
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            BotWindow.LogToWindow(log, Brushes.Yellow);
        }

        if (!ConsoleOnly)
        {
            Logs.Add(log);
        }
    }
    #endregion
}
