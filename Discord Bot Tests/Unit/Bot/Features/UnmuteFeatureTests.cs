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

//Runs every minute from App's timer
public sealed class UnmuteFeatureTests
{
    private const ulong MuteRole = DiscordIds.OtherRole;
    private static readonly DateTime minuteStart = new(2026, 11, 18, 14, 5, 0, DateTimeKind.Utc);

    private readonly IServerMutedUserService mutedUserService = Substitute.For<IServerMutedUserService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly UnmuteFeature feature;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser user = DiscordMocks.GuildUser();
    private readonly RecordingChannel userMessages;

    public UnmuteFeatureTests()
    {
        feature = new UnmuteFeature(mutedUserService, client, new FakeTimeProvider(TestTimes.Now), serverService, logger);
        client.ConnectionState.Returns(ConnectionState.Connected);
        client.GetGuildAsync(DiscordIds.Server, CacheMode.CacheOnly).Returns(guild);
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(user);
        userMessages = RecordingChannel.DirectMessagesOf(user);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server, MuteRoleDiscordId = MuteRole });
        mutedUserService.RemoveMutedUserAsync(default, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);
    }

    [Fact]
    public async Task Run_AsksForTheMutesEndedByTheStartOfThisMinute()
    {
        mutedUserService.GetExpiredMutedUsersAsync(default).ReturnsForAnyArgs([]);

        Assert.True(await feature.Run());
        await mutedUserService.Received().GetExpiredMutedUsersAsync(minuteStart);
    }

    [Fact]
    public async Task Run_ExpiredMute_TakesTheMuteRoleOffGivesTheRolesBackAndTellsTheUser()
    {
        ExpiredMutes(Mute(DiscordIds.User, $"{DiscordIds.Role};500003"));

        Assert.True(await feature.Run());

        await user.Received().RemoveRoleAsync(MuteRole);
        await user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles => string.Join(";", roles) == $"{DiscordIds.Role};500003"));
        Assert.Equal($"You have now been unmuted in '**{TestValues.ServerName}**'.", Assert.Single(userMessages.Sent).Text);
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
    }

    [Fact]
    public async Task Run_NoRolesToGiveBack_OnlyTakesTheMuteRoleOff()
    {
        ExpiredMutes(Mute(DiscordIds.User, ""));

        Assert.True(await feature.Run());

        await user.Received().RemoveRoleAsync(MuteRole);
        await user.DidNotReceiveWithAnyArgs().AddRolesAsync(default(IEnumerable<ulong>)!);
    }

    [Fact]
    public async Task Run_ServerWithoutAMuteRole_StillGivesTheRolesBack()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        ExpiredMutes(Mute(DiscordIds.User, $"{DiscordIds.Role}"));

        Assert.True(await feature.Run());

        await user.DidNotReceiveWithAnyArgs().RemoveRoleAsync(default(ulong));
        await user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles => string.Join(";", roles) == $"{DiscordIds.Role}"));
    }

    //Leaving the server also takes away a member's roles, so there is nothing to give back
    [Fact]
    public async Task Run_UserLeftTheServer_OnlyRemovesTheMute()
    {
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns((IGuildUser?) null);
        ExpiredMutes(Mute(DiscordIds.User, $"{DiscordIds.Role}"));

        Assert.True(await feature.Run());

        Assert.Empty(user.ReceivedCalls());
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
    }

    [Fact]
    public async Task Run_BotLeftTheServer_OnlyRemovesTheMute()
    {
        client.GetGuildAsync(DiscordIds.Server, CacheMode.CacheOnly).Returns((IGuild?) null);
        ExpiredMutes(Mute(DiscordIds.User, $"{DiscordIds.Role}"));

        Assert.True(await feature.Run());

        Assert.Empty(user.ReceivedCalls());
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
    }

    //Before the gateway connects no server is cached, which must not count as the bot having left them
    [Theory]
    [InlineData(ConnectionState.Disconnected)]
    [InlineData(ConnectionState.Connecting)]
    public async Task Run_NotConnected_KeepsEveryMuteForTheNextRun(ConnectionState state)
    {
        client.ConnectionState.Returns(state);
        client.GetGuildAsync(DiscordIds.Server, CacheMode.CacheOnly).Returns((IGuild?) null);
        ExpiredMutes(Mute(DiscordIds.User, $"{DiscordIds.Role}"));

        Assert.False(await feature.Run());

        await mutedUserService.DidNotReceiveWithAnyArgs().RemoveMutedUserAsync(default, default);
    }

    //The roles are back by then; kept, the old entry would take the mute role off again if the user were muted meanwhile
    [Fact]
    public async Task Run_DirectMessagesRefused_StillRemovesTheMute()
    {
        user.CreateDMChannelAsync(default).ThrowsAsyncForAnyArgs(DiscordErrors.ClosedDirectMessages());
        ExpiredMutes(Mute(DiscordIds.User, $"{DiscordIds.Role}"));

        Assert.True(await feature.Run());

        await user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles => string.Join(";", roles) == $"{DiscordIds.Role}"));
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
        logger.Received().Warning("UnmuteFeature.cs TellUserAsync", Arg.Any<HttpException>());
    }

    [Fact]
    public async Task Run_RoleChangeRefused_KeepsThatMuteForTheNextRunAndUnmutesTheOthers()
    {
        user.RemoveRoleAsync(MuteRole, Arg.Any<RequestOptions>()).ThrowsAsync(DiscordErrors.MissingPermissions());
        IGuildUser otherUser = DiscordMocks.GuildUser(DiscordIds.OtherUser);
        RecordingChannel.DirectMessagesOf(otherUser);
        guild.GetUserAsync(DiscordIds.OtherUser, CacheMode.CacheOnly).Returns(otherUser);
        ExpiredMutes(Mute(DiscordIds.User, ""), Mute(DiscordIds.OtherUser, ""));

        Assert.False(await feature.Run());

        await mutedUserService.DidNotReceive().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.User);
        await otherUser.Received().RemoveRoleAsync(MuteRole);
        await mutedUserService.Received().RemoveMutedUserAsync(DiscordIds.Server, DiscordIds.OtherUser);
        logger.Received().Error("UnmuteFeature.cs TryUnmuteAsync", Arg.Any<HttpException>());
    }

    private void ExpiredMutes(params ServerMutedUserResource[] mutes)
    {
        mutedUserService.GetExpiredMutedUsersAsync(minuteStart).Returns([.. mutes]);
    }

    private static ServerMutedUserResource Mute(ulong userId, string removedRoles)
    {
        return new ServerMutedUserResource
        {
            ServerDiscordId = DiscordIds.Server.ToString(),
            UserDiscordId = userId.ToString(),
            MutedUntil = minuteStart,
            RemovedRoleDiscordIds = removedRoles
        };
    }
}
