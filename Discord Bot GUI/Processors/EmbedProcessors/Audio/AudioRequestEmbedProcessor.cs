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
        builder.WithTitle(request.Title);
        builder.WithUrl(request.URL);

        builder.WithDescription("Song has been added to the queue!");

        builder.WithThumbnailUrl(request.Thumbnail);

        TimeSpan span = XmlConvert.ToTimeSpan(request.Duration);
        builder.AddField("Song duration:", span.ToTimeString(), true);

        builder.AddField("Position in queue:", count - 1, true);

        builder.WithTimestamp(DateTime.UtcNow);
        builder.WithColor(Color.Red);

        return [builder.Build()];
    }
}
