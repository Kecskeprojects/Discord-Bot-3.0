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

//!command add/remove by an administrator and !custom list
public sealed class CustomCommandCommandTests : IDisposable
{
    private const string Link = "https://example.com/wave.gif";

    private readonly ICustomCommandService customCommandService = Substitute.For<ICustomCommandService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser admin = DiscordMocks.GuildUser(DiscordIds.OtherUser);
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public CustomCommandCommandTests()
    {
        host = new TestHost().Replace(customCommandService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        customCommandService.AddCustomCommandAsync(default, default!, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Fact]
    public async Task CommandAdd_NameAndLink_SavesTheCommand()
    {
        CommandResult result = await RunAsync($"!command add wave {Link}", admin);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await customCommandService.Received().AddCustomCommandAsync(DiscordIds.Server, "wave", Link);
        Assert.Equal("New command successfully added: wave.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task CommandAdd_NotALink_SaysSo()
    {
        await RunAsync("!command add wave hello", admin);

        await customCommandService.DidNotReceiveWithAnyArgs().AddCustomCommandAsync(default, default!, default!);
        Assert.Equal("That link is invalid!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task CommandAdd_NameTaken_SaysSo()
    {
        customCommandService.AddCustomCommandAsync(DiscordIds.Server, "wave", Link).Returns(DbProcessResultEnum.AlreadyExists);

        await RunAsync($"!command add wave {Link}", admin);

        Assert.Equal("A command with this name already exists on this server.", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "The wave command has been removed.")]
    [InlineData(DbProcessResultEnum.NotFound, "Command does not exist.")]
    public async Task CommandRemove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        customCommandService.RemoveCustomCommandAsync(DiscordIds.Server, "wave").Returns(outcome);

        await RunAsync("!command remove wave", admin);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task CommandAdd_AskedByAMember_IsRefusedByTheCommandService()
    {
        CommandResult result = await RunAsync($"!command add wave {Link}", DiscordMocks.GuildUser());

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
    }

    [Fact]
    public async Task CustomList_ListsTheServersCommands()
    {
        customCommandService.GetServerCustomCommandListAsync(DiscordIds.Server).Returns([new CustomCommandResource { Command = "hello" }, new CustomCommandResource { Command = "wave" }]);

        await RunAsync("!custom list", DiscordMocks.GuildUser());

        Assert.Equal("!hello , !wave", Assert.Single(Assert.Single(channel.Sent).Embeds).Description);
    }

    [Fact]
    public async Task CustomList_NoCommands_SaysSo()
    {
        customCommandService.GetServerCustomCommandListAsync(DiscordIds.Server).Returns([]);

        await RunAsync("!custom list", DiscordMocks.GuildUser());

        Assert.Equal("There are no custom commands on this server!", Assert.Single(channel.Sent).Text);
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
