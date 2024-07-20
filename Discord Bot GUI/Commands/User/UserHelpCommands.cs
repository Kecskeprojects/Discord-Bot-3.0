using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Help;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;
public class UserHelpCommands(
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{

    [Command("help")]
    [Summary("List out commands everybody has access to")]
    public async Task Help()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            Embed[] embed = HelpUserEmbedProcessor.CreateEmbed(config.Img);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("UserHelpCommands.cs Help", ex);
        }
    }
}
