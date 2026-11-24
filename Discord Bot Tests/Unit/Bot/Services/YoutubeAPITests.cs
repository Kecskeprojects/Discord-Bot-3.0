using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Config;
using Discord_Bot.Tests.Mocks.Discord;
using Discord_Bot.Tests.Mocks.Youtube;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Time.Testing;
using System.Net.Http;

namespace Discord_Bot.Tests.Unit.Bot.Services;

public sealed class YoutubeAPITests
{
    private const string Key = TestValues.YoutubeApiKey;
    private const string SpentKey = TestValues.SpentYoutubeApiKey;

    private readonly IYoutubeDataClient youtube = Substitute.For<IYoutubeDataClient>();
    private readonly YoutubeKeyStore keys = new();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly ServerAudioResource audio;
    private readonly YoutubeAPI api;

    public YoutubeAPITests()
    {
        AudioStateStore audioStates = new();
        audio = audioStates.GetOrAdd(DiscordIds.Server);

        //Asks in Discord whether to add a playlist; while the client finds no channel, the answer is no
        YoutubeAddPlaylistFeature addPlaylist = new(client, new FakeTimeProvider(), Substitute.For<IServerService>(), logger);
        api = new YoutubeAPI(addPlaylist, youtube, keys, audioStates, logger, new TestConfig());
        keys.Reset([Key]);
    }

    [Fact]
    public async Task Searching_Keywords_QueuesTheFirstVideoFound()
    {
        youtube.SearchAsync(Key, "some song").Returns(YoutubeResponses.SearchFindingVideo("vid1"));
        youtube.GetVideoAsync(Key, "vid1").Returns(YoutubeResponses.Video("vid1", "Some Song"));

        SearchResultEnum result = await SearchAsync("some song");

        Assert.Equal(SearchResultEnum.YoutubeFoundVideo, result);
        MusicRequest queued = Assert.Single(audio.MusicRequests);
        Assert.Equal("Some Song", queued.Title);
        Assert.Equal(TestValues.Username, queued.User);
    }

    [Fact]
    public async Task Searching_YoutuBeLink_LooksUpOnlyThatVideo()
    {
        youtube.GetVideoAsync(Key, "vid2").Returns(YoutubeResponses.Video("vid2"));

        SearchResultEnum result = await SearchAsync("https://youtu.be/vid2");

        Assert.Equal(SearchResultEnum.YoutubeFoundVideo, result);
        Assert.Equal("https://www.youtube.com/watch?v=vid2", Assert.Single(audio.MusicRequests).URL);
        await youtube.DidNotReceiveWithAnyArgs().SearchAsync(default!, default!);
    }

    [Fact]
    public async Task Searching_WithAUsedUpKey_SwitchesToTheNextKey()
    {
        //Neither key has been used yet, so the store starts with the first
        keys.Reset([SpentKey, Key]);
        youtube.SearchAsync(SpentKey, Arg.Any<string>()).Returns(YoutubeResponses.QuotaExceeded<SearchListResponse>());
        youtube.SearchAsync(Key, "some song").Returns(YoutubeResponses.SearchFindingVideo("vid1"));
        youtube.GetVideoAsync(Key, "vid1").Returns(YoutubeResponses.Video("vid1"));

        SearchResultEnum result = await SearchAsync("some song");

        Assert.Equal(SearchResultEnum.YoutubeFoundVideo, result);
        Assert.Equal(Key, keys.CurrentKey);
        Assert.Single(audio.MusicRequests);
    }

    [Fact]
    public async Task Searching_WhenEveryKeyIsUsedUp_FindsAndQueuesNothing()
    {
        keys.Reset([SpentKey]);
        youtube.SearchAsync(SpentKey, Arg.Any<string>()).Returns(YoutubeResponses.QuotaExceeded<SearchListResponse>());

        SearchResultEnum result = await SearchAsync("some song");

        Assert.Equal(SearchResultEnum.YoutubeNotFound, result);
        Assert.Null(keys.CurrentKey);
        Assert.Empty(audio.MusicRequests);
    }

