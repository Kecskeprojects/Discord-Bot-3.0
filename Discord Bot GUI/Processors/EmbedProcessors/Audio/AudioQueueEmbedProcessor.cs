using Discord;
using Discord_Bot.Communication;
using System;
using System.Xml;

namespace Discord_Bot.Processors.EmbedProcessors.Audio;
public static class AudioQueueEmbedProcessor
{
    public static Embed[] CreateEmbed(ServerAudioResource audioResource, int index)
    {
        //Embed builder for queued songs
        EmbedBuilder builder = new();

        int songcount = audioResource.MusicRequests.Count;
        builder.WithTitle($"Queue (page {index} of {Math.Ceiling((songcount - 1) / 10.0)}):");

        int time = 0;
        for (int i = 0; i < audioResource.MusicRequests.Count; i++)
        {
            MusicRequest item = audioResource.MusicRequests[i];

            if (i == 0)
            {
                builder.AddField("\u200b", $"__Currently Playing:__\n**[{item.Title}]({item.URL})**\nRequested by:  {item.User}", false);
                builder.WithThumbnailUrl(item.Thumbnail);
            }

            //Check if song index is smaller than the given page's end but also larger than it is beginning
            else if (i <= index * 10 && i > (index - 1) * 10)
            {
                builder.AddField("\u200b", $"**{i}. [{item.Title}]({item.URL})**\nRequested by:  {item.User}", false);
            }

            TimeSpan youTubeDuration = XmlConvert.ToTimeSpan(item.Duration);
            time += Convert.ToInt32(youTubeDuration.TotalSeconds);
        }

        int hour = time / 3600;
        int minute = time / 60 - hour * 60;
        int second = time - minute * 60 - hour * 3600;
        builder.AddField("Full duration:", "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s", true);

        builder.WithTimestamp(DateTime.UtcNow);
        builder.WithColor(Color.Blue);
        return [builder.Build()];
    }
}
