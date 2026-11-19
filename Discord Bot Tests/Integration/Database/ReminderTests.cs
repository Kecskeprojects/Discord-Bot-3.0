using DatabaseORM.Context;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Microsoft.EntityFrameworkCore;

namespace Discord_Bot.Tests.Integration.Database;

//Reminders through the real services and repositories on LocalDB: the due ones, and a user's list by position
public sealed class ReminderTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;

    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
    }

    [Fact]
    public async Task AddReminderAsync_TwoReminders_ListsThemByDateForOneUser()
    {
        await AddAsync(DiscordIds.User, now.AddHours(2), "second");
        await AddAsync(DiscordIds.User, now.AddHours(1), "first");

        Assert.Equal(["first", "second"], await MessagesAsync(DiscordIds.User));
        await using MainDbContext context = database.NewContext();
        Assert.Equal(1, await context.Users.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RemoveUserReminderAsync_SecondPosition_RemovesTheLaterOne()
    {
        await AddAsync(DiscordIds.User, now.AddHours(1), "first");
        await AddAsync(DiscordIds.User, now.AddHours(2), "second");

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IReminderService>().RemoveUserReminderAsync(DiscordIds.User, 2));
        Assert.Equal(["first"], await MessagesAsync(DiscordIds.User));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task RemoveUserReminderAsync_NoReminderAtThatPosition_ReturnsNotFound(int position)
    {
        await AddAsync(DiscordIds.User, now.AddHours(1), "first");
        await AddAsync(DiscordIds.User, now.AddHours(2), "second");

        Assert.Equal(DbProcessResultEnum.NotFound, await host.FromNewScope<IReminderService>().RemoveUserReminderAsync(DiscordIds.User, position));
        Assert.Equal(2, (await MessagesAsync(DiscordIds.User)).Count);
    }

    [Fact]
    public async Task RemoveUserReminderAsync_CountsOnlyTheUsersOwnReminders()
    {
        await AddAsync(DiscordIds.OtherUser, now.AddMinutes(30), "someone else's");
        await AddAsync(DiscordIds.User, now.AddHours(1), "mine");

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IReminderService>().RemoveUserReminderAsync(DiscordIds.User, 1));
        Assert.Empty(await MessagesAsync(DiscordIds.User));
        Assert.Equal(["someone else's"], await MessagesAsync(DiscordIds.OtherUser));
    }

    [Fact]
    public async Task GetCurrentRemindersAsync_GivesTheDueOnes_WhichRemoveCurrentRemindersAsyncThenRemoves()
    {
        await AddAsync(DiscordIds.User, now.AddMinutes(-1), "due");
        await AddAsync(DiscordIds.OtherUser, now, "due now");
        await AddAsync(DiscordIds.User, now.AddMinutes(1), "later");

        List<ReminderResource> due = await host.FromNewScope<IReminderService>().GetCurrentRemindersAsync(now);
        Assert.Equal(["due", "due now"], due.Select(reminder => reminder.Message).Order(StringComparer.Ordinal));
        Assert.Equal([DiscordIds.User, DiscordIds.OtherUser], due.OrderBy(reminder => reminder.Date).Select(reminder => reminder.UserDiscordId));

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IReminderService>().RemoveCurrentRemindersAsync([.. due.Select(reminder => reminder.ReminderId)]));
        Assert.Equal(["later"], await MessagesAsync(DiscordIds.User));
        Assert.Empty(await MessagesAsync(DiscordIds.OtherUser));
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task AddAsync(ulong userId, DateTime date, string message)
    {
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IReminderService>().AddReminderAsync(userId, date, message));
    }

    private async Task<List<string>> MessagesAsync(ulong userId)
    {
        List<ReminderResource> reminders = await host.FromNewScope<IReminderService>().GetUserReminderListAsync(userId);
        return [.. reminders.Select(reminder => reminder.Message)];
    }
}
