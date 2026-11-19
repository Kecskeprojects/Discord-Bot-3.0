using DatabaseORM.Enums;
using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Core;
using CommandResult = Discord.Commands.IResult;
using InteractionResult = Discord.Interactions.IResult;

namespace Discord_Bot.Tests.Integration.BiasGame;

//The bias game from the text command to its first button, through Discord.Net's own command and interaction services.
//It runs in a direct message, which needs no database.
public sealed class BiasGameFlowTests : IDisposable
{
    private readonly TestHost host = new();
    private readonly IUser user = DiscordMocks.User();
    private readonly RecordingChannel directMessage = RecordingChannel.DirectMessage();

    [Fact]
    public async Task BiasGame_InADirectMessage_AsksForTheGender()
    {
        CommandResult result = await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game", user, directMessage.Channel));

        Assert.True(result.IsSuccess, result.ErrorReason);
        SentMessage sent = Assert.Single(directMessage.Sent);
        Assert.Single(sent.Embeds);
        Assert.NotNull(sent.Components);
    }

    [Fact]
    public async Task BiasGame_WhileOneIsRunning_SaysSo()
    {
        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game", user, directMessage.Channel));

        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game", user, directMessage.Channel));

        Assert.Equal("You already have a game going!", directMessage.Sent[^1].Text);
    }

    [Fact]
    public async Task GenderButton_DefersThenShowsTheDebutYears()
    {
        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game", user, directMessage.Channel));
        IComponentInteraction click = DiscordMocks.ButtonClick($"BiasGame_Setup_Gender_{(int) GenderEnum.Female}_{DiscordIds.User}", user);

        InteractionResult result = await host.RunInteractionAsync(new FakeInteractionContext(click, user, directMessage.Channel));

        Assert.True(result.IsSuccess, result.ErrorReason);
        await click.ReceivedWithAnyArgs().DeferAsync();
        await click.ReceivedWithAnyArgs().ModifyOriginalResponseAsync(default!);
        Assert.True(host.Services.GetRequiredService<BiasGameStore>().TryGet(DiscordIds.User, out BiasGameData? game));
        Assert.Equal(GenderEnum.Female, game.Gender);
    }

    [Fact]
    public async Task GenderButton_OnSomeoneElsesGame_IsTurnedAway()
    {
        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias game", user, directMessage.Channel));
        IUser someoneElse = DiscordMocks.User(DiscordIds.OtherUser, "someone else");
        IComponentInteraction click = DiscordMocks.ButtonClick($"BiasGame_Setup_Gender_{(int) GenderEnum.Female}_{DiscordIds.User}", someoneElse);

        await host.RunInteractionAsync(new FakeInteractionContext(click, someoneElse, directMessage.Channel));

        ICall respond = Assert.Single(click.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IDiscordInteraction.RespondAsync));
        Assert.Equal("You are not the owner of this interaction.", respond.GetArguments()[0]);
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
