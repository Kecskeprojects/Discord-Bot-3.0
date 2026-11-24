using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Core;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot.Tests.Integration.Handlers;

//How CommandHandler routes each message the gateway delivers: commands after ! or ., self-role requests in the role channel,
//the easter eggs for everything else, and what it does with commands that fail. The handler is the bot's own, with the modules
//it loads at startup; the tests hand it messages directly instead of raising the socket event
public sealed class CommandHandlerTests : IAsyncLifetime
{
    private const string EightBall = "!8ball will it rain?";

    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly ICustomCommandService customCommandService = Substitute.For<ICustomCommandService>();
    private readonly IRoleService roleService = Substitute.For<IRoleService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    //Its rolls for !8ball and the easter eggs overlap (both lists have 20 entries), so the tests check that the command replied, not what with
    private readonly Random random = Substitute.For<Random>();
    private readonly TestHost host;
    private readonly CommandHandler handler;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();
    private readonly RecordingChannel roleChannel = RecordingChannel.Text(DiscordIds.OtherChannel);

    public CommandHandlerTests()
    {
        host = new TestHost().Replace(serverService).Replace(customCommandService).Replace(roleService).Replace(client).Replace(random);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource
        {
            DiscordId = DiscordIds.Server,
            SettingsChannels = new() { [ChannelTypeEnum.RoleText] = [DiscordIds.OtherChannel] }
        });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
        ((ITextChannel) roleChannel.Channel).Guild.Returns(guild);

        //The easter eggs answer mentions of the bot, and roll for the rest; none comes up unless a test rolls for it
        ISelfUser bot = Substitute.For<ISelfUser>();
        bot.Id.Returns(DiscordIds.Bot);
        client.CurrentUser.Returns(bot);
        random.Next(0, 5000).Returns(1);
        random.Next(0, 20).Returns(1);

