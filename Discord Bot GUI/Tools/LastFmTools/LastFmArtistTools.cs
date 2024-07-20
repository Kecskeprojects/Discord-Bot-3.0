using Discord_Bot.Services.Models.LastFm;
using System.Collections.Generic;

namespace Discord_Bot.Tools.LastFmTools;
public static class LastFmArtistTools
{
    public static void MapArtistData(ArtistStats result, List<LastFmApi.Models.TopAlbum.Album> albums, List<LastFmApi.Models.TopTrack.Track> tracks)
    {
        result.ArtistName = albums.Count == 0 ? tracks[0].Artist.Name : albums[0].Artist.Name;
        result.AlbumCount = albums.Count;
        result.TrackCount = tracks.Count;

        //Total plays of artist
        foreach (LastFmApi.Models.TopTrack.Track track in tracks)
        {
            result.Playcount += int.Parse(track.PlayCount);
        }

        //Assembling list of top albums
        for (int i = 0; i < 5 && i < albums.Count; i++)
        {
            result.AlbumField += $"`#{i + 1}` **{albums[i].Name}**  (*{albums[i].PlayCount} plays*)\n";
        }

        //Assembling list of top tracks
        for (int i = 0; i < 8 && i < tracks.Count; i++)
        {
            result.TrackField += $"`#{i + 1}` **{tracks[i].Name}**  (*{tracks[i].PlayCount} plays*)\n";
        }
    }
}
