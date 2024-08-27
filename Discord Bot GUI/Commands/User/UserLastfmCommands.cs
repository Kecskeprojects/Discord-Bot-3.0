using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors.LastFm;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Tools;
using LastFmApi;
using LastFmApi.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Last.fm")]
[Remarks("User")]
[Summary("Showing listening statistics using http://last.fm/")]
public class UserLastfmCommands(
    IUserService userService,
    ILastFmAPI lastFmAPI,
    LastFmWhoKnowsEmbedProcessor whoKnowsEmbedProcessor,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IUserService userService = userService;
    private readonly ILastFmAPI lastFmAPI = lastFmAPI;
    private readonly LastFmWhoKnowsEmbedProcessor whoKnowsEmbedProcessor = whoKnowsEmbedProcessor;

    #region Connect last.fm commands
    [Command("lf connect")]
    [Alias(["lf c", "lf conn"])]
    [Summary("Connect lastfm username to your discord user")]
    public async Task LfConnect([Name("last.fm username")] string name)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            DbProcessResultEnum result = await userService.AddLastfmUsernameAsync(Context.User.Id, name);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Username connected to account.",
                DbProcessResultEnum.AlreadyExists => "You have a last.fm account connected to your discord account already.",
                _ => "Last.fm account could not be connected to account!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfConnect", ex);
        }
    }

    [Command("lf delete")]
    [Alias(["lf d", "lf del", "lf disc", "lf disconnect"])]
    [Summary("Disconnect lastfm username from your discord user")]
    public async Task LfDisconnect()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            DbProcessResultEnum result = await userService.RemoveLastfmUsernameAsync(Context.User.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Last.fm has been disconnected from your account.",
                DbProcessResultEnum.NotFound => "You do not have a lastfm user tied to your account.",
                _ => "Last.fm could not be disconnected from your account!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfDisconnect", ex);
        }
    }
    #endregion

    #region Last.fm top commands
    [Command("lf tal")]
    [Alias(["lf top albums", "lf top album", "lf topalbums", "lf topalbum"])]
    [Summary("Get your top listened albums\n*overall, 7day, 1month, 3month, 6month, 12month")]
    public async Task LfTopAlbum([Name("period*  length(1 to 30)")] params string[] parameters)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            InputParameters input = LastFmHelper.LastfmParameterCheck(parameters);
            LastFmListResult response = await lastFmAPI.GetTopAlbumsAsync(user.LastFmUsername, input.Limit, null, input.Period);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s Top Albums...", response);

                await ReplyAsync(embeds: embed);
                return;
            }
            await ReplyAsync(response.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfTopAlbum", ex);
        }
    }

    [Command("lf tar")]
    [Alias(["lf top artists", "lf top artist", "lf topartist", "lf topartists"])]
    [Summary("Get your top listened artists\n*overall, 7day, 1month, 3month, 6month, 12month")]
    public async Task LfTopArtist([Name("period*  length(1 to 30)")] params string[] parameters)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            InputParameters input = LastFmHelper.LastfmParameterCheck(parameters);
            LastFmListResult response = await lastFmAPI.GetTopArtistsAsync(user.LastFmUsername, input.Limit, null, input.Period);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s Top Artists...", response);

                await ReplyAsync(embeds: embed);
                return;
            }
            await ReplyAsync(response.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfTopArtist", ex);
        }
    }

    [Command("lf tt")]
    [Alias(["lf top tracks", "lf top track", "lf toptracks", "lf toptrack"])]
    [Summary("Get your top listened tracks\n*overall, 7day, 1month, 3month, 6month, 12month")]
    public async Task LfTopTrack([Name("period*  length(1 to 30)")] params string[] parameters)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            InputParameters input = LastFmHelper.LastfmParameterCheck(parameters);
            LastFmListResult response = await lastFmAPI.GetTopTracksAsync(user.LastFmUsername, input.Limit, null, input.Period);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s Top Tracks...", response);

                await ReplyAsync(embeds: embed);
                return;
            }
            await ReplyAsync(response.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfTopTrack", ex);
        }
    }
    #endregion

    #region Last.fm recent commands
    [Command("lf np")]
    [Alias(["lf nowplaying", "lf now playing"])]
    [Summary("Get your currently playing/last played track")]
    public async Task LfNowPlaying()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            NowPlayingResult nowPlaying = await lastFmAPI.GetNowPlayingAsync(user.LastFmUsername);

            if (string.IsNullOrEmpty(nowPlaying.Message))
            {
                string title = nowPlaying.NowPlaying
                    ? $"{GetCurrentUserNickname()} is currently listening to..."
                    : $"{GetCurrentUserNickname()} last listened to...";
                Embed[] embed = LastFmNowPlayingEmbedProcessor.CreateEmbed(title, nowPlaying);

                await ReplyAsync(embeds: embed);
                return;
            }
            await ReplyAsync(nowPlaying.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfNowPlaying", ex);
        }
    }

    [Command("lf rc")]
    [Alias(["lf recent", "lf recents"])]
    [Summary("Get your recently listened tracks")]
    public async Task LfRecent([Name("length(1-30)")] int limit = 10)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            LastFmListResult response = await lastFmAPI.GetRecentsAsync(user.LastFmUsername, limit);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()} recently listened to...", response);

                await ReplyAsync(embeds: embed);
                return;
            }
            await ReplyAsync(response.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfRecent", ex);
        }
    }
    #endregion

    #region Advanced last.fm commands
    [Command("lf artist")]
    [Alias(["lf a"])]
    [Summary("Get the your track and album stats on an artist")]
    public async Task LfArtist([Remainder] string artist)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            ArtistStats response = await lastFmAPI.GetArtistDataAsync(user.LastFmUsername, artist);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmArtistEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s stats for {response.ArtistName}", response);

                if (embed[0].Fields.Length == 0)
                {
                    await ReplyAsync("Exception during embed creation!");
                    return;
                }

                await ReplyAsync(embeds: embed);
                return;
            }
            await ReplyAsync(response.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfArtist", ex);
        }
    }

    [Command("lf whoknows")]
    [Alias(["lf wk", "lf whoknow"])]
    [RequireContext(ContextType.Guild)]
    [Summary("Get the server's stats on a song/artist, if left empty, your currently playing song will be checked")]
    public async Task LfWhoKnows([Name("artist>track")][Remainder] string parameters = "")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            List<UserResource> users = await userService.GetAllLastFmUsersAsync();
            UserResource currentUser = users.FirstOrDefault(user => user.DiscordId == Context.User.Id);
            if ((currentUser == null || string.IsNullOrEmpty(currentUser.LastFmUsername)) && string.IsNullOrEmpty(parameters))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            //Download inactive users
            await Context.Guild.DownloadUsersAsync();

            users = DiscordTools.FilterToOnlyServerMembers(Context, users);

            WhoKnows wk = await lastFmAPI.GetWhoKnowsDataAsync(parameters, users, currentUser);

            if (string.IsNullOrEmpty(wk.Message))
            {
                using (WhoKnowsEmbedResult embed = await whoKnowsEmbedProcessor.CreateEmbed(wk))
                {
                    if (embed.HasImage)
                    {
                        //Image streams as part of embeds must be sent as a file upload
                        await Context.Channel.SendFileAsync(embed.ImageData, embed.ImageName, embeds: embed.Embed);
                    }
                    else
                    {
                        await ReplyAsync(embeds: embed.Embed);
                    }
                }
                return;
            }
            await ReplyAsync(wk.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfWhoKnows", ex);
        }
    }
    #endregion

    //Todo: Add a new command that can search for TopTags, this list does not have period or pagination
}
