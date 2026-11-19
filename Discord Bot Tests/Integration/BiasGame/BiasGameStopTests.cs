using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.BiasGame;

//!bias game stop through Discord.Net's command service, in a direct message (which needs no database)
public sealed class BiasGameStopTests : IDisposable
{
    private const ulong GameMessage = 400_009;

    private readonly TestHost host = new();
    //Decides the 1-in-100 roll; every other test rolls past it
    private readonly Random random = Substitute.For<Random>();
    private readonly IUser user = DiscordMocks.User();
    private readonly RecordingChannel directMessage = RecordingChannel.DirectMessage();

    public BiasGameStopTests()
    {
        host.Replace(random);
        random.Next(0, 100).Returns(1);
    }

    private BiasGameStore Games => host.Services.GetRequiredService<BiasGameStore>();

    [Theory]
    [InlineData("!bias game stop")]
    [InlineData("!bgs")]
    [InlineData("!biasgamestop")]
    public async Task BiasGameStop_RunningGame_StopsItAndDeletesItsMessage(string text)
    {
        BiasGameData game = Games.TryStart(DiscordIds.User)!;
        game.MessageId = GameMessage;

        CommandResult result = await host.RunCommandAsync(DiscordMocks.TextCommand(text, user, directMessage.Channel));

        Assert.True(result.IsSuccess, result.ErrorReason);
        Assert.Equal("Game stopped!", Assert.Single(directMessage.Sent).Text);
        Assert.Equal([GameMessage], directMessage.Deleted);
        Assert.False(Games.TryGet(DiscordIds.User, out _));
        Assert.Throws<ObjectDisposedException>(() => game.WinnerBracket.Length);
    }

    [Fact]
    public async Task BiasGameStop_NoGame_SaysNothing()
    {
        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game stop", user, directMessage.Channel));

        Assert.Empty(directMessage.Sent);
        Assert.Empty(directMessage.Deleted);
    }

    [Fact]
    public async Task BiasGameStop_OneInAHundred_RefusesWithAnEasterEggAndKeepsTheGame()
    {
        Games.TryStart(DiscordIds.User);
        random.Next(0, 100).Returns(0);
        random.Next(0, Constant.BiasGameStopMessages.Length).Returns(1);

        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game stop", user, directMessage.Channel));

        Assert.Equal(Constant.BiasGameStopMessages[1], Assert.Single(directMessage.Sent).Text);
        Assert.True(Games.TryGet(DiscordIds.User, out _));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
