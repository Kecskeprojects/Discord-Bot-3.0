using DatabaseORM.Context;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Discord;
using Discord.Interactions;
using Discord_Bot.Processors.EmbedProcessors.Polls;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using Microsoft.EntityFrameworkCore;
using InteractionResult = Discord.Interactions.IResult;

namespace Discord_Bot.Tests.Integration.Polls;

//Choosing an answer slot in a poll's or a preset's edit message: the menu as PollEditEmbedProcessor and
//PollPresetEditEmbedProcessor build it, the choice through Discord.Net's interaction service, the rows on LocalDB
public sealed class AnswerEditTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();
    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser admin = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();
    //The bot's owner: the administrator choosing, unless a test says otherwise
    private ulong ownerId = DiscordIds.User;

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        await host.FromNewScope<IServerService>().AddServerAsync(DiscordIds.Server);
        ((IGuildChannel) channel.Channel).Guild.Returns(guild);
        //A poll's edit message is for the server's administrators, a preset's for the bot's owner
        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));
    }

    [Fact]
    public async Task PollsAnswerMenu_EmptySlot_CreatesAnAnswerOfThePollAndOpensThePollsModal()
    {
        int pollId = await NewPollAsync();
        WeeklyPollEditResource? poll = await host.FromNewScope<IWeeklyPollService>().GetPollByIdForEditAsync(pollId);
        SelectMenuComponent menu = AnswerMenu(PollEditEmbedProcessor.CreateComponent(poll!, []));

        IComponentInteraction choice = await ChooseAsync(menu, menu.Options.First().Value);

        WeeklyPollOption answer = await SingleAnswerAsync();
        Assert.Equal(pollId, answer.WeeklyPollId);
        Assert.Null(answer.WeeklyPollOptionPresetId);
        await choice.Received().RespondWithModalAsync(Arg.Is<Modal>(modal => modal.CustomId == $"EditPollOptionModal_{pollId}_{answer.WeeklyPollOptionId}"), Arg.Any<RequestOptions>());
    }

    [Fact]
    public async Task PresetsAnswerMenu_ExistingAnswer_OpensThatAnswer()
    {
        //A poll's answer first, so the preset's answer has a different ID from the preset
        await NewPollAsync("Elsewhere");
        int presetId = await NewPresetAsync("Yes");
        WeeklyPollOption presetAnswer;
        await using (MainDbContext context = database.NewContext())
        {
            presetAnswer = await context.WeeklyPollOptions.SingleAsync(option => option.WeeklyPollOptionPresetId == presetId, TestContext.Current.CancellationToken);
        }
        Assert.NotEqual(presetId, presetAnswer.WeeklyPollOptionId);
        WeeklyPollOptionPresetResource? preset = await host.FromNewScope<IWeeklyPollOptionPresetService>().GetPresetByIdAsync(presetId);
        SelectMenuComponent menu = AnswerMenu(PollPresetEditEmbedProcessor.CreateComponent(preset!));

        IComponentInteraction choice = await ChooseAsync(menu, menu.Options.First().Value);

        await choice.Received().RespondWithModalAsync(Arg.Is<Modal>(modal => modal.CustomId == $"EditPollOptionPresetModal_{presetId}_{presetAnswer.WeeklyPollOptionId}"), Arg.Any<RequestOptions>());
    }

    //Until 2026-11-23 "Create Polls" in the channel was enough for all of these, which Discord gives everyone by default
    [Fact]
    public async Task PollsEditMessage_MemberWhoIsNoAdministrator_IsRefused()
    {
        IGuildUser member = DiscordMocks.GuildUser(DiscordIds.OtherUser);
        member.GetPermissions(Arg.Any<IGuildChannel>()).Returns(new ChannelPermissions(ulong.MaxValue));

        Assert.Equal(InteractionCommandError.UnmetPrecondition, (await RunAsync(DiscordMocks.SelectMenuChoice("PollOption_Change_CustomOption_False_1", "0_0", member), member)).Error);
        Assert.Equal(InteractionCommandError.UnmetPrecondition, (await RunAsync(DiscordMocks.ButtonClick("EditPoll_1", member), member)).Error);
    }

    //Presets are shared by every server's polls
    [Fact]
    public async Task PresetsEditMessage_AdministratorWhoIsNotTheOwner_IsRefused()
    {
        ownerId = DiscordIds.OtherUser;

        Assert.Equal(InteractionCommandError.UnmetPrecondition, (await RunAsync(DiscordMocks.SelectMenuChoice("PollOption_Change_CustomOption_True_1", "0_0", admin), admin)).Error);
        Assert.Equal(InteractionCommandError.UnmetPrecondition, (await RunAsync(DiscordMocks.ButtonClick("EditPollPreset_1", admin), admin)).Error);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private static SelectMenuComponent AnswerMenu(MessageComponent component)
    {
        return ComponentParts.SelectMenus(component).Single(menu => menu.CustomId.StartsWith("PollOption_Change_CustomOption_", StringComparison.Ordinal));
    }

    private async Task<IComponentInteraction> ChooseAsync(SelectMenuComponent menu, string value)
    {
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice(menu.CustomId, value, admin);
        InteractionResult result = await RunAsync(choice, admin);
        Assert.True(result.IsSuccess, result.ErrorReason);
        return choice;
    }

    private Task<InteractionResult> RunAsync(IComponentInteraction interaction, IGuildUser user)
    {
        FakeInteractionContext context = new(interaction, user, channel.Channel, guild);
        IUser owner = DiscordMocks.User(ownerId);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(owner);
        context.Client.TokenType.Returns(TokenType.Bot);
        context.Client.GetApplicationInfoAsync().Returns(application);
        return host.RunInteractionAsync(context);
    }

    private async Task<WeeklyPollOption> SingleAnswerAsync()
    {
        await using MainDbContext context = database.NewContext();
        return await context.WeeklyPollOptions.SingleAsync(TestContext.Current.CancellationToken);
    }

    //A named poll; with an answer when one is given
    private async Task<int> NewPollAsync(string? answer = null)
    {
        WeeklyPollEditResource? draft = await host.FromNewScope<IWeeklyPollService>().GetOrCreateDummyPollAsync(DiscordIds.Server);
        await host.FromNewScope<IWeeklyPollService>().UpdateAsync(draft!.WeeklyPollId, "weekly", "Weekly question", DiscordIds.Channel, null);
        if (answer != null)
        {
            WeeklyPollOptionResource? option = await host.FromNewScope<IWeeklyPollOptionService>().GetOrCreateOptionAsync(false, draft.WeeklyPollId, 0, 0);
            await host.FromNewScope<IWeeklyPollOptionService>().UpdateAsync(option!.WeeklyPollOptionId!.Value, answer);
        }
        return draft.WeeklyPollId;
    }

    private async Task<int> NewPresetAsync(string answer)
    {
        WeeklyPollOptionPresetResource? draft = await host.FromNewScope<IWeeklyPollOptionPresetService>().GetOrCreateDummyPresetAsync();
        await host.FromNewScope<IWeeklyPollOptionPresetService>().UpdateAsync(draft!.WeeklyPollOptionPresetId, "Yes or No", "");
        WeeklyPollOptionResource? option = await host.FromNewScope<IWeeklyPollOptionService>().GetOrCreateOptionAsync(true, draft.WeeklyPollOptionPresetId, 0, 0);
        await host.FromNewScope<IWeeklyPollOptionService>().UpdateAsync(option!.WeeklyPollOptionId!.Value, answer);
        return draft.WeeklyPollOptionPresetId;
    }
}
