using LastFmApi.Communication;
using LastFmApi.Enum;
using LastFmApi.Models.TopArtist;
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
}
