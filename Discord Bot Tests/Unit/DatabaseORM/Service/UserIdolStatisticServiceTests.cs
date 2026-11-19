using DatabaseORM.Caching;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

//The places a user's bias game results give each idol
public sealed class UserIdolStatisticServiceTests
{
    private readonly IUserIdolStatisticRepository statistics = Substitute.For<IUserIdolStatisticRepository>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly UserIdolStatisticService service;

    private readonly User user = Entities.User();

    public UserIdolStatisticServiceTests()
    {
        service = new UserIdolStatisticService(statistics, users, TestMapper.Database, logger, new ServerCache());
    }

    [Fact]
    public async Task UpdateUserStatisticsAsync_CountsEachIdolsPlaceAndTheGame()
    {
        users.HasRows(user);
        UserIdolStatistic winnerBefore = new() { IdolId = 1, UserId = user.UserId, User = user, Placed1 = 2 };
        List<UserIdolStatistic> table = statistics.HasRows(winnerBefore);

        //The winner is on top of the stack
        await service.UpdateUserStatisticsAsync(DiscordIds.User, Ranking(3, 2, 1));

        Assert.Equal(3, winnerBefore.Placed1);
        Assert.Equal(1, table.Single(stat => stat.IdolId == 2).Placed2);
        Assert.Equal(1, table.Single(stat => stat.IdolId == 3).Placed3);
        Assert.Equal(1, user.BiasGameCount);
    }

    [Fact]
    public async Task UpdateUserStatisticsAsync_FullGame_GroupsTheLowerPlaces()
    {
        users.HasRows(user);
        List<UserIdolStatistic> table = statistics.HasRows();

        await service.UpdateUserStatisticsAsync(DiscordIds.User, Ranking([.. Enumerable.Range(1, 16).Reverse()]));

        Assert.Equal(16, table.Count);
        Assert.Equal(1, table.Single(stat => stat.IdolId == 4).Placed4);
        Assert.Equal(1, table.Single(stat => stat.IdolId == 7).Placed4);
        Assert.Equal(1, table.Single(stat => stat.IdolId == 8).Placed5);
        Assert.Equal(1, table.Single(stat => stat.IdolId == 16).Placed5);
    }

    [Fact]
    public async Task UpdateUserStatisticsAsync_NewUser_AddsThem()
    {
        List<User> userTable = users.HasRows();
        statistics.HasRows();

        await service.UpdateUserStatisticsAsync(DiscordIds.User, Ranking(1));

        User added = Assert.Single(userTable);
        Assert.Equal(DiscordIds.User.ToString(), added.DiscordId);
        Assert.Equal(1, added.BiasGameCount);
    }

    [Fact]
    public async Task UpdateUserStatisticsAsync_DatabaseFails_LogsTheError()
    {
        users.Fails();

        await service.UpdateUserStatisticsAsync(DiscordIds.User, Ranking(1));

        logger.Received().Error("UserIdolStatisticService.cs UpdateUserStatisticsAsync", Arg.Any<Exception>());
    }

    //Pushed in this order, so the last one is popped first
    private static Stack<int> Ranking(params int[] idolIds)
    {
        return new Stack<int>(idolIds);
    }
}
