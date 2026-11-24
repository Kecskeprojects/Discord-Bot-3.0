using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Communication;
using Discord_Bot.Features;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools.NativeTools;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Discord_Bot.Services;

//The monitor's event handlers are async void and can overlap, so they share nothing that is unsafe to use twice at once:
//database work and features get a new DI scope per use (a DbContext allows one operation at a time, and a feature
//keeps the parameters of its current run), and the stream states are kept in a ConcurrentDictionary
public class TwitchAPI(
    IServiceProvider services,
    ITwitchCLI twitchCLI,
    IBotLogger logger,
    IConfig config) : ITwitchAPI
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly ITwitchCLI twitchCLI = twitchCLI;
    private readonly IBotLogger logger = logger;
    private readonly IConfig config = config;

    //Both are set by Check before the monitor raises any event, and only the event handlers use them after that
    private LiveStreamMonitorService monitor = null!;
    private TwitchLib.Api.TwitchAPI api = null!;
    private readonly ConcurrentDictionary<string, bool> channelStatuses = new();
    private string token = "";
    private int tokenTick;
    #endregion

    #region Base Methods
    //Running the Twitch api request and catching errors
    public async Task Start()
    {
        try
        {
            logger.Log("Twitch monitoring starting!");

            token = twitchCLI.GenerateToken();

            await Check();
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs Start", ex);
        }
    }

    //The main function that keeps checking the stream state
    private async Task Check()
    {
        if (string.IsNullOrEmpty(config.Twitch_Client_Id))
        {
            logger.Error("TwitchAPI.cs Check", "Twitch checker could not be initialized, fill out the config file!");
            return;
        }

        api = new TwitchLib.Api.TwitchAPI();

        api.Settings.ClientId = config.Twitch_Client_Id;
        api.Settings.AccessToken = token;

        monitor = new LiveStreamMonitorService(api);

        List<string> lst = await GetChannelsAsync();

        if (CollectionTools.IsNullOrEmpty(lst))
        {
            logger.Query("No channels to monitor on twitch, service stopped!");
            return;
        }

        monitor.OnStreamOnline += MonitorOnStreamOnline;
        monitor.OnStreamOffline += MonitorOnStreamOffline;
        monitor.OnServiceTick += MonitorOnServiceTick;

        monitor.OnServiceStarted += MonitorOnServiceStarted;
        monitor.OnChannelsSet += MonitorOnChannelsSet;

        monitor.SetChannelsById(lst);

        monitor.Start(); //Keep at the end!

        await Task.Delay(-1);
    }
    #endregion

    #region Event Handlers
    //Make announcement when stream comes online
    private async void MonitorOnStreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        try
        {
            logger.Query($"Streamer turned online: {e.Stream.UserName} with id: {e.Stream.Id}");
            await StreamOnlineAsync(e.Stream.UserId, e.Stream.ThumbnailUrl, e.Stream.Title);
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnStreamOnline", ex);
        }
    }

    //A tracked stream that went online notifies each server following it, once until it goes offline; internal so tests can run it
    //without TwitchLib's monitor. The handlers can overlap, so the status flips atomically. Until 2026-11-23 it flipped after the
    //first server's notification, so the other servers following the same stream got none
    internal async Task StreamOnlineAsync(string twitchId, string thumbnailUrl, string title)
    {
        if (!channelStatuses.TryUpdate(twitchId, true, false))
        {
            return;
        }

        List<TwitchChannelResource> channels = await GetTwitchChannelsAsync();
        foreach (TwitchChannelResource channel in channels.Where(channel => channel.TwitchId == twitchId))
        {
            TwitchNotificationData data = new()
            {
                TwitchChannel = channel,
                ThumbnailUrl = thumbnailUrl,
                Title = title
            };
            using (IServiceScope scope = services.CreateScope())
            {
                TwitchNotificationFeature twitchNotificationFeature = scope.ServiceProvider.GetRequiredService<TwitchNotificationFeature>();
                _ = await twitchNotificationFeature.Run(data);
            }
        }
    }

    //Make console message when stream goes offline
    private void MonitorOnStreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        try
        {
            logger.Query($"Streamer turned offline: {e.Stream.UserName} with id: {e.Stream.Id}");
            StreamOffline(e.Stream.UserId);
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnStreamOffline", ex);
        }
    }

    //The next time the stream goes online notifies again; internal so tests can run it without TwitchLib's monitor
    internal void StreamOffline(string twitchId)
    {
        _ = channelStatuses.TryUpdate(twitchId, false, true);
    }

    //Every 2 hours, send message on console, every 24 hours, refresh token and reset counter
    private async void MonitorOnServiceTick(object? sender, OnServiceTickArgs e)
    {
        try
        {
            tokenTick++;
            if (tokenTick > 1440)
            {
                token = twitchCLI.GenerateToken();
                //The monitor shares this client, and its requests read the token from the settings, not from the field
                api.Settings.AccessToken = token;
                tokenTick = 0;
            }

            if (tokenTick % 120 == 0)
            {
                logger.Log("120 queries have been completed!");
            }

            List<string> lst = await GetChannelsAsync();

            if (!CollectionTools.IsNullOrEmpty(lst)
                && (monitor.ChannelsToMonitor.Except(lst).Any()
                    || lst.Except(monitor.ChannelsToMonitor).Any()))
            {
                monitor.SetChannelsById(lst);
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnServiceTick", ex);
        }
    }

    private void MonitorOnChannelsSet(object? sender, OnChannelsSetArgs e)
    {
        try
        {
            string channels = string.Join(", ", e.Channels);
            logger.Query("Now listening to twitch channel ids: " + channels);
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnChannelsSet", ex);
        }
    }

    private void MonitorOnServiceStarted(object? sender, OnServiceStartedArgs e)
    {
        try
        {
            logger.Log("Twitch monitoring started!");
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnServiceStarted", ex);
        }
    }
    #endregion

    #region Helper Methods
    //The resources are plain DTOs, so they stay usable after the scope and its DbContext are disposed
    private async Task<List<TwitchChannelResource>> GetTwitchChannelsAsync()
    {
        using (IServiceScope scope = services.CreateScope())
        {
            ITwitchChannelService twitchChannelService = scope.ServiceProvider.GetRequiredService<ITwitchChannelService>();
            return await twitchChannelService.GetChannelsAsync();
        }
    }

    //Tracks every followed channel as offline until the monitor says otherwise; internal so tests can load them
    internal async Task<List<string>> GetChannelsAsync()
    {
        List<TwitchChannelResource> channels = await GetTwitchChannelsAsync();
        List<string> lst = [];
        if (!CollectionTools.IsNullOrEmpty(channels))
        {
            foreach (TwitchChannelResource channel in channels)
            {
                _ = channelStatuses.TryAdd(channel.TwitchId, false);
                lst.Add(channel.TwitchId);
            }
        }
        return lst;
    }
    #endregion
}