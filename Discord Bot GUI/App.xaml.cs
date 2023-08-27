using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Discord_Bot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Variables
        private IServiceProvider _services;
        private Config _config;
        private Logging _logging;
        private static DiscordSocketClient _client;
        private InteractionService _interactions;
        private CommandService _commands;
        private CoreLogic _coreLogic;
        private IServerService serverService;
        private IGreetingService greetingService;
        #endregion

        #region Main methods
        //The main program, runs even if the bot crashes, and restarts it
        protected override async void OnStartup(StartupEventArgs e)
        {
            Timer aTimer = new(60000) { AutoReset = true }; //1 minute
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();

            _services = ServiceBuilder.BuildService();
            _config = _services.GetService<Config>();
            _logging = _services.GetService<Logging>();
            _client = _services.GetService<DiscordSocketClient>();
            _interactions = _services.GetService<InteractionService>();
            _commands = _services.GetService<CommandService>();
            _coreLogic = _services.GetService<CoreLogic>();
            serverService = _services.GetService<IServerService>();
            greetingService = _services.GetService<IGreetingService>();
            MainWindow mainWindow = _services.GetRequiredService<MainWindow>();

            mainWindow.Show();

            _logging.Log("App started!");

            _coreLogic.Check_Folders();

            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(_coreLogic.Closing);

            await RunBotAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _coreLogic.Closing();
            base.OnExit(e);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            _coreLogic.Closing();
            base.OnSessionEnding(e);
        }

        //Main Bot Startup Logic
        public async Task RunBotAsync()
        {
            if (!Global.Connection()) return;

            _client.Log += ClientLog;

            await RegisterCommandsAsync();
            await RegisterInteractionsAsync();

            if (string.IsNullOrEmpty(_config.Token))
            {
                _logging.Error("App.xaml.cs RunBotAsync", "Bot cannot start without a valid token, fill out it's filled in the config!");
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
            if (_client.LoginState == LoginState.LoggedOut)
            {
                await RunBotAsync();
            }

            //Logic to be done once a day
            if (minutes_count == 1440)
            {
                minutes_count = 0;
            }
            minutes_count++;

            //Youtube api key reset function
            //if (DateTime.Now.Hour == 10 && DateTime.Now.Minute == 0) YoutubeAPI.KeyReset();

            _coreLogic.LogToFile();

            await _coreLogic.ReminderCheck(_client);
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
                    case "A task was canceled":
                        {
                            _logging.Warning("App.xaml.cs ClientLog", $"{arg.Exception.Message}!", ConsoleOnly: true);
                            _logging.Warning("App.xaml.cs ClientLog", arg.Exception.ToString(), LogOnly: true);
                            break;
                        }
                    default:
                        {
                            _logging.Error("App.xaml.cs ClientLog", arg.Exception.ToString());
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

            SocketUserMessage message = arg as SocketUserMessage;
            SocketCommandContext context = new(_client, message);
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
            ServerResource server = null;
            if (message.Channel.GetChannelType() != ChannelType.DM)
            {
                server = await serverService.GetByDiscordIdAsync(context.Guild.Id);
                if (server == null)
                {
                    await serverService.AddServerAsync(context.Guild.Id);
                    server = await serverService.GetByDiscordIdAsync(context.Guild.Id);

                    if (server == null)
                    {
                        _logging.Log($"{context.Guild.Name} could not be added to list!");
                    }
                }
            }

            if (message.HasCharPrefix('!', ref argPos))
            {
                Discord.Commands.IResult result = await _commands.ExecuteAsync(context, argPos, _services);

                //In case there is no such hard coded command, check the list of custom commands
                if (!result.IsSuccess)
                {
                    if (result.ErrorReason == "Unknown command.")
                    {
                        if (message.Channel.GetChannelType() == ChannelType.Text)
                        {
                            await _coreLogic.CustomCommands(context);
                            return;
                        }
                    }

                    if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);

                    _logging.Warning("App.xaml.cs HandleCommandAsync", result.ErrorReason);
                }
            }
            else if (Global.TwitterChecker && message.HasStringPrefix(".twt", ref argPos))
            {
                _coreLogic.TwitterEmbed(context);
            }
            else if (message.Channel.GetChannelType() == ChannelType.Text && Global.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id))
            {
                if (message.HasCharPrefix('+', ref argPos) || message.HasCharPrefix('-', ref argPos))
                {
                    //self roles
                    await _coreLogic.SelfRole(context);
                }

                await context.Message.DeleteAsync();
            }
            else
            {
                //Response to mention
                if (message.Content.Contains(_client.CurrentUser.Mention) || message.Content.Contains(_client.CurrentUser.Mention.Remove(2, 1)))
                {
                    List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
                    if (list.Count > 0)
                    {
                        await message.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].Url);
                    }
                }

                //Responses to triggers
                await _coreLogic.FeatureChecks(context);

                //Make embed independently from main thread
                if (Global.InstagramChecker)
                {
                    _coreLogic.InstagramEmbed(context);
                }
            }
        }

        //Watching Interactions
        public async Task RegisterInteractionsAsync()
        {
            _client.InteractionCreated += HandleInteractionAsync;
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        //Handling Interactions
        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            SocketInteractionContext context = new(_client, arg);
            await _interactions.ExecuteCommandAsync(context, _services);
        }
        #endregion Main methods
    }
}
