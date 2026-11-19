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

public sealed class TwitchChannelServiceTests
{
    private const string TwitchId = "twitch-id";
    private const string TwitchName = "tester";
    private const string TwitchLink = "https://www.twitch.tv/tester";

    private readonly ITwitchChannelRepository twitchChannels = Substitute.For<ITwitchChannelRepository>();
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly ServerCache cache = new();
    private readonly TwitchChannelService service;

    private readonly Server server = Entities.Server();
    private readonly Server otherServer = Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer);

    public TwitchChannelServiceTests()
    {
        service = new TwitchChannelService(TestMapper.Database, logger, cache, twitchChannels, servers);
        servers.HasRows(server, otherServer);
    }

    [Fact]
    public async Task AddTwitchChannelAsync_NewChannel_AddsItAndDropsTheCachedServer()
    {
        List<TwitchChannel> table = twitchChannels.HasRows();
        CachedServer.Put(cache);

        DbProcessResultEnum result = await service.AddTwitchChannelAsync(DiscordIds.Server, TwitchId, TwitchName, TwitchLink);

        Assert.Equal(DbProcessResultEnum.Success, result);
        TwitchChannel added = Assert.Single(table);
        Assert.Equal(TwitchId, added.TwitchId);
        Assert.Equal(TwitchName, added.TwitchName);
        Assert.Equal(TwitchLink, added.TwitchLink);
        Assert.Same(server, added.Server);
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task AddTwitchChannelAsync_SameTwitchUserAgain_ReturnsAlreadyExists()
    {
        List<TwitchChannel> table = twitchChannels.HasRows(Channel(server, TwitchId, TwitchName));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddTwitchChannelAsync(DiscordIds.Server, TwitchId, TwitchName, TwitchLink));
        Assert.Single(table);
    }

    [Fact]
    public async Task AddTwitchChannelAsync_TwitchUserOfAnotherServer_AddsItHereToo()
    {
        List<TwitchChannel> table = twitchChannels.HasRows(Channel(otherServer, TwitchId, TwitchName));

        Assert.Equal(DbProcessResultEnum.Success, await service.AddTwitchChannelAsync(DiscordIds.Server, TwitchId, TwitchName, TwitchLink));
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public async Task AddTwitchChannelAsync_UnknownServer_ReturnsNotFound()
    {
        servers.HasRows();
        twitchChannels.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.AddTwitchChannelAsync(DiscordIds.Server, TwitchId, TwitchName, TwitchLink));
    }

    [Fact]
    public async Task AddTwitchChannelAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        twitchChannels.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddTwitchChannelAsync(DiscordIds.Server, TwitchId, TwitchName, TwitchLink));
        logger.Received().Error("TwitchChannelService.cs AddTwitchChannelAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetChannelsAsync_GivesEveryChannelWithItsServer()
    {
        twitchChannels.HasRows(Channel(server, TwitchId, TwitchName), Channel(otherServer, "other-id", "other"));

        List<TwitchChannelResource> result = await service.GetChannelsAsync();

        Assert.Equal([DiscordIds.Server, DiscordIds.OtherServer], result.Select(channel => channel.ServerDiscordId));
    }

    [Fact]
    public async Task RemoveTwitchChannelAsync_WithoutAName_RemovesEveryChannelOfTheServer()
    {
        TwitchChannel otherServers = Channel(otherServer, TwitchId, TwitchName);
        List<TwitchChannel> table = twitchChannels.HasRows(Channel(server, TwitchId, TwitchName), Channel(server, "second-id", "second"), otherServers);
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveTwitchChannelAsync(DiscordIds.Server));
        Assert.Same(otherServers, Assert.Single(table));
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task RemoveTwitchChannelAsync_WithAName_RemovesOnlyThatOne()
    {
        TwitchChannel second = Channel(server, "second-id", "second");
        List<TwitchChannel> table = twitchChannels.HasRows(Channel(server, TwitchId, TwitchName), second);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveTwitchChannelAsync(DiscordIds.Server, TwitchName));
        Assert.Same(second, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveTwitchChannelAsync_NoneFound_ReturnsNotFoundAndKeepsTheCache()
    {
        twitchChannels.HasRows(Channel(otherServer, TwitchId, TwitchName));
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveTwitchChannelAsync(DiscordIds.Server));
        Assert.True(CachedServer.IsIn(cache));
    }

    private static TwitchChannel Channel(Server server, string twitchId, string name)
    {
        return new TwitchChannel { TwitchId = twitchId, TwitchName = name, TwitchLink = $"https://www.twitch.tv/{name}", ServerId = server.ServerId, Server = server };
    }
}
