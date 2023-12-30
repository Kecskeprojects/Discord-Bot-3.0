using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
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
    public class CoreLogic(Logging logger, IServerService serverService, ICoreToDiscordCommunication coreDiscordCommunication, IBiasDatabaseService biasDatabaseService, IIdolService idolService) : ICoreLogic
    {
        private readonly Logging logger = logger;
        private readonly IServerService serverService = serverService;
        private readonly ICoreToDiscordCommunication coreDiscordCommunication = coreDiscordCommunication;
        private readonly IBiasDatabaseService biasDatabaseService = biasDatabaseService;
        private readonly IIdolService idolService = idolService;

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

        public async Task UpdateExtendedBiasData()
        {
            try
            {
                logger.Log("Update Bias Data Logic started!");
                List<ExtendedBiasData> completeList = await biasDatabaseService.GetBiasDataAsync();
                logger.Log($"Found {completeList.Count} idols on site that have profile pages.");

                List<IdolResource> localIdols = await idolService.GetAllIdolsAsync();
                logger.Log($"Found {localIdols.Count} idols in our database.");

                int count = 0;
                for (int i = 0; i < localIdols.Count; i++)
                {
                    string profileUrl = GetProfileUrl(localIdols[i], completeList, out ExtendedBiasData data);

                    if (string.IsNullOrEmpty(profileUrl))
                    {
                        continue;
                    }

                    AdditionalIdolData additional = await biasDatabaseService.GetAdditionalBiasDataAsync(profileUrl, localIdols[i].GroupDebutDate == null);

                    if (localIdols[i].CurrentImageUrl == additional.ImageUrl)
                    {
                        continue;
                    }

                    logger.Log($"Updating {data.StageName} of {data.GroupName}'s details.");
                    count++;
                    await idolService.UpdateIdolDetailsAsync(localIdols[i], data, additional);
                }
                logger.Log($"Updated {count} idol's details.");
                logger.Log("Update Bias Data Logic ended!");
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs UpdateExtendedBiasData", ex.ToString());
            }
        }

        #region OnClose logic
        //Things to do when app is closing
        //3 second time limit to event by default
        public async void Closing()
        {
            try
            {
                logger.Log("Application closing...");
                await BrowserService.CloseBrowser();
                LogToFile();
            }
            catch (Exception) { }
        }

        public async void Closing(object sender, EventArgs e)
        {
            try
            {
                logger.Log("Application closing...");
                await BrowserService.CloseBrowser();
                LogToFile();
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
                logger.Error("CoreLogic.css CheckFolder", ex.ToString());
            }
        }

        public static string GetProfileUrl(IdolResource resource, List<ExtendedBiasData> completeList, out ExtendedBiasData data)
        {
            string profileUrl = "";
            data = null;
            if (!string.IsNullOrEmpty(resource.ProfileUrl))
            {
                profileUrl = resource.ProfileUrl;
            }
            else
            {
                List<ExtendedBiasData> datas = completeList.Where(x => x.StageName.Equals(resource.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (datas.Count > 1)
                {
                    data = datas.FirstOrDefault(x => x.GroupName.RemoveSpecialCharacters().Equals(resource.GroupName, StringComparison.OrdinalIgnoreCase));
                }
                else if (datas.Count == 1)
                {
                    data = datas[0];
                }
                profileUrl = data?.ProfileUrl;
            }
            return profileUrl;
        }
        #endregion
    }
}
