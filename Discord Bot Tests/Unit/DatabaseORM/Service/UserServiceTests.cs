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

public sealed class UserServiceTests
{
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IUserIdolStatisticRepository statistics = Substitute.For<IUserIdolStatisticRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly UserService service;

    private readonly User user = Entities.User();

    public UserServiceTests()
    {
        service = new UserService(users, statistics, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task AddLastfmUsernameAsync_KnownUserWithoutOne_SetsIt()
    {
        users.HasRows(user);

        Assert.Equal(DbProcessResultEnum.Success, await service.AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername));
        Assert.Equal(TestValues.LastFmUsername, user.LastFmusername);
        await users.Received().UpdateAsync(user);
    }

    [Fact]
    public async Task AddLastfmUsernameAsync_NewUser_SavesThemWithIt()
    {
        users.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername));
        await users.Received().UpdateAsync(Arg.Is<User>(saved => saved.DiscordId == DiscordIds.User.ToString() && saved.LastFmusername == TestValues.LastFmUsername));
    }

    [Fact]
    public async Task AddLastfmUsernameAsync_AlreadySet_ReturnsAlreadyExistsAndKeepsIt()
    {
        user.LastFmusername = "first-name";
        users.HasRows(user);

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername));
        Assert.Equal("first-name", user.LastFmusername);
    }

    [Fact]
    public async Task AddLastfmUsernameAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        users.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername));
        logger.Received().Error("UserService.cs AddLastfmUsernameAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetAllLastFmUsersAsync_GivesOnlyUsersWithAUsername()
    {
        user.LastFmusername = TestValues.LastFmUsername;
        User empty = Entities.User(RowIds.OtherUser, DiscordIds.OtherUser);
        empty.LastFmusername = "";
        users.HasRows(user, empty, Entities.User(5, DiscordIds.ThirdUser));

        UserResource found = Assert.Single(await service.GetAllLastFmUsersAsync());
        Assert.Equal(TestValues.LastFmUsername, found.LastFmUsername);
    }

    [Fact]
    public async Task GetTopIdolsAsync_GivesTheGameCountAndTheTopTen()
    {
        user.BiasGameCount = 12;
        users.HasRows(user);
        List<UserIdolStatisticResource> topTen = [new() { IdolStageName = "Stage", Weight = 3.5 }];
        statistics.GetTop10ForUserAsync(RowIds.User, GenderEnum.Female).Returns(topTen);

        UserBiasGameStatResource? result = await service.GetTopIdolsAsync(DiscordIds.User, GenderEnum.Female);

        Assert.NotNull(result);
        Assert.Equal(12, result.BiasGameCount);
        Assert.Same(topTen, result.Stats);
    }

    [Fact]
    public async Task GetTopIdolsAsync_UnknownUser_GivesNull()
    {
        users.HasRows();

        Assert.Null(await service.GetTopIdolsAsync(DiscordIds.User, GenderEnum.Female));
    }

    [Fact]
    public async Task GetUserAsync_KnownUser_GivesThem()
    {
        users.HasRows(Entities.User(RowIds.OtherUser, DiscordIds.OtherUser), user);

        UserResource? found = await service.GetUserAsync(DiscordIds.User);

        Assert.NotNull(found);
        Assert.Equal(RowIds.User, found.UserId);
    }

    [Fact]
    public async Task RemoveLastfmUsernameAsync_ClearsIt()
    {
        user.LastFmusername = TestValues.LastFmUsername;
        users.HasRows(user);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveLastfmUsernameAsync(DiscordIds.User));
        Assert.Null(user.LastFmusername);
        await users.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task RemoveLastfmUsernameAsync_NoneSet_ReturnsNotFound()
    {
        users.HasRows(user);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveLastfmUsernameAsync(DiscordIds.User));
    }
}
