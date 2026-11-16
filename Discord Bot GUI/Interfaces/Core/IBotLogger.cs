using System;

namespace Discord_Bot.Interfaces.Core;

//What the bot's code logs through. The concrete BotLogger also feeds the window (LogAdded) and the log file (LogToFile)
//ConsoleOnly: shown in the window, not written to the file. LogOnly: written to the file, not shown in the window
public interface IBotLogger
{
    void Log(string message, bool ConsoleOnly = false, bool LogOnly = false);
    void Query(string message, bool ConsoleOnly = false, bool LogOnly = false);
    void Client(string message, bool ConsoleOnly = false, bool LogOnly = false);
    void Audio(string message, bool ConsoleOnly = false, bool LogOnly = false);

    //Messages are only written to the file
    void MesUser(string message, string server = "DM");
    void MesOther(string message, string server = "DM");

    void Error(string location, string message, bool ConsoleOnly = false, bool LogOnly = false);
    void Error(string location, Exception ex, bool ConsoleOnly = false, bool LogOnly = false);
    void Warning(string location, string message, bool ConsoleOnly = false, bool LogOnly = false);
    void Warning(string location, Exception ex, bool ConsoleOnly = false, bool LogOnly = false);
}
