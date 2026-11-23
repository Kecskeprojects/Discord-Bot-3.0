using DatabaseORM.Communication;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//!ping's query through the real services and repositories on LocalDB: the fans of idols found by name, name-group, alias or group
public sealed class BiasPingTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        await database.AddAsync(
            Group("group", Idol("idol", alias: "nickname"), Idol("member"), Idol("loner")),
            Group("other", Idol("idol")));
        await LikesAsync(DiscordIds.User, "idol", "group");
        await LikesAsync(DiscordIds.OtherUser, "member", "group");
        await LikesAsync(DiscordIds.ThirdUser, "idol", "other");
    }

    [Theory]
    [InlineData("idol", new[] { DiscordIds.User, DiscordIds.ThirdUser })]
    [InlineData("idol-group", new[] { DiscordIds.User })]
    [InlineData("idol-other", new[] { DiscordIds.ThirdUser })]
    [InlineData("nickname", new[] { DiscordIds.User })]
    [InlineData("group", new[] { DiscordIds.User, DiscordIds.OtherUser })]
    public async Task GetUsersByIdolsAsync_FindsTheFans(string name, ulong[] fans)
    {
        ListWithDbResult<UserResource> result = await host.FromNewScope<IIdolService>().GetUsersByIdolsAsync([name]);

        Assert.Equal(DbProcessResultEnum.Success, result.ProcessResultEnum);
        Assert.Equal(fans, result.List.Select(user => user.DiscordId).Order());
    }

    [Fact]
    public async Task GetUsersByIdolsAsync_SeveralNames_GivesEachFanOnce()
    {
        ListWithDbResult<UserResource> result = await host.FromNewScope<IIdolService>().GetUsersByIdolsAsync(["idol-group", "group", "nickname"]);

        Assert.Equal([DiscordIds.User, DiscordIds.OtherUser], result.List.Select(user => user.DiscordId).Order());
    }

    //!ping then says "Not even you have this bias"
    [Fact]
    public async Task GetUsersByIdolsAsync_IdolNobodyLikes_GivesNoFans()
    {
        ListWithDbResult<UserResource> result = await host.FromNewScope<IIdolService>().GetUsersByIdolsAsync(["loner"]);

        Assert.Equal(DbProcessResultEnum.Success, result.ProcessResultEnum);
        Assert.Empty(result.List);
    }

    [Fact]
    public async Task GetUsersByIdolsAsync_UnknownName_ReturnsNotFound()
    {
        Assert.Equal(DbProcessResultEnum.NotFound, (await host.FromNewScope<IIdolService>().GetUsersByIdolsAsync(["nobody"])).ProcessResultEnum);
    }

    private async Task LikesAsync(ulong userId, string name, string group)
    {
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IUserIdolService>().AddUserIdolAsync(userId, name, group));
    }

    private static IdolGroup Group(string name, params Idol[] idols)
    {
        return new IdolGroup { Name = name, Idols = [.. idols] };
    }

    private static Idol Idol(string name, string? alias = null)
    {
        Idol idol = new() { Name = name };
        if (alias != null)
        {
            idol.IdolAliases.Add(new IdolAlias { Alias = alias });
        }
        return idol;
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }
}
