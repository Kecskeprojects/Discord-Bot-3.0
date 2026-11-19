using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//The server's settings channels through the real services, repositories and cache on LocalDB
public sealed class SettingsChannelTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        await database.AddAsync(Entities.Server(RowIds.New), Entities.Server(RowIds.New, DiscordIds.OtherServer));
    }

    [Fact]
    public async Task AddSettingChannelAsync_AfterTheServerWasCached_ShowsInTheServersSettings()
    {
        Assert.Empty((await GetServerAsync()).SettingsChannels);

        DbProcessResultEnum result = await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.Success, result);
        Assert.Equal([DiscordIds.Channel], (await GetServerAsync()).SettingsChannels[ChannelTypeEnum.MusicText]);
    }

    [Fact]
    public async Task AddSettingChannelAsync_TypeTheChannelHas_ReturnsAlreadyExists()
    {
        await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.Channel));
    }

    [Fact]
    public async Task AddSettingChannelAsync_SecondType_KeepsTheFirst()
    {
        await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.Success, await AddAsync(ChannelTypeEnum.CommandText, DiscordIds.Channel));

        ServerResource server = await GetServerAsync();
        Assert.Equal([DiscordIds.Channel], server.SettingsChannels[ChannelTypeEnum.MusicText]);
        Assert.Equal([DiscordIds.Channel], server.SettingsChannels[ChannelTypeEnum.CommandText]);
    }

    //Only one channel per server can be the role channel
    [Fact]
    public async Task AddSettingChannelAsync_RoleChannelOnAnotherChannel_MovesIt()
    {
        await AddAsync(ChannelTypeEnum.RoleText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.Success, await AddAsync(ChannelTypeEnum.RoleText, DiscordIds.OtherChannel));
        Assert.Equal([DiscordIds.OtherChannel], (await GetServerAsync()).SettingsChannels[ChannelTypeEnum.RoleText]);
    }

    [Fact]
    public async Task RemoveSettingChannelAsync_TakesOnlyThatTypeOff()
    {
        await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.Channel);
        await AddAsync(ChannelTypeEnum.CommandText, DiscordIds.Channel);
        await GetServerAsync();

        DbProcessResultEnum result = await host.FromNewScope<IChannelService>().RemoveSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.Success, result);
        ServerResource server = await GetServerAsync();
        Assert.False(server.SettingsChannels.ContainsKey(ChannelTypeEnum.MusicText));
        Assert.Equal([DiscordIds.Channel], server.SettingsChannels[ChannelTypeEnum.CommandText]);
    }

    [Fact]
    public async Task RemoveSettingChannelAsync_TypeTheChannelHasNot_ReturnsNotFound()
    {
        await AddAsync(ChannelTypeEnum.CommandText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.NotFound,
            await host.FromNewScope<IChannelService>().RemoveSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
    }

    [Fact]
    public async Task RemoveSettingChannelsAsync_TakesTheTypeOffEveryChannelOfTheServerOnly()
    {
        await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.Channel);
        await AddAsync(ChannelTypeEnum.MusicText, DiscordIds.OtherChannel);
        Assert.Equal(DbProcessResultEnum.Success,
            await host.FromNewScope<IChannelService>().AddSettingChannelAsync(DiscordIds.OtherServer, ChannelTypeEnum.MusicText, DiscordIds.DirectMessageChannel));

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IChannelService>().RemoveSettingChannelsAsync(DiscordIds.Server, ChannelTypeEnum.MusicText));

        Assert.Empty((await GetServerAsync()).SettingsChannels);
        ServerResource? otherServer = await host.FromNewScope<IServerService>().GetByDiscordIdAsync(DiscordIds.OtherServer);
        Assert.NotNull(otherServer);
        Assert.Equal([DiscordIds.DirectMessageChannel], otherServer.SettingsChannels[ChannelTypeEnum.MusicText]);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private Task<DbProcessResultEnum> AddAsync(ChannelTypeEnum type, ulong channelId)
    {
        return host.FromNewScope<IChannelService>().AddSettingChannelAsync(DiscordIds.Server, type, channelId);
    }

    //Through the cache, as the commands read it
    private async Task<ServerResource> GetServerAsync()
    {
        ServerResource? server = await host.FromNewScope<IServerService>().GetByDiscordIdAsync(DiscordIds.Server);
        Assert.NotNull(server);
        return server;
    }
}
