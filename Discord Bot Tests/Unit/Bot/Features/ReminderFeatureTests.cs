using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Net;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;
using Discord_Bot.Tools;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ExceptionExtensions;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//Runs every minute from App's timer
public sealed class ReminderFeatureTests
{
    //TestTimes.Now is 14:05:30, so this minute started at 14:05:00
    private static readonly DateTime minuteStart = new(2026, 11, 18, 14, 5, 0, DateTimeKind.Utc);

    private readonly IReminderService reminderService = Substitute.For<IReminderService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly ReminderFeature feature;

    public ReminderFeatureTests()
    {
        feature = new ReminderFeature(reminderService, client, new FakeTimeProvider(TestTimes.Now), Substitute.For<IServerService>(), logger);
        reminderService.RemoveCurrentRemindersAsync(default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
    }

    [Fact]
    public async Task Run_AsksForTheRemindersDueByTheStartOfThisMinute()
    {
        reminderService.GetCurrentRemindersAsync(default).ReturnsForAnyArgs([]);

        Assert.True(await feature.Run());
        await reminderService.Received().GetCurrentRemindersAsync(minuteStart);
    }

    [Fact]
    public async Task Run_DueReminders_SendsEachUserTheirMessageAndRemovesThem()
    {
        RecordingChannel userMessages = DirectMessagesOf(DiscordIds.User);
        RecordingChannel otherUserMessages = DirectMessagesOf(DiscordIds.OtherUser);
        DateTime setFor = minuteStart.AddMinutes(-1);
        DueReminders(Reminder(1, DiscordIds.User, setFor, "drink water"), Reminder(2, DiscordIds.OtherUser, setFor, "stretch"));

        Assert.True(await feature.Run());

        string intro = $"You told me to remind you at {DiscordTools.UtcTimestamp(setFor, TimestampTagStyles.ShortDateTime)} with the following message:\n\n";
        Assert.Equal(intro + "drink water", Assert.Single(userMessages.Sent).Text);
        Assert.Equal(intro + "stretch", Assert.Single(otherUserMessages.Sent).Text);
        await reminderService.Received().RemoveCurrentRemindersAsync(Arg.Is<List<int>>(ids => string.Join(",", ids) == "1,2"));
    }

    [Fact]
    public async Task Run_UserDiscordDoesNotKnow_StillRemovesTheirReminder()
    {
        client.GetUserAsync(DiscordIds.User, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns((IUser?) null);
        DueReminders(Reminder(1, DiscordIds.User, minuteStart, "drink water"));

        Assert.True(await feature.Run());
        await reminderService.Received().RemoveCurrentRemindersAsync(Arg.Is<List<int>>(ids => string.Join(",", ids) == "1"));
    }

    [Fact]
    public async Task Run_NoneDue_RemovesNothing()
    {
        reminderService.GetCurrentRemindersAsync(default).ReturnsForAnyArgs([]);

        Assert.True(await feature.Run());
        await reminderService.DidNotReceiveWithAnyArgs().RemoveCurrentRemindersAsync(default!);
    }

    [Fact]
    public async Task Run_RemovingThemFails_LogsItAndReturnsFalse()
    {
        DirectMessagesOf(DiscordIds.User);
        DueReminders(Reminder(1, DiscordIds.User, minuteStart, "drink water"));
        reminderService.RemoveCurrentRemindersAsync(default!).ReturnsForAnyArgs(DbProcessResultEnum.Failure);

        Assert.False(await feature.Run());
        logger.Received().Error("ReminderFeature.cs ExecuteCoreLogicAsync", "Failure during reminder check!");
    }

    //A refused reminder stays for the next minute, in case Discord's problem was temporary; it no longer holds up the others
    [Fact]
    public async Task Run_OneUsersDirectMessagesRefused_StillSendsTheOthersAndKeepsOnlyTheRefusedOne()
    {
        IUser closed = DiscordMocks.User(DiscordIds.User);
        closed.CreateDMChannelAsync(default).ThrowsAsyncForAnyArgs(DiscordErrors.ClosedDirectMessages());
        client.GetUserAsync(DiscordIds.User, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(closed);
        RecordingChannel otherUserMessages = DirectMessagesOf(DiscordIds.OtherUser);
        DueReminders(Reminder(1, DiscordIds.User, minuteStart, "drink water"), Reminder(2, DiscordIds.OtherUser, minuteStart, "stretch"));

        Assert.False(await feature.Run());

        Assert.Single(otherUserMessages.Sent);
        await reminderService.Received().RemoveCurrentRemindersAsync(Arg.Is<List<int>>(ids => string.Join(",", ids) == "2"));
        logger.Received().Warning("ReminderFeature.cs TrySendAsync", Arg.Any<HttpException>());
    }

    [Fact]
    public async Task Run_EveryReminderRefused_RemovesNone()
    {
        IUser closed = DiscordMocks.User(DiscordIds.User);
        closed.CreateDMChannelAsync(default).ThrowsAsyncForAnyArgs(DiscordErrors.ClosedDirectMessages());
        client.GetUserAsync(DiscordIds.User, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(closed);
        DueReminders(Reminder(1, DiscordIds.User, minuteStart, "drink water"));

        Assert.False(await feature.Run());
        await reminderService.DidNotReceiveWithAnyArgs().RemoveCurrentRemindersAsync(default!);
    }

    private RecordingChannel DirectMessagesOf(ulong userId)
    {
        IUser user = DiscordMocks.User(userId);
        client.GetUserAsync(userId, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(user);
        return RecordingChannel.DirectMessagesOf(user);
    }

    private void DueReminders(params ReminderResource[] reminders)
    {
        reminderService.GetCurrentRemindersAsync(minuteStart).Returns([.. reminders]);
    }

    private static ReminderResource Reminder(int id, ulong userId, DateTime date, string message)
    {
        return new ReminderResource { ReminderId = id, UserDiscordId = userId, Date = date, Message = message };
    }
}
