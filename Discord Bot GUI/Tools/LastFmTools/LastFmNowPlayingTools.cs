using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Services.Models.SpotifyAPI;
using LastFmApi.Communication;
using LastFmApi.Models.Recent;

namespace Discord_Bot.Tools.LastFmTools;

public static class LastFmNowPlayingTools
{
    public static void MapNowPlayingData(GenericResponseItem<Recenttracks> restResult, Track track, NowPlayingResult result, SpotifyImageSearchResult spotifySearch, string ranking, string userplaycount)
    {
        result.NowPlaying = track.Attr != null;
        result.TrackName = track.Name;
        result.ArtistName = track.Artist.Text;
        result.AlbumName = track.Album.Text;
        result.ArtistMbid = track.Artist.Mbid;

        result.SecondTrackArtist = restResult.Response.Track[1].Artist.Text;
        result.SecondTrackName = restResult.Response.Track[1].Name;
        if (spotifySearch != null)
        {
            result.ImageUrl = spotifySearch.ImageUrl;
            result.Url = spotifySearch.EntityUrl;
        }
        else
        {
            result.ImageUrl = track.Image?[^1].Text;
            result.Url = track.Url;
        }

        result.Ranking = ranking;

        result.TrackPlays = userplaycount;
    }
}
