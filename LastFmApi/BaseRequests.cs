using LastFmApi.Communication;
using RestSharp;

namespace LastFmApi
{
    public class BaseRequests
    {
        public static string BaseUrl { get; } = "http://ws.audioscrobbler.com/2.0/";

        //The main request handling function
        private static readonly RestClient _client = new(BaseUrl);
        protected static async Task<RestResponse> UserBasedRequestHandler(UserBasedRequestItem item)
        {
            string query = $"?method={item.Type}&user={Uri.EscapeDataString(item.Username)}&api_key={item.ApiKey}";

            if (item.Limit.HasValue && item.Limit > 0)
            {
                query += $"&limit={item.Limit.Value}";
            }

            if (item.Page.HasValue && item.Page > 0)
            {
                query += $"&page={item.Page.Value}";
            }

            if (!string.IsNullOrEmpty(item.Period))
            {
                query += $"&period={item.Period}";
            }

            query += "&format=json";

            RestRequest request = new(query);
            return await _client.GetAsync(request);
        }

        protected static async Task<RestResponse> InfoBasedRequestHandler(InfoBasedRequestItem item)
        {
            string query = $"?method={item.Type}&api_key={item.ApiKey}&artist={Uri.EscapeDataString(item.Artist)}&username={Uri.EscapeDataString(item.Username)}";

            if (!string.IsNullOrEmpty(item.Track))
            {
                query += $"&track={Uri.EscapeDataString(item.Track)}";
            }

            if (!string.IsNullOrEmpty(item.Album))
            {
                query += $"&album={Uri.EscapeDataString(item.Album)}";
            }

            query += "&format=json";

            RestRequest request = new(query);
            return await _client.GetAsync(request);
        }
    }
}
