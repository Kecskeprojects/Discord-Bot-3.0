using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class CustomCommandFeature(
    ICustomCommandService customCommandService,
    IServerService serverService,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly ICustomCommandService customCommandService = customCommandService;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            CustomCommandResource command = await customCommandService.GetCustomCommandAsync(Context.Guild.Id, Context.Message.Content[1..].ToLower());
            if (command != null)
            {
                _ = await Context.Channel.SendMessageAsync(command.Url);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.Error("CustomCommandFeature.cs ExecuteCoreLogicAsync", ex);
        }
        return false;
    }
}
