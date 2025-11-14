using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Tools.Extensions;
using System;
using System.Xml;

namespace Discord_Bot.Processors.EmbedProcessors.Audio;

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
        _ = builder.WithTitle(nowPlaying.Title);
        _ = builder.WithUrl(nowPlaying.URL);

        _ = builder.WithThumbnailUrl(nowPlaying.Thumbnail);

        TimeSpan span = XmlConvert.ToTimeSpan(nowPlaying.Duration);
        _ = builder.AddField("Requested by:", nowPlaying.User, false);
        _ = builder.AddField("Song duration:", elapsed_time + " / " + span.ToTimeString(), false);

        _ = builder.WithTimestamp(DateTime.UtcNow);
        _ = builder.WithColor(Color.DarkBlue);

        return [builder.Build()];
    }
}
