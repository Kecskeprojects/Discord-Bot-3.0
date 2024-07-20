using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

public class AdminKeywordCommands(
    IKeywordService keywordService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IKeywordService keywordService = keywordService;

    [Command("keyword add")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Keyword addition to database")]
    public async Task KeywordAdd([Remainder] string keyword_response)
    {
        try
        {
            string[] array = keyword_response.Split(">");

            DbProcessResultEnum result = await keywordService.AddKeywordAsync(Context.Guild.Id, array[0], array[1]);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Keyword added to database.",
                DbProcessResultEnum.AlreadyExists => "Keyword already in database.",
                _ => "Keyword could not be added to database!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminKeywordCommands.cs KeywordAdd", ex);
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
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Keyword removed from database.",
                DbProcessResultEnum.NotFound => "Keyword could not be found.",
                _ => "Keyword could not be removed from database!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminKeywordCommands.cs KeywordRemove", ex);
        }
    }
}
