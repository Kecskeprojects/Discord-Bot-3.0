using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
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

    public class YoutubeAPI : IYoutubeAPI
    {
        private static readonly Dictionary<string, int> keys = new();
        private static int youtube_index = 0;
        private readonly Logging logger;
        private readonly Config config;

        public YoutubeAPI(Logging logger, Config config)
        {
            this.logger = logger;
            this.config = config;
        }

        #region Main functions
        //Main function starting the api request
        public async Task<SearchResultEnum> Searching(string query, string username, ulong serverId)
        {
            logger.Query("=======================================================================");
            logger.Query("YouTube Data API: Search");
            SearchResultEnum result = SearchResultEnum.NotFound;

            try
            {
                if (keys.Count != 0)
                {
                    result = await Run(query, username, serverId);
                }
                else
                {
                    logger.Query("All api key's limits have been exceeded!");
                }
                logger.Query("=======================================================================");
            }
            catch (Exception ex)
            {
                //switching api keys if quota is exceeded
                if ((ex.ToString().Contains("quotaExceeded") || ex.ToString().Contains("you have exceeded your")) && keys.Count != 0)
                {
                    string current_key = config.Youtube_API_Keys[youtube_index];

                    keys.Remove(current_key);

                    Random r = new();

                    youtube_index = r.Next(0, keys.Count);
                    current_key = config.Youtube_API_Keys[youtube_index];

                    logger.Warning("YoutubeAPI.cs Searching", ex.ToString(), LogOnly: true);
                    logger.Log($"Key switched out to key in {youtube_index} position, value: {current_key}!");

                    result = await Run(query, username, serverId);
                }
                else
                {
                    logger.Error("YoutubeAPI.cs Searching", ex.ToString());
                }
            }
            return result;
        }

        //The function running the query
        private async Task<SearchResultEnum> Run(string query, string username, ulong serverId)
        {
            string current_key = config.Youtube_API_Keys[youtube_index];

            YouTubeService youtubeService = new(new BaseClientService.Initializer()
            {
                ApiKey = current_key,
                ApplicationName = GetType().ToString()
            });

            logger.Query($"API key used: {current_key}");

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

                        //Todo: Move add playlist logic to Command level
                        //if (queryPart.AllKeys.Contains("list")) _ = AddPlaylist(context, youtubeService, queryPart["list"]);

                        return await VideoSearch(youtubeService, queryPart["v"], username, serverId);
                    }
                    else if (uri.Segments[1] == "playlist")
                    {
                        NameValueCollection queryPart = HttpUtility.ParseQueryString(uri.Query);

                        return await PlaylistSearch(youtubeService, queryPart["list"], username, serverId);
                    }
                    else
                    {
                        return SearchResultEnum.NotFound;
                    }
                }
            }

            //In any other case, search the result as a keyword
            return await KeywordSearch(youtubeService, query, username, serverId);
        }
        #endregion

        #region Api calls
        //Searching by video ID
        private async Task<SearchResultEnum> VideoSearch(YouTubeService youtubeService, string query, string username, ulong serverId)
        {
            string current_key = config.Youtube_API_Keys[youtube_index];

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

            keys[current_key] += 1;

            if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
            {
                logger.Query("No videos found or it is unlisted/private!");
                //Todo: Move message to Command level
                //await context.Channel.SendMessageAsync("No videos found or it is unlisted/private!");
                return SearchResultEnum.NotFound;
            }

            Video video = searchListResponse.Items[0];

            string[] temp = { "https://www.youtube.com/watch?v=" + video.Id, video.Snippet.Title.Replace("&#39;", "'"), video.Snippet.Thumbnails.Default__.Url, video.ContentDetails.Duration };

            if (!Global.ServerAudioResources.ContainsKey(serverId))
            {
                Global.ServerAudioResources.Add(serverId, new(serverId));
            }
            Global.ServerAudioResources[serverId].MusicRequests.Add(new MusicRequest(temp[0], temp[1], temp[2], temp[3], username));

            logger.Query("Youtube video query Complete!");
            logger.Query($"{temp[0]}\n{temp[1]}");

            return SearchResultEnum.FoundVideo;
        }

        //Searching by keywords
        private async Task<SearchResultEnum> KeywordSearch(YouTubeService youtubeService, string query, string username, ulong serverId)
        {
            string current_key = config.Youtube_API_Keys[youtube_index];

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

            keys[current_key] += 100;

            if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
            {
                logger.Query("No videos/playlists found or it is unlisted/private!");
                //Todo: Move message to Command level
                //await context.Channel.SendMessageAsync("No videos/playlists found or it is unlisted/private!");
                return SearchResultEnum.NotFound;
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
            return SearchResultEnum.NotFound;
        }

        private IList<SearchResult> FilteredResults(IList<SearchResult> items, string query)
        {
            //Filter youtube channels
            items = items.Where(x => x.Id.Kind != "youtube#channel").ToList();

            //Filter for trigger words
            string[] filterWords = config.Youtube_Filter_Words;
            if (!filterWords.Any(query.ToLower().Contains))
            {
                List<SearchResult> filteredList = items.Where(result => !filterWords.Any(result.Snippet.Title.ToLower().Contains)).ToList();
                if (filteredList.Count > 0)
                {
                    items = filteredList;
                }
            }

            return items;
        }

        //Searching by playlist ID
        private async Task<SearchResultEnum> PlaylistSearch(YouTubeService youtubeService, string query, string username, ulong serverId)
        {
            string current_key = config.Youtube_API_Keys[youtube_index];

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

            keys[current_key] += 1;

            if (searchListResponse.Items == null || searchListResponse.Items.Count < 1)
            {
                logger.Query("No playlist found or it is unlisted/private!");
                //Todo: Move message to Command level
                //await context.Channel.SendMessageAsync("No playlist found or it is unlisted/private!");
                return SearchResultEnum.NotFound;
            }

            foreach (PlaylistItem item in searchListResponse.Items)
            {
                await VideoSearch(youtubeService, item.ContentDetails.VideoId, username, serverId);
            }

            logger.Query("Youtube playlist query Complete!");
            //Todo: Move message to Command level
            //await context.Channel.SendMessageAsync("Playlist added!");

            return SearchResultEnum.FoundPlaylist;
        }

        //Todo: Move add playlist logic to Command level
        //Checking if user wants to add playlist
        /*private static async Task AddPlaylist(SocketCommandContext context, YouTubeService youtubeService, string playlist_id)
        {
            var message = await context.Channel.SendMessageAsync("You requested a song from a playlist!\n Do you want to me to add the playlist to the queue?");
            await message.AddReactionAsync(new Emoji("\U00002705"));

            //Wait 15 seconds for user to react to message, and then delete it, also delete it if they react, but add playlist
            int timer = 0;
            while (timer <= 15)
            {
                var result = await message.GetReactionUsersAsync(new Emoji("\U00002705"), 5).FlattenAsync();
                if (result.Count() > 1) 
                {
                    await PlaylistSearch(context, youtubeService, playlist_id);
                    break; 
                }
                await Task.Delay(1000);
                timer++;
            }
            await message.DeleteAsync();
        }*/
        #endregion



        #region Key reset method
        //Resetting API key counters
        public static void KeyReset(string[] configKeys)
        {
            keys.Clear();
            foreach (string item in configKeys) keys.Add(item, 0);

            youtube_index = new Random().Next(0, keys.Count);
        }
        #endregion
    }
}