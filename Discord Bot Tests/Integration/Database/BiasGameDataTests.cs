using DatabaseORM.Context;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Microsoft.EntityFrameworkCore;

namespace Discord_Bot.Tests.Integration.Database;

//The bias game's queries on LocalDB: picking the idols for a game, the statistics a game leaves, and the newest image
public sealed class BiasGameDataTests(LocalDatabase database) : IClassFixture<LocalDatabase>, IAsyncLifetime
{
    //Well before any real clock, so the images the services add (dated by getutcdate()) are always newer
    private static readonly DateTime past = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestHost host = new();

    public async ValueTask InitializeAsync()
    {
        host.Config.SqlConnectionString = database.ConnectionString;
        await database.ResetAsync();
    }

    [Fact]
    public async Task GetListForGameAsync_GivesTheGendersIdolsDebutingInTheYearsWithAnImage()
    {
        IdolGroup group = Entities.Group("group");
        Idol("in range", group, "F", new DateOnly(2015, 6, 1), images: 1);
        Idol("first year", group, "F", new DateOnly(2010, 1, 1), images: 1);
        Idol("last year", group, "F", new DateOnly(2020, 12, 31), images: 1);
        Idol("too early", group, "F", new DateOnly(2009, 12, 31), images: 1);
        Idol("too late", group, "F", new DateOnly(2021, 1, 1), images: 1);
        Idol("no debut", group, "F", null, images: 1);
        Idol("no image", group, "F", new DateOnly(2015, 6, 1), images: 0);
        Idol("man", group, "M", new DateOnly(2015, 6, 1), images: 1);
        await database.AddAsync(group);

        List<IdolGameResource> picked = await host.FromNewScope<IIdolService>().GetListForGameAsync(GenderEnum.Female, 2010, 2020);

        Assert.Equal(["first year", "in range", "last year"], Names(picked));
    }

    [Fact]
    public async Task GetListForGameAsync_BothGenders_TakesMenAndWomen()
    {
        IdolGroup group = Entities.Group("group");
        Idol("woman", group, "F", new DateOnly(2015, 6, 1), images: 1);
        Idol("man", group, "M", new DateOnly(2015, 6, 1), images: 1);
        await database.AddAsync(group);

        List<IdolGameResource> picked = await host.FromNewScope<IIdolService>().GetListForGameAsync(GenderEnum.NotSpecified, 2010, 2020);

        Assert.Equal(["man", "woman"], Names(picked));
    }

    [Fact]
    public async Task GetListForGameAsync_MoreThanSixteenToChooseFrom_GivesSixteenDifferentOnes()
    {
        IdolGroup group = Entities.Group("group");
        for (int i = 1; i <= 20; i++)
        {
            Idol($"idol {i}", group, "F", new DateOnly(2015, 6, 1), images: 1);
        }
        await database.AddAsync(group);

        List<IdolGameResource> picked = await host.FromNewScope<IIdolService>().GetListForGameAsync(GenderEnum.Female, 2010, 2020);

        Assert.Equal(16, picked.Select(idol => idol.IdolId).Distinct().Count());
    }

    [Fact]
    public async Task GetListForGameAsync_GivesEachIdolsNewestImageAndGroupName()
    {
        IdolGroup group = Entities.Group("group");
        group.FullName = "Group Full";
        Idol("idol", group, "F", new DateOnly(2015, 6, 1), images: 3);
        IdolGroup soloist = Entities.Group("soloist");
        Idol("solo", soloist, "F", new DateOnly(2015, 6, 1), images: 1);
        await database.AddAsync(group, soloist);

        List<IdolGameResource> picked = await host.FromNewScope<IIdolService>().GetListForGameAsync(GenderEnum.Female, 2010, 2020);

        IdolGameResource idol = picked.Single(found => found.StageName == "idol");
        Assert.Equal("https://example.com/idol/3.png", idol.LatestImageUrl);
        Assert.Equal("Group Full", idol.GroupFullName);
        Assert.Equal("Soloist", picked.Single(found => found.StageName == "solo").GroupFullName);
    }

