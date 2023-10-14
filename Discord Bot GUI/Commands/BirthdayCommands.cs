using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Commands;
using System.Threading.Tasks;
using System;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;

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
        public async Task BirthdayAdd(string year, string month, string day)
        {
            try
            {
                if(DateTime.TryParse($"{year}.{month}.{day}", out DateTime date))
                {
                    DbProcessResultEnum result = await birthdayService.AddBirthdayAsync(Context.Guild.Id, Context.User.Id, date);
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
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayCommands.cs BirthdayRemove", ex.ToString());
            }
        }
    }
}
