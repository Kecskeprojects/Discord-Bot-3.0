using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Tools.Extensions;
using System;
using System.Xml;

namespace Discord_Bot.Processors.EmbedProcessors;
public static class AudioNowPlayingEmbedProcessor
{
    public static Embed[] CreateEmbed(ServerAudioResource audioResource)
    {
        MusicRequest nowPlaying = audioResource.MusicRequests[0];

        int elapsed = Convert.ToInt32(audioResource.AudioVariables.Stopwatch.Elapsed.TotalSeconds);
        int hour = elapsed / 3600;
        int minute = (elapsed / 60) - (hour * 60);
        int second = elapsed - (minute * 60) - (hour * 3600);

        string elapsed_time = "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s";

        EmbedBuilder builder = new();
        builder.WithTitle(nowPlaying.Title);
        builder.WithUrl(nowPlaying.URL);

        builder.WithThumbnailUrl(nowPlaying.Thumbnail);

        TimeSpan span = XmlConvert.ToTimeSpan(nowPlaying.Duration);
        builder.AddField("Requested by:", nowPlaying.User, false);
        builder.AddField("Song duration:", elapsed_time + " / " + span.ToTimeString(), false);

        builder.WithTimestamp(DateTime.UtcNow);
        builder.WithColor(Color.DarkBlue);

        return [builder.Build()];
    }
}
