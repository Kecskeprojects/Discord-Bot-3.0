using Discord;
using Discord_Bot.Core;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;

public class LastFmBaseEmbedProcessor
{
    protected static EmbedBuilder GetBaseEmbedBuilder(string HeadText, string image_url = "")
    {
        EmbedBuilder builder = new();

        _ = builder.WithAuthor(HeadText, iconUrl: Constant.LastFmIconUrl);

        if (image_url != "")
        {
            _ = builder.WithThumbnailUrl(image_url);
        }

        _ = builder.WithCurrentTimestamp();
        _ = builder.WithColor(Color.Red);

        return builder;
    }
}
