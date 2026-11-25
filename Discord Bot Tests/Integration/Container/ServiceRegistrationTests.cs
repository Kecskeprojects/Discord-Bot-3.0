using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord_Bot.Tests.Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot.Tests.Integration.Container;

//The registrations in Startup: a missing one fails here instead of when the bot starts or a command first runs
public sealed class ServiceRegistrationTests
{
    [Fact]
    public void EveryRegisteredService_HasEverythingItNeeds()
    {
        ServiceCollection services = [];
        Startup.ConfigureServices(services);

        //Throws on the first service with a dependency that isn't registered
        using (services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true }))
        {
        }
    }

    //Discord.Net creates the command and interaction modules itself, so the check above doesn't cover them
    [Theory]
    [MemberData(nameof(Modules))]
    public void EveryCommandAndInteractionModule_CanBeCreated(Type module)
    {
        using (TestHost host = new())
        using (IServiceScope scope = host.Services.CreateScope())
        {
            Assert.NotNull(ActivatorUtilities.CreateInstance(scope.ServiceProvider, module));
        }
    }

    public static IEnumerable<TheoryDataRow<Type>> Modules()
    {
        return typeof(Startup).Assembly.GetTypes().Where(IsModule).Select(type => new TheoryDataRow<Type>(type));
    }

    private static bool IsModule(Type type)
    {
        return !type.IsAbstract
            && (typeof(ModuleBase<ICommandContext>).IsAssignableFrom(type) || typeof(InteractionModuleBase<IInteractionContext>).IsAssignableFrom(type));
    }
}
