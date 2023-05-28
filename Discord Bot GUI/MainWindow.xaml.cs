using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Discord_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Main methods
        //The main program, runs even if the bot crashes, and restarts it
        public MainWindow()
        {
            StartupFunctions.Check_Folders();

            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(StartupFunctions.Closing);

            //Constant timer, only stopping when the Bot's process stops
            System.Timers.Timer aTimer = new(60000) { AutoReset = true }; //1 minute

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            InitializeComponent();

            while (true)
            {
                if (StartupFunctions.Connection())
                {
                    aTimer.Start();

                    Logging.Log("Application starting...");

                    try
                    {
                        RunBotAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Logging.Error("Program.cs Main", ex.ToString());
                    }

                    aTimer.Stop();
                }
                //Waiting 1 minute before checking connection again
                Thread.Sleep(60000);
            }
        }



        private static DiscordSocketClient _client;
        private InteractionService _interactions;
        private CommandService _commands;
        private IServiceProvider _services;



        //Main Service
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(
                //GatewayIntents.GuildPresences | GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites  NOT USED GATEWAY INTENTS
                new DiscordSocketConfig()
                {
                    GatewayIntents = GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageTyping |
                                                            GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildMembers |
                                                            GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping |
                                                            GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildWebhooks |
                                                            GatewayIntents.MessageContent,
                    LogLevel = LogSeverity.Info
                });
            _interactions = new InteractionService(_client, new InteractionServiceConfig() { DefaultRunMode = Discord.Interactions.RunMode.Async });
            _commands = new CommandService(new CommandServiceConfig() { DefaultRunMode = Discord.Commands.RunMode.Async });
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_interactions)
                .BuildServiceProvider();
            StartupFunctions.DBCheck();
            StartupFunctions.ServerList();

            YoutubeAPI.KeyReset();

            _client.Log += ClientLog;

            await RegisterCommandsAsync();
            await RegisterInteractionsAsync();

            if (string.IsNullOrEmpty(Global.Config.Token))
            {
                Logging.Error("Program.cs RunBotAsync", "Bot cannot start without a valid token, fill out it's filled in the config!");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, Global.Config.Token);

            await _client.StartAsync();

            _client.Disconnected += OnWebsocketDisconnect;
            _client.Ready += OnWebSocketReady;

            new Thread(() =>
            {
                TwitchAPI.Twitch(_client);
            }).Start();

            //Instagram embed function
            await InstagramAPI.Startup();

            await Task.Delay(-1);
        }



        //Repeated operations
        static int minutes_count = 0;
        public static async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Database backup function
            if (minutes_count == 1440) minutes_count = 0;
            if (minutes_count == 0) ProgramFunctions.DatabaseBackup();
            minutes_count++;

            //Youtube api key reset function
            if (DateTime.Now.Hour == 10 && DateTime.Now.Minute == 0) YoutubeAPI.KeyReset();

            //Logging function
            ProgramFunctions.LogToFile();

            //Reminder function
            await ProgramFunctions.ReminderCheck(_client);
        }
        #endregion



        #region Client logging
        //Client Messages
        private Task ClientLog(LogMessage arg)
        {
            if (arg.Exception != null)
            {
                switch (arg.Exception.Message)
                {
                    case "Server requested a reconnect":
                        {
                            Logging.Client($"{arg.Exception.Message}!");
                            break;
                        }
                    case "WebSocket connection was closed":
                    case "WebSocket session expired":
                        {
                            Logging.Warning("Program.cs ClientLog", $"{arg.Exception.Message}!", ConsoleOnly: true);
                            Logging.Warning("Program.cs ClientLog", arg.Exception.ToString(), LogOnly: true);
                            break;
                        }
                    default:
                        {
                            Logging.Error("Program.cs ClientLog", arg.Exception.ToString());
                            break;
                        }
                }
            }
            else
            {
                Logging.Client(arg.ToString());
            }
            return Task.CompletedTask;
        }
        #endregion



        #region Websocket events
        private Task OnWebSocketReady()
        {
            _client.Rest.CurrentUser.UpdateAsync().Wait();
            Logging.Log("Current user data updated!");
            return Task.CompletedTask;
        }

        private Task OnWebsocketDisconnect(Exception arg)
        {
            Logging.Log("Disconnect handled!");
            return Task.CompletedTask;
        }
        #endregion



        #region Message handling
        //Watching Messages
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }



        //Handling commands and special cases
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            //In case the message was a system message (eg. the message seen when someone a pin is made), a webhook's or a bot's message, the function stops
            if (arg.Source == MessageSource.System || arg.Source == MessageSource.Webhook || arg.Source == MessageSource.Bot)
            {
                if (arg.Content.Length < 1) return;
                else if (arg.Channel.GetChannelType() == ChannelType.Text)
                {
                    Logging.Mes_Other(arg.Content, (arg.Channel as SocketGuildChannel).Guild.Name);
                    return;
                }
                else
                {
                    Logging.Mes_Other(arg.Content);
                    return;
                }
            }

            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            int argPos = 0;


            //Check if the message is an embed or not
            if (message.Content.Length < 1) return;
            else if (message.Channel.GetChannelType() == ChannelType.Text)
            {
                Logging.Mes_User(message.Content, context.Guild.Name);
            }
            else
            {
                Logging.Mes_User(message.Content);
            }

            //If message is not private message, and the server is not on the list, add it to the database and the list
            if (message.Channel.GetChannelType() != ChannelType.DM && !Global.servers.ContainsKey(context.Guild.Id))
            {
                int affected = DBFunctions.AddNewServer(context.Guild.Id);

                if (affected > 0)
                {
                    Global.servers.Add(context.Guild.Id, new Server(context.Guild.Id));

                    Logging.Log($"{context.Guild.Name} added to the server list!");
                }
                else
                {
                    Logging.Log($"{context.Guild.Name} could not be added to list!");
                }
            }


            if (message.HasCharPrefix('!', ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                //In case there is no such hard coded command, check the list of custom commands
                if (!result.IsSuccess)
                {
                    if (result.ErrorReason == "Unknown command.")
                    {
                        if (message.Channel.GetChannelType() == ChannelType.Text)
                        {
                            await ProgramFunctions.CustomCommands(context);
                        }
                    }
                    else
                    {
                        if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);

                        Logging.Warning("Program.cs HandleCommandAsync", result.ErrorReason);
                    }
                }
            }
            else if (Global.TwitterChecker && message.HasStringPrefix(".twt", ref argPos))
            {
                ProgramFunctions.TwitterEmbed(context);
            }
            else if (message.Channel.GetChannelType() == ChannelType.Text && Global.servers[context.Guild.Id].RoleChannel == context.Channel.Id)
            {
                if (message.HasCharPrefix('+', ref argPos) || message.HasCharPrefix('-', ref argPos))
                {
                    //self roles
                    await ProgramFunctions.SelfRole(context);
                }

                await context.Message.DeleteAsync();
            }
            else
            {
                //Response to mention
                if (message.Content.Contains(_client.CurrentUser.Mention) || message.Content.Contains(_client.CurrentUser.Mention.Remove(2, 1)))
                {
                    var list = DBFunctions.AllGreeting();
                    if (list.Count > 0)
                    {
                        await message.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].URL);
                    }
                }

                //Responses to triggers
                await ProgramFunctions.FeatureChecks(context);

                //Make embed independently from main thread
                if (Global.InstagramChecker)
                {
                    ProgramFunctions.InstagramEmbed(context);
                }
            }
        }
        #endregion



        #region Interaction handling
        //Watching Interactions
        public async Task RegisterInteractionsAsync()
        {
            _client.InteractionCreated += HandleInteractionAsync;
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }


        //Handling Interactions
        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(_client, arg);
            await _interactions.ExecuteCommandAsync(context, _services);
        }
        #endregion
    }
}
