using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

public sealed class ServerServiceTests
{
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IRoleRepository roles = Substitute.For<IRoleRepository>();
    private readonly IChannelService channelService = Substitute.For<IChannelService>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly ServerCache cache = new();
    private readonly ServerService service;

    public ServerServiceTests()
    {
        service = new ServerService(channelService, servers, roles, TestMapper.Database, logger, cache);
    }

    [Fact]
    public async Task AddServerAsync_AddsTheServerByItsDiscordId()
    {
        List<Server> table = servers.HasRows();

        DbProcessResultEnum result = await service.AddServerAsync(DiscordIds.Server);

        Assert.Equal(DbProcessResultEnum.Success, result);
        Assert.Equal(DiscordIds.Server.ToString(), Assert.Single(table).DiscordId);
    }

    [Fact]
    public async Task AddServerAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        servers.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddServerAsync(DiscordIds.Server));
        logger.Received().Error("ServerService.cs AddServerAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task ChangeRoleMessageIdAsync_SavesItAndDropsTheCachedServer()
    {
        Server server = Entities.Server();
        servers.HasRows(Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer), server);
        CachedServer.Put(cache);

        DbProcessResultEnum result = await service.ChangeRoleMessageIdAsync(DiscordIds.Server, DiscordIds.Message);

        Assert.Equal(DbProcessResultEnum.Success, result);
        Assert.Equal(DiscordIds.Message.ToString(), server.RoleMessageDiscordId);
        await servers.Received().SaveChangesAsync();
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task ChangeRoleMessageIdAsync_UnknownServer_ReturnsNotFoundAndKeepsTheCache()
    {
        servers.HasRows(Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer));
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.ChangeRoleMessageIdAsync(DiscordIds.Server, DiscordIds.Message));
        Assert.True(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task AddNotificationRoleAsync_NewRole_SetsItWithItsTrimmedLowercaseName()
    {
        Server server = Entities.Server();
        servers.HasRows(server);
        roles.HasRows();
        CachedServer.Put(cache);

        DbProcessResultEnum result = await service.AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, " Notified ");

        Assert.Equal(DbProcessResultEnum.Success, result);
        Assert.NotNull(server.NotificationRole);
        Assert.Equal(DiscordIds.Role.ToString(), server.NotificationRole.DiscordId);
        Assert.Equal("notified", server.NotificationRole.RoleName);
        await servers.Received().UpdateAsync(server);
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task AddNotificationRoleAsync_RoleWithThatName_ReusesIt()
    {
        Server server = Entities.Server();
        Role role = Entities.Role(DiscordIds.Role, "notified", server);
        role.RoleId = 5;
        servers.HasRows(server);
        roles.HasRows(role);

        Assert.Equal(DbProcessResultEnum.Success, await service.AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.OtherRole, "Notified"));
        Assert.Same(role, server.NotificationRole);
    }

    [Fact]
    public async Task AddNotificationRoleAsync_ItsCurrentRole_ReturnsAlreadyExists()
    {
        Server server = Entities.Server();
        Role role = Entities.Role(DiscordIds.Role, "notified", server);
        role.RoleId = 5;
        server.NotificationRoleId = 5;
        servers.HasRows(server);
        roles.HasRows(role);

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, "notified"));
        await servers.DidNotReceiveWithAnyArgs().UpdateAsync(default(Server)!);
    }

    [Fact]
    public async Task RemoveNotificationRoleAsync_ClearsItAndDropsTheCachedServer()
    {
        Server server = Entities.Server();
        server.NotificationRoleId = 5;
        servers.HasRows(server);
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveNotificationRoleAsync(DiscordIds.Server));
        Assert.Null(server.NotificationRoleId);
        await servers.Received().SaveChangesAsync();
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task RemoveNotificationRoleAsync_WithoutOne_ReturnsNotFound()
    {
        servers.HasRows(Entities.Server());

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveNotificationRoleAsync(DiscordIds.Server));
    }

    [Fact]
    public async Task GetByDiscordIdAsync_CachedServer_DoesNotAskTheDatabase()
    {
        ServerResource cached = CachedServer.Put(cache);

        Assert.Same(cached, await service.GetByDiscordIdAsync(DiscordIds.Server));
        Assert.Empty(servers.ReceivedCalls());
    }

    [Fact]
    public async Task GetByDiscordIdAsync_FromTheDatabase_AddsTheSettingChannelsAndCachesIt()
    {
        servers.HasRows(Entities.Server());
        Dictionary<ChannelTypeEnum, List<ulong>> channels = new() { [ChannelTypeEnum.MusicText] = [DiscordIds.Channel] };
        channelService.GetServerChannelsAsync(RowIds.Server).Returns(channels);

        ServerResource? server = await service.GetByDiscordIdAsync(DiscordIds.Server);

        Assert.NotNull(server);
        Assert.Equal(DiscordIds.Server, server.DiscordId);
        Assert.Same(channels, server.SettingsChannels);
        Assert.Same(server, cache.TryGetValue(DiscordIds.Server));
    }

    [Fact]
    public async Task GetByDiscordIdAsync_UnknownServer_ReturnsNull()
    {
        servers.HasRows();

        Assert.Null(await service.GetByDiscordIdAsync(DiscordIds.Server));
    }

    [Fact]
    public async Task GetByDiscordIdAsync_DatabaseFails_LogsTheErrorAndReturnsNull()
    {
        servers.Fails();

        Assert.Null(await service.GetByDiscordIdAsync(DiscordIds.Server));
        logger.Received().Error("ServerService.cs GetByDiscordIdAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task ChangeServerMuteRoleAsync_FirstMuteRole_SetsItAndReturnsSuccess()
    {
        Server server = Entities.Server();
        servers.HasRows(server);
        roles.HasRows();
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.Success, await service.ChangeServerMuteRoleAsync(DiscordIds.Server, "Muted", DiscordIds.Role));
        Assert.Equal("muted", server.MuteRole?.RoleName);
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task ChangeServerMuteRoleAsync_ReplacingOne_ReturnsUpdatedExisting()
    {
        Server server = Entities.Server();
        server.MuteRoleId = 5;
        servers.HasRows(server);
        roles.HasRows();

        Assert.Equal(DbProcessResultEnum.UpdatedExisting, await service.ChangeServerMuteRoleAsync(DiscordIds.Server, "muted", DiscordIds.Role));
    }
}
