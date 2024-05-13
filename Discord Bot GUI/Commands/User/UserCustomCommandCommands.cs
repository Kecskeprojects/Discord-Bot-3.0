using Discord.Commands;
using Discord;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Enums;

namespace Discord_Bot.Commands.User
{
    public class UserCustomCommandCommands(
        ICustomCommandService customCommandService,
        IServerService serverService,
        Logging logger,
        Config config) : BaseCommand(logger, config, serverService)
    {
        private readonly ICustomCommandService customCommandService = customCommandService;

        [Command("custom list")]
        [Alias(["customlist", "customcommands"])]
        [RequireContext(ContextType.Guild)]
        [Summary("Command to list out all the currently available commands")]
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
                    await ReplyAsync("There are no custom commands on this server!");
                    return;
                }

                Embed[] embed = CustomCommandListEmbedProcessor.CreateEmbed(list);

                await ReplyAsync(embeds: embed);
            }
            catch (Exception ex)
            {
                logger.Error("UserCustomCommandCommands.cs CustomList", ex);
            }
        }
    }
}
