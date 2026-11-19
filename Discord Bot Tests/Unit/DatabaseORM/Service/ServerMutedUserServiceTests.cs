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

public sealed class ServerMutedUserServiceTests
{
    private const string RemovedRoles = "500001";
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;

    private readonly IServerMutedUserRepository mutes = Substitute.For<IServerMutedUserRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly ServerMutedUserService service;

    private readonly Server server = Entities.Server();
    private readonly User user = Entities.User();

    public ServerMutedUserServiceTests()
    {
        service = new ServerMutedUserService(mutes, users, servers, TestMapper.Database, logger, new ServerCache());
        servers.HasRows(server);
        users.HasRows(user);
    }

    [Fact]
    public async Task AddMutedUserAsync_NotMuted_AddsTheMute()
    {
        List<ServerMutedUser> table = mutes.HasRows();

        DbProcessResultEnum result = await service.AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, now, now.AddHours(1), RemovedRoles);

        Assert.Equal(DbProcessResultEnum.Success, result);
        ServerMutedUser added = Assert.Single(table);
        Assert.Equal(now.AddHours(1), added.MutedUntil);
        Assert.Equal(RemovedRoles, added.RemovedRoleDiscordIds);
        Assert.Same(user, added.User);
        Assert.Same(server, added.Server);
    }

    [Fact]
    public async Task AddMutedUserAsync_StillMuted_ReturnsAlreadyExists()
    {
        List<ServerMutedUser> table = mutes.HasRows(Mute(now.AddMinutes(1)));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, now, now.AddHours(1), RemovedRoles));
        Assert.Single(table);
    }

    //UnmuteFeature removes a mute only on its next run, so one that ran out can still be there
    [Fact]
    public async Task AddMutedUserAsync_MuteThatRanOut_AddsANewOne()
    {
        List<ServerMutedUser> table = mutes.HasRows(Mute(now.AddMinutes(-1)));

        Assert.Equal(DbProcessResultEnum.Success, await service.AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, now, now.AddHours(1), RemovedRoles));
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public async Task AddMutedUserAsync_NewUser_AddsTheUserWithTheMute()
    {
        users.HasRows();
        List<ServerMutedUser> table = mutes.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, now, now.AddHours(1), RemovedRoles));
        Assert.Equal(DiscordIds.User.ToString(), Assert.Single(table).User.DiscordId);
    }

    [Fact]
    public async Task AddMutedUserAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        mutes.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, now, now.AddHours(1), RemovedRoles));
        logger.Received().Error("ServerMutedUserService.cs AddMutedUserAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetExpiredMutedUsersAsync_GivesTheOnesThatRanOutByThen()
    {
        mutes.HasRows(Mute(now.AddMinutes(-1)), Mute(now), Mute(now.AddMinutes(1)));

        List<ServerMutedUserResource> result = await service.GetExpiredMutedUsersAsync(now);

        Assert.Equal([now.AddMinutes(-1), now], result.Select(mute => mute.MutedUntil));
        Assert.All(result, mute => Assert.Equal(DiscordIds.User.ToString(), mute.UserDiscordId));
    }

    [Fact]
    public async Task GetMutedUserByUsernameAsync_Muted_GivesTheMute()
    {
        mutes.HasRows(Mute(now.AddHours(1)));

        ServerMutedUserResource? mute = await service.GetMutedUserByUsernameAsync(DiscordIds.Server, DiscordIds.User);

        Assert.NotNull(mute);
        Assert.Equal(DiscordIds.Server.ToString(), mute.ServerDiscordId);
        Assert.Equal(RemovedRoles, mute.RemovedRoleDiscordIds);
    }

    [Fact]
    public async Task GetMutedUserByUsernameAsync_UnknownUser_GivesNull()
    {
        users.HasRows();
        mutes.HasRows(Mute(now.AddHours(1)));

        Assert.Null(await service.GetMutedUserByUsernameAsync(DiscordIds.Server, DiscordIds.User));
    }

    [Fact]
    public async Task RemoveMutedUserAsync_RemovesTheMute()
    {
        List<ServerMutedUser> table = mutes.HasRows(Mute(now.AddHours(1)));

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User));
        Assert.Empty(table);
    }

    [Fact]
    public async Task RemoveMutedUserAsync_NotMuted_ReturnsNotFound()
    {
        mutes.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User));
    }

    private ServerMutedUser Mute(DateTime mutedUntil)
    {
        return new ServerMutedUser
        {
            MutedUntil = mutedUntil,
            RemovedRoleDiscordIds = RemovedRoles,
            ServerId = server.ServerId,
            Server = server,
            UserId = user.UserId,
            User = user
        };
    }
}