    [Fact]
    public async Task GetTopIdolsAsync_RanksTheUsersIdolsByWeightAndMarksTheirBiases()
    {
        IdolGroup group = Entities.Group("group");
        Idol second = Idol("second", group, "F", new DateOnly(2015, 6, 1), images: 1);
        Idol first = Idol("first", group, "F", new DateOnly(2015, 6, 1), images: 1);
        Idol third = Idol("third", group, "M", new DateOnly(2015, 6, 1), images: 1);
        User user = Entities.User(RowIds.New);
        user.BiasGameCount = 3;
        user.Idols.Add(first);
        //Weights: a win counts 2, a second place 0.5, a quarter-final 0.125
        Stats(user, first, placed1: 1);
        Stats(user, second, placed2: 3);
        Stats(user, third, placed4: 10);
        await database.AddAsync(group, user);

        UserBiasGameStatResource? stats = await host.FromNewScope<IUserService>().GetTopIdolsAsync(DiscordIds.User, GenderEnum.NotSpecified);

        Assert.NotNull(stats);
        Assert.Equal(3, stats.BiasGameCount);
        Assert.Equal(["first", "second", "third"], stats.Stats.Select(stat => stat.IdolStageName));
        Assert.Equal([2, 1.5, 1.25], stats.Stats.Select(stat => stat.Weight));
        Assert.Equal([true, false, false], stats.Stats.Select(stat => stat.IsUserBias));
    }

    [Fact]
    public async Task GetTopIdolsAsync_GenderAndTopTen_LeaveOutTheRest()
    {
        IdolGroup group = Entities.Group("group");
        User user = Entities.User(RowIds.New);
        for (int i = 1; i <= 12; i++)
        {
            Stats(user, Idol($"woman {i}", group, "F", new DateOnly(2015, 6, 1), images: 1), placed1: i);
        }
        Stats(user, Idol("man", group, "M", new DateOnly(2015, 6, 1), images: 1), placed1: 50);
        await database.AddAsync(group, user);

        UserBiasGameStatResource? stats = await host.FromNewScope<IUserService>().GetTopIdolsAsync(DiscordIds.User, GenderEnum.Female);

        Assert.NotNull(stats);
        Assert.Equal(10, stats.Stats.Count);
        Assert.Equal("woman 12", stats.Stats[0].IdolStageName);
        Assert.DoesNotContain(stats.Stats, stat => stat.IdolStageName == "man");
    }

    [Fact]
    public async Task GetTopIdolsAsync_IdolWithoutImagesOrGroupName_GivesNoImageAndSoloist()
    {
        IdolGroup group = Entities.Group("soloist");
        User user = Entities.User(RowIds.New);
        Stats(user, Idol("solo", group, "F", new DateOnly(2015, 6, 1), images: 0), placed1: 1);
        await database.AddAsync(group, user);

        UserBiasGameStatResource? stats = await host.FromNewScope<IUserService>().GetTopIdolsAsync(DiscordIds.User, GenderEnum.NotSpecified);

        Assert.NotNull(stats);
        UserIdolStatisticResource stat = Assert.Single(stats.Stats);
        Assert.Null(stat.LatestImageUrl);
        Assert.Equal("Soloist", stat.IdolGroupFullName);
    }

