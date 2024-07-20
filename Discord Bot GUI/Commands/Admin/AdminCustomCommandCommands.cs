using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;
public class AdminCustomCommandCommands(
    ICustomCommandService customCommandService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly ICustomCommandService customCommandService = customCommandService;

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
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"New command successfully added: {name}.",
                    DbProcessResultEnum.AlreadyExists => "A command with this name already exists on this server.",
                    _ => "Command could not be added!"
                };
                await ReplyAsync(resultMessage);
            }
            else
            {
                await ReplyAsync("That link is invalid!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminCustomCommandCommands.cs CustomCommandAdd", ex);
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
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"The {name} command has been removed.",
                DbProcessResultEnum.NotFound => "Command does not exist.",
                _ => "Command could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminCustomCommandCommands.cs CustomCommandRemove", ex);
        }
    }
}
