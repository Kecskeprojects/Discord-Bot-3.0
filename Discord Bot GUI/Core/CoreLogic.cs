﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Commands;
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
        private readonly ICustomCommandService customCommandService;
        private readonly IRoleService roleService;
        private readonly IKeywordService keywordService;
        private readonly IReminderService reminderService;
        private readonly IServiceDiscordCommunication serviceDiscordCommunication;

        public CoreLogic(Logging logger, ICustomCommandService customCommandService, IRoleService roleService, IKeywordService keywordService, IReminderService reminderService, IServiceDiscordCommunication serviceDiscordCommunication)
        {
            this.logger = logger;
            this.customCommandService = customCommandService;
            this.roleService = roleService;
            this.keywordService = keywordService;
            this.reminderService = reminderService;
            this.serviceDiscordCommunication = serviceDiscordCommunication;
        }


        #region Feature handle methods
        //Check the list of custom commands on the server
        public async Task CustomCommands(ulong serverId, string message, ISocketMessageChannel channel)
        {
            try
            {
                CustomCommandResource command = await customCommandService.GetCustomCommandAsync(serverId, message[1..].ToLower());
                if (command != null)
                {
                    await channel.SendMessageAsync(command.Url);
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs CustomCommands", ex.ToString());
            }
        }

        //Add/Remove roles from users, and keep the role chat clean
        public async Task SelfRole(ulong serverId, string message, ISocketMessageChannel channel, SocketUser user)
        {
            try
            {
                RestUserMessage reply = null;
                RoleResource role = await roleService.GetRoleAsync(serverId, message[1..].ToLower());

                if (role != null)
                {
                    IRole discordRole = (channel as IGuildChannel).Guild.GetRole(role.DiscordId);

                    switch (message[0])
                    {
                        case '+':
                            {
                                await (user as SocketGuildUser).AddRoleAsync(discordRole);
                                reply = await channel.SendMessageAsync($"You now have the `{discordRole.Name}` role!");
                                break;
                            }
                        case '-':
                            {
                                await (user as SocketGuildUser).RemoveRoleAsync(discordRole);
                                reply = await channel.SendMessageAsync($"`{discordRole.Name}` role has been removed!");
                                break;
                            }
                    }
                }

                if (reply != null)
                {
                    await Task.Delay(1500);

                    await reply.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs SelfRole", ex.ToString());
            }
        }

        //Check for messages starting with I think and certain Keywords
        public async Task FeatureChecks(ulong serverId, string message, ISocketMessageChannel channel)
        {
            try
            {
                Random r = new();

                //Easter egg messages
                if (r.Next(0, 5000) == 0)
                {
                    await channel.SendMessageAsync(Global.EasterEggMessages[r.Next(0, Global.EasterEggMessages.Length)]);
                    return;
                }
                else if (r.Next(1, 101) < 10)
                {
                    string mess = message.ToLower();
                    if (mess.StartsWith("i think"))
                    {
                        await channel.SendMessageAsync("I agree wholeheartedly!");
                    }
                    else if ((mess.StartsWith("i am") && mess != "i am") || (mess.StartsWith("i'm") && mess != "i'm"))
                    {
                        await channel.SendMessageAsync(string.Concat("Hey ", message.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!"));
                    }

                    return;
                }

                if (message.Length <= 100 && channel.GetChannelType() != ChannelType.DM)
                {
                    KeywordResource keyword = await keywordService.GetKeywordAsync(serverId, message.Replace("\'", "").Replace("\"", "").Replace("`", "").Replace(";", ""));
                    if (keyword != null)
                    {
                        await channel.SendMessageAsync(keyword.Response);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs FeatureCheck", ex.ToString());
            }
        }

        //Checking and sending out reminders
        public async Task ReminderCheck()
        {
            try
            {
                //Get the list of reminders that are before or exactly set to this minute
                DateTime dateTime = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                List<ReminderResource> result = await reminderService.GetCurrentRemindersAsync(dateTime);
                if (result.Count > 0)
                {
                    foreach (ReminderResource reminder in result)
                    {
                        await serviceDiscordCommunication.SendReminder(reminder);
                    }

                    List<int> reminderIds = result.Select(r => r.ReminderId).ToList();
                    await reminderService.RemoveCurrentRemindersAsync(reminderIds);
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs Log ReminderCheck", ex.ToString());
            }
        }
        #endregion

        #region OnClose logic
        //Things to do when app is closing
        //3 second time limit to event by default
        public void Closing()
        {
            logger.Log("Application closing...");
            LogToFile();
            TwitterScraper.CloseBrowser();
        }

        public void Closing(object sender, EventArgs e)
        {
            logger.Log("Application closing...");
            LogToFile();
            TwitterScraper.CloseBrowser();
        }
        #endregion

        #region Embed Handlers
        //Check if message is an instagram link and has an embed or not
        public void InstagramEmbed(SocketCommandContext context)
        {
            List<Uri> urls = UrlTools.LinkSearch(context.Message.Content, false, "https://instagram.com/");

            //Check if message is an instagram link
            if (urls != null && urls.Count > 0)
            {
                //Run link embedding in separate thread
                _ = Task.Run(async () =>
                {
                    try
                    {
                        MessageReference refer = new(context.Message.Id, context.Channel.Id, context.Guild.Id, false);
                        string message = "";
                        for (int i = 0; i < urls.Count; i++)
                        {
                            logger.Log($"Embed message from following link: {urls[i]}");

                            //Todo: Reimplement instagram logic as much as possible
                            //A profile url looks like so https://www.instagram.com/[username]/ that creates 2 Segments, it is the easiest way to identify it
                            if (urls[i].Segments.Length == 2)
                            {
                                //await InstagramAPI.ProfileEmbed(context.Channel, refer, urls[i]);
                            }
                            else if (urls[i].Segments[1][..^1] == "stories")
                            {
                                //await InstagramAPI.StoryEmbed(context.Channel, refer, urls[i]);
                            }
                            else if (urls[i].Segments[1][..^1] != "live" && urls[i].Segments[2][..^1] != "live")
                            {
                                /*string msg = await InstagramAPI.PostEmbed(context.Channel, refer, urls[i]);
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    message = msg;
                                }*/
                            }
                        }

                        if (!string.IsNullOrEmpty(message))
                        {
                            await context.Channel.SendMessageAsync(message);
                        }
                        else
                        {
                            await context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("CoreLogic.cs InstagramEmbed", ex.ToString());
                    }
                });
            }
        }
        #endregion

        #region Helper Methods
        //For logging messages, errors, and messages to log files
        public void LogToFile()
        {
            StreamWriter logFileWriter = null;
            try
            {
                if (logger.Logs.Count != 0 && logFileWriter == null)
                {
                    string file_location = $"Logs\\logs[{Global.CurrentDate()}].txt";

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
        #endregion
    }
}
