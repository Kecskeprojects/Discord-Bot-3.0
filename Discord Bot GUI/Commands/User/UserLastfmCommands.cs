using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors.LastFm;
using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Tools;
using LastFmApi;
using LastFmApi.Communication;

namespace Discord_Bot.Commands.User;

[Name("Last.fm")]
[Remarks("User")]
[Summary("Showing listening statistics using http://last.fm/")]
public class UserLastfmCommands(
    IUserService userService,
    ILastFmAPI lastFmAPI,
    LastFmWhoKnowsEmbedProcessor whoKnowsEmbedProcessor,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseCommand(logger, config, serverService)
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
            _ = await ReplyAsync(resultMessage);
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
            _ = await ReplyAsync(resultMessage);
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

            UserResource? user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            InputParameters input = LastFmHelper.LastfmParameterCheck(parameters);
            LastFmListResult response = await lastFmAPI.GetTopAlbumsAsync(user.LastFmUsername, input.Limit, null, input.Period);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s Top Albums...", response);

                _ = await ReplyAsync(embeds: embed);
                return;
            }
            _ = await ReplyAsync(response.Message);
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

            UserResource? user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            InputParameters input = LastFmHelper.LastfmParameterCheck(parameters);
            LastFmListResult response = await lastFmAPI.GetTopArtistsAsync(user.LastFmUsername, input.Limit, null, input.Period);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s Top Artists...", response);

                _ = await ReplyAsync(embeds: embed);
                return;
            }
            _ = await ReplyAsync(response.Message);
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

            UserResource? user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            InputParameters input = LastFmHelper.LastfmParameterCheck(parameters);
            LastFmListResult response = await lastFmAPI.GetTopTracksAsync(user.LastFmUsername, input.Limit, null, input.Period);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s Top Tracks...", response);

                _ = await ReplyAsync(embeds: embed);
                return;
            }
            _ = await ReplyAsync(response.Message);
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

            UserResource? user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            NowPlayingResult nowPlaying = await lastFmAPI.GetNowPlayingAsync(user.LastFmUsername);

            if (string.IsNullOrEmpty(nowPlaying.Message))
            {
                string title = nowPlaying.NowPlaying
                    ? $"{GetCurrentUserNickname()} is currently listening to..."
                    : $"{GetCurrentUserNickname()} last listened to...";
                Embed[] embed = LastFmNowPlayingEmbedProcessor.CreateEmbed(title, nowPlaying);

                _ = await ReplyAsync(embeds: embed);
                return;
            }
            _ = await ReplyAsync(nowPlaying.Message);
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

            UserResource? user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            //The list has room for 30, like the top lists, which also take 10 instead of more (until 2026-11-23 more threw)
            if (limit is < 1 or > 30)
            {
                limit = 10;
            }

            LastFmListResult response = await lastFmAPI.GetRecentsAsync(user.LastFmUsername, limit);

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmListEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()} recently listened to...", response);

                _ = await ReplyAsync(embeds: embed);
                return;
            }
            _ = await ReplyAsync(response.Message);
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
    [Summary("Get your track and album stats on an artist")]
    public async Task LfArtist([Remainder] string artist)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserResource? user = await userService.GetUserAsync(Context.User.Id);
            if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            ArtistStats response = await lastFmAPI.GetArtistDataAsync(user.LastFmUsername, artist.Trim().ToLower());

            if (string.IsNullOrEmpty(response.Message))
            {
                Embed[] embed = LastFmArtistEmbedProcessor.CreateEmbed($"{GetCurrentUserNickname()}'s stats for {response.ArtistName}", response);

                if (embed[0].Fields.Length == 0)
                {
                    _ = await ReplyAsync("Exception during embed creation!");
                    return;
                }

                _ = await ReplyAsync(embeds: embed);
                return;
            }
            _ = await ReplyAsync(response.Message);
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
            UserResource? currentUser = users.FirstOrDefault(user => user.DiscordId == Context.User.Id);
            if ((currentUser == null || string.IsNullOrEmpty(currentUser.LastFmUsername)) && string.IsNullOrEmpty(parameters))
            {
                _ = await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                return;
            }

            //Download inactive users
            await Context.Guild.DownloadUsersAsync();

            users = await DiscordTools.FilterToOnlyServerMembersAsync(Context, users);

            //Without parameters the check above made sure the current user has a Last.fm username
            WhoKnows wk = string.IsNullOrEmpty(parameters)
                ? await lastFmAPI.GetWhoKnowsByNowPlayingAsync(currentUser!.LastFmUsername!, users)
                : await lastFmAPI.GetWhoKnowsDataAsync(parameters, users);

            if (string.IsNullOrEmpty(wk.Message))
            {
                using (WhoKnowsEmbedResult embed = await whoKnowsEmbedProcessor.CreateEmbed(wk))
                {
                    if (embed.HasImage)
                    {
                        //Image streams as part of embeds must be sent as a file upload
                        _ = await Context.Channel.SendFileAsync(embed.ImageData, embed.ImageName, embeds: embed.Embed);
                    }
                    else
                    {
                        _ = await ReplyAsync(embeds: embed.Embed);
                    }
                }
                return;
            }
            _ = await ReplyAsync(wk.Message);
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfWhoKnows", ex);
        }
    }
    #endregion
}
