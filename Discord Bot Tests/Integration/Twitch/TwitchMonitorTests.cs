using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot.Tests.Integration.Twitch;

//The Twitch monitor's bookkeeping (TwitchAPI): a followed stream that goes online notifies each server following it once, until it
//goes offline again. The tests raise what TwitchLib's monitor would; the notification itself is TwitchNotificationFeatureTests'
public sealed class TwitchMonitorTests : IDisposable
{
    private const string Streamer = "123";
    private const string OtherStreamer = "456";

    private readonly ITwitchChannelService twitchChannelService = Substitute.For<ITwitchChannelService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly TestHost host;
    private readonly TwitchAPI monitor;

    private readonly RecordingChannel firstServer = RecordingChannel.Text(DiscordIds.Channel);
    private readonly RecordingChannel secondServer = RecordingChannel.Text(DiscordIds.OtherChannel);

    public TwitchMonitorTests()
    {
        host = new TestHost().Replace(twitchChannelService).Replace(serverService).Replace(client);
        //One row per server following a Twitch channel
        twitchChannelService.GetChannelsAsync().Returns(
        [
            Row(Streamer, DiscordIds.Server),
            Row(Streamer, DiscordIds.OtherServer),
            Row(OtherStreamer, DiscordIds.Server)
        ]);
        ServerNotifiesIn(DiscordIds.Server, DiscordIds.Channel);
        ServerNotifiesIn(DiscordIds.OtherServer, DiscordIds.OtherChannel);
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns(firstServer.Channel);
        client.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns(secondServer.Channel);
        monitor = host.Services.GetRequiredService<TwitchAPI>();
    }

    //Until 2026-11-23 the stream counted as online after the first server's notification, so the others got none
    [Fact]
    public async Task StreamOnline_NotifiesEveryServerFollowingIt()
    {
        await monitor.GetChannelsAsync();

        await monitor.StreamOnlineAsync(Streamer, "https://example.com/live-{width}x{height}.jpg", "Playing games");

        Assert.Single(firstServer.Sent);
        Assert.Single(secondServer.Sent);
    }

    [Fact]
    public async Task StreamOnline_AgainBeforeGoingOffline_NotifiesNoOneAgain()
    {
        await monitor.GetChannelsAsync();
        await monitor.StreamOnlineAsync(OtherStreamer, "", "First");

        await monitor.StreamOnlineAsync(OtherStreamer, "", "Second");

        Assert.Single(firstServer.Sent);
    }

    [Fact]
    public async Task StreamOnline_AfterGoingOffline_NotifiesAgain()
    {
        await monitor.GetChannelsAsync();
        await monitor.StreamOnlineAsync(OtherStreamer, "", "First");
        monitor.StreamOffline(OtherStreamer);

        await monitor.StreamOnlineAsync(OtherStreamer, "", "Second");

        Assert.Equal(2, firstServer.Sent.Count);
    }

    [Fact]
    public async Task StreamOnline_ChannelNoLongerFollowed_NotifiesNoOne()
    {
        await monitor.GetChannelsAsync();
        twitchChannelService.GetChannelsAsync().Returns([Row(Streamer, DiscordIds.Server)]);

        await monitor.StreamOnlineAsync(OtherStreamer, "", "Playing games");

        Assert.Empty(firstServer.Sent);
    }

    [Fact]
    public async Task StreamOnline_StreamThatWasNeverTracked_NotifiesNoOne()
    {
        await monitor.StreamOnlineAsync(Streamer, "", "Playing games");

        Assert.Empty(firstServer.Sent);
        Assert.Empty(secondServer.Sent);
    }

    private void ServerNotifiesIn(ulong serverId, ulong channelId)
    {
        serverService.GetByDiscordIdAsync(serverId).Returns(new ServerResource
        {
            DiscordId = serverId,
            SettingsChannels = new() { [ChannelTypeEnum.TwitchNotificationText] = [channelId] }
        });
    }

    private static TwitchChannelResource Row(string twitchId, ulong serverId)
    {
        return new TwitchChannelResource { TwitchId = twitchId, ServerDiscordId = serverId, TwitchLink = $"https://www.twitch.tv/{twitchId}" };
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
