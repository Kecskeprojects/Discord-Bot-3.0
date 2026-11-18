using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using SpotifyAPI.Web;

namespace Discord_Bot.Services;

public sealed class SpotifyClientFactory(IConfig config) : ISpotifyClientFactory
{
    private readonly IConfig config = config;
    private readonly Lock clientLock = new();
    private (string Id, string Secret, ISpotifyClient Client)? current;

    //One client while the credentials stay the same, so its token is reused instead of a new one being requested for every call;
    //edited credentials in config.json still apply without a restart
    public ISpotifyClient GetClient()
    {
        string id = config.Spotify_Client_Id;
        string secret = config.Spotify_Client_Secret;
        lock (clientLock)
        {
            if (current is not { } cached || cached.Id != id || cached.Secret != secret)
            {
                SpotifyClientConfig configuration = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(id, secret));
                current = (id, secret, new SpotifyClient(configuration));
            }
            return current.Value.Client;
        }
    }
}
