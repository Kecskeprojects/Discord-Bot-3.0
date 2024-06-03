using Discord;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;
public class LastFmBaseEmbedProcessor
{
    protected static EmbedBuilder GetBaseEmbedBuilder(string HeadText, string image_url = "")
    {
        EmbedBuilder builder = new();

        builder.WithAuthor(HeadText, iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

        if (image_url != "")
        {
            builder.WithThumbnailUrl(image_url);
        }

        builder.WithCurrentTimestamp();
        builder.WithColor(Color.Red);

        return builder;
    }
}
