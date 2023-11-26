using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.ArtistInfo;
using LastFmApi.Models.TrackInfo;
using Newtonsoft.Json;

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

                //Getting data from api
                var restResultJSON = await InfoBasedRequestHandler(request);
                ArtistInfo deserialized = JsonConvert.DeserializeObject<ArtistInfo>(restResultJSON.Content);

                if (deserialized.Artist != null)
                {
                    response.Response = deserialized.Artist;
                    response.ResultCode = LastFmRequestResultEnum.Success;
                }
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
                    string.IsNullOrEmpty(artistName))
                {
                    response.ResultCode = LastFmRequestResultEnum.RequiredParameterEmpty;
                    return response;
                }

                InfoBasedRequestItem request = new("track.getInfo", apiKey, username, artistName) { Track = trackName };

                //Getting data from api
                var temp = await InfoBasedRequestHandler(request);
                var deserialized = JsonConvert.DeserializeObject<TrackInfo>(temp.Content);

                if (deserialized.Track != null)
                {
                    response.Response = deserialized.Track;
                    response.ResultCode = LastFmRequestResultEnum.Success;
                }
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
