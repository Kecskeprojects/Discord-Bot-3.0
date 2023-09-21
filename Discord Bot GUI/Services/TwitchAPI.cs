using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Twitch;
using Discord_Bot.Tools;
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
        #region Variables
        //Saved variables
        private LiveStreamMonitorService Monitor;
        private TwitchLib.Api.TwitchAPI API;
        private static readonly Dictionary<string, bool> channelStatuses = new();
        private static string Token;
        private static int TokenTick = 0;

        private readonly Logging logger;
        private readonly Config config;
        private readonly ITwitchChannelService twitchChannelService;
        private readonly IServiceDiscordCommunication serviceDiscordCommunication;
        #endregion

        public TwitchAPI(Logging logger, Config config, ITwitchChannelService twitchChannelService, IServiceDiscordCommunication serviceDiscordCommunication)
        {
            this.logger = logger;
            this.config = config;
            this.twitchChannelService = twitchChannelService;
            this.serviceDiscordCommunication = serviceDiscordCommunication;
        }

        #region Base Methods
        //Running the Twitch api request and catching errors
        public async void Start()
        {
            try
            {
                logger.Log("Twitch monitoring starting!");

                Token = GenerateToken();

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
                if (channels == null) return;
                foreach (TwitchChannelResource channel in channels)
                {
                    if (channelStatuses.ContainsKey(channel.TwitchId) &&
                        !channelStatuses[channel.TwitchId] &&
                        channel.TwitchId == e.Stream.UserId)
                    {
                        await serviceDiscordCommunication.SendTwitchEmbed(channel, e.Stream.ThumbnailUrl, e.Stream.Title);
                        channelStatuses[channel.TwitchId] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("TwitchAPI.cs MonitorOnStreamOnline", ex.ToString());
            }
        }

        //Make console message when stream goes offline
        private async void MonitorOnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            try
            {
                logger.Query($"Streamer turned offline: {e.Stream.UserName} with id: {e.Stream.Id}");

                List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
                if (channels == null) return;
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
            catch (Exception ex)
            {
                logger.Error("TwitchAPI.cs MonitorOnStreamOffline", ex.ToString());
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
                    Token = GenerateToken();
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
            catch (Exception ex)
            {
                logger.Error("TwitchAPI.cs MonitorOnServiceTick", ex.ToString());
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
                logger.Error("TwitchAPI.cs MonitorOnChannelsSet", ex.ToString());
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
                logger.Error("TwitchAPI.cs MonitorOnServiceStarted", ex.ToString());
            }
        }
        #endregion

        #region Helper Methods
        //Responsible for generating the access tokens to Twitch's api requests
        private string GenerateToken()
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

        private async Task<List<string>> GetChannels()
        {
            List<TwitchChannelResource> channels = await twitchChannelService.GetChannelsAsync();
            if (channels == null) return;
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