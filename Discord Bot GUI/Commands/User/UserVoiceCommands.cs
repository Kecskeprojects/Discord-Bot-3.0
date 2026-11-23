using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Features;
using Discord_Bot.Interfaces;
using Discord_Bot.Processors.EmbedProcessors.Audio;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Commands.User;

[Name("Voice")]
[Remarks("User")]
[Summary("Voice channel commands")]
public class UserVoiceCommands(
    AudioPlayFeature audioPlayFeature,
    AudioRequestFeature audioRequestFeature,
    AudioStateStore audioStates,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseCommand(logger, config, serverService)
{
    private readonly AudioPlayFeature audioPlayFeature = audioPlayFeature;
    private readonly AudioRequestFeature audioRequestFeature = audioRequestFeature;
    private readonly AudioStateStore audioStates = audioStates;

    [Command("play")]
    [Alias(["p"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Play music on the channel you are connected to")]
    public async Task Play([Name("youtube/spotify link/search keywords")][Remainder] string searchparameter)
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            ServerResource server = await GetCurrentServerAsync();

            _ = await audioRequestFeature.Run(Context, searchparameter);
            if (!audioResource.AudioVariables.Playing)
            {
                if (audioResource.MusicRequests.Count > 0)
                {
                    audioResource.AudioVariables.Playing = true;
                    _ = audioPlayFeature.Run(Context);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Play", ex);
        }
    }

    [Command("leave")]
    [Alias(["disconnect", "disconn", "disc", "dc"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Leaves the current voice channel and clears the playlist")]
    public async Task Leave()
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            audioResource.MusicRequests.Clear();

            //The bot may not be in a voice channel (e.g. leave sent twice)
            IVoiceChannel voiceChannel = (await Context.Guild.GetCurrentUserAsync(CacheMode.CacheOnly)).VoiceChannel;
            if (voiceChannel != null)
            {
                await voiceChannel.DisconnectAsync();
            }

            audioResource.AudioVariables.CancellationTokenSource?.Cancel(false);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Leave", ex);
        }
    }

    #region Playlist related commands
    [Command("queue")]
    [Alias(["q"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Current music request playlist, only shows 10 items per page")]
    public async Task Queue(int page = 1)
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            //It answered nothing until 2026-11-23
            if (audioResource.MusicRequests.Count == 0)
            {
                _ = await ReplyAsync("Nothing is playing right now!");
                return;
            }

            //The first song is the one playing, and ten of the others fit on a page; until 2026-11-23 the playing one was counted
            //here too, which allowed an empty page past the last, and a page that didn't exist answered nothing
            int pageCount = Math.Max(1, (int) Math.Ceiling((audioResource.MusicRequests.Count - 1) / 10.0));
            if (page < 1 || page > pageCount)
            {
                _ = await ReplyAsync("There is no such page in the queue!");
                return;
            }

            Embed[] embed = AudioQueueEmbedProcessor.CreateEmbed(audioResource, page);

            _ = await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Queue", ex);
        }
    }

    [Command("now playing")]
    [Alias(["np", "nowplaying"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Data about the currently playing song")]
    public async Task NowPlaying()
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            //It answered nothing until 2026-11-23
            if (audioResource.MusicRequests.Count == 0)
            {
                _ = await ReplyAsync("Nothing is playing right now!");
                return;
            }

            Embed[] embed = AudioNowPlayingEmbedProcessor.CreateEmbed(audioResource);

            _ = await Context.Channel.SendMessageAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs NowPlaying", ex);
        }
    }

    [Command("clear")]
    [RequireContext(ContextType.Guild)]
    [Summary("Stop currently playing song and clear playlist")]
    public async Task Clear()
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            //It answered nothing until 2026-11-23
            if (audioResource.MusicRequests.Count == 0)
            {
                _ = await ReplyAsync("Nothing is playing right now!");
                return;
            }

            audioResource.MusicRequests.Clear();

            _ = await Context.Channel.SendMessageAsync("The queue has been cleared!");

            audioResource.AudioVariables.CancellationTokenSource?.Cancel(false);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Clear", ex);
        }
    }

    [Command("skip")]
    [RequireContext(ContextType.Guild)]
    [Summary("Stop current song and start playing next one")]
    public async Task Skip()
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            //It answered nothing until 2026-11-23
            if (audioResource.MusicRequests.Count == 0)
            {
                _ = await ReplyAsync("Nothing is playing right now!");
                return;
            }

            _ = await Context.Channel.SendMessageAsync("Song skipped!");

            audioResource.AudioVariables.CancellationTokenSource?.Cancel(false);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Skip", ex);
        }
    }

    [Command("remove")]
    [Alias(["r"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Removes the song at the given position in the playlist, can't remove currently playing song")]
    public async Task Remove(int position)
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            //Position 0 is the song playing, which !skip is for (a position out of range answered nothing until 2026-11-23)
            if (position < 1 || position >= audioResource.MusicRequests.Count)
            {
                _ = await ReplyAsync("There is no song at that position in the queue!");
                return;
            }

            _ = await ReplyAsync($"`{audioResource.MusicRequests[position].Title}` has been removed from the playlist!");

            audioResource.MusicRequests.RemoveAt(position);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Remove", ex);
        }
    }

    [Command("shuffle")]
    [RequireContext(ContextType.Guild)]
    [Summary("Shuffle the current playlist")]
    public async Task Shuffle()
    {
        try
        {
            ServerAudioResource audioResource = audioStates.GetOrAdd(Context.Guild.Id);
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            //The playing song stays first, so it takes two songs after it (fewer answered nothing until 2026-11-23)
            if (audioResource.MusicRequests.Count < 3)
            {
                _ = await ReplyAsync("There is nothing to shuffle!");
                return;
            }

            audioResource.MusicRequests = CollectionTools.ShufflePlaylist(audioResource.MusicRequests);

            _ = await ReplyAsync("Shuffle complete!");
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Shuffle", ex);
        }
    }
    #endregion
}
