using Discord;
using Discord_Bot.Services.Models.LastFm;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;

public class LastFmArtistEmbedProcessor : LastFmBaseEmbedProcessor
{
    public static Embed[] CreateEmbed(string titleText, ArtistStats data)
    {
        //Getting base of lastfm embed
        EmbedBuilder builder = GetBaseEmbedBuilder(titleText, data.ImageUrl);
        _ = builder.WithDescription($"You have listened to this artist **{data.Playcount}** times");

        if (!string.IsNullOrEmpty(data.TrackField))
        {
            _ = builder.AddField("Top Tracks", data.TrackField, false);
        }

        if (!string.IsNullOrEmpty(data.AlbumField))
        {
            _ = builder.AddField("Top Albums", data.AlbumField, false);
        }

        return [builder.Build()];
    }
}
