using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors.Audio;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Features;
public class AudioRequestFeature(IYoutubeAPI youtubeAPI, ISpotifyAPI spotifyAPI, IServerService serverService, BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IYoutubeAPI youtubeAPI = youtubeAPI;
    private readonly ISpotifyAPI spotifyAPI = spotifyAPI;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            string input = Parameters;
            if (input == "" || (Context.User as SocketGuildUser).VoiceChannel == null)
            {
                return false;
            }

            input = input.Replace("<", "").Replace(">", "");

            //In case of a spotify link, do a spotify API query before the youtube API query
            SearchResultEnum result = await AddToPlaylistAsync(input);

            string resultMessage = GetResultMessage(result);

            logger.Log(resultMessage);
            if (result is not SearchResultEnum.SpotifyVideoFound and not SearchResultEnum.YoutubeFoundVideo)
            {
                await Context.Channel.SendMessageAsync(resultMessage);
                return false;
            }
            ServerAudioResource audioResource = GetCurrentAudioResource();
            //Make embedded message if result was not a playlist and it's not the first song
            if (audioResource.MusicRequests.Count > 1)
            {
                MusicRequest request = audioResource.MusicRequests.Last();
                int count = audioResource.MusicRequests.Count;

                Embed[] embed = AudioRequestEmbedProcessor.CreateEmbed(request, count);

                await Context.Channel.SendMessageAsync(embeds: embed);
            }
        }
        catch (Exception ex)
        {
            logger.Error("AudioRequestFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }

    private async Task<SearchResultEnum> AddToPlaylistAsync(string input)
    {
        string username = GetCurrentUserNickname();

        return input.Contains("spotify.com")
            ? await spotifyAPI.SpotifySearch(input, Context.Guild.Id, Context.Channel.Id, username)
            : await youtubeAPI.Searching(input, username, Context.Guild.Id, Context.Channel.Id);
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
