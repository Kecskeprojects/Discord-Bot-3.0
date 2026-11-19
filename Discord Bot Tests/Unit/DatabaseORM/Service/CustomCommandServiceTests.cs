using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

public sealed class CustomCommandServiceTests
{
    private const string Url = "https://example.com/hello.gif";

    private readonly ICustomCommandRepository commands = Substitute.For<ICustomCommandRepository>();
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly CustomCommandService service;

    private readonly Server server = Entities.Server();
    private readonly Server otherServer = Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer);

    public CustomCommandServiceTests()
    {
        service = new CustomCommandService(commands, servers, TestMapper.Database, logger, new ServerCache());
        servers.HasRows(server, otherServer);
    }

    [Fact]
    public async Task AddCustomCommandAsync_StoresTheTrimmedLowercaseName()
    {
        List<CustomCommand> table = commands.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddCustomCommandAsync(DiscordIds.Server, " Hello ", Url));
        CustomCommand added = Assert.Single(table);
        Assert.Equal("hello", added.Command);
        Assert.Equal(Url, added.Url);
        Assert.Same(server, added.Server);
    }

    [Fact]
    public async Task AddCustomCommandAsync_NameTakenOnTheServer_ReturnsAlreadyExists()
    {
        List<CustomCommand> table = commands.HasRows(Command(server, "hello"));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddCustomCommandAsync(DiscordIds.Server, "HELLO", Url));
        Assert.Single(table);
    }

    [Fact]
    public async Task AddCustomCommandAsync_NameUsedOnAnotherServer_AddsItHereToo()
    {
        List<CustomCommand> table = commands.HasRows(Command(otherServer, "hello"));

        Assert.Equal(DbProcessResultEnum.Success, await service.AddCustomCommandAsync(DiscordIds.Server, "hello", Url));
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public async Task AddCustomCommandAsync_UnknownServer_ReturnsNotFound()
    {
        servers.HasRows();
        commands.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.AddCustomCommandAsync(DiscordIds.Server, "hello", Url));
    }

    [Fact]
    public async Task AddCustomCommandAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        commands.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddCustomCommandAsync(DiscordIds.Server, "hello", Url));
        logger.Received().Error("CustomCommandService.cs AddCustomCommandAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetCustomCommandAsync_NameInAnotherCase_FindsTheServersCommand()
    {
        commands.HasRows(Command(otherServer, "hello"), Command(server, "hello"));

        CustomCommandResource? command = await service.GetCustomCommandAsync(DiscordIds.Server, " Hello");

        Assert.NotNull(command);
        Assert.Equal($"{Url}?server={RowIds.Server}", command.Url);
    }

    [Fact]
    public async Task GetCustomCommandAsync_Unknown_GivesNull()
    {
        commands.HasRows(Command(otherServer, "hello"));

        Assert.Null(await service.GetCustomCommandAsync(DiscordIds.Server, "hello"));
    }

    [Fact]
    public async Task GetServerCustomCommandListAsync_GivesTheServersCommandsByName()
    {
        commands.HasRows(Command(server, "wave"), Command(otherServer, "elsewhere"), Command(server, "hello"));

        List<CustomCommandResource> result = await service.GetServerCustomCommandListAsync(DiscordIds.Server);

        Assert.Equal(["hello", "wave"], result.Select(command => command.Command));
    }

    [Fact]
    public async Task RemoveCustomCommandAsync_RemovesTheServersCommand()
    {
        CustomCommand otherServers = Command(otherServer, "hello");
        List<CustomCommand> table = commands.HasRows(Command(server, "hello"), otherServers);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveCustomCommandAsync(DiscordIds.Server, "Hello"));
        Assert.Same(otherServers, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveCustomCommandAsync_Unknown_ReturnsNotFound()
    {
        commands.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveCustomCommandAsync(DiscordIds.Server, "hello"));
    }

    //The URL tells the servers' commands apart in the assertions
    private static CustomCommand Command(Server commandServer, string name)
    {
        return new CustomCommand { Command = name, Url = $"{Url}?server={commandServer.ServerId}", ServerId = commandServer.ServerId, Server = commandServer };
    }
}
