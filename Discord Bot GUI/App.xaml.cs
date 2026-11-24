using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors;
using Discord_Bot.Services;
using Discord_Bot.Tools;
using Discord_Bot.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Timers;
using System.Windows;

namespace Discord_Bot;

public sealed partial class App : Application, IDisposable
{
    #region Variables
    //Set in OnStartup, which WPF runs before anything else in this class
    private IServiceProvider services = null!;
    private BotLogger logger = null!;
    private IBrowserService browserService = null!;
    private Thread twitchThread = null!;
    //Null until OnStartup creates them, and stays null when startup fails before that
    private System.Timers.Timer? mainTimer;
    private BotMain? botService;
    private int closed;
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

    private void Closing(object? sender, EventArgs e)
    {
        Closing();
    }

    private void Closing()
    {
        //OnExit, OnSessionEnding and ProcessExit can all fire, the cleanup only has to run once
        if (Interlocked.Exchange(ref closed, 1) == 1)
        {
            return;
        }

        //Otherwise the reconnect timer could log the bot back in while it is shutting down
        mainTimer?.Stop();

        //The process exits right after these events, so the cleanup is waited for, with a time limit
        //It runs on the thread pool, waiting on the UI thread for work that continues on the UI thread would deadlock
        try
        {
            _ = Task.Run(CloseAsync).Wait(TimeSpan.FromSeconds(3));
        }
        catch (Exception) { }
    }

    private async Task CloseAsync()
    {
        try
        {
            logger.Log("Application closing...");

            services.GetRequiredService<AudioStateStore>().CancelAll();

            //Without this the bot stays visible as online until Discord times the session out
            if (botService != null)
            {
                await botService.StopBotAsync();
            }

            await browserService.CloseBrowser();
        }
        catch (Exception ex)
        {
            logger.Error("App.xaml.cs CloseAsync", ex);
        }
        finally
        {
            logger.LogToFile();

            //Last, because StopBotAsync above still needs the client that BotMain disposes
            Dispose();
        }
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
        logger = services.GetRequiredService<BotLogger>();
        browserService = services.GetRequiredService<IBrowserService>();

        using (IServiceScope scope = services.CreateScope())
        {
            BotWindow botWindow = scope.ServiceProvider.GetRequiredService<BotWindow>();

            botWindow.Show();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Logs")))
            {
                _ = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
                logger.Log("Logs folder created!");
            }

            logger.Log("App started!");

            IConfig config = scope.ServiceProvider.GetRequiredService<IConfig>();
            scope.ServiceProvider.GetRequiredService<YoutubeKeyStore>().Reset(config.Youtube_API_Keys);
        }

        twitchThread = new Thread(new ThreadStart(StartTwitchMonitor));
        twitchThread.Start();

        botService = services.GetRequiredService<BotMain>();
        try
        {
            await botService.RunBotAsync();
        }
        catch (Exception ex)
        {
            //OnStartup is async void, so an exception escaping here would close the app without a log entry
            //A failed login leaves the client logged out, and the reconnect timer in OnTimedEvent tries again every minute
            logger.Error("App.xaml.cs OnStartup", ex);
        }
    }

    private async void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        try
        {
            using (IServiceScope scope = services.CreateScope())
            {
                //Read once, before the reconnect, which can take a while (until 2026-11-23 each check read the clock again after it)
                ScheduledTaskEnum due = ScheduleTools.DueAt(scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime);

                DiscordSocketClient client = scope.ServiceProvider.GetRequiredService<DiscordSocketClient>();
                if (client.LoginState == LoginState.LoggedOut && botService != null)
                {
                    logger.Log("Attempting to re-connect bot after disconnect.");
                    await botService.RunBotAsync();
                }

                if (due.HasFlag(ScheduledTaskEnum.ClearWindowLog))
                {
                    BotWindow.ClearWindowLog();
                }

                if (due.HasFlag(ScheduledTaskEnum.BirthdaysAndWeeklyPolls))
                {
                    BirthdayFeature birthdayFeature = scope.ServiceProvider.GetRequiredService<BirthdayFeature>();
                    _ = await birthdayFeature.Run();

                    WeeklyPollFeature weeklyPollFeature = scope.ServiceProvider.GetRequiredService<WeeklyPollFeature>();
                    _ = await weeklyPollFeature.Run();
                }

                if (due.HasFlag(ScheduledTaskEnum.BiasScraping))
                {
                    _ = StartBiasScraping();
                }

                //YouTube resets the quotas at midnight Pacific time: 08:00 UTC in winter, 07:00 in summer.
                //Refilled any earlier, a search before that reset would drop the used-up keys again until the next refill
                if (due.HasFlag(ScheduledTaskEnum.YoutubeKeyRefill))
                {
                    IConfig config = scope.ServiceProvider.GetRequiredService<IConfig>();
                    scope.ServiceProvider.GetRequiredService<YoutubeKeyStore>().Reset(config.Youtube_API_Keys);
                    logger.Log("Youtube keys reset!");
                }

                ReminderFeature reminderFeature = scope.ServiceProvider.GetRequiredService<ReminderFeature>();
                _ = await reminderFeature.Run();

                UnmuteFeature unmuteFeature = scope.ServiceProvider.GetRequiredService<UnmuteFeature>();
                _ = await unmuteFeature.Run();

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
            BiasScrapingProcessor biasScrapingProcessor = scope.ServiceProvider.GetRequiredService<BiasScrapingProcessor>();
            await biasScrapingProcessor.RunUpdateBiasDataAsync();
        }
    }

    //Runs on its own thread as async void, so an exception escaping from here would end the whole process
    private async void StartTwitchMonitor()
    {
        try
        {
            using (IServiceScope scope = services.CreateScope())
            {
                TwitchAPI twitchAPI = scope.ServiceProvider.GetRequiredService<TwitchAPI>();
                await twitchAPI.Start();
            }
        }
        catch (Exception ex)
        {
            logger.Error("App.xaml.cs StartTwitchMonitor", ex);
        }
    }

    //Called at the end of CloseAsync. botService is null if startup failed before the bot was created
    public void Dispose()
    {
        mainTimer?.Dispose();
        botService?.Dispose();
    }
}
