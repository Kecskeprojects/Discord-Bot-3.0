using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Help;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner;

public class OwnerHelpCommands(IServerService serverService, BotLogger logger, Config config) : BaseCommand(logger, config, serverService)
{
    [Command("help owner")]
    [RequireOwner]
    [Summary("Embed complete list of commands in a text file")]
    public async Task Help()
    {
        try
        {
            Embed[] embed = HelpOwnerEmbedProcessor.CreateEmbed(config.Img);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerHelpCommands.cs Help", ex);
        }
    }
}
