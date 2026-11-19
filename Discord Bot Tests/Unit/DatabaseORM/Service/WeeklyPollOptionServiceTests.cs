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

//An option belongs to a poll (its own answers) or to a preset (answers several polls share)
public sealed class WeeklyPollOptionServiceTests
{
    private const int PollId = 21;
    private const int PresetId = 4;

    private readonly IWeeklyPollOptionRepository options = Substitute.For<IWeeklyPollOptionRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly WeeklyPollOptionService service;

    public WeeklyPollOptionServiceTests()
    {
        service = new WeeklyPollOptionService(options, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task GetOrCreateOptionAsync_NewPollOption_AddsAnEmptyOneForThePoll()
    {
        List<WeeklyPollOption> table = options.HasRows();

        WeeklyPollOptionResource? option = await service.GetOrCreateOptionAsync(isPresetOption: false, PollId, optionId: 0, orderNumber: 3);

        WeeklyPollOption added = Assert.Single(table);
        Assert.Equal(PollId, added.WeeklyPollId);
        Assert.Null(added.WeeklyPollOptionPresetId);
        Assert.Equal(3, added.OrderNumber);
        Assert.Equal("", added.Title);
        Assert.NotNull(option);
        Assert.Equal(PollId, option.WeeklyPollId);
    }

    [Fact]
    public async Task GetOrCreateOptionAsync_NewPresetOption_AddsItForThePreset()
    {
        List<WeeklyPollOption> table = options.HasRows();

        await service.GetOrCreateOptionAsync(isPresetOption: true, PresetId, optionId: 0, orderNumber: 1);

        WeeklyPollOption added = Assert.Single(table);
        Assert.Null(added.WeeklyPollId);
        Assert.Equal(PresetId, added.WeeklyPollOptionPresetId);
    }

    [Fact]
    public async Task GetOrCreateOptionAsync_ExistingOption_GivesItWithoutAddingAnother()
    {
        List<WeeklyPollOption> table = options.HasRows(new WeeklyPollOption { WeeklyPollOptionId = 8, WeeklyPollId = PollId, OrderNumber = 2, Title = "Yes" });

        WeeklyPollOptionResource? option = await service.GetOrCreateOptionAsync(isPresetOption: false, PollId, optionId: 8, orderNumber: 2);

        Assert.NotNull(option);
        Assert.Equal("Yes", option.Title);
        Assert.Single(table);
    }

    [Fact]
    public async Task GetOrCreateOptionAsync_DatabaseFails_LogsTheErrorAndGivesNull()
    {
        options.Fails();

        Assert.Null(await service.GetOrCreateOptionAsync(isPresetOption: false, PollId, optionId: 0, orderNumber: 1));
        logger.Received().Error("WeeklyPollOptionService.cs GetOrCreateOptionAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task UpdateAsync_ChangesTheTitle()
    {
        WeeklyPollOption option = new() { WeeklyPollOptionId = 8, WeeklyPollId = PollId, Title = "" };
        options.HasRows(option);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(8, "Yes"));
        Assert.Equal("Yes", option.Title);
        await options.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsync_UnknownOption_ReturnsNotFound()
    {
        options.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.UpdateAsync(8, "Yes"));
    }
}