    //A whole game, as BiasGameInteraction saves it: the ranking comes as a stack with the winner on top
    [Fact]
    public async Task UpdateUserStatisticsAsync_TwoGames_RankTheWinnerFirstAndCountBoth()
    {
        IdolGroup group = Entities.Group("group");
        for (int i = 1; i <= 16; i++)
        {
            Idol($"idol {i}", group, "F", new DateOnly(2015, 6, 1), images: 1);
        }
        await database.AddAsync(group);
        List<int> idolIds = await IdolIdsAsync();

        await host.FromNewScope<IUserIdolStatisticService>().UpdateUserStatisticsAsync(DiscordIds.User, new Stack<int>(idolIds.AsEnumerable().Reverse()));
        await host.FromNewScope<IUserIdolStatisticService>().UpdateUserStatisticsAsync(DiscordIds.User, new Stack<int>(idolIds.AsEnumerable().Reverse()));

        UserBiasGameStatResource? stats = await host.FromNewScope<IUserService>().GetTopIdolsAsync(DiscordIds.User, GenderEnum.NotSpecified);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.BiasGameCount);
        Assert.Equal("idol 1", stats.Stats[0].IdolStageName);
        Assert.Equal(4, stats.Stats[0].Weight);
        Assert.Equal("idol 2", stats.Stats[1].IdolStageName);
    }

    [Fact]
    public async Task GetIdolDetailsAsync_GivesTheNewestImage()
    {
        IdolGroup group = Entities.Group("group");
        Idol("idol", group, "F", new DateOnly(2015, 6, 1), images: 2);
        await database.AddAsync(group);

        IdolExtendedResource? details = await host.FromNewScope<IIdolService>().GetIdolDetailsAsync("idol", "group");

        Assert.NotNull(details);
        Assert.Equal("https://example.com/idol/2.png", details.LatestImageUrl);
    }

    [Fact]
    public async Task AddOverrideAsync_RecordsWhichImageItReplacesAndBecomesTheNewest()
    {
        IdolGroup group = Entities.Group("group");
        Idol("idol", group, "F", new DateOnly(2015, 6, 1), images: 2);
        await database.AddAsync(group);
        int idolId = Assert.Single(await IdolIdsAsync());

        Assert.Equal(DbProcessResultEnum.Success, await host.FromNewScope<IIdolImageService>().AddOverrideAsync(idolId, "https://example.com/chosen.png"));

        IdolExtendedResource? details = await host.FromNewScope<IIdolService>().GetIdolDetailsAsync("idol", "group");
        Assert.Equal("https://example.com/chosen.png", details?.LatestImageUrl);
        await using MainDbContext context = database.NewContext();
        string? overridden = await context.IdolImages.Where(image => image.ImageUrl == "https://example.com/chosen.png").Select(image => image.OverriddenUrl).SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("https://example.com/idol/2.png", overridden);
    }

    public ValueTask DisposeAsync()
    {
        host.Dispose();
        return ValueTask.CompletedTask;
    }

    //Stage name and name are the same; image n is n days newer than the first
    private static Idol Idol(string name, IdolGroup group, string gender, DateOnly? debut, int images)
    {
        Idol idol = Entities.Idol(name, group);
        idol.StageName = name;
        idol.Gender = gender;
        idol.DebutDate = debut;
        for (int i = 1; i <= images; i++)
        {
            idol.IdolImages.Add(new IdolImage { ImageUrl = $"https://example.com/{name.Replace(' ', '-')}/{i}.png", CreatedOn = past.AddDays(i) });
        }
        return idol;
    }

    private static void Stats(User user, Idol idol, int placed1 = 0, int placed2 = 0, int placed4 = 0)
    {
        user.UserIdolStatistics.Add(new UserIdolStatistic { Idol = idol, Placed1 = placed1, Placed2 = placed2, Placed4 = placed4 });
    }

    private static List<string?> Names(List<IdolGameResource> idols)
    {
        return idols.Select(idol => idol.StageName).Order(StringComparer.Ordinal).ToList();
    }

    //In the order they were added
    private async Task<List<int>> IdolIdsAsync()
    {
        await using MainDbContext context = database.NewContext();
        return await context.Idols.OrderBy(idol => idol.IdolId).Select(idol => idol.IdolId).ToListAsync(TestContext.Current.CancellationToken);
    }
}
