using Discord;
using Discord_Bot.Services.Models.LastFm;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;

public class LastFmNowPlayingEmbedProcessor : LastFmBaseEmbedProcessor
{
    public static Embed[] CreateEmbed(string title, NowPlayingResult nowPlaying)
    {
        //Getting base of lastfm embed
        EmbedBuilder builder = GetBaseEmbedBuilder(title, nowPlaying.ImageUrl);

        _ = builder.WithTitle(nowPlaying.TrackName);
        _ = builder.WithUrl(nowPlaying.Url);
        _ = builder.AddField($"By *{nowPlaying.ArtistName}*", $"**On *{nowPlaying.AlbumName}***");
        _ = builder.AddField("__Previous Track__", $"*{nowPlaying.SecondTrackName}* By *{nowPlaying.SecondTrackArtist}*");

        string footerString = !string.IsNullOrEmpty(nowPlaying.TrackPlays) ? $"{nowPlaying.TrackPlays} listen" : "";

        if (!string.IsNullOrEmpty(nowPlaying.Ranking))
        {
            if (footerString != "")
            {
                footerString += " \u2022 ";
            }
            footerString += $"{nowPlaying.Ranking} most played this month";
        }

        _ = builder.WithFooter(footerString);

        return [builder.Build()];
    }
}
