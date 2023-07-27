using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Discord_Bot.Assets;
using Discord_Bot.Logger;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System.Reflection;
using System.Timers;

namespace Discord_Bot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _services;
        private Config _config;
        private Logging _logging;
        private static DiscordSocketClient _client;
        private InteractionService _interactions;
        private CommandService _commands;
        private MainLogic _mainLogic;

        #region Main methods
        //The main program, runs even if the bot crashes, and restarts it
        protected override async void OnStartup(StartupEventArgs e)
        {
            _config = new Config();
            _logging = new Logging();
            _mainLogic = new MainLogic(_logging);
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

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_interactions)
                .AddSingleton(_logging)
                .AddSingleton(_config);
            _services = ServiceBuilder.BuildService(collection, _config);

            var mainWindow = _services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            _logging.Log("App started!");

            _mainLogic.Check_Folders();

            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(_mainLogic.Closing);

            Timer aTimer = new(60000) { AutoReset = true }; //1 minute
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();

            await RunBotAsync();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            //Implement StartupFunctions.Closing

            base.OnExit(e);
        }


        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            //Implement StartupFunctions.Closing

            base.OnSessionEnding(e);
        }


        //Main Bot Startup Logic
        public async Task RunBotAsync()
        {
            if (!Global.Connection()) return;
            //Might not be needed
            //StartupFunctions.ServerList();

            //YoutubeAPI.KeyReset();

            _client.Log += ClientLog;

            await RegisterCommandsAsync();
            await RegisterInteractionsAsync();

            if (string.IsNullOrEmpty(_config.Token))
            {
                _logging.Error("Program.cs RunBotAsync", "Bot cannot start without a valid token, fill out it's filled in the config!");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, _config.Token);

            await _client.StartAsync();

            _client.Disconnected += OnWebsocketDisconnect;
            _client.Ready += OnWebSocketReady;

            /*new Thread(() =>
            {
                TwitchAPI.Twitch(_client);
            }).Start();*/

            //Instagram embed function
            //await InstagramAPI.Startup();

            _logging.Log("Bot started!");
        }


        //Repeated operations
        private static int minutes_count = 0;
        public async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if(_client.LoginState == LoginState.LoggedOut)
            {
                //Start a modified login function
            }
            //Database backup function
            if (minutes_count == 1440) minutes_count = 0;
            minutes_count++;

            //Youtube api key reset function
           // if (DateTime.Now.Hour == 10 && DateTime.Now.Minute == 0) YoutubeAPI.KeyReset();

            //Logging function
            _mainLogic.LogToFile();

            //Reminder function
            await _mainLogic.ReminderCheck(_client);
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
                            _logging.Client($"{arg.Exception.Message}!");
                            break;
                        }
                    case "WebSocket connection was closed":
                    case "WebSocket session expired":
                        {
                            _logging.Warning("Program.cs ClientLog", $"{arg.Exception.Message}!", ConsoleOnly: true);
                            _logging.Warning("Program.cs ClientLog", arg.Exception.ToString(), LogOnly: true);
                            break;
                        }
                    default:
                        {
                            _logging.Error("Program.cs ClientLog", arg.Exception.ToString());
                            break;
                        }
                }
            }
            else
            {
                _logging.Client(arg.ToString());
            }
            return Task.CompletedTask;
        }
        #endregion


        #region Websocket events
        private Task OnWebSocketReady()
        {
            _client.Rest.CurrentUser.UpdateAsync().Wait();
            _logging.Log("Current user data updated!");
            return Task.CompletedTask;
        }


        private Task OnWebsocketDisconnect(Exception arg)
        {
            _logging.Log("Disconnect handled!");
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
                    _logging.Mes_Other(arg.Content, (arg.Channel as SocketGuildChannel).Guild.Name);
                    return;
                }
                else
                {
                    _logging.Mes_Other(arg.Content);
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
                _logging.Mes_User(message.Content, context.Guild.Name);
            }
            else
            {
                _logging.Mes_User(message.Content);
            }

            //If message is not private message, and the server is not on the list, add it to the database and the list
            if (message.Channel.GetChannelType() != ChannelType.DM && !Global.servers.ContainsKey(context.Guild.Id))
            {
                /*
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
                */
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
                            await _mainLogic.CustomCommands(context);
                        }
                    }
                    else
                    {
                        if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);

                        _logging.Warning("Program.cs HandleCommandAsync", result.ErrorReason);
                    }
                }
            }
            else if (Global.TwitterChecker && message.HasStringPrefix(".twt", ref argPos))
            {
                _mainLogic.TwitterEmbed(context);
            }
            else if (message.Channel.GetChannelType() == ChannelType.Text && Global.servers[context.Guild.Id].RoleChannel == context.Channel.Id)
            {
                if (message.HasCharPrefix('+', ref argPos) || message.HasCharPrefix('-', ref argPos))
                {
                    //self roles
                    await _mainLogic.SelfRole(context);
                }

                await context.Message.DeleteAsync();
            }
            else
            {
                //Response to mention
                if (message.Content.Contains(_client.CurrentUser.Mention) || message.Content.Contains(_client.CurrentUser.Mention.Remove(2, 1)))
                {
                    /*
                    var list = DBFunctions.AllGreeting();
                    if (list.Count > 0)
                    {
                        await message.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].URL);
                    }
                    */
                }

                //Responses to triggers
                await _mainLogic.FeatureChecks(context);

                //Make embed independently from main thread
                if (Global.InstagramChecker)
                {
                    _mainLogic.InstagramEmbed(context);
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
        #endregion Main methods
    }
}
