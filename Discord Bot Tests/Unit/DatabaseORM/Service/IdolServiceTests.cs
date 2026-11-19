using DatabaseORM.Caching;
using DatabaseORM.Communication;
using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using DatabaseORM.Service;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Database;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Service;

public sealed class IdolServiceTests
{
    private const int IdolId = 11;
    private const string ScrapedUrl = "https://example.com/scraped.png";
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;
    private static readonly DateOnly debut = new(2015, 4, 22);

    private readonly IIdolRepository idols = Substitute.For<IIdolRepository>();
    private readonly IIdolGroupRepository groups = Substitute.For<IIdolGroupRepository>();
    private readonly IIdolImageRepository images = Substitute.For<IIdolImageRepository>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly IdolService service;

    private readonly IdolGroup group = Entities.Group("group", 1);
    private readonly IdolGroup otherGroup = Entities.Group("other", 2);

    public IdolServiceTests()
    {
        service = new IdolService(idols, groups, images, TestMapper.Database, logger, new ServerCache());
        groups.HasRows(group, otherGroup);
    }

    [Fact]
    public async Task AddIdolAsync_ExistingGroup_AddsTheIdolToIt()
    {
        List<Idol> table = idols.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddIdolAsync("idol", "group"));
        Idol added = Assert.Single(table);
        Assert.Equal("idol", added.Name);
        Assert.Same(group, added.Group);
    }

    [Fact]
    public async Task AddIdolAsync_NewGroup_AddsTheGroupWithIt()
    {
        List<Idol> table = idols.HasRows();

        Assert.Equal(DbProcessResultEnum.Success, await service.AddIdolAsync("idol", "new group"));
        Assert.Equal("new group", Assert.Single(table).Group.Name);
    }

    [Fact]
    public async Task AddIdolAsync_AlreadyInThatGroup_ReturnsAlreadyExists()
    {
        idols.HasRows(Entities.Idol("idol", group));

        Assert.Equal(DbProcessResultEnum.AlreadyExists, await service.AddIdolAsync("idol", "group"));
    }

    [Fact]
    public async Task AddIdolAsync_DatabaseFails_LogsTheErrorAndReturnsFailure()
    {
        idols.Fails();

        Assert.Equal(DbProcessResultEnum.Failure, await service.AddIdolAsync("idol", "group"));
        logger.Received().Error("IdolService.cs AddIdolAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetIdolsByGroupAsync_GivesTheGroupsIdolsByName()
    {
        idols.HasRows(Entities.Idol("b", group), Entities.Idol("c", otherGroup), Entities.Idol("a", group));

        Assert.Equal(["a", "b"], (await service.GetIdolsByGroupAsync("group")).Select(idol => idol.Name));
    }

    [Fact]
    public async Task GetIdolsByGroupAsync_NoGroup_GivesEveryIdol()
    {
        idols.HasRows(Entities.Idol("b", group), Entities.Idol("c", otherGroup), Entities.Idol("a", group));

        Assert.Equal(["a", "b", "c"], (await service.GetIdolsByGroupAsync("")).Select(idol => idol.Name));
    }

    [Fact]
    public async Task RemoveIdolAsync_RemovesThatGroupsIdol()
    {
        Idol elsewhere = Entities.Idol("idol", otherGroup);
        List<Idol> table = idols.HasRows(Entities.Idol("idol", group), elsewhere);

        Assert.Equal(DbProcessResultEnum.Success, await service.RemoveIdolAsync("idol", "group"));
        Assert.Same(elsewhere, Assert.Single(table));
    }

    [Fact]
    public async Task RemoveIdolAsync_Unknown_ReturnsNotFound()
    {
        idols.HasRows(Entities.Idol("idol", otherGroup));

        Assert.Equal(DbProcessResultEnum.NotFound, await service.RemoveIdolAsync("idol", "group"));
    }

    [Fact]
    public async Task GetAllIdolsAsync_GivesEveryIdolWithItsNewestImage()
    {
        Idol idol = Entities.Idol("idol", group, IdolId);
        Image(idol, "https://example.com/old.png", now.AddDays(-7));
        Image(idol, ScrapedUrl, now);
        idols.HasRows(idol, Entities.Idol("member", group));

        List<IdolResource> result = await service.GetAllIdolsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(ScrapedUrl, result.Single(found => found.IdolId == IdolId).CurrentImageUrl);
    }

    //What the weekly bias scraping does for each idol
    [Fact]
    public async Task UpdateIdolDetailsAsync_NoProfileYet_FillsItFromTheScrapedTable()
    {
        Idol idol = KnownIdol();
        ExtendedBiasData data = new() { ProfileUrl = "https://example.com/profile", StageName = "Stage", Gender = GenderEnum.Female };

        Assert.True(await service.UpdateIdolDetailsAsync(Resource(), data, null));
        Assert.Equal("https://example.com/profile", idol.ProfileUrl);
        Assert.Equal("Stage", idol.StageName);
        Assert.Equal("F", idol.Gender);
        await idols.Received().UpdateAsync(idol);
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_HasAProfile_KeepsWhatIsThere()
    {
        Idol idol = KnownIdol();
        idol.ProfileUrl = "https://example.com/kept";
        idol.StageName = "Kept";

        await service.UpdateIdolDetailsAsync(Resource(), new ExtendedBiasData { ProfileUrl = "https://example.com/profile", StageName = "Stage" }, null);

        Assert.Equal("https://example.com/kept", idol.ProfileUrl);
        Assert.Equal("Kept", idol.StageName);
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_ScrapedImage_AddsIt()
    {
        Idol idol = KnownIdol();

        await service.UpdateIdolDetailsAsync(Resource(), null, new AdditionalIdolData { ImageUrl = ScrapedUrl });

        Assert.Equal(ScrapedUrl, Assert.Single(idol.IdolImages).ImageUrl);
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_ImageSomeoneReplaced_IsNotAddedBack()
    {
        Idol idol = KnownIdol();
        IdolImage chosen = Image(idol, "https://example.com/chosen.png", now.AddDays(-1));
        chosen.OverriddenUrl = ScrapedUrl;

        await service.UpdateIdolDetailsAsync(Resource(), null, new AdditionalIdolData { ImageUrl = ScrapedUrl });

        Assert.Same(chosen, Assert.Single(idol.IdolImages));
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_MoreThanThreeImages_RemovesAllButTheNewestThree()
    {
        Idol idol = KnownIdol();
        for (int daysAgo = 1; daysAgo <= 5; daysAgo++)
        {
            Image(idol, $"https://example.com/{daysAgo}.png", now.AddDays(-daysAgo));
        }
        List<IdolImage> imageTable = images.HasRows([.. idol.IdolImages]);

        await service.UpdateIdolDetailsAsync(Resource(), null, null);

        Assert.Equal(["https://example.com/1.png", "https://example.com/2.png", "https://example.com/3.png"], imageTable.Select(image => image.ImageUrl));
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_GroupWithoutDebut_FillsItFromTheProfile()
    {
        KnownIdol();
        AdditionalIdolData additional = new() { DebutDate = debut, GroupFullName = "Group Full", GroupFullKoreanName = "Group Korean" };

        await service.UpdateIdolDetailsAsync(Resource(), null, additional);

        Assert.Equal(debut, group.DebutDate);
        Assert.Equal("Group Full", group.FullName);
        Assert.Equal("Group Korean", group.FullKoreanName);
    }

    //A soloist's profile describes the idol, not a group
    [Fact]
    public async Task UpdateIdolDetailsAsync_Soloist_LeavesTheGroupAlone()
    {
        IdolGroup soloist = Entities.Group("soloist", 3);
        idols.HasRows(Entities.Idol("idol", soloist, IdolId));

        await service.UpdateIdolDetailsAsync(Resource(), null, new AdditionalIdolData { DebutDate = debut, GroupFullName = "Group Full" });

        Assert.Null(soloist.DebutDate);
        Assert.Null(soloist.FullName);
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_UnknownIdol_ReturnsFalse()
    {
        idols.HasRows();

        Assert.False(await service.UpdateIdolDetailsAsync(Resource(), null, null));
    }

    [Fact]
    public async Task UpdateIdolDetailsAsync_DatabaseFails_LogsTheErrorAndReturnsFalse()
    {
        idols.Fails();

        Assert.False(await service.UpdateIdolDetailsAsync(Resource(), null, null));
        logger.Received().Error("IdolService.cs UpdateIdolDetailsAsync", Arg.Any<Exception>());
    }

    [Fact]
    public async Task CorrectUpdateErrorsAsync_GivesProfiledIdolsWithoutADebutTheirGroupsDebut()
    {
        group.DebutDate = debut;
        Idol missing = Entities.Idol("missing", group);
        missing.ProfileUrl = "https://example.com/profile";
        Idol noProfile = Entities.Idol("no profile", group);
        Idol ownDebut = Entities.Idol("own debut", group);
        ownDebut.ProfileUrl = "https://example.com/profile";
        ownDebut.DebutDate = new DateOnly(2018, 1, 1);
        idols.HasRows(missing, noProfile, ownDebut);

        Assert.Equal(1, await service.CorrectUpdateErrorsAsync());
        Assert.Equal(debut, missing.DebutDate);
        Assert.Null(noProfile.DebutDate);
        Assert.Equal(new DateOnly(2018, 1, 1), ownDebut.DebutDate);
    }

    [Fact]
    public async Task GetIdolDetailsAsync_AddsTheLatestImage()
    {
        KnownIdol();
        images.GetLatestByIdolIdAsync(IdolId).Returns(new IdolImage { IdolId = IdolId, ImageUrl = ScrapedUrl });

        IdolExtendedResource? details = await service.GetIdolDetailsAsync("idol", "group");

        Assert.NotNull(details);
        Assert.Equal("group", details.GroupName);
        Assert.Equal(ScrapedUrl, details.LatestImageUrl);
    }

    [Fact]
    public async Task GetIdolDetailsAsync_Unknown_GivesNull()
    {
        idols.HasRows();

        Assert.Null(await service.GetIdolDetailsAsync("idol", "group"));
    }

    //From the edit modal
    [Fact]
    public async Task UpdateAsync_SavesTheTrimmedDetailsWithALowercaseName()
    {
        Idol idol = KnownIdol();
        IdolEditData data = new() { ProfileUrl = " https://example.com/profile ", Name = " New Name ", Group = "", DateOfBirth = "2000. 01. 02.", Gender = "f" };

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(IdolId, data));
        Assert.Equal("https://example.com/profile", idol.ProfileUrl);
        Assert.Equal("new name", idol.Name);
        Assert.Equal(new DateOnly(2000, 1, 2), idol.DateOfBirth);
        Assert.Equal("F", idol.Gender);
        Assert.Same(group, idol.Group);
    }

    [Fact]
    public async Task UpdateAsync_UnknownGender_KeepsTheCurrentOne()
    {
        Idol idol = KnownIdol();
        idol.Gender = "M";

        await service.UpdateAsync(IdolId, EditData(gender: "both"));

        Assert.Equal("M", idol.Gender);
    }

    [Theory]
    [InlineData("Other", "other")]
    [InlineData(" Brand New ", "brand new")]
    public async Task UpdateAsync_GroupName_MovesTheIdolToThatGroup(string groupName, string expected)
    {
        Idol idol = KnownIdol();

        await service.UpdateAsync(IdolId, EditData(groupName: groupName));

        Assert.Equal(expected, idol.Group.Name);
    }

    [Fact]
    public async Task UpdateAsync_NewGroupName_AddsTheGroup()
    {
        KnownIdol();
        List<IdolGroup> groupTable = groups.HasRows(group, otherGroup);

        await service.UpdateAsync(IdolId, EditData(groupName: "brand new"));

        Assert.Contains(groupTable, added => added.Name == "brand new");
    }

    //The edit modal shows dates as "yyyy. MM. dd.", which some cultures (such as German) can't read
    [Theory]
    [InlineData("hu-HU")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    public async Task UpdateAsync_DateOfBirthInTheModalsFormat_IsReadInAnyCulture(string culture)
    {
        Idol idol = KnownIdol();

        using (new CultureScope(culture))
        {
            await service.UpdateAsync(IdolId, EditData(dateOfBirth: "2000. 01. 02."));
        }

        Assert.Equal(new DateOnly(2000, 1, 2), idol.DateOfBirth);
    }

    [Fact]
    public async Task UpdateAsync_UnknownIdol_ReturnsNotFound()
    {
        idols.HasRows();

        Assert.Equal(DbProcessResultEnum.NotFound, await service.UpdateAsync(IdolId, EditData()));
    }

    [Fact]
    public async Task UpdateAsync_ExtendedDetails_SavesThemTrimmed()
    {
        Idol idol = KnownIdol();
        IdolExtendedEditData data = new() { StageName = " Stage ", KoreanStageName = " Korean Stage ", FullName = " Full Name ", KoreanFullName = " Korean Full ", DebutDate = "2015. 04. 22." };

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateAsync(IdolId, data));
        Assert.Equal("Stage", idol.StageName);
        Assert.Equal("Korean Stage", idol.KoreanStageName);
        Assert.Equal("Full Name", idol.FullName);
        Assert.Equal("Korean Full", idol.KoreanFullName);
        Assert.Equal(debut, idol.DebutDate);
    }

    [Theory]
    [InlineData("hu-HU")]
    [InlineData("de-DE")]
    public async Task UpdateAsync_ExtendedDebutInTheModalsFormat_IsReadInAnyCulture(string culture)
    {
        Idol idol = KnownIdol();

        using (new CultureScope(culture))
        {
            await service.UpdateAsync(IdolId, new IdolExtendedEditData { DebutDate = "2015. 04. 22." });
        }

        Assert.Equal(debut, idol.DebutDate);
    }

    [Fact]
    public async Task UpdateProfileUrlAsync_SavesTheTrimmedUrl()
    {
        Idol idol = KnownIdol();

        Assert.Equal(DbProcessResultEnum.Success, await service.UpdateProfileUrlAsync(IdolId, " https://example.com/profile "));
        Assert.Equal("https://example.com/profile", idol.ProfileUrl);
    }

    [Fact]
    public async Task ChangeGroupAsync_MovesTheIdolToTheLowercaseGroup()
    {
        Idol idol = KnownIdol();

        Assert.Equal(DbProcessResultEnum.Success, await service.ChangeGroupAsync(IdolId, "OTHER"));
        Assert.Same(otherGroup, idol.Group);
    }

    //For the who-knows style lookups: everyone who has any of the named idols on their list
    [Fact]
    public async Task GetUsersByIdolsAsync_GivesEachUserOnce()
    {
        User user = Entities.User();
        User otherUser = Entities.User(RowIds.OtherUser, DiscordIds.OtherUser);
        Idol idol = Entities.Idol("idol", group);
        idol.Users.Add(user);
        Idol member = Entities.Idol("member", group);
        member.Users.Add(user);
        member.Users.Add(otherUser);
        idols.GetListByNamesAsync("idol", "group").Returns([idol]);
        idols.GetListByNamesAsync("member", "").Returns([member]);

        ListWithDbResult<UserResource> result = await service.GetUsersByIdolsAsync(["idol-group", "member"]);

        Assert.Equal(DbProcessResultEnum.Success, result.ProcessResultEnum);
        Assert.Equal([DiscordIds.User, DiscordIds.OtherUser], result.List.Select(found => found.DiscordId));
    }

    [Fact]
    public async Task GetUsersByIdolsAsync_NoIdolFound_ReturnsNotFound()
    {
        idols.GetListByNamesAsync("nobody", "").Returns([]);

        Assert.Equal(DbProcessResultEnum.NotFound, (await service.GetUsersByIdolsAsync(["nobody"])).ProcessResultEnum);
    }

    [Fact]
    public async Task GetListForGameAsync_GivesTheChosenIdolsWithTheirImages()
    {
        Idol idol = Entities.Idol("idol", group, IdolId);
        Image(idol, ScrapedUrl, now);
        idols.GetListForGameAsync(GenderEnum.Female, 2010, 2020).Returns([idol]);

        IdolGameResource chosen = Assert.Single(await service.GetListForGameAsync(GenderEnum.Female, 2010, 2020));
        Assert.Equal(ScrapedUrl, chosen.LatestImageUrl);
    }

    [Fact]
    public async Task GetIdolByIdAsync_GivesTheIdolForTheGame()
    {
        KnownIdol();

        IdolGameResource? idol = await service.GetIdolByIdAsync(IdolId);

        Assert.NotNull(idol);
        Assert.Equal(IdolId, idol.IdolId);
    }

    //The idol the tests work on, in the repository with a same-named idol of another group
    private Idol KnownIdol()
    {
        Idol idol = Entities.Idol("idol", group, IdolId);
        idols.HasRows(Entities.Idol("idol", otherGroup, 12), idol);
        return idol;
    }

    private static IdolResource Resource()
    {
        return new IdolResource { IdolId = IdolId, Name = "idol", GroupName = "group" };
    }

    private static IdolEditData EditData(string groupName = "", string dateOfBirth = "", string gender = "")
    {
        return new IdolEditData { Name = "idol", Group = groupName, DateOfBirth = dateOfBirth, Gender = gender };
    }

    private static IdolImage Image(Idol idol, string url, DateTime createdOn)
    {
        IdolImage image = new() { IdolId = idol.IdolId, Idol = idol, ImageUrl = url, CreatedOn = createdOn };
        idol.IdolImages.Add(image);
        return image;
    }
}
