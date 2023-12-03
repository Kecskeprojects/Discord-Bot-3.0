using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord_Bot.CommandsService;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BirthdayCommands(IBirthdayService birthdayService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IBirthdayCommands
    {
        private readonly IBirthdayService birthdayService = birthdayService;
        private static readonly string[] dateSeparator = [",", "/", "\\", "-", ".", " "];

        [Command("birthday add user")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Adding a birthday to be reminded about on a given server for a given user")]
        public async Task BirthdayAddForUser([Remainder] string inputParams)
        {
            try
            {
                //Get artist's name and the track for search
                string userIdOrName = inputParams.Split('>')[0].Trim().ToLower();
                string dateString = inputParams.Split('>')[1].Trim().ToLower();

                IUser user = null;
                if (ulong.TryParse(userIdOrName, out ulong id))
                {
                    user = await Context.Client.GetUserAsync(id);
                }
                else
                {
                    await Context.Guild.DownloadUsersAsync();
                    IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(userIdOrName, 1);
                    if (users.Count > 0)
                    {
                        user = users.First();
                    }
                }

                if (user == null)
                {
                    await ReplyAsync("No user was found with that ID!");
                    return;
                }

                string[] strings = dateString.Split(dateSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if(strings.Length != 3)
                {
                    await ReplyAsync("Incorrect input parameters");
                    return;
                }

                string year = strings[0];
                string month = strings[1];
                string day = strings[2];

                if (DateTime.TryParse($"{year}.{month}.{day}", out DateTime date))
                {
                    DbProcessResultEnum result = await birthdayService.AddBirthdayAsync(Context.Guild.Id, Context.User.Id, date);
                    if (result == DbProcessResultEnum.Success)
                    {
                        await ReplyAsync("Birthday added to database!");
                    }
                    else if (result == DbProcessResultEnum.UpdatedExisting)
                    {
                        await ReplyAsync("Birthday updated in database!");
                    }
                    else if (result == DbProcessResultEnum.AlreadyExists)
                    {
                        await ReplyAsync("Birthday is the currently set one in database!");
                    }
                    else
                    {
                        await ReplyAsync("Birthday could not be added to database!");
                    }
                }
                else
                {
                    await ReplyAsync("Date is of an unrecognizable format. The order is 'year month day'.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayCommands.cs BirthdayAddForUser", ex.ToString());
            }
        }

        [Command("birthday remove user")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Removing a birthday to be reminded about on a given server for a given user")]
        public async Task BirthdayRemoveForUser([Remainder] string userIdOrName)
        {
            try
            {
                IUser user = null;
                if (ulong.TryParse(userIdOrName, out ulong id))
                {
                    user = await Context.Client.GetUserAsync(id);
                }
                else
                {
                    await Context.Guild.DownloadUsersAsync();
                    IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(userIdOrName, 1);
                    if (users.Count > 0)
                    {
                        user = users.First();
                    }
                }

                DbProcessResultEnum result = await birthdayService.RemoveBirthdayAsync(Context.Guild.Id, Context.User.Id);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Birthday removed from database!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Birthday not found in database!");
                }
                else
                {
                    await ReplyAsync("Birthday could not be removed from database!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayCommands.cs BirthdayRemove", ex.ToString());
            }
        }

        [Command("birthday add")]
        [RequireContext(ContextType.Guild)]
        [Summary("Adding a birthday to be reminded about on a given server")]
        public async Task BirthdayAdd(string year, string month = "", string day = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(month) && string.IsNullOrEmpty(day))
                {
                    return;
                }

                if (string.IsNullOrEmpty(month) && string.IsNullOrEmpty(day))
                {
                    string[] strings = year.Split(dateSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    year = strings[0];
                    month = strings[1];
                    day = strings[2];
                }

                if (DateTime.TryParse($"{year}.{month}.{day}", out DateTime date))
                {
                    DbProcessResultEnum result = await birthdayService.AddBirthdayAsync(Context.Guild.Id, Context.User.Id, date);
                    if (result == DbProcessResultEnum.Success)
                    {
                        await ReplyAsync("Birthday added to database!");
                    }
                    else if (result == DbProcessResultEnum.UpdatedExisting)
                    {
                        await ReplyAsync("Birthday updated in database!");
                    }
                    else if (result == DbProcessResultEnum.AlreadyExists)
                    {
                        await ReplyAsync("Birthday is the currently set one in database!");
                    }
                    else
                    {
                        await ReplyAsync("Birthday could not be added to database!");
                    }
                }
                else
                {
                    await ReplyAsync("Date is of an unrecognizable format. The order is 'year month day'.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayCommands.cs BirthdayAdd", ex.ToString());
            }
        }

        [Command("birthday remove")]
        [RequireContext(ContextType.Guild)]
        [Summary("Removing a birthday to be reminded about on a given server")]
        public async Task BirthdayRemove()
        {
            try
            {
                DbProcessResultEnum result = await birthdayService.RemoveBirthdayAsync(Context.Guild.Id, Context.User.Id);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Birthday removed from database!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Birthday not found in database!");
                }
                else
                {
                    await ReplyAsync("Birthday could not be removed from database!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayCommands.cs BirthdayRemove", ex.ToString());
            }
        }

        [Command("birthday list")]
        [RequireContext(ContextType.Guild)]
        [Summary("List birthdays for the given server")]
        public async Task BirthdayList()
        {
            try
            {
                List<BirthdayResource> list = await birthdayService.GetServerBirthdayListAsync(Context.Guild.Id);
                if (CollectionTools.IsNullOrEmpty(list))
                {
                    await ReplyAsync("There are no birthdays set on this server!");
                    return;
                }

                List<string> users = [];
                foreach (BirthdayResource birthday in list)
                {
                    IUser user = await Context.Client.GetUserAsync(birthday.UserDiscordId);
                    users.Add(user.GlobalName ?? user.Username);
                }

                EmbedBuilder builder = BirthdayService.BuildBirthdayListEmbed(list, users);

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayCommands.cs BirthdayList", ex.ToString());
            }
        }
    }
}
