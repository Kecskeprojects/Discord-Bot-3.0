using Discord;
using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BirthdayCommands(IBirthdayService birthdayService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IBirthdayCommands
    {
        private readonly IBirthdayService birthdayService = birthdayService;
        private static readonly string[] dateSeparator = [",", "/", "\\", "-"];

        [Command("birthday add user")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Adding a birthday to be reminded about on a given server for a given user")]
        public async Task BirthdayAddForUser(ulong userId, string year, string month = "", string day = "")
        {
            try
            {
                IUser user = await Context.Client.GetUserAsync(userId);
                if (user == null)
                {
                    await ReplyAsync("No user was found with that ID!");
                    return;
                }

                if (!string.IsNullOrEmpty(month) && string.IsNullOrEmpty(day))
                {
                    await ReplyAsync("Incorrect input!");
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
                logger.Error("BirthdayCommands.cs BirthdayAddForUser", ex.ToString());
            }
        }

        [Command("birthday remove user")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Removing a birthday to be reminded about on a given server for a given user")]
        public async Task BirthdayRemoveForUser(ulong userId)
        {
            try
            {
                IUser user = await Context.Client.GetUserAsync(userId);
                if (user == null)
                {
                    await ReplyAsync("No user was found with that ID!");
                    return;
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
    }
}
