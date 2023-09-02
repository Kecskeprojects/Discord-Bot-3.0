﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services;
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
        #endregion

        #region Main methods
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

            coreLogic.Check_Folders();

            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(coreLogic.Closing);

            //Todo: Reimplement TwitchAPI
            twitchThread = new Thread(() =>
            {
                services.GetService<ITwitchAPI>().Start();
            });
            twitchThread.Start();

            await RunBotAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            coreLogic.Closing();
            base.OnExit(e);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            coreLogic.Closing();
            base.OnSessionEnding(e);
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

            client.Disconnected += OnWebsocketDisconnect;
            client.Ready += OnWebSocketReady;

            //Todo:Reimplement instagram logic as much as possible
            //Instagram embed function
            //await InstagramAPI.Startup();

            logger.Log("Bot started!");
        }

        //Repeated operations
        private static int minutes_count = 0;
        public async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (client.LoginState == LoginState.LoggedOut)
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
            if (DateTime.UtcNow.Hour == 8 && DateTime.UtcNow.Minute == 0)
            {
                YoutubeAPI.KeyReset(config.Youtube_API_Keys);
                logger.Log("Youtube keys reset!");
            }

            coreLogic.LogToFile();

            await coreLogic.ReminderCheck(client);
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
            return Task.CompletedTask;
        }
        #endregion

        #region Websocket events
        private Task OnWebSocketReady()
        {
            client.Rest.CurrentUser.UpdateAsync().Wait();
            logger.Log("Current user data updated!");
            return Task.CompletedTask;
        }

        private Task OnWebsocketDisconnect(Exception arg)
        {
            logger.Log("Disconnect handled!");
            return Task.CompletedTask;
        }
        #endregion

        #region Message handling
        //Watching Messages
        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        //Handling commands and special cases
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            //In case the message was a system message (eg. the message seen when someone a pin is made), a webhook's or a bot's message, the function stops as it would cause an infinite loop
            if (arg.Source == MessageSource.System || arg.Source == MessageSource.Webhook || arg.Source == MessageSource.Bot)
            {
                if (arg.Content.Length < 1)
                {
                    return;
                }
                else if (arg.Channel.GetChannelType() == ChannelType.Text)
                {
                    logger.Mes_Other(arg.Content, (arg.Channel as SocketGuildChannel).Guild.Name);
                    return;
                }
                else
                {
                    logger.Mes_Other(arg.Content);
                    return;
                }
            }

            SocketUserMessage message = arg as SocketUserMessage;
            SocketCommandContext context = new(client, message);
            int argPos = 0;

            //Check if the message is an embed or not
            if (message.Content.Length < 1)
            {
                return;
            }
            else if (message.Channel.GetChannelType() == ChannelType.Text)
            {
                logger.Mes_User(message.Content, context.Guild.Name);
            }
            else
            {
                logger.Mes_User(message.Content);
            }

            //If message is not private message, and the server is not on our losts, add it to the database
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
                        logger.Log($"{context.Guild.Name} could not be added to list!");
                    }
                }
            }

            if (message.HasCharPrefix('!', ref argPos))
            {
                Discord.Commands.IResult result = await commands.ExecuteAsync(context, argPos, services);

                //In case there is no such hard coded command, check the list of custom commands
                if (!result.IsSuccess)
                {
                    if (result.ErrorReason == "Unknown command.")
                    {
                        if (message.Channel.GetChannelType() == ChannelType.Text)
                        {
                            await coreLogic.CustomCommands(context);
                            return;
                        }
                    }

                    if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);

                    logger.Warning("App.xaml.cs HandleCommandAsync", result.ErrorReason);
                }
            }
            else if (Global.TwitterChecker && message.HasStringPrefix(".twt", ref argPos))
            {
                coreLogic.TwitterEmbed(context);
            }
            else if (message.Channel.GetChannelType() == ChannelType.Text && Global.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id))
            {
                if (message.HasCharPrefix('+', ref argPos) || message.HasCharPrefix('-', ref argPos))
                {
                    //self roles
                    await coreLogic.SelfRole(context);
                }

                await context.Message.DeleteAsync();
            }
            else
            {
                //Response to mention
                if (message.Content.Contains(client.CurrentUser.Mention) || message.Content.Contains(client.CurrentUser.Mention.Remove(2, 1)))
                {
                    List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
                    if (list.Count > 0)
                    {
                        await message.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].Url);
                    }
                }

                //Responses to triggers
                await coreLogic.FeatureChecks(context);

                //Todo:Reimplement instagram logic as much as possible
                //Make embed independently from main thread
                //if (Global.InstagramChecker)
                //{
                //    coreLogic.InstagramEmbed(context);
                //}
            }
        }

        //Watching Interactions
        public async Task RegisterInteractionsAsync()
        {
            client.InteractionCreated += HandleInteractionAsync;
            await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        //Handling Interactions
        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            SocketInteractionContext context = new(client, arg);
            await interactions.ExecuteCommandAsync(context, services);
        }
        #endregion Main methods
    }
}
