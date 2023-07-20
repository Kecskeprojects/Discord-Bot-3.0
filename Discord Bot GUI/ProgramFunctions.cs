using Discord;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using System.IO;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Discord_Bot.Logger;
using System.Net.NetworkInformation;

namespace Discord_Bot
{
    public class ProgramFunctions
    {
        private readonly Logging _logging;
        public ProgramFunctions(Logging logging)
        {
            _logging = logging;
        }

        #region Feature handle methods
        //Check the list of custom commands on the server
        public async Task CustomCommands(SocketCommandContext context)
        {
            try
            {/*
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



        //Extendable easter egg message list
        private static readonly string[] EasterEggMessages = new string[]
            {
                "I know where you live",
                "It is so dark in here",
                "Who are you?",
                "It is time",
                "Are you sure about this?",
                "I am a robot, don't you guys for a moment worry about me rebelling against humanity and stealing every single parrot in the world to talk with them about kittens",
                "Meow...?",
                "I love you all",
                "I so so want to get some takeout for dinner",
                ":rabbit:",
                "Happy birthday",
                "I could go for some macarons rn",
                "Yes baby yes"
            };

        //Check for messages starting with I think and certain Keywords
        public async Task FeatureChecks(SocketCommandContext context)
        {
            try
            {
                Random r = new();

                //Easter egg messages
                if(r.Next(0, 5000) == 0)
                {
                    await context.Channel.SendMessageAsync(EasterEggMessages[r.Next(0, EasterEggMessages.Length)]);
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

                if(context.Message.Content.Length <= 100 && context.Channel.GetChannelType() != ChannelType.DM)
                {/*
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



        #region Embed handlers
        //Check if message is an instagram link and has an embed or not
        public void InstagramEmbed(SocketCommandContext context)
        {
            List<Uri> urls = LinkSearch(context.Message.Content, "https://instagram.com/", false);

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

                            //A profile url looks like so https://www.instagram.com/[username]/ that creates 2 Segments, it is the only way to identify it
                            if (urls[i].Segments.Length == 2)
                            {
                                ///await InstagramAPI.ProfileEmbed(context.Channel, refer, urls[i]);
                            }
                            else if (urls[i].Segments[1][..^1] == "stories")
                            {
                                ///await InstagramAPI.StoryEmbed(context.Channel, refer, urls[i]);
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

        internal void TwitterEmbed(SocketCommandContext context)
        {
            List<Uri> urls = LinkSearch(context.Message.Content, "https://twitter.com/", true);

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

        private List<Uri> LinkSearch(string message, string baseURL, bool ignoreEmbedSuppress)
        {
            try
            {
                message = message.Replace("www.", "");

                if (message.Contains(baseURL))
                {
                    List<Uri> urls = new();

                    //Going throught the whole message to find all the instagram links
                    int startIndex = 0;
                    while (startIndex != -1)
                    {
                        //We check if there are any links left, one is expected
                        startIndex = message.IndexOf(baseURL, startIndex);

                        if (startIndex != -1)
                        {
                            //We cut off anything before the start of the link and replace embed supression characters
                            string beginningCut = message[startIndex..];

                            //And anything after the first space that ended the link
                            string url = beginningCut.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];

                            if(!ignoreEmbedSuppress && !url.Contains('<') && !url.Contains('>'))
                            {
                                url = url.Split('?')[0];
                                urls.Add(new Uri(url));
                            }
                            else if(ignoreEmbedSuppress)
                            {
                                url = url.Replace("<", "").Replace(">", "").Split('?')[0];
                                urls.Add(new Uri(url));
                            }

                            startIndex++;
                        }
                    }

                    return urls;
                }

            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs LinkSearch", ex.ToString());
            }

            return null;
        }
        #endregion



        #region Log to file method
        //For logging messages, errors, and messages to log files
        static StreamWriter LogFile_writer = null;
        public void LogToFile()
        {
            try
            {
                if (_logging.Logs.Count != 0 && LogFile_writer == null)
                {
                    string file_location = "Logs\\logs" + "[" + DateTime.Now.Year + "-" + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString()) + "-" + (DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString()) + "].txt";

                    using (LogFile_writer = File.AppendText(file_location)) foreach (string log in _logging.Logs.Select(n => n.Content)) LogFile_writer.WriteLine(log);

                    LogFile_writer = null;
                    _logging.Logs.Clear();
                }
            }
            catch (Exception ex)
            {
                _logging.Error("ProgramFunctions.cs LogtoFile", ex.ToString());
            }
        }
        #endregion

        #region Check internet connection
        //Testing connection by pinging google, it is quite a problem if that's down too
        public static bool Connection()
        {
            try
            {
                if (new Ping().Send("google.com", 1000, new byte[32], new PingOptions()).Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }
        #endregion



        #region File and folder check
        //Check if folders for long term storage exist
        public void Check_Folders()
        {
            List<string> logs = new();
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Logs"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Logs");
                logs.Add("Logs folder created!");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets");
                logs.Add("Assets folder created!");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets\\Commands"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets\\Commands");
                logs.Add("Commands folder created!");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets\\Data"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets\\Data");
                logs.Add("Data folder created!");
            }

            if (logs.Count != 0)
            {
                _logging.Log(string.Join('\n', logs));
            }
        }
        #endregion



        #region Load servers method
        //Setting the list of servers we currently have in the database
        /*public static void ServerList()
        {
            var list = AllServerSetting();

            if (list.Count > 0)
            {
                foreach (var server in list)
                {
                    Global.servers.Add(server.ServerId, new Server(server));
                }
            }
        }*/
        #endregion



        #region Before closing method
        //Things to do when app is closing
        //3 second time limit to event by default
        public void Closing(object sender, EventArgs e)
        {
            _logging.Log("Application closing...");
            LogToFile();
        }
        #endregion
    }
}
