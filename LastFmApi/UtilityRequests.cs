using LastFmApi.Communication;
using LastFmApi.Enum;
using System.Text.RegularExpressions;

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
                GenericResponseItem<Models.TopArtist.Topartists> restResult = await UserBasedRequests.TopArtists(apiKey, username, 1000, page, period);
                response.RequestDetailList.Add(restResult.RequestDetails);

                if (restResult.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = restResult.Exception;
                    response.Message = restResult.Message;
                    response.ResultCode = restResult.ResultCode;
                    response.ErrorCode = restResult.ErrorCode;
                    return response;
                }

                foreach (Models.TopArtist.Artist artist in restResult.Response.Artist)
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
            int page = 1;
            int totalpage;
            List<Models.TopTrack.Track> allPlays = [];
            do
            {
                GenericResponseItem<Models.TopTrack.Toptracks> restResult = await UserBasedRequests.TopTracks(apiKey, username, 1000, page, "1month");
                response.RequestDetailList.Add(restResult.RequestDetails);

                if (restResult.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = restResult.Exception;
                    response.Message = restResult.Message;
                    response.ResultCode = restResult.ResultCode;
                    response.ErrorCode = restResult.ErrorCode;
                    return response;
                }

                allPlays.AddRange(restResult.Response.Track);

                if(allPlays.Any(track => track.Name == track_name && track.Artist.Name == artist_name))
                {
                    break;
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            List<IGrouping<int, Models.TopTrack.Track>> totalGroups = allPlays
                .GroupBy(t => int.Parse(t.PlayCount))
                .OrderByDescending(x => x.Key)
                .ToList();

            string position = null;
            for (int i = 0; i < totalGroups.Count; i++)
            {
                IGrouping<int, Models.TopTrack.Track> playcountGroup = totalGroups[i];
                if (playcountGroup.Any(track => track.Name == track_name && track.Artist.Name == artist_name))
                {
                    position = $"{i + 1}";
                    break;
                }
            }

            response.Response = position ?? $"{totalGroups.Count + 1}";
            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public static async Task<GenericResponseItem<List<Models.TopAlbum.Album>>> GetEveryAlbumUserListenedToFromArtistAsync(string apiKey, string username, string artistName)
    {
        GenericResponseItem<List<Models.TopAlbum.Album>> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure
        };
        try
        {
            List<Models.TopAlbum.Album> albums = [];
            int page = 1, totalpage;

            do
            {
                GenericResponseItem<Models.TopAlbum.Topalbums> restResult = await UserBasedRequests.TopAlbums(apiKey, username, 1000, page, null);
                response.RequestDetailList.Add(restResult.RequestDetails);

                if (restResult.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = restResult.Exception;
                    response.Message = restResult.Message;
                    response.ResultCode = restResult.ResultCode;
                    response.ErrorCode = restResult.ErrorCode;
                    return response;
                }

                foreach (Models.TopAlbum.Album album in restResult.Response.Album)
                {
                    if (album.Artist.Name.Equals(artistName, StringComparison.OrdinalIgnoreCase))
                    {
                        albums.Add(album);
                    }
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            response.Response = albums;
            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public static async Task<GenericResponseItem<List<Models.TopTrack.Track>>> GetEveryTrackUserListenedToFromArtistAsync(string apiKey, string username, string artistName)
    {
        GenericResponseItem<List<Models.TopTrack.Track>> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure
        };
        try
        {
            List<Models.TopTrack.Track> tracks = [];
            int page = 1, totalpage;

            do
            {
                GenericResponseItem<Models.TopTrack.Toptracks> restResult = await UserBasedRequests.TopTracks(apiKey, username, 1000, page, null);
                response.RequestDetailList.Add(restResult.RequestDetails);

                if (restResult.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = restResult.Exception;
                    response.Message = restResult.Message;
                    response.ResultCode = restResult.ResultCode;
                    response.ErrorCode = restResult.ErrorCode;
                    return response;
                }

                foreach (Models.TopTrack.Track track in restResult.Response.Track)
                {
                    if (track.Artist.Name.Equals(artistName, StringComparison.OrdinalIgnoreCase))
                    {
                        tracks.Add(track);
                    }
                }

                totalpage = int.Parse(restResult.Response.Attr.TotalPages);
                page++;
            } while (page <= totalpage);

            response.Response = tracks;
            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }
}
