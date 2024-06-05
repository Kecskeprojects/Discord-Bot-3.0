using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Features;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Bot;
public class CommandHandler(
    IServiceProvider services,
    DiscordSocketClient client,
    CommandService commands,
    BotLogger logger)
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly DiscordSocketClient client = client;
    private readonly CommandService commands = commands;
    private readonly BotLogger logger = logger;
    #endregion

    public async Task RegisterCommandsAsync()
    {
        client.MessageReceived += HandleCommandAsync;
        commands.CommandExecuted += HandleCommandExecutionAsync;
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    #region Handle Command
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
                if (!DiscordTools.IsDM(arg))
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
            if (!DiscordTools.IsDM(context))
            {
                logger.MesUser(context.Message.Content, context.Guild.Name);
            }
            else
            {
                logger.MesUser(context.Message.Content);
            }

            //If message is not private message, and the server is not in our database, add it
            ServerResource server = await GetServerAsync(context);

            _ = context.Message.HasCharPrefix('!', ref argPos) || context.Message.HasCharPrefix('.', ref argPos)
                ? ExecuteCommandAsync(context, argPos)
                : !DiscordTools.IsDM(context) && DiscordTools.IsTypeOfChannel(server, ChannelTypeEnum.RoleText, context.Channel.Id, false)
                    ? HandleRoleAssignmentAsync(context, argPos)
                    : HandleFeatureCheckAsync(context);
        }
        catch (Exception ex)
        {
            logger.Error("CommandHandler.cs HandleCommandAsync", ex);
        }
    }

    private async Task<ServerResource> GetServerAsync(SocketCommandContext context)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            ServerResource server = null;
            if (!DiscordTools.IsDM(context))
            {
                IServerService serverService = scope.ServiceProvider.GetService<IServerService>();
                server = await serverService.GetByDiscordIdAsync(context.Guild.Id);
                if (server == null)
                {
                    DbProcessResultEnum result = await serverService.AddServerAsync(context.Guild.Id);
                    server = result == DbProcessResultEnum.Success
                        ? await serverService.GetByDiscordIdAsync(context.Guild.Id)
                        : throw new Exception($"{context.Guild.Name} could not be added to list!");
                }
            }
            return server;
        }
    }

    private async Task ExecuteCommandAsync(SocketCommandContext context, int argPos)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            await commands.ExecuteAsync(context, argPos, scope.ServiceProvider);
        }
    }

    private async Task HandleRoleAssignmentAsync(SocketCommandContext context, int argPos)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            if (context.Message.HasCharPrefix('+', ref argPos) || context.Message.HasCharPrefix('-', ref argPos))
            {
                //self roles
                SelfRoleFeature selfRoleFeature = scope.ServiceProvider.GetService<SelfRoleFeature>();
                await selfRoleFeature.Run(context);
            }

            await context.Message.DeleteAsync();
        }
    }

    private async Task HandleFeatureCheckAsync(SocketCommandContext context)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            EasterEggFeature easterEggFeature = scope.ServiceProvider.GetService<EasterEggFeature>();
            Config config = scope.ServiceProvider.GetService<Config>();
            await easterEggFeature.Run(context);

            //Make embed for instagram links
            if (config.Enable_Instagram_Embed)
            {
                InstagramEmbedFeature instagramEmbedFeature = scope.ServiceProvider.GetService<InstagramEmbedFeature>();
                await instagramEmbedFeature.Run(context);
            }
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
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    break;
                }
                case CommandError.UnknownCommand:
                {
                    _ = CheckCustomCommandAsync(context);
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
            if (!DiscordTools.IsDM(context as SocketCommandContext))
            {
                CustomCommandFeature customCommandFeature = scope.ServiceProvider.GetService<CustomCommandFeature>();
                await customCommandFeature.Run(context as SocketCommandContext);
            }
        }
    }
    #endregion
}
