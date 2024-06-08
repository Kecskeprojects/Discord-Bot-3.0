using LastFmApi.Communication;
using LastFmApi.Enum;

namespace LastFmApi;
public class WhoKnowsRequests
{
    public static async Task<GenericResponseItem<WhoKnowsResponseItem>> WhoKnowsByCurrentlyPlayingAsync(string apiKey, string username, List<string> usernameList)
    {
        GenericResponseItem<WhoKnowsResponseItem> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure,
            Response = new()
        };
        try
        {
            //Check if they are playing something
            GenericResponseItem<Models.Recent.Recenttracks> restNowPlaying = await UserBasedRequests.NowPlaying(apiKey, username);
            response.RequestDetailList.Add(restNowPlaying.RequestDetails);

            if (restNowPlaying.ResultCode != LastFmRequestResultEnum.Success)
            {
                response.Exception = restNowPlaying.Exception;
                response.Message = restNowPlaying.Message;
                response.ResultCode = restNowPlaying.ResultCode;
                response.ErrorCode = restNowPlaying.ErrorCode;
                return response;
            }

            Models.Recent.Track nowPlaying = restNowPlaying.Response.Track[0];

            //Get artist's name and the track for search
            string artist_name = nowPlaying.Artist.Text;
            string track_name = nowPlaying.Name;

            foreach (string user in usernameList)
            {
                //Get their number of plays on given song
                GenericResponseItem<Models.TrackInfo.Track> trackInfo =
                    await InfoBasedRequests.TrackPlays(apiKey, user, artist_name, track_name);
                response.RequestDetailList.Add(trackInfo.RequestDetails);
                Models.TrackInfo.Track trackRequest = trackInfo.Response;

                if (trackInfo.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = trackInfo.Exception;
                    response.Message = trackInfo.Message;
                    response.ResultCode = trackInfo.ResultCode;
                    response.ErrorCode = trackInfo.ErrorCode;
                    return response;
                }

                if (user == usernameList[0])
                {
                    await FillArtistDataAsync(apiKey, response, user, artist_name, track_name);
                    if (!string.IsNullOrEmpty(response.Message))
                    {
                        return response;
                    }
                }

                if (int.TryParse(trackRequest.Userplaycount, out int playcount) && playcount > 0)
                {
                    //Add user to dictionary
                    response.Response.Plays.Add(user, playcount);
                }
            }

            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public static async Task<GenericResponseItem<WhoKnowsResponseItem>> WhoKnowsByTrackAsync(string apiKey, string artistName, string trackName, List<string> usernameList)
    {
        GenericResponseItem<WhoKnowsResponseItem> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure,
            Response = new()
        };
        try
        {
            foreach (string user in usernameList)
            {
                //Get their number of plays on given song
                GenericResponseItem<Models.TrackInfo.Track> trackInfo =
                    await InfoBasedRequests.TrackPlays(apiKey, user, artistName, trackName);
                response.RequestDetailList.Add(trackInfo.RequestDetails);
                Models.TrackInfo.Track trackRequest = trackInfo.Response;

                if (trackInfo.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = trackInfo.Exception;
                    response.Message = trackInfo.Message;
                    response.ResultCode = trackInfo.ResultCode;
                    response.ErrorCode = trackInfo.ErrorCode;
                    return response;
                }

                if (user == usernameList[0])
                {
                    await FillArtistDataAsync(apiKey, response, user, artistName, trackName);
                    if (!string.IsNullOrEmpty(response.Message))
                    {
                        return response;
                    }
                }

                if (int.TryParse(trackRequest.Userplaycount, out int playcount) && playcount > 0)
                {
                    //Add user to dictionary
                    response.Response.Plays.Add(user, playcount);
                }
            }

            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    public static async Task<GenericResponseItem<WhoKnowsResponseItem>> WhoKnowsByArtistAsync(string apiKey, string artistName, List<string> usernameList)
    {
        GenericResponseItem<WhoKnowsResponseItem> response = new()
        {
            ResultCode = LastFmRequestResultEnum.Failure,
            Response = new()
        };
        try
        {

            foreach (string user in usernameList)
            {
                Models.ArtistInfo.Artist request = await FillArtistDataAsync(apiKey, response, user, artistName);
                if (!string.IsNullOrEmpty(response.Message))
                {
                    return response;
                }

                if (int.TryParse(request.Stats.Userplaycount, out int playcount) && playcount > 0)
                {
                    //Add user to dictionary
                    response.Response.Plays.Add(user, playcount);
                }
            }

            response.ResultCode = LastFmRequestResultEnum.Success;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
        }
        return response;
    }

    private static async Task<Models.ArtistInfo.Artist> FillArtistDataAsync(string apiKey, GenericResponseItem<WhoKnowsResponseItem> response, string userName, string artistName, string trackName = null)
    {
        //Get artist data for a few stats
        GenericResponseItem<Models.ArtistInfo.Artist> artistInfo =
            await InfoBasedRequests.ArtistPlays(apiKey, userName, artistName);
        response.RequestDetailList.Add(artistInfo.RequestDetails);
        Models.ArtistInfo.Artist artistRequest = artistInfo.Response;

        if (artistInfo.ResultCode != LastFmRequestResultEnum.Success)
        {
            response.Exception = artistInfo.Exception;
            response.Message = artistInfo.Message;
            response.ResultCode = artistInfo.ResultCode;
            response.ErrorCode = artistInfo.ErrorCode;
            return null;
        }
        response.Response.EmbedTitle = (string.IsNullOrEmpty(trackName) ? "" : $"{trackName} by ") + $"{artistRequest.Name}";
        response.Response.ImageUrl = artistRequest.Image?[^1].Text;
        response.Response.ArtistMbid = artistRequest.Mbid;
        response.Response.ArtistName = artistRequest.Name;
        response.Response.TrackName = trackName;

        return artistRequest;
    }
}
