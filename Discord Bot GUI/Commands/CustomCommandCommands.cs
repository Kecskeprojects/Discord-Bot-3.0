using Discord;
using Discord.Commands;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class CustomCommandCommands(ICustomCommandService customCommandService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService)
    {
        private readonly ICustomCommandService customCommandService = customCommandService;

        [Command("custom list")]
        [RequireContext(ContextType.Guild)]
        [Alias(["customlist", "customcommands"])]
        [Summary("Command to list out all the currently available commands")]
        public async Task CustomList()
        {
            try
            {
                List<CustomCommandResource> list = await customCommandService.GetServerCustomCommandListAsync(Context.Guild.Id);
                if (CollectionTools.IsNullOrEmpty(list))
                {
                    await ReplyAsync("There are no custom commands on this server!");
                    return;
                }

                EmbedBuilder builder = ChatService.BuildCustomListEmbed(list);

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception ex)
            {
                logger.Error("CustomCommandCommands.cs CustomList", ex.ToString());
            }
        }

        [Command("command add")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [Summary("Custom command adding, gifs and pics mainly")]
        public async Task CustomCommandAdd(string name, string link)
        {
            try
            {
                //Check if the url is a valid url, not just a string of characters
                if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                {
                    DbProcessResultEnum result = await customCommandService.AddCustomCommandAsync(Context.Guild.Id, name, link);
                    if (result == DbProcessResultEnum.Success)
                    {
                        await ReplyAsync($"New command successfully added: {name}");
                    }
                    else if (result == DbProcessResultEnum.AlreadyExists)
                    {
                        await ReplyAsync("A command with this name already exists on this server!");
                    }
                    else
                    {
                        await ReplyAsync("Command could not be added!");
                    }
                }
                else
                {
                    await ReplyAsync("That link is invalid!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("CustomCommandCommands.cs CustomCommandAdd", ex.ToString());
            }
        }

        [Command("command remove")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [Summary("Custom command removing, gifs and pics mainly")]
        public async Task CustomCommandRemove(string name)
        {
            try
            {
                DbProcessResultEnum result = await customCommandService.RemoveCustomCommandAsync(Context.Guild.Id, name);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync($"The {name} command has been removed.");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Command does not exist.");
                }
                else
                {
                    await ReplyAsync("Command could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("CustomCommandCommands.cs CustomCommandRemove", ex.ToString());
            }
        }
    }
}
