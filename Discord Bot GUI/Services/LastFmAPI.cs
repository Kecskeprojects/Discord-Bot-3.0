using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Services;
using LastFmApi;
using LastFmApi.Communication;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using LastFmApi.Models.TopTrack;
using Discord_Bot.Services.Models.LastFm;
using LastFmApi.Models.TopArtist;
using LastFmApi.Models.TopAlbum;
using Discord_Bot.Tools;
using Discord_Bot.Services.Models.SpotifyAPI;

namespace Discord_Bot.Services
{
    public class LastFmAPI(ISpotifyAPI spotifyAPI, Logging logger, Config config) : ILastFmAPI
    {
        private readonly ISpotifyAPI spotifyAPI = spotifyAPI;
        private readonly Logging logger = logger;
        private readonly Config config = config;

        public async Task<NowPlayingResult> GetNowPlayingAsync(string lastFmUsername)
        {
            NowPlayingResult result = new();
            try
            {
                GenericResponseItem<LastFmApi.Models.Recent.Track> restResult = await UserBasedRequests.NowPlaying(config.Lastfm_API_Key, lastFmUsername);

                if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    result.Attr = restResult.Response.Attr;
                    result.TrackName = restResult.Response.Name;
                    result.ArtistName = restResult.Response.Artist.Text;
                    result.AlbumName = restResult.Response.Album.Text;

                    SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(restResult.Response.Artist.Mbid, restResult.Response.Artist.Text, restResult.Response.Name);
                    if(spotifySearch != null)
                    {
                        result.ImageUrl = spotifySearch.ImageUrl;
                        result.Url = spotifySearch.EntityUrl;
                    }

                    LastFmApi.Models.TrackInfo.Track track = await GetTrackPlaysAsync(lastFmUsername, restResult.Response.Artist.Text, restResult.Response.Name);
                    string userplaycount = (int.Parse(track.Userplaycount) + 1).ToString();

                    result.TrackPlays = StringTools.AddNumberPositionIdentifier(userplaycount);

                    result.Ranking = await GetSongMonthlyRankingAsync(lastFmUsername, restResult.Response.Artist.Text, restResult.Response.Name);
                }
                else if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.RequiredParameterEmpty)
                {
                    result.Message = "Error during request!\nCheck to make sure if you set your lastfm username correctly!";
                }
                else
                {
                    result.Message = "Unexpected exception occured!";
                    logger.Error("LastFmAPI.cs GetNowPlayingAsync", restResult.Exception.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetNowPlayingAsync", ex.ToString());
            }
            return result;
        }

        public async Task<LastFmListResult> GetTopAlbumsAsync(string lastFmUsername, int? limit, int? page, string period)
        {
            LastFmListResult result = new();
            try
            {
                GenericResponseItem<Topalbums> restResult = await UserBasedRequests.TopAlbums(config.Lastfm_API_Key, lastFmUsername, limit, page, period);

                if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    result.TotalPlays = await TotalPlays(lastFmUsername, period);

                    SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(restResult.Response.Album[0].Artist.Mbid, restResult.Response.Album[0].Artist.Name, restResult.Response.Album[0].Name);
                    if (spotifySearch != null)
                    {
                        result.ImageUrl = spotifySearch.ImageUrl;
                    }

                    int index = 0;
                    for (int i = 0; i < restResult.Response.Album.Count || i > 30; i++)
                    {
                        LastFmApi.Models.TopAlbum.Album album = restResult.Response.Album[i];
                        //One line in the embed
                        result.EmbedFields[index] += $"`#{i}`**{album.Name}** by **{album.Artist.Name}** - *{Math.Round(double.Parse(album.PlayCount) / result.TotalPlays * 100)}%* (*{album.PlayCount} plays*)\n";

                        //If we went through 10 results, start filling a new list page
                        if (i % 10 == 0) index++;
                    }
                }
                else if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.RequiredParameterEmpty)
                {
                    result.Message = "Error during request!\nCheck to make sure if you set your lastfm username correctly!";
                }
                else
                {
                    result.Message = "Unexpected exception occured!";
                    logger.Error("LastFmAPI.cs GetTopAlbumsAsync", restResult.Exception.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetTopAlbumsAsync", ex.ToString());
            }
            return result;
        }

