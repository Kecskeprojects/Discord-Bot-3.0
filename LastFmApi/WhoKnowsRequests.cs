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

            if (restNowPlaying.ResultCode != LastFmRequestResultEnum.Success)
            {
                response.Exception = restNowPlaying.Exception;
                response.Message = restNowPlaying.Message;
                response.ResultCode = restNowPlaying.ResultCode;
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
                Models.TrackInfo.Track request = trackInfo.Response;

                if (trackInfo.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = trackInfo.Exception;
                    response.Message = trackInfo.Message;
                    response.ResultCode = trackInfo.ResultCode;
                    return response;
                }
                if (user == usernameList[0])
                {
                    response.Response.EmbedTitle = $"{request.Name} by {request.Artist.Name}";
                    response.Response.ImageUrl = request.Album.Image?[^1].Text;
                    response.Response.ArtistMbid = request.Artist.Mbid;
                    response.Response.ArtistName = request.Artist.Name;
                    response.Response.TrackName = request.Name;
                }

                if (int.TryParse(request.Userplaycount, out int playcount) && playcount > 0)
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
                Models.TrackInfo.Track request = trackInfo.Response;

                if (trackInfo.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = trackInfo.Exception;
                    response.Message = trackInfo.Message;
                    response.ResultCode = trackInfo.ResultCode;
                    return response;
                }

                if (user == usernameList[0])
                {
                    response.Response.EmbedTitle = $"{request.Name} by {request.Artist.Name}";
                    response.Response.ImageUrl = request.Album.Image?[^1].Text;
                    response.Response.ArtistMbid = request.Artist.Mbid;
                    response.Response.ArtistName = request.Artist.Name;
                    response.Response.TrackName = request.Name;
                }

                if (int.TryParse(request.Userplaycount, out int playcount) && playcount > 0)
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
                //Get their number of plays on given artists
                GenericResponseItem<Models.ArtistInfo.Artist> artistInfo = await InfoBasedRequests.ArtistPlays(apiKey, user, artistName);
                Models.ArtistInfo.Artist request = artistInfo.Response;

                if (artistInfo.ResultCode != LastFmRequestResultEnum.Success)
                {
                    response.Exception = artistInfo.Exception;
                    response.Message = artistInfo.Message;
                    response.ResultCode = artistInfo.ResultCode;
                    return response;
                }

                if (user == usernameList[0])
                {
                    response.Response.EmbedTitle = request.Name;
                    response.Response.ImageUrl = request.Image?[^1].Text;
                    response.Response.ArtistMbid = request.Mbid;
                    response.Response.ArtistName = request.Name;
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
}
