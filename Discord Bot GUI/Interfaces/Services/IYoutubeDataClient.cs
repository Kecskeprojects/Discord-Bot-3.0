using Google.Apis.YouTube.v3.Data;

namespace Discord_Bot.Interfaces.Services;

/// <summary>
/// The three YouTube Data API calls the bot makes, each with the API key to use, so YoutubeAPI keeps the key rotation.
/// A used-up quota comes back as the Google.GoogleApiException YouTube answers with.
/// </summary>
public interface IYoutubeDataClient
{
    //One video by ID (1 quota unit)
    Task<VideoListResponse> GetVideoAsync(string apiKey, string videoId);
    //The first 20 results of a keyword search (100 quota units)
    Task<SearchListResponse> SearchAsync(string apiKey, string query);
    //The first 50 items of a playlist (1 quota unit)
    Task<PlaylistItemListResponse> GetPlaylistItemsAsync(string apiKey, string playlistId);
}
