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
    public class BirthdayCommands : CommandBase, IBirthdayCommands
    {
        private readonly IBirthdayService birthdayService;

        public BirthdayCommands(Logging logger, Config config, IBirthdayService birthdayService) : base(logger, config)
        {
            this.birthdayService = birthdayService;
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
                    string[] strings = year.Split(new string[] { ",", "/", "\\", "-" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
