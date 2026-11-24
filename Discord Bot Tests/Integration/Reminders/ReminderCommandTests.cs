using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Discord_Bot.Tools;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Reminders;

//!remind in, list and remove through Discord.Net's command service, in a server channel and in direct messages
public sealed class ReminderCommandTests : IDisposable
{
    //An hour and a half after TestTimes.Now, without its seconds
    private static readonly DateTime inAnHourAndAHalf = new(2026, 11, 18, 15, 35, 0, DateTimeKind.Utc);

    private readonly IReminderService reminderService = Substitute.For<IReminderService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public ReminderCommandTests()
    {
        host = new TestHost().Replace(reminderService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        reminderService.AddReminderAsync(default, default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData("!remind in")]
    [InlineData("!reminder in")]
    //!remind at was a command of its own that took a date until 2024-08-28; it is an alias since
    [InlineData("!remind at")]
    [InlineData("!reminder at")]
    public async Task RemindIn_AmountAndMessage_SavesTheReminderAndSaysWhen(string command)
    {
        CommandResult result = await RunAsync($"{command} 1h 30m>stretch");

        Assert.True(result.IsSuccess, result.ErrorReason);
        await reminderService.Received().AddReminderAsync(DiscordIds.User, inAnHourAndAHalf, "stretch");
        Assert.Equal($"Alright, I will remind you {DiscordTools.UtcTimestamp(inAnHourAndAHalf, TimestampTagStyles.Relative)}.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task RemindIn_InDirectMessages_SavesTheReminder()
    {
        RecordingChannel directMessages = RecordingChannel.DirectMessage();

        await host.RunCommandAsync(DiscordMocks.TextCommand("!remind in 1h 30m>stretch", DiscordMocks.User(), directMessages.Channel));

        await reminderService.Received().AddReminderAsync(DiscordIds.User, inAnHourAndAHalf, "stretch");
        Assert.Single(directMessages.Sent);
    }

    [Fact]
    public async Task RemindIn_ServerKeepsCommandsToAnotherChannel_DoesNothing()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource
        {
            DiscordId = DiscordIds.Server,
            SettingsChannels = new() { [ChannelTypeEnum.CommandText] = [DiscordIds.OtherChannel] }
        });

        await RunAsync("!remind in 1h>stretch");

        await reminderService.DidNotReceiveWithAnyArgs().AddReminderAsync(default, default, default!);
        Assert.Empty(channel.Sent);
    }

    [Fact]
    public async Task RemindIn_MessageWithAnArrow_SavesAllOfIt()
    {
        await RunAsync("!remind in 1h>check if a > b");

        await reminderService.Received().AddReminderAsync(DiscordIds.User, Arg.Any<DateTime>(), "check if a > b");
    }

    [Fact]
    public async Task RemindIn_MessageOverTheColumnsLength_IsRefused()
    {
        await RunAsync($"!remind in 1h>{new string('a', 151)}");

        await reminderService.DidNotReceiveWithAnyArgs().AddReminderAsync(default, default, default!);
        Assert.Equal("Reminder message too long!(maximum **150** characters)", Assert.Single(channel.Sent).Text);
    }


    //It answered nothing until 2026-11-23
    [Fact]
    public async Task RemindIn_WithoutAnArrow_SaysHowToSeparateTheMessage()
    {
        await RunAsync("!remind in 1h stretch");

        await reminderService.DidNotReceiveWithAnyArgs().AddReminderAsync(default, default, default!);
        Assert.Equal("The amount of time and the message have to be separated by `>`, e.g.: `!remind in 1h 30m>stretch`", Assert.Single(channel.Sent).Text);
    }

    //They answered nothing until 2026-11-23, and the next three logged an exception; units are case-sensitive, as M is months.
    //The last three, where a number or unit is missing, answered "Incorrect number of inputs!" until 2026-11-24. A time of day
    //is a likely one, since !remind at is an alias of !remind in
    [Theory]
    [InlineData("in 1x")]
    [InlineData("in 1H")]
    [InlineData("in h1")]
    [InlineData("in 99999999999h")]
    [InlineData("in 10000y")]
    [InlineData("in tomorrow")]
    [InlineData("in 1h 30")]
    [InlineData("at 18:00")]
    public async Task RemindIn_AmountItCantRead_SaysHowToWriteIt(string amount)
    {
        await RunAsync($"!remind {amount}>stretch");

        await reminderService.DidNotReceiveWithAnyArgs().AddReminderAsync(default, default, default!);
        Assert.Equal("The amount of time could not be read! Put y, M, w, d, h or m after each number (`M` is months, `m` is minutes), e.g.: `1h 30m`", Assert.Single(channel.Sent).Text);
        host.Logger.DidNotReceiveWithAnyArgs().Error(default!, default(Exception)!);
    }

    [Fact]
    public async Task RemindIn_DatabaseFails_SaysSo()
    {
        reminderService.AddReminderAsync(default, default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Failure);

        await RunAsync("!remind in 1h>stretch");

        Assert.Equal("Reminder could not be added, talk to dumbass owner!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task RemindList_UserHasReminders_SendsTheList()
    {
        reminderService.GetUserReminderListAsync(DiscordIds.User).Returns([new ReminderResource { Date = inAnHourAndAHalf, Message = "stretch" }]);

        await RunAsync("!remind list");

        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("stretch", Assert.Single(embed.Fields).Value);
    }

    //It answered nothing until 2026-11-23
    [Fact]
    public async Task RemindList_NoReminders_SaysSo()
    {
        reminderService.GetUserReminderListAsync(DiscordIds.User).Returns([]);

        await RunAsync("!remind list");

        Assert.Equal("You do not have any reminders set!", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Reminder in position #2 has been removed.")]
    [InlineData(DbProcessResultEnum.NotFound, "Reminder doesn't exist with that ID or it is not yours.")]
    [InlineData(DbProcessResultEnum.Failure, "Reminder could not be removed!")]
    public async Task RemindRemove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        reminderService.RemoveUserReminderAsync(DiscordIds.User, 2).Returns(outcome);

        await RunAsync("!remind remove 2");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, member, channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
