using DatabaseORM.Interface;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Interfaces;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using CommandResult = Discord.Commands.IResult;
using InteractionResult = Discord.Interactions.IResult;

namespace Discord_Bot.Tests.Common.Tools;

/// <summary>
/// The bot's real container (<see cref="Startup.ConfigureServices"/>) for integration tests, with only the outside world
/// replaced: a test config, a fake clock and a substitute logger. Replace more with <see cref="Replace{T}"/>, before the
/// first use of <see cref="Services"/>.
/// </summary>
public sealed class TestHost : IDisposable
{
    private readonly ServiceCollection services = [];
    private ServiceProvider? provider;
    private bool modulesLoaded;

    public TestHost()
    {
        Startup.ConfigureServices(services);
        //The type argument is the service being replaced, where it differs from the fake's own type
        Replace<IConfig>(Config);
        Replace<TimeProvider>(Clock);
        Replace(Logger);
    }

    public TestConfig Config { get; } = new();
    public FakeTimeProvider Clock { get; } = new(TestTimes.Now);
    public IBotLogger Logger { get; } = Substitute.For<IBotLogger>();

    //Built on first use, so every Replace has to come before it
    public IServiceProvider Services => provider ??= services.BuildServiceProvider();

    public TestHost Replace<T>(T instance) where T : class
    {
        if (provider != null)
        {
            throw new InvalidOperationException("Replace services before the first use of Services.");
        }

        services.RemoveAll<T>();
        services.AddSingleton(instance);
        return this;
    }

    //What App does at startup: fills the YouTube keys from the config
    public TestHost WithYoutubeKeysFromConfig()
    {
        Services.GetRequiredService<YoutubeKeyStore>().Reset(Config.Youtube_API_Keys);
        return this;
    }

    //Runs a text command the way CommandHandler does: in its own scope, after the one-character prefix
    public async Task<CommandResult> RunCommandAsync(ICommandContext context)
    {
        await LoadModulesAsync();
        using (IServiceScope scope = Services.CreateScope())
        {
            return await Services.GetRequiredService<CommandService>().ExecuteAsync(context, 1, scope.ServiceProvider);
        }
    }

    //Runs a button, select or modal the way InteractionHandler does, in its own scope
    public async Task<InteractionResult> RunInteractionAsync(IInteractionContext context)
    {
        await LoadModulesAsync();
        using (IServiceScope scope = Services.CreateScope())
        {
            return await Services.GetRequiredService<InteractionService>().ExecuteCommandAsync(context, scope.ServiceProvider);
        }
    }

    private async Task LoadModulesAsync()
    {
        if (!modulesLoaded)
        {
            await Services.GetRequiredService<CommandService>().AddModulesAsync(typeof(Startup).Assembly, Services);
            await Services.GetRequiredService<InteractionService>().AddModulesAsync(typeof(Startup).Assembly, Services);
            modulesLoaded = true;
        }
    }

    public void Dispose()
    {
        if (provider != null)
        {
            //Registered as instances, so the container leaves them to their creator (in the bot, BotMain disposes the client)
            provider.GetRequiredService<InteractionService>().Dispose();
            ((IDisposable) provider.GetRequiredService<CommandService>()).Dispose();
            provider.GetRequiredService<DiscordSocketClient>().Dispose();
            provider.Dispose();
        }
    }
}
