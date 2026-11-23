using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Discord_Bot.Tools;
using NSubstitute.ExceptionExtensions;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Mute;

//!mute and !unmute through Discord.Net's command service, sent by an administrator in a server channel
public sealed class MuteCommandTests : IDisposable
{
    private const ulong MuteRole = DiscordIds.OtherRole;
    private const ulong BotRole = 500_003;
    private const ulong RoleAboveTheBot = 500_004;
    //An hour after TestTimes.Now, without its seconds
    private static readonly DateTime inAnHour = new(2026, 11, 18, 15, 5, 0, DateTimeKind.Utc);

    private readonly IServerMutedUserService mutedUserService = Substitute.For<IServerMutedUserService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser admin = DiscordMocks.GuildUser(DiscordIds.OtherUser);
    private readonly IGuildUser user = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();
    private readonly RecordingChannel userMessages;

    public MuteCommandTests()
    {
        host = new TestHost().Replace(mutedUserService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server, MuteRoleDiscordId = MuteRole });
        mutedUserService.AddMutedUserAsync(default, default, default, default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        mutedUserService.RemoveMutedUserAsync(default, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);

        //Built before they go into Returns, so their own setups don't take over the call being configured
        IRole everyone = Role(DiscordIds.Server, 0);
        IRole[] roles = [everyone, Role(DiscordIds.Role, 1), Role(MuteRole, 2), Role(BotRole, 5), Role(RoleAboveTheBot, 9)];
        guild.EveryoneRole.Returns(everyone);
        guild.Roles.Returns(roles);

        IGuildUser bot = DiscordMocks.GuildUser(DiscordIds.Bot);
        bot.RoleIds.Returns([BotRole]);
        guild.GetCurrentUserAsync(CacheMode.CacheOnly).Returns(bot);

        user.RoleIds.Returns([DiscordIds.Role, RoleAboveTheBot]);
        //!mute looks the user up itself; !unmute's user parameter is read from the member cache
        guild.GetUserAsync(DiscordIds.User).Returns(user);
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(user);
        userMessages = RecordingChannel.DirectMessagesOf(user);

        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Fact]
    public async Task Mute_UserWithRoles_SavesTheRolesBelowTheBotThenSwapsThemForTheMuteRole()
    {
        CommandResult result = await RunAsync($"!mute {DiscordIds.User}>1h");

        Assert.True(result.IsSuccess, result.ErrorReason);
        await mutedUserService.Received().AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, TestTimes.Now.UtcDateTime, inAnHour, $"{DiscordIds.Role}");
        await user.Received().RemoveRolesAsync(Arg.Is<IEnumerable<IRole>>(taken => taken.Single().Id == DiscordIds.Role));
        await user.Received().AddRoleAsync(MuteRole);
        Assert.Equal($"User has been muted until {DiscordTools.UtcTimestamp(inAnHour, TimestampTagStyles.ShortDateTime)}", Assert.Single(channel.Sent).Text);
        Assert.Single(userMessages.Sent);
        host.Logger.DidNotReceiveWithAnyArgs().Error(default!, default(Exception)!);
    }

    //Kept, unmuting would put the mute role back on together with the others
    [Fact]
    public async Task Mute_UserAlreadyWearingTheMuteRole_DoesNotSaveItAsARoleToGiveBack()
    {
        user.RoleIds.Returns([DiscordIds.Role, MuteRole]);

        await RunAsync($"!mute {DiscordIds.User}>1h");

        await mutedUserService.Received().AddMutedUserAsync(DiscordIds.Server, DiscordIds.User, TestTimes.Now.UtcDateTime, inAnHour, $"{DiscordIds.Role}");
    }

    [Fact]
    public async Task Mute_AlreadyMuted_LeavesTheRolesAndTellsOnlyTheAdmin()
    {
        mutedUserService.AddMutedUserAsync(default, default, default, default, default!).ReturnsForAnyArgs(DbProcessResultEnum.AlreadyExists);

        await RunAsync($"!mute {DiscordIds.User}>1h");

        Assert.Equal("User Already muted.", Assert.Single(channel.Sent).Text);
        Assert.Empty(RoleChanges());
        Assert.Empty(userMessages.Sent);
    }

    [Theory]
    [InlineData("1 parsec")]
    [InlineData("1h 2")]
    //These three threw until 2026-11-23, and !mute answered nothing
    [InlineData("h1")]
    [InlineData("99999999999h")]
    [InlineData("10000y")]
    public async Task Mute_DurationThatCantBeRead_ChangesNothing(string duration)
    {
        await RunAsync($"!mute {DiscordIds.User}>{duration}");

        Assert.Equal("The duration could not be read!", Assert.Single(channel.Sent).Text);
        await mutedUserService.DidNotReceiveWithAnyArgs().AddMutedUserAsync(default, default, default, default, default!);
        Assert.Empty(RoleChanges());
    }

    [Fact]
    public async Task Mute_MuteRoleRefused_GivesTheRolesBackAndDropsTheMute()
    {
        user.AddRoleAsync(MuteRole, Arg.Any<RequestOptions>()).ThrowsAsync(DiscordErrors.MissingPermissions());

        await RunAsync($"!mute {DiscordIds.User}>1h");

        await user.Received().AddRolesAsync(Arg.Is<IEnumerable<IRole>>(given => given.Single().Id == DiscordIds.Role));
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
        Assert.StartsWith("The user's roles could not be changed.", Assert.Single(channel.Sent).Text);
        Assert.Empty(userMessages.Sent);
    }

    [Fact]
    public async Task Unmute_MuteWithNoRolesToGiveBack_TakesTheMuteRoleOff()
    {
        mutedUserService.GetMutedUserByUsernameAsync(DiscordIds.Server, DiscordIds.User).Returns(Muted(""));

        CommandResult result = await RunAsync($"!unmute {DiscordIds.User}");

        Assert.True(result.IsSuccess, result.ErrorReason);
        await user.Received().RemoveRoleAsync(MuteRole);
        await user.DidNotReceiveWithAnyArgs().AddRolesAsync(default(IEnumerable<ulong>)!);
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
        Assert.Equal("User has been unmuted.", Assert.Single(channel.Sent).Text);
        Assert.Single(userMessages.Sent);
    }

    [Fact]
    public async Task Unmute_MuteWithRoles_GivesThemBackAndTakesTheMuteRoleOff()
    {
        mutedUserService.GetMutedUserByUsernameAsync(DiscordIds.Server, DiscordIds.User).Returns(Muted($"{DiscordIds.Role}"));

        await RunAsync($"!unmute {DiscordIds.User}");

        await user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(given => given.Single() == DiscordIds.Role));
        await user.Received().RemoveRoleAsync(MuteRole);
        Assert.Equal("User has been unmuted.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Unmute_NotMuted_ChangesNothing()
    {
        await RunAsync($"!unmute {DiscordIds.User}");

        Assert.Equal("User is not currently muted.", Assert.Single(channel.Sent).Text);
        Assert.Empty(RoleChanges());
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, admin, channel.Channel));
    }

    private IEnumerable<string> RoleChanges()
    {
        return user.ReceivedCalls()
            .Select(call => call.GetMethodInfo().Name)
            .Where(name => name is "AddRoleAsync" or "AddRolesAsync" or "RemoveRoleAsync" or "RemoveRolesAsync");
    }

    private static IRole Role(ulong id, int position)
    {
        IRole role = Substitute.For<IRole>();
        role.Id.Returns(id);
        role.Position.Returns(position);
        return role;
    }

    private static ServerMutedUserResource Muted(string removedRoles)
    {
        return new ServerMutedUserResource
        {
            ServerDiscordId = DiscordIds.Server.ToString(),
            UserDiscordId = DiscordIds.User.ToString(),
            MutedUntil = inAnHour,
            RemovedRoleDiscordIds = removedRoles
        };
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
