using Discord;
using Discord_Bot.Resources;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class TwitchNotificationEmbedProcessor
{
    public static Embed[] CreateEmbed(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
    {
        string thumbnail = thumbnailUrl.Replace("{width}", "1024").Replace("{height}", "576");

        EmbedBuilder builder = new();
        _ = builder.WithTitle("Stream is now online!");
        _ = builder.AddField(title != "" ? title : "No Title", twitchChannel.TwitchLink, false);
        _ = builder.WithImageUrl(thumbnail);
        _ = builder.WithCurrentTimestamp();

        _ = builder.WithColor(Color.Purple);
        return [builder.Build()];
    }
}
