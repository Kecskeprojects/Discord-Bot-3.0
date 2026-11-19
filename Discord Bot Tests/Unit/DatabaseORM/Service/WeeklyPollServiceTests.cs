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
using System.Globalization;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

//Each server's weekly polls; the nameless one is the draft the create form edits
public sealed class WeeklyPollServiceTests
{
    private const int PollId = 21;

    private readonly IWeeklyPollRepository polls = Substitute.For<IWeeklyPollRepository>();
    private readonly IWeeklyPollOptionRepository options = Substitute.For<IWeeklyPollOptionRepository>();
    private readonly IServerRepository servers = Substitute.For<IServerRepository>();
    private readonly IChannelRepository channels = Substitute.For<IChannelRepository>();
    private readonly IRoleRepository roles = Substitute.For<IRoleRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly WeeklyPollService service;

    private readonly Server server = Entities.Server();
    private readonly Server otherServer = Entities.Server(RowIds.OtherServer, DiscordIds.OtherServer);

    public WeeklyPollServiceTests()
    {
        service = new WeeklyPollService(polls, options, servers, channels, roles, TestMapper.Database, logger, new ServerCache());
        servers.HasRows(server, otherServer);
    }

    [Fact]
    public async Task GetOrCreateDummyPollAsync_NoneYet_AddsAnInactiveNamelessOneForTheServer()
    {
        List<WeeklyPoll> table = polls.HasRows(Poll(1, otherServer, ""));

        WeeklyPollEditResource? dummy = await service.GetOrCreateDummyPollAsync(DiscordIds.Server);

        Assert.NotNull(dummy);
        WeeklyPoll added = table.Single(poll => poll.Server == server);
        Assert.Equal("", added.Name);
        Assert.False(added.IsActive);
        Assert.Equal(nameof(DayOfWeek.Monday), added.RepeatOnDayOfWeek);
    }

