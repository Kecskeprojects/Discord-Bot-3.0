using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Owner;

//The bot owner's !greeting add/list/remove and !say. Discord.Net's [RequireOwner] asks the client for the application's owner
public sealed class OwnerCommandTests : IDisposable
{
    private const ulong OwnerId = DiscordIds.ThirdUser;
    private const string Link = "https://example.com/hello.gif";

    private readonly IGreetingService greetingService = Substitute.For<IGreetingService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser owner = DiscordMocks.GuildUser(OwnerId);
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public OwnerCommandTests()
    {
        host = new TestHost().Replace(greetingService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Greeting added.")]
    [InlineData(DbProcessResultEnum.Failure, "Greeting could not be added!")]
    public async Task GreetingAdd_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        greetingService.AddGreetingAsync(Link).Returns(outcome);

        CommandResult result = await RunAsync($"!greeting add {Link}", owner);

        Assert.True(result.IsSuccess, result.ErrorReason);
        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task GreetingAdd_SomeoneElse_IsRefusedByTheCommandService()
    {
        CommandResult result = await RunAsync($"!greeting add {Link}", DiscordMocks.GuildUser());

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        await greetingService.DidNotReceiveWithAnyArgs().AddGreetingAsync(default!);
    }

    [Fact]
    public async Task GreetingList_ListsTheGreetings()
    {
        greetingService.GetAllGreetingAsync().Returns([new GreetingResource { GreetingId = 1, Url = Link }]);

        await RunAsync("!greeting list", owner);

        Assert.Equal("ID:1", Assert.Single(Assert.Single(Assert.Single(channel.Sent).Embeds).Fields).Name);
    }

    //It answered nothing until 2026-11-23
    [Fact]
    public async Task GreetingList_NoGreetings_SaysSo()
    {
        greetingService.GetAllGreetingAsync().Returns([]);

        await RunAsync("!greeting list", owner);

        Assert.Equal("There are no greetings yet!", Assert.Single(channel.Sent).Text);
    }

    //Greetings are the bot's, not a user's; until 2026-11-23 the reply added "or it is not yours", as the reminders' does
    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Greeting with the ID 2 has been removed.")]
    [InlineData(DbProcessResultEnum.NotFound, "Greeting doesn't exist with that ID.")]
    public async Task GreetingRemove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        greetingService.RemoveGreetingAsync(2).Returns(outcome);

        await RunAsync("!greeting remove 2", owner);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Say_ChannelAndMessage_DeletesTheCommandAndSendsTheMessageThere()
    {
        RecordingChannel target = RecordingChannel.Text(DiscordIds.OtherChannel);
        ITextChannel announcements = (ITextChannel) target.Channel;
        announcements.Name.Returns("announcements");
        guild.GetChannelsAsync(CacheMode.CacheOnly).Returns([announcements]);
        ICommandContext context = Command("!say announcements hello there", owner);

        CommandResult result = await host.RunCommandAsync(context);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await context.Message.Received().DeleteAsync();
        Assert.Equal("hello there", Assert.Single(target.Sent).Text);
        Assert.Empty(channel.Sent);
    }

    private ICommandContext Command(string text, IGuildUser author)
    {
        IUser applicationOwner = DiscordMocks.User(OwnerId);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(applicationOwner);
        ICommandContext context = DiscordMocks.TextCommand(text, author, channel.Channel);
        context.Client.TokenType.Returns(TokenType.Bot);
        context.Client.GetApplicationInfoAsync().Returns(application);
        return context;
    }

    private Task<CommandResult> RunAsync(string text, IGuildUser author)
    {
        return host.RunCommandAsync(Command(text, author));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
