using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions;
public class HelpComponentInteraction(
    CommandService commandService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseInteraction(serverService, logger, config)
{

    [ComponentInteraction("HelpMenu_*")]
    public async Task HelpMenuHandler(CommandLevelEnum commandLevel, string[] selectedCategories)
    {
        try
        {
            await DeferAsync();
            logger.Log($"Help menu item selected with following parameters: {commandLevel}, {string.Join(",", selectedCategories)}", LogOnly: true);

            string category = selectedCategories[0];

            if ((commandLevel == CommandLevelEnum.Owner && !await IsOwner()) ||
                (commandLevel == CommandLevelEnum.Admin && !IsAdmin()) ||
                (commandLevel == CommandLevelEnum.User && !await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true)))
            {
                return;
            }

            IReadOnlyList<CommandInfo> commands = commandService.Modules
                .Where(x => x.Remarks == commandLevel.ToString() && x.Name == category)
                .First()
                .Commands;

            Embed[] embed = HelpDetailEmbedProcessor.CreateEmbed(commandLevel, category, [.. commands], config.Img);

            await FollowupAsync(embeds: embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            logger.Error("HelpComponentInteraction.cs HelpMenuHandler", ex);
            await RespondAsync("Something went wrong while creating help detail embed.");
        }
    }
}
