using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//Muted users through the real services and repositories on LocalDB: the "still muted" check and the expired mutes
public sealed class MuteTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private const string RemovedRoles = "500001";
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;

    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
    }

    [Fact]
    public async Task AddMutedUserAsync_WhileStillMuted_ReturnsAlreadyExists()
    {
        Assert.Equal(DbProcessResultEnum.Success, await MuteAsync(DiscordIds.Server, DiscordIds.User, now.AddHours(1)));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await MuteAsync(DiscordIds.Server, DiscordIds.User, now.AddHours(2)));
    }

    [Fact]
    public async Task AddMutedUserAsync_SameUserOnAnotherServer_MutesThemThereToo()
    {
        await MuteAsync(DiscordIds.Server, DiscordIds.User, now.AddHours(1));

        Assert.Equal(DbProcessResultEnum.Success, await MuteAsync(DiscordIds.OtherServer, DiscordIds.User, now.AddHours(1)));
    }

    [Fact]
    public async Task GetExpiredMutedUsersAsync_GivesTheMutesThatRanOut()
    {
        await MuteAsync(DiscordIds.Server, DiscordIds.User, now.AddMinutes(-1));
        await MuteAsync(DiscordIds.Server, DiscordIds.OtherUser, now.AddHours(1));

        ServerMutedUserResource expired = Assert.Single(await host.FromNewScope<IServerMutedUserService>().GetExpiredMutedUsersAsync(now));
        Assert.Equal(DiscordIds.User.ToString(), expired.UserDiscordId);
        Assert.Equal(DiscordIds.Server.ToString(), expired.ServerDiscordId);
        Assert.Equal(RemovedRoles, expired.RemovedRoleDiscordIds);
    }

    [Fact]
    public async Task RemoveMutedUserAsync_EndsTheMute()
    {
        await MuteAsync(DiscordIds.Server, DiscordIds.User, now.AddHours(1));
        Assert.NotNull(await host.FromNewScope<IServerMutedUserService>().GetMutedUserByUsernameAsync(DiscordIds.Server, DiscordIds.User));

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerMutedUserService>().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User));
        Assert.Null(await host.FromNewScope<IServerMutedUserService>().GetMutedUserByUsernameAsync(DiscordIds.Server, DiscordIds.User));
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private Task<DbProcessResultEnum> MuteAsync(ulong serverId, ulong userId, DateTime mutedUntil)
    {
        return host.FromNewScope<IServerMutedUserService>().AddMutedUserAsync(serverId, userId, now, mutedUntil, RemovedRoles);
    }
}
