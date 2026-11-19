using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

public sealed class IdolAliasServiceTests
{
    private readonly IIdolAliasRepository aliases = Substitute.For<IIdolAliasRepository>();
    private readonly IIdolRepository idols = Substitute.For<IIdolRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly IdolAliasService service;

    private readonly Idol idol = Entities.Idol("idol", Entities.Group("group"));
    private readonly Idol sameNameElsewhere = Entities.Idol("idol", Entities.Group("other"));

    public IdolAliasServiceTests()
    {
        service = new IdolAliasService(aliases, idols, TestMapper.Database, logger, new ServerCache());
        idols.HasRows(sameNameElsewhere, idol);
    }

    [Fact]
    public async Task AddIdolAliasAsync_NewAlias_AddsItToThatIdol()
    {
        aliases.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddIdolAliasAsync("nickname", "idol", "group"));
        Assert.Equal("nickname", Assert.Single(idol.IdolAliases).Alias);
        Assert.Empty(sameNameElsewhere.IdolAliases);
        await idols.Received().UpdateAsync(idol);
    }

    [Fact]
    public async Task AddIdolAliasAsync_AliasTheIdolHas_ReturnsAlreadyExists()
    {
        aliases.HasRows(Alias(idol, "nickname"));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddIdolAliasAsync("nickname", "idol", "group"));
        Assert.Single(idol.IdolAliases);
    }

    [Fact]
    public async Task AddIdolAliasAsync_UnknownIdol_ReturnsNotFound()
    {
        aliases.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.AddIdolAliasAsync("nickname", "idol", "nowhere"));
    }

    [Fact]
    public async Task AddIdolAliasAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        aliases.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddIdolAliasAsync("nickname", "idol", "group"));
        logger.Received().Error("IdolAliasService.cs AddIdolAliasAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task RemoveIdolAliasAsync_RemovesThatIdolsAlias()
    {
        IdolAlias elsewhere = Alias(sameNameElsewhere, "nickname");
        List<IdolAlias> table = aliases.HasRows(Alias(idol, "nickname"), elsewhere);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveIdolAliasAsync("nickname", "idol", "group"));
        Assert.Same(elsewhere, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveIdolAliasAsync_Unknown_ReturnsNotFound()
    {
        aliases.HasRows(Alias(sameNameElsewhere, "nickname"));

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveIdolAliasAsync("nickname", "idol", "group"));
    }

    private static IdolAlias Alias(Idol aliasIdol, string alias)
    {
        IdolAlias idolAlias = new() { Alias = alias, IdolId = aliasIdol.IdolId, Idol = aliasIdol };
        aliasIdol.IdolAliases.Add(idolAlias);
        return idolAlias;
    }
}
