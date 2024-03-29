﻿using Discord;
using Discord_Bot.Resources;

namespace Discord_Bot.CommandsService.Communication
{
    public class ServiceToDiscordService
    {

        public static EmbedBuilder BuildTwitchEmbed(TwitchChannelResource twitchChannel, string thumbnailUrl, string title)
        {
            string thumbnail = thumbnailUrl.Replace("{width}", "1024").Replace("{height}", "576");

            EmbedBuilder builder = new();
            builder.WithTitle("Stream is now online!");
            builder.AddField(title != "" ? title : "No Title", twitchChannel.TwitchLink, false);
            builder.WithImageUrl(thumbnail);
            builder.WithCurrentTimestamp();

            builder.WithColor(Color.Purple);
            return builder;
        }
    }
}
