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

public sealed class BirthdayServiceTests
{
    private static readonly DateOnly birthday = new(2000, 3, 4);

    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IBirthdayRepository birthdays = Substitute.For<IBirthdayRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly BirthdayService service;

    private readonly Server server = Entities.Server();
    private readonly User user = Entities.User();

    public BirthdayServiceTests()
    {
        service = new BirthdayService(servers, users, birthdays, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task AddBirthdayAsync_New_AddsTheDayForTheUserOnTheServer()
    {
        servers.HasRows(server);
        users.HasRows(user);
        List<Birthday> table = birthdays.HasRows();

        DbProcessResultEnum result = await service.AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 3, 4, 15, 30, 0));

        Assert.Equal(DbProcessResultEnum.Success, result);
        Birthday added = Assert.Single(table);
        Assert.Equal(birthday, added.Date);
        Assert.Same(user, added.User);
        Assert.Same(server, added.Server);
    }

    [Fact]
    public async Task AddBirthdayAsync_NewUserAndServer_AddsThemWithIt()
    {
        servers.HasRows();
        users.HasRows();
        List<Birthday> table = birthdays.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, birthday.ToDateTime(TimeOnly.MinValue)));
        Birthday added = Assert.Single(table);
        Assert.Equal(DiscordIds.User.ToString(), added.User.DiscordId);
        Assert.Equal(DiscordIds.Server.ToString(), added.Server.DiscordId);
    }

    [Fact]
    public async Task AddBirthdayAsync_SameDayAgain_ReturnsAlreadyExists()
    {
        birthdays.HasRows(Birthday(server, birthday));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 3, 4, 8, 0, 0)));
        await birthdays.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task AddBirthdayAsync_AnotherDay_ChangesIt()
    {
        Birthday existing = Birthday(server, birthday);
        birthdays.HasRows(existing);

        Assert.Equal(DbProcessResultEnum.UpdatedExisting, await service.AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 3, 5)));
        Assert.Equal(new DateOnly(2000, 3, 5), existing.Date);
        await birthdays.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task AddBirthdayAsync_SetOnAnotherServer_AddsOneHereToo()
    {
        servers.HasRows(server);
        users.HasRows(user);
        List<Birthday> table = birthdays.HasRows(Birthday(Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer), birthday));

        Assert.Equal(DbProcessResultEnum.Success, await service.AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, birthday.ToDateTime(TimeOnly.MinValue)));
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public async Task GetBirthdaysByDateAsync_MatchesTheMonthAndDayOfAnyYear()
    {
        //TestTimes.Now is November 18
        birthdays.HasRows(
            Birthday(server, new DateOnly(2000, 11, 18)),
            Birthday(server, new DateOnly(1995, 11, 18)),
            Birthday(server, new DateOnly(2000, 11, 19)),
            Birthday(server, new DateOnly(2000, 12, 18)));

        List<BirthdayResource> result = await service.GetBirthdaysByDateAsync(TestTimes.Now.UtcDateTime);

        Assert.Equal([new DateOnly(2000, 11, 18), new DateOnly(1995, 11, 18)], result.Select(found => found.Date));
    }

    [Fact]
    public async Task GetBirthdaysByDateAsync_DatabaseFails_LogsTheErrorAndGivesNone()
    {
        birthdays.Fails();

        Assert.Empty(await service.GetBirthdaysByDateAsync(TestTimes.Now.UtcDateTime));
        logger.Received().Error("BirthdayService.cs GetBirthdaysByDateAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetServerBirthdayListAsync_GivesTheServersList()
    {
        birthdays.GetListForServerAsync(DiscordIds.Server.ToString()).Returns([Birthday(server, birthday)]);

        BirthdayResource found = Assert.Single(await service.GetServerBirthdayListAsync(DiscordIds.Server));
        Assert.Equal(DiscordIds.User, found.UserDiscordId);
        Assert.Equal(birthday, found.Date);
    }

    [Fact]
    public async Task RemoveBirthdayAsync_RemovesTheUsersBirthdayOnTheServer()
    {
        Birthday otherServers = Birthday(Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer), birthday);
        List<Birthday> table = birthdays.HasRows(Birthday(server, birthday), otherServers);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveBirthdayAsync(DiscordIds.Server, DiscordIds.User));
        Assert.Same(otherServers, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveBirthdayAsync_NoneSet_ReturnsNotFound()
    {
        birthdays.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveBirthdayAsync(DiscordIds.Server, DiscordIds.User));
    }

    private Birthday Birthday(Server birthdayServer, DateOnly date)
    {
        return new Birthday { Date = date, ServerId = birthdayServer.ServerId, Server = birthdayServer, UserId = user.UserId, User = user };
    }
}
