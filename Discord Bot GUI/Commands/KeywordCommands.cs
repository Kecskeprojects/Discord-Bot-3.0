using Discord.Commands;
using Discord;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using System.Threading.Tasks;
using System;

namespace Discord_Bot.Commands
{
    public class KeywordCommands : CommandBase, IKeywordCommands
    {
        private readonly IKeywordService keywordService;

        public KeywordCommands(IKeywordService keywordService, Logging logger, Config config) : base(logger, config)
        {
            this.keywordService = keywordService;
        }

        [Command("keyword add")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [Summary("Keyword addition to database")]
        public async Task KeywordAdd([Remainder] string keyword_response)
        {
            try
            {
                string[] array = keyword_response[1..^1].Split("` `");

                DbProcessResultEnum result = await keywordService.AddKeywordAsync(Context.Guild.Id, array[0], array[1]);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Keyword added to database!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Keyword already in database!");
                }
                else
                {
                    await ReplyAsync("Keyword could not be added to database!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs KeywordAdd", ex.ToString());
            }
        }

        [Command("keyword remove")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [Summary("Keyword removal to database")]
        public async Task KeywordRemove(string keyword)
        {
            try
            {
                DbProcessResultEnum result = await keywordService.RemoveKeywordAsync(Context.Guild.Id, keyword);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Keyword removed from database!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Keyword could not be found.");
                }
                else
                {
                    await ReplyAsync("Keyword could not be removed from database!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs KeywordRemove", ex.ToString());
            }
        }
    }
}
