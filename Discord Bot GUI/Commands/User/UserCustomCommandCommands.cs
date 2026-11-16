using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Custom Command")]
[Remarks("User")]
[Summary("Server specific command related commands")]
public class UserCustomCommandCommands(
    ICustomCommandService customCommandService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly ICustomCommandService customCommandService = customCommandService;

    [Command("custom list")]
    [Alias(["customlist", "customcommands"])]
    [RequireContext(ContextType.Guild)]
    [Summary("List of currently existing custom commands")]
    public async Task CustomList()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText))
            {
                return;
            }

            List<CustomCommandResource> list = await customCommandService.GetServerCustomCommandListAsync(Context.Guild.Id);
            if (CollectionTools.IsNullOrEmpty(list))
            {
                _ = await ReplyAsync("There are no custom commands on this server!");
                return;
            }

            Embed[] embed = CustomCommandListEmbedProcessor.CreateEmbed(list);

            _ = await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("UserCustomCommandCommands.cs CustomList", ex);
        }
    }
}
