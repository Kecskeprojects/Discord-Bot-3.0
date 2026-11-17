using DatabaseORM.Interface;
using Discord;
using Discord.LibDave.Binding;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot;

public sealed class BotMain(
    IServiceProvider services,
    InteractionHandler interactionHandler,
    CommandHandler commandHandler,
    DiscordSocketClient client,
    IBotLogger logger) : IDisposable
{
    #region Variables
    private readonly IServiceProvider services = services;
    private readonly InteractionHandler interactionHandler = interactionHandler;
    private readonly CommandHandler commandHandler = commandHandler;
    private readonly DiscordSocketClient client = client;
    private readonly IBotLogger logger = logger;

    //Stops the startup call and the reconnect timer from logging in at the same time
    private readonly SemaphoreSlim startLock = new(1, 1);
    private bool handlersRegistered;
    #endregion

    //Called on startup, and by the timer in App.xaml.cs whenever the client is logged out
    public async Task RunBotAsync()
    {
        if (!await startLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            if (!WebTools.TestConnection())
            {
                return;
            }

            //Handlers and modules are registered only once, otherwise every retry would add another copy and events would be handled multiple times
            if (!handlersRegistered)
            {
                Discord.LibDave.Dave.SetLogSink(LibDaveLogSink);

                client.Log += ClientLog;
                client.Ready += OnWebSocketReady;
                client.Disconnected += OnWebsocketDisconnect;

                await commandHandler.RegisterCommandsAsync();
                await interactionHandler.RegisterInteractionsAsync();

                handlersRegistered = true;
            }

            using (IServiceScope scope = services.CreateScope())
            {
                Config config = scope.ServiceProvider.GetRequiredService<Config>();
                if (string.IsNullOrEmpty(config.Token))
                {
                    logger.Error("BotMain.cs RunBotAsync", "Bot cannot start without a valid token, fill out the proper fields in the config file!");
                    return;
                }

                await client.LoginAsync(TokenType.Bot, config.Token);

                await client.StartAsync();

                logger.Log("Bot started!");
            }
        }
        finally
        {
            _ = startLock.Release();
        }
    }

    //Closes the gateway connection and logs out, so the bot goes offline right away instead of when Discord times the session out
    public async Task StopBotAsync()
    {
        await client.StopAsync();
        await client.LogoutAsync();
    }

    private void LibDaveLogSink(
        LoggingSeverity severity,
        string filePath,
        int lineNumber,
        string message
    )
    {
        if (severity > LoggingSeverity.Info)
        {
            logger.Audio(message);
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

                    if (arg.Exception.ToString().Contains("The server sent close 1001"))
                    {
                        foreach (KeyValuePair<ulong, Communication.ServerAudioResource> item in Global.ServerAudioResources)
                        {
                            item.Value.AudioVariables.CancellationTokenSource?.Cancel(false);
                        }
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

    //Called by App.xaml.cs at shutdown, after StopBotAsync. The client is disposed here because the root service provider never is
    public void Dispose()
    {
        startLock.Dispose();
        client.Dispose();
    }
}
