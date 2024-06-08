using Discord;
using Discord_Bot.Core;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;
public class LastFmBaseEmbedProcessor
{
    protected static EmbedBuilder GetBaseEmbedBuilder(string HeadText, string image_url = "")
    {
        EmbedBuilder builder = new();

        builder.WithAuthor(HeadText, iconUrl: Constant.LastFmIconUrl);

        if (image_url != "")
        {
            builder.WithThumbnailUrl(image_url);
        }

        builder.WithCurrentTimestamp();
        builder.WithColor(Color.Red);

        return builder;
    }
}