    [Fact]
    public async Task GetOrCreateDummyPollAsync_DraftThere_GivesIt()
    {
        List<WeeklyPoll> table = polls.HasRows(Poll(PollId, server, ""), Poll(22, server, "weekly"));

        WeeklyPollEditResource? dummy = await service.GetOrCreateDummyPollAsync(DiscordIds.Server);

        Assert.NotNull(dummy);
        Assert.Equal(PollId, dummy.WeeklyPollId);
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public async Task GetOrCreateDummyPollAsync_DatabaseFails_LogsTheErrorAndGivesNull()
    {
        polls.Fails();

        Assert.Null(await service.GetOrCreateDummyPollAsync(DiscordIds.Server));
        logger.Received().Error("WeeklyPollService.cs GetOrCreateDummyPollAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetPollByIdAsync_GivesThePollWithItsOwnOptions()
    {
        polls.HasRows(Poll(22, server, "other"), Poll(PollId, server, "weekly", "First", "Second"));

        WeeklyPollResource? poll = await service.GetPollByIdAsync(PollId);

        Assert.NotNull(poll);
        Assert.Equal("weekly", poll.Name);
        Assert.Equal(["First", "Second"], poll.Options);
    }

    [Fact]
    public async Task GetPollByIdForEditAsync_GivesItsOptionsToEdit()
    {
        polls.HasRows(Poll(PollId, server, "weekly", "First"));

        WeeklyPollEditResource? poll = await service.GetPollByIdForEditAsync(PollId);

        Assert.NotNull(poll);
        Assert.Equal("First", Assert.Single(poll.Options).Title);
    }

    [Fact]
    public async Task GetPollByNameForEditAsync_NameInAnotherCase_FindsTheServersPoll()
    {
        polls.HasRows(Poll(22, otherServer, "weekly"), Poll(PollId, server, "Weekly"));

        WeeklyPollEditResource? poll = await service.GetPollByNameForEditAsync(DiscordIds.Server, "WEEKLY");

        Assert.NotNull(poll);
        Assert.Equal(PollId, poll.WeeklyPollId);
    }

    [Fact]
    public async Task GetPollForForceResendAsync_UsesThePresetsAnswers()
    {
        WeeklyPoll poll = Poll(PollId, server, "weekly");
        poll.OptionPreset = new WeeklyPollOptionPreset { Name = "Yes or No", Description = "" };
        poll.OptionPreset.WeeklyPollOptions.Add(new WeeklyPollOption { OrderNumber = 1, Title = "Yes" });
        poll.OptionPreset.WeeklyPollOptions.Add(new WeeklyPollOption { OrderNumber = 2, Title = "No" });
        polls.HasRows(poll);

        WeeklyPollResource? resent = await service.GetPollForForceResendAsync(DiscordIds.Server, "weekly");

        Assert.NotNull(resent);
        Assert.Equal(DiscordIds.Server, resent.ServerDiscordId);
        Assert.Equal(["Yes", "No"], resent.Options);
    }

    [Fact]
    public async Task GetPollsByDayOfWeekAsync_GivesTheActivePollsOfThatDay()
    {
        WeeklyPoll inactive = Poll(22, server, "inactive");
        inactive.IsActive = false;
        WeeklyPoll tuesday = Poll(23, otherServer, "tuesday");
        tuesday.RepeatOnDayOfWeek = nameof(DayOfWeek.Tuesday);
        polls.HasRows(Poll(PollId, server, "monday"), inactive, tuesday, Poll(24, otherServer, "also monday"));

        List<WeeklyPollResource> due = await service.GetPollsByDayOfWeekAsync(DayOfWeek.Monday);

        Assert.Equal(["monday", "also monday"], due.Select(poll => poll.Name));
    }

    [Fact]
    public async Task GetPollsByServerIdAsync_LeavesOutTheDraftAndOtherServers()
    {
        polls.HasRows(Poll(PollId, server, "weekly"), Poll(22, server, ""), Poll(23, otherServer, "elsewhere"));

        Assert.Equal("weekly", Assert.Single(await service.GetPollsByServerIdAsync(DiscordIds.Server)).Name);
    }

    [Fact]
    public async Task RemovePollByNameAsync_NameInAnotherCase_RemovesTheServersPoll()
    {
        WeeklyPoll otherServers = Poll(22, otherServer, "weekly");
        List<WeeklyPoll> table = polls.HasRows(Poll(PollId, server, "weekly"), otherServers);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemovePollByNameAsync(DiscordIds.Server, "Weekly"));
        Assert.Same(otherServers, Assert.Single(table));
    }

    [Fact]
    public async Task RemovePollByNameAsync_PollWithItsOwnAnswers_RemovesThemWithIt()
    {
        WeeklyPoll poll = Poll(PollId, server, "weekly", "Yes", "No");
        WeeklyPollOption otherPolls = new() { WeeklyPollId = 22, Title = "Kept" };
        List<WeeklyPoll> pollTable = polls.HasRows(poll);
        List<WeeklyPollOption> answerTable = options.HasRows([.. poll.WeeklyPollOptions, otherPolls]);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemovePollByNameAsync(DiscordIds.Server, "weekly"));
        Assert.Empty(pollTable);
        Assert.Same(otherPolls, Assert.Single(answerTable));
    }

    [Fact]
    public async Task RemovePollByNameAsync_Unknown_ReturnsNotFound()
    {
        polls.HasRows(Poll(22, otherServer, "weekly"));

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemovePollByNameAsync(DiscordIds.Server, "weekly"));
    }

