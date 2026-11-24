using DatabaseORM.Resources;
using Discord_Bot.Communication.LastFm;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot.Tests.Integration.LastFm;

//The bot's LastFmAPI over the LastFmApi library and a fake Last.fm: a user's stats on an artist (!lf artist) and the server's
//listeners of an artist, a track or what someone plays now (!lf whoknows)
public sealed class ArtistAndWhoKnowsTests
{
    private const string User = TestValues.LastFmUsername;

    private static readonly string albums = LastFmResponses.TopAlbums(("Born Pink", "BLACKPINK", 10), ("Armageddon", "aespa", 5));
    private static readonly string tracks = LastFmResponses.TopTracks(("Shut Down", "BLACKPINK", 4), ("Supernova", "aespa", 3), ("Pink Venom", "BLACKPINK", 2));

    [Fact]
    public async Task ArtistData_GivesTheUsersPlaysAndTheirTopTracksAndAlbumsOfTheArtist()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.gettopalbums"] = albums,
            ["user.gettoptracks"] = tracks,
            ["artist.getInfo"] = LastFmResponses.ArtistInfo("BLACKPINK", 40)
        }));
        using TestHost host = lastFm.Host();

        ArtistStats result = await Api(host).GetArtistDataAsync(User, "blackpink");

        Assert.Null(result.Message);
        Assert.Equal(("BLACKPINK", 40), (result.ArtistName, result.Playcount));
        Assert.Equal("`#1` **Born Pink**  (*10 plays*)\n", result.AlbumField);
        Assert.Equal("`#1` **Shut Down**  (*4 plays*)\n`#2` **Pink Venom**  (*2 plays*)\n", result.TrackField);
    }

    [Fact]
    public async Task ArtistData_NameWithAReplacementInTheConfig_AsksForTheReplacement()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.gettopalbums"] = albums,
            ["user.gettoptracks"] = tracks,
            ["artist.getInfo"] = LastFmResponses.ArtistInfo("BLACKPINK", 40)
        }));
        using TestHost host = lastFm.Host();
        host.Config.Lastfm_Artist_Input_Replacement["bp"] = "blackpink";

        ArtistStats result = await Api(host).GetArtistDataAsync(User, "bp");

        Assert.Equal("BLACKPINK", result.ArtistName);
        Assert.Contains(lastFm.Handler.Requests, uri => uri.Query.Contains("method=artist.getInfo", StringComparison.Ordinal) && uri.Query.Contains("artist=blackpink", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ArtistData_ArtistNeverListenedTo_SaysSo()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new() { ["user.gettopalbums"] = albums, ["user.gettoptracks"] = tracks }));
        using TestHost host = lastFm.Host();

        Assert.Equal("You haven't listened to this artist!", (await Api(host).GetArtistDataAsync(User, "twice")).Message);
    }

    //Albums count by the album's artist and tracks by the track's, so a collaboration can give albums without tracks;
    //until 2026-11-23 that threw
    [Fact]
    public async Task ArtistData_AlbumsButNoTracksOfTheArtist_GivesTheAlbums()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.gettopalbums"] = LastFmResponses.TopAlbums(("Sour Candy", "BLACKPINK", 6)),
            ["user.gettoptracks"] = LastFmResponses.TopTracks(("Sour Candy", "Lady Gaga & BLACKPINK", 6)),
            ["artist.getInfo"] = LastFmResponses.ArtistInfo("BLACKPINK", 6)
        }));
        using TestHost host = lastFm.Host();

        ArtistStats result = await Api(host).GetArtistDataAsync(User, "blackpink");

        Assert.Null(result.Message);
        Assert.Equal("BLACKPINK", result.ArtistName);
        Assert.Equal("`#1` **Sour Candy**  (*6 plays*)\n", result.AlbumField);
        Assert.Equal("", result.TrackField);
    }

    [Fact]
    public async Task WhoKnows_Artist_GivesEachListenerByPlaysUnderTheirServerName()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["artist.getInfo:alice-fm"] = LastFmResponses.ArtistInfo("BLACKPINK", 3),
            ["artist.getInfo:bob-fm"] = LastFmResponses.ArtistInfo("BLACKPINK", 9),
            ["artist.getInfo:carol-fm"] = LastFmResponses.ArtistInfo("BLACKPINK", 0)
        }));
        using TestHost host = lastFm.Host();

        WhoKnows result = await WhoKnowsAsync(host, api => api.GetWhoKnowsDataAsync("BLACKPINK", Members(("alice-fm", "Alice"), ("bob-fm", "Bob"), ("carol-fm", "Carol"))));

        Assert.Null(result.Message);
        Assert.Equal("BLACKPINK", result.EmbedTitle);
        Assert.Equal([("Bob", 9), ("Alice", 3)], result.Plays.Select(play => (play.Key, play.Value)));
    }

    [Fact]
    public async Task WhoKnows_ArtistAndTrack_GivesTheTracksListeners()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["track.getInfo:alice-fm"] = LastFmResponses.TrackInfo("Shut Down", "BLACKPINK", 2),
            ["track.getInfo:bob-fm"] = LastFmResponses.TrackInfo("Shut Down", "BLACKPINK", 5),
            ["artist.getInfo"] = LastFmResponses.ArtistInfo("BLACKPINK", 9)
        }));
        using TestHost host = lastFm.Host();

        WhoKnows result = await WhoKnowsAsync(host, api => api.GetWhoKnowsDataAsync("BLACKPINK > Shut Down", Members(("alice-fm", "Alice"), ("bob-fm", "Bob"))));

        Assert.Equal("shut down by BLACKPINK", result.EmbedTitle);
        Assert.Equal([("Bob", 5), ("Alice", 2)], result.Plays.Select(play => (play.Key, play.Value)));
        Assert.Contains(lastFm.Handler.Requests, uri => uri.Query.Contains("track=shut%20down", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WhoKnows_NowPlaying_GivesTheListenersOfTheUsersTrack()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["user.getrecenttracks"] = LastFmResponses.RecentTracks(true, ("Shut Down", "BLACKPINK", "Born Pink")),
            ["track.getInfo"] = LastFmResponses.TrackInfo("Shut Down", "BLACKPINK", 4),
            ["artist.getInfo"] = LastFmResponses.ArtistInfo("BLACKPINK", 9)
        }));
        using TestHost host = lastFm.Host();

        WhoKnows result = await WhoKnowsAsync(host, api => api.GetWhoKnowsByNowPlayingAsync("alice-fm", Members(("alice-fm", "Alice"))));

        Assert.Equal("Shut Down by BLACKPINK", result.EmbedTitle);
        Assert.Equal([("Alice", 4)], result.Plays.Select(play => (play.Key, play.Value)));
    }

    [Fact]
    public async Task WhoKnows_NobodyListened_SaysSo()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new() { ["artist.getInfo"] = LastFmResponses.ArtistInfo("BLACKPINK", 0) }));
        using TestHost host = lastFm.Host();

        WhoKnows result = await WhoKnowsAsync(host, api => api.GetWhoKnowsDataAsync("blackpink", Members(("alice-fm", "Alice"))));

        Assert.Equal("No one has listened to this song/artist according to last.fm!", result.Message);
    }

    //Nicknames aren't unique on a server; until 2026-11-23 two listeners with the same one threw
    [Fact]
    public async Task WhoKnows_TwoListenersWithTheSameNickname_ListsBoth()
    {
        using FakeLastFm lastFm = new(LastFmResponses.Router(new()
        {
            ["artist.getInfo:alice-fm"] = LastFmResponses.ArtistInfo("BLACKPINK", 3),
            ["artist.getInfo:other-alice-fm"] = LastFmResponses.ArtistInfo("BLACKPINK", 9)
        }));
        using TestHost host = lastFm.Host();

        WhoKnows result = await WhoKnowsAsync(host, api => api.GetWhoKnowsDataAsync("blackpink", Members(("alice-fm", "Alice"), ("other-alice-fm", "Alice"))));

        Assert.Equal([("Alice", 9), ("Alice (alice-fm)", 3)], result.Plays.Select(play => (play.Key, play.Value)));
    }

    //The members with a Last.fm account, as !lf whoknows passes them: the name is their name on the server
    private static List<UserResource> Members(params (string LastFm, string Name)[] members)
    {
        return [.. members.Select((member, i) => new UserResource { DiscordId = DiscordIds.User + (ulong) i, LastFmUsername = member.LastFm, Username = member.Name })];
    }

    private static ILastFmAPI Api(TestHost host)
    {
        return host.Services.GetRequiredService<ILastFmAPI>();
    }

    //Who-knows pauses on the clock after each member's request (it waited 50 ms each for real until 2026-11-24), so the fake clock moves on
    private static Task<WhoKnows> WhoKnowsAsync(TestHost host, Func<ILastFmAPI, Task<WhoKnows>> call)
    {
        return host.RunOnClockAsync(call(Api(host)), TimeSpan.FromMilliseconds(50));
    }
}
