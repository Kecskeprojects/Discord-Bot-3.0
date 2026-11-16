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

namespace Discord_Bot.Commands.Owner;

[Name("Greeting")]
[Remarks("Owner")]
[Summary("Manage greeting gifs of bot, one is choosen if the bot is pinged as a response")]
public class OwnerGreetingCommands(
    IGreetingService greetingService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IGreetingService greetingService = greetingService;

    [Command("greeting list")]
    [RequireOwner]
    [Summary("Check global list of greeting gifs")]
    public async Task GreetingList()
    {
        try
        {
            List<GreetingResource> greetings = await greetingService.GetAllGreetingAsync();
            if (!CollectionTools.IsNullOrEmpty(greetings))
            {
                Embed[] embed = GreetingListEmbedProcessor.CreateEmbed(greetings);
                _ = await ReplyAsync(embeds: embed);
            }
        }
        catch (Exception ex)
        {
            logger.Error("OwnerGreetingCommands.cs GreetingList", ex);
        }
    }

    [Command("greeting add")]
    [RequireOwner]
    [Summary("Add a new greeting gif")]
    public async Task GreetingAdd([Name("gif link")] string giflink)
    {
        try
        {
            DbProcessResultEnum result = await greetingService.AddGreetingAsync(giflink);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Greeting added.",
                _ => "Greeting could not be added!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerGreetingCommands.cs GreetingAdd", ex);
        }
    }

    [Command("greeting remove")]
    [RequireOwner]
    [Summary("Remove a greeting gif based on it's ID, that is displayed in the '!greeting list' command")]
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
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("OwnerGreetingCommands.cs GreetingRemove", ex);
        }
    }
}
