using DatabaseORM.Communication.Bias;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors;
using NSubstitute.ExceptionExtensions;

namespace Discord_Bot.Tests.Unit.Bot.Processors;

//The weekly bias data refresh (Mondays, and the owner's !manual update bias): each idol with a profile link gets its page read,
//and the newest image and details are saved when the image changed. The dbkpop.com table step is commented out, so the
//list it would give is empty and only idols with a profile link are refreshed
public sealed class BiasScrapingProcessorTests
{
    private readonly IIdolService idolService = Substitute.For<IIdolService>();
    private readonly IKpopDbScraper scraper = Substitute.For<IKpopDbScraper>();
    private readonly IBrowserService browserService = Substitute.For<IBrowserService>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly BiasScrapingProcessor processor;

    public BiasScrapingProcessorTests()
    {
        processor = new BiasScrapingProcessor(idolService, scraper, browserService, logger);
        idolService.UpdateIdolDetailsAsync(default!, default, default).ReturnsForAnyArgs(true);
    }

    [Fact]
    public async Task Refresh_IdolWithANewImage_SavesItsDetails()
    {
        IdolResource jisoo = Idol("jisoo", "https://kprofiles.com/jisoo", "https://example.com/old.png");
        AdditionalIdolData scraped = new() { ImageUrl = "https://example.com/new.png" };
        idolService.GetAllIdolsAsync().Returns([jisoo]);
        scraper.GetProfileDataAsync("https://kprofiles.com/jisoo", true).Returns(scraped);

        await processor.RunUpdateBiasDataAsync();

        await idolService.Received().UpdateIdolDetailsAsync(jisoo, null, scraped);
        logger.Received().Log("Updated 1 idol's details.");
    }

    [Fact]
    public async Task Refresh_IdolsWithNothingToDo_AreLeftAsTheyAre()
    {
        IdolResource noLink = Idol("lisa", null, "https://example.com/lisa.png");
        IdolResource sameImage = Idol("rose", "https://kprofiles.com/rose", "https://example.com/rose.png");
        IdolResource pageWithoutImage = Idol("jennie", "https://kprofiles.com/jennie", "https://example.com/jennie.png");
        idolService.GetAllIdolsAsync().Returns([noLink, sameImage, pageWithoutImage]);
        scraper.GetProfileDataAsync("https://kprofiles.com/rose", Arg.Any<bool>()).Returns(new AdditionalIdolData { ImageUrl = "https://example.com/rose.png" });
        scraper.GetProfileDataAsync("https://kprofiles.com/jennie", Arg.Any<bool>()).Returns((AdditionalIdolData?) null);

        await processor.RunUpdateBiasDataAsync();

        await idolService.DidNotReceiveWithAnyArgs().UpdateIdolDetailsAsync(default!, default, default);
        await scraper.DidNotReceive().GetProfileDataAsync("", Arg.Any<bool>());
        logger.Received().Log("Updated 0 idol's details.");
    }

    //The group's page is read too only while the group's debut date is missing
    [Fact]
    public async Task Refresh_GroupWithADebutDate_IsNotReadAgain()
    {
        IdolResource jisoo = Idol("jisoo", "https://kprofiles.com/jisoo", "https://example.com/old.png");
        jisoo.GroupDebutDate = new DateOnly(2016, 8, 8);
        idolService.GetAllIdolsAsync().Returns([jisoo]);

        await processor.RunUpdateBiasDataAsync();

        await scraper.Received().GetProfileDataAsync("https://kprofiles.com/jisoo", false);
    }

    [Fact]
    public async Task Refresh_EndsWithTheCorrectionsAndClosesTheBrowser()
    {
        idolService.GetAllIdolsAsync().Returns([]);
        idolService.CorrectUpdateErrorsAsync().Returns(2);

        await processor.RunUpdateBiasDataAsync();

        await idolService.Received(1).CorrectUpdateErrorsAsync();
        logger.Received().Log("Corrected 2 idols with errors created during update.");
        await browserService.Received(1).CloseBrowser();
    }

    [Fact]
    public async Task Refresh_Fails_LogsItAndStillClosesTheBrowser()
    {
        idolService.GetAllIdolsAsync().ThrowsAsync(new InvalidOperationException("database down"));

        await processor.RunUpdateBiasDataAsync();

        logger.Received().Error("BiasScrapingProcessor.cs RunUpdateBiasDataAsync", Arg.Any<InvalidOperationException>());
        await browserService.Received(1).CloseBrowser();
    }

    private static IdolResource Idol(string name, string? profileUrl, string currentImageUrl)
    {
        return new IdolResource { Name = name, GroupName = "blackpink", ProfileUrl = profileUrl, CurrentImageUrl = currentImageUrl };
    }
}
