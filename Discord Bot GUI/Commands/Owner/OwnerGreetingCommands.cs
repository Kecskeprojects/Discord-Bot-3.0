﻿using Discord;
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

namespace Discord_Bot.Commands.Owner;

public class OwnerGreetingCommands(
    IGreetingService greetingService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IGreetingService greetingService = greetingService;

    [Command("greeting list")]
    [RequireOwner]
    [Summary("Command for owner to list global greeting gifs")]
    public async Task GreetingList()
    {
        try
        {
            List<GreetingResource> greetings = await greetingService.GetAllGreetingAsync();
            if (!CollectionTools.IsNullOrEmpty(greetings))
            {
                Embed[] embed = GreetingListEmbedProcessor.CreateEmbed(greetings);
                await ReplyAsync(embeds: embed);
            }
        }
        catch (Exception ex)
        {
            logger.Error("OwnerGreetingCommands.cs GreetingList", ex);
        }
    }

    [Command("greeting add")]
    [RequireOwner]
    [Summary("Command for owner to add global greeting gifs")]
    public async Task GreetingAdd(string url)
    {
        try
        {
            DbProcessResultEnum result = await greetingService.AddGreetingAsync(url);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Greeting added.",
                _ => "Greeting could not be added!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerGreetingCommands.cs GreetingAdd", ex);
        }
    }

    [Command("greeting remove")]
    [RequireOwner]
    [Summary("Command for owner to remove global greeting gifs")]
    public async Task GreetingRemove(int id)
    {
        try
        {
            DbProcessResultEnum result = await greetingService.RemoveGreetingAsync(id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => $"Greeting with the ID {id} has been removed.",
                DbProcessResultEnum.NotFound => "Greeting doesn't exist with that ID or it is not yours.",
                _ => "Greeting could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerGreetingCommands.cs GreetingRemove", ex);
        }
    }
}
