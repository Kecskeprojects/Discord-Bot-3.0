using Discord;
using Discord.Commands;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class ReminderCommands(IReminderService reminderService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IReminderCommands
    {
        private readonly IReminderService reminderService = reminderService;

        [Command("remind at")]
        [Alias(["reminder at"])]
        [Summary("Adding reminding messages to database via dates")]
        public async Task RemindAt([Remainder] string message)
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                if (message.Split(">").Length < 2)
                {
                    return;
                }

                //Take the message apart and clear trailing whitespaces
                string datestring = message.Split(">")[0].Trim();
                string remindMessage = message.Split(">")[1].Trim();

                //Length check, the message row of the database only accepts lengths of up to 150
                if (remindMessage.Length > 150)
                {
                    await ReplyAsync("Reminder message too long!(maximum **150** characters)");
                    return;
                }

                //Add last two digits of current year to beginning in case it was left off as the datetime parse doesn't always assume a year
                if (datestring.Split(".").Length == 2)
                {
                    datestring = datestring.Insert(0, $"{DateTime.Now.Year.ToString()[2..]}.");
                }

                //Try parsing date into an exact format, in which case one can write timezones
                if (DateTime.TryParseExact(datestring, "yy.MM.dd HH:mm z", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime date))
                {
                    //Convert date to local timezone
                    DateTime ConvertedDate = date.ToUniversalTime();

                    //Check if date is not already in the past
                    if (DateTime.Compare(ConvertedDate, DateTime.UtcNow) > 0)
                    {
                        DbProcessResultEnum result = await reminderService.AddReminderAsync(Context.User.Id, ConvertedDate, remindMessage);

                        if (result == DbProcessResultEnum.Success)
                        {
                            await ReplyAsync($"Alright, I will remind you at {TimestampTag.FromDateTime(date, TimestampTagStyles.ShortDateTime)}!");
                        }
                        else
                        {
                            await ReplyAsync("Reminder could not be added, talk to dumbass owner.");
                        }

                        return;
                    }
                }
                else
                {
                    //Try parsing the date
                    if (DateTime.TryParse(datestring, out date))
                    {
                        //Check if date is not already in the past
                        if (DateTime.Compare(date, DateTime.UtcNow) > 0)
                        {
                            //Add reminder to database
                            DbProcessResultEnum result = await reminderService.AddReminderAsync(Context.User.Id, date, remindMessage);

                            if (result == DbProcessResultEnum.Success)
                            {
                                await ReplyAsync($"Alright, I will remind you at {TimestampTag.FromDateTime(date, TimestampTagStyles.ShortDateTime)}!");
                            }
                            else
                            {
                                await ReplyAsync("Reminder could not be added, talk to dumbass owner.");
                            }

                            return;
                        }
                    }
                }

                await ReplyAsync("Invalit input format, the order is the following:\n`[year].[month].[day] [hour]:[minute] +-[timezone]`\nYear, hour, minute are optional unless using timezones!");
            }
            catch (Exception ex)
            {
                logger.Error("ReminderCommands.cs RemindAt", ex.ToString());
            }
        }

        [Command("remind in")]
        [Alias(["reminder in"])]
        [Summary("Adding reminding messages to database via amounts of time from current date")]
        public async Task RemindIn([Remainder] string message)
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                if (message.Split(">").Length < 2)
                {
                    return;
                }

                //Take the message apart and clear trailing whitespaces
                string amountstring = message.Split(">")[0].Trim();
                string remindMessage = message.Split(">")[1].Trim();
                List<string> amounts = ReminderService.GetAmountsList(amountstring);

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
                    if (!ReminderService.TryAddValuesToDate(amounts, out DateTime date))
                    {
                        return;
                    }

                    DbProcessResultEnum result = await reminderService.AddReminderAsync(Context.User.Id, date, remindMessage);

                    if (result == DbProcessResultEnum.Success)
                    {
                        await ReplyAsync($"Alright, I will remind you at {TimestampTag.FromDateTime(date, TimestampTagStyles.Relative)}!");
                    }
                    else
                    {
                        await ReplyAsync("Reminder could not be added, talk to dumbass owner.");
                    }
                }
                else
                {
                    await ReplyAsync("Incorrect number of inputs!");
                }

            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs RemindIn", ex.ToString());
            }
        }

        [Command("remind list")]
        [Alias(["reminder list"])]
        [Summary("Remove a reminder from their list of reminders")]
        public async Task RemindList()
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                List<ReminderResource> list = await reminderService.GetUserReminderListAsync(Context.User.Id);

                if (list.Count > 0)
                {
                    EmbedBuilder builder = new();
                    builder.WithTitle("Your reminders:");

                    int i = 1;
                    foreach (ReminderResource reminder in list)
                    {
                        builder.AddField($"#{i} {TimestampTag.FromDateTime(reminder.Date, TimestampTagStyles.ShortDateTime)}", reminder.Message);
                        i++;
                    }

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs RemindList", ex.ToString());
            }
        }

        [Command("remind remove")]
        [Alias(["reminder remove"])]
        [Summary("Remove a reminder from the user's list of reminders")]
        public async Task RemindRemove(int reminderOrderId)
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                DbProcessResultEnum result = await reminderService.RemoveUserReminderAsync(Context.User.Id, reminderOrderId);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync($"Reminder in position #{reminderOrderId} has been removed!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Reminder doesn't exist with that ID or it is not yours!");
                }
                else
                {
                    await ReplyAsync("Reminder could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs RemindRemove", ex.ToString());
            }
        }
    }
}
