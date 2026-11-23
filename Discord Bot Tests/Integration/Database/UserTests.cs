using DatabaseORM.Context;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Microsoft.EntityFrameworkCore;

namespace Discord_Bot.Tests.Integration.Database;

//Users and their Last.fm names through the real services and repositories on LocalDB (!lastfm set and !lastfm unset)
public sealed class UserTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
    }

    //A user the bot hasn't seen yet gets a row of their own
    [Fact]
    public async Task AddLastfmUsernameAsync_NewUser_AddsThemWithTheName()
    {
        Assert.Equal(DbProcessResultEnum.Success, await Service().AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername));

        Assert.Equal(TestValues.LastFmUsername, (await Service().GetUserAsync(DiscordIds.User))?.LastFmUsername);
        await using MainDbContext context = database.NewContext();
        Assert.Equal(1, await context.Users.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddLastfmUsernameAsync_KnownUserWithoutAName_GivesThemTheName()
    {
        await database.AddAsync(Entities.User(RowIds.New));

        Assert.Equal(DbProcessResultEnum.Success, await Service().AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername));
        Assert.Equal(TestValues.LastFmUsername, (await Service().GetUserAsync(DiscordIds.User))?.LastFmUsername);
    }

    [Fact]
    public async Task AddLastfmUsernameAsync_UserWithAName_ReturnsAlreadyExistsAndKeepsIt()
    {
        await Service().AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername);

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await Service().AddLastfmUsernameAsync(DiscordIds.User, "someone-else"));
        Assert.Equal(TestValues.LastFmUsername, (await Service().GetUserAsync(DiscordIds.User))?.LastFmUsername);
    }

    [Fact]
    public async Task GetAllLastFmUsersAsync_LeavesOutUsersWithoutAName()
    {
        User withoutName = Entities.User(RowIds.New, DiscordIds.OtherUser);
        User withEmptyName = Entities.User(RowIds.New, DiscordIds.ThirdUser);
        withEmptyName.LastFmusername = "";
        await database.AddAsync(withoutName, withEmptyName);
        await Service().AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername);

        Assert.Equal([DiscordIds.User], (await Service().GetAllLastFmUsersAsync()).Select(user => user.DiscordId));
    }

    [Fact]
    public async Task RemoveLastfmUsernameAsync_UserWithAName_ClearsIt()
    {
        await Service().AddLastfmUsernameAsync(DiscordIds.User, TestValues.LastFmUsername);

        Assert.Equal(DbProcessResultEnum.Success, await Service().RemoveLastfmUsernameAsync(DiscordIds.User));
        Assert.Null((await Service().GetUserAsync(DiscordIds.User))?.LastFmUsername);
        Assert.Equal(DbProcessResultEnum.NotFound, await Service().RemoveLastfmUsernameAsync(DiscordIds.User));
    }

    [Fact]
    public async Task UnknownUser_IsNotFound()
    {
        Assert.Null(await Service().GetUserAsync(DiscordIds.User));
        Assert.Equal(DbProcessResultEnum.NotFound, await Service().RemoveLastfmUsernameAsync(DiscordIds.User));
    }

    private IUserService Service()
    {
        return host.FromNewScope<IUserService>();
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }
}
