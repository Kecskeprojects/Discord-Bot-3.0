using LastFmApi.Communication;
using RestSharp;

namespace LastFmApi;

/// <summary>
/// Sends the two kinds of Last.fm requests. The application keeps one instance with one long-lived client,
/// as RestSharp recommends; tests pass a client whose message handler returns saved responses.
/// </summary>
public sealed class BaseRequests(RestClient client)
{
    private readonly RestClient client = client;

    internal async Task<RestResponse> UserBasedRequestHandler(UserBasedRequestItem item)
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
        return await client.GetAsync(request);
    }

    internal async Task<RestResponse> InfoBasedRequestHandler(InfoBasedRequestItem item)
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
        return await client.GetAsync(request);
    }
}
