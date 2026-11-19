using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ExceptionExtensions;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//A "+role" or "-role" message in the server's role channel; CommandHandler deletes the message itself afterwards
public sealed class SelfRoleFeatureTests
{
    private static readonly TimeSpan replyLifetime = TimeSpan.FromMilliseconds(1500);

    private readonly IRoleService roleService = Substitute.For<IRoleService>();
    private readonly FakeTimeProvider clock = new(TestTimes.Now);
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly SelfRoleFeature feature;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel roleChannel = RecordingChannel.Text();
    private readonly IRole dancer = Substitute.For<IRole>();

    public SelfRoleFeatureTests()
    {
        feature = new SelfRoleFeature(roleService, clock, Substitute.For<IServerService>(), logger);
        //Discord.Net's CommandContext takes the guild from the channel
        ((IGuildChannel) roleChannel.Channel).Guild.Returns(guild);
        dancer.Id.Returns(DiscordIds.Role);
        dancer.Name.Returns("Dancer");
        guild.GetRole(DiscordIds.Role).Returns(dancer);
        roleService.GetRoleAsync(DiscordIds.Server, "dancer").Returns(new RoleResource { DiscordId = DiscordIds.Role, RoleName = "dancer" });
    }

    [Fact]
    public async Task Run_PlusAndTheRoleName_GivesTheRoleAndSaysSo()
    {
        Task<bool> run = feature.Run(Message("+Dancer"));
        clock.Advance(replyLifetime);

        Assert.True(await run);
        await member.Received().AddRoleAsync(dancer);
        Assert.Equal("You now have the `Dancer` role!", Assert.Single(roleChannel.Sent).Text);
    }

    [Fact]
    public async Task Run_MinusAndTheRoleName_TakesTheRoleOffAndSaysSo()
    {
        Task<bool> run = feature.Run(Message("-Dancer"));
        clock.Advance(replyLifetime);

        Assert.True(await run);
        await member.Received().RemoveRoleAsync(dancer);
        Assert.Equal("`Dancer` role has been removed!", Assert.Single(roleChannel.Sent).Text);
    }

    [Fact]
    public async Task Run_Reply_IsDeletedAfterOneAndAHalfSeconds()
    {
        Task<bool> run = feature.Run(Message("+dancer"));
        IUserMessage reply = Assert.Single(roleChannel.Sent).Message;

        clock.Advance(replyLifetime - TimeSpan.FromMilliseconds(1));
        await reply.DidNotReceiveWithAnyArgs().DeleteAsync();

        clock.Advance(TimeSpan.FromMilliseconds(1));
        Assert.True(await run);
        await reply.Received().DeleteAsync();
    }

    [Fact]
    public async Task Run_RoleThatIsNotASelfRole_DoesNothing()
    {
        roleService.GetRoleAsync(DiscordIds.Server, "singer").Returns((RoleResource?) null);

        Assert.True(await feature.Run(Message("+singer")));

        Assert.Empty(member.ReceivedCalls());
        Assert.Empty(roleChannel.Sent);
    }

    [Fact]
    public async Task Run_RoleDeletedOnDiscord_LogsTheErrorAndRepliesNothing()
    {
        guild.GetRole(DiscordIds.Role).Returns((IRole?) null);

        Assert.False(await feature.Run(Message("+dancer")));

        Assert.Empty(roleChannel.Sent);
        logger.Received().Error("SelfRoleFeature.cs ExecuteCoreLogicAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task Run_DiscordRefusesTheRoleChange_LogsTheErrorAndRepliesNothing()
    {
        member.AddRoleAsync(dancer, Arg.Any<RequestOptions>()).ThrowsAsync(DiscordErrors.MissingPermissions());

        Assert.False(await feature.Run(Message("+dancer")));

        Assert.Empty(roleChannel.Sent);
        logger.Received().Error("SelfRoleFeature.cs ExecuteCoreLogicAsync", Arg.Any<HttpException>());
    }

    private ICommandContext Message(string text)
    {
        return DiscordMocks.TextCommand(text, member, roleChannel.Channel);
    }
}
