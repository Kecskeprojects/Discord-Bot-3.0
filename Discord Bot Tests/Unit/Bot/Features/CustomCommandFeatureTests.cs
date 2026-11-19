using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;
using NSubstitute.ExceptionExtensions;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//What CommandHandler falls back to when a server message starts with the prefix but isn't one of the bot's commands
public sealed class CustomCommandFeatureTests
{
    private const string Url = "https://example.com/hello.gif";

    private readonly ICustomCommandService commandService = Substitute.For<ICustomCommandService>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly CustomCommandFeature feature;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public CustomCommandFeatureTests()
    {
        feature = new CustomCommandFeature(commandService, Substitute.For<IServerService>(), logger);
        ((IGuildChannel) channel.Channel).Guild.Returns(guild);
        commandService.GetCustomCommandAsync(DiscordIds.Server, "hello").Returns(new CustomCommandResource { Command = "hello", Url = Url });
    }

    [Fact]
    public async Task Run_TheServersCommand_PostsItsLink()
    {
        Assert.True(await feature.Run(Message("!Hello")));

        Assert.Equal(Url, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Run_UnknownCommand_SendsNothingAndReturnsFalse()
    {
        commandService.GetCustomCommandAsync(DiscordIds.Server, "wave").Returns((CustomCommandResource?) null);

        Assert.False(await feature.Run(Message("!wave")));
        Assert.Empty(channel.Sent);
    }

    [Fact]
    public async Task Run_ChannelRefusesTheLink_LogsTheErrorAndReturnsFalse()
    {
        ITextChannel refusing = Substitute.For<ITextChannel>();
        refusing.Guild.Returns(guild);
        refusing.SendMessageAsync().ThrowsAsyncForAnyArgs(DiscordErrors.MissingPermissions());

        Assert.False(await feature.Run(DiscordMocks.TextCommand("!hello", DiscordMocks.GuildUser(), refusing)));
        logger.Received().Error("CustomCommandFeature.cs ExecuteCoreLogicAsync", Arg.Any<Exception>());
    }

    private ICommandContext Message(string text)
    {
        return DiscordMocks.TextCommand(text, DiscordMocks.GuildUser(), channel.Channel);
    }
}
