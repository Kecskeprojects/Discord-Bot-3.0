using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Features;
public class AudioPlayFeature(
    IYoutubeDownloadService youtubeDownloadService,
    IServerService serverService,
    Config config,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IYoutubeDownloadService youtubeStreamService = youtubeDownloadService;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        ServerAudioResource audioResource = GetCurrentAudioResource();
        ServerResource server = await GetCurrentServerAsync();
        try
        {
            //If function returns false, it means it could not connect correctly for some reason
            if (!await DiscordTools.ConnectBot(Context, audioResource, server))
            {
                logger.Log("User must be in a voice channel, or a voice channel must be passed as an argument!");
                audioResource.AudioVariables.Playing = false;

                audioResource.AudioVariables = new();
                audioResource.MusicRequests.Clear();
                return false;
            }

            while (audioResource.MusicRequests.Count > 0)
            {
                if (!await DiscordTools.CheckAndReconnectBotIfNeeded(Context, audioResource))
                {
                    throw new Exception("Bot could not reconnect after gateway disconnect");
                }

                MusicRequest current = audioResource.MusicRequests[0];

                await Context.Channel.SendMessageAsync($"Now Playing:\n`{current.Title}`");

                await StreamAudioAsync(audioResource, current);

                //Deleting the finished song if the list was not cleared for some other reason
                if (audioResource.MusicRequests.Count > 0)
                {
                    audioResource.MusicRequests.RemoveAt(0);
                }

                //If the playlist is empty and there is no song playing, start counting down for 300 seconds
                if (audioResource.MusicRequests.Count == 0)
                {
                    logger.Log("Playlist empty!");

                    int waitedSeconds = await WaitAsync(audioResource);

                    //In case counter reached it's limit, disconnect,
                    //or if the bot disconnected for some other reason, leave the loop and clear the request list
                    SocketGuildUser clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;
                    if (waitedSeconds > 299 || clientUser.VoiceChannel == null)
                    {
                        if (waitedSeconds > 299 && clientUser.VoiceChannel != null)
                        {
                            await Context.Channel.SendMessageAsync("`Disconnected due to inactivity.`");

                            await clientUser.VoiceChannel.DisconnectAsync();
                        }

                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SocketGuildUser clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;
            if (clientUser.VoiceChannel != null)
            {
                logger.Log("Disconnected due to Error.");
                await clientUser.VoiceChannel.DisconnectAsync();
            }

            logger.Error("AudioPlayFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }

        audioResource.AudioVariables.Playing = false;
        audioResource.AudioVariables = new();
        audioResource.MusicRequests.Clear();
        return true;
    }

    private async Task StreamAudioAsync(ServerAudioResource audioResource, MusicRequest current)
    {
        //Streaming the music
        try
        {
            double length = System.Xml.XmlConvert.ToTimeSpan(current.Duration).TotalMilliseconds;

            if (length > 3000)
            {
                await youtubeStreamService.StreamAsync(audioResource, current.URL.Split('&')[0]);
            }
            else
            {
                logger.Log("Intentional waiting when a video is too short to play");
                await Task.Delay((int) length + 2000);
            }
        }
        catch (Exception ex)
        {
            logger.Error("AudioPlayFeature.cs StreamAudioAsync", ex);
        }
    }

    private async Task<int> WaitAsync(ServerAudioResource audioResource)
    {
        //In case counter reached it's limit, disconnect
        int j = 0;
        while (audioResource.MusicRequests.Count == 0 && j < config.VoiceWaitSeconds)
        {
            j++;

            SocketGuildUser clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;
            if (clientUser.VoiceChannel == null)
            {
                logger.Log("Bot not in voice channel anymore!");
                break;
            }

            await Task.Delay(1000);
        }
        return j;
    }
}
