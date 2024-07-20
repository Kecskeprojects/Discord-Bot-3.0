namespace LastFmApi.Communication;

public class LastFmRequestDetails
{
    public override string ToString()
    {
        string FinalUrl = $"{Constant.LastFmApiBaseUri.OriginalString}?method={Type}";

        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            FinalUrl += $"&api_key={ApiKey}";
        }

        if (!string.IsNullOrWhiteSpace(UserName))
        {
            if (Type.EndsWith("getInfo"))
            {
                FinalUrl += $"&username={UserName}";
            }
            FinalUrl += $"&user={Uri.EscapeDataString(UserName)}";
        }

        if (!string.IsNullOrWhiteSpace(Limit))
        {
            FinalUrl += $"&limit={Limit}";
        }

        if (!string.IsNullOrWhiteSpace(Page))
        {
            FinalUrl += $"&page={Page}";
        }

        if (!string.IsNullOrWhiteSpace(Period))
        {
            FinalUrl += $"&period={Period}";
        }

        if (!string.IsNullOrWhiteSpace(ArtistName))
        {
            FinalUrl += $"&artist={Uri.EscapeDataString(ArtistName)}";
        }

        if (!string.IsNullOrWhiteSpace(TrackName))
        {
            FinalUrl += $"&track={Uri.EscapeDataString(TrackName)}";
        }

        if (!string.IsNullOrWhiteSpace(AlbumName))
        {
            FinalUrl += $"&album={Uri.EscapeDataString(AlbumName)}";
        }

        FinalUrl += "&format=json";

        return FinalUrl;
    }

    public LastFmRequestDetails(InfoBasedRequestItem infoBasedRequest)
    {
        Type = infoBasedRequest.Type ?? "";
        ApiKey = infoBasedRequest.ApiKey;
        UserName = infoBasedRequest.Username;
        TrackName = infoBasedRequest.Track;
        AlbumName = infoBasedRequest.Album;
        ArtistName = infoBasedRequest.Artist;
    }

    public LastFmRequestDetails(UserBasedRequestItem userBasedRequest)
    {
        Type = userBasedRequest.Type ?? "";
        ApiKey = userBasedRequest.ApiKey;
        UserName = userBasedRequest.Username;
        Limit = userBasedRequest.Limit.ToString();
        Page = userBasedRequest.Page.ToString();
        Period = userBasedRequest.Period;
    }

    public string Type { get; private set; }
    public string ApiKey { get; private set; }
    public string UserName { get; private set; }
    public string Limit { get; private set; }
    public string Page { get; private set; }
    public string Period { get; private set; }
    public string ArtistName { get; private set; }
    public string TrackName { get; private set; }
    public string AlbumName { get; private set; }
}
