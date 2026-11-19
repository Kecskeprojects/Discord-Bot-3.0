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

//The self-roles: the roles users can give themselves in the server's role channel
public sealed class RoleServiceTests
{
    private readonly IRoleRepository roles = Substitute.For<IRoleRepository>();
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly RoleService service;

    private readonly Server server = Entities.Server();
    private readonly Server otherServer = Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer);

    public RoleServiceTests()
    {
        service = new RoleService(roles, servers, TestMapper.Database, logger, new ServerCache());
        servers.HasRows(server, otherServer);
    }

    [Fact]
    public async Task AddSelfRoleAsync_NewRole_AddsItToTheServer()
    {
        List<Role> table = roles.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddSelfRoleAsync(DiscordIds.Server, "Dancer", DiscordIds.Role));
        Role added = Assert.Single(table);
        Assert.Equal(DiscordIds.Role.ToString(), added.DiscordId);
        Assert.Equal("Dancer", added.RoleName);
        Assert.Same(server, added.Server);
    }

    [Fact]
    public async Task AddSelfRoleAsync_SameRoleAgain_ReturnsAlreadyExists()
    {
        roles.HasRows(Entities.Role(DiscordIds.Role, "dancer", server));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddSelfRoleAsync(DiscordIds.Server, "dancer", DiscordIds.Role));
    }

    [Fact]
    public async Task AddSelfRoleAsync_UnknownServer_ReturnsNotFound()
    {
        servers.HasRows();
        roles.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.AddSelfRoleAsync(DiscordIds.Server, "dancer", DiscordIds.Role));
    }

    [Fact]
    public async Task AddSelfRoleAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        roles.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddSelfRoleAsync(DiscordIds.Server, "dancer", DiscordIds.Role));
        logger.Received().Error("RoleService.cs AddSelfRoleAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetRoleAsync_NameInAnotherCase_FindsTheServersRole()
    {
        roles.HasRows(Entities.Role(DiscordIds.OtherRole, "dancer", otherServer), Entities.Role(DiscordIds.Role, "Dancer", server));

        RoleResource? role = await service.GetRoleAsync(DiscordIds.Server, " DANCER ");

        Assert.NotNull(role);
        Assert.Equal(DiscordIds.Role, role.DiscordId);
    }

    [Fact]
    public async Task GetRoleAsync_Unknown_GivesNull()
    {
        roles.HasRows(Entities.Role(DiscordIds.OtherRole, "dancer", otherServer));

        Assert.Null(await service.GetRoleAsync(DiscordIds.Server, "dancer"));
    }

    [Fact]
    public async Task GetServerRolesAsync_GivesOnlyTheServersRoles()
    {
        roles.HasRows(Entities.Role(DiscordIds.Role, "dancer", server), Entities.Role(DiscordIds.OtherRole, "singer", otherServer));

        Assert.Equal(DiscordIds.Role, Assert.Single(await service.GetServerRolesAsync(DiscordIds.Server)).DiscordId);
    }

    [Fact]
    public async Task RemoveSelfRoleAsync_NameInAnotherCase_RemovesTheServersRole()
    {
        Role otherServers = Entities.Role(DiscordIds.OtherRole, "dancer", otherServer);
        List<Role> table = roles.HasRows(Entities.Role(DiscordIds.Role, "Dancer", server), otherServers);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveSelfRoleAsync(DiscordIds.Server, "dancer "));
        Assert.Same(otherServers, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveSelfRoleAsync_Unknown_ReturnsNotFound()
    {
        roles.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveSelfRoleAsync(DiscordIds.Server, "dancer"));
    }
}