    [Fact]
    public async Task Searching_PlaylistRunningOutOfQuota_KeepsTheSongsItQueued()
    {
        youtube.GetPlaylistItemsAsync(Key, "PL1").Returns(YoutubeResponses.Playlist("p1", "p2", "p3", "p4"));
        youtube.GetVideoAsync(Key, Arg.Any<string>()).Returns(call => Task.FromResult(YoutubeResponses.Video(call.ArgAt<string>(1))));
        youtube.GetVideoAsync(Key, "p3").Returns(YoutubeResponses.QuotaExceeded<VideoListResponse>());

        SearchResultEnum result = await SearchAsync("https://www.youtube.com/playlist?list=PL1");

        Assert.Equal(SearchResultEnum.YoutubePlaylistFound, result);
        string[] expected = ["Title of p1", "Title of p2"];
        Assert.Equal(expected, audio.MusicRequests.Select(request => request.Title));
        //Not started over with another key, which would queue p1 and p2 again
        await youtube.Received(1).GetPlaylistItemsAsync(Key, "PL1");
        await youtube.DidNotReceive().GetVideoAsync(Key, "p4");
    }

    //Until 2026-11-24 the linked video was queued a second time when the import reached it
    [Fact]
    public async Task Searching_WatchLinkWithItsPlaylistAdded_LeavesTheLinkedVideoOutOfTheImport()
    {
        PlaylistOfferAnsweredYes();
        youtube.GetVideoAsync(Key, Arg.Any<string>()).Returns(call => Task.FromResult(YoutubeResponses.Video(call.ArgAt<string>(1))));
        youtube.GetPlaylistItemsAsync(Key, "PL1").Returns(YoutubeResponses.Playlist("p1", "vid1", "p2"));

        SearchResultEnum result = await SearchAsync("https://www.youtube.com/watch?v=vid1&list=PL1");

        //The offer and the import run on after the search returned
        for (int i = 0; audio.MusicRequests.Count < 3 && i < 100; i++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        Assert.Equal(SearchResultEnum.YoutubeFoundVideo, result);
        string[] expected = ["Title of vid1", "Title of p1", "Title of p2"];
        Assert.Equal(expected, audio.MusicRequests.Select(request => request.Title));
    }

    [Fact]
    public async Task Searching_OtherErrors_AreLoggedAndFindNothing()
    {
        youtube.SearchAsync(Key, Arg.Any<string>()).Returns(Task.FromException<SearchListResponse>(new HttpRequestException("Connection reset")));

        SearchResultEnum result = await SearchAsync("some song");

        Assert.Equal(SearchResultEnum.YoutubeSearchNotFound, result);
        logger.Received().Error("YoutubeAPI.cs KeywordSearch", Arg.Any<Exception>());
    }

    private Task<SearchResultEnum> SearchAsync(string query)
    {
        return api.Searching(query, TestValues.Username, DiscordIds.Server, DiscordIds.Channel);
    }

    //The offer in the command's channel gets the user's reaction next to the bot's own, which YoutubeAddPlaylistFeature reads as yes
    private void PlaylistOfferAnsweredYes()
    {
        IReadOnlyCollection<IUser> reacted = [DiscordMocks.User(DiscordIds.Bot), DiscordMocks.User()];
        IAsyncEnumerable<IReadOnlyCollection<IUser>> reactions = new[] { reacted }.ToAsyncEnumerable();

        IUserMessage offer = Substitute.For<IUserMessage>();
        offer.GetReactionUsersAsync(default, default).ReturnsForAnyArgs(reactions);
        IMessageChannel channel = Substitute.For<IMessageChannel>();
        channel.SendMessageAsync().ReturnsForAnyArgs(offer);
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns(channel);
    }
}
