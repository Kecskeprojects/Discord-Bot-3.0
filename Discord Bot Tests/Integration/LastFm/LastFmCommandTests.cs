using System.IO;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Interfaces.Processors;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using NSubstitute.Core;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.LastFm;

//The !lf commands through Discord.Net's command service, with the bot's Last.fm service replaced
//(ListeningTests and ArtistAndWhoKnowsTests run that service over a fake Last.fm)
public sealed class LastFmCommandTests : IDisposable
{
    private const string NotConnected = "You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!";
    private const string Account = TestValues.LastFmUsername;

    private readonly IUserService userService = Substitute.For<IUserService>();
    private readonly ILastFmAPI lastFmAPI = Substitute.For<ILastFmAPI>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IWhoKnowsImageProcessor imageProcessor = Substitute.For<IWhoKnowsImageProcessor>();
    private readonly IWebService webService = Substitute.For<IWebService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public LastFmCommandTests()
    {
        host = new TestHost().Replace(userService).Replace(lastFmAPI).Replace(serverService).Replace(imageProcessor).Replace(webService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        userService.GetUserAsync(DiscordIds.User).Returns(new UserResource { DiscordId = DiscordIds.User, LastFmUsername = Account });
        member.Nickname.Returns("Ali");
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(member);
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Username connected to account.")]
    [InlineData(DbProcessResultEnum.AlreadyExists, "You have a last.fm account connected to your discord account already.")]
    [InlineData(DbProcessResultEnum.Failure, "Last.fm account could not be connected to account!")]
    public async Task Connect_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        userService.AddLastfmUsernameAsync(DiscordIds.User, Account).Returns(outcome);

        CommandResult result = await RunAsync($"!lf connect {Account}");

        Assert.True(result.IsSuccess, result.ErrorReason);
        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Last.fm has been disconnected from your account.")]
    [InlineData(DbProcessResultEnum.NotFound, "You do not have a lastfm user tied to your account.")]
    public async Task Delete_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        userService.RemoveLastfmUsernameAsync(DiscordIds.User).Returns(outcome);

        await RunAsync("!lf delete");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("!lf tal")]
    [InlineData("!lf tar")]
    [InlineData("!lf tt")]
    [InlineData("!lf np")]
    [InlineData("!lf rc")]
    [InlineData("!lf artist blackpink")]
    public async Task Commands_NoAccountConnected_SayHowToConnectOne(string command)
    {
        userService.GetUserAsync(DiscordIds.User).Returns((UserResource?) null);

        await RunAsync(command);

        Assert.Equal(NotConnected, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("!lf tal 7day 5", "Ali's Top Albums...")]
    [InlineData("!lf tar 7day 5", "Ali's Top Artists...")]
    [InlineData("!lf tt 7day 5", "Ali's Top Tracks...")]
    public async Task TopLists_SendTheListUnderTheUsersNickname(string command, string title)
    {
        LastFmListResult list = new() { TotalPlays = 40, EmbedFields = ["`#1`**Shut Down** by **BLACKPINK**\n", "", ""] };
        lastFmAPI.GetTopAlbumsAsync(Account, 5, null, "7day").Returns(list);
        lastFmAPI.GetTopArtistsAsync(Account, 5, null, "7day").Returns(list);
        lastFmAPI.GetTopTracksAsync(Account, 5, null, "7day").Returns(list);

        await RunAsync(command);

        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal(title, embed.Author?.Name);
        Assert.Equal("Total plays: 40", embed.Footer?.Text);
    }

    [Fact]
    public async Task TopAlbums_LastFmMessage_IsPassedOn()
    {
        lastFmAPI.GetTopAlbumsAsync(Account, 10, null, "overall").Returns(new LastFmListResult { Message = "User not found" });

        await RunAsync("!lf tal");

        Assert.Equal("User not found", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(true, "Ali is currently listening to...")]
    [InlineData(false, "Ali last listened to...")]
    public async Task NowPlaying_SaysWhetherItPlaysNow(bool playing, string title)
    {
        lastFmAPI.GetNowPlayingAsync(Account).Returns(new NowPlayingResult { NowPlaying = playing, TrackName = "Shut Down", ArtistName = "BLACKPINK", AlbumName = "Born Pink" });

        await RunAsync("!lf np");

        Assert.Equal(title, Assert.Single(Assert.Single(channel.Sent).Embeds).Author?.Name);
    }

    //Until 2026-11-23 a length over 30 went to Last.fm as it was, and the list then threw past its 30 lines
    [Theory]
    [InlineData("!lf rc", 10)]
    [InlineData("!lf rc 25", 25)]
    [InlineData("!lf rc 40", 10)]
    [InlineData("!lf rc 0", 10)]
    public async Task Recents_AskForALengthUpToThirty(string command, int limit)
    {
        lastFmAPI.GetRecentsAsync(default!, default).ReturnsForAnyArgs(new LastFmListResult { Message = "listed" });

        await RunAsync(command);

        await lastFmAPI.Received().GetRecentsAsync(Account, limit);
    }

    [Fact]
    public async Task Artist_SendsTheStatsUnderTheArtistsName()
    {
        lastFmAPI.GetArtistDataAsync(Account, "blackpink").Returns(new ArtistStats { ArtistName = "BLACKPINK", Playcount = 40, TrackField = "`#1` **Shut Down**  (*4 plays*)\n" });

        await RunAsync("!lf artist BLACKPINK ");

        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("Ali's stats for BLACKPINK", embed.Author?.Name);
        Assert.Equal("You have listened to this artist **40** times", embed.Description);
    }

    [Fact]
    public async Task WhoKnows_NoAccountAndNothingNamed_SaysHowToConnectOne()
    {
        userService.GetAllLastFmUsersAsync().Returns([]);

        await RunAsync("!lf wk");

        Assert.Equal(NotConnected, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task WhoKnows_NothingNamed_AsksAboutTheUsersTrackAmongTheServersMembers()
    {
        UsersLinked();
        lastFmAPI.GetWhoKnowsByNowPlayingAsync(default!, default!).ReturnsForAnyArgs(call => WhoKnows(call.ArgAt<List<UserResource>>(1)));

        await RunAsync("!lf wk");

        //The third account's owner isn't on this server
        await lastFmAPI.Received().GetWhoKnowsByNowPlayingAsync(Account, Arg.Is<List<UserResource>>(users => users.Select(user => user.DiscordId).SequenceEqual(new[] { DiscordIds.User, DiscordIds.OtherUser })));
        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("`#1` **Bob** with *9 plays*\n`#2` **Ali** with *3 plays*\n", Assert.Single(embed.Fields).Value);
    }

    [Fact]
    public async Task WhoKnows_WithACover_SendsTheRankingImage()
    {
        UsersLinked();
        lastFmAPI.GetWhoKnowsDataAsync(default!, default!).ReturnsForAnyArgs(call =>
        {
            WhoKnows wk = WhoKnows(call.ArgAt<List<UserResource>>(1));
            wk.ImageUrl = "https://example.com/cover.png";
            return wk;
        });
        webService.GetStream("https://example.com/cover.png").Returns(_ => new MemoryStream([1, 2, 3]));
        imageProcessor.EditPicture(default!, default!, default!).ReturnsForAnyArgs(_ => new EditPictureResult("ranking.png", new MemoryStream([4, 5, 6])));

        await RunAsync("!lf wk blackpink");

        ICall sent = Assert.Single(channel.Channel.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IMessageChannel.SendFileAsync));
        Assert.Equal("ranking.png", sent.GetArguments()[1]);
        Assert.Equal("attachment://ranking.png", Assert.Single(sent.GetArguments().OfType<Embed[]>().Single()).Image?.Url);
        Assert.Empty(channel.Sent);
    }

    [Fact]
    public async Task WhoKnows_Message_IsPassedOn()
    {
        UsersLinked();
        lastFmAPI.GetWhoKnowsDataAsync(default!, default!).ReturnsForAnyArgs(call => new WhoKnows(call.ArgAt<List<UserResource>>(1)) { Message = "No one has listened to this song/artist according to last.fm!" });

        await RunAsync("!lf wk twice");

        Assert.Equal("No one has listened to this song/artist according to last.fm!", Assert.Single(channel.Sent).Text);
    }

    //The sender and one other member have accounts, and so does someone who isn't on the server
    private void UsersLinked()
    {
        IGuildUser bob = DiscordMocks.GuildUser(DiscordIds.OtherUser, username: "Bob");
        bob.Nickname.Returns((string?) null);
        guild.GetUserAsync(DiscordIds.OtherUser, CacheMode.CacheOnly).Returns(bob);
        guild.GetUserAsync(DiscordIds.ThirdUser, CacheMode.CacheOnly).Returns((IGuildUser?) null);
        userService.GetAllLastFmUsersAsync().Returns(
        [
            new UserResource { DiscordId = DiscordIds.User, LastFmUsername = Account },
            new UserResource { DiscordId = DiscordIds.OtherUser, LastFmUsername = "bob-fm" },
            new UserResource { DiscordId = DiscordIds.ThirdUser, LastFmUsername = "carol-fm" }
        ]);
    }

    private static WhoKnows WhoKnows(List<UserResource> users)
    {
        return new WhoKnows(users) { EmbedTitle = "Shut Down by BLACKPINK", Plays = new() { ["Bob"] = 9, ["Ali"] = 3 } };
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
