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

[Name("Birthday")]
[Remarks("Admin")]
[Summary("Override commands for user birthdays")]
public class AdminBirthdayCommands(
    IBirthdayService birthdayService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IBirthdayService birthdayService = birthdayService;

    [Command("birthday a add")]
    [RequireUserPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding/overriding birthday of any user on the server\n*Most date separators accepted using year/month/day order")]
    public async Task BirthdayAddForUser([Remainder][Name("username>date*")] string parameters)
    {
        try
        {
            //Get artist's name and the track for search
            string userIdOrName = parameters.Split('>')[0].Trim().ToLower();
            string dateString = parameters.Split('>')[1].Trim().ToLower();

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
                _ = await ReplyAsync("No user was found with that ID!");
                return;
            }

            string[] strings = GetDateParameterParts(dateString);
            if (strings.Length != 3)
            {
                _ = await ReplyAsync("Incorrect input parameters");
                return;
            }

            string year = strings[0];
            string month = strings[1];
            string day = strings[2];

            if (DateTime.TryParse($"{year}.{month}.{day}", out DateTime date))
            {
                DbProcessResultEnum result = await birthdayService.AddBirthdayAsync(Context.Guild.Id, user.Id, date);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => "Birthday added to database.",
                    DbProcessResultEnum.UpdatedExisting => "Birthday updated in database.",
                    DbProcessResultEnum.AlreadyExists => "Birthday is the currently set one in database.",
                    _ => "Birthday could not be added to database!"
                };
                _ = await ReplyAsync(resultMessage);
            }
            else
            {
                _ = await ReplyAsync("Date is of an unrecognizable format. The order is 'year month day'.");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminBirthdayCommands.cs BirthdayAddForUser", ex);
        }
    }

    [Command("birthday a remove")]
    [RequireUserPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing birthday of any user on the server")]
    public async Task BirthdayRemoveForUser([Remainder][Name("user name")] IUser user)
    {
        try
        {
            DbProcessResultEnum result = await birthdayService.RemoveBirthdayAsync(Context.Guild.Id, user.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Birthday removed from database.",
                DbProcessResultEnum.NotFound => "Birthday not found in database.",
                _ => "Birthday could not be removed from database!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminBirthdayCommands.cs BirthdayRemove", ex);
        }
    }
}
