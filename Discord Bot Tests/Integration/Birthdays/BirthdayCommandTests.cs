using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Birthdays;

//!birthday add, remove and list, and the admin's !birthday a add and a remove, through Discord.Net's command service
public sealed class BirthdayCommandTests : IDisposable
{
    private static readonly DateTime march4 = new(2000, 3, 4);

    private readonly IBirthdayService birthdayService = Substitute.For<IBirthdayService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly IGuildUser admin = DiscordMocks.GuildUser(DiscordIds.OtherUser, username: "admin");
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public BirthdayCommandTests()
    {
        host = new TestHost().Replace(birthdayService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        birthdayService.AddBirthdayAsync(default, default, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        birthdayService.RemoveBirthdayAsync(default, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);

        member.Nickname.Returns("Ali");
        admin.Nickname.Returns((string?) null);
        admin.GetPermissions(Arg.Any<IGuildChannel>()).Returns(new ChannelPermissions(manageRoles: true));
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(member);
        guild.GetUserAsync(DiscordIds.OtherUser, CacheMode.CacheOnly).Returns(admin);
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    //Constant.DateSeparator lists the separators
    [Theory]
    [InlineData("2000.03.04")]
    [InlineData("2000-3-4")]
    [InlineData("2000/03/04")]
    [InlineData("2000 3 4")]
    [InlineData("2000, 03, 04")]
    public async Task BirthdayAdd_DateWithAnySeparator_SavesIt(string date)
    {
        CommandResult result = await RunAsync($"!birthday add {date}", member);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await birthdayService.Received().AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, march4);
        Assert.Equal("Birthday added to database.", Assert.Single(channel.Sent).Text);
    }

    //The bot's machine decides the culture; the order is year, month, day in any of them
    [Theory]
    [InlineData("hu-HU")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    public async Task BirthdayAdd_UnderAnyCulture_ReadsYearMonthDay(string culture)
    {
        using (new CultureScope(culture))
        {
            await RunAsync("!birthday add 2000.03.04", member);
        }

        await birthdayService.Received().AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, march4);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.UpdatedExisting, "Birthday updated in database.")]
    [InlineData(DbProcessResultEnum.AlreadyExists, "Birthday is the currently set one in database.")]
    [InlineData(DbProcessResultEnum.Failure, "Birthday could not be added to database!")]
    public async Task BirthdayAdd_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        birthdayService.AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, march4).Returns(outcome);