        handler = host.Services.GetRequiredService<CommandHandler>();
    }

    public async ValueTask InitializeAsync()
    {
        await handler.RegisterCommandsAsync();
    }

    [Theory]
    [InlineData(EightBall)]
    [InlineData(".8ball will it rain?")]
    public async Task Prefix_RunsTheCommandAndLogsTheMessage(string text)
    {
        await handler.ProcessMessageAsync(DiscordMocks.UserMessage(text, member, channel.Channel));

        Assert.Single(channel.Sent);
        host.Logger.Received().MesUser(text, TestValues.ServerName);
    }

    [Fact]
    public async Task DirectMessage_RunsTheCommandWithoutAServer()
    {
        RecordingChannel directMessage = RecordingChannel.DirectMessage();

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage(EightBall, DiscordMocks.User(), directMessage.Channel));

        Assert.Single(directMessage.Sent);
        host.Logger.Received().MesUser(EightBall);
        await serverService.DidNotReceiveWithAnyArgs().AddServerAsync(default);
    }

    [Fact]
    public async Task NewServer_IsAddedBeforeTheCommandRuns()
    {
        UseNewServer();
        serverService.GetByDiscordIdAsync(DiscordIds.OtherServer).Returns(null, new ServerResource { DiscordId = DiscordIds.OtherServer });
        serverService.AddServerAsync(DiscordIds.OtherServer).Returns(DbProcessResultEnum.Success);

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage(EightBall, member, channel.Channel));

        await serverService.Received(1).AddServerAsync(DiscordIds.OtherServer);
        Assert.Single(channel.Sent);
    }

    [Fact]
    public async Task NewServer_CannotBeAdded_LogsTheErrorAndRunsNothing()
    {
        UseNewServer();
        serverService.GetByDiscordIdAsync(DiscordIds.OtherServer).Returns((ServerResource?) null);
        serverService.AddServerAsync(DiscordIds.OtherServer).Returns(DbProcessResultEnum.Failure);

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage(EightBall, member, channel.Channel));

        host.Logger.Received().Error("CommandHandler.cs ProcessMessageAsync", Arg.Any<InvalidOperationException>());
        Assert.Empty(channel.Sent);
    }

    //Answering them could start a loop between bots
    [Theory]
    [InlineData(MessageSource.Bot)]
    [InlineData(MessageSource.Webhook)]
    [InlineData(MessageSource.System)]
    public async Task NotFromAUser_IsOnlyLogged(MessageSource source)
    {
        await handler.ProcessMessageAsync(DiscordMocks.UserMessage(EightBall, member, channel.Channel, source));

        host.Logger.Received().MesOther(EightBall, TestValues.ServerName);
        Assert.Empty(channel.Sent);
        await serverService.DidNotReceiveWithAnyArgs().GetByDiscordIdAsync(default);
    }

    //Such as a message with only an image
    [Fact]
    public async Task NoText_IsIgnored()
    {
        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("", member, channel.Channel));

        host.Logger.DidNotReceiveWithAnyArgs().MesUser(default!, default!);
        await serverService.DidNotReceiveWithAnyArgs().GetByDiscordIdAsync(default);
    }

    [Theory]
    [InlineData("+Dancer")]
    [InlineData("-Dancer")]
    public async Task RoleChannel_RoleRequest_LooksTheRoleUpAndDeletesTheMessage(string text)
    {
        IUserMessage message = DiscordMocks.UserMessage(text, member, roleChannel.Channel);

        await handler.ProcessMessageAsync(message);

        await roleService.Received().GetRoleAsync(DiscordIds.Server, "dancer");
        await message.Received().DeleteAsync();
    }

    [Fact]
    public async Task RoleChannel_OtherText_IsOnlyDeleted()
    {
        IUserMessage message = DiscordMocks.UserMessage("can I have a role?", member, roleChannel.Channel);

        await handler.ProcessMessageAsync(message);

        await message.Received().DeleteAsync();
        await roleService.DidNotReceiveWithAnyArgs().GetRoleAsync(default, default!);
        random.DidNotReceiveWithAnyArgs().Next(default, default);
    }

    [Fact]
    public async Task RoleChannel_Command_RunsItAndKeepsTheMessage()
    {
        IUserMessage message = DiscordMocks.UserMessage(EightBall, member, roleChannel.Channel);

        await handler.ProcessMessageAsync(message);

        Assert.Single(roleChannel.Sent);
        await message.DidNotReceive().DeleteAsync();
    }

    //A server that set no role channel has none, rather than every channel being one (where every message would be deleted)
    [Fact]
    public async Task NoRoleChannel_RoleRequest_IsNeitherHandledNorDeleted()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        IUserMessage message = DiscordMocks.UserMessage("+Dancer", member, channel.Channel);

        await handler.ProcessMessageAsync(message);

        await roleService.DidNotReceiveWithAnyArgs().GetRoleAsync(default, default!);
        await message.DidNotReceive().DeleteAsync();
    }

    [Fact]
    public async Task OtherMessage_RunsTheEasterEggs()
    {
        random.Next(0, 5000).Returns(0);

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("good morning", member, channel.Channel));

        Assert.Contains(Assert.Single(channel.Sent).Text, Constant.EasterEggMessages);
    }

    [Fact]
    public async Task UnknownCommand_InAServer_PostsTheServersCustomCommand()
    {
        customCommandService.GetCustomCommandAsync(DiscordIds.Server, "hello").Returns(new CustomCommandResource { Command = "hello", Url = "https://example.com/hello.gif" });

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("!hello", member, channel.Channel));

        Assert.Equal("https://example.com/hello.gif", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task UnknownCommand_InADirectMessage_LooksForNoCustomCommand()
    {
        RecordingChannel directMessage = RecordingChannel.DirectMessage();

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("!hello", DiscordMocks.User(), directMessage.Channel));

        await customCommandService.DidNotReceiveWithAnyArgs().GetCustomCommandAsync(default, default!);
        Assert.Empty(directMessage.Sent);
    }

    [Fact]
    public async Task UnmetPrecondition_RepliesWithTheReason()
    {
        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("!self role add Dancer", member, channel.Channel));

        Assert.Equal("User requires guild permission Administrator.", Assert.Single(channel.Sent).Text);
        await roleService.DidNotReceiveWithAnyArgs().AddSelfRoleAsync(default, default!, default);
    }

    [Fact]
    public async Task ObjectNotFound_RepliesWithTheReason()
    {
        member.GuildPermissions.Returns(new GuildPermissions(administrator: true));
        guild.Roles.Returns([]);

        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("!self role add Dancer", member, channel.Channel));

        Assert.Equal("Role not found.", Assert.Single(channel.Sent).Text);
    }

    //Other failures, such as a command missing its arguments, are only logged
    [Fact]
    public async Task MissingArguments_LogsAWarningAndRepliesNothing()
    {
        await handler.ProcessMessageAsync(DiscordMocks.UserMessage("!8ball", member, channel.Channel));

        host.Logger.Received().Warning("CommandHandler.cs HandleCommandExecutionAsync", "The input text has too few parameters.");
        Assert.Empty(channel.Sent);
    }

    //The message's channel belongs to a server the bot hasn't seen yet
    private void UseNewServer()
    {
        guild.Id.Returns(DiscordIds.OtherServer);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }
}
