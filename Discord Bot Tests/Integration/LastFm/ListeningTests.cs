using Discord_Bot.Communication.LastFm;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot.Tests.Integration.LastFm;

//The bot's LastFmAPI over the LastFmApi library and a fake Last.fm: top albums and artists, recent tracks and now playing
public sealed class ListeningTests
{
    private const string User = TestValues.LastFmUsername;

    //The total behind the percentages comes from the user's top artists
    private static readonly string totalPlays = LastFmResponses.TopArtists(1, 1, 40);

    [Fact]
    public async Task TopAlbums_ListsThemWithTheirShareOfAllPlays()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.gettopalbums"] = LastFmResponses.TopAlbums(("Born Pink", "BLACKPINK", 10)),
            ["user.gettopartists"] = totalPlays
        }));
        using TestHost host = lastFm.Host();

        LastFmListResult result = await Api(host).GetTopAlbumsAsync(User, 10, null, "overall");

        Assert.Null(result.Message);
        Assert.Equal(40, result.TotalPlays);
        Assert.Equal("`#1`**Born Pink** by **BLACKPINK** - *25%* (*10 plays*)\n", result.EmbedFields[0]);
        Assert.Equal("https://example.com/cover.png", result.ImageUrl);
    }

    [Fact]
    public async Task TopArtists_ListsThemWithTheirShareOfAllPlays()
    {
        using FakeLastFm lastFm = new(uri => uri.Query.Contains("limit=1000", StringComparison.Ordinal)
            ? totalPlays
            : LastFmResponses.TopArtistsNamed(("BLACKPINK", 30), ("aespa", 10)));
        using TestHost host = lastFm.Host();

        LastFmListResult result = await Api(host).GetTopArtistsAsync(User, 10, null, "7day");

        Assert.Equal("`#1`**BLACKPINK** - *75%* (*30 plays*)\n`#2`**aespa** - *25%* (*10 plays*)\n", result.EmbedFields[0]);
    }

    [Fact]
    public async Task TopAlbums_NothingListened_SaysSo()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.gettopalbums"] = LastFmResponses.TopAlbums(),
            ["user.gettopartists"] = LastFmResponses.TopArtists(1, 1)
        }));
        using TestHost host = lastFm.Host();

        LastFmListResult result = await Api(host).GetTopAlbumsAsync(User, 10, null, "7day");

        Assert.Equal("You haven't listened to anything recently!", result.Message);
    }

    [Fact]
    public async Task Recents_ListsTheNowPlayingTrackFirst()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.getrecenttracks"] = LastFmResponses.RecentTracks(true, ("Shut Down", "BLACKPINK", "Born Pink"), ("Supernova", "aespa", "Armageddon")),
            ["user.gettopartists"] = totalPlays
        }));
        using TestHost host = lastFm.Host();

        LastFmListResult result = await Api(host).GetRecentsAsync(User, 10);

        string[] lines = result.EmbedFields[0].Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("`#1` **Shut Down** by **BLACKPINK** - *Now playing*", lines[0]);
        Assert.StartsWith("`#2` **Supernova** by **aespa** - *<t:", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public async Task NowPlaying_GivesTheTrackThePreviousOneAndTheUsersPlaysAndRanking()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.getrecenttracks"] = LastFmResponses.RecentTracks(true, ("Shut Down", "BLACKPINK", "Born Pink"), ("Supernova", "aespa", "Armageddon")),
            //Two plays so far, so this is the third
            ["track.getInfo"] = LastFmResponses.TrackInfo("Shut Down", "BLACKPINK", 2),
            //This month's top tracks: one more played, so it ranks second
            ["user.gettoptracks"] = LastFmResponses.TopTracks(("Pink Venom", "BLACKPINK", 9), ("Shut Down", "BLACKPINK", 4))
        }));
        using TestHost host = lastFm.Host();

        NowPlayingResult result = await Api(host).GetNowPlayingAsync(User);

        Assert.Null(result.Message);
        Assert.True(result.NowPlaying);
        Assert.Equal(("Shut Down", "BLACKPINK", "Born Pink"), (result.TrackName, result.ArtistName, result.AlbumName));
        Assert.Equal(("Supernova", "aespa"), (result.SecondTrackName, result.SecondTrackArtist));
        Assert.Equal(("3rd", "2nd"), (result.TrackPlays, result.Ranking));
    }

    [Fact]
    public async Task NowPlaying_NothingPlaying_GivesTheLastTrack()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.getrecenttracks"] = LastFmResponses.RecentTracks(false, ("Shut Down", "BLACKPINK", "Born Pink"), ("Supernova", "aespa", "Armageddon")),
            ["track.getInfo"] = LastFmResponses.TrackInfo("Shut Down", "BLACKPINK", 2),
            ["user.gettoptracks"] = LastFmResponses.TopTracks(("Shut Down", "BLACKPINK", 4))
        }));
        using TestHost host = lastFm.Host();

        NowPlayingResult result = await Api(host).GetNowPlayingAsync(User);

        Assert.False(result.NowPlaying);
        Assert.Equal("Shut Down", result.TrackName);
    }

    //A new Last.fm account: until 2026-11-23 it took two scrobbles for !lf np to work
    [Fact]
    public async Task NowPlaying_OnlyOneScrobble_GivesItWithoutAPreviousTrack()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.getrecenttracks"] = LastFmResponses.RecentTracks(true, ("Shut Down", "BLACKPINK", "Born Pink")),
            ["track.getInfo"] = LastFmResponses.TrackInfo("Shut Down", "BLACKPINK", 0),
            ["user.gettoptracks"] = LastFmResponses.TopTracks()
        }));
        using TestHost host = lastFm.Host();

        NowPlayingResult result = await Api(host).GetNowPlayingAsync(User);

        Assert.Equal("Shut Down", result.TrackName);
        Assert.Null(result.SecondTrackName);
    }

    [Fact]
    public async Task NowPlaying_NoScrobbles_SaysSo()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new() { ["user.getrecenttracks"] = LastFmResponses.RecentTracks(false) }));
        using TestHost host = lastFm.Host();

        NowPlayingResult result = await Api(host).GetNowPlayingAsync(User);

        Assert.Equal("You haven't listened to anything recently!", result.Message);
    }

    [Fact]
    public async Task NowPlaying_UnknownUser_GivesLastFmsMessage()
    {
        using FakeLastFm lastFm = new(_ => LastFmResponses.Error(6, "User not found"));
        using TestHost host = lastFm.Host();

        Assert.Equal("User not found", (await Api(host).GetNowPlayingAsync("nobody")).Message);
    }

    private static ILastFmAPI Api(TestHost host)
    {
        return host.Services.GetRequiredService<ILastFmAPI>();
    }
}
