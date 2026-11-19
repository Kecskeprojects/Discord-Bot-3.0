using DatabaseORM.Model;
using DatabaseORM.Tools;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Tools;

public sealed class UserIdolStatisticToolsTests
{
    //Places 1 to 3 have their own counts; 4 to 7 (the quarter-finals) and 8 to 16 (the first round) share one each
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(7, 4)]
    [InlineData(8, 5)]
    [InlineData(16, 5)]
    public void AddRanking_CountsThePlaceInItsColumn(int place, int column)
    {
        UserIdolStatistic stat = new();

        UserIdolStatisticTools.AddRanking(stat, place);

        int[] expected = [0, 0, 0, 0, 0];
        expected[column - 1] = 1;
        int[] counts = [stat.Placed1, stat.Placed2, stat.Placed3, stat.Placed4, stat.Placed5];
        Assert.Equal(expected, counts);
    }

    [Fact]
    public void AddRanking_PlaceBeyondTheGame_CountsNothing()
    {
        UserIdolStatistic stat = new();

        UserIdolStatisticTools.AddRanking(stat, 17);

        Assert.Equal(0, stat.Placed1 + stat.Placed2 + stat.Placed3 + stat.Placed4 + stat.Placed5);
    }
}
