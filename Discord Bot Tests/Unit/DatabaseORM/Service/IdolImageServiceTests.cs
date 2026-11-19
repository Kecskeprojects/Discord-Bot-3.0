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

public sealed class IdolImageServiceTests
{
    private const int IdolId = 11;
    private const string ScrapedUrl = "https://example.com/scraped.png";
    private const string OwnUrl = "https://example.com/chosen.png";

    private readonly IIdolRepository idols = Substitute.For<IIdolRepository>();
    private readonly IIdolImageRepository images = Substitute.For<IIdolImageRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly IdolImageService service;

    public IdolImageServiceTests()
    {
        service = new IdolImageService(idols, images, TestMapper.Database, logger, new ServerCache());
    }

    //The scraper doesn't add back an image someone replaced (see IdolService.UpdateIdolDetailsAsync)
    [Fact]
    public async Task AddOverrideAsync_RecordsWhichImageItReplaces()
    {
        List<IdolImage> table = images.HasRows();
        images.GetLatestByIdolIdAsync(IdolId).Returns(new IdolImage { IdolId = IdolId, ImageUrl = ScrapedUrl });

        Assert.Equal(DbProcessResultEnum.Success, await service.AddOverrideAsync(IdolId, OwnUrl));
        IdolImage added = Assert.Single(table);
        Assert.Equal(IdolId, added.IdolId);
        Assert.Equal(OwnUrl, added.ImageUrl);
        Assert.Equal(ScrapedUrl, added.OverriddenUrl);
    }

    [Fact]
    public async Task AddOverrideAsync_NoImageYet_ReplacesNothing()
    {
        List<IdolImage> table = images.HasRows();
        images.GetLatestByIdolIdAsync(IdolId).Returns((IdolImage?) null);

        Assert.Equal(DbProcessResultEnum.Success, await service.AddOverrideAsync(IdolId, OwnUrl));
        Assert.Null(Assert.Single(table).OverriddenUrl);
    }

    [Fact]
    public async Task AddOverrideAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        images.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddOverrideAsync(IdolId, OwnUrl));
        logger.Received().Error("IdolImageService.cs AddOverrideAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task RemoveIdolImagesAsync_RemovesAllOfThatIdolsImages()
    {
        Idol idol = Entities.Idol("idol", Entities.Group("group"), IdolId);
        Idol other = Entities.Idol("idol", Entities.Group("other"), 12);
        IdolImage kept = Image(other, ScrapedUrl);
        idols.HasRows(other, idol);
        List<IdolImage> table = images.HasRows(Image(idol, ScrapedUrl), kept, Image(idol, OwnUrl));

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveIdolImagesAsync("idol", "group"));
        Assert.Same(kept, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveIdolImagesAsync_UnknownIdol_ReturnsNotFound()
    {
        idols.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveIdolImagesAsync("idol", "group"));
    }

    private static IdolImage Image(Idol idol, string url)
    {
        IdolImage image = new() { IdolId = idol.IdolId, Idol = idol, ImageUrl = url };
        idol.IdolImages.Add(image);
        return image;
    }
}
