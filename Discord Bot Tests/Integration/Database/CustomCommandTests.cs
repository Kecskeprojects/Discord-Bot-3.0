using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//Custom commands through the real services and repositories on LocalDB: names are per server and in any letter case
public sealed class CustomCommandTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private const string Link = "https://example.com/wave.gif";
    private const string OtherLink = "https://example.com/other.gif";

    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        await database.AddAsync(Entities.Server(RowIds.New), Entities.Server(RowIds.New, DiscordIds.OtherServer));
    }

    [Fact]
    public async Task AddCustomCommandAsync_SavesTheNameInLowerCase_FoundInAnyCase()
    {
        Assert.Equal(DbProcessResultEnum.Success, await Service().AddCustomCommandAsync(DiscordIds.Server, " Wave ", Link));

        CustomCommandResource? command = await Service().GetCustomCommandAsync(DiscordIds.Server, "WAVE");
        Assert.NotNull(command);
        Assert.Equal(("wave", Link), (command.Command, command.Url));
    }

    [Fact]
    public async Task AddCustomCommandAsync_NameTakenInAnotherCase_ReturnsAlreadyExists()
    {
        await Service().AddCustomCommandAsync(DiscordIds.Server, "wave", Link);

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await Service().AddCustomCommandAsync(DiscordIds.Server, "Wave", OtherLink));
        Assert.Equal(Link, (await Service().GetCustomCommandAsync(DiscordIds.Server, "wave"))?.Url);
    }

    [Fact]
    public async Task AddCustomCommandAsync_SameNameOnAnotherServer_IsThatServersOwn()
    {
        await Service().AddCustomCommandAsync(DiscordIds.Server, "wave", Link);

        Assert.Equal(DbProcessResultEnum.Success, await Service().AddCustomCommandAsync(DiscordIds.OtherServer, "wave", OtherLink));
        Assert.Equal(Link, (await Service().GetCustomCommandAsync(DiscordIds.Server, "wave"))?.Url);
        Assert.Equal(OtherLink, (await Service().GetCustomCommandAsync(DiscordIds.OtherServer, "wave"))?.Url);
    }

    [Fact]
    public async Task AddCustomCommandAsync_ServerNotInTheDatabase_ReturnsNotFound()
    {
        Assert.Equal(DbProcessResultEnum.NotFound, await Service().AddCustomCommandAsync(299_999, "wave", Link));
    }

    [Fact]
    public async Task GetServerCustomCommandListAsync_GivesTheServersCommandsByName()
    {
        await Service().AddCustomCommandAsync(DiscordIds.Server, "wave", Link);
        await Service().AddCustomCommandAsync(DiscordIds.Server, "hello", Link);
        await Service().AddCustomCommandAsync(DiscordIds.OtherServer, "bye", OtherLink);

        Assert.Equal(["hello", "wave"], (await Service().GetServerCustomCommandListAsync(DiscordIds.Server)).Select(command => command.Command));
    }

    [Fact]
    public async Task RemoveCustomCommandAsync_NameInAnotherCase_RemovesOnlyThatServersCommand()
    {
        await Service().AddCustomCommandAsync(DiscordIds.Server, "wave", Link);
        await Service().AddCustomCommandAsync(DiscordIds.OtherServer, "wave", OtherLink);

        Assert.Equal(DbProcessResultEnum.Success, await Service().RemoveCustomCommandAsync(DiscordIds.Server, "WAVE"));
        Assert.Null(await Service().GetCustomCommandAsync(DiscordIds.Server, "wave"));
        Assert.NotNull(await Service().GetCustomCommandAsync(DiscordIds.OtherServer, "wave"));
        Assert.Equal(DbProcessResultEnum.NotFound, await Service().RemoveCustomCommandAsync(DiscordIds.Server, "wave"));
    }

    private ICustomCommandService Service()
    {
        return host.FromNewScope<ICustomCommandService>();
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }
}
