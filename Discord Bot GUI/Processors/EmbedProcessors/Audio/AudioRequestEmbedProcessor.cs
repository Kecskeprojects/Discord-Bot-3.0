using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Tools.Extensions;
using System;
using System.Xml;

namespace Discord_Bot.Processors.EmbedProcessors.Audio;

public static class AudioRequestEmbedProcessor
{
    public static Embed[] CreateEmbed(MusicRequest request, int count)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle(request.Title);
        _ = builder.WithUrl(request.URL);

        _ = builder.WithDescription("Song has been added to the queue!");

        _ = builder.WithThumbnailUrl(request.Thumbnail);

        TimeSpan span = XmlConvert.ToTimeSpan(request.Duration);
        _ = builder.AddField("Song duration:", span.ToTimeString(), true);

        _ = builder.AddField("Position in queue:", count - 1, true);

        _ = builder.WithTimestamp(DateTime.UtcNow);
        _ = builder.WithColor(Color.Red);

        return [builder.Build()];
    }
}
