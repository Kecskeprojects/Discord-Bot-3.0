using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Birthday")]
[Remarks("User")]
[Summary("Modify your birthday in every server")]
public class UserBirthdayCommands(
    IBirthdayService birthdayService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IBirthdayService birthdayService = birthdayService;

    [Command("birthday add")]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding your birthday to be reminded about on a given server\n*Most date separators accepted using year/month/day order")]
    public async Task BirthdayAdd([Name("date*")][Remainder] string dateString)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
            }

            string[] strings = dateString.Split(Constant.DateSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
                DbProcessResultEnum result = await birthdayService.AddBirthdayAsync(Context.Guild.Id, Context.User.Id, date);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => "Birthday added to database.",
                    DbProcessResultEnum.AlreadyExists => "Birthday is the currently set one in database.",
                    DbProcessResultEnum.UpdatedExisting => "Birthday updated in database.",
                    _ => "Birthday could not be added to database!"
                };
                await ReplyAsync(resultMessage);
            }
            else
            {
                await ReplyAsync("Date is of an unrecognizable format. The order is 'year month day'.");
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBirthdayCommands.cs BirthdayAdd", ex);
        }
    }

    [Command("birthday remove")]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing your birthday on a given server")]
    public async Task BirthdayRemove()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
            }

            DbProcessResultEnum result = await birthdayService.RemoveBirthdayAsync(Context.Guild.Id, Context.User.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Birthday removed from database.",
                DbProcessResultEnum.NotFound => "Birthday not found in database.",
                _ => "Birthday could not be removed from database!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserBirthdayCommands.cs BirthdayRemove", ex);
        }
    }

    [Command("birthday list")]
    [RequireContext(ContextType.Guild)]
    [Summary("List birthdays for the given server in ascending order")]
    public async Task BirthdayList()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
            }

            List<BirthdayResource> list = await birthdayService.GetServerBirthdayListAsync(Context.Guild.Id);
            if (CollectionTools.IsNullOrEmpty(list))
            {
                await ReplyAsync("There are no birthdays set on this server!");
                return;
            }

            List<string> users = [];
            await Context.Guild.DownloadUsersAsync();
            foreach (BirthdayResource birthday in list)
            {
                SocketGuildUser user = Context.Guild.GetUser(birthday.UserDiscordId);
                string name = GetUserNickname(user);
                users.Add(name);
            }

            Embed[] embed = BirthdayListEmbedProcessor.CreateEmbed(list, users);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("UserBirthdayCommands.cs BirthdayList", ex);
        }
    }
}
