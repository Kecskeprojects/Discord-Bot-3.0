using DatabaseORM.Interface;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot;

public class InteractionHandler(
    IServiceProvider services,
    DiscordSocketClient client,
    InteractionService interactions,
    IBotLogger logger)
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly DiscordSocketClient client = client;
    private readonly InteractionService interactions = interactions;
    private readonly IBotLogger logger = logger;
    #endregion

    public async Task RegisterInteractionsAsync()
    {
        client.InteractionCreated += HandleInteractionAsync;
        interactions.InteractionExecuted += HandleInteractionExecutionAsync;
        interactions.Log += HandleServiceLogAsync;
        _ = await interactions.AddModulesAsync(typeof(InteractionHandler).Assembly, services);
    }

    #region Handle Interaction
    private Task HandleInteractionAsync(SocketInteraction arg)
    {
        //Executed in the background so the gateway task is not blocked
        _ = ExecuteCommandAsync(arg);
        return Task.CompletedTask;
    }

    private async Task ExecuteCommandAsync(SocketInteraction arg)
    {
        try
        {
            SocketInteractionContext context = new(client, arg);
            using (IServiceScope scope = services.CreateScope())
            {
                _ = await interactions.ExecuteCommandAsync(context, scope.ServiceProvider);
            }
        }
        catch (Exception ex)
        {
            logger.Error("InteractionHandler.cs ExecuteCommandAsync", ex);
        }
    }
    #endregion

    #region Handle Interaction Execution
    private async Task HandleInteractionExecutionAsync(ICommandInfo info, IInteractionContext context, IResult result)
    {
        try
        {
            if (result.IsSuccess)
            {
                return;
            }

            if (result is ExecuteResult { Exception: not null } executeResult)
            {
                logger.Error("InteractionHandler.cs HandleInteractionExecutionAsync", executeResult.Exception);
            }
            else
            {
                logger.Warning("InteractionHandler.cs HandleInteractionExecutionAsync", $"{info?.Name ?? "Unknown interaction"}: {result.Error} - {result.ErrorReason}");
            }

            //Without a response the user only sees "This interaction failed"
            string message = result.Error switch
            {
                InteractionCommandError.UnknownCommand => "This button or menu is no longer available.",
                InteractionCommandError.UnmetPrecondition => result.ErrorReason,
                _ => "Something went wrong during the process."
            };
            await DiscordTools.RespondOrFollowupAsync(context.Interaction, message, ephemeral: true);
        }
        catch (Exception ex)
        {
            logger.Error("InteractionHandler.cs HandleInteractionExecutionAsync", ex);
        }
    }

    private Task HandleServiceLogAsync(LogMessage arg)
    {
        try
        {
            //Exceptions thrown inside interactions arrive here wrapped in an InteractionException,
            //they are already logged by HandleInteractionExecutionAsync
            if (arg.Exception is InteractionException)
            {
                return Task.CompletedTask;
            }

            if (arg.Exception != null)
            {
                logger.Error("InteractionHandler.cs HandleServiceLogAsync", arg.Exception);
            }
            else
            {
                logger.Client(arg.ToString());
            }
        }
        catch (Exception ex)
        {
            logger.Error("InteractionHandler.cs HandleServiceLogAsync", ex);
        }
        return Task.CompletedTask;
    }
    #endregion
}
