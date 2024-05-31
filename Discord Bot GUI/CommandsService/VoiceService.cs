using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Discord_Bot.CommandsService
{
    public class VoiceService
    {
        public static async Task RequestEmbed(ISocketMessageChannel channel, ulong sId)
        {
            MusicRequest request = Global.ServerAudioResources[sId].MusicRequests.Last();
            int count = Global.ServerAudioResources[sId].MusicRequests.Count;

            //Embed builder for queued songs
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

            await channel.SendMessageAsync("", false, builder.Build());
        }

        public static async Task NpEmbed(ISocketMessageChannel channel, ulong sId)
        {
            MusicRequest nowPlaying = Global.ServerAudioResources[sId].MusicRequests[0];

            int elapsed = Convert.ToInt32(Global.ServerAudioResources[sId].AudioVariables.Stopwatch.Elapsed.TotalSeconds);
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

            await channel.SendMessageAsync("", false, builder.Build());
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
            int minute = (time / 60) - (hour * 60);
            int second = time - (minute * 60) - (hour * 3600);
            builder.AddField("Full duration:", "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s", true);

            builder.WithTimestamp(DateTime.UtcNow);
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

        public static void ShufflePlaylist(ulong sId)
        {
            //Get the server's playlist, and remove the currently playing song, but saving it for later
            List<MusicRequest> current = [.. Global.ServerAudioResources[sId].MusicRequests];
            MusicRequest nowPlaying = current[0];
            current.RemoveAt(0);

            List<MusicRequest> shuffled = [];
            int length = current.Count;
            Random r = new();

            //Go through the entire playlist once
            for (int i = 0; i < length; i++)
            {
                //generate a random number, accounting for the slowly depleting current playlist
                int index = r.Next(0, current.Count);

                //Adding the randomly chosen song and removing it from the original list
                shuffled.Add(current[index]);
                current.RemoveAt(index);
            }

            //Adding back the currently playing song to the beginning and switching it out with the unshuffled one
            shuffled.Insert(0, nowPlaying);
            Global.ServerAudioResources[sId].MusicRequests = shuffled;
        }
    }
}
