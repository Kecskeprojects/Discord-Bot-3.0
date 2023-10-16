using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Services;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Core
{
    public class CoreLogic : ICoreLogic
    {
        private readonly Logging logger;
        private readonly ICoreToDiscordCommunication coreDiscordCommunication;
        private readonly IServerService serverService;

        public CoreLogic(Logging logger, IServerService serverService, ICoreToDiscordCommunication coreDiscordCommunication)
        {
            this.logger = logger;
            this.coreDiscordCommunication = coreDiscordCommunication;
            this.serverService = serverService;
        }

        public async Task<ServerResource> GetServerAsync(ulong serverId, string serverName)
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

        #region OnClose logic
        //Things to do when app is closing
        //3 second time limit to event by default
        public async void Closing()
        {
            try
            {
                logger.Log("Application closing...");
                LogToFile();
                await TwitterScraper.CloseBrowser();
            }
            catch (Exception) { }
        }

        public async void Closing(object sender, EventArgs e)
        {
            try
            {
                logger.Log("Application closing...");
                LogToFile();
                await TwitterScraper.CloseBrowser();
            }
            catch (Exception) { }
        }
        #endregion

        #region CoreDiscordCommunication Calls
        public async Task ReminderCheck()
        {
            await coreDiscordCommunication.SendReminders();
        }

        public async Task BirthdayCheck()
        {
            await coreDiscordCommunication.SendBirthdayMessages();
        }

        //Check if message is an instagram link and has an embed or not
        public void InstagramEmbed(string message, ulong messageId, ulong channelId, ulong? guildId)
        {
            try
            {
                List<Uri> urls = UrlTools.LinkSearch(message, false, "https://instagram.com/");

                //Check if message is an instagram link
                if (!CollectionTools.IsNullOrEmpty(urls))
                {
                    //Run link embedding in separate thread
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            for (int i = 0; i < urls.Count; i++)
                            {
                                logger.Log($"Embed message from following link: {urls[i]}");
                                if (urls[i].Segments.Length != 2 &&
                                    urls[i].Segments[1][..^1] != "stories" &&
                                    urls[i].Segments[1][..^1] != "live" &&
                                    urls[i].Segments[2][..^1] != "live")
                                {
                                    await coreDiscordCommunication.SendInstagramPostEmbed(urls[i], messageId, channelId, guildId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("CoreLogic.cs InstagramEmbed AsyncTask", ex.ToString());
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs InstagramEmbed", ex.ToString());
            }
        }
        #endregion

        #region Helper Methods
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
                logger.Error("CoreLogic.cs LogtoFile", ex.ToString());
            }
        }

        //Check if folders for long term storage exist
        public void CheckFolders()
        {
            try
            {
                List<string> logs = new();

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
                logger.Error("CoreLogic.css CheckFolder", ex.ToString());
            }
        }
        #endregion
    }
}
