using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.ArtistInfo;
using LastFmApi.Models.TrackInfo;
using Newtonsoft.Json;
using RestSharp;

namespace LastFmApi;

public sealed class InfoBasedRequests(BaseRequests baseRequests)
{
    private readonly BaseRequests baseRequests = baseRequests;

    #region Api getInfo calls
    public async Task<GenericResponseItem<Models.ArtistInfo.Artist>> ArtistPlays(string apiKey, string username, string artistName)
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

            RestResponse restResultJSON = await baseRequests.InfoBasedRequestHandler(request);
            ArtistInfo? deserialized = JsonConvert.DeserializeObject<ArtistInfo>(restResultJSON.Content ?? "");

            response.Response = deserialized?.Artist;
            response.ResultCode = LastFmHelper.GetResultCode(response.Response, deserialized?.Message);
            response.Message = deserialized?.Message;
            response.ErrorCode = deserialized?.Error;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public async Task<GenericResponseItem<Track>> TrackPlays(string apiKey, string username, string artistName, string trackName)
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

            RestResponse restResultJSON = await baseRequests.InfoBasedRequestHandler(request);
            TrackInfo? deserialized = JsonConvert.DeserializeObject<TrackInfo>(restResultJSON.Content ?? "");

            response.Response = deserialized?.Track;
            response.ResultCode = LastFmHelper.GetResultCode(response.Response, deserialized?.Message);
            response.Message = deserialized?.Message;
            response.ErrorCode = deserialized?.Error;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }
    #endregion
}
