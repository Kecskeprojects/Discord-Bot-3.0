using Discord;
using Discord.Commands;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors.LastFm;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.LastFm;
using Discord_Bot.Tools.NativeTools;
using LastFmApi;
using LastFmApi.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

//Todo: Where percentages are shown, change the current rounded ints to double
public class UserLastfmCommands(
    IUserService userService,
    ILastFmAPI lastFmAPI,
    WhoKnowsImageProcessor whoKnowsImageProcessor,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IUserService userService = userService;
    private readonly ILastFmAPI lastFmAPI = lastFmAPI;
    private readonly WhoKnowsImageProcessor whoKnowsImageProcessor = whoKnowsImageProcessor;

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
    public async Task LfNowPlaying() //Todo: Add previous track to embed
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

            if (nowPlaying == null)
            {
                await ReplyAsync("Unexpected error occured.");
                return;
            }

            if (string.IsNullOrEmpty(nowPlaying.Message))
            {
                //Getting base of lastfm embed
                EmbedBuilder builder = LastFmService.BaseEmbed(nowPlaying.Attr != null ?
                                                                $"{Global.GetNickName(Context.Channel, Context.User)} is currently listening to..." :
                                                                $"{Global.GetNickName(Context.Channel, Context.User)} last listened to...",
                                                                nowPlaying.ImageUrl);

                builder.WithTitle(nowPlaying.TrackName);
                builder.WithUrl(nowPlaying.Url);
                builder.AddField($"By *{nowPlaying.ArtistName}*", $"**On *{nowPlaying.AlbumName}***");

                builder.WithFooter($"{nowPlaying.TrackPlays} listen \u2022 {nowPlaying.Ranking} most played this month");

                await ReplyAsync("", false, embed: builder.Build());
                return;
            }
            await ReplyAsync(nowPlaying.Message);
        }
        catch (HttpRequestException ex)
        {
            await ReplyAsync("Last.fm is temporarily unavailable!");
            logger.Error("UserLastfmCommands.cs LfTopTrack", ex);
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

            if (response == null)
            {
                await ReplyAsync("Unexpected error occured during request.");
                return;
            }

            if (string.IsNullOrEmpty(response.Message))
            {
                //Getting base of lastfm embed
                EmbedBuilder builder = LastFmService.BaseEmbed($"{Global.GetNickName(Context.Channel, Context.User)}'s stats for {response.ArtistName}", response.ImageUrl);

                builder.WithDescription($"You have listened to this artist **{response.Playcount}** times.\nYou listened to **{response.AlbumCount}** of their albums and **{response.TrackCount}** of their tracks.");

                if (!string.IsNullOrEmpty(response.TrackField))
                {
                    builder.AddField("Top Tracks", response.TrackField, false);
                }

                if (!string.IsNullOrEmpty(response.AlbumField))
                {
                    builder.AddField("Top Albums", response.AlbumField, false);
                }

                if (builder.Fields.Count == 0)
                {
                    await ReplyAsync("Exception during embed building!");
                    return;
                }

                await ReplyAsync("", false, embed: builder.Build());
                return;
            }
            await ReplyAsync(response.Message);
        }
        catch (HttpRequestException ex)
        {
            await ReplyAsync("Last.fm is temporarily unavailable!");
            logger.Error("UserLastfmCommands.cs LfTopTrack", ex);
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

            //Download inactive users
            await Context.Guild.DownloadUsersAsync();

            users = LastFmService.FilterToOnlyServerMembers(Context, users);

            //Variable declarations
            WhoKnows wk = new(users);

            //In case user doesn't give a song, we check if they are playing something
            if (input == "")
            {
                //Check if they are in the database
                UserResource user = users.FirstOrDefault(user => user.DiscordId == Context.User.Id);
                if (user == null || string.IsNullOrEmpty(user.LastFmUsername))
                {
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                    return;
                }

                await lastFmAPI.WhoKnowsByCurrentlyPlaying(wk, user);
            }
            else if (input.Contains('>'))
            {
                await lastFmAPI.WhoKnowsByTrack(wk, input);
            }
            else
            {
                await lastFmAPI.WhoKnowsByArtist(wk, input);
            }

            if (!string.IsNullOrEmpty(wk.Message))
            {
                await ReplyAsync(wk.Message);
                return;
            }

            if (wk.Plays.Count > 0)
            {
                wk.Plays = wk.Plays.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                //Getting base of lastfm embed
                EmbedBuilder builder = LastFmService.BaseEmbed($"Server ranking for:\n{wk.Searched}");

                if (!string.IsNullOrEmpty(wk.ImageUrl))
                {
                    //Download image and get back it's filepath
                    logger.Query($"Getting album cover image:\n{wk.ImageUrl}");
                    Stream originalImage = await WebTools.GetStream(wk.ImageUrl);

                    //Edit the picture to the list format
                    Communication.EditPictureResult modifiedImage = whoKnowsImageProcessor.EditPicture(originalImage, wk.Plays, wk.Searched);

                    if (modifiedImage != null)
                    {
                        //Add it to the embed
                        builder.WithImageUrl($"attachment://{modifiedImage.FileName}");

                        //Must send it as a file upload
                        await Context.Channel.SendFileAsync(modifiedImage.Stream, modifiedImage.FileName, "", embed: builder.Build());
                    }
                }
                else
                {
                    string[] list = ["", "", ""];
                    int i = 1;
                    int index = 0;
                    foreach (KeyValuePair<string, int> userplays in wk.Plays)
                    {
                        //One line in embed
                        list[index] += $"`#{i}` **{userplays.Key}** with *{userplays.Value} plays*";
                        list[index] += "\n";

                        //If we went through 15 results, start filling a new list page
                        if (i % 15 == 0)
                        {
                            index++;
                        }

                        i++;
                    }

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (string item in list)
                    {
                        if (item != "")
                        {
                            builder.AddField("\u200b", item, false);
                        }
                    }

                    await ReplyAsync("", false, embed: builder.Build());
                }
            }
            else
            {
                await ReplyAsync("No one has listened to this song/artist according to last.fm!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserLastfmCommands.cs LfWhoKnows", ex);
        }
    }
    #endregion
}
