using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Music;

//!queue, !now playing, !clear, !skip, !remove, !shuffle and !leave through Discord.Net's command service. They work on the
//server's queue in AudioStateStore, the first song being the one playing
public sealed class QueueCommandTests : IDisposable
{
    private const string NothingPlaying = "Nothing is playing right now!";

    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;
    private readonly ServerAudioResource audio;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public QueueCommandTests()
    {
        host = new TestHost().Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        audio = host.Services.GetRequiredService<AudioStateStore>().GetOrAdd(DiscordIds.Server);
        audio.AudioVariables.CancellationTokenSource = new CancellationTokenSource();
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Fact]
    public async Task Queue_ShowsThePlayingSongTheNextOnesAndTheFullDuration()
    {
        Songs(3);

        CommandResult result = await RunAsync("!queue");

        Assert.True(result.IsSuccess, result.ErrorReason);
        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("Queue (page 1 of 1):", embed.Title);
        Assert.StartsWith("__Currently Playing:__\n**[Song 1](https://www.youtube.com/watch?v=1)**", embed.Fields[0].Value, StringComparison.Ordinal);
        Assert.Equal(["**1. [Song 2](https://www.youtube.com/watch?v=2)**\nRequested by:  tester", "**2. [Song 3](https://www.youtube.com/watch?v=3)**\nRequested by:  tester"], embed.Fields[1..3].Select(field => field.Value));
        Assert.Equal("10m30s", embed.Fields[^1].Value);
    }

    [Fact]
    public async Task Queue_SecondPage_ShowsTheNextTen()
    {
        Songs(15);

        await RunAsync("!q 2");

        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("Queue (page 2 of 2):", embed.Title);
        Assert.StartsWith("**11. [Song 12]", embed.Fields[1].Value, StringComparison.Ordinal);
    }

    //It said "page 1 of 0" until 2026-11-23
    [Fact]
    public async Task Queue_OnlyThePlayingSong_IsOnePage()
    {
        Songs(1);

        await RunAsync("!queue");

        Assert.Equal("Queue (page 1 of 1):", Assert.Single(Assert.Single(channel.Sent).Embeds).Title);
    }

    //Ten songs after the playing one fill one page; until 2026-11-23 the playing song counted too, which allowed an empty second page
    [Theory]
    [InlineData(11, 2)]
    [InlineData(3, 0)]
    public async Task Queue_PageThatDoesNotExist_SaysSo(int songs, int page)
    {
        Songs(songs);

        await RunAsync($"!queue {page}");

        Assert.Equal("There is no such page in the queue!", Assert.Single(channel.Sent).Text);
    }

    //They answered nothing until 2026-11-23
    [Theory]
    [InlineData("!queue")]
    [InlineData("!now playing")]
    [InlineData("!clear")]
    [InlineData("!skip")]
    public async Task Commands_NothingPlaying_SaySo(string command)
    {
        await RunAsync(command);

        Assert.Equal(NothingPlaying, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task NowPlaying_ShowsTheSongWhoAskedForItAndHowFarItIs()
    {
        Songs(2);

        await RunAsync("!np");

        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("Song 1", embed.Title);
        Assert.Equal(("tester", "0m0s / 03m:30s"), (embed.Fields[0].Value, embed.Fields[1].Value));
    }

    [Fact]
    public async Task Clear_EmptiesTheQueueAndStopsTheSong()
    {
        Songs(3);

        await RunAsync("!clear");

        Assert.Empty(audio.MusicRequests);
        Assert.True(audio.AudioVariables.CancellationTokenSource!.IsCancellationRequested);
        Assert.Equal("The queue has been cleared!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Skip_StopsTheSongButKeepsTheQueue()
    {
        Songs(3);

        await RunAsync("!skip");

        Assert.Equal(3, audio.MusicRequests.Count);
        Assert.True(audio.AudioVariables.CancellationTokenSource!.IsCancellationRequested);
        Assert.Equal("Song skipped!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Remove_PositionInTheQueue_RemovesThatSong()
    {
        Songs(3);

        await RunAsync("!remove 2");

        Assert.Equal(["Song 1", "Song 2"], audio.MusicRequests.Select(song => song.Title));
        Assert.Equal("`Song 3` has been removed from the playlist!", Assert.Single(channel.Sent).Text);
    }

    //The playing song is position 0, which !skip is for; these answered nothing until 2026-11-23
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task Remove_NoSongAtThatPosition_SaysSo(int position)
    {
        Songs(3);

        await RunAsync($"!remove {position}");

        Assert.Equal(3, audio.MusicRequests.Count);
        Assert.Equal("There is no song at that position in the queue!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Shuffle_KeepsThePlayingSongFirst()
    {
        Songs(5);

        await RunAsync("!shuffle");

        Assert.Equal("Song 1", audio.MusicRequests[0].Title);
        Assert.Equal(5, audio.MusicRequests.Select(song => song.Title).Distinct().Count());
        Assert.Equal("Shuffle complete!", Assert.Single(channel.Sent).Text);
    }

    //It answered nothing until 2026-11-23
    [Fact]
    public async Task Shuffle_NothingQueuedAfterThePlayingSong_SaysSo()
    {
        Songs(1);

        await RunAsync("!shuffle");

        Assert.Equal("There is nothing to shuffle!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Leave_EmptiesTheQueueStopsTheSongAndLeavesTheVoiceChannel()
    {
        Songs(2);
        IVoiceChannel voice = Substitute.For<IVoiceChannel>();
        IGuildUser bot = DiscordMocks.GuildUser(DiscordIds.Bot);
        bot.VoiceChannel.Returns(voice);
        guild.GetCurrentUserAsync(CacheMode.CacheOnly).Returns(bot);

        await RunAsync("!leave");

        Assert.Empty(audio.MusicRequests);
        Assert.True(audio.AudioVariables.CancellationTokenSource!.IsCancellationRequested);
        await voice.Received().DisconnectAsync();
    }

    //Songs of 3 minutes 30 seconds, requested by the test user; the first is playing
    private void Songs(int count)
    {
        audio.MusicRequests.AddRange(Enumerable.Range(1, count).Select(i => new MusicRequest($"https://www.youtube.com/watch?v={i}", $"Song {i}", "https://example.com/thumb.png", "PT3M30S", TestValues.Username)));
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, member, channel.Channel));
    }

    public void Dispose()
    {
        audio.AudioVariables.CancellationTokenSource?.Dispose();
        host.Dispose();
    }
}
