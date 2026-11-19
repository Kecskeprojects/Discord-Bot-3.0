using Google;
using Google.Apis.YouTube.v3.Data;

namespace Discord_Bot.Tests.Mocks.Youtube;

//What an IYoutubeDataClient substitute returns: Google's own response classes, filled with the fields YoutubeAPI reads
public static class YoutubeResponses
{
    public static VideoListResponse Video(string id, string? title = null)
    {
        Video video = new()
        {
            Id = id,
            Snippet = new VideoSnippet
            {
                Title = title ?? $"Title of {id}",
                Thumbnails = new ThumbnailDetails { Default__ = new Thumbnail { Url = $"https://example.com/{id}.jpg" } }
            },
            ContentDetails = new VideoContentDetails { Duration = "PT3M30S" }
        };
        return new VideoListResponse { Items = [video] };
    }

    public static SearchListResponse SearchFindingVideo(string videoId)
    {
        SearchResult result = new()
        {
            Id = new ResourceId { Kind = "youtube#video", VideoId = videoId },
            Snippet = new SearchResultSnippet { Title = $"Title of {videoId}" }
        };
        return new SearchListResponse { Items = [result] };
    }

    public static PlaylistItemListResponse Playlist(params string[] videoIds)
    {
        return new PlaylistItemListResponse
        {
            Items = [.. videoIds.Select(id => new PlaylistItem { ContentDetails = new PlaylistItemContentDetails { VideoId = id } })]
        };
    }

    //The error YouTube answers with once a key's daily quota is used up
    public static Task<T> QuotaExceeded<T>()
    {
        return Task.FromException<T>(new GoogleApiException("youtube", "The request cannot be completed because you have exceeded your quota."));
    }
}
