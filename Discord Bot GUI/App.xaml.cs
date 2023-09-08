using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
        private IServiceProvider services;
        private Config config;
        private Logging logger;
        private static DiscordSocketClient client;
        private InteractionService interactions;
        private CommandService commands;
        private ICoreLogic coreLogic;
        private IServerService serverService;
        private IGreetingService greetingService;
        private MainWindow mainWindow;
        private Thread twitchThread;
        private static int minutesCount = 0;
        #endregion

        #region Main Methods
        //The main program, runs even if the bot crashes, and restarts it
        protected override async void OnStartup(StartupEventArgs e)
        {
            System.Timers.Timer aTimer = new(60000) { AutoReset = true }; //1 minute
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();

            services = ServiceBuilder.BuildService();
            config = services.GetService<Config>();
            logger = services.GetService<Logging>();
            client = services.GetService<DiscordSocketClient>();
            interactions = services.GetService<InteractionService>();
            commands = services.GetService<CommandService>();
            coreLogic = services.GetService<ICoreLogic>();
            serverService = services.GetService<IServerService>();
            greetingService = services.GetService<IGreetingService>();
            mainWindow = services.GetRequiredService<MainWindow>();

            mainWindow.Show();

            logger.Log("App started!");

            coreLogic.CheckFolders();

            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(coreLogic.Closing);

            YoutubeAPI.KeyReset(config.Youtube_API_Keys);

            twitchThread = new Thread(() =>
            {
                services.GetService<ITwitchAPI>().Start();
            });
            twitchThread.Start();

            await RunBotAsync();

            client.Disconnected += OnWebsocketDisconnect;
            client.Ready += OnWebSocketReady;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                coreLogic.Closing();
                base.OnExit(e);
            }
            catch (Exception) { }
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {

            try
            {
                coreLogic.Closing();
                base.OnSessionEnding(e);
            }
            catch (Exception) { }
        }

        //Main Bot Startup Logic
        public async Task RunBotAsync()
        {
            if (!Global.Connection()) return;

            client.Log += ClientLog;

            await RegisterCommandsAsync();
            await RegisterInteractionsAsync();

            if (string.IsNullOrEmpty(config.Token))
            {
                logger.Error("App.xaml.cs RunBotAsync", "Bot cannot start without a valid token, fill out it's filled in the config!");
                return;
            }

            await client.LoginAsync(TokenType.Bot, config.Token);

            await client.StartAsync();

            logger.Log("Bot started!");
        }

        //Repeated operations
        public async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (client.LoginState == LoginState.LoggedOut)
                {
                    logger.Log("Attempting to connect bot after disconnect.");
                    await RunBotAsync();
                }

                //Logic to be done once a day
                if (minutesCount == 1440)
                {
                    minutesCount = 0;
                }
                minutesCount++;

                //Youtube api key reset function
                if (DateTime.UtcNow.Hour == 8 && DateTime.UtcNow.Minute == 0)
                {
                    YoutubeAPI.KeyReset(config.Youtube_API_Keys);
                    logger.Log("Youtube keys reset!");
                }

                coreLogic.LogToFile();

                await coreLogic.ReminderCheck();
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs OnTimedEvent", ex.ToString());
            }
        }
        #endregion

        #region Client Logging
        //Client Messages
        private Task ClientLog(LogMessage arg)
        {
            try
            {
                if (arg.Exception != null)
                {
                    switch (arg.Exception.Message)
                    {
                        case "Server requested a reconnect":
                            {
                                logger.Client($"{arg.Exception.Message}!");
                                break;
                            }
                        case "WebSocket connection was closed":
                        case "WebSocket session expired":
                        case "A task was canceled":
                            {
                                logger.Warning("App.xaml.cs ClientLog", $"{arg.Exception.Message}!", ConsoleOnly: true);
                                logger.Warning("App.xaml.cs ClientLog", arg.Exception.ToString(), LogOnly: true);
                                break;
                            }
                        default:
                            {
                                logger.Error("App.xaml.cs ClientLog", arg.Exception.ToString());
                                break;
                            }
                    }
                }
                else
                {
                    logger.Client(arg.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs ClientLog", ex.ToString());
            }
            return Task.CompletedTask;
        }
        #endregion

        #region Discord Events
        private async Task OnWebSocketReady()
        {
            try
            {
                await client.Rest.CurrentUser.UpdateAsync();
                logger.Log("Current user data updated!");
            }
            catch(Exception ex)
            {
                logger.Error("App.xaml.cs OnWebSocketReady", ex.ToString());
            }
        }

        private Task OnWebsocketDisconnect(Exception arg)
        {
            try
            {
                logger.Log("Disconnect handled!");
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs OnWebSocketDisconnect", ex.ToString());
            }
            return Task.CompletedTask;
        }

        //Watching Messages
        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        //Watching Interactions
        public async Task RegisterInteractionsAsync()
        {
            client.InteractionCreated += HandleInteractionAsync;
            await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        //Handling commands and special cases
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            try
            {
                if (string.IsNullOrEmpty(arg.Content)) return;

                //In case the message was a system message (eg. the message seen when someone a pin is made), a webhook's or a bot's message, the function stops as it would cause an infinite loop
                if (arg.Source == MessageSource.System || arg.Source == MessageSource.Webhook || arg.Source == MessageSource.Bot)
                {
                    if (arg.Channel.GetChannelType() != ChannelType.DM)
                    {
                        logger.MesOther(arg.Content, (arg.Channel as SocketGuildChannel).Guild.Name);
                    }
                    else
                    {
                        logger.MesOther(arg.Content);
                    }
                    return;
                }

                SocketCommandContext context = new(client, arg as SocketUserMessage);
                int argPos = 0;

                //Check if the message is an embed or not
                if (context.Message.Channel.GetChannelType() != ChannelType.DM)
                {
                    logger.MesUser(context.Message.Content, context.Guild.Name);
                }
                else
                {
                    logger.MesUser(context.Message.Content);
                }

                //If message is not private message, and the server is not in our database, add it
                ServerResource server = null;
                if (context.Channel.GetChannelType() != ChannelType.DM)
                {
                    server = await serverService.GetByDiscordIdAsync(context.Guild.Id);
                    if (server == null)
                    {
                        await serverService.AddServerAsync(context.Guild.Id);
                        server = await serverService.GetByDiscordIdAsync(context.Guild.Id);

                        if (server == null)
                        {
                            logger.Log($"{context.Guild.Name} could not be added to list!");
                            return;
                        }
                    }
                }

                if (context.Message.HasCharPrefix('!', ref argPos))
                {
                    Discord.Commands.IResult result = await commands.ExecuteAsync(context, argPos, services);

                    //In case there is no such hard coded command, check the list of custom commands
                    if (!result.IsSuccess)
                    {
                        if (result.ErrorReason == "Unknown command.")
                        {
                            if (context.Channel.GetChannelType() != ChannelType.DM)
                            {
                                await coreLogic.CustomCommands(context.Guild.Id, context.Message.Content, context.Channel);
                                return;
                            }
                        }

                        if (result.Error.Equals(CommandError.UnmetPrecondition))
                            await context.Channel.SendMessageAsync(result.ErrorReason);

                        logger.Warning("App.xaml.cs HandleCommandAsync", result.ErrorReason);
                    }
                }
                else if (config.Enable_Twitter_Embed && context.Message.HasStringPrefix(".", ref argPos))
                {
                    await commands.ExecuteAsync(context, argPos, services);
                }
                else if (context.Channel.GetChannelType() != ChannelType.DM && Global.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id))
                {
                    if (context.Message.HasCharPrefix('+', ref argPos) || context.Message.HasCharPrefix('-', ref argPos))
                    {
                        //self roles
                        await coreLogic.SelfRole(context.Guild.Id, context.Message.Content, context.Channel, context.User);
                    }

                    await context.Message.DeleteAsync();
                }
                else
                {
                    //Response to mention
                    if (context.Message.Content.Contains(client.CurrentUser.Mention) || context.Message.Content.Contains(client.CurrentUser.Mention.Remove(2, 1)))
                    {
                        List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
                        if (!CollectionTools.IsNullOrEmpty(list))
                        {
                            await context.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].Url);
                        }
                    }

                    await coreLogic.FeatureChecks(context.Guild.Id, context.Message.Content, context.Channel);

                    //Make embed independently from main thread
                    if (config.Enable_Instagram_Embed)
                    {
                        coreLogic.InstagramEmbed(context.Message.Content, context.Message.Id, context.Channel.Id, context.Guild == null ? context.Guild.Id : null);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs HandleCommandAsync", ex.ToString());
            }
        }

        //Handling Interactions
        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            try
            {
                SocketInteractionContext context = new(client, arg);
                await interactions.ExecuteCommandAsync(context, services);
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs HandleInteractionAsync", ex.ToString());
            }
        }
        #endregion
    }
}
