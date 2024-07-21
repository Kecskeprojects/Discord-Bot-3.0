using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.Help;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

public class AdminHelpCommands(
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    [Command("help admin")]
    [Alias(["help a"])]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Embed complete list of commands in a text file")]
    public async Task Help()
    {
        try
        {
            Embed[] embed = HelpAdminEmbedProcessor.CreateEmbed(config.Img);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("AdminHelpCommands.cs Help", ex);
        }
    }

    [Command("feature")]
    [Alias(["features"])]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Embed complete list of features")]
    public async Task Features()
    {
        try
        {
            Embed[] embed = FeaturesEmbedProcessor.CreateEmbed(config.Img);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("AdminHelpCommands.cs Features", ex);
        }
    }

    [Command("help channel banner")]
    [Alias(["help messages", "help banner", "help channel messages"])]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Embed guide to managing channel banner messages")]
    public async Task HelpChannelBanner()
    {
        try
        {
            //Short explanation, an attached json for example and links to online embed editors
            await ReplyAsync("HelpChannelBanner Placeholder.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminHelpCommands.cs HelpChannelBanner", ex);
        }
    }
}
