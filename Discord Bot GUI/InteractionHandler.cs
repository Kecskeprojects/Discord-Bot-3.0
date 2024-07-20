using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Bot;
public class InteractionHandler(
    IServiceProvider services,
    DiscordSocketClient client,
    InteractionService interactions,
    BotLogger logger)
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly DiscordSocketClient client = client;
    private readonly InteractionService interactions = interactions;
    private readonly BotLogger logger = logger;
    #endregion

    public async Task RegisterInteractionsAsync()
    {
        client.InteractionCreated += HandleInteractionAsync;
        interactions.InteractionExecuted += HandleInteractionExecutionAsync;
        await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    #region Handle Interaction
    private Task HandleInteractionAsync(SocketInteraction arg)
    {
        try
        {
            SocketInteractionContext context = new(client, arg);
            _ = ExecuteCommandAsync(context);
        }
        catch (Exception ex)
        {
            logger.Error("InteractionHandler.cs HandleInteractionAsync", ex);
        }
        return Task.CompletedTask;
    }

    private async Task ExecuteCommandAsync(SocketInteractionContext context)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            await interactions.ExecuteCommandAsync(context, scope.ServiceProvider);
        }
    }
    #endregion

    #region Handle Interaction Execution
    private Task HandleInteractionExecutionAsync(ICommandInfo info, IInteractionContext context, IResult result)
    {
        try
        {
            switch (result.Error)
            {
                case null:
                    break;
                default:
                {
                    logger.Warning("InteractionHandler.cs HandleInteractionExecutionAsync", result.ErrorReason);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("InteractionHandler.cs HandleInteractionExecutionAsync", ex);
        }
        return Task.CompletedTask;
    }
    #endregion
}
