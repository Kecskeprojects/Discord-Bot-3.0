using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.TopArtist;
using LastFmApi.Models.TopTrack;
using RestSharp.Serializers;

namespace LastFmApi;
public class UtilityRequests
{
    public static async Task<GenericResponseItem<int>> TotalPlays(string apiKey, string username, string period)
    {
        GenericResponseItem<int> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure
        };
        try
        {
            int page = 1, totalpage, totalplays = 0;
            do
            {
                GenericResponseItem<Topartists> restResult = await UserBasedRequests.TopArtists(apiKey, username, 1000, page, period);
                response.RequestDetailList.Add(restResult.RequestDetails);

                if (restResult.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = restResult.Exception;
                    response.Message = restResult.Message;
                    response.ResultCode = restResult.ResultCode;
                    return response;
                }

                foreach (LastFmApi.Models.TopArtist.Artist artist in restResult.Response.Artist)
                {
                    totalplays += int.Parse(artist.PlayCount);
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            response.Response = totalplays;
            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public static async Task<GenericResponseItem<string>> GetSongMonthlyRankingAsync(string apiKey, string username, string artist_name, string track_name)
    {
        GenericResponseItem<string> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure
        };
        try
        {
            LastFmApi.Models.TopTrack.Attr attr = null;
            int page = 1, totalpage;
            string position = "";
            do
            {
                GenericResponseItem<Toptracks> restResult = await UserBasedRequests.TopTracks(apiKey, username, 1000, page, "1month");
                response.RequestDetailList.Add(restResult.RequestDetails);

                if (restResult.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = restResult.Exception;
                    response.Message = restResult.Message;
                    response.ResultCode = restResult.ResultCode;
                    return response;
                }

                for (int i = 0; i < restResult.Response.Track.Count; i++)
                {
                    LastFmApi.Models.TopTrack.Track track = restResult.Response.Track[i];
                    if (track.Name == track_name && track.Artist.Name == artist_name)
                    {
                        position = $"{i + 1 + ((page - 1) * 1000)}";
                    }
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                attr ??= restResult.Response.Attr;
                page++;
            } while (page <= totalpage);

            if (position == "")
            {
                position = attr.Total;

            }

            response.Response = position;
            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }
}
