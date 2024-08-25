using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot;

public class BotMain(
    IServiceProvider services,
    InteractionHandler interactionHandler,
    CommandHandler commandHandler,
    DiscordSocketClient client,
    BotLogger logger)
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly InteractionHandler interactionHandler = interactionHandler;
    private readonly CommandHandler commandHandler = commandHandler;
    private readonly DiscordSocketClient client = client;
    private readonly BotLogger logger = logger;
    #endregion

    public async Task RunBotAsync()
    {
        if (!WebTools.TestConnection())
        {
            return;
        }

        client.Log += ClientLog;
        client.Ready += OnWebSocketReady;
        client.Disconnected += OnWebsocketDisconnect;

        await commandHandler.RegisterCommandsAsync();
        await interactionHandler.RegisterInteractionsAsync();

        using (IServiceScope scope = services.CreateScope())
        {
            Config config = scope.ServiceProvider.GetService<Config>();
            if (string.IsNullOrEmpty(config.Token))
            {
                logger.Error("App.xaml.cs RunBotAsync", "Bot cannot start without a valid token, fill out the proper fields in the config file!");
                return;
            }

            await client.LoginAsync(TokenType.Bot, config.Token);

            await client.StartAsync();

            logger.Log("Bot started!");
        }
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
                {
                    string message = arg.Exception.Message;
                    if (message.EndsWith('.'))
                    {
                        message = message[..^1];
                    }

                    foreach (KeyValuePair<ulong, Communication.ServerAudioResource> item in Global.ServerAudioResources)
                    {
                        item.Value.AudioVariables.CancellationTokenSource?.Cancel(false);
                    }

                    logger.Client($"{message}!", ConsoleOnly: true);
                    logger.Warning("BotMain.cs ClientLog", arg.Exception, LogOnly: true);
                    break;
                }
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
    #endregion
}
