using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Polls;

//!weekly poll create/edit/remove/list/resend through Discord.Net's command service, sent by a server administrator
public sealed class WeeklyPollCommandTests : IDisposable
{
    private readonly IWeeklyPollService pollService = Substitute.For<IWeeklyPollService>();
    private readonly IWeeklyPollOptionPresetService presetService = Substitute.For<IWeeklyPollOptionPresetService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser admin = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public WeeklyPollCommandTests()
    {
        host = new TestHost().Replace(pollService).Replace(presetService).Replace(serverService).Replace(client);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        presetService.GetActivePresetsAsync().Returns([]);
        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData("!weekly poll create")]
    [InlineData("!wp c")]
    public async Task Create_ShowsTheEditMessageForTheNewPoll(string command)
    {
        pollService.GetOrCreateDummyPollAsync(DiscordIds.Server).Returns(new WeeklyPollEditResource { WeeklyPollId = 7 });

        CommandResult result = await RunAsync(command, admin);

        Assert.True(result.IsSuccess, result.ErrorReason);
        SentMessage sent = Assert.Single(channel.Sent);
        Assert.Single(sent.Embeds);
        Assert.NotNull(sent.Components);
    }

    [Fact]
    public async Task Create_PollCouldNotBeMade_SaysSo()
    {
        pollService.GetOrCreateDummyPollAsync(DiscordIds.Server).Returns((WeeklyPollEditResource?) null);

        await RunAsync("!weekly poll create", admin);

        Assert.Equal("The poll could not be created.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Edit_PollByName_ShowsItsEditMessage()
    {
        pollService.GetPollByNameForEditAsync(DiscordIds.Server, "weekly").Returns(new WeeklyPollEditResource { WeeklyPollId = 7, Name = "weekly" });

        await RunAsync("!weekly poll edit weekly", admin);

        Assert.Single(Assert.Single(channel.Sent).Embeds);
    }

    [Theory]
    [InlineData("!weekly poll edit nothing")]
    [InlineData("!weekly poll resend nothing")]
    public async Task EditOrResend_UnknownPoll_SaysSo(string command)
    {
        await RunAsync(command, admin);

        Assert.Equal("Poll not found!", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "The 'weekly' poll has been removed.")]
    [InlineData(DbProcessResultEnum.NotFound, "Poll does not exist.")]
    [InlineData(DbProcessResultEnum.Failure, "Poll could not be removed!")]
    public async Task Remove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        pollService.RemovePollByNameAsync(DiscordIds.Server, "weekly").Returns(outcome);

        await RunAsync("!weekly poll remove weekly", admin);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task List_SendsTheServersPolls()
    {
        pollService.GetPollsByServerIdAsync(DiscordIds.Server).Returns([new WeeklyPollResource { Name = "weekly", Title = "Best song?" }]);

        await RunAsync("!wp l", admin);

        Assert.Single(Assert.Single(channel.Sent).Embeds);
    }

    [Fact]
    public async Task Resend_PostsThePollInItsChannelNow()
    {
        RecordingChannel pollChannel = RecordingChannel.Text(DiscordIds.OtherChannel);
        client.GetGuildAsync(DiscordIds.Server, CacheMode.CacheOnly).Returns(guild);
        guild.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns((ITextChannel) pollChannel.Channel);
        pollService.GetPollForForceResendAsync(DiscordIds.Server, "weekly").Returns(Poll(["first", "second"]));

        await RunAsync("!weekly poll resend weekly", admin);

        Assert.Equal("Best song?", Assert.Single(pollChannel.Sent).Poll?.Question.Text);
    }

    //It answered nothing until 2026-11-23
    [Fact]
    public async Task Resend_PollWithoutAnswers_SaysSo()
    {
        pollService.GetPollForForceResendAsync(DiscordIds.Server, "weekly").Returns(Poll([]));

        await RunAsync("!weekly poll resend weekly", admin);

        Assert.Equal("The poll has no answers to send yet, add them with `!weekly poll edit`.", Assert.Single(channel.Sent).Text);
    }

    //Until 2026-11-23 "Create Polls" in the channel was enough, which Discord gives everyone by default
    [Fact]
    public async Task Commands_MemberWhoCanCreatePollsButIsNoAdministrator_IsRefusedByTheCommandService()
    {
        IGuildUser member = DiscordMocks.GuildUser(DiscordIds.OtherUser);
        member.GetPermissions(Arg.Any<IGuildChannel>()).Returns(new ChannelPermissions((ulong) ChannelPermission.SendPolls));

        CommandResult result = await RunAsync("!weekly poll remove weekly", member);

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        await pollService.DidNotReceiveWithAnyArgs().RemovePollByNameAsync(default, default!);
    }

    private static WeeklyPollResource Poll(List<string> options)
    {
        return new WeeklyPollResource
        {
            Name = "weekly",
            Title = "Best song?",
            ServerDiscordId = DiscordIds.Server,
            ChannelDiscordId = DiscordIds.OtherChannel,
            CloseInTimeSpanTicks = TimeSpan.FromDays(1).Ticks,
            Options = options
        };
    }

    private Task<CommandResult> RunAsync(string text, IGuildUser author)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, author, channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
