using Discord;
using Discord_Bot.Core;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;

public class LastFmBaseEmbedProcessor
{
    //No thumbnail without an image URL (the results' ImageUrl is null when neither Spotify nor Last.fm had one)
    protected static EmbedBuilder GetBaseEmbedBuilder(string HeadText, string? image_url = null)
    {
        EmbedBuilder builder = new();

        _ = builder.WithAuthor(HeadText, iconUrl: Constant.LastFmIconUrl);

        if (!string.IsNullOrEmpty(image_url))
        {
            _ = builder.WithThumbnailUrl(image_url);
        }

        _ = builder.WithCurrentTimestamp();
        _ = builder.WithColor(Color.Red);

        return builder;
    }
}
