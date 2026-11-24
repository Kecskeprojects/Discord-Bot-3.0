using Discord;
using Discord.Interactions;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Core;

namespace Discord_Bot.Tests.Integration.Handlers;

//What InteractionHandler answers when a button, menu or modal fails: without an answer the user only sees "This interaction failed".
//The handler is the bot's own, with the modules it loads at startup; the tests run the interaction service as it does
public sealed class InteractionHandlerTests : IAsyncLifetime
{
    private readonly TestHost host = new();
    private readonly InteractionHandler handler;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public InteractionHandlerTests()
    {
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
        handler = host.Services.GetRequiredService<InteractionHandler>();
    }

    public async ValueTask InitializeAsync()
    {
        await handler.RegisterInteractionsAsync();
    }

    //Such as a button on a message from before a restart, or from a version of the bot that had it
    [Fact]
    public async Task UnknownCustomId_SaysItIsNoLongerAvailable()
    {
        IComponentInteraction click = DiscordMocks.ButtonClick("RemovedFeature_Button", member);

        await RunAsync(click);

        Assert.Equal(("This button or menu is no longer available.", true), Response(click, nameof(IDiscordInteraction.RespondAsync)));
        host.Logger.Received().Warning("InteractionHandler.cs HandleInteractionExecutionAsync", Arg.Is<string>(text => text.StartsWith("Unknown interaction: UnknownCommand", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task UnmetPrecondition_RepliesWithTheReason()
    {
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("EditIdolData", "name", member);

        await RunAsync(choice);

        Assert.Equal(("Command can only be run by the owner of the bot.", true), Response(choice, nameof(IDiscordInteraction.RespondAsync)));
    }

    //The custom ID's parts can't be read as the handler's parameters, here a gender and a user ID. For such a part Discord.Net
    //raises InteractionExecuted twice (improvement notes, 2026-11-24), so this checks what every answer says, not how many there are
    [Fact]
    public async Task OtherFailure_SaysSomethingWentWrong()
    {
        IComponentInteraction click = DiscordMocks.ButtonClick("BiasGame_Setup_Gender_nobody_someone", member);

        await RunAsync(click);

        List<ICall> responses = [.. click.ReceivedCalls().Where(call => call.GetMethodInfo().Name == nameof(IDiscordInteraction.RespondAsync))];
        Assert.NotEmpty(responses);
        Assert.All(responses, call => Assert.Equal("Something went wrong during the process.", call.GetArguments()[0]));
    }

    //A handler that deferred before failing can't respond a second time
    [Fact]
    public async Task Failure_AfterTheInteractionWasAnswered_FollowsUpInstead()
    {
        IComponentInteraction click = DiscordMocks.ButtonClick("RemovedFeature_Button", member);
        click.HasResponded.Returns(true);

        await RunAsync(click);

        Assert.Equal(("This button or menu is no longer available.", true), Response(click, nameof(IDiscordInteraction.FollowupAsync)));
        Assert.DoesNotContain(click.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IDiscordInteraction.RespondAsync));
    }

    private async Task RunAsync(IDiscordInteraction interaction)
    {
        FakeInteractionContext context = new(interaction, member, channel.Channel, guild);
        //The bot's owner is someone else
        IUser owner = DiscordMocks.User(DiscordIds.OtherUser);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(owner);
        context.Client.TokenType.Returns(TokenType.Bot);
        context.Client.GetApplicationInfoAsync().Returns(application);

        using (IServiceScope scope = host.Services.CreateScope())
        {
            _ = await host.Services.GetRequiredService<InteractionService>().ExecuteCommandAsync(context, scope.ServiceProvider);
        }
    }

    private static (string? Text, bool Ephemeral) Response(IDiscordInteraction interaction, string method)
    {
        ICall call = Assert.Single(interaction.ReceivedCalls(), call => call.GetMethodInfo().Name == method);
        System.Reflection.ParameterInfo[] parameters = call.GetMethodInfo().GetParameters();
        object?[] arguments = call.GetArguments();
        return ((string?) arguments[0], (bool) arguments[Array.FindIndex(parameters, parameter => parameter.Name == "ephemeral")]!);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }
}
