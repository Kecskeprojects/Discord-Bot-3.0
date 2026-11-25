using System.IO;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Audio;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Music;

//!play and the play loop (AudioPlayFeature), with YouTube, Spotify, the download (yt-dlp and ffmpeg) and the voice connection
//replaced. The waits run on the fake clock, which the loop tests move on while the loop waits
public sealed class PlayTests : IDisposable
{
    private const ulong VoiceId = 300_010;
    private const ulong OtherVoiceId = 300_011;

    private readonly IYoutubeAPI youtubeAPI = Substitute.For<IYoutubeAPI>();
    private readonly ISpotifyAPI spotifyAPI = Substitute.For<ISpotifyAPI>();
    private readonly IYoutubeDownloadService download = Substitute.For<IYoutubeDownloadService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;
    private readonly ServerAudioResource audio;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly IGuildUser bot = DiscordMocks.GuildUser(DiscordIds.Bot);
    private readonly IVoiceChannel voice = Substitute.For<IVoiceChannel>();
    private readonly IAudioClient audioClient = Substitute.For<IAudioClient>();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public PlayTests()
    {
        host = new TestHost().Replace(youtubeAPI).Replace(spotifyAPI).Replace(download).Replace(serverService);
        host.Config.VoiceWaitSeconds = 3;
        ServerHas([]);
        audio = host.Services.GetRequiredService<AudioStateStore>().GetOrAdd(DiscordIds.Server);

        voice.Id.Returns(VoiceId);
        voice.ConnectAsync(default, default, default, default).ReturnsForAnyArgs(audioClient);
        member.Nickname.Returns("Ali");
        member.VoiceChannel.Returns(voice);
        //The bot leaves with !leave or after the wait; until then it is in the channel it joined
        bot.VoiceChannel.Returns(voice);
        guild.GetCurrentUserAsync(CacheMode.CacheOnly).Returns(bot);
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    //It answered nothing until 2026-11-23: the request returned before the loop that says this
    [Fact]
    public async Task Play_OutsideAVoiceChannel_SaysToJoinOne()
    {
        member.VoiceChannel.Returns((IVoiceChannel?) null);

        await RunAsync("!play shut down");

        Assert.Equal("You must be in a valid voice channel!", Assert.Single(channel.Sent).Text);
        await youtubeAPI.DidNotReceiveWithAnyArgs().Searching(default!, default!, default, default);
    }

    [Fact]
    public async Task Play_WhileASongPlays_QueuesTheSearchAndShowsItsPlace()
    {
        Songs(1);
        audio.AudioVariables.Playing = true;
        youtubeAPI.Searching("shut down", "Ali", DiscordIds.Server, DiscordIds.Channel).Returns(_ =>
        {
            audio.MusicRequests.Add(Song("Shut Down"));
            return SearchResultEnum.YoutubeFoundVideo;
        });

        await RunAsync("!play shut down");

        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal(("Shut Down", "Song has been added to the queue!"), (embed.Title, embed.Description));
        Assert.Equal("1", embed.Fields.Single(field => field.Name == "Position in queue:").Value);
        await download.DidNotReceiveWithAnyArgs().StreamAsync(default!, default!);
    }

    [Fact]
    public async Task Play_NothingFound_SaysSo()
    {
        youtubeAPI.Searching(default!, default!, default, default).ReturnsForAnyArgs(SearchResultEnum.YoutubeSearchNotFound);

        await RunAsync("!play nothing like it");

        Assert.Equal("Youtube video/playlist not found!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Play_SpotifyLink_IsLookedUpOnSpotify()
    {
        spotifyAPI.SpotifySearch(default!, default, default, default!).ReturnsForAnyArgs(SearchResultEnum.SpotifyNotFound);

        await RunAsync($"!play <{TestValues.SpotifyTrackLink}>");

        await spotifyAPI.Received().SpotifySearch(TestValues.SpotifyTrackLink, DiscordIds.Server, DiscordIds.Channel, "Ali");
        await youtubeAPI.DidNotReceiveWithAnyArgs().Searching(default!, default!, default, default);
    }

    [Fact]
    public async Task Play_FirstSong_StartsPlayingIt()
    {
        //The bot isn't kept in the channel, so the loop ends after the song instead of waiting
        bot.VoiceChannel.Returns((IVoiceChannel?) null);
        youtubeAPI.Searching(default!, default!, default, default).ReturnsForAnyArgs(_ =>
        {
            audio.MusicRequests.Add(Song("Shut Down"));
            return SearchResultEnum.YoutubeFoundVideo;
        });

        await RunAsync("!play shut down");

        //The command starts the loop without waiting for it
        await Until(() => download.ReceivedCalls().Any() && !audio.AudioVariables.Playing);
        await download.Received().StreamAsync(Arg.Any<ServerAudioResource>(), "https://www.youtube.com/watch?v=Shut Down");
    }

    [Fact]
    public async Task Loop_PlaysEachSongInTurnThenLeavesAfterTheWait()
    {
        Songs(2);

        Assert.True(await PlayLoopAsync());

        Assert.Equal(["Now Playing:\n`Song 1`", "Now Playing:\n`Song 2`", "`Disconnected due to inactivity.`"], channel.Sent.Select(sent => sent.Text));
        Assert.Equal(["https://www.youtube.com/watch?v=Song 1", "https://www.youtube.com/watch?v=Song 2"], StreamedUrls());
        await voice.Received().DisconnectAsync();
        Assert.Empty(audio.MusicRequests);
        Assert.False(audio.AudioVariables.Playing);
        //The session's voice client goes with it (until 2026-11-23 it was replaced without being disposed)
        audioClient.Received().Dispose();
        Assert.Null(audio.AudioVariables.AudioClient);
    }

    [Fact]
    public async Task Loop_SongAddedDuringTheWait_IsPlayedToo()
    {
        Songs(1);

        Assert.True(await PlayLoopAsync(afterFirstTick: () => audio.MusicRequests.Add(Song("Late"))));

        Assert.Equal(["Now Playing:\n`Song 1`", "Now Playing:\n`Late`", "`Disconnected due to inactivity.`"], channel.Sent.Select(sent => sent.Text));
    }

    [Fact]
    public async Task Loop_BotRemovedFromTheChannelDuringTheWait_StopsWithoutAMessage()
    {
        Songs(1);
        bot.VoiceChannel.Returns((IVoiceChannel?) null);

        Assert.True(await PlayLoopAsync());

        Assert.Equal(["Now Playing:\n`Song 1`"], channel.Sent.Select(sent => sent.Text));
        await voice.DidNotReceive().DisconnectAsync();
    }

    //Under 3 seconds a song isn't downloaded; the loop waits its length and 2 more seconds instead
    [Fact]
    public async Task Loop_VeryShortSong_IsWaitedOutInsteadOfDownloaded()
    {
        audio.MusicRequests.Add(new MusicRequest("https://www.youtube.com/watch?v=short", "Short", "", "PT2S", TestValues.Username));
        bot.VoiceChannel.Returns((IVoiceChannel?) null);

        Assert.True(await PlayLoopAsync());

        await download.DidNotReceiveWithAnyArgs().StreamAsync(default!, default!);
    }

    [Fact]
    public async Task Loop_LinkFromAPlaylist_IsDownloadedWithoutTheListPart()
    {
        audio.MusicRequests.Add(new MusicRequest("https://www.youtube.com/watch?v=abc&list=PL1&index=2", "Song", "", "PT3M", TestValues.Username));
        bot.VoiceChannel.Returns((IVoiceChannel?) null);

        await PlayLoopAsync();

        Assert.Equal(["https://www.youtube.com/watch?v=abc"], StreamedUrls());
    }

    [Fact]
    public async Task Loop_DownloadFails_GoesOnWithTheNextSong()
    {
        Songs(2);
        bot.VoiceChannel.Returns((IVoiceChannel?) null);
        download.StreamAsync(Arg.Any<ServerAudioResource>(), "https://www.youtube.com/watch?v=Song 1").ThrowsAsync(new IOException("yt-dlp failed"));

        await PlayLoopAsync();

        Assert.Equal(["Now Playing:\n`Song 1`", "Now Playing:\n`Song 2`"], channel.Sent.Select(sent => sent.Text));
    }

    [Fact]
    public async Task Loop_UserLeftTheVoiceChannel_SaysToJoinOneAndClearsTheQueue()
    {
        Songs(2);
        member.VoiceChannel.Returns((IVoiceChannel?) null);

        Assert.False(await PlayLoopAsync());

        Assert.Equal(["You must be in a valid voice channel!"], channel.Sent.Select(sent => sent.Text));
        Assert.Empty(audio.MusicRequests);
    }

    //Until 2026-11-23 the text channel of the command was checked instead of the voice channel (since the refactor of
    //2024-08-27), so a server that set a music voice channel couldn't play at all
    [Fact]
    public async Task Loop_ServerWithAMusicVoiceChannel_PlaysInIt()
    {
        Songs(1);
        ServerHas(new() { [ChannelTypeEnum.MusicVoice] = [VoiceId] });
        bot.VoiceChannel.Returns((IVoiceChannel?) null);

        Assert.True(await PlayLoopAsync());

        Assert.Equal(["Now Playing:\n`Song 1`"], channel.Sent.Select(sent => sent.Text));
    }

    [Fact]
    public async Task Loop_UserInAnotherVoiceChannelThanTheMusicOne_SaysSo()
    {
        Songs(1);
        ServerHas(new() { [ChannelTypeEnum.MusicVoice] = [OtherVoiceId] });

        Assert.False(await PlayLoopAsync());

        Assert.Equal(["Current voice channel is not music channel!"], channel.Sent.Select(sent => sent.Text));
    }

    //Runs the loop as !play starts it, moving the fake clock a second at a time while it waits
    private async Task<bool> PlayLoopAsync(Action? afterFirstTick = null)
    {
        AudioPlayFeature feature = host.FromNewScope<AudioPlayFeature>();
        audio.AudioVariables.Playing = true;
        Task<bool> loop = feature.Run(DiscordMocks.TextCommand("!play", member, channel.Channel));
        for (int tick = 0; !loop.IsCompleted && tick < 200; tick++)
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
            host.Clock.Advance(TimeSpan.FromSeconds(1));
            if (tick == 0)
            {
                afterFirstTick?.Invoke();
            }
        }
        Assert.True(loop.IsCompleted, "The play loop didn't finish");
        return await loop;
    }

    private List<string> StreamedUrls()
    {
        return download.ReceivedCalls().Select(call => (string) call.GetArguments()[1]!).ToList();
    }

    private static async Task Until(Func<bool> condition)
    {
        for (int i = 0; !condition() && i < 200; i++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        Assert.True(condition(), "Timed out waiting");
    }

    private void ServerHas(Dictionary<ChannelTypeEnum, List<ulong>> channels)
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server, SettingsChannels = channels });
    }

    private void Songs(int count)
    {
        audio.MusicRequests.AddRange(Enumerable.Range(1, count).Select(i => Song($"Song {i}")));
    }

    private static MusicRequest Song(string title)
    {
        return new MusicRequest($"https://www.youtube.com/watch?v={title}", title, "https://example.com/thumb.png", "PT3M30S", TestValues.Username);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, member, channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
