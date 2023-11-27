﻿using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.ArtistInfo;
using LastFmApi.Models.TrackInfo;
using Newtonsoft.Json;
using RestSharp;

namespace LastFmApi
{
    public class InfoBasedRequests : BaseRequests
    {
        #region Api getInfo calls
        public static async Task<GenericResponseItem<Models.ArtistInfo.Artist>> ArtistPlays(string apiKey, string username, string artistName)
        {
            GenericResponseItem<Models.ArtistInfo.Artist> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username) ||
                    string.IsNullOrEmpty(artistName))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                InfoBasedRequestItem request = new("artist.getInfo", apiKey, username, artistName);

                RestResponse restResultJSON = await InfoBasedRequestHandler(request);
                ArtistInfo deserialized = JsonConvert.DeserializeObject<ArtistInfo>(restResultJSON.Content);

                response.Response = deserialized.Artist;
                response.ResultCode = deserialized.Artist != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            return response;
        }

        public static async Task<GenericResponseItem<Track>> TrackPlays(string apiKey, string username, string artistName, string trackName)
        {
            GenericResponseItem<Track> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username) ||
                    string.IsNullOrEmpty(artistName) ||
                    string.IsNullOrEmpty(trackName))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                InfoBasedRequestItem request = new("track.getInfo", apiKey, username, artistName) { Track = trackName };

                RestResponse restResultJSON = await InfoBasedRequestHandler(request);
                TrackInfo deserialized = JsonConvert.DeserializeObject<TrackInfo>(restResultJSON.Content);

                response.Response = deserialized.Track;
                response.ResultCode = deserialized.Track != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            return response;
        }
        #endregion
    }
}
