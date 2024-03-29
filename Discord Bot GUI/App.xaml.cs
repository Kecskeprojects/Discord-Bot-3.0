﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        private Thread twitchThread;
        private Thread biasUpdateThread;
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

            using IServiceScope scope = services.CreateScope();
            ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();

            MainWindow mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            logger.Log("App started!");

            coreLogic.CheckFolders();

            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(coreLogic.Closing);

            YoutubeAPI.KeyReset(config.Youtube_API_Keys);

            twitchThread = new Thread(async () =>
            {
                await services.GetService<ITwitchAPI>().Start();
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
                using IServiceScope scope = services.CreateScope();
                ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();

                coreLogic.Closing();
                base.OnExit(e);
            }
            catch (Exception) { }
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {

            try
            {
                using IServiceScope scope = services.CreateScope();
                ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();

                coreLogic.Closing();
                base.OnSessionEnding(e);
            }
            catch (Exception) { }
        }

        //Main Bot Startup Logic
        public async Task RunBotAsync()
        {
            if (!Global.Connection())
            {
                return;
            }

            client.Log += ClientLog;

            await RegisterCommandsAsync();
            await RegisterInteractionsAsync();

            if (string.IsNullOrEmpty(config.Token))
            {
                logger.Error("App.xaml.cs RunBotAsync", "Bot cannot start without a valid token, fill out the proper fields in the config file!");
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
                using IServiceScope scope = services.CreateScope();
                ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                ICoreToDiscordCommunication discordCommunication = scope.ServiceProvider.GetService<ICoreToDiscordCommunication>();
                if (client.LoginState == LoginState.LoggedOut)
                {
                    logger.Log("Attempting to re-connect bot after disconnect.");
                    await RunBotAsync();
                }

                //Logic to be done once a day

                //Do at GMT+0 midnight every day
                if (DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute == 0)
                {
                    Logging.ClearWindowLog();
                }

                //Do at GMT+0 8 am every day
                if (DateTime.UtcNow.Hour == 8 && DateTime.UtcNow.Minute == 0)
                {
                    await discordCommunication.SendBirthdayMessages();

                    YoutubeAPI.KeyReset(config.Youtube_API_Keys);
                    logger.Log("Youtube keys reset!");

                    //Only on a monday
                    if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
                    {
                        IBiasDatabaseService biasDatabaseService = scope.ServiceProvider.GetService<IBiasDatabaseService>();
                        biasUpdateThread = new Thread(async () =>
                        {
                            await biasDatabaseService.RunUpdateBiasDataAsync();
                        });
                        biasUpdateThread.Start();
                    }
                }

                coreLogic.LogToFile();

                await discordCommunication.SendReminders();
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
                if (arg.Exception == null)
                {
                    logger.Client(arg.ToString());
                    return Task.CompletedTask;
                }

                switch (arg.Exception.Message)
                {
                    case "Server requested a reconnect":
                    case "Unable to connect to the remote server":
                        {
                            logger.Client($"{arg.Exception.Message}!");
                            break;
                        }
                    case "WebSocket connection was closed":
                    case "WebSocket session expired":
                    case "A task was canceled.":
                        {
                            string message = arg.Exception.Message;
                            if (message.EndsWith('.'))
                            {
                                message = message[..^1];
                            }

                            logger.Client($"{message}!", ConsoleOnly: true);
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
            catch (Exception ex)
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
            commands.CommandExecuted += HandleCommandExecutionAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        //Watching Interactions
        public async Task RegisterInteractionsAsync()
        {
            client.InteractionCreated += HandleInteractionAsync;
            interactions.InteractionExecuted += HandleInteractionExecutionAsync;
            await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        //Handling commands and special cases
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            try
            {
                if (string.IsNullOrEmpty(arg.Content))
                {
                    return;
                }

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

                using IServiceScope scope = services.CreateScope();
                ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                ICoreToDiscordCommunication discordCommunication = scope.ServiceProvider.GetService<ICoreToDiscordCommunication>();

                //If message is not private message, and the server is not in our database, add it
                ServerResource server = null;
                if (context.Channel.GetChannelType() != ChannelType.DM)
                {
                    server = await coreLogic.GetServerAsync(context.Guild.Id, context.Guild.Name);
                    if (server == null)
                    {
                        return;
                    }
                }

                if (context.Message.HasCharPrefix('!', ref argPos) || context.Message.HasCharPrefix('.', ref argPos))
                {
                    Discord.Commands.IResult result = await commands.ExecuteAsync(context, argPos, services);
                }
                else if (context.Channel.GetChannelType() != ChannelType.DM && Global.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id, false))
                {
                    if (context.Message.HasCharPrefix('+', ref argPos) || context.Message.HasCharPrefix('-', ref argPos))
                    {
                        //self roles
                        await discordCommunication.SelfRole(context);
                    }

                    await context.Message.DeleteAsync();
                }
                else
                {
                    //Response to mention
                    if (context.Message.Content.Contains(client.CurrentUser.Mention) || context.Message.Content.Contains(client.CurrentUser.Mention.Remove(2, 1)))
                    {
                        await discordCommunication.GreetAsync(context);
                    }

                    await discordCommunication.FeatureChecksAsync(context);

                    //Make embed independently from main thread
                    if (config.Enable_Instagram_Embed)
                    {
                        coreLogic.InstagramEmbed(context.Message.Content, context.Message.Id, context.Channel.Id, context.Guild?.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs HandleCommandAsync", ex.ToString());
            }
        }

        private async Task HandleCommandExecutionAsync(Optional<CommandInfo> info, ICommandContext context, Discord.Commands.IResult result)
        {
            switch (result.Error)
            {
                case null:
                    break;
                case CommandError.UnmetPrecondition:
                    {
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;
                    }
                case CommandError.UnknownCommand:
                    {
                        if (context.Channel.GetChannelType() != ChannelType.DM)
                        {
                            using IServiceScope scope = services.CreateScope();
                            ICoreToDiscordCommunication discordCommunication = scope.ServiceProvider.GetService<ICoreToDiscordCommunication>();
                            await discordCommunication.CustomCommands(context);
                        }
                        break;
                    }
                default:
                    {
                        logger.Warning("App.xaml.cs HandleCommandExecutionAsync", result.ErrorReason);
                        break;
                    }
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

        private Task HandleInteractionExecutionAsync(ICommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
        {
            switch (result.Error)
            {
                case null:
                    break;
                default:
                    {
                        logger.Warning("App.xaml.cs HandleInteractionExecutionAsync", result.ErrorReason);
                        break;
                    }
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
