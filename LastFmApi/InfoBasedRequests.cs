using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.ArtistInfo;
using LastFmApi.Models.TrackInfo;
using Newtonsoft.Json;
using RestSharp;
using System.Text.Json.Serialization;

namespace LastFmApi;

public class InfoBasedRequests : BaseRequests
{
    #region Api getInfo calls
    public static async Task<GenericResponseItem<Models.ArtistInfo.Artist>> ArtistPlays(string apiKey, string username, string artistName)
    {
        GenericResponseItem<Models.ArtistInfo.Artist> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure
        };
        try
        {
            if (string.IsNullOrEmpty(apiKey) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(artistName))
            {
                response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                return response;
            }

            InfoBasedRequestItem request = new("artist.getInfo", username, apiKey, artistName);
            response.RequestDetails = new LastFmRequestDetails(request);

            RestResponse restResultJSON = await InfoBasedRequestHandler(request);
            ArtistInfo deserialized = JsonConvert.DeserializeObject<ArtistInfo>(restResultJSON.Content);

            response.Response = deserialized.Artist;
            response.ResultCode = deserialized.Artist != null
                                    ? LastFmRequestResultEnum.Success
                                    : !string.IsNullOrEmpty(deserialized.Message)
                                        ? LastFmRequestResultEnum.Failure
                                        : LastFmRequestResultEnum.EmptyResponse;
            response.Message = deserialized.Message;
            response.ErrorCode = deserialized.Error;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public static async Task<GenericResponseItem<Track>> TrackPlays(string apiKey, string username, string artistName, string trackName)
    {
        GenericResponseItem<Track> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure
        };
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

            InfoBasedRequestItem request = new("track.getInfo", username, apiKey, artistName)
            {
                Track = trackName
            };
            response.RequestDetails = new LastFmRequestDetails(request);

            RestResponse restResultJSON = await InfoBasedRequestHandler(request);
            TrackInfo deserialized = JsonConvert.DeserializeObject<TrackInfo>(restResultJSON.Content);

            response.Response = deserialized.Track;
            response.ResultCode = deserialized.Track != null
                                    ? LastFmRequestResultEnum.Success
                                    : !string.IsNullOrEmpty(deserialized.Message)
                                        ? LastFmRequestResultEnum.Failure
                                        : LastFmRequestResultEnum.EmptyResponse;
            response.Message = deserialized.Message;
            response.ErrorCode = deserialized.Error;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }
    #endregion
}
