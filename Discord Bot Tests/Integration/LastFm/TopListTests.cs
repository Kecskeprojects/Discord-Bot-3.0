using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Web;

namespace Discord_Bot.Tests.Integration.LastFm;

//The bot's LastFmAPI and the LastFmApi library together, from the list result down to the HTTP request
public sealed class TopListTests
{
    [Fact]
    public async Task TopTracks_SendsTheQueryAndAddsUpThePlaysOfEveryPage()
    {
        //The total comes from the user's top artists, two pages of them
        int[][] artistPages = [[30, 20], [7]];
        using (FakeLastFm lastFm = new(uri => Respond(uri, artistPages)))
        using (TestHost host = HostWith(lastFm))
        {
            LastFmListResult result = await host.Services.GetRequiredService<ILastFmAPI>().GetTopTracksAsync(TestValues.LastFmUsername, 10, null, "7day");

            Assert.Equal(57, result.TotalPlays);
            Assert.StartsWith("`#1`**Song A** by **Artist**", result.EmbedFields[0]);

            string[] queries = [.. lastFm.Handler.Requests.Select(uri => uri.Query)];
            Assert.Equal(3, queries.Length);
            Assert.Equal($"?method=user.gettoptracks&user={TestValues.LastFmUsername}&api_key={TestValues.LastFmApiKey}&limit=10&period=7day&format=json", queries[0]);
            Assert.Contains("&page=2&", queries[2]);
        }
    }

    [Fact]
    public async Task TopTracks_ForAnUnknownUser_GivesLastFmsMessage()
    {
        using (FakeLastFm lastFm = new(_ => LastFmResponses.Error(6, "User not found")))
        using (TestHost host = HostWith(lastFm))
        {
            LastFmListResult result = await host.Services.GetRequiredService<ILastFmAPI>().GetTopTracksAsync("nobody", 10, null, "7day");

            Assert.Equal("User not found", result.Message);
        }
    }

    //Spotify only adds the cover image; a substitute that finds none makes the bot use Last.fm's own
    private static TestHost HostWith(FakeLastFm lastFm)
    {
        return new TestHost().Replace(lastFm.Requests).Replace(Substitute.For<ISpotifyAPI>());
    }

    private static string Respond(Uri uri, int[][] artistPages)
    {
        System.Collections.Specialized.NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
        if (query["method"] == "user.gettoptracks")
        {
            return LastFmResponses.TopTracks(("Song A", "Artist", 19));
        }

        int page = int.Parse(query["page"] ?? "1");
        return LastFmResponses.TopArtists(page, artistPages.Length, artistPages[page - 1]);
    }
}
