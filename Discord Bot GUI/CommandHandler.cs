using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Features;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot;

public class CommandHandler(
    IServiceProvider services,
    DiscordSocketClient client,
    CommandService commands,
    IBotLogger logger)
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly DiscordSocketClient client = client;
    private readonly CommandService commands = commands;
    private readonly IBotLogger logger = logger;
    #endregion

    public async Task RegisterCommandsAsync()
    {
        client.MessageReceived += HandleCommandAsync;
        commands.CommandExecuted += HandleCommandExecutionAsync;
        commands.Log += HandleServiceLogAsync;
        _ = await commands.AddModulesAsync(typeof(CommandHandler).Assembly, services);
    }

    #region Handle Command
    private Task HandleCommandAsync(SocketMessage arg)
    {
        //Everything, including the database lookup, runs on the thread pool so the gateway task is never blocked
        //Commands still use RunMode.Sync inside, so their scope lives until they finish and their result reaches CommandExecuted
        _ = Task.Run(() => ProcessMessageAsync(arg));
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(SocketMessage arg)
    {
        try
        {
            if (string.IsNullOrEmpty(arg.Content))
            {
                return;
            }

            //In case the message was a system message (eg. the message seen when someone a pin is made), a webhook's or a bot's message
            //The function stops as it could cause an infinite loop
            if (arg.Source is MessageSource.System or MessageSource.Webhook or MessageSource.Bot)
            {
                if (!DiscordTools.IsDM(arg))
                {
                    logger.MesOther(arg.Content, ((SocketGuildChannel) arg.Channel).Guild.Name);
                }
                else
                {
                    logger.MesOther(arg.Content);
                }
                return;
            }

            if (arg is not SocketUserMessage message)
            {
                return;
            }

            SocketCommandContext context = new(client, message);
            int argPos = 0;

            //Check if the message is an embed or not
            if (!DiscordTools.IsDM(context))
            {
                logger.MesUser(context.Message.Content, context.Guild.Name);
            }
            else
            {
                logger.MesUser(context.Message.Content);
            }

            //If message is not private message, and the server is not in our database, add it
            ServerResource? server = await GetServerAsync(context);

            if (context.Message.HasCharPrefix('!', ref argPos) || context.Message.HasCharPrefix('.', ref argPos))
            {
                //Command execution
                await ExecuteCommandAsync(context, argPos);
            }
            else if (server != null && DiscordTools.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id, false))
            {
                //Role handling
                await HandleRoleAssignmentAsync(context, argPos);
            }
            else
            {
                //Feature handling
                await HandleFeatureCheckAsync(context);
            }
        }
        catch (Exception ex)
        {
            logger.Error("CommandHandler.cs ProcessMessageAsync", ex);
        }
    }

    //Null only in DMs; in a server it throws when the server can't be loaded or added
    private async Task<ServerResource?> GetServerAsync(SocketCommandContext context)
    {
        if (DiscordTools.IsDM(context))
        {
            return null;
        }

        using (IServiceScope scope = services.CreateScope())
        {
            IServerService serverService = scope.ServiceProvider.GetRequiredService<IServerService>();
            ServerResource? server = await serverService.GetByDiscordIdAsync(context.Guild.Id);
            if (server != null)
            {
                return server;
            }

            DbProcessResultEnum result = await serverService.AddServerAsync(context.Guild.Id);
            if (result != DbProcessResultEnum.Success)
            {
                throw new InvalidOperationException($"{context.Guild.Name} could not be added to list!");
            }

            return await serverService.GetByDiscordIdAsync(context.Guild.Id)
                ?? throw new InvalidOperationException($"{context.Guild.Name} was added to list, but could not be loaded!");
        }
    }

    private async Task ExecuteCommandAsync(SocketCommandContext context, int argPos)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            _ = await commands.ExecuteAsync(context, argPos, scope.ServiceProvider);
        }
    }

    private async Task HandleRoleAssignmentAsync(SocketCommandContext context, int argPos)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            if (context.Message.HasCharPrefix('+', ref argPos) || context.Message.HasCharPrefix('-', ref argPos))
            {
                //self roles
                SelfRoleFeature selfRoleFeature = scope.ServiceProvider.GetRequiredService<SelfRoleFeature>();
                _ = await selfRoleFeature.Run(context);
            }

            await context.Message.DeleteAsync();
        }
    }

    private async Task HandleFeatureCheckAsync(SocketCommandContext context)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            EasterEggFeature easterEggFeature = scope.ServiceProvider.GetRequiredService<EasterEggFeature>();
            Config config = scope.ServiceProvider.GetRequiredService<Config>();
            _ = await easterEggFeature.Run(context);

            //Make embed for instagram links
            //if (config.Enable_Instagram_Embed)
            //{
            //    InstagramEmbedFeature instagramEmbedFeature = scope.ServiceProvider.GetRequiredService<InstagramEmbedFeature>();
            //    _ = await instagramEmbedFeature.Run(context);
            //}
        }
    }
    #endregion

    #region Handle Command Execution
    private async Task HandleCommandExecutionAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
    {
        try
        {
            switch (result.Error)
            {
                case null:
                    break;
                case CommandError.UnmetPrecondition:
                case CommandError.ObjectNotFound:
                {
                    _ = await context.Channel.SendMessageAsync(result.ErrorReason);
                    break;
                }
                case CommandError.UnknownCommand:
                {
                    await CheckCustomCommandAsync(context);
                    break;
                }
                case CommandError.Exception when result is ExecuteResult { Exception: not null } executeResult:
                {
                    logger.Error("CommandHandler.cs HandleCommandExecutionAsync", executeResult.Exception);
                    break;
                }
                default:
                {
                    logger.Warning("CommandHandler.cs HandleCommandExecutionAsync", result.ErrorReason);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("CommandHandler.cs HandleCommandExecutionAsync", ex);
        }
    }

    private async Task CheckCustomCommandAsync(ICommandContext context)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            if (context is SocketCommandContext socketContext && !DiscordTools.IsDM(socketContext))
            {
                CustomCommandFeature customCommandFeature = scope.ServiceProvider.GetRequiredService<CustomCommandFeature>();
                _ = await customCommandFeature.Run(socketContext);
            }
        }
    }

    private Task HandleServiceLogAsync(LogMessage arg)
    {
        try
        {
            //Exceptions thrown inside commands arrive here wrapped in a CommandException,
            //they are already logged by HandleCommandExecutionAsync
            if (arg.Exception is CommandException)
            {
                return Task.CompletedTask;
            }

            if (arg.Exception != null)
            {
                logger.Error("CommandHandler.cs HandleServiceLogAsync", arg.Exception);
            }
            else
            {
                logger.Client(arg.ToString());
            }
        }
        catch (Exception ex)
        {
            logger.Error("CommandHandler.cs HandleServiceLogAsync", ex);
        }
        return Task.CompletedTask;
    }
    #endregion
}
