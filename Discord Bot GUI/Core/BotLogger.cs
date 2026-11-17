using DatabaseORM.Interface;
using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using System.Collections.Concurrent;
using System.IO;

namespace Discord_Bot.Core;

//Only Startup, App and BotWindow use this type directly, for LogAdded and LogToFile; everything else logs through IBotLogger
public class BotLogger : IBotLogger
{
    //Logs waiting for the file. Any thread can add to it, LogToFile empties it
    private readonly ConcurrentQueue<Log> logs = new();
    //Taken from the queue but not written yet: kept for the next try when writing the file fails
    private readonly List<Log> unwritten = [];
    //The timer, the window's button and the shutdown can all write the file at the same time
    private readonly Lock fileLock = new();

    //Raised for every log meant for the window, on the thread that logged it
    public event EventHandler<Log>? LogAdded;

    public void LogToFile()
    {
        try
        {
            lock (fileLock)
            {
                while (logs.TryDequeue(out Log? queued))
                {
                    unwritten.Add(queued);
                }

                if (unwritten.Count != 0)
                {
                    string file_location = $"Logs\\logs[{DateTimeTools.CurrentDate()}].txt";

                    using (StreamWriter logFileWriter = File.AppendText(file_location))
                    {
                        foreach (Log log in unwritten)
                        {
                            logFileWriter.WriteLine(log.Content);
                        }
                    }

                    unwritten.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Error("BotLogger.cs LogToFile", ex);
        }
    }

    #region Internal Logging
    public void Log(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Log, message, ConsoleOnly, LogOnly);
    }

    public void Query(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Query, message, ConsoleOnly, LogOnly);
    }

    public void Client(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Client, message, ConsoleOnly, LogOnly);
    }

    public void Audio(string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Audio, message, ConsoleOnly, LogOnly);
    }
    #endregion

    #region Message Logging
    public void MesUser(string message, string server = "DM")
    {
        Add(LogTypeEnum.MesUser, $"Server: {server}, Content: {message}", ConsoleOnly: false, LogOnly: true);
    }

    public void MesOther(string message, string server = "DM")
    {
        Add(LogTypeEnum.MesOther, $"Server: {server}, Content: {message}", ConsoleOnly: false, LogOnly: true);
    }
    #endregion

    #region Error Logging
    public void Error(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Error, $"Location: {location}\n{message}", ConsoleOnly, LogOnly);
    }

    public void Error(string location, Exception ex, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Error, $"Location: {location}\n{ex}", ConsoleOnly, LogOnly);
    }

    public void Warning(string location, string message, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Warning, $"Location: {location}, Content: {message}", ConsoleOnly, LogOnly);
    }

    public void Warning(string location, Exception ex, bool ConsoleOnly = false, bool LogOnly = false)
    {
        Add(LogTypeEnum.Warning, $"Location: {location}, Content: {ex}", ConsoleOnly, LogOnly);
    }
    #endregion

    private void Add(LogTypeEnum type, string content, bool ConsoleOnly, bool LogOnly)
    {
        Log log = BotLoggerTools.BaseLog(type);

        log.Content += content;
        log.Content = BotLoggerTools.PutTabsOnNewLines(log.Content);

        if (!LogOnly)
        {
            LogAdded?.Invoke(this, log);
        }

        if (!ConsoleOnly)
        {
            logs.Enqueue(log);
        }
    }
}
