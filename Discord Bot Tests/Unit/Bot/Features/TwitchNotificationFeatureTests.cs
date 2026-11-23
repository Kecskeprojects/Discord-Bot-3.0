using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//TwitchAPI runs it when a monitored stream goes live, once for each server following the channel
public sealed class TwitchNotificationFeatureTests
{
    private const string Link = "https://www.twitch.tv/tester";

    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly TwitchNotificationFeature feature;

    private readonly RecordingChannel notificationChannel = RecordingChannel.Text();

    public TwitchNotificationFeatureTests()
    {
        feature = new TwitchNotificationFeature(client, serverService, logger);
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns(notificationChannel.Channel);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource
        {
            DiscordId = DiscordIds.Server,
            SettingsChannels = new() { [ChannelTypeEnum.TwitchNotificationText] = [DiscordIds.Channel] }
        });
    }

    [Fact]
    public async Task Run_StreamOnline_PostsItInTheNotificationChannelWithTheRolePing()
    {
        Assert.True(await feature.Run(Data(DiscordIds.Role, "Playing games")));

        SentMessage sent = Assert.Single(notificationChannel.Sent);
        Assert.Equal($"<@&{DiscordIds.Role}>", sent.Text);
        Embed embed = Assert.Single(sent.Embeds);
        Assert.Equal("Stream is now online!", embed.Title);
        Assert.Equal("Playing games", embed.Fields[0].Name);
        Assert.Equal(Link, embed.Fields[0].Value);
        //Twitch gives the thumbnail with {width} and {height} to fill in
        Assert.Equal("https://static-cdn.example.com/live-1024x576.jpg", embed.Image?.Url);
    }

    [Fact]
    public async Task Run_NoNotificationRoleOrTitle_PostsWithoutAPingAndWithNoTitle()
    {
        Assert.True(await feature.Run(Data(null, "")));

        SentMessage sent = Assert.Single(notificationChannel.Sent);
        Assert.Equal("", sent.Text);
        Assert.Equal("No Title", Assert.Single(sent.Embeds).Fields[0].Name);
    }

    [Fact]
    public async Task Run_ServerWithoutANotificationChannel_PostsNothing()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });

        Assert.True(await feature.Run(Data(DiscordIds.Role, "Playing games")));
        Assert.Empty(notificationChannel.Sent);
    }

    [Fact]
    public async Task Run_NotificationChannelDeleted_LogsTheErrorAndReturnsFalse()
    {
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns((IChannel?) null);

        Assert.False(await feature.Run(Data(DiscordIds.Role, "Playing games")));
        logger.Received().Error("TwitchNotificationFeature.cs ExecuteCoreLogicAsync", Arg.Is<string>(message => message.Contains($"{DiscordIds.Channel}", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Run_ServerNotLoaded_WarnsAndReturnsFalse()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns((ServerResource?) null);

        Assert.False(await feature.Run(Data(DiscordIds.Role, "Playing games")));
        logger.Received().Warning("TwitchNotificationFeature.cs ExecuteCoreLogicAsync", Arg.Any<string>());
        Assert.Empty(notificationChannel.Sent);
    }

    private static TwitchNotificationData Data(ulong? roleId, string title)
    {
        return new TwitchNotificationData
        {
            TwitchChannel = new TwitchChannelResource { ServerDiscordId = DiscordIds.Server, TwitchLink = Link, NotificationRoleDiscordId = roleId },
            ThumbnailUrl = "https://static-cdn.example.com/live-{width}x{height}.jpg",
            Title = title
        };
    }
}
