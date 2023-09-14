using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    class SpotifyAPI : ISpotifyAPI
    {
        private readonly Logging logger;
        private readonly Config config;
        private readonly IYoutubeAPI youtubeAPI;

        public SpotifyAPI(Logging logger, Config config, IYoutubeAPI youtubeAPI)
        {
            this.logger = logger;
            this.config = config;
            this.youtubeAPI = youtubeAPI;
        }

        #region Main functions
        //Main function starting the query and catching errors
        public async Task<SearchResultEnum> SpotifySearch(string query, ulong serverId, ulong channelId, string username)
        {
            logger.Query("============================================================================");
            logger.Query("Spotify Data API: Search");

            try
            {
                SearchResultEnum result = await Run(query, serverId, channelId, username);

                logger.Query("Spotify query complete!");
                logger.Query("============================================================================");

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("SpotifyAPI.cs SpotifySearch", ex.ToString());
            }
            return SearchResultEnum.SpotifyNotFound;
        }


        //The function running the query
        private async Task<SearchResultEnum> Run(string query, ulong serverId, ulong channelId, string username)
        {
            SpotifyClientConfig configuration = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(config.Spotify_Client_Id, config.Spotify_Client_Secret));
            SpotifyClient spotify = new(configuration);

            //Spotify link format: https://open.spotify.com/[TYPE]/[ID]?query, the id is 22 characters long
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                Uri uri = new(query);

                string type = "";
                string id = "";
                if (uri.Segments.Length >= 4)
                {
                    type = uri.Segments[2].EndsWith('/') ? uri.Segments[2][..^1] : uri.Segments[2];
                    id = uri.Segments[3];
                }
                else if (uri.Segments.Length >= 3)
                {
                    type = uri.Segments[1].EndsWith('/') ? uri.Segments[1][..^1] : uri.Segments[1];
                    id = uri.Segments[2];
                }

                if (type == "track")
                {
                    FullTrack track = await spotify.Tracks.Get(id);

                    if (track != null)
                    {
                        string temp = $"{track.Name.Trim()} {track.Artists[0].Name.Trim()}";

                        logger.Query($"Result: {temp}");

                        return await youtubeAPI.Searching(temp, username, serverId, channelId) == SearchResultEnum.YoutubeFoundVideo
                            ? SearchResultEnum.SpotifyVideoFound
                            : SearchResultEnum.SpotifyFoundYoutubeNotFound;
                    }
                }
                else if (type == "playlist" || type == "album")
                {
                    string[] list = null;

                    if (type == "playlist")
                    {
                        Paging<PlaylistTrack<IPlayableItem>> playlist = await spotify.Playlists.GetItems(id, new PlaylistGetItemsRequest { Limit = 25 });

                        list = playlist.Items.Select(n => $"{(n.Track as FullTrack).Name.Trim()} {(n.Track as FullTrack).Artists[0].Name.Trim()}").ToArray();
                    }
                    else
                    {
                        Paging<SimpleTrack> album = await spotify.Albums.GetTracks(id);
                        list = album.Items.Select(n => $"{n.Name.Trim()} {n.Artists[0].Name.Trim()}").ToArray();
                    }

                    if (CollectionTools.IsNullOrEmpty(list))
                    {
                        foreach (string track in list)
                        {
                            logger.Query($"List item: {track}");
                            await youtubeAPI.Searching(track, username, serverId, channelId);
                        }

                        //Todo: Move to command level
                        //await context.Channel.SendMessageAsync("Playlist added!");

                        return SearchResultEnum.SpotifyPlaylistFound;
                    }
                }
            }
            return SearchResultEnum.SpotifyNotFound;
        }
        #endregion

        #region Image search
        //Lastfm complimentary function
        public async Task<string> ImageSearch(string artist, string song = "", string[] tags = null)
        {
            try
            {
                SpotifyClientConfig configuration = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(config.Spotify_Client_Id, config.Spotify_Client_Secret));
                SpotifyClient spotify = new(configuration);

                string spotifyArtist = "", spotifyImage = "";

                logger.Query("============================================================================");
                logger.Query("Spotify image search:");

                if (song == "")
                {
                    SearchRequest request = new(SearchRequest.Types.Artist, artist);
                    SearchResponse result = await spotify.Search.Item(request);

                    if (result.Artists.Items.Count == 0)
                    {
                        logger.Query("No results for artist!");
                        return "";
                    }

                    if (tags != null)
                    {
                        foreach (FullArtist item in result.Artists.Items)
                        {
                            List<string> artist_genres = item.Genres;
                            IEnumerable<string> union = artist_genres.Select(x => x.ToLower()).Intersect(tags.Select(x => x.ToLower()));
                            if (union.Any() && item.Name.ToLower() == artist.ToLower())
                            {
                                spotifyArtist = item.Name;
                                spotifyImage = item.Images[0].Url;
                                break;
                            }
                        }

                        if (spotifyArtist == "")
                        {
                            logger.Query("Genre and name check failed, finding first artist with just the same name");
                        }
                    }

                    if (spotifyArtist == "")
                    {
                        foreach (FullArtist item in result.Artists.Items)
                        {
                            if (item.Name.ToLower() == artist.ToLower())
                            {
                                spotifyArtist = item.Name;
                                spotifyImage = item.Images[0].Url;
                                break;
                            }
                        }

                        if (spotifyArtist == "")
                        {
                            logger.Query("Name only search also failed, returning first item on list");

                            spotifyArtist = result.Artists.Items[0].Name;
                            spotifyImage = result.Artists.Items[0].Images[0].Url;
                        }
                    }
                }
                else
                {
                    SearchRequest request = new(SearchRequest.Types.Track, song + " " + artist);
                    SearchResponse result = await spotify.Search.Item(request);

                    if (result.Tracks.Items.Count == 0)
                    {
                        logger.Query("No results for track!");
                        return "";
                    }

                    if (tags != null)
                    {
                        foreach (FullTrack item in result.Tracks.Items)
                        {
                            FullArtist temp_artist = await spotify.Artists.Get(item.Album.Artists[0].Id);

                            List<string> artist_genres = temp_artist.Genres;
                            IEnumerable<string> union = artist_genres.Select(x => x.ToLower()).Intersect(tags.Select(x => x.ToLower()));
                            if (union.Any() && item.Artists[0].Name.ToLower() == artist.ToLower())
                            {
                                spotifyArtist = item.Artists[0].Name;
                                spotifyImage = item.Album.Images[0].Url;
                                break;
                            }
                        }

                        if (spotifyArtist == "")
                        {
                            logger.Query("Genre and name check failed, finding first artist with just the same name");
                        }
                    }

                    if (spotifyArtist == "")
                    {
                        foreach (FullTrack item in result.Tracks.Items)
                        {
                            if (item.Artists[0].Name.ToLower() == artist.ToLower())
                            {
                                spotifyArtist = item.Artists[0].Name;
                                spotifyImage = item.Album.Images[0].Url;
                                break;
                            }
                        }

                        if (spotifyArtist == "")
                        {
                            logger.Query("Name only search also failed, returning first item on list");

                            spotifyArtist = result.Tracks.Items[0].Name;
                            spotifyImage = result.Tracks.Items[0].Album.Images[0].Url;
                        }
                    }
                }

                logger.Query($"Artist found by Last.fm: {artist}\nArtist found by Spotify: {spotifyArtist}\nWith image link: {spotifyImage}");
                logger.Query("============================================================================");

                return spotifyImage;
            }
            catch (Exception ex)
            {
                logger.Error("SpotifyAPI.cs ImageSearch", ex.ToString());
            }
            return "";
        }
        #endregion
    }
}