    [Fact]
    public async Task UpdateAsync_KnownChannelAndRole_UsesTheirRows()
    {
        WeeklyPoll poll = Poll(PollId, server, "");
        polls.HasRows(poll);
        Channel channel = Entities.Channel(server, DiscordIds.Channel);
        channels.HasRows(channel);
        Role role = Entities.Role(DiscordIds.Role, "voters", server);
        roles.HasRows(role);

        DbProcessResultEnum result = await service.UpdateAsync(PollId, "weekly", "Weekly question", DiscordIds.Channel, new RoleResource { DiscordId = DiscordIds.Role, RoleName = "voters" });

        Assert.Equal(DbProcessResultEnum.Success, result);
        Assert.Equal("weekly", poll.Name);
        Assert.Equal("Weekly question", poll.Title);
        Assert.Same(channel, poll.Channel);
        Assert.Same(role, poll.Role);
        await polls.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsync_NewChannelAndRole_CreatesThemOnThePollsServer()
    {
        WeeklyPoll poll = Poll(PollId, server, "");
        polls.HasRows(poll);
        channels.HasRows();
        roles.HasRows();

        await service.UpdateAsync(PollId, "weekly", "Weekly question", DiscordIds.Channel, new RoleResource { DiscordId = DiscordIds.Role, RoleName = "voters" });

        Assert.NotNull(poll.Channel);
        Assert.Equal(DiscordIds.Channel.ToString(), poll.Channel.DiscordId);
        Assert.Equal(RowIds.Server, poll.Channel.ServerId);
        Assert.NotNull(poll.Role);
        Assert.Equal("voters", poll.Role.RoleName);
        Assert.Equal(RowIds.Server, poll.Role.ServerId);
    }

    [Fact]
    public async Task UpdateAsync_NoRole_ClearsIt()
    {
        WeeklyPoll poll = Poll(PollId, server, "weekly");
        poll.RoleId = 5;
        poll.Role = Entities.Role(DiscordIds.Role, "voters", server);
        polls.HasRows(poll);
        channels.HasRows(Entities.Channel(server, DiscordIds.Channel));

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(PollId, "weekly", "Weekly question", DiscordIds.Channel, null));
        Assert.Null(poll.RoleId);
        Assert.Null(poll.Role);
    }

    [Fact]
    public async Task UpdateAsync_UnknownPoll_ReturnsNotFound()
    {
        polls.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.UpdateAsync(PollId, "weekly", "", DiscordIds.Channel, null));
    }

    //The buttons and selects on the edit message send the field's name and its new value as text
    [Fact]
    public async Task UpdateFieldAsync_PresetId_StoresTheNumber()
    {
        WeeklyPoll poll = Poll(PollId, server, "weekly");
        polls.HasRows(poll);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateFieldAsync(PollId, nameof(WeeklyPoll.OptionPresetId), "4"));
        Assert.Equal(4, poll.OptionPresetId);
        await polls.Received().UpdateAsync(poll);
    }

    //"custom" is the select's choice for the poll's own answers
    [Fact]
    public async Task UpdateFieldAsync_CustomPreset_ClearsThePreset()
    {
        WeeklyPoll poll = Poll(PollId, server, "weekly");
        poll.OptionPresetId = 4;
        polls.HasRows(poll);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateFieldAsync(PollId, nameof(WeeklyPoll.OptionPresetId), "custom"));
        Assert.Null(poll.OptionPresetId);
    }

    [Theory]
    [InlineData(nameof(WeeklyPoll.IsPinned), "True")]
    [InlineData(nameof(WeeklyPoll.RepeatOnDayOfWeek), "Friday")]
    [InlineData(nameof(WeeklyPoll.CloseInTimeSpanTicks), "864000000000")]
    public async Task UpdateFieldAsync_OtherFields_ConvertTheTextToTheirType(string field, string value)
    {
        WeeklyPoll poll = Poll(PollId, server, "weekly");
        polls.HasRows(poll);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateFieldAsync(PollId, field, value));
        Assert.Equal(value, Convert.ToString(typeof(WeeklyPoll).GetProperty(field)!.GetValue(poll), CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task UpdateFieldAsync_UnknownField_LogsTheErrorAndReturnsFailure()
    {
        polls.HasRows(Poll(PollId, server, "weekly"));

        Assert.Equal(DbProcessResultEnum.Failure, await service.UpdateFieldAsync(PollId, "Colour", "red"));
        logger.Received().Error("WeeklyPollService.cs UpdateFieldAsync", Arg.Any<Exception>());
    }

    //An active Monday poll with its own answers
    private static WeeklyPoll Poll(int id, Server pollServer, string name, params string[] answers)
    {
        WeeklyPoll poll = new()
        {
            WeeklyPollId = id,
            Name = name,
            Title = "",
            RepeatOnDayOfWeek = nameof(DayOfWeek.Monday),
            IsActive = true,
            ServerId = pollServer.ServerId,
            Server = pollServer
        };
        foreach ((string answer, int index) in answers.Select((answer, index) => (answer, index)))
        {
            poll.WeeklyPollOptions.Add(new WeeklyPollOption { WeeklyPollId = id, OrderNumber = (byte) (index + 1), Title = answer });
        }
        return poll;
    }
}
