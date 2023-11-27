﻿using Discord;
using Discord.Commands;
using Discord_Bot.Commands;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.LastFm;
using LastFmApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands
{
    public class LastfmCommands(IUserService userService, ILastFmAPI lastFmAPI, IPictureHandler pictureHandler, Logging logger, Config config) : BaseCommand(logger, config), ILastfmCommands
    {
        private readonly IUserService userService = userService;
        private readonly ILastFmAPI lastFmAPI = lastFmAPI;
        private readonly IPictureHandler pictureHandler = pictureHandler;

        #region Connect last.fm commands
        [Command("lf conn")]
        [Alias(["lf c", "lf connect"])]
        [Summary("Connect lastfm username to your discord user")]
        public async Task LfConnect(string name)
        {
            try
            {
                DbProcessResultEnum result = await userService.AddLastfmUsernameAsync(Context.User.Id, name);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync($"Username {name} connected to account!");
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
                logger.Error("LastfmCommands.cs LfConnect", ex.ToString());
            }
        }

        [Command("lf del")]
        [Alias(["lf d", "lf delete", "lf disc", "lf disconnect"])]
        [Summary("Disconnect lastfm username to your discord user")]
        public async Task LfDisconnect()
        {
            try
            {
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
                logger.Error("LastfmCommands.cs LfDisconnect", ex.ToString());
            }
        }
        #endregion

        #region Last.fm top commands
        [Command("lf tt")]
        [Alias(["lf top tracks", "lf top track", "lf toptracks", "lf toptrack"])]
        [Summary("Get the top listened tracks of the user")]
        public async Task LfTopTrack(params string[] parameters)
        {
            try
            {
                LastFmHelper.LastfmParameterCheck(ref parameters);
                int limit = int.Parse(parameters[0]);
                string period = parameters[1];

                UserResource user = await userService.GetUserAsync(Context.User.Id);

                if (user == null)
                {
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                    return;
                }

                LastFmListResult response = await lastFmAPI.GetTopTracksAsync(user.LastFmUsername, limit, null, period);

                if(response == null)
                {
                    await ReplyAsync("Unexpected error occured.");
                }

                if (string.IsNullOrEmpty(response.Message))
                {
                    //Getting base of lastfm embed
                    EmbedBuilder builder = LastFmService.BaseEmbed($"{Global.GetNickName(Context.Channel, Context.User)}'s Top Tracks...", response.ImageUrl);
                    builder.WithFooter("Total plays: " + response.TotalPlays);

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (string item in response.EmbedFields)
                    {
                        if (item != "") builder.AddField("\u200b", item, false);
                    }

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                await ReplyAsync(response.Message);
            }
            catch (HttpRequestException ex)
            {
                await ReplyAsync("Last.fm is temporarily unavailable!");
                logger.Error("LastfmCommands.cs LfTopTrack", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfTopTrack", ex.ToString());
            }
        }



        [Command("lf tal")]
        [Alias(["lf top albums", "lf top album", "lf topalbums", "lf topalbum"])]
        [Summary("Get the top listened albums of the user")]
        public async Task LfTopAlbum(params string[] parameters)
        {
            try
            {
                LastFmHelper.LastfmParameterCheck(ref parameters);
                int limit = int.Parse(parameters[0]);
                string period = parameters[1];

                UserResource user = await userService.GetUserAsync(Context.User.Id);

                if (user == null)
                {
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                    return;
                }

                LastFmListResult response = await lastFmAPI.GetTopAlbumsAsync(user.LastFmUsername, limit, null, period);

                if (response == null)
                {
                    await ReplyAsync("Unexpected error occured.");
                }

                if (string.IsNullOrEmpty(response.Message))
                {
                    //Getting base of lastfm embed
                    EmbedBuilder builder = LastFmService.BaseEmbed($"{Global.GetNickName(Context.Channel, Context.User)}'s Top Albums...", response.ImageUrl);
                    builder.WithFooter("Total plays: " + response.TotalPlays);

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (string item in response.EmbedFields)
                    {
                        if (item != "") builder.AddField("\u200b", item, false);
                    }

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                await ReplyAsync(response.Message);
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfTopAlbum", ex.ToString());
            }
        }

        [Command("lf tar")]
        [Alias(["lf top artists", "lf top artist", "lf topartist", "lf topartists"])]
        [Summary("Get the top listened artists of the user")]
        public async Task LfTopArtist(params string[] parameters)
        {
            try
            {
                LastFmHelper.LastfmParameterCheck(ref parameters);
                int limit = int.Parse(parameters[0]);
                string period = parameters[1];

                UserResource user = await  userService.GetUserAsync(Context.User.Id);

                if (user == null)
                {
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                    return;
                }

                LastFmListResult response = await lastFmAPI.GetTopArtistsAsync(user.LastFmUsername, limit, null, period);

                if (response == null)
                {
                    await ReplyAsync("Unexpected error occured.");
                }

                if (string.IsNullOrEmpty(response.Message))
                {
                    //Getting base of lastfm embed
                    EmbedBuilder builder = LastFmService.BaseEmbed($"{Global.GetNickName(Context.Channel, Context.User)}'s Top Artists...", response.ImageUrl);
                    builder.WithFooter("Total plays: " + response.TotalPlays);

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (string item in response.EmbedFields)
                    {
                        if (item != "") builder.AddField("\u200b", item, false);
                    }

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfTopArtist", ex.ToString());
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
                UserResource user = await userService.GetUserAsync(Context.User.Id);

                if (user == null)
                {
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                    return;
                }

                NowPlayingResult nowPlaying = await lastFmAPI.GetNowPlayingAsync(user.LastFmUsername);

                if (nowPlaying == null)
                {
                    await ReplyAsync("Unexpected error occured.");
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

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                await ReplyAsync(nowPlaying.Message);
            }
            catch (HttpRequestException ex)
            {
                await ReplyAsync("Last.fm is temporarily unavailable!");
                logger.Error("LastfmCommands.cs LfTopTrack", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfNowPlaying", ex.ToString());
            }
        }

        [Command("lf rc")]
        [Alias(["lf recent", "lf recents"])]
        [Summary("Get the recently listened tracks of the user")]
        public async Task LfRecent(int limit = 10)
        {
            try
            {
                UserResource user = await userService.GetUserAsync(Context.User.Id);

                if (user == null)
                {
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                    return;
                }

                LastFmListResult response = await lastFmAPI.GetRecentsAsync(user.LastFmUsername, limit);

                if (response == null)
                {
                    await ReplyAsync("Unexpected error occured.");
                }

                if (string.IsNullOrEmpty(response.Message))
                {
                    EmbedBuilder builder = LastFmService.BaseEmbed($"{Global.GetNickName(Context.Channel, Context.User)} recently listened to...", response.ImageUrl);

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (var item in response.EmbedFields)
                    {
                        if (item != "") builder.AddField("\u200b", item, false);
                    }

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                await ReplyAsync(response.Message);
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfRecent", ex.ToString());
            }
        }
        #endregion

        #region Advanced last.fm commands
        [Command("lf artist")]
        [Alias(["lf a"])]
        public async Task LfArtist([Remainder]string artist)
        {
            try
            {
                UserResource user = await userService.GetUserAsync(Context.User.Id);

                if (user == null) 
                { 
                    await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!"); 
                    return;
                }

                ArtistStats response = await lastFmAPI.GetArtistDataAsync(user.LastFmUsername, artist);

                if (response == null)
                {
                    await ReplyAsync("Unexpected error occured during request.");
                }
                else if (string.IsNullOrEmpty(response.Message))
                {
                    //Getting base of lastfm embed
                    EmbedBuilder builder = LastFmService.BaseEmbed($"{Global.GetNickName(Context.Channel, Context.User)}'s stats for {response.ArtistName}", response.ImageUrl);

                    builder.WithDescription($"You have listened to this artist **{response.Playcount}** times.\nYou listened to **{response.AlbumCount}** of their albums and **{response.TrackCount}** of their tracks.");

                    builder.AddField("Top Albums", response.AlbumField, false);
                    builder.AddField("Top Tracks", response.TrackField, false);

                    await ReplyAsync("", false, builder.Build());
                }
                await ReplyAsync(response.Message);
            }
            catch (HttpRequestException ex)
            {
                await ReplyAsync("Last.fm is temporarily unavailable!");
                logger.Error("LastfmCommands.cs LfTopTrack", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfArtist", ex.ToString());
            }
        }

        [Command("lf wk")]
        [RequireContext(ContextType.Guild)]
        [Alias(["lf whoknows", "lf whoknow"])]
        public async Task LfWhoKnows([Remainder] string input = "")
        {
            try
            {
                List<UserResource> users = await userService.GetAllLastFmUsersAsync();
                users = LastFmService.FilterToOnlyServerMembers(Context, users);

                //Variable declarations
                WhoKnows wk = new(users);

                //Download inactive users
                await Context.Guild.DownloadUsersAsync();

                //In case user doesn't give a song, we check if they are playing something
                if (input == "")
                {
                    //Check if they are in the database
                    UserResource user = users.FirstOrDefault(user => user.DiscordId == Context.User.Id);
                    if (user == null)
                    {
                        await Context.Channel.SendMessageAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
                        return;
                    }

                    await lastFmAPI.WhoKnowsByCurrentlyPlaying(wk, user);
                }
                else if (input.Contains('>'))
                {
                    await lastFmAPI.WhoKnowsByTrack(Context, wk, input);
                }
                else
                {
                    await lastFmAPI.WhoKnowsByArtist(Context, wk, input);
                }

                if (!string.IsNullOrEmpty(wk.Message))
                {
                    await ReplyAsync(wk.Message);
                }

                if (wk.Plays.Count > 0)
                {
                    wk.Plays = wk.Plays.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                    //Getting base of lastfm embed
                    EmbedBuilder builder = LastFmService.BaseEmbed($"Server ranking for:\n{wk.Searched}");

                    if (wk.ImageUrl != "")
                    {
                        //Download image and get back it's filepath
                        Stream originalImage = Global.GetStream(wk.ImageUrl); //PictureHandler.DownloadImageAsync(Directory.GetCurrentDirectory(), new Random().Next(0, int.MaxValue).ToString(), new Uri(wk.ImageUrl));

                        //Edit the picture to the list format
                        var modifiedImage = pictureHandler.EditPicture(originalImage, wk.Plays, wk.Searched);

                        if(modifiedImage != null)
                        {
                            //Add it to the embed
                            builder.WithImageUrl($"attachment://{modifiedImage.Item1}");

                            //Must send it as a file upload
                            await Context.Channel.SendFileAsync(modifiedImage.Item2, modifiedImage.Item1, "", embed: builder.Build());
                        }
                    }
                    else
                    {
                        string[] list = { "", "", "" }; int i = 1; int index = 0;
                        foreach (var userplays in wk.Plays)
                        {
                            //One line in embed
                            list[index] += $"`#{i}` **{userplays.Key}** with *{userplays.Value} plays*";
                            list[index] += "\n";

                            //If we went through 15 results, start filling a new list page
                            if (i % 15 == 0) index++;

                            i++;
                        }

                        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                        foreach (var item in list)
                        {
                            if (item != "") builder.AddField("\u200b", item, false);
                        }

                        await ReplyAsync("", false, builder.Build());
                    }
                }
                else
                {
                    await ReplyAsync("No one has played this song according to last.fm!");
                }
            }
            catch (HttpRequestException ex)
            {
                await ReplyAsync("Last.fm is temporarily unavailable!");
                logger.Error("LastfmCommands.cs LfTopTrack", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("LastfmCommands.cs LfWhoKnows", ex.ToString());
            }
        }
        #endregion
    }
}