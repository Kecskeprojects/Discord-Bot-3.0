using SpotifyAPI.Web;

namespace Discord_Bot.Interfaces.Services;

/// <summary>
/// Hands out the Spotify client for the credentials in the config; tests return a mocked ISpotifyClient.
/// </summary>
public interface ISpotifyClientFactory
{
    ISpotifyClient GetClient();
}
