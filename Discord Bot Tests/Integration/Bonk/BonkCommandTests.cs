using System.IO;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using ImageMagick;
using NSubstitute.Core;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Bonk;

//!bonk through Discord.Net's command service, with the real GIF processor and the avatar download replaced
public sealed class BonkCommandTests : IDisposable
{
    private const string NoSuchUser = "No user was found with that name or ID!";
    //Real Discord IDs are too long for an int, which keeps them apart from a frame delay
    private const ulong AliceId = 123_456_789_012_345_678;
    private const ulong StrangerId = 987_654_321_098_765_432;

    private readonly IWebService webService = Substitute.For<IWebService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly IGuildUser alice = DiscordMocks.GuildUser(AliceId, username: "alice");
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public BonkCommandTests()
    {
        host = new TestHost().Replace(webService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        webService.GetStream(default!).ReturnsForAnyArgs(_ => Avatar());
        member.GetDisplayAvatarUrl(default, default).ReturnsForAnyArgs(TestValues.AvatarUrl);
        alice.GetDisplayAvatarUrl(default, default).ReturnsForAnyArgs("https://example.com/alice.png");
        guild.GetUserAsync(AliceId, CacheMode.CacheOnly).Returns(alice);
        guild.SearchUsersAsync("alice", 1).Returns([alice]);
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData("!bonk")]
    //It was one alias, "hornyjail, hit me please", until 2026-11-23
    [InlineData("!hornyjail")]
    [InlineData("!hit me please")]
    public async Task Bonk_NoOneNamed_BonksTheSender(string command)
    {
        CommandResult result = await RunAsync(command);

        Assert.True(result.IsSuccess, result.ErrorReason);
        (string fileName, MagickImageCollection gif) = SentGif();
        using (gif)
        {
            Assert.Equal($"bonk_{TestValues.Username}.gif", fileName);
            Assert.Equal(10u, gif[0].AnimationDelay);
        }
        await webService.Received().GetStream(TestValues.AvatarUrl);
    }

    [Theory]
    [InlineData("!bonk alice 25")]
    [InlineData("!bonk 123456789012345678 25")]
    [InlineData("!bonk 123456789012345678")]
    public async Task Bonk_UserByNameOrIdAndADelay_BonksThemWithIt(string command)
    {
        await RunAsync(command);

        (string fileName, MagickImageCollection gif) = SentGif();
        using (gif)
        {
            Assert.Equal("bonk_alice.gif", fileName);
            Assert.Equal(command.EndsWith(" 25", StringComparison.Ordinal) ? 25u : 10u, gif[0].AnimationDelay);
        }
        await webService.Received().GetStream("https://example.com/alice.png");
    }

    [Fact]
    public async Task Bonk_DelayOutOfRange_SaysSo()
    {
        await RunAsync("!bonk 5000");

        Assert.Equal("Invalid frame delay length. (1-1000)", Assert.Single(channel.Sent).Text);
    }

    //A name answered nothing until 2026-11-23, and an ID of someone not on the server threw
    [Theory]
    [InlineData("!bonk nobody")]
    [InlineData("!bonk 987654321098765432")]
    public async Task Bonk_UserNotOnTheServer_SaysSo(string command)
    {
        guild.SearchUsersAsync("nobody", 1).Returns([]);
        guild.GetUserAsync(StrangerId, CacheMode.CacheOnly).Returns((IGuildUser?) null);

        await RunAsync(command);

        Assert.Equal(NoSuchUser, Assert.Single(channel.Sent).Text);
        host.Logger.DidNotReceiveWithAnyArgs().Error(default!, default(Exception)!);
    }

    //It answered nothing until 2026-11-23; the processor logs why
    [Fact]
    public async Task Bonk_AvatarThatIsNoImage_SaysTheGifCouldNotBeMade()
    {
        webService.GetStream(default!).ReturnsForAnyArgs(_ => new MemoryStream([1, 2, 3]));

        await RunAsync("!bonk");

        Assert.Equal("The bonk gif could not be made!", Assert.Single(channel.Sent).Text);
    }

    private (string FileName, MagickImageCollection Gif) SentGif()
    {
        ICall sent = Assert.Single(channel.Channel.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IMessageChannel.SendFileAsync));
        object?[] arguments = sent.GetArguments();
        //The command disposes the stream after sending, which still leaves its bytes readable
        return ((string) arguments[1]!, new MagickImageCollection(((MemoryStream) arguments[0]!).ToArray()));
    }

    private static MemoryStream Avatar()
    {
        using MagickImage image = new(MagickColors.MediumPurple, 128, 128);
        MemoryStream stream = new();
        image.Write(stream, MagickFormat.Png);
        stream.Position = 0;
        return stream;
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
