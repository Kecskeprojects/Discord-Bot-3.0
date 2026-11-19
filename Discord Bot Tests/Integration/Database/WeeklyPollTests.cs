using DatabaseORM.Context;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Microsoft.EntityFrameworkCore;

namespace Discord_Bot.Tests.Integration.Database;

//Weekly polls through the real services and repositories on LocalDB, built the way the bot's edit message builds them:
//!weekly poll add makes a nameless draft, the edit modal names it, and each answer is created empty, then given its title
public sealed class WeeklyPollTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IServerService>().AddServerAsync(DiscordIds.Server));
    }

    //TestTimes.Now is a Wednesday
    [Fact]
    public async Task NewPoll_ThroughTheEditSteps_IsSentOnItsDayWithItsAnswersInOrder()
    {
        int pollId = await NewPollAsync("weekly", "Yes", "No");

        WeeklyPollResource poll = Assert.Single(await PollsService.GetPollsByDayOfWeekAsync(DayOfWeek.Wednesday));
        Assert.Equal(pollId, poll.WeeklyPollId);
        Assert.Equal("Weekly question", poll.Title);
        Assert.Equal(DiscordIds.Server, poll.ServerDiscordId);
        Assert.Equal(DiscordIds.Channel, poll.ChannelDiscordId);
        Assert.Equal(DiscordIds.Role, poll.RoleDiscordId);
        Assert.Equal(["Yes", "No"], poll.Options);
        Assert.Empty(await PollsService.GetPollsByDayOfWeekAsync(DayOfWeek.Thursday));
    }

    [Fact]
    public async Task GetPollsByDayOfWeekAsync_InactivePoll_IsNotSent()
    {
        int pollId = await NewPollAsync("weekly", "Yes", "No");
        await PollsService.UpdateFieldAsync(pollId, nameof(WeeklyPoll.IsActive), "False");

        Assert.Empty(await PollsService.GetPollsByDayOfWeekAsync(DayOfWeek.Wednesday));
    }

    //The poll's own answers and its preset's answers come through two includes, the second nested in the first
    [Fact]
    public async Task GetPollsByDayOfWeekAsync_PollWithAPreset_UsesThePresetsAnswers()
    {
        int presetId = await NewPresetAsync("Yes or No", "Yes", "No", "Maybe");
        int pollId = await NewPollAsync("weekly");
        await PollsService.UpdateFieldAsync(pollId, nameof(WeeklyPoll.OptionPresetId), presetId.ToString());

        WeeklyPollResource poll = Assert.Single(await PollsService.GetPollsByDayOfWeekAsync(DayOfWeek.Wednesday));

        Assert.Equal(["Yes", "No", "Maybe"], poll.Options);
    }

    //Its answers are the coming week's days (from the system clock, see WeeklyPollToolsTests) and "None of the above"
    [Fact]
    public async Task GetPollsByDayOfWeekAsync_DaysOfWeekPreset_GivesEightAnswers()
    {
        await database.AddAsync(new WeeklyPollOptionPreset { Name = "Days Of Week", Description = "", IsSpecialPreset = true });
        int presetId = (await PresetsService.GetPresetByNameAsync("days of week"))!.WeeklyPollOptionPresetId;
        int pollId = await NewPollAsync("weekly");
        await PollsService.UpdateFieldAsync(pollId, nameof(WeeklyPoll.OptionPresetId), presetId.ToString());

        WeeklyPollResource poll = Assert.Single(await PollsService.GetPollsByDayOfWeekAsync(DayOfWeek.Wednesday));

        Assert.Equal(8, poll.Options.Count);
        Assert.Equal("None of the above", poll.Options[^1]);
    }

    [Fact]
    public async Task GetOrCreateDummyPollAsync_Twice_GivesTheSameDraft()
    {
        WeeklyPollEditResource? first = await PollsService.GetOrCreateDummyPollAsync(DiscordIds.Server);
        WeeklyPollEditResource? second = await PollsService.GetOrCreateDummyPollAsync(DiscordIds.Server);

        Assert.NotNull(first);
        Assert.Equal(first.WeeklyPollId, second?.WeeklyPollId);
    }

    [Fact]
    public async Task GetPollsByServerIdAsync_LeavesOutTheDraft()
    {
        await NewPollAsync("weekly", "Yes", "No");
        await PollsService.GetOrCreateDummyPollAsync(DiscordIds.Server);

        Assert.Equal("weekly", Assert.Single(await PollsService.GetPollsByServerIdAsync(DiscordIds.Server)).Name);
    }

    [Fact]
    public async Task GetPollByNameForEditAsync_NameInAnotherCase_FindsItWithItsAnswers()
    {
        int pollId = await NewPollAsync("Weekly", "Yes", "No");

        WeeklyPollEditResource? poll = await PollsService.GetPollByNameForEditAsync(DiscordIds.Server, "WEEKLY");

        Assert.Equal(pollId, poll?.WeeklyPollId);
        Assert.Equal(2, poll?.Options.Count);
    }

    [Fact]
    public async Task UpdateAsync_SameChannelAgainAndNoRole_ReusesTheChannelAndClearsTheRole()
    {
        int pollId = await NewPollAsync("weekly");

        Assert.Equal(DbProcessResultEnum.Success, await PollsService.UpdateAsync(pollId, "weekly", "Other question", DiscordIds.Channel, null));

        WeeklyPollResource? poll = await PollsService.GetPollByIdAsync(pollId);
        Assert.NotNull(poll);
        Assert.Null(poll.RoleDiscordId);
        Assert.Equal(DiscordIds.Channel, poll.ChannelDiscordId);
        await using MainDbContext context = database.NewContext();
        Assert.Equal(1, await context.Channels.CountAsync(TestContext.Current.CancellationToken));
    }

    //Their foreign key refused the poll's removal until 2026-11-19
    [Fact]
    public async Task RemovePollByNameAsync_PollWithItsOwnAnswers_RemovesItWithThem()
    {
        await NewPollAsync("weekly", "Yes", "No");

        Assert.Equal(DbProcessResultEnum.Success, await PollsService.RemovePollByNameAsync(DiscordIds.Server, "weekly"));
        Assert.Empty(await PollsService.GetPollsByServerIdAsync(DiscordIds.Server));
        Assert.Equal(0, await AnswerRowsAsync());
    }

    //They stayed behind belonging to nothing until 2026-11-19
    [Fact]
    public async Task RemovePresetByNameAsync_UnusedPreset_RemovesItWithItsAnswers()
    {
        await NewPresetAsync("Yes or No", "Yes", "No");

        Assert.Equal(DbProcessResultEnum.Success, await PresetsService.RemovePresetByNameAsync("yes or no"));
        Assert.Null(await PresetsService.GetPresetByNameAsync("yes or no"));
        Assert.Equal(0, await AnswerRowsAsync());
    }

    [Fact]
    public async Task GetPresetByNameAsync_NameInAnotherCase_FindsItWithItsAnswers()
    {
        int presetId = await NewPresetAsync("Yes or No", "Yes", "No");

        WeeklyPollOptionPresetResource? preset = await PresetsService.GetPresetByNameAsync("YES OR NO");

        Assert.Equal(presetId, preset?.WeeklyPollOptionPresetId);
        Assert.Equal(["Yes", "No"], preset?.Options.OrderBy(option => option.OrderNumber).Select(option => option.Title));
    }

    [Fact]
    public async Task GetActivePresetsAsync_LeavesOutTheDraftAndInactiveOnes()
    {
        await NewPresetAsync("Yes or No", "Yes", "No");
        int retired = await NewPresetAsync("Retired", "Old");
        await PresetsService.UpdateFieldAsync(retired, nameof(WeeklyPollOptionPreset.IsActive), "False");
        await PresetsService.GetOrCreateDummyPresetAsync();

        Assert.Equal("Yes or No", Assert.Single(await PresetsService.GetActivePresetsAsync()).Name);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private IWeeklyPollService PollsService => host.FromNewScope<IWeeklyPollService>();

    private IWeeklyPollOptionPresetService PresetsService => host.FromNewScope<IWeeklyPollOptionPresetService>();

    //An active Wednesday poll in the test channel, pinging the test role
    private async Task<int> NewPollAsync(string name, params string[] answers)
    {
        WeeklyPollEditResource? draft = await PollsService.GetOrCreateDummyPollAsync(DiscordIds.Server);
        Assert.NotNull(draft);
        RoleResource role = new() { DiscordId = DiscordIds.Role, RoleName = "voters" };
        Assert.Equal(DbProcessResultEnum.Success, await PollsService.UpdateAsync(draft.WeeklyPollId, name, "Weekly question", DiscordIds.Channel, role));
        Assert.Equal(DbProcessResultEnum.Success, await PollsService.UpdateFieldAsync(draft.WeeklyPollId, nameof(WeeklyPoll.RepeatOnDayOfWeek), nameof(DayOfWeek.Wednesday)));
        Assert.Equal(DbProcessResultEnum.Success, await PollsService.UpdateFieldAsync(draft.WeeklyPollId, nameof(WeeklyPoll.IsActive), "True"));
        await AddAnswersAsync(isPreset: false, draft.WeeklyPollId, answers);
        return draft.WeeklyPollId;
    }

    private async Task<int> NewPresetAsync(string name, params string[] answers)
    {
        WeeklyPollOptionPresetResource? draft = await PresetsService.GetOrCreateDummyPresetAsync();
        Assert.NotNull(draft);
        Assert.Equal(DbProcessResultEnum.Success, await PresetsService.UpdateAsync(draft.WeeklyPollOptionPresetId, name, ""));
        Assert.Equal(DbProcessResultEnum.Success, await PresetsService.UpdateFieldAsync(draft.WeeklyPollOptionPresetId, nameof(WeeklyPollOptionPreset.IsActive), "True"));
        await AddAnswersAsync(isPreset: true, draft.WeeklyPollOptionPresetId, answers);
        return draft.WeeklyPollOptionPresetId;
    }

    private async Task<int> AnswerRowsAsync()
    {
        await using MainDbContext context = database.NewContext();
        return await context.WeeklyPollOptions.CountAsync(TestContext.Current.CancellationToken);
    }

    //Each answer as the edit message makes it: the button creates an empty one, the modal gives it its title
    private async Task AddAnswersAsync(bool isPreset, int ownerId, string[] answers)
    {
        for (int i = 0; i < answers.Length; i++)
        {
            WeeklyPollOptionResource? option = await host.FromNewScope<IWeeklyPollOptionService>().GetOrCreateOptionAsync(isPreset, ownerId, optionId: 0, orderNumber: (byte) (i + 1));
            Assert.NotNull(option?.WeeklyPollOptionId);
            Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IWeeklyPollOptionService>().UpdateAsync(option.WeeklyPollOptionId.Value, answers[i]));
        }
    }
}
