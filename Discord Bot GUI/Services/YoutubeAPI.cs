using DatabaseORM.Interface;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Specialized;
using System.Web;

namespace Discord_Bot.Services;

public class YoutubeAPI(
    YoutubeAddPlaylistFeature youtubeAddPlaylistFeature,
    IYoutubeDataClient youtubeData,
    YoutubeKeyStore youtubeKeys,
    AudioStateStore audioStates,
    IBotLogger logger,
    IConfig config) : IYoutubeAPI
{
    #region Variables
    private readonly YoutubeAddPlaylistFeature youtubeAddPlaylistFeature = youtubeAddPlaylistFeature;
    private readonly IYoutubeDataClient youtubeData = youtubeData;
    private readonly YoutubeKeyStore youtubeKeys = youtubeKeys;
    private readonly AudioStateStore audioStates = audioStates;
    private readonly IBotLogger logger = logger;
    private readonly IConfig config = config;
    #endregion

    #region Base Methods
    //Main function starting the api request
    public async Task<SearchResultEnum> Searching(string query, string username, ulong serverId, ulong channelId)
    {
        logger.Query("============================================================================");
        logger.Query("YouTube Data API: Search");
        SearchResultEnum result = SearchResultEnum.YoutubeNotFound;

        try
        {
            result = await RunWithKeyRotationAsync(apiKey => Run(apiKey, query, username, serverId, channelId));
            logger.Query("============================================================================");
        }
        catch (Exception ex)
        {
            logger.Error("YoutubeAPI.cs Searching", ex);
        }
        return result;
    }

    //Runs a request with the current key, and again with the next key each time a key's quota runs out
    //A request is only repeated from the start if it queued nothing before the quota ran out (see Run and PlaylistSearch)
    private async Task<SearchResultEnum> RunWithKeyRotationAsync(Func<string, Task<SearchResultEnum>> request)
    {
        string? apiKey = youtubeKeys.CurrentKey;
        while (apiKey != null)
        {
            try
            {
                return await request(apiKey);
            }
            catch (Exception ex) when (IsQuotaExceeded(ex))
            {
                logger.Warning("YoutubeAPI.cs RunWithKeyRotationAsync", ex, LogOnly: true);
                apiKey = youtubeKeys.ReplaceExhaustedKey(apiKey);
                if (apiKey != null)
                {
                    logger.Log($"Key switched out, value: {apiKey}!");
                }
            }
        }

        logger.Query("All api key's limits have been exceeded!");
        return SearchResultEnum.YoutubeNotFound;
    }

    //YouTube answers 403 "quotaExceeded" once a key's daily quota is used up
    private static bool IsQuotaExceeded(Exception ex)
    {
        string text = ex.ToString();
        return text.Contains("quotaExceeded") || text.Contains("you have exceeded your");
    }

    //The function running the query
    private async Task<SearchResultEnum> Run(string apiKey, string query, string username, ulong serverId, ulong channelId)
    {
        logger.Query($"API key used: {apiKey}");

        if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
        {
            Uri uri = new(query);
            if (uri.Segments.Length == 2)
            {
                if (uri.Host.Contains("youtu.be"))
                {
                    return await VideoSearch(apiKey, uri.Segments[1], username, serverId);
                }
                else if (uri.Segments[1] == "watch")
                {
                    NameValueCollection queryPart = HttpUtility.ParseQueryString(uri.Query);

                    string? videoId = queryPart["v"];
                    SearchResultEnum videoResult = string.IsNullOrEmpty(videoId)
                        ? SearchResultEnum.YoutubeNotFound
                        : await VideoSearch(apiKey, videoId, username, serverId);

                    //Offered after the video search, so a retry with the next key (quota) does not offer it twice.
                    //The import leaves out the video this link queued
                    string? playlistId = queryPart["list"];
                    if (!string.IsNullOrEmpty(playlistId))
                    {
                        _ = AddPlaylistAsync(playlistId, videoResult == SearchResultEnum.YoutubeFoundVideo ? videoId : null, username, serverId, channelId);
                    }

                    return videoResult;
                }
                else if (uri.Segments[1] == "playlist")
                {
                    NameValueCollection queryPart = HttpUtility.ParseQueryString(uri.Query);

                    string? playlistId = queryPart["list"];
                    return string.IsNullOrEmpty(playlistId)
                        ? SearchResultEnum.YoutubeNotFound
                        : await PlaylistSearch(apiKey, playlistId, username, serverId);
                }
                else
                {
                    return SearchResultEnum.YoutubeNotFound;
                }
            }
        }

        //In any other case, search the result as a keyword
        return await KeywordSearch(apiKey, query, username, serverId);
    }
    #endregion

    #region Helper Methods
    //Searching by video ID
    private async Task<SearchResultEnum> VideoSearch(string apiKey, string query, string username, ulong serverId)
    {
        VideoListResponse searchListResponse = new();
        try
        {
            searchListResponse = await youtubeData.GetVideoAsync(apiKey, query);
        }
        catch (Exception ex) when (!IsQuotaExceeded(ex)) //A used-up quota goes up to RunWithKeyRotationAsync
        {
            logger.Error("YoutubeAPI.cs VideoSearch", ex);
        }

        youtubeKeys.AddUsage(apiKey, 1);

        if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
        {
            return SearchResultEnum.YoutubeNotFound;
        }

        Video video = searchListResponse.Items[0];

        string[] temp = [Constant.YoutubeBaseUrl.UrlCombine($"watch?v={video.Id}"), video.Snippet.Title.Replace("&#39;", "'"), video.Snippet.Thumbnails.Default__.Url, video.ContentDetails.Duration];

        if (!audioStates.TryGet(serverId, out ServerAudioResource? audioResource))
        {
            throw new InvalidOperationException("Server Audio Resource does not exist");
        }

        audioResource.MusicRequests.Add(new MusicRequest(temp[0], temp[1], temp[2], temp[3], username));

        logger.Query("Youtube video query Complete!");
        logger.Query($"{temp[0]}\n{temp[1]}");

        return SearchResultEnum.YoutubeFoundVideo;
    }

    //Searching by keywords
    private async Task<SearchResultEnum> KeywordSearch(string apiKey, string query, string username, ulong serverId)
    {
        SearchListResponse searchListResponse = new();
        try
        {
            searchListResponse = await youtubeData.SearchAsync(apiKey, query);
        }
        catch (Exception ex) when (!IsQuotaExceeded(ex)) //A used-up quota goes up to RunWithKeyRotationAsync
        {
            logger.Error("YoutubeAPI.cs KeywordSearch", ex);
        }

        youtubeKeys.AddUsage(apiKey, 100);

        if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
        {
            return SearchResultEnum.YoutubeSearchNotFound;
        }

        logger.Query("Youtube keyword query Complete!");

        searchListResponse.Items = FilteredResults(searchListResponse.Items, query);

        if (searchListResponse.Items[0].Id.PlaylistId != null)
        {
            return await PlaylistSearch(apiKey, searchListResponse.Items[0].Id.PlaylistId, username, serverId);
        }
        else if (searchListResponse.Items[0].Id.VideoId != null)
        {
            return await VideoSearch(apiKey, searchListResponse.Items[0].Id.VideoId, username, serverId);
        }
        return SearchResultEnum.YoutubeNotFound;
    }

    //Searching by playlist ID; queuedVideoId is a video already queued from the same link, which is left out once.
    //Until 2026-11-24 a watch link's video was queued again if it was in the imported part of its playlist
    private async Task<SearchResultEnum> PlaylistSearch(string apiKey, string query, string username, ulong serverId, string? queuedVideoId = null)
    {
        PlaylistItemListResponse searchListResponse = new();
        try
        {
            searchListResponse = await youtubeData.GetPlaylistItemsAsync(apiKey, query);
        }
        catch (Exception ex) when (!IsQuotaExceeded(ex)) //A used-up quota goes up to RunWithKeyRotationAsync
        {
            logger.Error("YoutubeAPI.cs PlaylistSearch", ex);
        }

        youtubeKeys.AddUsage(apiKey, 1);

        if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
        {
            return SearchResultEnum.YoutubeNotFound;
        }

        int queued = 0;
        foreach (PlaylistItem item in searchListResponse.Items)
        {
            if (queuedVideoId != null && item.ContentDetails.VideoId == queuedVideoId)
            {
                queuedVideoId = null;
                continue;
            }

            try
            {
                if (await VideoSearch(apiKey, item.ContentDetails.VideoId, username, serverId) == SearchResultEnum.YoutubeFoundVideo)
                {
                    queued++;
                }
            }
            //Repeating the import with the next key would queue these songs again, so it keeps what it has
            catch (Exception ex) when (queued > 0 && IsQuotaExceeded(ex))
            {
                logger.Warning("YoutubeAPI.cs PlaylistSearch", ex, LogOnly: true);
                _ = youtubeKeys.ReplaceExhaustedKey(apiKey);
                logger.Log($"The quota ran out during the playlist import, {queued} songs were added.");
                break;
            }
        }

        return SearchResultEnum.YoutubePlaylistFound;
    }

    private IList<SearchResult> FilteredResults(IList<SearchResult> items, string query)
    {
        //Filter youtube channels
        items = items.Where(x => x.Id.Kind != "youtube#channel").ToList();

        //Filter for trigger words
        string[] filterWords = config.Youtube_Filter_Words;
        if (filterWords != null && !filterWords.Any(query.ToLower().Contains))
        {
            List<SearchResult> filteredList = items.Where(result => !filterWords.Any(result.Snippet.Title.ToLower().Contains)).ToList();
            if (!CollectionTools.IsNullOrEmpty(filteredList))
            {
                items = filteredList;
            }
        }

        return items;
    }

    //Checking if user wants to add playlist; it keeps running after Run returned, while the user decides
    private async Task AddPlaylistAsync(string playlistId, string? queuedVideoId, string username, ulong serverId, ulong channelId)
    {
        try
        {
            bool result = await youtubeAddPlaylistFeature.Run(channelId);

            if (result)
            {
                _ = await RunWithKeyRotationAsync(apiKey => PlaylistSearch(apiKey, playlistId, username, serverId, queuedVideoId));
            }
        }
        catch (Exception ex)
        {
            logger.Error("YoutubeAPI.cs AddPlaylistAsync", ex);
        }
    }
    #endregion
}