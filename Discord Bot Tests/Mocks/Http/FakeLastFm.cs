using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tests.Common.Tools;
using LastFmApi;
using RestSharp;

namespace Discord_Bot.Tests.Mocks.Http;

/// <summary>
/// The Last.fm API for tests: a <see cref="BaseRequests"/> whose HTTP layer answers from the responder. Register
/// <see cref="Requests"/> in a TestHost, or build the request classes over it directly.
/// </summary>
public sealed class FakeLastFm : IDisposable
{
    private readonly RestClient client;

    public FakeLastFm(Func<Uri, string> respond)
    {
        Handler = new FakeHttpHandler(respond);
        client = new RestClient(new RestClientOptions(Constant.LastFmApiBaseUri) { ConfigureMessageHandler = _ => Handler });
        Requests = new BaseRequests(client);
    }

    public FakeHttpHandler Handler { get; }
    public BaseRequests Requests { get; }

    //A TestHost whose LastFmAPI asks this fake; Spotify finds no cover, so Last.fm's own image is used
    public TestHost Host()
    {
        return new TestHost().Replace(Requests).Replace(Substitute.For<ISpotifyAPI>());
    }

    public void Dispose()
    {
        client.Dispose();
        Handler.Dispose();
    }
}
