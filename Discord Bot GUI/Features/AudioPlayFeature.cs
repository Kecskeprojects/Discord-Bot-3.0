using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools;
using System.Xml;

namespace Discord_Bot.Features;

public class AudioPlayFeature(
    IYoutubeDownloadService youtubeDownloadService,
    AudioStateStore audioStates,
    IConfig config,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IYoutubeDownloadService youtubeStreamService = youtubeDownloadService;
    private readonly AudioStateStore audioStates = audioStates;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
        try
        {
            while (audioResource.MusicRequests.Count > 0)
            {
                VoiceConnectionResultEnum connectResult = await CheckAndReconnectBotIfNeededAsync(Context, audioResource);
                logger.Audio($"Voice connection result: {connectResult}");
                switch (connectResult)
                {
                    case VoiceConnectionResultEnum.UserNotInVoiceChannel:
                    {
                        _ = await Context.Channel.SendMessageAsync("You must be in a valid voice channel!");
                        return false;
                    }
                    case VoiceConnectionResultEnum.VoiceChannelNotMusicChannel:
                    {
                        _ = await Context.Channel.SendMessageAsync("Current voice channel is not music channel!");
                        return false;
                    }
                }

                MusicRequest current = audioResource.MusicRequests[0];

                _ = await Context.Channel.SendMessageAsync($"Now Playing:\n`{current.Title}`");

                await StreamAudioAsync(audioResource, current);

                //Deleting the finished song if the list was not cleared for some other reason
                if (audioResource.MusicRequests.Count > 0)
                {
                    audioResource.MusicRequests.RemoveAt(0);
                }

                //If the playlist is empty and there is no song playing, start counting down for 300 seconds
                if (audioResource.MusicRequests.Count == 0)
                {
                    logger.Audio("Playlist empty!");

                    if (await WaitAsync(audioResource))
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //Logged first, so the original error is not lost if disconnecting fails too
            logger.Error("AudioPlayFeature.cs ExecuteCoreLogicAsync", ex);

            IGuildUser clientUser = await Context.Guild.GetCurrentUserAsync(CacheMode.CacheOnly);
            if (clientUser.VoiceChannel != null)
            {
                logger.Audio("Disconnected due to Error.");
                await clientUser.VoiceChannel.DisconnectAsync();
            }

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

    public async Task<VoiceConnectionResultEnum> CheckAndReconnectBotIfNeededAsync(ICommandContext context, ServerAudioResource audioResource)
    {
        IGuildUser clientUser = await Context.Guild.GetCurrentUserAsync(CacheMode.CacheOnly);
        VoiceConnectionResultEnum result = VoiceConnectionResultEnum.AlreadyConnected;
        //First connection
        if (audioResource.AudioVariables.FallbackVoiceChannelId == 0)
        {
            IVoiceChannel channel = ((IGuildUser) context.User).VoiceChannel;

            if (channel == null)
            {
                return VoiceConnectionResultEnum.UserNotInVoiceChannel;
            }

            //The voice channel the user is in; from the 2024-08-27 refactor until 2026-11-23 the command's text channel was checked,
            //so a server that set a music voice channel couldn't play at all
            if (!DiscordTools.IsTypeOfChannel(await GetCurrentServerAsync(), ChannelTypeEnum.MusicVoice, channel.Id, allowLackOfType: true))
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
            IVoiceChannel channel = await context.Guild.GetVoiceChannelAsync(audioResource.AudioVariables.FallbackVoiceChannelId, CacheMode.CacheOnly);

            audioResource.AudioVariables.AudioClient = await channel.ConnectAsync(disconnect: false);
            result = VoiceConnectionResultEnum.Reconnected;
        }

        if (audioResource.AudioVariables.AudioClient == null)
        {
            throw new InvalidOperationException("Bot could not reconnect after an unexpected disconnect");
        }

        return result;
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
                logger.Audio("Intentional waiting when a video is too short to play");
                await Task.Delay(TimeSpan.FromMilliseconds(length + 2000), timeProvider);
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
        IGuildUser clientUser;
        do
        {
            clientUser = await Context.Guild.GetCurrentUserAsync(CacheMode.CacheOnly);
            //if the bot disconnected for some other reason, leave the loop and clear the request list
            if (clientUser.VoiceChannel == null)
            {
                logger.Audio("Bot not in voice channel anymore!");
                return true;
            }

            await Task.Delay(TimeSpan.FromSeconds(1), timeProvider);
            waitedSeconds++;
        }
        while (audioResource.MusicRequests.Count == 0 && waitedSeconds < config.VoiceWaitSeconds);

        //In case counter reached it's limit, disconnect,
        //or if the bot disconnected for some other reason, leave the loop and clear the request list
        if (waitedSeconds >= config.VoiceWaitSeconds)
        {
            if (clientUser.VoiceChannel != null)
            {
                _ = await Context.Channel.SendMessageAsync("`Disconnected due to inactivity.`");

                await clientUser.VoiceChannel.DisconnectAsync();
            }

            return true;
        }

        //This is only reached if a new request has been added to the list since bot started waiting
        return false;
    }
}
