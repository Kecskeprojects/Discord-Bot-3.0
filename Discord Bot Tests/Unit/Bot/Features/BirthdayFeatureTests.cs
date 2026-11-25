using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Net;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ExceptionExtensions;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//Runs once a day, at 06:00 UTC, from App's timer
public sealed class BirthdayFeatureTests
{
    //TestTimes.Now is 2026-11-18
    private static readonly DateOnly birthday = new(2000, 11, 18);

    private readonly IBirthdayService birthdayService = Substitute.For<IBirthdayService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly BirthdayFeature feature;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly RecordingChannel birthdayChannel = RecordingChannel.Text();

    public BirthdayFeatureTests()
    {
        feature = new BirthdayFeature(birthdayService, client, new FakeTimeProvider(TestTimes.Now), serverService, logger);
        client.GetGuildAsync(DiscordIds.Server, CacheMode.CacheOnly).Returns(guild);
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns(birthdayChannel.Channel);
        //Built first: a substitute set up inside Returns(...) would take over the call being configured
        IGuildUser member = DiscordMocks.GuildUser();
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(member);
        ServerWithBirthdayChannel();
    }

    //BirthdayMessageProcessor picks one of several greetings at random; only some of them give the age, but all mention the user
    [Fact]
    public async Task Run_BirthdayToday_GreetsTheUserInTheServersBirthdayChannel()
    {
        Birthdays(Birthday(DiscordIds.User));

        Assert.True(await feature.Run());

        Assert.Contains($"<@{DiscordIds.User}>", Assert.Single(birthdayChannel.Sent).Text, StringComparison.Ordinal);
        await birthdayService.Received().GetBirthdaysByDateAsync(TestTimes.Now.UtcDateTime);
    }

    [Fact]
    public async Task Run_ServerWithoutABirthdayChannel_SendsNothing()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        Birthdays(Birthday(DiscordIds.User));

        Assert.True(await feature.Run());
        Assert.Empty(birthdayChannel.Sent);
    }

    [Fact]
    public async Task Run_UserLeftTheServer_SkipsThemButGreetsTheOthers()
    {
        guild.GetUserAsync(DiscordIds.OtherUser, CacheMode.CacheOnly).Returns((IGuildUser?) null);
        Birthdays(Birthday(DiscordIds.OtherUser), Birthday(DiscordIds.User));

        Assert.True(await feature.Run());

        Assert.Contains($"<@{DiscordIds.User}>", Assert.Single(birthdayChannel.Sent).Text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_BirthdayChannelDeleted_SkipsThatServer()
    {
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns((IChannel?) null);
        Birthdays(Birthday(DiscordIds.User));

        Assert.True(await feature.Run());
        Assert.Empty(birthdayChannel.Sent);
    }

    [Fact]
    public async Task Run_ServerNotLoaded_SkipsIt()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns((ServerResource?) null);
        Birthdays(Birthday(DiscordIds.User));

        Assert.True(await feature.Run());
        Assert.Empty(birthdayChannel.Sent);
    }

    [Fact]
    public async Task Run_OneServersChannelRefuses_StillGreetsTheOtherServers()
    {
        ITextChannel refusing = Substitute.For<ITextChannel>();
        refusing.SendMessageAsync().ThrowsAsyncForAnyArgs(DiscordErrors.MissingPermissions());
        client.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns(refusing);
        IGuild otherGuild = DiscordMocks.Guild(DiscordIds.OtherServer);
        IGuildUser member = DiscordMocks.GuildUser(DiscordIds.User, DiscordIds.OtherServer);
        otherGuild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(member);
        client.GetGuildAsync(DiscordIds.OtherServer, CacheMode.CacheOnly).Returns(otherGuild);
        RecordingChannel otherChannel = RecordingChannel.Text(DiscordIds.OtherChannel);
        client.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns(otherChannel.Channel);
        serverService.GetByDiscordIdAsync(DiscordIds.OtherServer).Returns(new ServerResource
        {
            DiscordId = DiscordIds.OtherServer,
            SettingsChannels = new() { [ChannelTypeEnum.BirthdayText] = [DiscordIds.OtherChannel] }
        });
        Birthdays(Birthday(DiscordIds.User), Birthday(DiscordIds.User, DiscordIds.OtherServer));

        Assert.False(await feature.Run());

        Assert.Single(otherChannel.Sent);
        logger.Received().Error("BirthdayFeature.cs SendBirthdayMessageAsync", Arg.Any<HttpException>());
    }

    private void ServerWithBirthdayChannel()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource
        {
            DiscordId = DiscordIds.Server,
            SettingsChannels = new() { [ChannelTypeEnum.BirthdayText] = [DiscordIds.Channel] }
        });
    }

    private void Birthdays(params List<BirthdayResource> birthdays)
    {
        birthdayService.GetBirthdaysByDateAsync(TestTimes.Now.UtcDateTime).Returns(birthdays);
    }

    private static BirthdayResource Birthday(ulong userId, ulong serverId = DiscordIds.Server)
    {
        return new BirthdayResource { ServerDiscordId = serverId, UserDiscordId = userId, Date = birthday };
    }
}
