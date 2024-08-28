using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

[Name("Keyword")]
[Remarks("Admin")]
[Summary("Managing server specific keywords")]
public class AdminKeywordCommands(
    IKeywordService keywordService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IKeywordService keywordService = keywordService;

    [Command("keyword list")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Check current trigger words/sentences and their responses on the server")]
    public async Task KeywordList()
    {
        try
        {
            List<KeywordResource> keywords = await keywordService.ListKeywordsByServerIdAsync(Context.Guild.Id);
            Embed[] embed = KeywordListEmbedProcessor.CreateEmbed(Context.Guild.Name, keywords);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("AdminKeywordCommands.cs KeywordList", ex);
        }
    }

    [Command("keyword add")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Add trigger word/sentence to current server that if typed in itself will trigger a reaction from the bot\n*Limited to 100 characters\n**Limited to 300 characters")]
    public async Task KeywordAdd([Name("keyword*>response**")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '>', false);
            if (paramArray.Length != 2)
            {
                return;
            }

            string keyword = paramArray[0];
            string response = paramArray[1];

            DbProcessResultEnum result = await keywordService.AddKeywordAsync(Context.Guild.Id, keyword, response);
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
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Remove trigger word from current server")]
    public async Task KeywordRemove([Remainder] string keyword)
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
