using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

[Name("Custom Command")]
[Remarks("Admin")]
[Summary("Modifying list of server specific commands")]
public class AdminCustomCommandCommands(
    ICustomCommandService customCommandService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly ICustomCommandService customCommandService = customCommandService;

    [Command("command add")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding command to server, mainly gifs and pictures")]
    public async Task CustomCommandAdd([Name("command name")] string commandname, [Name("response link")] string responselink)
    {
        try
        {
            //Check if the url is a valid url, not just a string of characters
            if (Uri.IsWellFormedUriString(responselink, UriKind.Absolute))
            {
                DbProcessResultEnum result = await customCommandService.AddCustomCommandAsync(Context.Guild.Id, commandname, responselink);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"New command successfully added: {commandname}.",
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
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing command from server")]
    public async Task CustomCommandRemove([Name("command name")] string commandname)
    {
        try
        {
            DbProcessResultEnum result = await customCommandService.RemoveCustomCommandAsync(Context.Guild.Id, commandname);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"The {commandname} command has been removed.",
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
