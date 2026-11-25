using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//A user's bias list through the real services and repositories on LocalDB: the name, alias and group lookups and the user-idol links
public sealed class BiasListTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
        await database.AddAsync(
            Group("group", Idol("idol", alias: "nickname"), Idol("member")),
            Group("other", Idol("idol")));
    }

    [Fact]
    public async Task AddUserIdolAsync_NameAndGroup_AddsThatIdol()
    {
        Assert.Equal(DbProcessResultEnum.Success, await AddAsync("idol", "group"));
        Assert.Equal(["idol>group"], await BiasesAsync());
    }

    //The bot lowercases what users type, but the database's collation would match either way
    [Fact]
    public async Task AddUserIdolAsync_InAnotherCase_FindsTheIdol()
    {
        Assert.Equal(DbProcessResultEnum.Success, await AddAsync("IDOL", "Group"));
        Assert.Equal(["idol>group"], await BiasesAsync());
    }

    [Fact]
    public async Task AddUserIdolAsync_Alias_FindsTheIdol()
    {
        Assert.Equal(DbProcessResultEnum.Success, await AddAsync("nickname", ""));
        Assert.Equal(["idol>group"], await BiasesAsync());
    }

    [Fact]
    public async Task AddUserIdolAsync_NameOfIdolsInTwoGroups_ReturnsMultipleResults()
    {
        Assert.Equal(DbProcessResultEnum.MultipleResults, await AddAsync("idol", ""));
        Assert.Empty(await BiasesAsync());
    }

    [Fact]
    public async Task AddUserIdolAsync_Twice_ReturnsAlreadyExists()
    {
        await AddAsync("idol", "group");

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await AddAsync("idol", "group"));
    }

    [Fact]
    public async Task RemoveUserIdolAsync_NameAndGroup_RemovesThatBias()
    {
        await AddAsync("idol", "group");
        await AddAsync("idol", "other");

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IUserIdolService>().RemoveUserIdolAsync(DiscordIds.User, "idol", "group"));
        Assert.Equal(["idol>other"], await BiasesAsync());
    }

    [Fact]
    public async Task RemoveUserIdolAsync_NameOfTwoBiases_ReturnsMultipleResults()
    {
        await AddAsync("idol", "group");
        await AddAsync("idol", "other");

        Assert.Equal(DbProcessResultEnum.MultipleResults, await host.FromNewScope<IUserIdolService>().RemoveUserIdolAsync(DiscordIds.User, "idol", ""));
        Assert.Equal(2, (await BiasesAsync()).Count);
    }

    [Fact]
    public async Task AddUserIdolGroupAsync_ThenRemoveUserIdolGroupAsync_LeavesTheOtherGroupsBiases()
    {
        await AddAsync("idol", "other");

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IUserIdolService>().AddUserIdolGroupAsync(DiscordIds.User, "group"));
        Assert.Equal(["idol>group", "idol>other", "member>group"], await BiasesAsync());

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IUserIdolService>().RemoveUserIdolGroupAsync(DiscordIds.User, "group"));
        Assert.Equal(["idol>other"], await BiasesAsync());
    }

    [Fact]
    public async Task ClearUserIdolAsync_EmptiesTheList()
    {
        await AddAsync("idol", "group");
        await AddAsync("member", "group");

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IUserIdolService>().ClearUserIdolAsync(DiscordIds.User));
        Assert.Empty(await BiasesAsync());
    }

    [Fact]
    public async Task GetUserIdolsListAsync_WithAGroup_GivesThatGroupsBiasesByName()
    {
        await AddAsync("member", "group");
        await AddAsync("idol", "other");
        await AddAsync("idol", "group");

        List<IdolResource> biases = await host.FromNewScope<IUserIdolService>().GetUserIdolsListAsync(DiscordIds.User, "group");

        Assert.Equal(["idol", "member"], biases.Select(idol => idol.Name));
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private Task<DbProcessResultEnum> AddAsync(string name, string group)
    {
        return host.FromNewScope<IUserIdolService>().AddUserIdolAsync(DiscordIds.User, name, group);
    }

    //The user's biases as name>group, sorted
    private async Task<List<string>> BiasesAsync()
    {
        List<IdolResource> biases = await host.FromNewScope<IUserIdolService>().GetUserIdolsListAsync(DiscordIds.User, "");
        return biases.Select(idol => $"{idol.Name}>{idol.GroupName}").Order(StringComparer.Ordinal).ToList();
    }

    private static IdolGroup Group(string name, params List<Idol> idols)
    {
        return new IdolGroup { Name = name, Idols = idols };
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
}
