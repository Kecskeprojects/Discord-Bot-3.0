using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Core
{
    //Move Logic from here to Commands as another Communication file similar to the Service one
    public class CoreLogic : ICoreLogic
    {
        private readonly Logging _logging;
        public CoreLogic(Logging logging) => _logging = logging;


        #region Feature handle methods
        //Check the list of custom commands on the server
        public async Task CustomCommands(SocketCommandContext context)
        {
            try
            {
                //Todo: Reimplement custom command
                /*
                var command = DBFunctions.CustomCommandGet(context.Guild.Id, context.Message.Content[1..].ToLower());

                if (command != null) 
                {
                    await context.Message.Channel.SendMessageAsync(command.URL);
                }*/
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs CustomCommands", ex.ToString());
            }
        }

        //Add/Remove roles from users, and keep the role chat clean
        public async Task SelfRole(SocketCommandContext context)
        {
            try
            {
                RestUserMessage reply = null;
                //Todo: Reimplement self role
                /*
                var role = DBFunctions.SelfRoleGet(context.Guild.Id, context.Message.Content[1..].ToLower());

                if (role != null)
                {
                    IRole get_role = (context.Channel as IGuildChannel).Guild.GetRole(role.RoleId);

                    switch (context.Message.Content[0])
                    {
                        case '+':
                            {
                                await (context.User as SocketGuildUser).AddRoleAsync(get_role);
                                reply = await context.Channel.SendMessageAsync("You now have the `" + role.RoleName + "` role!");
                                break;
                            }
                        case '-':
                            {
                                await (context.User as SocketGuildUser).RemoveRoleAsync(get_role);
                                reply = await context.Channel.SendMessageAsync("`" + role.RoleName+ "` role has been removed!");
                                break;
                            }
                    }
                }
                */
                if (reply != null)
                {
                    await Task.Delay(2000);

                    await reply.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs SelfRole", ex.ToString());
            }
        }

        //Check for messages starting with I think and certain Keywords
        public async Task FeatureChecks(SocketCommandContext context)
        {
            try
            {
                Random r = new();

                //Easter egg messages
                if (r.Next(0, 5000) == 0)
                {
                    await context.Channel.SendMessageAsync(Global.EasterEggMessages[r.Next(0, Global.EasterEggMessages.Length)]);
                    return;
                }
                else if (r.Next(1, 101) < 10)
                {
                    string mess = context.Message.Content.ToLower();
                    if (mess.StartsWith("i think"))
                    {
                        await context.Channel.SendMessageAsync("I agree wholeheartedly!");
                    }
                    else if ((mess.StartsWith("i am") && mess != "i am") || (mess.StartsWith("i'm") && mess != "i'm"))
                    {
                        await context.Channel.SendMessageAsync(string.Concat("Hey ", context.Message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!"));
                    }

                    return;
                }

                if (context.Message.Content.Length <= 100 && context.Channel.GetChannelType() != ChannelType.DM)
                {
                    //Todo: Reimplement keyword
                    /*
                    var keyword = DBFunctions.KeywordGet(context.Guild.Id, context.Message.Content.Replace("\'" , "").Replace("\"", "").Replace("`", "").Replace(";", ""));
                    if (keyword != null)
                    {
                        await context.Channel.SendMessageAsync(keyword.Response);
                    }*/
                }
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs FeatureCheck", ex.ToString());
            }
        }

        //Checking and sending out reminders
        public async Task ReminderCheck(DiscordSocketClient Client)
        {
            try
            {
                //Todo: Reimplement reminder
                //Get the list of reminders that are before or exactly set to this minute
                //Also format date to sql compatible format
                /*var result = DBFunctions.ReminderList(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                if (result.Count > 0)
                {
                    foreach (var reminder in result)
                    {
                        //Modify message
                        reminder.Message = reminder.Message.Insert(0, $"You told me to remind you at `{reminder.Date}` with the following message:\n\n");

                        //Try getting user
                        var user = await Client.GetUserAsync(reminder.UserId);

                        //If user exists send a direct message to the user
                        if (user != null)
                        {
                            await UserExtensions.SendMessageAsync(user, reminder.Message);
                        }

                        //Delete the user regardless of the outcome, unless an error occurs of course, keep it in that case
                        //Also format date to sql compatible format
                        DBFunctions.ReminderRemove(reminder.UserId, reminder.Date.ToString("yyyy-MM-dd HH:mm"));
                    }
                }*/
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs Log ReminderCheck", ex.ToString());
            }
        }
        #endregion

        #region Before closing method
        //Things to do when app is closing
        //3 second time limit to event by default
        public void Closing()
        {
            _logging.Log("Application closing...");
            LogToFile();
        }

        public void Closing(object sender, EventArgs e)
        {
            _logging.Log("Application closing...");
            LogToFile();
        }
        #endregion

        #region Embed handlers
        //Check if message is an instagram link and has an embed or not
        public void InstagramEmbed(SocketCommandContext context)
        {
            List<Uri> urls = LinkSearch(context.Message.Content, false, "https://instagram.com/");

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
                            _logging.Log($"Embed message from following link: {urls[i]}");

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
                        _logging.Error("ProgramFunctions.cs InstagramEmbed", ex.ToString());
                    }
                });
            }
        }

        public void TwitterEmbed(SocketCommandContext context)
        {
            List<Uri> urls = LinkSearch(context.Message.Content, true, new string[] { "https://twitter.com/", "https://x.com/" });

            //Check if message is an instagram link
            if (urls != null)
            {
                urls = urls.Where(x => x.Segments.Length >= 3 && x.Segments[2] == "status/").ToList();
                if (urls.Count > 0)
                {
                    //Run link embedding in separate thread
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _logging.Log($"Embed message from following links: \n{string.Join("\n", urls)}");

                            //Todo: Reimplement twitter scraper
                            MessageReference refer = new(context.Message.Id, context.Channel.Id, context.Guild.Id, false);
                            /*string message = await new TwitterScraper().Main(context.Channel, refer, urls);

                            if (!string.IsNullOrEmpty(message))
                            {
                                await context.Channel.SendMessageAsync(message);
                            }
                            else
                            {
                                await context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                            }*/
                        }
                        catch (Exception ex)
                        {
                            _logging.Error("ProgramFunctions.cs TwitterEmbed", ex.ToString());
                        }
                    });
                }
            }
        }

        //For logging messages, errors, and messages to log files
        public void LogToFile()
        {
            StreamWriter LogFile_writer = null;
            try
            {
                if (_logging.Logs.Count != 0 && LogFile_writer == null)
                {
                    string file_location = $"Logs\\logs[{Global.CurrentDate()}].txt";

                    using (LogFile_writer = File.AppendText(file_location))
                    {
                        foreach (string log in _logging.Logs.Select(n => n.Content))
                        {
                            LogFile_writer.WriteLine(log);
                        }
                    }

                    LogFile_writer = null;
                    _logging.Logs.Clear();
                }
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs LogtoFile", ex.ToString());
            }
        }

        //Check if folders for long term storage exist
        public void Check_Folders()
        {
            List<string> logs = new();
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "\\Logs")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "\\Logs"));
                logs.Add("Logs folder created!");
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets"));
                logs.Add("Assets folder created!");
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets\\Commands")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "\\Assets\\Commands"));
                logs.Add("Commands folder created!");
            }

            if (logs.Count != 0)
            {
                _logging.Log(string.Join('\n', logs));
            }
        }

        private List<Uri> LinkSearch(string message, bool ignoreEmbedSuppress, params string[] baseURLs)
        {
            try
            {
                message = message.Replace("www.", "");

                if (baseURLs.Any(x => message.Contains(x)))
                {
                    List<Uri> urls = new();

                    //We check for each baseURL for each that was sent, one is expected
                    foreach (string baseURL in baseURLs)
                    {
                        int startIndex = 0;
                        while (startIndex != -1)
                        {
                            startIndex = message.IndexOf(baseURL, startIndex);
                            if (startIndex != -1)
                            {
                                string beginningCut = message[startIndex..];

                                string url = beginningCut.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                                if (!ignoreEmbedSuppress && !url.Contains('<') && !url.Contains('>'))
                                {
                                    url = url.Split('?')[0];
                                    urls.Add(new Uri(url));
                                }
                                else if (ignoreEmbedSuppress)
                                {
                                    url = url.Replace("<", "").Replace(">", "").Split('?')[0];
                                    urls.Add(new Uri(url));
                                }
                                startIndex++;
                            }
                        }
                    }

                    return urls;
                }

            }
            catch (Exception ex)
            {
                _logging.Error("CoreLogic.cs LinkSearch", ex.ToString());
            }

            return null;
        }
        #endregion
    }
}
