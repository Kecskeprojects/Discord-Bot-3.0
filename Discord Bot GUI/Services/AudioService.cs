using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.CommandsService;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class AudioService(Logging logger, Config config, IYoutubeAPI youtubeAPI, ISpotifyAPI spotifyAPI, IYoutubeDownloadService youtubeDownloadService) : IAudioService
    {
        private readonly Logging logger = logger;
        private readonly Config config = config;
        private readonly IYoutubeAPI youtubeAPI = youtubeAPI;
        private readonly ISpotifyAPI spotifyAPI = spotifyAPI;
        private readonly IYoutubeDownloadService youtubeDownloadService = youtubeDownloadService;

        public async Task RequestHandler(SocketCommandContext context, string input)
        {
            try
            {
                if (input == "" || (context.User as SocketGuildUser).VoiceChannel == null)
                {
                    return;
                }

                input = input.Replace("<", "").Replace(">", "");

                //In case of a spotify link, do a spotify API query before the youtube API query
                SearchResultEnum result;
                string username = Global.GetNickName(context.Channel, context.User);
                result = input.Contains("spotify.com")
                    ? await spotifyAPI.SpotifySearch(input, context.Guild.Id, context.Channel.Id, username)
                    : await youtubeAPI.Searching(input, username, context.Guild.Id, context.Channel.Id);

                string resultMessage = VoiceService.GetResultMessage(result);

                logger.Log(resultMessage);
                if (result is not SearchResultEnum.SpotifyVideoFound and not SearchResultEnum.YoutubeFoundVideo)
                {
                    await context.Channel.SendMessageAsync(resultMessage);
                    return;
                }

                //Make embedded message if result was not a playlist and it's not the first song
                if (Global.ServerAudioResources[context.Guild.Id].MusicRequests.Count > 1)
                {
                    await VoiceService.RequestEmbed(context.Channel, context.Guild.Id);
                }
            }
            catch (Exception ex)
            {
                logger.Error("AudioFunctions.cs RequestHandler", ex.ToString());
            }
        }

        public async Task PlayHandler(SocketCommandContext context, ServerResource server, ulong sId)
        {
            try
            {
                //If function returns false, it means it could not connect correctly for some reason
                if (!await ConnectBot(context, server, sId))
                {
                    return;
                }

                while (Global.ServerAudioResources[sId].MusicRequests.Count > 0)
                {
                    if (Global.ServerAudioResources[sId].AudioVariables.AbruptDisconnect)
                    {
                        await Task.Delay(5000);

                        if (!await ReConnectBot(context.Guild, sId))
                        {
                            throw new Exception("Bot could not reconnect after gateway disconnect");
                        }
                    }

                    MusicRequest current = Global.ServerAudioResources[sId].MusicRequests[0];

                    await context.Channel.SendMessageAsync($"Now Playing:\n`{current.Title}`");

                    //Streaming the music
                    try
                    {
                        double length = System.Xml.XmlConvert.ToTimeSpan(current.Duration).TotalMilliseconds;

                        if (length > 3000)
                        {
                            await Stream(context.Guild.Id, current.URL.Split('&')[0]);
                        }
                        else
                        {
                            logger.Log("Intentional waiting when a video is too short to play");
                            await Task.Delay((int)length + 2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("AudioFunctions.cs PlayHandler", ex.ToString());
                    }

                    //Deleting the finished song if the list was not cleared for some other reason
                    if (Global.ServerAudioResources[sId].MusicRequests.Count > 0)
                    {
                        Global.ServerAudioResources[sId].MusicRequests.RemoveAt(0);
                    }

                    //If the playlist is empty and there is no song playing, start counting down for 300 seconds
                    if (Global.ServerAudioResources[sId].MusicRequests.Count == 0)
                    {
                        logger.Log("Playlist empty!");

                        //In case counter reached it's limit, disconnect
                        int j = 0;
                        SocketGuildUser clientUser;
                        while (Global.ServerAudioResources[sId].MusicRequests.Count == 0 && j < 300)
                        {
                            j++;

                            clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as SocketGuildUser;
                            if (clientUser.VoiceChannel == null)
                            {
                                logger.Log("Bot not in voice channel anymore!");
                                break;
                            }

                            await Task.Delay(1000);
                        }

                        //In case counter reached it's limit, disconnect,
                        //or if the bot disconnected for some other reason, leave the loop and clear the request list
                        clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as SocketGuildUser;
                        if (j > 299 || clientUser.VoiceChannel == null)
                        {
                            if (j > 299 && clientUser.VoiceChannel != null)
                            {
                                await context.Channel.SendMessageAsync("`Disconnected due to inactivity.`");

                                await clientUser.VoiceChannel.DisconnectAsync();
                            }

                            Global.ServerAudioResources[sId].MusicRequests.Clear();

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SocketGuildUser clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as SocketGuildUser;
                if (clientUser.VoiceChannel != null)
                {
                    logger.Log("Disconnected due to Error.");
                    await clientUser.VoiceChannel.DisconnectAsync();
                }

                logger.Error("AudioFunctions.cs PlayHandler", ex.ToString());
            }
        }

        public async Task<bool> ConnectBot(SocketCommandContext context, ServerResource server, ulong sId)
        {
            try
            {
                SocketGuildUser clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as SocketGuildUser;

                SocketVoiceChannel channel = (context.User as SocketGuildUser).VoiceChannel;

                if (clientUser.VoiceChannel != null)
                {
                    await clientUser.VoiceChannel.DisconnectAsync();
                }

                if (channel != null && Global.IsTypeOfChannel(server, ChannelTypeEnum.MusicVoice, channel.Id))
                {
                    Global.ServerAudioResources[sId].AudioVariables.AudioClient = await channel.ConnectAsync();

                    if (Global.ServerAudioResources[sId].AudioVariables.AudioClient != null)
                    {
                        Global.ServerAudioResources[sId].AudioVariables.FallbackVoiceChannelId = channel.Id;
                        return true;
                    }
                }
                else
                {
                    logger.Log("User must be in a voice channel, or a voice channel must be passed as an argument!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AudioFunctions.cs ConnectBot", ex.ToString());
            }
            return false;
        }

        private async Task Stream(ulong sId, string url)
        {
            try
            {
                Global.ServerAudioResources[sId].AudioVariables.FFmpeg = youtubeDownloadService.CreateYoutubeStream(url);
                Global.ServerAudioResources[sId].AudioVariables.Output = Global.ServerAudioResources[sId].AudioVariables.FFmpeg.StandardOutput.BaseStream;

                Global.ServerAudioResources[sId].AudioVariables.Stopwatch = Stopwatch.StartNew();

                //Audio streaming
                using (Global.ServerAudioResources[sId].AudioVariables.Discord = Global.ServerAudioResources[sId].AudioVariables.AudioClient.CreatePCMStream(AudioApplication.Mixed, config.Bitrate, 3000))
                {
                    logger.Log("Audio stream starting!");

                    await Global.ServerAudioResources[sId].AudioVariables.Output.CopyToAsync(Global.ServerAudioResources[sId].AudioVariables.Discord);
                    await Global.ServerAudioResources[sId].AudioVariables.Discord.FlushAsync();
                };

                Global.ServerAudioResources[sId].AudioVariables.FFmpeg.WaitForExit();
            }
            //Exception thrown with current version of skipping song
            catch (ObjectDisposedException)
            {
                logger.Log("Exception throw when skipping song!");
            }
            //Exception thrown when bot abruptly leaves voice channel
            catch (OperationCanceledException ex)
            {
                logger.Log("Exception thrown when audio stream is cancelled!");
                logger.Warning("AudioService.cs Stream", ex.ToString(), LogOnly: true);

                Global.ServerAudioResources[sId].AudioVariables.FFmpeg.Kill();
                Global.ServerAudioResources[sId].AudioVariables.AbruptDisconnect = true;
            }
            catch (Exception ex)
            {
                logger.Error("AudioService.cs Stream", ex.ToString());
            }

            if (!Global.ServerAudioResources[sId].AudioVariables.FFmpeg.HasExited)
            {
                Global.ServerAudioResources[sId].AudioVariables.FFmpeg.WaitForExit();
            }
            Global.ServerAudioResources[sId].AudioVariables.Stopwatch.Stop();

            logger.Log("Audio stream finished!");
            return;
        }

        private async Task<bool> ReConnectBot(SocketGuild guild, ulong sId)
        {
            try
            {
                SocketVoiceChannel channel = guild.GetVoiceChannel(Global.ServerAudioResources[sId].AudioVariables.FallbackVoiceChannelId);

                Global.ServerAudioResources[sId].AudioVariables.AudioClient = await channel.ConnectAsync();

                if (Global.ServerAudioResources[sId].AudioVariables.AudioClient != null)
                {
                    Global.ServerAudioResources[sId].AudioVariables.AbruptDisconnect = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("AudioFunctions.cs ReConnectBot", ex.ToString());
            }
            return false;
        }
    }
}
