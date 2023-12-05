using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.Recent;
using LastFmApi.Models.TopAlbum;
using LastFmApi.Models.TopArtist;
using LastFmApi.Models.TopTrack;
using Newtonsoft.Json;
using RestSharp;

namespace LastFmApi
{
    public class UserBasedRequests : BaseRequests
    {
        public static async Task<GenericResponseItem<Toptracks>> TopTracks(string apiKey, string username, int? limit, int? page, string period)
        {
            GenericResponseItem<Toptracks> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }
                UserBasedRequestItem request = new("user.gettoptracks", username, apiKey, limit, page, period);
                response.RequestDetails = new LastFmRequestDetails(request);

                RestResponse restResultJSON = await UserBasedRequestHandler(request);
                TopTrack deserialized = JsonConvert.DeserializeObject<TopTrack>(restResultJSON.Content);

                response.Response = deserialized.TopTracks;
                response.ResultCode = deserialized.TopTracks != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            return response;
        }

        public static async Task<GenericResponseItem<Topalbums>> TopAlbums(string apiKey, string username, int? limit, int? page, string period)
        {
            GenericResponseItem<Topalbums> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                UserBasedRequestItem request = new("user.gettopalbums", username, apiKey, limit, page, period);
                response.RequestDetails = new LastFmRequestDetails(request);

                //Getting data from api
                RestResponse restResultJSON = await UserBasedRequestHandler(request);
                TopAlbum deserialized = JsonConvert.DeserializeObject<TopAlbum>(restResultJSON.Content);

                response.Response = deserialized.TopAlbums;
                response.ResultCode = deserialized.TopAlbums != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            return response;
        }

        public static async Task<GenericResponseItem<Topartists>> TopArtists(string apiKey, string username, int? limit, int? page, string period)
        {
            GenericResponseItem<Topartists> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                UserBasedRequestItem request = new("user.gettopartists", username, apiKey, limit, page, period);
                response.RequestDetails = new LastFmRequestDetails(request);

                //Getting data from api
                RestResponse restResultJSON = await UserBasedRequestHandler(request);
                TopArtist deserialized = JsonConvert.DeserializeObject<TopArtist>(restResultJSON.Content);

                response.Response = deserialized.TopArtists;
                response.ResultCode = deserialized.TopArtists != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            return response;
        }

        public static async Task<GenericResponseItem<Recenttracks>> Recents(string apiKey, string username, int? limit)
        {
            GenericResponseItem<Recenttracks> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                UserBasedRequestItem request = new("user.getrecenttracks", username, apiKey) { Limit = limit };
                response.RequestDetails = new LastFmRequestDetails(request);

                //Getting data from api
                RestResponse restResultJSON = await UserBasedRequestHandler(request);
                Recent deserialized = JsonConvert.DeserializeObject<Recent>(restResultJSON.Content);

                response.Response = deserialized.RecentTracks;
                response.ResultCode = deserialized.RecentTracks != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            return response;
        }

        public static async Task<GenericResponseItem<Models.Recent.Track>> NowPlaying(string apiKey, string username)
        {
            GenericResponseItem<Models.Recent.Track> response = new() { ResultCode = LastFmRequestResultEnum.Failure };
            try
            {
                if (string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(username))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                UserBasedRequestItem request = new("user.getrecenttracks", username, apiKey) { Limit = 1 };
                response.RequestDetails = new LastFmRequestDetails(request);

                //Getting data from api
                RestResponse restResultJSON = await UserBasedRequestHandler(request);
                Recent deserialized = JsonConvert.DeserializeObject<Recent>(restResultJSON.Content);

                //If the Attr is not empty in the first index, it means the first song is a song that is currently playing
                response.Response = deserialized.RecentTracks?.Track.Count > 0 ?
                                    deserialized.RecentTracks?.Track[0] : null;
                response.ResultCode = deserialized.RecentTracks != null ?
                                        LastFmRequestResultEnum.Success :
                                        LastFmRequestResultEnum.EmptyResponse;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }

            return response;
        }
    }
}
