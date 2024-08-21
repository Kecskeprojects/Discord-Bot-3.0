﻿using Discord;
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
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding command to server, mainly gifs and pictures")]
    public async Task CustomCommandAdd(string commandName, string responseLink)
    {
        try
        {
            //Check if the url is a valid url, not just a string of characters
            if (Uri.IsWellFormedUriString(responseLink, UriKind.Absolute))
            {
                DbProcessResultEnum result = await customCommandService.AddCustomCommandAsync(Context.Guild.Id, commandName, responseLink);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"New command successfully added: {commandName}.",
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
    [Summary("Removing command from server")]
    public async Task CustomCommandRemove(string commandName)
    {
        try
        {
            DbProcessResultEnum result = await customCommandService.RemoveCustomCommandAsync(Context.Guild.Id, commandName);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"The {commandName} command has been removed.",
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
