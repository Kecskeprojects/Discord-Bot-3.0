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
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

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
    [Summary("Play music on the channel the user is connected to")]
    public async Task Play([Remainder] string content)
    {
        try
        {
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            ServerResource server = await GetCurrentServerAsync();

            await audioRequestFeature.Run(Context, content);
            if (!audioResource.AudioVariables.Playing)
            {
                audioResource.AudioVariables.Playing = true;
                audioResource.AudioVariables.AbruptDisconnect = false;

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

    #region Voice connection commands
    [Command("join")]
    [Alias(["connect", "conn"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Joins the bot to the user's voice channel")]
    public async Task Join()
    {
        try
        {
            ServerAudioResource audioResource = GetCurrentAudioResource();
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.MusicText))
            {
                return;
            }

            ServerResource server = await GetCurrentServerAsync();
            await DiscordTools.ConnectBot(Context, audioResource, server);
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Join", ex);
        }
    }

    [Command("leave")]
    [Alias(["disconnect", "disconn", "disc", "dc"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Leaves the current voice channel")]
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

            audioResource.AudioVariables.FFmpeg.Kill();
            audioResource.AudioVariables.Output.Dispose();
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Leave", ex);
        }
    }
    #endregion

    #region Playlist related commands
    [Command("queue")]
    [Alias(["q"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Current music request queue")]
    public async Task Queue(int index = 1)
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
            if (index * 10 <= songcount || ((index - 1) * 10 < songcount && index * 10 >= songcount))
            {
                Embed[] embed = AudioQueueEmbedProcessor.CreateEmbed(audioResource, index);

                await ReplyAsync(embeds: embed);
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Queue", ex);
        }
    }

    [Command("np")]
    [Alias(["now playing", "nowplaying"])]
    [RequireContext(ContextType.Guild)]
    [Summary("The currently playing song")]
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
    [Summary("Clear playlist and leave voice channel")]
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

            audioResource.AudioVariables.FFmpeg.Kill();
            audioResource.AudioVariables.Output.Dispose();
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Clear", ex);
        }
    }

    [Command("skip")]
    [RequireContext(ContextType.Guild)]
    [Summary("Skip current song")]
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

            audioResource.AudioVariables.FFmpeg.Kill();
            audioResource.AudioVariables.Output.Dispose();
        }
        catch (Exception ex)
        {
            logger.Error("UserVoiceCommands.cs Skip", ex);
        }
    }

    [Command("remove")]
    [Alias(["r"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Removes the song at the given position")]
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
