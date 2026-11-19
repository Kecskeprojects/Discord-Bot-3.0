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

public sealed class ReminderServiceTests
{
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;

    private readonly IReminderRepository reminders = Substitute.For<IReminderRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly ReminderService service;

    private readonly User user = Entities.User();

    public ReminderServiceTests()
    {
        service = new ReminderService(reminders, users, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task AddReminderAsync_KnownUser_AddsItForThem()
    {
        users.HasRows(user);
        List<Reminder> table = reminders.HasRows();

        DbProcessResultEnum result = await service.AddReminderAsync(DiscordIds.User, now.AddHours(1), "test");

        Assert.Equal(DbProcessResultEnum.Success, result);
        Reminder added = Assert.Single(table);
        Assert.Same(user, added.User);
        Assert.Equal(now.AddHours(1), added.Date);
        Assert.Equal("test", added.Message);
    }

    [Fact]
    public async Task AddReminderAsync_NewUser_AddsTheUserWithIt()
    {
        users.HasRows();
        List<Reminder> table = reminders.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddReminderAsync(DiscordIds.User, now.AddHours(1), "test"));
        Assert.Equal(DiscordIds.User.ToString(), Assert.Single(table).User.DiscordId);
    }

    [Fact]
    public async Task AddReminderAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        users.HasRows(user);
        reminders.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddReminderAsync(DiscordIds.User, now, "test"));
        logger.Received().Error("ReminderService.cs AddReminderAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetCurrentRemindersAsync_GivesTheOnesDueByThen()
    {
        reminders.HasRows(Reminder(1, now.AddMinutes(-1), "earlier"), Reminder(2, now, "now"), Reminder(3, now.AddMinutes(1), "later"));

        List<ReminderResource> result = await service.GetCurrentRemindersAsync(now);

        Assert.Equal(["earlier", "now"], result.Select(reminder => reminder.Message));
        Assert.All(result, reminder => Assert.Equal(DiscordIds.User, reminder.UserDiscordId));
    }

    [Fact]
    public async Task GetUserReminderListAsync_GivesTheUsersRemindersByDate()
    {
        Reminder otherUsers = Reminder(3, now.AddMinutes(30), "someone else's");
        otherUsers.User = Entities.User(RowIds.OtherUser, DiscordIds.OtherUser);
        reminders.HasRows(Reminder(1, now.AddHours(2), "second"), otherUsers, Reminder(2, now.AddHours(1), "first"));

        List<ReminderResource> result = await service.GetUserReminderListAsync(DiscordIds.User);

        Assert.Equal(["first", "second"], result.Select(reminder => reminder.Message));
    }

    [Fact]
    public async Task RemoveCurrentRemindersAsync_RemovesTheGivenOnes()
    {
        Reminder kept = Reminder(2, now, "kept");
        List<Reminder> table = reminders.HasRows(Reminder(1, now, "sent"), kept, Reminder(3, now, "also sent"));

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveCurrentRemindersAsync([1, 3]));
        Assert.Same(kept, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveCurrentRemindersAsync_NoneOfThem_RemovesNothingAndSucceeds()
    {
        reminders.HasRows(Reminder(2, now, "kept"));

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveCurrentRemindersAsync([1]));
        await reminders.DidNotReceiveWithAnyArgs().RemoveAsync(default(IEnumerable<Reminder>)!);
    }

    [Fact]
    public async Task RemoveUserReminderAsync_ExistingPosition_RemovesThatReminder()
    {
        Reminder second = Reminder(2, now.AddHours(2), "second");
        List<Reminder> table = reminders.HasRows(Reminder(1, now.AddHours(1), "first"), second);
        reminders.GetByIndexAsync(DiscordIds.User.ToString(), 2).Returns(second);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveUserReminderAsync(DiscordIds.User, 2));
        Assert.DoesNotContain(second, table);
    }

    [Fact]
    public async Task RemoveUserReminderAsync_NoReminderAtThatPosition_ReturnsNotFound()
    {
        reminders.GetByIndexAsync(DiscordIds.User.ToString(), 5).Returns((Reminder?) null);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveUserReminderAsync(DiscordIds.User, 5));
    }

    private Reminder Reminder(int id, DateTime date, string message)
    {
        return new Reminder { ReminderId = id, Date = date, Message = message, UserId = user.UserId, User = user };
    }
}
