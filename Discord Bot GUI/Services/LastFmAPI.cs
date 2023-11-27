﻿using Discord_Bot.Core.Config;
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
using LastFmApi.Models.Recent;
using Discord.Commands;
using System.Collections.Generic;
using Discord.Interactions;
using System.Linq;
using Discord_Bot.Resources;
using SpotifyAPI.Web;

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
                    result.ArtistMbid = restResult.Response.Artist.Mbid;

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
                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetNowPlayingAsync", ex.ToString());
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
            }
            return null;
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
                    for (int i = 0; i < restResult.Response.Album.Count && i < limit; i++)
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
                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetTopAlbumsAsync", ex.ToString());
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
            }
            return null;
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
                    for (int i = 0; i < restResult.Response.Artist.Count && i < limit; i++)
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
                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetTopArtistsAsync", ex.ToString());
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
            }
            return null;
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
                    for (int i = 0; i < restResult.Response.Track.Count && i < limit; i++)
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
                    logger.Error("LastFmAPI.cs GetNowPlayingAsync", restResult.Exception.ToString());
                }
                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetNowPlayingAsync", ex.ToString());
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
            }
            return null;
        }

        public async Task<LastFmListResult> GetRecentsAsync(string lastFmUsername, int limit)
        {
            LastFmListResult result = new();
            try
            {
                GenericResponseItem<Recenttracks> restResult = await UserBasedRequests.Recents(config.Lastfm_API_Key, lastFmUsername, limit);

                if (restResult.ResultCode == LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(restResult.Response.Track[0].Artist.Mbid, restResult.Response.Track[0].Artist.Text, restResult.Response.Track[0].Name);
                    if (spotifySearch != null)
                    {
                        result.ImageUrl = spotifySearch.ImageUrl;
                    }

                    int index = 0;
                    for (int i = 0; i < restResult.Response.Track.Count && i < limit; i++)
                    {
                        LastFmApi.Models.Recent.Track track = restResult.Response.Track[i];

                        result.EmbedFields[index] += $"`#{i}` **{track.Name}** by **{track.Artist.Text}** - *";
                        result.EmbedFields[index] += track.Attr != null ? "Now playing*" : track.Date.Text.Replace(DateTime.Now.Year.ToString(), "") + "*";
                        result.EmbedFields[index] += "\n";

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
                    logger.Error("LastFmAPI.cs GetRecentsAsync", restResult.Exception.ToString());
                }
                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
            }
            return null;
        }

        public async Task<ArtistStats> GetArtistDataAsync(string username, string artistName)
        {
            ArtistStats result = new(username);
            try
            {
                List<LastFmApi.Models.TopAlbum.Album> albums = await GetEveryAlbumUserListenedToFromArtist(username, artistName);

                //if there are no albums found, they have not listened to the artist
                if (albums.Count == 0)
                {
                    result.Message = "You haven't listened to this artist!";
                    return result;
                }

                List<LastFmApi.Models.TopTrack.Track> tracks = await GetEveryTrackUserListenedToFromArtist(username, artistName);

                SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(tracks[0].Artist.Mbid, tracks[0].Artist.Name, tracks[0].Name);
                if (spotifySearch != null)
                {
                    result.ImageUrl = spotifySearch.ImageUrl;
                }
                result.ArtistName = albums[0].Artist.Name;
                result.AlbumCount = albums.Count;
                result.TrackCount = tracks.Count;

                //Total plays of artist
                foreach (LastFmApi.Models.TopAlbum.Album album in albums)
                {
                    result.Playcount += int.Parse(album.PlayCount);
                }

                //Assembling list of top albums
                for(int i = 0; i < 5 && i < albums.Count; i++)
                {
                    result.AlbumField += $"`#{i}` **{albums[i].Name}**  (*{albums[i].PlayCount} plays*)\n";
                }

                //Assembling list of top tracks
                for (int i = 0; i < 8 && i < tracks.Count; i++)
                {
                    result.TrackField += $"`#{i}` **{tracks[i].Name}**  (*{tracks[i].PlayCount} plays*)\n";
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = "Last.fm is temporarily unavailable!";
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
                return result;
            }
            catch(Exception ex)
            {
                logger.Error("LastFmAPI.cs GetRecentsAsync", ex.ToString());
            }

            return null;
        }
        public async Task WhoKnowsByCurrentlyPlaying(WhoKnows wk, UserResource user)
        {
            //Check if they are playing something
            NowPlayingResult nowPlaying = await GetNowPlayingAsync(user.LastFmUsername);
            if (nowPlaying == null)
            {
                wk.Message = "Unexpected error occured during request!";
                return;
            }
            else if (!string.IsNullOrEmpty(nowPlaying.Message))
            {
                wk.Message = nowPlaying.Message;
                return;
            }

            //Get artist's name and the track for search
            string artist_name = nowPlaying.ArtistName;
            string track_name = nowPlaying.TrackName;

            foreach (var item in wk.Users)
            {
                //Get their number of plays on given song
                var request = await GetTrackPlaysAsync(item.LastFmUsername, artist_name, track_name);

                if (request != null)
                {
                    if (item == wk.Users[0])
                    {
                        //Save the names for use in embed
                        wk.Searched = $"{request.Name} by {request.Artist.Name}";

                        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(nowPlaying.ArtistMbid, artist_name, track_name);
                        if(spotifySearch != null)
                        {
                            wk.ImageUrl = spotifySearch.ImageUrl;
                        }
                    }

                    if (int.TryParse(request.Userplaycount, out int playcount) && playcount > 0)
                    {
                        //Add user to dictionary
                        wk.Plays.Add(item.Username, playcount);
                    }
                }
                else
                {
                    wk.Message = "No track found!";
                    break;
                }
            }
        }


        public async Task<WhoKnows> WhoKnowsByTrack(SocketCommandContext Context, WhoKnows wk, string input)
        {
            //Get artist's name and the track for search
            string artist_name = input.Split('>')[0].Trim().ToLower();
            string track_name = input.Split('>')[1].Trim().ToLower();

            foreach (var item in wk.Users)
            {
                //Get their number of plays on given song
                LastFmApi.Models.TrackInfo.Track request = await GetTrackPlaysAsync(item.LastFmUsername, artist_name, track_name);

                if (request != null)
                {
                    if (item == wk.Users[0])
                    {
                        //Save the names for use in embed
                        wk.Searched = $"{request.Name} by {request.Artist.Name}";

                        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(request.Artist.Mbid, artist_name, track_name);
                        if (spotifySearch != null)
                        {
                            wk.ImageUrl = spotifySearch.ImageUrl;
                        }
                    }

                    if (int.TryParse(request.Userplaycount, out int playcount) && playcount > 0)
                    {
                        //Add user to dictionary
                        wk.Plays.Add(item.Username, playcount);
                    }
                }
                else
                {
                    wk.Message = "No track found!";
                    break;
                }
            }

            return wk;
        }

        public async Task<WhoKnows> WhoKnowsByArtist(SocketCommandContext Context, WhoKnows wk, string input)
        {
            //Get artist's name for search
            string artist_name = input.Trim().ToLower();

            foreach (var item in wk.Users)
            {
                //Get their number of plays on given artists
                LastFmApi.Models.ArtistInfo.Artist request = await GetArtistPlaysAsync(item.Username, artist_name);
                if (request != null)
                {
                    if (item == wk.Users[0])
                    {
                        //Save the name for use in embed
                        wk.Searched = request.Name;

                        SpotifyImageSearchResult spotifySearch = await spotifyAPI.SearchItemAsync(request.Mbid, artist_name);
                        if (spotifySearch != null)
                        {
                            wk.ImageUrl = spotifySearch.ImageUrl;
                        }
                    }

                    if (int.TryParse(request.Stats.Userplaycount, out int playcount) && playcount > 0)
                    {
                        //Add user to dictionary
                        wk.Plays.Add(item.Username, playcount);
                    }
                }
                else
                {
                    wk.Message = "No artist found!";
                    break;
                }
            }

            return wk;
        }

        private async Task<LastFmApi.Models.ArtistInfo.Artist> GetArtistPlaysAsync(string lastFmUsername, string artistName)
        {
            GenericResponseItem<LastFmApi.Models.ArtistInfo.Artist> restResult = await InfoBasedRequests.ArtistPlays(config.Lastfm_API_Key, lastFmUsername, artistName);

            if (restResult.ResultCode != LastFmApi.Enum.LastFmRequestResultEnum.Success)
            {
                throw restResult.Exception ?? new Exception("Track plays request resulted in an error!");
            }

            return restResult.Response;
        }

        #region Helper API calls
        private async Task<LastFmApi.Models.TrackInfo.Track> GetTrackPlaysAsync(string lastFmUsername, string artistName, string trackName)
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

        private async Task<List<LastFmApi.Models.TopAlbum.Album>> GetEveryAlbumUserListenedToFromArtist(string username, string artistName)
        {
            List<LastFmApi.Models.TopAlbum.Album> albums = [];
            int page = 1, totalpage;

            do
            {
                //Getting data from api
                GenericResponseItem<Topalbums> restResult = await UserBasedRequests.TopAlbums(config.Lastfm_API_Key, username, 1000, page, null);

                if (restResult.ResultCode != LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    throw restResult.Exception ?? new Exception("Top albums request resulted in an error!");
                }

                foreach (var album in restResult.Response.Album)
                {
                    if (album.Artist.Name.Equals(artistName, StringComparison.OrdinalIgnoreCase)) albums.Add(album);
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            return albums;
        }

        private async Task<List<LastFmApi.Models.TopTrack.Track>> GetEveryTrackUserListenedToFromArtist(string username, string artistName)
        {
            List<LastFmApi.Models.TopTrack.Track> tracks = [];
            int page = 1, totalpage;

            do
            {
                //Getting data from api
                GenericResponseItem<Toptracks> restResult = await UserBasedRequests.TopTracks(config.Lastfm_API_Key, username, 1000, page, null);

                if (restResult.ResultCode != LastFmApi.Enum.LastFmRequestResultEnum.Success)
                {
                    throw restResult.Exception ?? new Exception("Top tracks request resulted in an error!");
                }

                foreach (var track in restResult.Response.Track)
                {
                    if (track.Artist.Name.Equals(artistName, StringComparison.OrdinalIgnoreCase)) tracks.Add(track);
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            return tracks;
        }
        #endregion
    }
}