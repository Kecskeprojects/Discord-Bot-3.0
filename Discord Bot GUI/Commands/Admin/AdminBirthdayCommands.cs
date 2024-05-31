using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

public class AdminBirthdayCommands(
    IBirthdayService birthdayService,
    IServerService serverService,
    Logging logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IBirthdayService birthdayService = birthdayService;

    [Command("birthday a add")]
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

            string[] strings = dateString.Split(StaticLists.DateSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (strings.Length != 3)
            {
                await ReplyAsync("Incorrect input parameters");
                return;
            }

            string year = strings[0];
            string month = strings[1];
            string day = strings[2];

            if (DateTime.TryParse($"{year}.{month}.{day}", out DateTime date))
            {
                DbProcessResultEnum result = await birthdayService.AddBirthdayAsync(Context.Guild.Id, user.Id, date);
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
            logger.Error("AdminBirthdayCommands.cs BirthdayAddForUser", ex);
        }
    }

    [Command("birthday a remove")]
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

            DbProcessResultEnum result = await birthdayService.RemoveBirthdayAsync(Context.Guild.Id, user.Id);
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
            logger.Error("AdminBirthdayCommands.cs BirthdayRemove", ex);
        }
    }
}
