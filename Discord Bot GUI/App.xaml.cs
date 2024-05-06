using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Discord_Bot
{
    public partial class App : Application
    {
        #region Variables
        private IServiceProvider services;
        private Logging logger;
        private Thread twitchThread;
        private Thread biasUpdateThread;
        private System.Timers.Timer mainTimer;
        private BotMain Bot;
        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            mainTimer = new(60000) { AutoReset = true }; //1 minute
            mainTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mainTimer.Start();

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(Closing);

            services = ServiceBuilder.BuildService();
            logger = services.GetService<Logging>();

            using (IServiceScope scope = services.CreateScope())
            {
                MainWindow mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
                Config config = scope.ServiceProvider.GetService<Config>();

                mainWindow.Show();

                logger.Log("App started!");

                ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                coreLogic.CheckFolders();

                YoutubeAPI.KeyReset(config.Youtube_API_Keys);
            }

            StartTwitchThread();

            Bot = services.GetRequiredService<BotMain>();
            await Bot.RunBotAsync();
        }

        public async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                using (IServiceScope scope = services.CreateScope())
                {
                    ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                    ICoreToDiscordCommunication discordCommunication = scope.ServiceProvider.GetService<ICoreToDiscordCommunication>();
                    DiscordSocketClient client = scope.ServiceProvider.GetService<DiscordSocketClient>();
                    Config config = scope.ServiceProvider.GetService<Config>();

                    if (client.LoginState == LoginState.LoggedOut)
                    {
                        logger.Log("Attempting to re-connect bot after disconnect.");
                        await Bot.RunBotAsync();
                    }

                    //Do at GMT+0 midnight every day
                    if (DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute == 0)
                    {
                        Logging.ClearWindowLog();
                    }

                    //Do at GMT+0 6 am every day
                    if (DateTime.UtcNow.Hour == 6 && DateTime.UtcNow.Minute == 0)
                    {
                        await discordCommunication.SendBirthdayMessages();

                        YoutubeAPI.KeyReset(config.Youtube_API_Keys);
                        logger.Log("Youtube keys reset!");

                        //Only on a monday
                        if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
                        {
                            StartBiasUpdate();
                        }
                    }

                    coreLogic.LogToFile();

                    await discordCommunication.SendReminders();
                }
            }
            catch (Exception ex)
            {
                logger.Error("App.xaml.cs OnTimedEvent", ex);
            }
        }

        private void StartTwitchThread()
        {
            twitchThread = new Thread(async () =>
            {
                using (IServiceScope scope = services.CreateScope())
                {
                    ITwitchAPI twitchAPI = scope.ServiceProvider.GetService<ITwitchAPI>();
                    await twitchAPI.Start();
                }
            });
            twitchThread.Start();
        }

        private void StartBiasUpdate()
        {
            _ = Task.Run(async () =>
            {
                using (IServiceScope scope = services.CreateScope())
                {
                    IBiasDatabaseService biasDatabaseService = scope.ServiceProvider.GetService<IBiasDatabaseService>();
                    await biasDatabaseService.RunUpdateBiasDataAsync();
                }
            });
            biasUpdateThread.Start();
        }

        #region OnClose logic
        protected override void OnExit(ExitEventArgs e)
        {
            Closing();
            base.OnExit(e);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            Closing();
            base.OnSessionEnding(e);
        }

        //Things to do when app is closing
        //3 second time limit to event by default
        public async void Closing()
        {
            try
            {
                await BrowserService.CloseBrowser();
                logger.Log("Application closing...");

                using (IServiceScope scope = services.CreateScope())
                {
                    ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                    coreLogic.LogToFile();
                }
            }
            catch (Exception) { }
        }

        public async void Closing(object sender, EventArgs e)
        {
            try
            {
                await BrowserService.CloseBrowser();
                logger.Log("Application closing...");

                using (IServiceScope scope = services.CreateScope())
                {
                    ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                    coreLogic.LogToFile();
                }
            }
            catch (Exception) { }
        }
        #endregion
    }
}
