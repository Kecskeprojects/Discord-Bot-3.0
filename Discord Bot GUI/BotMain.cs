using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Bot;

public class BotMain(
    IServiceProvider services,
    DiscordSocketClient client,
    InteractionService interactions,
    CommandService commands,
    Logging logger,
    Config config)
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly DiscordSocketClient client = client;
    private readonly InteractionService interactions = interactions;
    private readonly CommandService commands = commands;
    private readonly Logging logger = logger;
    private readonly Config config = config;
    #endregion

    public async Task RunBotAsync()
    {
        if (!WebTools.TestConnection())
        {
            return;
        }

        client.Log += ClientLog;
        client.Disconnected += OnWebsocketDisconnect;
        client.Ready += OnWebSocketReady;

        await RegisterCommandsAsync();
        await RegisterInteractionsAsync();

        if (string.IsNullOrEmpty(config.Token))
        {
            logger.Error("App.xaml.cs RunBotAsync", "Bot cannot start without a valid token, fill out the proper fields in the config file!");
            return;
        }

        await client.LoginAsync(TokenType.Bot, config.Token);

        await client.StartAsync();

        logger.Log("Bot started!");
    }

    #region Client Event Handlers
    private Task ClientLog(LogMessage arg)
    {
        try
        {
            if (arg.Exception == null)
            {
                logger.Client(arg.ToString());
                return Task.CompletedTask;
            }

            switch (arg.Exception.Message)
            {
                case "Server requested a reconnect":
                case "Unable to connect to the remote server":
                {
                    logger.Client($"{arg.Exception.Message}!");
                    break;
                }
                case "WebSocket connection was closed":
                case "WebSocket session expired":
                case "A task was canceled.":
                {
                    string message = arg.Exception.Message;
                    if (message.EndsWith('.'))
                    {
                        message = message[..^1];
                    }

                    logger.Client($"{message}!", ConsoleOnly: true);
                    logger.Warning("BotMain.cs ClientLog", arg.Exception, LogOnly: true);
                    break;
                }
                default:
                {
                    logger.Error("BotMain.cs ClientLog", arg.Exception);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs ClientLog", ex);
        }
        return Task.CompletedTask;
    }
    private async Task OnWebSocketReady()
    {
        try
        {
            await client.Rest.CurrentUser.UpdateAsync();
            logger.Log("Current user data updated!");
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs OnWebSocketReady", ex);
        }
    }

    private Task OnWebsocketDisconnect(Exception arg)
    {
        try
        {
            logger.Log("Disconnect handled!");
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs OnWebSocketDisconnect", ex);
        }
        return Task.CompletedTask;
    }

    public async Task RegisterCommandsAsync()
    {
        client.MessageReceived += HandleCommandAsync;
        commands.CommandExecuted += HandleCommandExecutionAsync;
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    public async Task RegisterInteractionsAsync()
    {
        client.InteractionCreated += HandleInteractionAsync;
        interactions.InteractionExecuted += HandleInteractionExecutionAsync;
        await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }
    #endregion

    #region Command and Interaction Event Handlers
    private async Task HandleCommandAsync(SocketMessage arg)
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
                if (arg.Channel.GetChannelType() != ChannelType.DM)
                {
                    logger.MesOther(arg.Content, (arg.Channel as SocketGuildChannel).Guild.Name);
                }
                else
                {
                    logger.MesOther(arg.Content);
                }
                return;
            }

            SocketCommandContext context = new(client, arg as SocketUserMessage);
            int argPos = 0;

            //Check if the message is an embed or not
            if (context.Message.Channel.GetChannelType() != ChannelType.DM)
            {
                logger.MesUser(context.Message.Content, context.Guild.Name);
            }
            else
            {
                logger.MesUser(context.Message.Content);
            }

            using (IServiceScope scope = services.CreateScope())
            {
                //If message is not private message, and the server is not in our database, add it
                ServerResource server = null;
                if (context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ICoreLogic coreLogic = scope.ServiceProvider.GetService<ICoreLogic>();
                    server = await coreLogic.GetOrAddServerAsync(context.Guild.Id, context.Guild.Name);
                    if (server == null)
                    {
                        return;
                    }
                }

                if (context.Message.HasCharPrefix('!', ref argPos) || context.Message.HasCharPrefix('.', ref argPos))
                {
                    _ = Task.Run(async () =>
                    {
                        using (IServiceScope commandScope = services.CreateScope())
                        {
                            await commands.ExecuteAsync(context, argPos, commandScope.ServiceProvider);
                        }
                    });
                    return;
                }
                else if (context.Channel.GetChannelType() != ChannelType.DM && DiscordTools.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id, false))
                {
                    if (context.Message.HasCharPrefix('+', ref argPos) || context.Message.HasCharPrefix('-', ref argPos))
                    {
                        //self roles
                        SelfRoleFeature selfRoleFeature = scope.ServiceProvider.GetService<SelfRoleFeature>();
                        await selfRoleFeature.Run(context);
                    }

                    await context.Message.DeleteAsync();
                    return;
                }
            }

            _ = Task.Run(async () =>
            {
                using (IServiceScope scope = services.CreateScope())
                {
                    EasterEggFeature easterEggFeature = scope.ServiceProvider.GetService<EasterEggFeature>();
                    await easterEggFeature.Run(context);

                    //Make embed for instagram links
                    if (config.Enable_Instagram_Embed)
                    {
                        InstagramEmbedFeature instagramEmbedFeature = scope.ServiceProvider.GetService<InstagramEmbedFeature>();
                        await instagramEmbedFeature.Run(context);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs HandleCommandAsync", ex);
        }
    }

    private async Task HandleCommandExecutionAsync(Optional<CommandInfo> info, ICommandContext context, Discord.Commands.IResult result)
    {
        try
        {
            switch (result.Error)
            {
                case null:
                    break;
                case CommandError.UnmetPrecondition:
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    break;
                }
                case CommandError.UnknownCommand:
                {
                    if (context.Channel.GetChannelType() != ChannelType.DM)
                    {
                        using (IServiceScope scope = services.CreateScope())
                        {
                            CustomCommandFeature customCommandFeature = scope.ServiceProvider.GetService<CustomCommandFeature>();
                            await customCommandFeature.Run(context as SocketCommandContext);
                        }
                    }
                    break;
                }
                default:
                {
                    logger.Warning("BotMain.cs HandleCommandExecutionAsync", result.ErrorReason);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs HandleCommandExecutionAsync", ex);
        }
    }

    private Task HandleInteractionAsync(SocketInteraction arg)
    {
        try
        {
            SocketInteractionContext context = new(client, arg);
            _ = Task.Run(async () =>
            {
                using (IServiceScope interactionScope = services.CreateScope())
                {
                    await interactions.ExecuteCommandAsync(context, interactionScope.ServiceProvider);
                }
            });
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs HandleInteractionAsync", ex);
        }
        return Task.CompletedTask;
    }

    private Task HandleInteractionExecutionAsync(ICommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
    {
        try
        {
            switch (result.Error)
            {
                case null:
                    break;
                default:
                {
                    logger.Warning("BotMain.cs HandleInteractionExecutionAsync", result.ErrorReason);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("BotMain.cs HandleInteractionExecutionAsync", ex);
        }
        return Task.CompletedTask;
    }
    #endregion
}