        public async Task<LastFmListResult> GetTopArtistsAsync(string lastFmUsername, int? limit, int? page, string period)
        {
            LastFmListResult result = new();
            try
            {
                GenericResponseItem<Topartists> restResult = await UserBasedRequests.TopArtists(config.Lastfm_API_Key, lastFmUsername, limit, page, period);

                if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    result.TotalPlays = await TotalPlays(lastFmUsername, period);

                    SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(restResult.Response.Artist[0].Mbid, restResult.Response.Artist[0].Name);
                    if (spotifySearch != null)
                    {
                        result.ImageUrl = spotifySearch.ImageUrl;
                    }

                    int index = 0;
                    for (int i = 0; i < restResult.Response.Artist.Count || i > 30; i++)
                    {
                        LastFmApi.Models.TopArtist.Artist artist = restResult.Response.Artist[i];
                        //One line in the embed
                        result.EmbedFields[index] += $"`#{i}`**{artist.Name}** - *{Math.Round(double.Parse(artist.PlayCount) / result.TotalPlays * 100)}%* (*{artist.PlayCount} plays*)\n";

                        //If we went through 10 results, start filling a new list page
                        if (i % 10 == 0) index++;
                    }
                }
                else if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.RequiredParameterEmpty)
                {
                    result.Message = "Error during request!\nCheck to make sure if you set your lastfm username correctly!";
                }
                else
                {
                    result.Message = "Unexpected exception occured!";
                    logger.Error("LastFmAPI.cs GetTopArtistsAsync", restResult.Exception.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetTopArtistsAsync", ex.ToString());
            }
            return result;
        }

        public async Task<LastFmListResult> GetTopTracksAsync(string lastFmUsername, int? limit, int? page, string period)
        {
            LastFmListResult result = new();
            try
            {
                GenericResponseItem<Toptracks> restResult = await UserBasedRequests.TopTracks(config.Lastfm_API_Key, lastFmUsername, limit, page, period);

                if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    result.TotalPlays = await TotalPlays(lastFmUsername, period);

                    SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(restResult.Response.Track[0].Artist.Mbid, restResult.Response.Track[0].Artist.Name, restResult.Response.Track[0].Name);
                    if (spotifySearch != null)
                    {
                        result.ImageUrl = spotifySearch.ImageUrl;
                    }

                    int index = 0;
                    for (int i = 0; i < restResult.Response.Track.Count || i > 30; i++)
                    {
                        LastFmApi.Models.TopTrack.Track track = restResult.Response.Track[i];
                        //One line in the embed
                        result.EmbedFields[index] += $"`#{i}`**{track.Name}** by **{track.Artist.Name}** - *{Math.Round(double.Parse(track.PlayCount) / result.TotalPlays * 100)}%* (*{track.PlayCount} plays*)\n";

                        //If we went through 10 results, start filling a new list page
                        if (i % 10 == 0) index++;
                    }
                }
                else if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.RequiredParameterEmpty)
                {
                    result.Message = "Error during request!\nCheck to make sure if you set your lastfm username correctly!";
                }
                else
                {
                    result.Message = "Error during request!\nCheck to make sure if you set your lastfm username correctly!";
                    logger.Error("LastFmAPI.cs GetTopTracksAsync", restResult.Exception.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetTopTracksAsync", ex.ToString());
            }
            return result;
        }

        public async Task<LastFmApi.Models.TrackInfo.Track> GetTrackPlaysAsync(string lastFmUsername, string artistName, string trackName)
        {
            GenericResponseItem<LastFmApi.Models.TrackInfo.Track> restResult = await InfoBasedRequests.TrackPlays(config.Lastfm_API_Key, lastFmUsername, artistName, trackName);

            if (restResult.ResultCode != LastFmApi.Enum.LastFmRequestResultEnum.Success)
            {
                throw restResult.Exception ?? new Exception("Track plays request resulted in an error!");
            }

            return restResult.Response;
        }

        //Checking the total plays of a user using the topartist part of the api, as it has the least amount of entries in any case
        private async Task<int> TotalPlays(string lastFmUsername, string period)
        {
            int page = 1, totalpage, totalplays = 0;
            do
            {
                GenericResponseItem<Topartists> restResult = await UserBasedRequests.TopArtists(config.Lastfm_API_Key, lastFmUsername, 1000, page, period);

                if (restResult.ResultCode != LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    throw restResult.Exception ?? new Exception("Total plays request resulted in an error!");
                }

                foreach (var artist in restResult.Response.Artist)
                {
                    totalplays += int.Parse(artist.PlayCount);
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            return totalplays;
        }

        private async Task<string> GetSongMonthlyRankingAsync(string username, string artist_name, string track_name)
        {
            int page = 1, totalpage;
            LastFmApi.Models.TopTrack.Attr attr;
            string position = "";
            do
            {
                GenericResponseItem<Toptracks> restResult = await UserBasedRequests.TopTracks(config.Lastfm_API_Key, username, 1000, page, "1month");

                if (restResult.ResultCode != LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    throw restResult.Exception ?? new Exception("Monthly ranking request resulted in an error!");
                }

                for (int i = 0; i < restResult.Response.Track.Count; i++)
                {
                    var track = restResult.Response.Track[i];
                    if (track.Name == track_name && track.Artist.Name == artist_name)
                    {
                        position = $"{i + ((page - 1) * 1000)}";
                    }
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                attr = restResult.Response.Attr;
                page++;
            } while (page <= totalpage);

            if (position == "")
            {
                position = attr.Total;

            }

            position = StringTools.AddNumberPositionIdentifier(position);

            return position;
        }
    }
}