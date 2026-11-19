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
    private readonly ServerAudioResource audio;
    private readonly YoutubeAPI api;

    public YoutubeAPITests()
    {
        AudioStateStore audioStates = new();
        audio = audioStates.GetOrAdd(DiscordIds.Server);

        //Asks in Discord whether to add a playlist; with a client that finds no channel, the answer is always no
        YoutubeAddPlaylistFeature addPlaylist = new(Substitute.For<IDiscordClient>(), new FakeTimeProvider(), Substitute.For<IServerService>(), logger);
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
        //The store picks its first key at random, so reset until the used-up one comes first
        do
        {
            keys.Reset([SpentKey, Key]);
        }
        while (keys.CurrentKey != SpentKey);
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
}
