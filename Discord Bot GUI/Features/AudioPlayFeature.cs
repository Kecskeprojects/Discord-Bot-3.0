using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using System;
using System.Threading.Tasks;
using System.Xml;

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
        try
        {
            while (audioResource.MusicRequests.Count > 0)
            {
                VoiceConnectionResultEnum connectResult = await CheckAndReconnectBotIfNeeded(Context, audioResource);
                logger.Log($"Voice connection result: {connectResult}");
                switch (connectResult)
                {
                    case VoiceConnectionResultEnum.UserNotInVoiceChannel:
                    {
                        await Context.Channel.SendMessageAsync("You must be in a valid voice channel!");
                        return false;
                    }
                    case VoiceConnectionResultEnum.VoiceChannelNotMusicChannel:
                    {
                        await Context.Channel.SendMessageAsync("Current voice channel is not music channel!");
                        return false;
                    }
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

                    if (await WaitAsync(audioResource))
                    {
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
        finally
        {
            audioResource.AudioVariables.Playing = false;
            audioResource.AudioVariables = new();
            audioResource.MusicRequests.Clear();
        }

        return true;
    }

    public async Task<VoiceConnectionResultEnum> CheckAndReconnectBotIfNeeded(SocketCommandContext context, ServerAudioResource audioResource)
    {
        SocketGuildUser clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;
        VoiceConnectionResultEnum result = VoiceConnectionResultEnum.AlreadyConnected;
        //First connection
        if (audioResource.AudioVariables.FallbackVoiceChannelId == 0)
        {
            SocketVoiceChannel channel = (context.User as SocketGuildUser).VoiceChannel;

            if (channel == null)
            {
                return VoiceConnectionResultEnum.UserNotInVoiceChannel;
            }

            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicVoice, allowLackOfType: true))
            {
                return VoiceConnectionResultEnum.VoiceChannelNotMusicChannel;
            }

            audioResource.AudioVariables.AudioClient = await channel.ConnectAsync();

            audioResource.AudioVariables.FallbackVoiceChannelId = channel.Id;
            result = VoiceConnectionResultEnum.Connected;
        }
        //Reconnection
        else if (clientUser.VoiceChannel == null)
        {
            SocketVoiceChannel channel = context.Guild.GetVoiceChannel(audioResource.AudioVariables.FallbackVoiceChannelId);

            audioResource.AudioVariables.AudioClient = await channel.ConnectAsync(disconnect: false);
            result = VoiceConnectionResultEnum.Reconnected;
        }

        return audioResource.AudioVariables.AudioClient != null
            ? result
            : throw new Exception("Bot could not reconnect after an unexpected disconnect");
    }

    private async Task StreamAudioAsync(ServerAudioResource audioResource, MusicRequest current)
    {
        //Streaming the music
        try
        {
            double length = XmlConvert.ToTimeSpan(current.Duration).TotalMilliseconds;

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

    private async Task<bool> WaitAsync(ServerAudioResource audioResource)
    {
        int waitedSeconds = 0;
        SocketGuildUser clientUser;
        do
        {
            clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;
            //if the bot disconnected for some other reason, leave the loop and clear the request list
            if (clientUser.VoiceChannel == null)
            {
                logger.Log("Bot not in voice channel anymore!");
                return true;
            }

            await Task.Delay(1000);
            waitedSeconds++;
        }
        while (audioResource.MusicRequests.Count == 0 && waitedSeconds < config.VoiceWaitSeconds);

        //In case counter reached it's limit, disconnect,
        //or if the bot disconnected for some other reason, leave the loop and clear the request list
        if (waitedSeconds >= config.VoiceWaitSeconds)
        {
            if (clientUser.VoiceChannel != null)
            {
                await Context.Channel.SendMessageAsync("`Disconnected due to inactivity.`");

                await clientUser.VoiceChannel.DisconnectAsync();
            }

            return true;
        }

        //This is only reached if a new request has been added to the list since bot started waiting
        return false;
    }
}
