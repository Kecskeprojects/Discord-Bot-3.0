using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands;

public class HelpCommands(
    CommandService commandService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly CommandService commandService = commandService;

    [Command("help")]
    public async Task Help(string commandLevel = "user")
    {
        try
        {
            commandLevel = commandLevel.ToLower();

            CommandLevelEnum commandLevelEnum = commandLevel switch
            {
                "o" or "own" or "owner" => CommandLevelEnum.Owner,
                "a" or "adm" or "admin" => CommandLevelEnum.Admin,
                "user" => CommandLevelEnum.User,
                _ => CommandLevelEnum.User
            };

            if ((commandLevelEnum == CommandLevelEnum.Owner && !await IsOwner()) ||
                (commandLevelEnum == CommandLevelEnum.Admin && !IsAdmin()) ||
                (commandLevelEnum == CommandLevelEnum.User && !await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true)))
            {
                return;
            }

            List<ModuleInfo> modules = commandService.Modules
                .Where(x => x.Remarks == commandLevelEnum.ToString())
                .ToList();

            Embed[] embed = HelpEmbedProcessor.CreateEmbed(commandLevelEnum, modules, config.Img);
            MessageComponent moduleSelector = HelpEmbedProcessor.CreateComponent(commandLevelEnum, modules);

            await ReplyAsync(embeds: embed, components: moduleSelector);
        }
        catch (Exception ex)
        {
            logger.Error("HelpCommands.cs Help", ex);
        }
    }
}
