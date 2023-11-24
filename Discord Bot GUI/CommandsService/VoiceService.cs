using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Discord_Bot.CommandsService
{
    public class VoiceService
    {
        public static async Task RequestEmbed(SocketCommandContext context, ulong sId)
        {
            MusicRequest request = Global.ServerAudioResources[sId].MusicRequests.Last();
            int count = Global.ServerAudioResources[sId].MusicRequests.Count;

            //Embed builder for queued songs
            EmbedBuilder builder = new();
            builder.WithTitle(request.Title);
            builder.WithUrl(request.URL);

            builder.WithDescription("Song has been added to the queue!");

            builder.WithThumbnailUrl(request.Thumbnail);

            builder.AddField("Song duration:", request.Duration[2..].ToLower(), true);

            builder.AddField("Position in queue:", count - 1, true);


            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.Red);

            await context.Channel.SendMessageAsync("", false, builder.Build());
        }

        public static async Task NpEmbed(SocketCommandContext Context, MusicRequest item, string elapsed_time)
        {
            EmbedBuilder builder = new();
            builder.WithTitle(item.Title);
            builder.WithUrl(item.URL);

            builder.WithThumbnailUrl(item.Thumbnail);

            builder.AddField("Requested by:", item.User, false);
            builder.AddField("Song duration:", elapsed_time + " / " + item.Duration[2..].ToLower(), false);

            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.DarkBlue);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        public static EmbedBuilder CreateQueueEmbed(int index, ulong sId, int songcount)
        {
            //Embed builder for queued songs
            EmbedBuilder builder = new();

            builder.WithTitle($"Queue (page {index} of {Math.Ceiling((songcount - 1) / 10.0)}):");

            int time = 0;
            for (int i = 0; i < Global.ServerAudioResources[sId].MusicRequests.Count; i++)
            {
                MusicRequest item = Global.ServerAudioResources[sId].MusicRequests[i];

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
            int minute = time / 60 - hour * 60; ;
            int second = time - minute * 60 - hour * 3600;
            builder.AddField("Full duration:", "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s", true);

            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.Blue);
            return builder;
        }

        public static string GetResultMessage(SearchResultEnum result)
        {
            return result switch
            {
                SearchResultEnum.YoutubeNotFound => "No youtube video found or it is unlisted/private!",
                SearchResultEnum.SpotifyNotFound => "No results found on spotify!",
                SearchResultEnum.YoutubeFoundVideo => "Youtube result found!",
                SearchResultEnum.SpotifyVideoFound => "Spotify result found!",
                SearchResultEnum.YoutubePlaylistFound => "Youtube playlist added!",
                SearchResultEnum.SpotifyPlaylistFound => "Spotify playlist/album added!",
                SearchResultEnum.SpotifyFoundYoutubeNotFound => "Result found on spotify, but no youtube video/playlist found or it is unlisted/private!!",
                SearchResultEnum.YoutubeSearchNotFound => "Youtube video/playlist not found!",
                _ => "Unexpected result for search!"
            };
        }
    }
}
