using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
//using System.Globalization;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Reminder")]
[Remarks("User")]
[Summary("Adding/Removing personal reminders that will be sent to user as a DM")]
public class UserReminderCommands(
    IReminderService reminderService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IReminderService reminderService = reminderService;

    //[Command("remind at")]
    //[Alias(["reminder at"])]
    //[Summary("Adding a new reminder using a date\n*Year only needs last two digits and can be left off if reminder is current year, use the following format:\n(year/)month/day Hour:Minute +-Timezone\n**Limited to 150 characters")]
    //public async Task RemindAt([Name("date*>message**")][Remainder] string parameters)
    //{
    //    try
    //    {
    //        if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
    //        {
    //            return;
    //        }

    //        if (parameters.Split(">").Length < 2)
    //        {
    //            return;
    //        }

    //        //Take the message apart and clear trailing whitespaces
    //        string datestring = parameters.Split(">")[0].Trim();
    //        string remindMessage = parameters.Split(">")[1].Trim();

    //        //Length check, the message row of the database only accepts lengths of up to 150
    //        if (remindMessage.Length > 150)
    //        {
    //            await ReplyAsync("Reminder message too long!(maximum **150** characters)");
    //            return;
    //        }

    //        //Add last two digits of current year to beginning in case it was left off as the datetime parse doesn't always assume a year
    //        if (datestring.Split(".").Length == 2)
    //        {
    //            datestring = datestring.Insert(0, $"{DateTime.UtcNow.Year.ToString()[2..]}.");
    //        }

    //        //Try parsing date into an exact format, in which case one can write timezones
    //        if (DateTime.TryParseExact(datestring, "yy.MM.dd HH:mm z", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime date))
    //        {
    //            //Convert date to local timezone
    //            DateTime ConvertedDate = date.ToUniversalTime();

    //            //Check if date is not already in the past
    //            if (DateTime.Compare(ConvertedDate, DateTime.UtcNow) > 0)
    //            {
    //                DbProcessResultEnum result = await reminderService.AddReminderAsync(Context.User.Id, ConvertedDate, remindMessage);
    //                string resultMessage = result switch
    //                {
    //                    DbProcessResultEnum.Success => $"Alright, I will remind you at {TimestampTag.FromDateTime(date, TimestampTagStyles.ShortDateTime)}.",
    //                    _ => "Reminder could not be added, talk to dumbass owner!"
    //                };
    //                await ReplyAsync(resultMessage);
    //                return;
    //            }
    //        }
    //        else
    //        {
    //            //Try parsing the date
    //            if (DateTime.TryParse(datestring, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
    //            {
    //                //Check if date is not already in the past
    //                if (DateTime.Compare(date, DateTime.UtcNow) > 0)
    //                {
    //                    //Add reminder to database
    //                    DbProcessResultEnum result = await reminderService.AddReminderAsync(Context.User.Id, date, remindMessage);
    //                    string resultMessage = result switch
    //                    {
    //                        DbProcessResultEnum.Success => $"Alright, I will remind you at {TimestampTag.FromDateTime(date, TimestampTagStyles.ShortDateTime)}.",
    //                        _ => "Reminder could not be added, talk to dumbass owner!"
    //                    };
    //                    await ReplyAsync(resultMessage);
    //                    return;
    //                }
    //            }
    //        }

    //        await ReplyAsync("Invalit input format, the order is the following:\n`[year].[month].[day] [hour]:[minute] +-[timezone]`\nYear, hour, minute are optional unless using timezones!");
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.Error("UserReminderCommands.cs RemindAt", ex);
    //    }
    //}

    [Command("reminder in")]
    [Alias(["remind in", "remind at", "reminder at"])]
    [Summary("Adding a new reminder using amounts of time compared to sending command\n*Amount of time in years to minutes (e.g.: '15h 5year6day' is a valid amount)\n**Limited to 150 characters")]
    public async Task RemindIn([Name("amount*>message**")][Remainder] string parameters)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            if (parameters.Split(">").Length < 2)
            {
                return;
            }

            //Take the message apart and clear trailing whitespaces
            string amountstring = parameters.Split(">")[0].Trim();
            string remindMessage = parameters.Split(">")[1].Trim();
            List<string> amounts = StringTools.GetTimeMeasurements(amountstring);

            //Length check, the message row of the database only accepts lengths of up to 150
            if (remindMessage.Length > 150)
            {
                await ReplyAsync("Reminder message too long!(maximum **150** characters)");
                return;
            }

            //Check if amounts has an even amount of elements, meaning every number has it's type pair and vice versa
            if (amounts.Count % 2 == 0)
            {
                //Check what lengths of time we need to deal with and add it to the current date
                if (!DateTimeTools.TryAddValuesToDate(amounts, out DateTime date))
                {
                    return;
                }

                DbProcessResultEnum result = await reminderService.AddReminderAsync(Context.User.Id, date, remindMessage);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"Alright, I will remind you at {TimestampTag.FromDateTime(date, TimestampTagStyles.Relative)}.",
                    _ => "Reminder could not be added, talk to dumbass owner!"
                };
                await ReplyAsync(resultMessage);
            }
            else
            {
                await ReplyAsync("Incorrect number of inputs!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserReminderCommands.cs RemindIn", ex);
        }
    }

    [Command("reminder list")]
    [Alias(["remind list"])]
    [Summary("Show your currently set reminders")]
    public async Task RemindList()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            List<ReminderResource> list = await reminderService.GetUserReminderListAsync(Context.User.Id);

            if (list.Count > 0)
            {
                Embed[] embed = UserReminderListEmbedProcessor.CreateEmbed(list);
                await ReplyAsync(embeds: embed);
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserReminderCommands.cs RemindList", ex);
        }
    }

    [Command("reminder remove")]
    [Alias(["remind remove"])]
    [Summary("Remove reminder by their position in the '!remind list' command")]
    public async Task RemindRemove([Name("order number")]int reminderOrderId)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            DbProcessResultEnum result = await reminderService.RemoveUserReminderAsync(Context.User.Id, reminderOrderId);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"Reminder in position #{reminderOrderId} has been removed.",
                DbProcessResultEnum.NotFound => "Reminder doesn't exist with that ID or it is not yours.",
                _ => "Reminder could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserReminderCommands.cs RemindRemove", ex);
        }
    }
}
