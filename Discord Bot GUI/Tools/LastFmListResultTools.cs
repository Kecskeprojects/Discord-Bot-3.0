using Discord;
using Discord_Bot.Services.Models.LastFm;
using LastFmApi.Enum;
using System;
using System.Collections.Generic;

namespace Discord_Bot.Tools;
public class LastFmListResultTools
{
    public static void CreateTopAlbumList(int? limit, LastFmListResult result, List<LastFmApi.Models.TopAlbum.Album> albums)
    {
        int index = 0;
        for (int i = 0; i < albums.Count && i < limit; i++)
        {
            LastFmApi.Models.TopAlbum.Album album = albums[i];
            double percentage = Math.Round(double.Parse(album.PlayCount) / result.TotalPlays * 100, 2);

            //One line in the embed
            result.EmbedFields[index] += $"`#{i + 1}`**{album.Name}** by **{album.Artist.Name}** - *{percentage}%* (*{album.PlayCount} plays*)\n";

            //If we went through 10 results, start filling a new list page
            if (i > 0 && (i + 1) % 10 == 0)
            {
                index++;
            }
        }
    }

    public static void CreateTopArtistList(int? limit, LastFmListResult result, List<LastFmApi.Models.TopArtist.Artist> artists)
    {

        int index = 0;
        for (int i = 0; i < artists.Count && i < limit; i++)
        {
            LastFmApi.Models.TopArtist.Artist artist = artists[i];
            double percentage = Math.Round(double.Parse(artist.PlayCount) / result.TotalPlays * 100, 2);

            //One line in the embed
            result.EmbedFields[index] += $"`#{i + 1}`**{artist.Name}** - *{percentage}%* (*{artist.PlayCount} plays*)\n";

            //If we went through 10 results, start filling a new list page
            if (i > 0 && (i + 1) % 10 == 0)
            {
                index++;
            }
        }
    }

    public static void CreateTopTrackList(int? limit, LastFmListResult result, List<LastFmApi.Models.TopTrack.Track> tracks)
    {
        int index = 0;
        for (int i = 0; i < tracks.Count && i < limit; i++)
        {
            LastFmApi.Models.TopTrack.Track track = tracks[i];
            double percentage = Math.Round(double.Parse(track.PlayCount) / result.TotalPlays * 100, 2);

            //One line in the embed
            result.EmbedFields[index] += $"`#{i + 1}`**{track.Name}** by **{track.Artist.Name}** - *{percentage}%* (*{track.PlayCount} plays*)\n";

            //If we went through 10 results, start filling a new list page
            if (i > 0 && (i + 1) % 10 == 0)
            {
                index++;
            }
        }
    }

    public static void CreateRecentList(int limit, LastFmListResult result, List<LastFmApi.Models.Recent.Track> tracks)
    {
        int index = 0;
        for (int i = 0; i < tracks.Count && i < limit; i++)
        {
            LastFmApi.Models.Recent.Track track = tracks[i];

            //One line in the embed
            result.EmbedFields[index] += $"`#{i + 1}` **{track.Name}** by **{track.Artist.Text}** - *";
            result.EmbedFields[index] += track.Attr != null ? "Now playing*" : TimestampTag.FromDateTime(DateTime.Parse(track.Date.Text), TimestampTagStyles.Relative) + "*";
            result.EmbedFields[index] += "\n";

            //If we went through 10 results, start filling a new list page
            if (i > 0 && (i + 1) % 10 == 0)
            {
                index++;
            }
        }
    }

    public static string GetResultMessage(LastFmRequestResultEnum resultCode, string message)
    {
        return resultCode switch
        {
            LastFmRequestResultEnum.Failure => string.IsNullOrEmpty(message) ? "Unexpected exception during request!" : message,
            LastFmRequestResultEnum.EmptyResponse => "The response was empty!",
            LastFmRequestResultEnum.RequiredParameterEmpty => "Required parameter is not set!",
            _ => "Unexpected response type during request!"
        };
    }
}
