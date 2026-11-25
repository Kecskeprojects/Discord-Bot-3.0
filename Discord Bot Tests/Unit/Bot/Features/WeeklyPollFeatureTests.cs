using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Net;
using Discord_Bot.Core;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ExceptionExtensions;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//Runs once a day, at 06:00 UTC, from App's timer
public sealed class WeeklyPollFeatureTests
{
    private readonly IWeeklyPollService pollService = Substitute.For<IWeeklyPollService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly WeeklyPollFeature feature;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly RecordingChannel pollChannel = RecordingChannel.Text();

    public WeeklyPollFeatureTests()
    {
        feature = new WeeklyPollFeature(pollService, client, new FakeTimeProvider(TestTimes.Now), Substitute.For<IServerService>(), TestMapper.Create(new MapperConfig()), logger);
        client.GetGuildAsync(DiscordIds.Server, CacheMode.CacheOnly).Returns(guild);
        guild.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns((IGuildChannel) pollChannel.Channel);
        IRole role = Substitute.For<IRole>();
        role.Id.Returns(DiscordIds.Role);
        guild.GetRole(DiscordIds.Role).Returns(role);
    }

    //TestTimes.Now is a Wednesday
    [Fact]
    public async Task Run_AsksForTodaysPolls()
    {
        pollService.GetPollsByDayOfWeekAsync(default).ReturnsForAnyArgs([]);

        Assert.True(await feature.Run());
        await pollService.Received().GetPollsByDayOfWeekAsync(DayOfWeek.Wednesday);
    }

    [Fact]
    public async Task Run_TodaysPoll_PostsItWithTheRoleMention()
    {
        TodaysPolls(Poll());

        Assert.True(await feature.Run());

        SentMessage sent = Assert.Single(pollChannel.Sent);
        Assert.Equal($"<@&{DiscordIds.Role}>", sent.Text);
        Assert.NotNull(sent.Poll);
        Assert.Equal("Weekly question", sent.Poll.Question.Text);
        Assert.Equal(["Yes", "No"], sent.Poll.Answers.Select(answer => answer.Text));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Run_TodaysPoll_IsPinnedOnlyIfSetTo(bool isPinned)
    {
        WeeklyPollResource poll = Poll();
        poll.IsPinned = isPinned;
        TodaysPolls(poll);

        await feature.Run();

        await Assert.Single(pollChannel.Sent).Message.Received(isPinned ? 1 : 0).PinAsync(Arg.Any<RequestOptions>());
    }

    [Fact]
    public async Task Run_PollWithoutARole_MentionsNobody()
    {
        WeeklyPollResource poll = Poll();
        poll.RoleDiscordId = null;
        TodaysPolls(poll);

        await feature.Run();

        Assert.Equal("", Assert.Single(pollChannel.Sent).Text);
    }

    [Fact]
    public async Task Run_PollWithoutAnswers_IsNotPosted()
    {
        WeeklyPollResource poll = Poll();
        poll.Options = [];
        TodaysPolls(poll);

        Assert.True(await feature.Run());
        Assert.Empty(pollChannel.Sent);
    }

    [Fact]
    public async Task Run_PollChannelDeleted_PostsNothing()
    {
        guild.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns((IGuildChannel?) null);
        TodaysPolls(Poll());

        Assert.True(await feature.Run());
        Assert.Empty(pollChannel.Sent);
    }

    [Fact]
    public async Task Run_OneChannelRefuses_StillPostsTheOtherPolls()
    {
        ITextChannel refusing = Substitute.For<ITextChannel>();
        refusing.SendMessageAsync().ThrowsAsyncForAnyArgs(DiscordErrors.MissingPermissions());
        guild.GetChannelAsync(DiscordIds.Channel, CacheMode.CacheOnly).Returns(refusing);
        RecordingChannel otherChannel = RecordingChannel.Text(DiscordIds.OtherChannel);
        guild.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns((IGuildChannel) otherChannel.Channel);
        WeeklyPollResource other = Poll();
        other.ChannelDiscordId = DiscordIds.OtherChannel;
        TodaysPolls(Poll(), other);

        Assert.False(await feature.Run());

        Assert.Single(otherChannel.Sent);
        logger.Received().Error("WeeklyPollFeature.cs ProcessWeeklyPollAsync", Arg.Any<HttpException>());
    }

    private void TodaysPolls(params List<WeeklyPollResource> polls)
    {
        pollService.GetPollsByDayOfWeekAsync(DayOfWeek.Wednesday).Returns(polls);
    }

    private static WeeklyPollResource Poll()
    {
        return new WeeklyPollResource
        {
            WeeklyPollId = 21,
            ServerDiscordId = DiscordIds.Server,
            ChannelDiscordId = DiscordIds.Channel,
            RoleDiscordId = DiscordIds.Role,
            Title = "Weekly question",
            Options = ["Yes", "No"],
            CloseInTimeSpanTicks = TimeSpan.FromDays(1).Ticks
        };
    }
}
