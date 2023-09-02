﻿using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Twitch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Discord_Bot.Services
{
    public class TwitchAPI : ITwitchAPI
    {
        private readonly Logging logger;
        private readonly Config config;
        private readonly ITwitchChannelService twitchChannelService;
        public TwitchAPI(Logging logger, Config config, ITwitchChannelService twitchChannelService)
        {
            this.logger = logger;
            this.config = config;
            this.twitchChannelService = twitchChannelService;
        }

        #region Global variables
        //Saved variables
        private LiveStreamMonitorService Monitor;
        private TwitchLib.Api.TwitchAPI API;
        private static readonly Dictionary<string, bool> channelStatuses = new();
        private static string Token;
        private static int TokenTick = 0;
        #endregion

        #region Startup functions
        //Running the Twitch api request and catching errors
        public async void Start()
        {
            try
            {
                logger.Log("Twitch monitoring starting!");

                Token = Generate_Token();

                await Check();
            }
            catch (Exception ex)
            {
                logger.Error("TwitchAPI.cs Twitch", ex.ToString());
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

            List<string> lst = await GetChannels();

            Monitor.OnStreamOnline += Monitor_OnStreamOnline;
            Monitor.OnStreamOffline += Monitor_OnStreamOffline;
            Monitor.OnServiceTick += Monitor_OnServiceTick;

            Monitor.OnServiceStarted += Monitor_OnServiceStarted;
            Monitor.OnChannelsSet += Monitor_OnChannelsSet;

            Monitor.SetChannelsById(lst);

            Monitor.Start(); //Keep at the end!

            await Task.Delay(-1);
        }
        #endregion

        #region Event handlers
        //Make announcement when stream comes online
        private async void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            logger.Query($"Streamer turned online: {e.Stream.UserName} with id: {e.Stream.Id}");
            List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
            foreach (TwitchChannelResource channel in channels)
            {
                if (channelStatuses.ContainsKey(channel.TwitchId) &&
                    !channelStatuses[channel.TwitchId] &&
                    channel.TwitchId == e.Stream.UserId)
                {
                    //Todo: Move to command level
                    /*
                    //Do not send a message if a channel was not set
                    if(server.Value.TNotifChannel != 0)
                    {
                        IMessageChannel channel = client.GetChannel(server.Value.TNotifChannel) as IMessageChannel;

                        string thumbnail = e.Stream.ThumbnailUrl.Replace("{width}", "1024").Replace("{height}", "576");

                        EmbedBuilder builder = new();
                        builder.WithTitle("Stream is now online!");
                        builder.AddField(e.Stream.Title != "" ? e.Stream.Title : "No Title", server.Value.TChannelLink, false);
                        builder.WithImageUrl(thumbnail);
                        builder.WithCurrentTimestamp();

                        builder.WithColor(Color.Purple);

                        //If there is no notification role set on the server, we just send a message without the role ping
                        string notifRole = server.Value.TNotifChannel != 0 ? $"<@&{server.Value.TNotifRole}>" : "";

                        await channel.SendMessageAsync(notifRole, false, builder.Build());
                    }
                    */
                    channelStatuses[channel.TwitchId] = true;
                }
            }
        }

        //Make console message when stream goes offline
        private async void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            logger.Query($"Streamer turned offline: {e.Stream.UserName} with id: {e.Stream.Id}");

            List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
            foreach (TwitchChannelResource channel in channels)
            {
                if (channelStatuses.ContainsKey(channel.TwitchId) &&
                    channelStatuses[channel.TwitchId] &&
                    channel.TwitchId == e.Stream.UserId)
                {
                    channelStatuses[channel.TwitchId] = false;
                }
            }
        }

        //Every 2 hours, send message on console, every 24 hours, refresh token and reset counter
        private async void Monitor_OnServiceTick(object sender, OnServiceTickArgs e)
        {
            TokenTick++;
            if (TokenTick > 1440)
            {
                Token = Generate_Token();
                TokenTick = 0;
            }

            if (TokenTick % 120 == 0)
            {
                logger.Log("===================================");
                logger.Log("120 queries have been completed!");
                logger.Log("===================================");
            }

            if (TokenTick % 5 == 0)
            {
                List<string> lst = await GetChannels();

                if (Monitor.ChannelsToMonitor.Except(lst).Any()) Monitor.SetChannelsById(lst);
            }
        }

        private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)
        {
            string channels = string.Join(", ", e.Channels);
            logger.Query("Now listening to twitch channel ids: " + channels);
        }

        private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e) => logger.Log("Twitch monitoring started!");
        #endregion

        #region Helper functions
        //Responsible for generating the access tokens to Twitch's api requests
        private string Generate_Token()
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/C twitch.exe token",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            string response = process.StandardError.ReadToEnd();
            process.WaitForExit();

            response = response.Substring(response.IndexOf("Token: ") + 7, 30);
            logger.Query($"Twitch API token: {response}");
            return response;
        }

        //Get user data by username
        public UserData GetChannel(string username)
        {
            try
            {
                Process process = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "cmd.exe",
                        Arguments = $"/C twitch.exe api get users?login={username.ToLower()}",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                string response = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                User twitchUser = JsonConvert.DeserializeObject<User>(response);

                logger.Query($"Twitch user found: {twitchUser.Response[0].DisplayName}");

                return twitchUser.Response[0];
            }
            catch (Exception ex)
            {
                logger.Error("TwitchAPI.cs GetChannel", ex.ToString());
            }

            return null;
        }

        private async Task<List<string>> GetChannels()
        {
            List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
            List<string> lst = new();
            foreach (TwitchChannelResource channel in channels)
            {
                if (!channelStatuses.ContainsKey(channel.TwitchId))
                    channelStatuses.Add(channel.TwitchId, false);
                lst.Add(channel.TwitchId);
            }

            return lst;
        }
        #endregion
    }
}