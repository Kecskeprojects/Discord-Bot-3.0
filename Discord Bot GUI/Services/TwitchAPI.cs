using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Discord_Bot.Services;

public class TwitchAPI(
    ITwitchCLI twitchCLI,
    ITwitchChannelService twitchChannelService,
    TwitchNotificationFeature twitchNotificationFeature,
    BotLogger logger,
    Config config) : ITwitchAPI
{
    #region Variables
    private readonly ITwitchCLI twitchCLI = twitchCLI;
    private readonly ITwitchChannelService twitchChannelService = twitchChannelService;
    private readonly TwitchNotificationFeature twitchNotificationFeature = twitchNotificationFeature;
    private readonly BotLogger logger = logger;
    private readonly Config config = config;

    private LiveStreamMonitorService Monitor;
    private TwitchLib.Api.TwitchAPI API;
    private readonly Dictionary<string, bool> channelStatuses = [];
    private string Token;
    private int TokenTick = 0;
    #endregion

    #region Base Methods
    //Running the Twitch api request and catching errors
    public async Task Start()
    {
        try
        {
            logger.Log("Twitch monitoring starting!");

            Token = twitchCLI.GenerateToken();

            await Check();
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs Twitch", ex);
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

        API = new TwitchLib.Api.TwitchAPI();

        API.Settings.ClientId = config.Twitch_Client_Id;
        API.Settings.AccessToken = Token;

        Monitor = new LiveStreamMonitorService(API);

        List<string> lst = await GetChannelsAsync();

        if (CollectionTools.IsNullOrEmpty(lst))
        {
            logger.Query("No channels to monitor on twitch, service stopped!");
            return;
        }

        Monitor.OnStreamOnline += MonitorOnStreamOnline;
        Monitor.OnStreamOffline += MonitorOnStreamOffline;
        Monitor.OnServiceTick += MonitorOnServiceTick;

        Monitor.OnServiceStarted += MonitorOnServiceStarted;
        Monitor.OnChannelsSet += MonitorOnChannelsSet;

        Monitor.SetChannelsById(lst);

        Monitor.Start(); //Keep at the end!

        await Task.Delay(-1);
    }
    #endregion

    #region Event Handlers
    //Make announcement when stream comes online
    private async void MonitorOnStreamOnline(object sender, OnStreamOnlineArgs e)
    {
        try
        {
            logger.Query($"Streamer turned online: {e.Stream.UserName} with id: {e.Stream.Id}");
            List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
            if (channels == null)
            {
                return;
            }

            foreach (TwitchChannelResource channel in channels)
            {
                if (channelStatuses.TryGetValue(channel.TwitchId, out bool channelStatus) &&
                    !channelStatus &&
                    channel.TwitchId == e.Stream.UserId)
                {
                    await twitchNotificationFeature.Run(new { TwitchChannel = channel, e.Stream.ThumbnailUrl, e.Stream.Title });
                    channelStatuses[channel.TwitchId] = true;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnStreamOnline", ex);
        }
    }

    //Make console message when stream goes offline
    private async void MonitorOnStreamOffline(object sender, OnStreamOfflineArgs e)
    {
        try
        {
            logger.Query($"Streamer turned offline: {e.Stream.UserName} with id: {e.Stream.Id}");

            List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
            if (channels == null)
            {
                return;
            }

            foreach (TwitchChannelResource channel in channels)
            {
                if (channelStatuses.TryGetValue(channel.TwitchId, out bool channelStatus) &&
                    channelStatus &&
                    channel.TwitchId == e.Stream.UserId)
                {
                    channelStatuses[channel.TwitchId] = false;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnStreamOffline", ex);
        }
    }

    //Every 2 hours, send message on console, every 24 hours, refresh token and reset counter
    private async void MonitorOnServiceTick(object sender, OnServiceTickArgs e)
    {
        try
        {
            TokenTick++;
            if (TokenTick > 1440)
            {
                Token = twitchCLI.GenerateToken();
                TokenTick = 0;
            }

            if (TokenTick % 120 == 0)
            {
                logger.Log("120 queries have been completed!");
            }

            if (TokenTick % 5 == 0)
            {
                List<string> lst = await GetChannelsAsync();

                if (!CollectionTools.IsNullOrEmpty(lst) && Monitor.ChannelsToMonitor.Except(lst).Any())
                {
                    Monitor.SetChannelsById(lst);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitchAPI.cs MonitorOnServiceTick", ex);
        }
    }

    private void MonitorOnChannelsSet(object sender, OnChannelsSetArgs e)
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

    private void MonitorOnServiceStarted(object sender, OnServiceStartedArgs e)
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

    private async Task<List<string>> GetChannelsAsync()
    {
        List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
        List<string> lst = [];
        if (!CollectionTools.IsNullOrEmpty(channels))
        {
            foreach (TwitchChannelResource channel in channels)
            {
                channelStatuses.TryAdd(channel.TwitchId, false);
                lst.Add(channel.TwitchId);
            }
        }
        return lst;
    }
    #endregion
}