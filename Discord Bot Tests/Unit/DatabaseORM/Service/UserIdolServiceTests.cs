using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

//A user's biases are the idols on their list (User.Idols, Idol.Users)
public sealed class UserIdolServiceTests
{
    private readonly IIdolRepository idols = Substitute.For<IIdolRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly UserIdolService service;

    private readonly User user = Entities.User();
    private readonly IdolGroup group = new() { GroupId = 1, Name = "group" };
    private readonly IdolGroup otherGroup = new() { GroupId = 2, Name = "other" };

    public UserIdolServiceTests()
    {
        service = new UserIdolService(idols, users, TestMapper.Database, logger, new ServerCache());
        users.HasRows(user);
    }

    [Fact]
    public async Task GetUserIdolsListAsync_WithAGroup_GivesTheUsersBiasesInItByName()
    {
        idols.HasRows(Idol("b", group, isBias: true), Idol("c", otherGroup, isBias: true), Idol("d", group, isBias: false), Idol("a", group, isBias: true));

        List<IdolResource> result = await service.GetUserIdolsListAsync(DiscordIds.User, "group");

        Assert.Equal(["a", "b"], result.Select(idol => idol.Name));
    }

    [Fact]
    public async Task AddUserIdolAsync_OneMatch_AddsIt()
    {
        Idol idol = Idol("idol", group, isBias: false);
        idols.GetListByNamesAsync("idol", "group").Returns([idol]);
        idols.GetListByNamesAsync("idol", "group", DiscordIds.User.ToString()).Returns([]);

        Assert.Equal(DbProcessResultEnum.Success, await service.AddUserIdolAsync(DiscordIds.User, "idol", "group"));
        Assert.Same(idol, Assert.Single(user.Idols));
        await users.Received().UpdateAsync(user);
    }

    [Fact]
    public async Task AddUserIdolAsync_NoMatch_ReturnsNotFound()
    {
        idols.GetListByNamesAsync("idol", "").Returns([]);

        Assert.Equal(DbProcessResultEnum.NotFound, await service.AddUserIdolAsync(DiscordIds.User, "idol", ""));
    }

    [Fact]
    public async Task AddUserIdolAsync_IdolsInSeveralGroups_ReturnsMultipleResults()
    {
        idols.GetListByNamesAsync("idol", "").Returns([Idol("idol", group, isBias: false), Idol("idol", otherGroup, isBias: false)]);

        Assert.Equal(DbProcessResultEnum.MultipleResults, await service.AddUserIdolAsync(DiscordIds.User, "idol", ""));
        Assert.Empty(user.Idols);
    }

    [Fact]
    public async Task AddUserIdolAsync_AlreadyABias_ReturnsAlreadyExists()
    {
        Idol idol = Idol("idol", group, isBias: true);
        idols.GetListByNamesAsync("idol", "group").Returns([idol]);
        idols.GetListByNamesAsync("idol", "group", DiscordIds.User.ToString()).Returns([idol]);

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddUserIdolAsync(DiscordIds.User, "idol", "group"));
        Assert.Single(user.Idols);
    }

    [Fact]
    public async Task AddUserIdolGroupAsync_AddsTheGroupsIdolsTheUserHasNotGot()
    {
        Idol bias = Idol("bias", group, isBias: true);
        Idol newOne = Idol("new", group, isBias: false);
        idols.HasRows(bias, newOne, Idol("elsewhere", otherGroup, isBias: false));

        Assert.Equal(DbProcessResultEnum.Success, await service.AddUserIdolGroupAsync(DiscordIds.User, "group"));
        Assert.Equal([bias, newOne], user.Idols);
    }

    [Fact]
    public async Task AddUserIdolGroupAsync_WholeGroupAlreadyBiases_ReturnsAlreadyExists()
    {
        idols.HasRows(Idol("first", group, isBias: true), Idol("second", group, isBias: true));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddUserIdolGroupAsync(DiscordIds.User, "group"));
    }

    [Fact]
    public async Task ClearUserIdolAsync_EmptiesTheList()
    {
        Idol("idol", group, isBias: true);

        Assert.Equal(DbProcessResultEnum.Success, await service.ClearUserIdolAsync(DiscordIds.User));
        Assert.Empty(user.Idols);
        await users.Received().UpdateAsync(user);
    }

    [Fact]
    public async Task RemoveUserIdolAsync_NameAndGroup_RemovesThatBias()
    {
        Idol sameName = Idol("idol", otherGroup, isBias: true);
        idols.HasRows(Idol("idol", group, isBias: true), sameName);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveUserIdolAsync(DiscordIds.User, "idol", "group"));
        Assert.Same(sameName, Assert.Single(user.Idols));
        await users.Received().UpdateAsync(user);
    }

    [Fact]
    public async Task RemoveUserIdolAsync_NameOfTwoBiases_ReturnsMultipleResults()
    {
        idols.HasRows(Idol("idol", group, isBias: true), Idol("idol", otherGroup, isBias: true));

        Assert.Equal(DbProcessResultEnum.MultipleResults, await service.RemoveUserIdolAsync(DiscordIds.User, "idol", ""));
        Assert.Equal(2, user.Idols.Count);
    }

    [Fact]
    public async Task RemoveUserIdolAsync_NotABias_ReturnsPartialNotFound()
    {
        idols.HasRows(Idol("idol", group, isBias: false));

        Assert.Equal(DbProcessResultEnum.PartialNotFound, await service.RemoveUserIdolAsync(DiscordIds.User, "idol", "group"));
    }

    [Fact]
    public async Task RemoveUserIdolAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        users.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.RemoveUserIdolAsync(DiscordIds.User, "idol", "group"));
        logger.Received().Error("UserIdolService.cs RemoveUserIdolAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task RemoveUserIdolGroupAsync_RemovesOnlyThatGroupsBiases()
    {
        Idol elsewhere = Idol("elsewhere", otherGroup, isBias: true);
        idols.HasRows(Idol("first", group, isBias: true), Idol("second", group, isBias: true), elsewhere);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveUserIdolGroupAsync(DiscordIds.User, "group"));
        Assert.Same(elsewhere, Assert.Single(user.Idols));
    }

    [Fact]
    public async Task RemoveUserIdolGroupAsync_NoBiasInTheGroup_ReturnsPartialNotFound()
    {
        idols.HasRows(Idol("idol", group, isBias: false));

        Assert.Equal(DbProcessResultEnum.PartialNotFound, await service.RemoveUserIdolGroupAsync(DiscordIds.User, "group"));
    }

    //Puts it on the user's list too when it's a bias, as the database returns both sides
    private Idol Idol(string name, IdolGroup idolGroup, bool isBias)
    {
        Idol idol = new() { Name = name, GroupId = idolGroup.GroupId, Group = idolGroup };
        if (isBias)
        {
            idol.Users.Add(user);
            user.Idols.Add(idol);
        }
        return idol;
    }
}
