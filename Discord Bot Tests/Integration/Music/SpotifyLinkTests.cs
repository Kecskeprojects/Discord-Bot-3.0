using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Youtube;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;

namespace Discord_Bot.Tests.Integration.Music;

//A Spotify link through the real SpotifyAPI and YoutubeAPI, with only the Spotify and YouTube clients replaced
public sealed class SpotifyLinkTests : IDisposable
{
    private readonly IYoutubeDataClient youtube = Substitute.For<IYoutubeDataClient>();
    private readonly ISpotifyClient spotify = Substitute.For<ISpotifyClient>();
    private readonly TestHost host;

    public SpotifyLinkTests()
    {
        ISpotifyClientFactory spotifyClients = Substitute.For<ISpotifyClientFactory>();
        spotifyClients.GetClient().Returns(spotify);
        host = new TestHost().Replace(youtube).Replace(spotifyClients).WithYoutubeKeysFromConfig();
    }

    [Fact]
    public async Task TrackLink_IsSearchedOnYoutubeAndQueued()
    {
        spotify.Tracks.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FullTrack { Name = " Song A ", Artists = [new SimpleArtist { Name = " Artist " }] });
        youtube.SearchAsync(TestValues.YoutubeApiKey, "Song A Artist").Returns(YoutubeResponses.SearchFindingVideo("vid1"));
        youtube.GetVideoAsync(TestValues.YoutubeApiKey, "vid1").Returns(YoutubeResponses.Video("vid1", "Song A (Official Video)"));
        ServerAudioResource audio = host.Services.GetRequiredService<AudioStateStore>().GetOrAdd(DiscordIds.Server);

        SearchResultEnum result;
        using (IServiceScope scope = host.Services.CreateScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<ISpotifyAPI>()
                .SpotifySearch(TestValues.SpotifyTrackLink, DiscordIds.Server, DiscordIds.Channel, TestValues.Username);
        }

        Assert.Equal(SearchResultEnum.SpotifyVideoFound, result);
        Assert.Equal("Song A (Official Video)", Assert.Single(audio.MusicRequests).Title);
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
