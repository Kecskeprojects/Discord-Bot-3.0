using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Services.Models.SpotifyAPI;
using Discord_Bot.Tools.LastFmTools;
using Discord_Bot.Tools.NativeTools;
using LastFmApi;
using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.Recent;
using LastFmApi.Models.TopAlbum;
using LastFmApi.Models.TopArtist;
using LastFmApi.Models.TopTrack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.Core.Enums;

namespace Discord_Bot.Services;

public class LastFmAPI(ISpotifyAPI spotifyAPI, BotLogger logger, Config config) : ILastFmAPI
{
    private readonly ISpotifyAPI spotifyAPI = spotifyAPI;
    private readonly BotLogger logger = logger;
    private readonly Config config = config;

    #region Last.fm top calls
    public async Task<LastFmListResult> GetTopAlbumsAsync(string lastFmUsername, int? limit, int? page, string period)
    {
        GenericResponseItem<Topalbums> restResult = await UserBasedRequests.TopAlbums(config.Lastfm_API_Key, lastFmUsername, limit, page, period);
        logger.Query("Last.fm request URL:\n" + restResult.RequestDetails.ToString());

        LastFmListResult result = new();
        if (restResult.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restResult.ResultCode, restResult.Message);
            if (restResult.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetTopAlbumsAsync", restResult.Exception);
            }
            return result;
        }
        List<LastFmApi.Models.TopAlbum.Album> albums = restResult.Response.Album;

        GenericResponseItem<int> playsResponse = await TotalPlaysAsync(lastFmUsername, period);
        if (!string.IsNullOrEmpty(playsResponse.Message))
        {
            result.Message = playsResponse.Message;
            return result;
        }

        result.TotalPlays = playsResponse.Response;

        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(albums[0].Artist.Mbid, albums[0].Artist.Name, albums[0].Name);
        result.ImageUrl = spotifySearch != null ? spotifySearch.ImageUrl : (albums[0].Image?[^1].Text);

        LastFmListResultTools.CreateTopAlbumList(limit, result, albums);

