using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Core;

//Todo: This class might not be needed at all, revisit it
public class CoreLogic(Logging logger, IServerService serverService) : ICoreLogic
{
    private readonly Logging logger = logger;
    private readonly IServerService serverService = serverService;

    public async Task<ServerResource> GetOrAddServerAsync(ulong serverId, string serverName)
    {
        ServerResource server = await serverService.GetByDiscordIdAsync(serverId);
        if (server == null)
        {
            DbProcessResultEnum result = await serverService.AddServerAsync(serverId);
            if (result == DbProcessResultEnum.Success)
            {
                server = await serverService.GetByDiscordIdAsync(serverId);
            }
            else
            {
                logger.Log($"{serverName} could not be added to list!");
            }
        }

        return server;
    }

    //For logging messages, errors, and messages to log files
    public void LogToFile()
    {
        try
        {
            StreamWriter logFileWriter = null;
            if (logger.Logs.Count != 0 && logFileWriter == null)
            {
                string file_location = $"Logs\\logs[{DateTimeTools.CurrentDate()}].txt";

                using (logFileWriter = File.AppendText(file_location))
                {
                    string[] contents = logger.Logs.Select(n => n.Content).ToArray();
                    foreach (string log in contents)
                    {
                        logFileWriter.WriteLine(log);
                    }
                }

                logFileWriter = null;
                logger.Logs.Clear();
            }
        }
        catch (Exception ex)
        {
            logger.Error("CoreLogic.cs LogtoFile", ex);
        }
    }

    //Check if folders for long term storage exist
    public void CheckFolders()
    {
        try
        {
            List<string> logs = [];

            string currentDir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(currentDir, "Logs")))
            {
                Directory.CreateDirectory(Path.Combine(currentDir, "Logs"));
                logs.Add("Logs folder created!");
            }

            if (!Directory.Exists(Path.Combine(currentDir, "Assets")))
            {
                Directory.CreateDirectory(Path.Combine(currentDir, "Assets"));
                logs.Add("Assets folder created!");
            }

            if (!Directory.Exists(Path.Combine(currentDir, "Assets\\Commands")))
            {
                Directory.CreateDirectory(Path.Combine(currentDir, "Assets\\Commands"));
                logs.Add("Commands folder created!");
            }

            if (logs.Count != 0)
            {
                logger.Log(string.Join('\n', logs));
            }
        }
        catch (Exception ex)
        {
            logger.Error("CoreLogic.css CheckFolder", ex);
        }
    }
}
