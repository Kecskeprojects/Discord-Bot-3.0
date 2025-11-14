using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.SpotifyAPI;
using Discord_Bot.Tools.NativeTools;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services;

public class SpotifyAPI(BotLogger logger, Config config, IYoutubeAPI youtubeAPI, IMusicBrainzAPI musicBrainzAPI) : ISpotifyAPI
{
    private readonly BotLogger logger = logger;
    private readonly Config config = config;
    private readonly IYoutubeAPI youtubeAPI = youtubeAPI;
    private readonly IMusicBrainzAPI musicBrainzAPI = musicBrainzAPI;

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
            logger.Error("SpotifyAPI.cs SpotifySearch", ex);
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
            else if (type is "playlist" or "album")
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

                if (!CollectionTools.IsNullOrEmpty(list))
                {
                    foreach (string track in list)
                    {
                        logger.Query($"List item: {track}");
                        _ = await youtubeAPI.Searching(track, username, serverId, channelId);
                    }

                    return SearchResultEnum.SpotifyPlaylistFound;
                }
            }
        }
        return SearchResultEnum.SpotifyNotFound;
    }
    #endregion

    #region Image search
    //Lastfm complimentary function
    public async Task<SpotifyImageSearchResult> SearchItemAsync(string artistMbid, string artistName, string songName = "")
    {
        try
        {
            if (string.IsNullOrEmpty(artistMbid))
            {
                logger.Log("Artist MBID is empty, defaulting to last.fm image URL.");
                return null;
            }
            string url = await musicBrainzAPI.GetArtistSpotifyUrlAsync(artistMbid);
            if (string.IsNullOrEmpty(url))
            {
                logger.Log("Musicbrainz entry not found for artist, defaulting to last.fm image URL.");
                return null;
            }

            string artistId = new Uri(url).Segments[^1];

            SpotifyClientConfig configuration = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(config.Spotify_Client_Id, config.Spotify_Client_Secret));
            SpotifyClient spotify = new(configuration);

            string spotifyArtist = "";

            logger.Query("============================================================================");
            logger.Query("Spotify image search:");

            SpotifyImageSearchResult result = new();
            if (string.IsNullOrEmpty(songName))
            {
                FullArtist artist = await spotify.Artists.Get(artistId);

                if (artist == null)
                {
                    logger.Query("Artist not found!");
                    logger.Query("============================================================================");
                    return null;
                }

                spotifyArtist = artist.Name;
                result.ImageUrl = artist.Images[0].Url;
                result.EntityUrl = artist.ExternalUrls["spotify"];
            }
            else
            {
                SearchRequest request = new(SearchRequest.Types.Track, $"{songName} {artistName}");
                SearchResponse searchResult = await spotify.Search.Item(request);

                if (searchResult.Tracks.Items.Count == 0)
                {
                    logger.Query("No results for track!");
                    logger.Query("============================================================================");
                    return null;
                }

                FullTrack track = searchResult.Tracks.Items.FirstOrDefault(x => x.Artists.Any(y => y.Id == artistId));

                if (track == null)
                {
                    logger.Query("Track not found!");
                    logger.Query("============================================================================");
                    return null;
                }

                spotifyArtist = track.Artists[0].Name;
                result.ImageUrl = track.Album.Images[0].Url;
                result.EntityUrl = track.ExternalUrls["spotify"];
            }

            logger.Query($"Artist found by Last.fm: {artistName}\nArtist found by Spotify: {spotifyArtist}\nWith image link: {result.ImageUrl}");
            logger.Query("============================================================================");

            return result;
        }
        catch (Exception ex)
        {
            logger.Error("SpotifyAPI.cs SearchItemAsync", ex);
        }
        logger.Log("Unknown exception during search, defaulting to last.fm image URL.");
        logger.Query("============================================================================");
        return null;
    }
    #endregion
}
