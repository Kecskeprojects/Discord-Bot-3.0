using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//The bot's greeting gifs through the real services and repositories on LocalDB: !greeting remove takes the ID the list shows
public sealed class GreetingTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
    }

    [Fact]
    public async Task RemoveGreetingAsync_IdFromTheList_RemovesThatGreeting()
    {
        await Service().AddGreetingAsync("https://example.com/a.gif");
        await Service().AddGreetingAsync("https://example.com/b.gif");
        GreetingResource first = (await Service().GetAllGreetingAsync()).Single(greeting => greeting.Url == "https://example.com/a.gif");

        Assert.Equal(DbProcessResultEnum.Success, await Service().RemoveGreetingAsync(first.GreetingId));
        Assert.Equal(["https://example.com/b.gif"], (await Service().GetAllGreetingAsync()).Select(greeting => greeting.Url));
        Assert.Equal(DbProcessResultEnum.NotFound, await Service().RemoveGreetingAsync(first.GreetingId));
    }

    private IGreetingService Service()
    {
        return host.FromNewScope<IGreetingService>();
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }
}
