using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.ServerSetup;

//!self role add/remove and !update role message, sent by an administrator in a server channel
public sealed class SelfRoleCommandTests : IDisposable
{
    private const ulong PreviousRoleMessage = 400_099;

    private readonly IRoleService roleService = Substitute.For<IRoleService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser admin = DiscordMocks.GuildUser(DiscordIds.OtherUser);
    private readonly RecordingChannel channel = RecordingChannel.Text();
    private readonly IRole valorant = Substitute.For<IRole>();

    public SelfRoleCommandTests()
    {
        host = new TestHost().Replace(roleService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        roleService.AddSelfRoleAsync(default, default!, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        roleService.RemoveSelfRoleAsync(default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);

        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));
        ((ITextChannel) channel.Channel).Guild.Returns(guild);

        //Discord.Net's role reader finds a role by its mention, ID or name among the server's roles
        valorant.Id.Returns(DiscordIds.Role);
        valorant.Name.Returns("Valorant");
        guild.GetRole(Arg.Any<ulong>()).Returns((IRole?) null);
        guild.GetRole(DiscordIds.Role).Returns(valorant);
        guild.Roles.Returns([valorant]);
    }

    [Theory]
    [InlineData("Valorant")]
    [InlineData("<@&500001>")]
    public async Task SelfRoleAdd_Role_SavesItWithItsNameInLowerCase(string role)
    {
        CommandResult result = await RunAsync($"!self role add {role}");

        Assert.True(result.IsSuccess, result.ErrorReason);
        await roleService.Received().AddSelfRoleAsync(DiscordIds.Server, "valorant", DiscordIds.Role);
        Assert.Equal("New role successfully added: Valorant.", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("Valorant")]
    [InlineData("<@&500001>")]
    public async Task SelfRoleRemove_Role_RemovesItByName(string role)
    {
        await RunAsync($"!self role remove {role}");

        await roleService.Received().RemoveSelfRoleAsync(DiscordIds.Server, "valorant");
        Assert.Equal("The Valorant role has been removed.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task SelfRoleRemove_NotOnTheList_SaysSo()
    {
        roleService.RemoveSelfRoleAsync(DiscordIds.Server, "valorant").Returns(DbProcessResultEnum.NotFound);

        await RunAsync("!self role remove Valorant");

        Assert.Equal("Role could not be found.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task UpdateRoleMessage_ReplacesThePreviousMessageAndSavesTheNewOnesId()
    {
        RecordingChannel roleChannel = RecordingChannel.Text(DiscordIds.OtherChannel);
        IUserMessage previous = Substitute.For<IUserMessage>();
        roleChannel.Channel.GetMessageAsync(PreviousRoleMessage).Returns(previous);
        ServerHasRoleChannel(PreviousRoleMessage);
        roleService.GetServerRolesAsync(DiscordIds.Server).Returns([new RoleResource { RoleName = "valorant" }]);
        serverService.ChangeRoleMessageIdAsync(default, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        ICommandContext context = Command("!update role message");
        context.Client.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns(roleChannel.Channel);

        await host.RunCommandAsync(context);

        await roleChannel.Channel.Received().DeleteMessageAsync(previous);
        SentMessage roleMessage = Assert.Single(roleChannel.Sent);
        Assert.EndsWith("- `Valorant`", roleMessage.Text, StringComparison.Ordinal);
        await serverService.Received().ChangeRoleMessageIdAsync(DiscordIds.Server, roleMessage.Message.Id);
        Assert.Equal("The role message has been updated.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task UpdateRoleMessage_RoleChannelDeleted_SaysSo()
    {
        ServerHasRoleChannel(null);
        ICommandContext context = Command("!update role message");
        context.Client.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns((IChannel?) null);

        await host.RunCommandAsync(context);

        Assert.Equal("The role channel could not be found, it may have been deleted.", Assert.Single(channel.Sent).Text);
    }

    //It answered nothing until 2026-11-23
    [Fact]
    public async Task UpdateRoleMessage_NoRoleChannel_SaysHowToSetOne()
    {
        await RunAsync("!update role message");

        Assert.Equal("This server has no role channel, set one with `!channel add role [channel name]`", Assert.Single(channel.Sent).Text);
    }

    private void ServerHasRoleChannel(ulong? roleMessageId)
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource
        {
            DiscordId = DiscordIds.Server,
            RoleMessageDiscordId = roleMessageId,
            SettingsChannels = new() { [ChannelTypeEnum.RoleText] = [DiscordIds.OtherChannel] }
        });
    }

    private ICommandContext Command(string text)
    {
        return DiscordMocks.TextCommand(text, admin, channel.Channel);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(Command(text));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
