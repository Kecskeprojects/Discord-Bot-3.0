using DatabaseORM.Caching;
using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

public sealed class IdolGroupServiceTests
{
    private const int GroupId = 9;

    private readonly IIdolGroupRepository groups = Substitute.For<IIdolGroupRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly IdolGroupService service;

    private readonly IdolGroup group = Entities.Group("group", GroupId);

    public IdolGroupServiceTests()
    {
        service = new IdolGroupService(groups, TestMapper.Database, logger, new ServerCache());
        groups.HasRows(Entities.Group("other", 10), group);
        groups.FindByIdAsync(GroupId).Returns(group);
    }

    [Fact]
    public async Task GetIdolGroupDetailsAsync_KnownGroup_GivesItsDetails()
    {
        group.FullName = "Group Full";

        IdolGroupExtendedResource? details = await service.GetIdolGroupDetailsAsync("group");

        Assert.NotNull(details);
        Assert.Equal(GroupId, details.GroupId);
        Assert.Equal("Group Full", details.FullName);
    }

    [Fact]
    public async Task GetIdolGroupDetailsAsync_Unknown_GivesNull()
    {
        Assert.Null(await service.GetIdolGroupDetailsAsync("nowhere"));
    }

    [Theory]
    [InlineData("group", true)]
    [InlineData("nowhere", false)]
    public async Task GroupExistsAsnyc_TellsWhetherTheGroupIsThere(string name, bool expected)
    {
        Assert.Equal(expected, await service.GroupExistsAsnyc(name));
    }

    [Fact]
    public async Task GroupExistsAsnyc_DatabaseFails_LogsTheErrorAndSaysNo()
    {
        groups.Fails();

        Assert.False(await service.GroupExistsAsnyc("group"));
        logger.Received().Error("IdolGroupService.cs GroupExistsAsnyc", Arg.Any<Exception>());
    }

    [Fact]
    public async Task UpdateAsync_SavesTheTrimmedDetailsWithALowercaseName()
    {
        IdolGroupEditData data = new() { Name = " New Name ", FullName = " Full Name ", FullKoreanName = " Korean Name ", DebutDate = "2015. 04. 22." };

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(GroupId, data));
        Assert.Equal("new name", group.Name);
        Assert.Equal("Full Name", group.FullName);
        Assert.Equal("Korean Name", group.FullKoreanName);
        Assert.Equal(new DateOnly(2015, 4, 22), group.DebutDate);
        await groups.Received().SaveChangesAsync();
    }

    //The edit modal shows dates as "yyyy. MM. dd.", which some cultures (such as German) can't read
    [Theory]
    [InlineData("hu-HU")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    public async Task UpdateAsync_DebutInTheModalsFormat_IsReadInAnyCulture(string culture)
    {
        using (new CultureScope(culture))
        {
            Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(GroupId, Data("2015. 04. 22.")));
        }

        Assert.Equal(new DateOnly(2015, 4, 22), group.DebutDate);
    }

    [Fact]
    public async Task UpdateAsync_UnreadableDebut_KeepsTheOldOne()
    {
        group.DebutDate = new DateOnly(2015, 4, 22);

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(GroupId, Data("soon")));
        Assert.Equal(new DateOnly(2015, 4, 22), group.DebutDate);
    }

    [Fact]
    public async Task UpdateAsync_UnknownGroup_ReturnsNotFound()
    {
        groups.FindByIdAsync(99).Returns((IdolGroup?) null);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.UpdateAsync(99, Data("2015. 04. 22.")));
    }

    private static IdolGroupEditData Data(string debutDate)
    {
        return new IdolGroupEditData { Name = "group", DebutDate = debutDate };
    }
}
