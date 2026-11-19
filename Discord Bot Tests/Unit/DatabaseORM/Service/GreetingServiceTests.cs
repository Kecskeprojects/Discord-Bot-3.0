using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

public sealed class GreetingServiceTests
{
    private const string Url = "https://example.com/hello.gif";

    private readonly IGreetingRepository greetings = Substitute.For<IGreetingRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly GreetingService service;

    public GreetingServiceTests()
    {
        service = new GreetingService(greetings, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task GetAllGreetingAsync_GivesEveryGreeting()
    {
        greetings.HasRows(new Greeting { GreetingId = 1, Url = Url }, new Greeting { GreetingId = 2, Url = "https://example.com/hi.gif" });

        List<GreetingResource> result = await service.GetAllGreetingAsync();

        Assert.Equal([1, 2], result.Select(greeting => greeting.GreetingId));
    }

    [Fact]
    public async Task GetAllGreetingAsync_DatabaseFails_LogsTheErrorAndGivesNone()
    {
        greetings.Fails();

        Assert.Empty(await service.GetAllGreetingAsync());
        logger.Received().Error("GreetingService.cs GetAllGreetingAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task AddGreetingAsync_AddsTheUrl()
    {
        List<Greeting> table = greetings.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddGreetingAsync(Url));
        Assert.Equal(Url, Assert.Single(table).Url);
    }

    [Fact]
    public async Task RemoveGreetingAsync_KnownId_RemovesIt()
    {
        Greeting greeting = new() { GreetingId = 5, Url = Url };
        List<Greeting> table = greetings.HasRows(greeting);
        greetings.FindByIdAsync(5).Returns(greeting);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveGreetingAsync(5));
        Assert.Empty(table);
    }

    [Fact]
    public async Task RemoveGreetingAsync_UnknownId_ReturnsNotFound()
    {
        greetings.FindByIdAsync(5).Returns((Greeting?) null);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveGreetingAsync(5));
    }
}
