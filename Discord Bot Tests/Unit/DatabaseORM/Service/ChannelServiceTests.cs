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

public sealed class ChannelServiceTests
{
    private readonly IChannelRepository channels = Substitute.For<IChannelRepository>();
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IChannelTypeRepository channelTypes = Substitute.For<IChannelTypeRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly ServerCache cache = new();
    private readonly ChannelService service;

    private readonly Server server = Entities.Server();
    private readonly ChannelType music = Entities.ChannelType(ChannelTypeEnum.MusicText);
    private readonly ChannelType command = Entities.ChannelType(ChannelTypeEnum.CommandText);
    //Only one channel per server can have it
    private readonly ChannelType role = Entities.ChannelType(ChannelTypeEnum.RoleText);

    public ChannelServiceTests()
    {
        service = new ChannelService(channels, servers, channelTypes, TestMapper.Database, logger, cache);
        servers.HasRows(server);
        channelTypes.HasRows(music, command, role);
    }

    [Fact]
    public async Task AddSettingChannelAsync_NewChannel_AddsItWithTheTypeAndDropsTheCachedServer()
    {
        List<Channel> table = channels.HasRows();
        CachedServer.Put(cache);

        DbProcessResultEnum result = await service.AddSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel);

        Assert.Equal(DbProcessResultEnum.Success, result);
        Channel added = Assert.Single(table);
        Assert.Equal(DiscordIds.Channel.ToString(), added.DiscordId);
        Assert.Same(server, added.Server);
        Assert.Same(music, Assert.Single(added.ChannelTypes));
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task AddSettingChannelAsync_ChannelWithAnotherType_AddsThisOne()
    {
        Channel channel = Entities.Channel(server, DiscordIds.Channel, music);
        channels.HasRows(channel);

        Assert.Equal(DbProcessResultEnum.Success, await service.AddSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.CommandText, DiscordIds.Channel));
        Assert.Equal([music, command], channel.ChannelTypes);
        await channels.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task AddSettingChannelAsync_TypeTheChannelHas_ReturnsAlreadyExists()
    {
        channels.HasRows(Entities.Channel(server, DiscordIds.Channel, music));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
        await channels.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task AddSettingChannelAsync_OneChannelType_TakesItOffTheServersOtherChannel()
    {
        Channel other = Entities.Channel(server, DiscordIds.OtherChannel, role, music);
        channels.HasRows(other);

        Assert.Equal(DbProcessResultEnum.Success, await service.AddSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.RoleText, DiscordIds.Channel));
        Assert.Same(music, Assert.Single(other.ChannelTypes));
    }

    [Fact]
    public async Task AddSettingChannelAsync_UnknownChannelType_ReturnsNotFound()
    {
        channelTypes.HasRows();
        channels.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.AddSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
    }

    [Fact]
    public async Task AddSettingChannelAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        channels.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
        logger.Received().Error("ChannelService.cs AddSettingChannelAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task RemoveSettingChannelAsync_TakesTheTypeOffAndDropsTheCachedServer()
    {
        Channel channel = Entities.Channel(server, DiscordIds.Channel, music, command);
        channels.HasRows(channel);
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
        Assert.Same(command, Assert.Single(channel.ChannelTypes));
        await channels.Received().SaveChangesAsync();
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task RemoveSettingChannelAsync_ChannelWithoutThatType_ReturnsNotFoundAndKeepsTheCache()
    {
        channels.HasRows(Entities.Channel(server, DiscordIds.Channel, command));
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
        Assert.True(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task RemoveSettingChannelAsync_UnknownChannel_ReturnsNotFound()
    {
        channels.HasRows(Entities.Channel(server, DiscordIds.OtherChannel, music));

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.Channel));
    }

    [Fact]
    public async Task RemoveSettingChannelsAsync_TakesTheTypeOffEveryChannelOfTheServer()
    {
        Channel first = Entities.Channel(server, DiscordIds.Channel, music);
        Channel second = Entities.Channel(server, DiscordIds.OtherChannel, music, command);
        Channel otherServers = Entities.Channel(Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer), DiscordIds.Channel, music);
        channels.HasRows(first, second, otherServers);
        CachedServer.Put(cache);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveSettingChannelsAsync(DiscordIds.Server, ChannelTypeEnum.MusicText));
        Assert.Empty(first.ChannelTypes);
        Assert.Same(command, Assert.Single(second.ChannelTypes));
        Assert.Same(music, Assert.Single(otherServers.ChannelTypes));
        Assert.False(CachedServer.IsIn(cache));
    }

    [Fact]
    public async Task RemoveSettingChannelsAsync_NoChannelOfThatType_ReturnsNotFound()
    {
        channels.HasRows(Entities.Channel(server, DiscordIds.Channel, command));

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveSettingChannelsAsync(DiscordIds.Server, ChannelTypeEnum.MusicText));
    }

    [Fact]
    public async Task GetServerChannelsAsync_GroupsTheChannelIdsByType()
    {
        List<ServerChannelResource> rows =
        [
            new() { ChannelTypeId = (int) ChannelTypeEnum.MusicText, ChannelDiscordId = DiscordIds.Channel.ToString() },
            new() { ChannelTypeId = (int) ChannelTypeEnum.RoleText, ChannelDiscordId = DiscordIds.Channel.ToString() },
            new() { ChannelTypeId = (int) ChannelTypeEnum.MusicText, ChannelDiscordId = DiscordIds.OtherChannel.ToString() }
        ];
        channels.GetServerChannelsAsync(RowIds.Server).Returns(rows);

        Dictionary<ChannelTypeEnum, List<ulong>> result = await service.GetServerChannelsAsync(RowIds.Server);

        Assert.Equal([DiscordIds.Channel, DiscordIds.OtherChannel], result[ChannelTypeEnum.MusicText]);
        Assert.Equal([DiscordIds.Channel], result[ChannelTypeEnum.RoleText]);
        Assert.Equal(2, result.Count);
    }
}
