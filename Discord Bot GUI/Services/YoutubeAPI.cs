using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Discord_Bot.Services
{

    public class YoutubeAPI(Logging logger, Config config, IServiceToDiscordCommunication serviceDiscordCommunication) : IYoutubeAPI
    {
        #region Variables
        private static readonly Dictionary<string, int> keys = [];
        private static int youtubeIndex = 0;
        private readonly Logging logger = logger;
        private readonly Config config = config;
        private readonly IServiceToDiscordCommunication serviceDiscordCommunication = serviceDiscordCommunication;
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
                if (keys.Count != 0)
                {
                    result = await Run(query, username, serverId, channelId);
                }
                else
                {
                    logger.Query("All api key's limits have been exceeded!");
                }
                logger.Query("============================================================================");
            }
            catch (Exception ex)
            {
                //switching api keys if quota is exceeded
                if ((ex.ToString().Contains("quotaExceeded") || ex.ToString().Contains("you have exceeded your")) && keys.Count != 0)
                {
                    string currentKey = config.Youtube_API_Keys[youtubeIndex];

                    keys.Remove(currentKey);

                    Random r = new();

                    youtubeIndex = r.Next(0, keys.Count);
                    currentKey = config.Youtube_API_Keys[youtubeIndex];

                    logger.Warning("YoutubeAPI.cs Searching", ex.ToString(), LogOnly: true);
                    logger.Log($"Key switched out to key in {youtubeIndex} position, value: {currentKey}!");

                    result = await Run(query, username, serverId, channelId);
                }
                else
                {
                    logger.Error("YoutubeAPI.cs Searching", ex.ToString());
                }
            }
            return result;
        }

        //The function running the query
        private async Task<SearchResultEnum> Run(string query, string username, ulong serverId, ulong channelId)
        {
            string currentKey = config.Youtube_API_Keys[youtubeIndex];

            YouTubeService youtubeService = new(new BaseClientService.Initializer()
            {
                ApiKey = currentKey,
                ApplicationName = GetType().ToString()
            });

            logger.Query($"API key used: {currentKey}");

            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                Uri uri = new(query);
                if (uri.Segments.Length == 2)
                {
                    if (uri.Host.Contains("youtu.be"))
                    {
                        return await VideoSearch(youtubeService, uri.Segments[1], username, serverId);
                    }
                    else if (uri.Segments[1] == "watch")
                    {
                        NameValueCollection queryPart = HttpUtility.ParseQueryString(uri.Query);

                        if (queryPart.AllKeys.Contains("list"))
                        {
                            _ = AddPlaylistAsync(youtubeService, queryPart, username, serverId, channelId);
                        }

                        return await VideoSearch(youtubeService, queryPart["v"], username, serverId);
                    }
                    else if (uri.Segments[1] == "playlist")
                    {
                        NameValueCollection queryPart = HttpUtility.ParseQueryString(uri.Query);

                        return await PlaylistSearch(youtubeService, queryPart["list"], username, serverId);
                    }
                    else
                    {
                        return SearchResultEnum.YoutubeNotFound;
                    }
                }
            }

            //In any other case, search the result as a keyword
            return await KeywordSearch(youtubeService, query, username, serverId);
        }
        #endregion

        #region Helper Methods
        //Searching by video ID
        private async Task<SearchResultEnum> VideoSearch(YouTubeService youtubeService, string query, string username, ulong serverId)
        {
            string currentKey = config.Youtube_API_Keys[youtubeIndex];

            VideosResource.ListRequest searchListRequest = youtubeService.Videos.List("snippet,contentDetails");
            searchListRequest.Id = query;
            searchListRequest.MaxResults = 1;

            VideoListResponse searchListResponse = new();
            try
            {
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                logger.Error("YoutubeAPI.cs VideoSearch", ex.ToString());
            }

            keys[currentKey] += 1;

            if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
            {
                return SearchResultEnum.YoutubeNotFound;
            }

            Video video = searchListResponse.Items[0];

            string[] temp = ["https://www.youtube.com/watch?v=" + video.Id, video.Snippet.Title.Replace("&#39;", "'"), video.Snippet.Thumbnails.Default__.Url, video.ContentDetails.Duration];

            if (!Global.ServerAudioResources.TryGetValue(serverId, out ServerAudioResource audioResource))
            {
                audioResource = new(serverId);
                Global.ServerAudioResources.Add(serverId, audioResource);
            }

            audioResource.MusicRequests.Add(new MusicRequest(temp[0], temp[1], temp[2], temp[3], username));

            logger.Query("Youtube video query Complete!");
            logger.Query($"{temp[0]}\n{temp[1]}");

            return SearchResultEnum.YoutubeFoundVideo;
        }

        //Searching by keywords
        private async Task<SearchResultEnum> KeywordSearch(YouTubeService youtubeService, string query, string username, ulong serverId)
        {
            string currentKey = config.Youtube_API_Keys[youtubeIndex];

            SearchResource.ListRequest searchListRequest = youtubeService.Search.List("id,snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = 20;

            SearchListResponse searchListResponse = new();
            try
            {
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                logger.Error("YoutubeAPI.cs KeywordSearch", ex.ToString());
            }

            keys[currentKey] += 100;

            if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
            {
                return SearchResultEnum.YoutubeSearchNotFound;
            }

            logger.Query("Youtube keyword query Complete!");

            searchListResponse.Items = FilteredResults(searchListResponse.Items, query);

            if (searchListResponse.Items[0].Id.PlaylistId != null)
            {
                return await PlaylistSearch(youtubeService, searchListResponse.Items[0].Id.PlaylistId, username, serverId);
            }
            else if (searchListResponse.Items[0].Id.VideoId != null)
            {
                return await VideoSearch(youtubeService, searchListResponse.Items[0].Id.VideoId, username, serverId);
            }
            return SearchResultEnum.YoutubeNotFound;
        }

        //Searching by playlist ID
        //Todo: Be able to choose where to start the 25 songs from
        private async Task<SearchResultEnum> PlaylistSearch(YouTubeService youtubeService, string query, string username, ulong serverId)
        {
            string currentKey = config.Youtube_API_Keys[youtubeIndex];

            PlaylistItemsResource.ListRequest searchListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");

            searchListRequest.PlaylistId = query;
            searchListRequest.MaxResults = 50;

            PlaylistItemListResponse searchListResponse = new();
            try
            {
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                logger.Error("YoutubeAPI.cs PlaylistSearch", ex.ToString());
            }

            keys[currentKey] += 1;

            if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
            {
                return SearchResultEnum.YoutubeNotFound;
            }

            foreach (PlaylistItem item in searchListResponse.Items)
            {
                await VideoSearch(youtubeService, item.ContentDetails.VideoId, username, serverId);
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

        //Checking if user wants to add playlist
        private async Task AddPlaylistAsync(YouTubeService youtubeService, NameValueCollection queryPart, string username, ulong serverId, ulong channelId)
        {
            try
            {
                bool result = await serviceDiscordCommunication.YoutubeAddPlaylistMessage(channelId);

                //Todo: Check if the given song is part of the first 25 songs that will be imported, and do not import it twice
                if (result)
                {
                    await PlaylistSearch(youtubeService, queryPart["list"], username, serverId);
                }
            }
            catch (Exception ex)
            {
                logger.Error("YoutubeAPI.cs AddPlaylistAsync", ex.ToString());
            }
        }

        //Resetting API key counters
        public static void KeyReset(string[] configKeys)
        {
            keys.Clear();
            foreach (string item in configKeys)
            {
                keys.Add(item, 0);
            }

            youtubeIndex = new Random().Next(0, keys.Count);
        }
        #endregion
    }
}