using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Tools.NativeTools;
using System.IO;

namespace Discord_Bot.Tests.Unit.Bot.Core;

//The bot's logger: each log goes to the window (LogAdded) and to the queue for today's file in Logs/, which LogToFile writes.
//The file is the one the bot writes in its working directory, so each test looks for a line of its own
public sealed class BotLoggerTests
{
    private readonly BotLogger logger = new();
    private readonly List<Log> shown = [];
    private readonly string marker = Guid.NewGuid().ToString("N");
    private readonly string logFile = Path.Combine("Logs", $"logs[{DateTimeTools.CurrentDate()}].txt");

    public BotLoggerTests()
    {
        logger.LogAdded += (_, log) => shown.Add(log);
        _ = Directory.CreateDirectory("Logs");
    }

    [Fact]
    public void Log_GoesToTheWindowAndToTheFile()
    {
        logger.Log($"hello {marker}");

        Log log = Assert.Single(shown);
        Assert.Equal(LogTypeEnum.Log, log.Type);
        Assert.EndsWith($"hello {marker}", log.Content, StringComparison.Ordinal);
        logger.LogToFile();
        Assert.Contains(File.ReadAllLines(logFile), line => line.EndsWith($"hello {marker}", StringComparison.Ordinal));
    }

    [Fact]
    public void LogOnlyAndConsoleOnly_GoToTheFileOrTheWindowAlone()
    {
        logger.Query($"file {marker}", LogOnly: true);
        logger.Query($"window {marker}", ConsoleOnly: true);

        Assert.EndsWith($"window {marker}", Assert.Single(shown).Content, StringComparison.Ordinal);
        logger.LogToFile();
        string[] lines = File.ReadAllLines(logFile);
        Assert.Contains(lines, line => line.EndsWith($"file {marker}", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.EndsWith($"window {marker}", StringComparison.Ordinal));
    }

    //A message over several lines stays one entry, the following lines indented under it
    [Fact]
    public void Error_GivesTheLocationAndTheExceptionIndented()
    {
        logger.Error("BotLoggerTests.cs Error", new InvalidOperationException($"broken {marker}"));

        Log log = Assert.Single(shown);
        Assert.Equal(LogTypeEnum.Error, log.Type);
        Assert.Contains("Location: BotLoggerTests.cs Error\n\t\t\tSystem.InvalidOperationException: broken", log.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void LogToFile_FileInUse_KeepsTheLogsForTheNextTry()
    {
        logger.Log($"kept {marker}");
        using (new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {
            logger.LogToFile();
        }

        Assert.Contains(shown, log => log.Type == LogTypeEnum.Error && log.Content.Contains("BotLogger.cs LogToFile", StringComparison.Ordinal));
        logger.LogToFile();
        Assert.Contains(File.ReadAllLines(logFile), line => line.EndsWith($"kept {marker}", StringComparison.Ordinal));
    }
}
