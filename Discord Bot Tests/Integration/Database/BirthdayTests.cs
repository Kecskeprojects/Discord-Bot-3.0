using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Integration.Database;

//Birthdays through the real services and repositories on LocalDB: the day-of-year query and the server's list order
public sealed class BirthdayTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
    }

    //TestTimes.Now is 2026-11-18
    [Fact]
    public async Task GetBirthdaysByDateAsync_FindsTheBirthdayOnItsDayInAnyYear()
    {
        await AddAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 11, 18));

        BirthdayResource today = Assert.Single(await host.FromNewScope<IBirthdayService>().GetBirthdaysByDateAsync(TestTimes.Now.UtcDateTime));
        Assert.Equal(DiscordIds.User, today.UserDiscordId);
        Assert.Equal(DiscordIds.Server, today.ServerDiscordId);
        Assert.Empty(await host.FromNewScope<IBirthdayService>().GetBirthdaysByDateAsync(TestTimes.Now.UtcDateTime.AddDays(1)));
    }

    [Fact]
    public async Task AddBirthdayAsync_SameUserAgain_ChangesTheDay()
    {
        await AddAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 3, 4));

        Assert.Equal(DbProcessResultEnum.UpdatedExisting, await host.FromNewScope<IBirthdayService>().AddBirthdayAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 3, 5)));
        Assert.Equal(new DateOnly(2000, 3, 5), Assert.Single(await ListAsync(DiscordIds.Server)).Date);
    }

    [Fact]
    public async Task GetServerBirthdayListAsync_IsInMonthThenDayThenYearOrder()
    {
        await AddAsync(DiscordIds.Server, DiscordIds.User, new DateTime(1990, 12, 1));
        await AddAsync(DiscordIds.Server, DiscordIds.OtherUser, new DateTime(2000, 3, 4));
        await AddAsync(DiscordIds.Server, DiscordIds.ThirdUser, new DateTime(1995, 3, 4));

        List<BirthdayResource> list = await ListAsync(DiscordIds.Server);

        Assert.Equal([DiscordIds.ThirdUser, DiscordIds.OtherUser, DiscordIds.User], list.Select(birthday => birthday.UserDiscordId));
    }

    [Fact]
    public async Task RemoveBirthdayAsync_RemovesOnlyThatServersBirthday()
    {
        await AddAsync(DiscordIds.Server, DiscordIds.User, new DateTime(2000, 3, 4));
        await AddAsync(DiscordIds.OtherServer, DiscordIds.User, new DateTime(2000, 3, 4));

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IBirthdayService>().RemoveBirthdayAsync(DiscordIds.Server, DiscordIds.User));
        Assert.Empty(await ListAsync(DiscordIds.Server));
        Assert.Single(await ListAsync(DiscordIds.OtherServer));
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task AddAsync(ulong serverId, ulong userId, DateTime date)
    {
        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IBirthdayService>().AddBirthdayAsync(serverId, userId, date));
    }

    private Task<List<BirthdayResource>> ListAsync(ulong serverId)
    {
        return host.FromNewScope<IBirthdayService>().GetServerBirthdayListAsync(serverId);
    }
}