        return result;
    }

    public async Task<LastFmListResult> GetTopArtistsAsync(string lastFmUsername, int? limit, int? page, string period)
    {
        GenericResponseItem<Topartists> restResult = await UserBasedRequests.TopArtists(config.Lastfm_API_Key, lastFmUsername, limit, page, period);
        logger.Query("Last.fm request URL:\n" + restResult.RequestDetails.ToString());

        LastFmListResult result = new();
        if (restResult.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restResult.ResultCode, restResult.Message);
            if (restResult.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetTopArtistsAsync", restResult.Exception);
            }
            return result;
        }
        List<LastFmApi.Models.TopArtist.Artist> artists = restResult.Response.Artist;

        GenericResponseItem<int> playsResponse = await TotalPlaysAsync(lastFmUsername, period);
        if (!string.IsNullOrEmpty(playsResponse.Message))
        {
            result.Message = playsResponse.Message;
            return result;
        }

        result.TotalPlays = playsResponse.Response;

        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(artists[0].Mbid, artists[0].Name);
        result.ImageUrl = spotifySearch != null ? spotifySearch.ImageUrl : (artists[0].Image?[^1].Text);

        LastFmListResultTools.CreateTopArtistList(limit, result, artists);

        return result;
    }

    public async Task<LastFmListResult> GetTopTracksAsync(string lastFmUsername, int? limit, int? page, string period)
    {
        GenericResponseItem<Toptracks> restResult = await UserBasedRequests.TopTracks(config.Lastfm_API_Key, lastFmUsername, limit, page, period);
        logger.Query("Last.fm request URL:\n" + restResult.RequestDetails.ToString());

        LastFmListResult result = new();
        if (restResult.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restResult.ResultCode, restResult.Message);
            if (restResult.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetTopTracksAsync", restResult.Exception);
            }
            return result;
        }
        List<LastFmApi.Models.TopTrack.Track> tracks = restResult.Response.Track;

        GenericResponseItem<int> playsResponse = await TotalPlaysAsync(lastFmUsername, period);
        if (!string.IsNullOrEmpty(playsResponse.Message))
        {
            result.Message = playsResponse.Message;
            return result;
        }

        result.TotalPlays = playsResponse.Response;

        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(tracks[0].Artist.Mbid, tracks[0].Artist.Name, tracks[0].Name);
        result.ImageUrl = spotifySearch != null ? spotifySearch.ImageUrl : (tracks[0].Image?[^1].Text);

        LastFmListResultTools.CreateTopTrackList(limit, result, tracks);

        return result;
    }
    #endregion

    #region Last.fm recent calls
    public async Task<NowPlayingResult> GetNowPlayingAsync(string lastFmUsername)
    {
        GenericResponseItem<Recenttracks> restResult = await UserBasedRequests.NowPlaying(config.Lastfm_API_Key, lastFmUsername);
        logger.Query("Last.fm request URL:\n" + restResult.RequestDetails.ToString());

        NowPlayingResult result = new();
        if (restResult.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restResult.ResultCode, restResult.Message);
            if (restResult.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetNowPlayingAsync", restResult.Exception);
            }
            return result;
        }
        LastFmApi.Models.Recent.Track track = restResult.Response.Track[0];

        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(track.Artist.Mbid, track.Artist.Text, track.Name);

        GenericResponseItem<LastFmApi.Models.TrackInfo.Track> trackInfo =
            await InfoBasedRequests.TrackPlays(config.Lastfm_API_Key, lastFmUsername, track.Artist.Text, track.Name);
        if (trackInfo.ResultCode != LastFmRequestResultEnum.Success)
        {
            string resultMessage = LastFmHelper.GetResultMessage(trackInfo.ResultCode, trackInfo.Message);
            logger.Warning("LastFmAPI.cs GetNowPlayingAsync", $"Last.fm request response message: {resultMessage}");
            if (trackInfo.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetNowPlayingAsync", trackInfo.Exception);
            }
        }
        int? userplaycount = int.TryParse(trackInfo.Response?.Userplaycount, out int playCount) ? playCount + 1 : null;
        string playCountString = StringTools.AddNumberPositionIdentifier(userplaycount.ToString());

        string ranking = await GetSongMonthlyRankingAsync(lastFmUsername, track.Artist.Text, track.Name);

        LastFmNowPlayingTools.MapNowPlayingData(restResult, track, result, spotifySearch, ranking, playCountString);

        return result;
    }

    public async Task<LastFmListResult> GetRecentsAsync(string lastFmUsername, int limit)
    {
        GenericResponseItem<Recenttracks> restResult = await UserBasedRequests.Recents(config.Lastfm_API_Key, lastFmUsername, limit);
        logger.Query("Last.fm request URL:\n" + restResult.RequestDetails.ToString());

        LastFmListResult result = new();
        if (restResult.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restResult.ResultCode, restResult.Message);
            if (restResult.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", restResult.Exception);
            }
            return result;
        }
        List<LastFmApi.Models.Recent.Track> tracks = restResult.Response.Track;

        GenericResponseItem<int> playsResponse = await TotalPlaysAsync(lastFmUsername);
        if (!string.IsNullOrEmpty(playsResponse.Message))
        {
            result.Message = playsResponse.Message;
            return result;
        }

        result.TotalPlays = playsResponse.Response;

        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(tracks[0].Artist.Mbid, tracks[0].Artist.Text, tracks[0].Name);
        result.ImageUrl = spotifySearch != null ? spotifySearch.ImageUrl : (tracks[0].Image?[^1].Text);

        LastFmListResultTools.CreateRecentList(limit, result, tracks);

        return result;
    }
    #endregion

    #region Last.fm advanced calls
    public async Task<ArtistStats> GetArtistDataAsync(string username, string artistName)
    {
        ArtistStats result = new(username);

        GenericResponseItem<List<LastFmApi.Models.TopAlbum.Album>> restAlbum = await GetEveryAlbumUserListenedToFromArtistAsync(username, artistName);

        if (restAlbum.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restAlbum.ResultCode, restAlbum.Message);
            if (restAlbum.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetArtistDataAsync", restAlbum.Exception);
            }
            return result;
        }
        List<LastFmApi.Models.TopAlbum.Album> albums = restAlbum.Response;

        GenericResponseItem<List<LastFmApi.Models.TopTrack.Track>> restTrack = await GetEveryTrackUserListenedToFromArtistAsync(username, artistName);

        if (restTrack.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restTrack.ResultCode, restTrack.Message);
            if (restTrack.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetArtistDataAsync", restTrack.Exception);
            }
            return result;
        }
        List<LastFmApi.Models.TopTrack.Track> tracks = restTrack.Response;

        //if there are no albums found, they have not listened to the artist
        if (albums.Count == 0 && tracks.Count == 0)
        {
            result.Message = "You haven't listened to this artist!";
            return result;
        }

        string mbid = albums.Count == 0 ? tracks[0].Artist.Mbid : albums[0].Artist.Mbid;
        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(mbid, tracks[0].Artist.Name, tracks[0].Name);
        result.ImageUrl = spotifySearch != null ? spotifySearch.ImageUrl : (albums.Count == 0 ? tracks[0].Image?[^1].Text : albums[0].Image?[^1].Text);

        LastFmArtistTools.MapArtistData(result, albums, tracks);

        return result;
    }

    public async Task<WhoKnows> GetWhoKnowsDataAsync(string input, List<UserResource> users, UserResource currentUser)
    {
        //Variable declarations
        WhoKnows result = new(users);

        //In case user doesn't give a song, we check if they are playing something
        GenericResponseItem<WhoKnowsResponseItem> restResult;
        List<string> usernameList = users.Select(x => x.LastFmUsername).ToList();
        if (input == "")
        {
            restResult = await WhoKnowsRequests.WhoKnowsByCurrentlyPlayingAsync(config.Lastfm_API_Key, currentUser.LastFmUsername, usernameList);
        }
        else if (input.Contains('>'))
        {
            //Get artist's name and the track for search
            string artistName = input.Split('>')[0].Trim().ToLower();
            string trackName = input.Split('>')[1].Trim().ToLower();

            restResult = await WhoKnowsRequests.WhoKnowsByTrackAsync(config.Lastfm_API_Key, artistName, trackName, usernameList);
        }
        else
        {
            //Get artist's name for search
            string artistName = input.Trim().ToLower();

            restResult = await WhoKnowsRequests.WhoKnowsByArtistAsync(config.Lastfm_API_Key, artistName, usernameList);
        }

        foreach (LastFmRequestDetails item in restResult.RequestDetailList)
        {
            logger.Query("Last.fm request URL:\n" + item.ToString());
        }

        if (restResult.ResultCode != LastFmRequestResultEnum.Success)
        {
            result.Message = LastFmHelper.GetResultMessage(restResult.ResultCode, restResult.Message);
            if (restResult.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetWhoKnowsDataAsync", restResult.Exception);
            }
            return result;
        }

        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(restResult.Response.ArtistMbid, restResult.Response.ArtistName, restResult.Response.TrackName);
        result.ImageUrl = spotifySearch != null ? spotifySearch.ImageUrl : restResult.Response.ImageUrl;

        result.EmbedTitle = restResult.Response.EmbedTitle;

        if (restResult.Response.Plays.Count == 0)
        {
            result.Message = "No one has listened to this song/artist according to last.fm!";
            return result;
        }

        result.Plays = restResult.Response.Plays.OrderByDescending(x => x.Value).ToDictionary(x => users.First(y => y.LastFmUsername == x.Key).Username, x => x.Value);

        return result;
    }
    #endregion

    #region Helper API calls
    private async Task<GenericResponseItem<int>> TotalPlaysAsync(string lastFmUsername, string period = null)
    {
        GenericResponseItem<int> totalPlay =
            await UtilityRequests.TotalPlays(config.Lastfm_API_Key, lastFmUsername, period);

        foreach (LastFmRequestDetails item in totalPlay.RequestDetailList)
        {
            logger.Query("Last.fm request URL:\n" + item.ToString());
        }

        if (totalPlay.ResultCode != LastFmRequestResultEnum.Success)
        {
            totalPlay.Message = LastFmHelper.GetResultMessage(totalPlay.ResultCode, totalPlay.Message);
            if (totalPlay.Exception != null)
            {
                logger.Error("LastFmAPI.cs TotalPlaysAsync", totalPlay.Exception);
            }
        }

        return totalPlay;
    }

    private async Task<string> GetSongMonthlyRankingAsync(string username, string artist_name, string track_name)
    {
        GenericResponseItem<string> ranking =
            await UtilityRequests.GetSongMonthlyRankingAsync(config.Lastfm_API_Key, username, artist_name, track_name);

        foreach (LastFmRequestDetails item in ranking.RequestDetailList)
        {
            logger.Query("Last.fm request URL:\n" + item.ToString());
        }

        if (ranking.ResultCode != LastFmRequestResultEnum.Success)
        {
            string resultMessage = LastFmHelper.GetResultMessage(ranking.ResultCode, ranking.Message);
            logger.Warning("LastFmAPI.cs GetNowPlayingAsync", $"Last.fm request response message: {resultMessage}");
            if (ranking.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetSongMonthlyRankingAsync", ranking.Exception);
            }
        }
        ranking.Response = StringTools.AddNumberPositionIdentifier(ranking.Response);

        return ranking.Response;
    }

    private async Task<GenericResponseItem<List<LastFmApi.Models.TopAlbum.Album>>> GetEveryAlbumUserListenedToFromArtistAsync(string username, string artistName)
    {
        GenericResponseItem<List<LastFmApi.Models.TopAlbum.Album>> albumsListenedTo =
            await UtilityRequests.GetEveryAlbumUserListenedToFromArtistAsync(config.Lastfm_API_Key, username, artistName);

        foreach (LastFmRequestDetails item in albumsListenedTo.RequestDetailList)
        {
            logger.Query("Last.fm request URL:\n" + item.ToString());
        }

        if (albumsListenedTo.ResultCode != LastFmRequestResultEnum.Success)
        {
            albumsListenedTo.Message = LastFmHelper.GetResultMessage(albumsListenedTo.ResultCode, albumsListenedTo.Message);
            if (albumsListenedTo.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetEveryAlbumUserListenedToFromArtistAsync", albumsListenedTo.Exception);
            }
        }

        return albumsListenedTo;
    }

    private async Task<GenericResponseItem<List<LastFmApi.Models.TopTrack.Track>>> GetEveryTrackUserListenedToFromArtistAsync(string username, string artistName)
    {
        GenericResponseItem<List<LastFmApi.Models.TopTrack.Track>> tracksListenedTo =
            await UtilityRequests.GetEveryTrackUserListenedToFromArtistAsync(config.Lastfm_API_Key, username, artistName);

        foreach (LastFmRequestDetails item in tracksListenedTo.RequestDetailList)
        {
            logger.Query("Last.fm request URL:\n" + item.ToString());
        }

        if (tracksListenedTo.ResultCode != LastFmRequestResultEnum.Success)
        {
            tracksListenedTo.Message = LastFmHelper.GetResultMessage(tracksListenedTo.ResultCode, tracksListenedTo.Message);
            if (tracksListenedTo.Exception != null)
            {
                logger.Error("LastFmAPI.cs GetEveryTrackUserListenedToFromArtistAsync", tracksListenedTo.Exception);
            }
        }

        return tracksListenedTo;
    }
    #endregion
}