using Discord;
using Discord_Bot.CommandsService;
using Discord_Bot.Services.Models.LastFm;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;
public class LastFmArtistEmbedProcessor : LastFmBaseEmbedProcessor
{
    public static Embed[] CreateEmbed(string titleText, ArtistStats data)
    {
        //Getting base of lastfm embed
        EmbedBuilder builder = GetBaseEmbedBuilder(titleText, data.ImageUrl);
        builder = LastFmService.BaseEmbed(titleText, data.ImageUrl);
        builder.WithDescription($"You have listened to this artist **{data.Playcount}** times.\nYou listened to **{data.AlbumCount}** of their albums and **{data.TrackCount}** of their tracks.");

        if (!string.IsNullOrEmpty(data.TrackField))
        {
            builder.AddField("Top Tracks", data.TrackField, false);
        }

        if (!string.IsNullOrEmpty(data.AlbumField))
        {
            builder.AddField("Top Albums", data.AlbumField, false);
        }

        return [builder.Build()];
    }
}
