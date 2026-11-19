using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//A server's row, roles and Twitch channels through the real services, repositories and cache on LocalDB
public sealed class ServerSettingsTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerService>().AddServerAsync(DiscordIds.Server));
    }

    [Fact]
    public async Task AddServerAsync_GivesAServerWithoutSettings()
    {
        ServerResource server = await GetServerAsync();

        Assert.Equal(DiscordIds.Server, server.DiscordId);
        Assert.Null(server.RoleMessageDiscordId);
        Assert.Null(server.NotificationRoleDiscordId);
        Assert.Null(server.MuteRoleDiscordId);
        Assert.Empty(server.TwitchChannels);
        Assert.Empty(server.SettingsChannels);
    }

    [Fact]
    public async Task ChangeRoleMessageIdAsync_AfterTheServerWasCached_ShowsTheNewMessage()
    {
        await GetServerAsync();

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerService>().ChangeRoleMessageIdAsync(DiscordIds.Server, DiscordIds.Message));
        Assert.Equal(DiscordIds.Message, (await GetServerAsync()).RoleMessageDiscordId);
    }

    [Fact]
    public async Task AddNotificationRoleAsync_ShowsTheRoleUntilItIsRemoved()
    {
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerService>().AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, "Notified"));

        ServerResource server = await GetServerAsync();
        Assert.Equal(DiscordIds.Role, server.NotificationRoleDiscordId);
        Assert.Equal("notified", server.NotificationRoleName);

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerService>().RemoveNotificationRoleAsync(DiscordIds.Server));
        Assert.Null((await GetServerAsync()).NotificationRoleDiscordId);
    }

    [Fact]
    public async Task AddNotificationRoleAsync_SameRoleInAnotherCase_ReturnsAlreadyExists()
    {
        await host.FromNewScope<IServerService>().AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, "notified");

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await host.FromNewScope<IServerService>().AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, " NOTIFIED "));
    }

    [Fact]
    public async Task ChangeServerMuteRoleAsync_SecondRole_ReplacesTheFirst()
    {
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerService>().ChangeServerMuteRoleAsync(DiscordIds.Server, "muted", DiscordIds.Role));
        Assert.Equal(DbProcessResultEnum.UpdatedExisting,
            await host.FromNewScope<IServerService>().ChangeServerMuteRoleAsync(DiscordIds.Server, "silenced", DiscordIds.OtherRole));

        ServerResource server = await GetServerAsync();
        Assert.Equal(DiscordIds.OtherRole, server.MuteRoleDiscordId);
        Assert.Equal("silenced", server.MuteRoleName);
    }

    [Fact]
    public async Task AddTwitchChannelAsync_ShowsOnTheServerAndInTheMonitorsListWithTheNotificationRole()
    {
        await host.FromNewScope<IServerService>().AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, "notified");
        await GetServerAsync();

        Assert.Equal(DbProcessResultEnum.Success,
            await host.FromNewScope<ITwitchChannelService>().AddTwitchChannelAsync(DiscordIds.Server, "twitch-id", "tester", "https://www.twitch.tv/tester"));

        Assert.Equal("twitch-id", Assert.Single((await GetServerAsync()).TwitchChannels).TwitchId);
        TwitchChannelResource monitored = Assert.Single(await host.FromNewScope<ITwitchChannelService>().GetChannelsAsync());
        Assert.Equal(DiscordIds.Server, monitored.ServerDiscordId);
        Assert.Equal(DiscordIds.Role, monitored.NotificationRoleDiscordId);
    }

    [Fact]
    public async Task RemoveTwitchChannelAsync_ByName_RemovesOnlyThatOne()
    {
        await host.FromNewScope<ITwitchChannelService>().AddTwitchChannelAsync(DiscordIds.Server, "first-id", "first", "https://www.twitch.tv/first");
        await host.FromNewScope<ITwitchChannelService>().AddTwitchChannelAsync(DiscordIds.Server, "second-id", "second", "https://www.twitch.tv/second");

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<ITwitchChannelService>().RemoveTwitchChannelAsync(DiscordIds.Server, "first"));
        Assert.Equal("second-id", Assert.Single((await GetServerAsync()).TwitchChannels).TwitchId);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    //Through the cache, as the commands read it
    private async Task<ServerResource> GetServerAsync()
    {
        ServerResource? server = await host.FromNewScope<IServerService>().GetByDiscordIdAsync(DiscordIds.Server);
        Assert.NotNull(server);
        return server;
    }
}
