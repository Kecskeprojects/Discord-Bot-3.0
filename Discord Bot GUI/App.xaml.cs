﻿using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors;
using Discord_Bot.Services;
using Discord_Bot.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Discord_Bot;
//Todo: Investigate if the various API and other Services can be cleaned up, split up, and also if a base class can be or is needed for them, perhaps multiple base classes

//Additional Feature Ideas:
//Halloween game
//Blackjack game
//Christmas game
//Counting messages/files of a user as a command
public partial class App : Application
{
    #region Variables
    private IServiceProvider services;
    private BotLogger logger;
    private BrowserService browserService;
    private Thread twitchThread;
    private System.Timers.Timer mainTimer;
    private BotMain BotService;
    #endregion

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

    private void Closing(object sender, EventArgs e)
    {
        Closing();
    }

    private async void Closing()
    {
        //3 second time limit to event before app closes
        try
        {
            await browserService.CloseBrowser();
            logger.Log("Application closing...");
            logger.LogToFile();
        }
        catch (Exception) { }
    }
    #endregion

    protected override async void OnStartup(StartupEventArgs e)
    {
        mainTimer = new(60000)//1 minute
        {
            AutoReset = true
        };
        mainTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        mainTimer.Start();

        AppDomain.CurrentDomain.ProcessExit += new EventHandler(Closing);

        services = Discord_Bot.Startup.Run();
        logger = services.GetService<BotLogger>();
        browserService = services.GetService<BrowserService>();

        using (IServiceScope scope = services.CreateScope())
        {
            BotWindow botWindow = scope.ServiceProvider.GetRequiredService<BotWindow>();

            botWindow.Show();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Logs")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
                logger.Log("Logs folder created!");
            }

            logger.Log("App started!");

            Config config = scope.ServiceProvider.GetService<Config>();
            YoutubeAPI.KeyReset(config.Youtube_API_Keys);
        }

        twitchThread = new Thread(new ThreadStart(StartTwitchMonitor));
        twitchThread.Start();

        BotService = services.GetRequiredService<BotMain>();
        await BotService.RunBotAsync();
    }

    private async void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        try
        {
            using (IServiceScope scope = services.CreateScope())
            {
                DiscordSocketClient client = scope.ServiceProvider.GetService<DiscordSocketClient>();
                if (client.LoginState == LoginState.LoggedOut)
                {
                    logger.Log("Attempting to re-connect bot after disconnect.");
                    await BotService.RunBotAsync();
                }

                //Do at GMT+0 midnight every day
                if (DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute == 0)
                {
                    BotWindow.ClearWindowLog();
                }

                //WeeklyPollFeature weeklyPollFeature = scope.ServiceProvider.GetService<WeeklyPollFeature>();
                //await weeklyPollFeature.Run();

                //Do at GMT+0 6 am every day
                if (DateTime.UtcNow.Hour == 6 && DateTime.UtcNow.Minute == 0)
                {
                    BirthdayFeature birthdayFeature = scope.ServiceProvider.GetService<BirthdayFeature>();
                    await birthdayFeature.Run();

                    WeeklyPollFeature weeklyPollFeature = scope.ServiceProvider.GetService<WeeklyPollFeature>();
                    await weeklyPollFeature.Run();

                    Config config = scope.ServiceProvider.GetService<Config>();
                    YoutubeAPI.KeyReset(config.Youtube_API_Keys);
                    logger.Log("Youtube keys reset!");

                    //Only on a monday
                    if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
                    {
                        _ = StartBiasScraping();
                    }
                }

                ReminderFeature reminderFeature = scope.ServiceProvider.GetService<ReminderFeature>();
                await reminderFeature.Run();

                UnmuteFeature unmuteFeature = scope.ServiceProvider.GetService<UnmuteFeature>();
                await unmuteFeature.Run();

                logger.LogToFile();
            }
        }
        catch (Exception ex)
        {
            logger.Error("App.xaml.cs OnTimedEvent", ex);
        }
    }

    private async Task StartBiasScraping()
    {
        using (IServiceScope scope = services.CreateScope())
        {
            BiasScrapingProcessor biasScrapingProcessor = scope.ServiceProvider.GetService<BiasScrapingProcessor>();
            await biasScrapingProcessor.RunUpdateBiasDataAsync();
        }
    }

    private async void StartTwitchMonitor()
    {
        using (IServiceScope scope = services.CreateScope())
        {
            ITwitchAPI twitchAPI = scope.ServiceProvider.GetService<ITwitchAPI>();
            await twitchAPI.Start();
        }
    }
}
