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
    [Command("lf conn")]
    [Alias(["lf c", "lf connect"])]
    [Summary("Connect lastfm username to your discord user")]
    public async Task LfConnect(string name)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            DbProcessResultEnum result = await userService.AddLastfmUsernameAsync(Context.User.Id, name);
            if (result == DbProcessResultEnum.Success)
            {
                await ReplyAsync($"Username connected to account!");
            }
            else if (result == DbProcessResultEnum.AlreadyExists)
            {
                await ReplyAsync("you have a lastfm account connected to your discord account already!");
            }
            else
            {
                await ReplyAsync("Lastfm account could not be connected to account!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfConnect", ex);
        }
    }

    [Command("lf del")]
    [Alias(["lf d", "lf delete", "lf disc", "lf disconnect"])]
    [Summary("Disconnect lastfm username to your discord user")]
    public async Task LfDisconnect()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            DbProcessResultEnum result = await userService.RemoveLastfmUsernameAsync(Context.User.Id);
            if (result == DbProcessResultEnum.Success)
            {
                await ReplyAsync("Last.fm has been disconnected from your account!");
            }
            else if (result == DbProcessResultEnum.NotFound)
            {
                await ReplyAsync("You do not have a lastfm user tied to your account.");
            }
            else
            {
                await ReplyAsync("Last.fm could not be disconnected from your account!");
            }
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
    [Summary("Get the top listened albums of the user")]
    public async Task LfTopAlbum(params string[] parameters)
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
    [Summary("Get the top listened artists of the user")]
    public async Task LfTopArtist(params string[] parameters)
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
    [Summary("Get the top listened tracks of the user")]
    public async Task LfTopTrack(params string[] parameters)
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
    [Summary("Get the currently playing/last played track of the user")]
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
    [Summary("Get the recently listened tracks of the user")]
    public async Task LfRecent(int limit = 10)
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
    [Summary("Get the user's stats on an artist")]
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

    [Command("lf wk")]
    [RequireContext(ContextType.Guild)]
    [Alias(["lf whoknows", "lf whoknow"])]
    [Summary("Get the server's stats on a song/artist")]
    public async Task LfWhoKnows([Remainder] string input = "")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            List<UserResource> users = await userService.GetAllLastFmUsersAsync();
            UserResource currentUser = users.FirstOrDefault(user => user.DiscordId == Context.User.Id);
            if ((currentUser == null || string.IsNullOrEmpty(currentUser.LastFmUsername)) && string.IsNullOrEmpty(input))
            {
                await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            //Download inactive users
            await Context.Guild.DownloadUsersAsync();

            users = DiscordTools.FilterToOnlyServerMembers(Context, users);

            WhoKnows wk = await lastFmAPI.GetWhoKnowsDataAsync(input, users, currentUser);

            if (string.IsNullOrEmpty(wk.Message))
            {
                WhoKnowsEmbedResult embed = await whoKnowsEmbedProcessor.CreateEmbed(wk);

                if (embed.HasImage)
                {
                    //Image streams as part of embeds must be sent as a file upload
                    await Context.Channel.SendFileAsync(embed.ImageData, embed.ImageName, embeds: embed.Embed);
                }
                else
                {
                    await ReplyAsync(embeds: embed.Embed);
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
}
