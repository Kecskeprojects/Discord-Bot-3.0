using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Audio;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Voice")]
[Remarks("User")]
[Summary("Voice channel commands")]
public class UserVoiceCommands(
    AudioPlayFeature audioPlayFeature,
    AudioRequestFeature audioRequestFeature,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly AudioPlayFeature audioPlayFeature = audioPlayFeature;
    private readonly AudioRequestFeature audioRequestFeature = audioRequestFeature;

    [Command("play")]
    [Alias(["p"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Play music on the channel you are connected to")]
    public async Task Play([Name("youtube/spotify link or keyword")][Remainder] string searchparameter)
    {
        try
        {
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            ServerResource server = await GetCurrentServerAsync();

            await audioRequestFeature.Run(Context, searchparameter);
            if (!audioResource.AudioVariables.Playing)
            {
                audioResource.AudioVariables.Playing = true;

                if (audioResource.MusicRequests.Count > 0)
                {
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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            audioResource.MusicRequests.Clear();

            IUser clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
            await (clientUser as SocketGuildUser).VoiceChannel.DisconnectAsync();

            audioResource.AudioVariables.CancellationTokenSource.Cancel(false);
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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText) || audioResource.MusicRequests.Count == 0)
            {
                return;
            }

            int songcount = audioResource.MusicRequests.Count;

            //If queue does not have songs on that page, do not show a queue
            if (page * 10 <= songcount || ((page - 1) * 10 < songcount && page * 10 >= songcount))
            {
                Embed[] embed = AudioQueueEmbedProcessor.CreateEmbed(audioResource, page);

                await ReplyAsync(embeds: embed);
            }
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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText) || audioResource.MusicRequests.Count == 0)
            {
                return;
            }

            Embed[] embed = AudioNowPlayingEmbedProcessor.CreateEmbed(audioResource);

            await Context.Channel.SendMessageAsync(embeds: embed);
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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText) || audioResource.MusicRequests.Count == 0)
            {
                return;
            }

            audioResource.MusicRequests.Clear();

            await Context.Channel.SendMessageAsync("The queue has been cleared!");

            audioResource.AudioVariables.CancellationTokenSource.Cancel(false);
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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText) || audioResource.MusicRequests.Count == 0)
            {
                return;
            }

            await Context.Channel.SendMessageAsync("Song skipped!");

            audioResource.AudioVariables.CancellationTokenSource.Cancel(false);
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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText) || position < 1 || position >= audioResource.MusicRequests.Count)
            {
                return;
            }

            await ReplyAsync($"`{audioResource.MusicRequests[position].Title}` has been removed from the playlist!");

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
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText) || audioResource.MusicRequests.Count < 2)
            {
                return;
            }

            audioResource.MusicRequests = CollectionTools.ShufflePlaylist(audioResource.MusicRequests);

            await ReplyAsync("Shuffle complete!");
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Shuffle", ex);
        }
    }
    #endregion
}