        await RunAsync("!birthday add 2000.03.04", member);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("2000.03", "Incorrect input parameters")]
    [InlineData("2000.13.40", "Date is of an unrecognizable format. The order is 'year month day'.")]
    public async Task BirthdayAdd_WrongDate_SaysWhatIsWrong(string date, string reply)
    {
        await RunAsync($"!birthday add {date}", member);

        await birthdayService.DidNotReceiveWithAnyArgs().AddBirthdayAsync(default, default, default);
        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BirthdayAdd_InDirectMessages_IsRefusedByTheCommandService()
    {
        RecordingChannel directMessages = RecordingChannel.DirectMessage();

        CommandResult result = await host.RunCommandAsync(DiscordMocks.TextCommand("!birthday add 2000.03.04", DiscordMocks.User(), directMessages.Channel));

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        await birthdayService.DidNotReceiveWithAnyArgs().AddBirthdayAsync(default, default, default);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Birthday removed from database.")]
    [InlineData(DbProcessResultEnum.NotFound, "Birthday not found in database.")]
    public async Task BirthdayRemove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        birthdayService.RemoveBirthdayAsync(DiscordIds.Server, DiscordIds.User).Returns(outcome);

        await RunAsync("!birthday remove", member);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BirthdayList_GivesEachMembersNicknameOrUsername()
    {
        birthdayService.GetServerBirthdayListAsync(DiscordIds.Server).Returns([Birthday(DiscordIds.User), Birthday(DiscordIds.OtherUser)]);

        await RunAsync("!birthday list", member);

        Assert.Equal("- **2000.03.04**: Ali\n- **2000.03.04**: admin\n", Assert.Single(Assert.Single(Assert.Single(channel.Sent).Embeds).Fields).Value);
    }

    //Until 2026-11-23 they were shown with the name of whoever asked for the list, as the daily greeting skips them
    [Fact]
    public async Task BirthdayList_UserWhoLeftTheServer_IsLeftOut()
    {
        guild.GetUserAsync(DiscordIds.ThirdUser, CacheMode.CacheOnly).Returns((IGuildUser?) null);
        birthdayService.GetServerBirthdayListAsync(DiscordIds.Server).Returns([Birthday(DiscordIds.ThirdUser), Birthday(DiscordIds.User)]);

        await RunAsync("!birthday list", admin);

        Assert.Equal("- **2000.03.04**: Ali\n", Assert.Single(Assert.Single(Assert.Single(channel.Sent).Embeds).Fields).Value);
    }

    [Fact]
    public async Task BirthdayList_NoBirthdays_SaysSo()
    {
        birthdayService.GetServerBirthdayListAsync(DiscordIds.Server).Returns([]);

        await RunAsync("!birthday list", member);

        Assert.Equal("There are no birthdays set on this server!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BirthdayList_OnlyUsersWhoLeft_SaysThereAreNone()
    {
        guild.GetUserAsync(DiscordIds.ThirdUser, CacheMode.CacheOnly).Returns((IGuildUser?) null);
        birthdayService.GetServerBirthdayListAsync(DiscordIds.Server).Returns([Birthday(DiscordIds.ThirdUser)]);

        await RunAsync("!birthday list", member);

        Assert.Equal("There are no birthdays set on this server!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task AdminBirthdayAdd_UserById_SavesTheirBirthday()
    {
        CommandResult result = await RunAsync($"!birthday a add {DiscordIds.User}>2000.03.04", admin);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await birthdayService.Received().AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, march4);
        Assert.Equal("Birthday added to database.", Assert.Single(channel.Sent).Text);
    }

    //Any Discord account was looked up by ID until 2026-11-24, so a wrong ID was saved, and its greeting skipped every year
    [Fact]
    public async Task AdminBirthdayAdd_IdOfSomeoneNotOnTheServer_SaysSo()
    {
        const ulong stranger = 987654321098765432;
        ICommandContext context = DiscordMocks.TextCommand($"!birthday a add {stranger}>2000.03.04", admin, channel.Channel);
        IUser strangerUser = DiscordMocks.User(stranger);
        context.Client.GetUserAsync(stranger).Returns(strangerUser);
        guild.GetUserAsync(stranger, CacheMode.CacheOnly).Returns((IGuildUser?) null);

        await host.RunCommandAsync(context);

        await birthdayService.DidNotReceiveWithAnyArgs().AddBirthdayAsync(default, default, default);
        Assert.Equal("No user was found with that ID!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task AdminBirthdayAdd_UserByName_SearchesTheServerInLowerCase()
    {
        guild.SearchUsersAsync("tester", 1).Returns([member]);

        await RunAsync("!birthday a add Tester>2000.03.04", admin);

        await birthdayService.Received().AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, march4);
    }

    [Fact]
    public async Task AdminBirthdayAdd_NoSuchUser_SaysSo()
    {
        guild.SearchUsersAsync("nobody", 1).Returns([]);

        await RunAsync("!birthday a add nobody>2000.03.04", admin);

        await birthdayService.DidNotReceiveWithAnyArgs().AddBirthdayAsync(default, default, default);
        Assert.Equal("No user was found with that ID!", Assert.Single(channel.Sent).Text);
    }

    //It threw and answered nothing until 2026-11-23
    [Fact]
    public async Task AdminBirthdayAdd_WithoutAnArrow_SaysTheInputIsWrong()
    {
        await RunAsync($"!birthday a add {DiscordIds.User} 2000.03.04", admin);

        await birthdayService.DidNotReceiveWithAnyArgs().AddBirthdayAsync(default, default, default);
        Assert.Equal("Incorrect input parameters", Assert.Single(channel.Sent).Text);
        host.Logger.DidNotReceiveWithAnyArgs().Error(default!, default(Exception)!);
    }

    [Fact]
    public async Task AdminBirthdayAdd_WithoutManageRoles_IsRefusedByTheCommandService()
    {
        CommandResult result = await RunAsync($"!birthday a add {DiscordIds.User}>2000.03.04", member);

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        await birthdayService.DidNotReceiveWithAnyArgs().AddBirthdayAsync(default, default, default);
    }

    [Fact]
    public async Task AdminBirthdayRemove_RemovesTheUsersBirthday()
    {
        CommandResult result = await RunAsync($"!birthday a remove {DiscordIds.User}", admin);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await birthdayService.Received().RemoveBirthdayAsync(DiscordIds.Server, DiscordIds.User);
        Assert.Equal("Birthday removed from database.", Assert.Single(channel.Sent).Text);
    }

    private static BirthdayResource Birthday(ulong userId)
    {
        return new BirthdayResource { ServerDiscordId = DiscordIds.Server, UserDiscordId = userId, Date = DateOnly.FromDateTime(march4) };
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
