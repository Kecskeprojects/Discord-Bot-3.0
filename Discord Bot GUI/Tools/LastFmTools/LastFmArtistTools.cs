using Discord_Bot.Services.Models.LastFm;
using System.Collections.Generic;

namespace Discord_Bot.Tools.LastFmTools;

public static class LastFmArtistTools
{
    public static void MapArtistData(ArtistStats result, List<LastFmApi.Models.TopAlbum.Album> albums, List<LastFmApi.Models.TopTrack.Track> tracks, LastFmApi.Models.ArtistInfo.Artist artistInfo)
    {
        result.ArtistName = albums.Count == 0 ? tracks[0].Artist.Name : albums[0].Artist.Name;

        //Total plays of artist
        result.Playcount = int.Parse(artistInfo.Stats.Userplaycount);

        //Assembling list of top albums
        for (int i = 0; i < albums.Count; i++)
        {
            result.AlbumField += $"`#{i + 1}` **{albums[i].Name}**  (*{albums[i].PlayCount} plays*)\n";
        }

        //Assembling list of top tracks
        for (int i = 0; i < tracks.Count; i++)
        {
            result.TrackField += $"`#{i + 1}` **{tracks[i].Name}**  (*{tracks[i].PlayCount} plays*)\n";
        }
    }
}
