using Discord_Bot.Interfaces.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Concurrent;

namespace Discord_Bot.Services;

//No finalizer: it holds only managed YouTube services, which clean up after themselves
public sealed class YoutubeDataClient : IYoutubeDataClient, IDisposable
{
    //One service per key for the bot's lifetime, instead of a new one (with its own HTTP client) for every search
    private readonly ConcurrentDictionary<string, YouTubeService> services = [];

    public Task<VideoListResponse> GetVideoAsync(string apiKey, string videoId)
    {
        VideosResource.ListRequest request = GetService(apiKey).Videos.List("snippet,contentDetails");
        request.Id = videoId;
        request.MaxResults = 1;
        return request.ExecuteAsync();
    }

    public Task<SearchListResponse> SearchAsync(string apiKey, string query)
    {
        SearchResource.ListRequest request = GetService(apiKey).Search.List("id,snippet");
        request.Q = query;
        request.MaxResults = 20;
        return request.ExecuteAsync();
    }

    public Task<PlaylistItemListResponse> GetPlaylistItemsAsync(string apiKey, string playlistId)
    {
        PlaylistItemsResource.ListRequest request = GetService(apiKey).PlaylistItems.List("snippet,contentDetails");
        request.PlaylistId = playlistId;
        request.MaxResults = 50;
        return request.ExecuteAsync();
    }

    private YouTubeService GetService(string apiKey)
    {
        return services.GetOrAdd(apiKey, key => new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = key,
            ApplicationName = typeof(YoutubeAPI).ToString()
        }));
    }

    public void Dispose()
    {
        foreach (YouTubeService service in services.Values)
        {
            service.Dispose();
        }
    }
}
