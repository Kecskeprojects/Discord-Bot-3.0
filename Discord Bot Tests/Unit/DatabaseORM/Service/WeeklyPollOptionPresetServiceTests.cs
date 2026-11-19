using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

//Presets are answer lists that several polls share; the nameless one is the draft the create form edits
public sealed class WeeklyPollOptionPresetServiceTests
{
    private readonly IWeeklyPollOptionPresetRepository presets = Substitute.For<IWeeklyPollOptionPresetRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly WeeklyPollOptionPresetService service;

    private readonly WeeklyPollOptionPreset yesOrNo = Preset(4, "Yes or No", isActive: true, "Yes", "No");
    private readonly WeeklyPollOptionPreset retired = Preset(5, "Retired", isActive: false, "Old");

    public WeeklyPollOptionPresetServiceTests()
    {
        service = new WeeklyPollOptionPresetService(presets, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task GetActivePresetsAsync_LeavesOutTheInactiveOnes()
    {
        presets.HasRows(yesOrNo, retired);

        Assert.Equal("Yes or No", Assert.Single(await service.GetActivePresetsAsync()).Name);
    }

    [Fact]
    public async Task GetActivePresetsAsync_DatabaseFails_LogsTheErrorAndGivesNone()
    {
        presets.Fails();

        Assert.Empty(await service.GetActivePresetsAsync());
        logger.Received().Error("WeeklyPollOptionPresetService.cs GetActivePresetsAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetOrCreateDummyPresetAsync_NoneYet_AddsAnInactiveNamelessOne()
    {
        List<WeeklyPollOptionPreset> table = presets.HasRows(yesOrNo);

        WeeklyPollOptionPresetResource? dummy = await service.GetOrCreateDummyPresetAsync();

        Assert.NotNull(dummy);
        WeeklyPollOptionPreset added = table.Single(preset => preset != yesOrNo);
        Assert.Equal("", added.Name);
        Assert.False(added.IsActive);
        Assert.False(added.IsSpecialPreset);
    }

    [Fact]
    public async Task GetOrCreateDummyPresetAsync_DraftThere_GivesItWithItsOptions()
    {
        List<WeeklyPollOptionPreset> table = presets.HasRows(yesOrNo, Preset(6, "", isActive: false, "Draft answer"));

        WeeklyPollOptionPresetResource? dummy = await service.GetOrCreateDummyPresetAsync();

        Assert.NotNull(dummy);
        Assert.Equal(6, dummy.WeeklyPollOptionPresetId);
        Assert.Equal("Draft answer", Assert.Single(dummy.Options).Title);
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public async Task GetPresetByIdAsync_GivesThePresetWithItsOptions()
    {
        presets.HasRows(retired, yesOrNo);

        WeeklyPollOptionPresetResource? preset = await service.GetPresetByIdAsync(4);

        Assert.NotNull(preset);
        Assert.Equal(["Yes", "No"], preset.Options.Select(option => option.Title));
    }

    [Fact]
    public async Task GetPresetByNameAsync_NameInAnotherCase_FindsIt()
    {
        presets.HasRows(retired, yesOrNo);

        WeeklyPollOptionPresetResource? preset = await service.GetPresetByNameAsync("YES OR NO");

        Assert.NotNull(preset);
        Assert.Equal(4, preset.WeeklyPollOptionPresetId);
    }

    [Fact]
    public async Task GetPresetsAsync_GivesActiveAndInactiveOnes()
    {
        presets.HasRows(yesOrNo, retired);

        Assert.Equal(2, (await service.GetPresetsAsync()).Count);
    }

    [Fact]
    public async Task RemovePresetByNameAsync_NameInAnotherCase_RemovesIt()
    {
        List<WeeklyPollOptionPreset> table = presets.HasRows(yesOrNo, retired);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemovePresetByNameAsync("retired"));
        Assert.Same(yesOrNo, Assert.Single(table));
    }

    [Fact]
    public async Task RemovePresetByNameAsync_Unknown_ReturnsNotFound()
    {
        presets.HasRows(yesOrNo);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemovePresetByNameAsync("nowhere"));
    }

    [Fact]
    public async Task UpdateAsync_ChangesTheNameAndDescription()
    {
        presets.HasRows(yesOrNo);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(4, "Agree?", "Two answers"));
        Assert.Equal("Agree?", yesOrNo.Name);
        Assert.Equal("Two answers", yesOrNo.Description);
    }

    [Fact]
    public async Task UpdateAsync_UnknownPreset_ReturnsNotFound()
    {
        presets.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.UpdateAsync(4, "Agree?", ""));
    }

    //The buttons on the edit message send the field's name and its new value as text
    [Fact]
    public async Task UpdateFieldAsync_TrueOrFalseField_ConvertsTheText()
    {
        presets.HasRows(retired);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateFieldAsync(5, nameof(WeeklyPollOptionPreset.IsActive), "True"));
        Assert.True(retired.IsActive);
        await presets.Received().UpdateAsync(retired);
    }

    [Fact]
    public async Task UpdateFieldAsync_UnknownField_LogsTheErrorAndReturnsFailure()
    {
        presets.HasRows(retired);

        Assert.Equal(DbProcessResultEnum.Failure, await service.UpdateFieldAsync(5, "Colour", "red"));
        logger.Received().Error("WeeklyPollOptionPresetService.cs UpdateFieldAsync", Arg.Any<Exception>());
    }

    private static WeeklyPollOptionPreset Preset(int id, string name, bool isActive, params string[] answers)
    {
        WeeklyPollOptionPreset preset = new() { WeeklyPollOptionPresetId = id, Name = name, Description = "", IsActive = isActive };
        foreach ((string answer, int index) in answers.Select((answer, index) => (answer, index)))
        {
            preset.WeeklyPollOptions.Add(new WeeklyPollOption { WeeklyPollOptionPresetId = id, OrderNumber = (byte) (index + 1), Title = answer });
        }
        return preset;
    }
}
