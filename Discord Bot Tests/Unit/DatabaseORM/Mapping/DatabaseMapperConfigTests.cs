using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Mapping;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Mapster;
using Mapster.Models;
using MapsterMapper;

namespace Discord_Bot.Tests.Unit.DatabaseORM.Mapping;

//The hand-written mappings: the string → ulong ids, the navigations (null when a query didn't load them) and the renamed members
public sealed class DatabaseMapperConfigTests
{
    private const int IdolId = 11;
    private const int PollId = 21;
    private const string NewestImageUrl = "https://example.com/newest.png";
    private static readonly DateTime today = TestTimes.Now.UtcDateTime;
    private static readonly DateOnly groupDebut = new(2015, 4, 22);

    private readonly IMapper mapper = TestMapper.Database;

    //A resource member without a source stays empty without any error, so each one is either mapped or ignored on purpose
    [Theory]
    [MemberData(nameof(ResourceMappings))]
    public void Register_ResourceMapping_GivesEveryMemberASource(Type source, Type destination)
    {
        TypeAdapterConfig config = new() { RequireDestinationMemberSource = true };
        new DatabaseMapperConfig().Register(config);

        //Throws for a member that is neither mapped nor ignored
        config.Compile(source, destination);
    }

    public static TheoryData<Type, Type> ResourceMappings()
    {
        TypeAdapterConfig config = new();
        new DatabaseMapperConfig().Register(config);

        TheoryData<Type, Type> mappings = [];
        foreach (TypeTuple pair in config.RuleMap.Keys.Where(pair => pair.Destination.Namespace == "DatabaseORM.Resources"))
        {
            mappings.Add(pair.Source, pair.Destination);
        }
        return mappings;
    }

    [Fact]
    public void Map_Server_ParsesTheIdsAndTakesTheRoles()
    {
        Server server = Entities.Server();
        server.RoleMessageDiscordId = DiscordIds.Message.ToString();
        server.NotificationRole = Entities.Role(DiscordIds.Role, "notified");
        server.MuteRole = Entities.Role(DiscordIds.OtherRole, "muted");

        ServerResource resource = mapper.Map<ServerResource>(server);

        Assert.Equal(RowIds.Server, resource.ServerId);
        Assert.Equal(DiscordIds.Server, resource.DiscordId);
        Assert.Equal(DiscordIds.Message, resource.RoleMessageDiscordId);
        Assert.Equal(DiscordIds.Role, resource.NotificationRoleDiscordId);
        Assert.Equal("notified", resource.NotificationRoleName);
        Assert.Equal(DiscordIds.OtherRole, resource.MuteRoleDiscordId);
        Assert.Equal("muted", resource.MuteRoleName);
        Assert.Empty(resource.SettingsChannels); //ServerService fills it
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ServerWithoutRoles_LeavesThemNull(string? roleMessageId)
    {
        Server server = Entities.Server();
        server.RoleMessageDiscordId = roleMessageId;

        ServerResource resource = mapper.Map<ServerResource>(server);

        Assert.Null(resource.RoleMessageDiscordId);
        Assert.Null(resource.NotificationRoleDiscordId);
        Assert.Null(resource.NotificationRoleName);
        Assert.Null(resource.MuteRoleDiscordId);
        Assert.Null(resource.MuteRoleName);
    }

    [Fact]
    public void Map_ServerWithTwitchChannels_GivesThemTheServersIdAndRole()
    {
        Server server = Entities.Server();
        server.NotificationRole = Entities.Role(DiscordIds.Role, "notified");
        server.TwitchChannels.Add(new TwitchChannel { TwitchId = "twitch-id", TwitchLink = "https://www.twitch.tv/tester", TwitchName = "tester", Server = server });

        TwitchChannelResource channel = Assert.Single(mapper.Map<ServerResource>(server).TwitchChannels);

        Assert.Equal("twitch-id", channel.TwitchId);
        Assert.Equal("https://www.twitch.tv/tester", channel.TwitchLink);
        Assert.Equal(DiscordIds.Server, channel.ServerDiscordId);
        Assert.Equal(DiscordIds.Role, channel.NotificationRoleDiscordId);
        Assert.Equal("notified", channel.NotificationRoleName);
    }

    [Fact]
    public void Map_TwitchChannelWithoutItsServer_GivesNoServerOrRole()
    {
        TwitchChannelResource resource = mapper.Map<TwitchChannelResource>(new TwitchChannel { TwitchId = "twitch-id", TwitchLink = "", TwitchName = "" });

        Assert.Equal(0ul, resource.ServerDiscordId);
        Assert.Null(resource.NotificationRoleDiscordId);
        Assert.Null(resource.NotificationRoleName);
    }

    [Fact]
    public void Map_Role_ParsesTheId()
    {
        RoleResource resource = mapper.Map<RoleResource>(Entities.Role(DiscordIds.Role, "notified"));

        Assert.Equal(DiscordIds.Role, resource.DiscordId);
        Assert.Equal("notified", resource.RoleName);
    }

    [Fact]
    public void Map_User_ParsesTheIdAndTakesTheLastFmUsername()
    {
        User user = Entities.User();
        user.LastFmusername = TestValues.LastFmUsername;

        UserResource resource = mapper.Map<UserResource>(user);

        Assert.Equal(RowIds.User, resource.UserId);
        Assert.Equal(DiscordIds.User, resource.DiscordId);
        Assert.Equal(TestValues.LastFmUsername, resource.LastFmUsername);
        Assert.Null(resource.Username); //The bot fills it from the server member
    }

    [Fact]
    public void Map_Reminder_TakesTheUsersId()
    {
        Reminder reminder = new() { ReminderId = 5, Message = "test", Date = today, User = Entities.User() };

        ReminderResource resource = mapper.Map<ReminderResource>(reminder);

        Assert.Equal(5, resource.ReminderId);
        Assert.Equal("test", resource.Message);
        Assert.Equal(today, resource.Date);
        Assert.Equal(DiscordIds.User, resource.UserDiscordId);
    }

    [Fact]
    public void Map_Birthday_TakesTheUsersAndTheServersIds()
    {
        Birthday birthday = new() { BirthdayId = 6, Date = new DateOnly(2000, 3, 4), User = Entities.User(), Server = Entities.Server() };

        BirthdayResource resource = mapper.Map<BirthdayResource>(birthday);

        Assert.Equal(6, resource.BirthdayId);
        Assert.Equal(new DateOnly(2000, 3, 4), resource.Date);
        Assert.Equal(DiscordIds.User, resource.UserDiscordId);
        Assert.Equal(DiscordIds.Server, resource.ServerDiscordId);
    }

    [Fact]
    public void Map_BirthdayWithoutItsUserOrServer_GivesZeroIds()
    {
        BirthdayResource resource = mapper.Map<BirthdayResource>(new Birthday { Date = new DateOnly(2000, 3, 4) });

        Assert.Equal(0ul, resource.UserDiscordId);
        Assert.Equal(0ul, resource.ServerDiscordId);
    }

    [Fact]
    public void Map_Idol_TakesItsGroupAndItsNewestImage()
    {
        IdolResource resource = mapper.Map<IdolResource>(NewIdol());

        Assert.Equal(IdolId, resource.IdolId);
        Assert.Equal("idol", resource.Name);
        Assert.Equal("group", resource.GroupName);
        Assert.Equal(groupDebut, resource.GroupDebutDate);
        Assert.Equal(NewestImageUrl, resource.CurrentImageUrl);
    }

    [Fact]
    public void Map_IdolWithoutGroupOrImages_GivesAnEmptyGroupNameAndNoImage()
    {
        IdolResource resource = mapper.Map<IdolResource>(new Idol { Name = "idol" });

        Assert.Equal("", resource.GroupName);
        Assert.Null(resource.GroupDebutDate);
        Assert.Null(resource.CurrentImageUrl);
    }

    [Fact]
    public void Map_IdolToExtendedResource_TakesTheDetailsButNotTheImage()
    {
        Idol idol = NewIdol();
        idol.FullName = "Full Name";
        idol.Gender = "F";
        idol.DateOfBirth = new DateOnly(2000, 1, 2);

        IdolExtendedResource resource = mapper.Map<IdolExtendedResource>(idol);

        Assert.Equal("group", resource.GroupName);
        Assert.Equal("Stage", resource.StageName);
        Assert.Equal("Full Name", resource.FullName);
        Assert.Equal("F", resource.Gender);
        Assert.Equal(new DateOnly(2000, 1, 2), resource.DateOfBirth);
        Assert.Null(resource.LatestImageUrl); //IdolService looks it up separately
    }

    [Fact]
    public void Map_IdolToGameResource_TakesTheGroupsFullNamesAndTheNewestImage()
    {
        IdolGameResource resource = mapper.Map<IdolGameResource>(NewIdol());

        Assert.Equal("Stage", resource.StageName);
        Assert.Equal("Korean Stage", resource.KoreanStageName);
        Assert.Equal("Group Full", resource.GroupFullName);
        Assert.Equal("Group Korean", resource.GroupFullKoreanName);
        Assert.Equal(NewestImageUrl, resource.LatestImageUrl);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Map_IdolToGameResource_WithoutAGroupFullName_ShowsSoloist(bool groupLoaded)
    {
        Idol idol = new() { Name = "idol", Group = groupLoaded ? new IdolGroup { Name = "soloist" } : null! };

        Assert.Equal("Soloist", mapper.Map<IdolGameResource>(idol).GroupFullName);
    }

    [Fact]
    public void Map_UserToBiasGameStats_LeavesTheStatsToTheService()
    {
        User user = Entities.User();
        user.BiasGameCount = 12;

        UserBiasGameStatResource resource = mapper.Map<UserBiasGameStatResource>(user);

        Assert.Equal(DiscordIds.User, resource.DiscordId);
        Assert.Equal(12, resource.BiasGameCount);
        Assert.Empty(resource.Stats);
    }

    [Fact]
    public void Map_ServerMutedUser_KeepsTheIdsAsText()
    {
        ServerMutedUser mutedUser = new() { MutedUntil = today, RemovedRoleDiscordIds = "removed roles", Server = Entities.Server(), User = Entities.User() };

        ServerMutedUserResource resource = mapper.Map<ServerMutedUserResource>(mutedUser);

        Assert.Equal(DiscordIds.Server.ToString(), resource.ServerDiscordId);
        Assert.Equal(DiscordIds.User.ToString(), resource.UserDiscordId);
        Assert.Equal(today, resource.MutedUntil);
        Assert.Equal("removed roles", resource.RemovedRoleDiscordIds);
    }

    [Fact]
    public void Map_ServerMutedUserWithoutItsServerOrUser_GivesEmptyIds()
    {
        ServerMutedUserResource resource = mapper.Map<ServerMutedUserResource>(new ServerMutedUser { RemovedRoleDiscordIds = "" });

        Assert.Equal("", resource.ServerDiscordId);
        Assert.Equal("", resource.UserDiscordId);
    }

    //A poll with its own options; the "Days Of Week" options depend on the system clock and are tested in WeeklyPollToolsTests
    [Fact]
    public void Map_WeeklyPoll_TakesTheIdsAndItsOwnOptionsInOrder()
    {
        WeeklyPoll poll = NewPoll();
        poll.Channel = new Channel { DiscordId = DiscordIds.Channel.ToString() };
        poll.Role = Entities.Role(DiscordIds.Role, "voters");

        WeeklyPollResource resource = mapper.Map<WeeklyPollResource>(poll);

        Assert.Equal(PollId, resource.WeeklyPollId);
        Assert.Equal(DiscordIds.Server, resource.ServerDiscordId);
        Assert.Equal(DiscordIds.Channel, resource.ChannelDiscordId);
        Assert.Equal(DiscordIds.Role, resource.RoleDiscordId);
        Assert.Equal("Weekly question", resource.Title);
        Assert.True(resource.IsMultipleAnswer);
        Assert.Equal(TimeSpan.FromDays(1).Ticks, resource.CloseInTimeSpanTicks);
        string[] expected = ["First", "Second"];
        Assert.Equal(expected, resource.Options);
    }

    [Fact]
    public void Map_WeeklyPollWithoutChannelOrRole_LeavesThemNull()
    {
        WeeklyPollResource resource = mapper.Map<WeeklyPollResource>(NewPoll());

        Assert.Null(resource.ChannelDiscordId);
        Assert.Null(resource.RoleDiscordId);
    }

    [Fact]
    public void Map_WeeklyPollToEditResource_TakesItsOptions()
    {
        WeeklyPollEditResource resource = mapper.Map<WeeklyPollEditResource>(NewPoll());

        Assert.Equal("Monday", resource.RepeatOnDayOfWeek);
        Assert.Equal(["Second", "First"], resource.Options.Select(option => option.Title));
        Assert.All(resource.Options, option => Assert.Equal(PollId, option.WeeklyPollId));
    }

    [Fact]
    public void Map_WeeklyPollOptionPreset_TakesItsOptionsWithoutAPollId()
    {
        WeeklyPollOptionPreset preset = new() { WeeklyPollOptionPresetId = 4, Name = "Yes or no", Description = "", IsActive = true };
        preset.WeeklyPollOptions.Add(new WeeklyPollOption { WeeklyPollOptionId = 8, WeeklyPollOptionPresetId = 4, OrderNumber = 1, Title = "Yes" });

        WeeklyPollOptionPresetResource resource = mapper.Map<WeeklyPollOptionPresetResource>(preset);

        Assert.Equal("Yes or no", resource.Name);
        WeeklyPollOptionResource option = Assert.Single(resource.Options);
        Assert.Equal(0, option.WeeklyPollId);
        Assert.Equal(8, option.WeeklyPollOptionId);
        Assert.Equal(4, option.WeeklyPollOptionPresetId);
        Assert.Equal("Yes", option.Title);
    }

    //The bias scraper fills new idols with it, and the bias game filters on these letters
    [Theory]
    [InlineData(GenderEnum.Male, "M")]
    [InlineData(GenderEnum.Female, "F")]
    [InlineData(null, null)]
    public void Map_ScrapedDataOntoAnIdol_StoresTheGenderAsItsDatabaseLetter(GenderEnum? gender, string? expected)
    {
        Idol idol = new() { Name = "idol" };

        mapper.Map(new ExtendedBiasData { Gender = gender }, idol);

        Assert.Equal(expected, idol.Gender);
    }

    [Fact]
    public void Map_ScrapedDataOntoAnIdol_FillsTheProfileAndKeepsWhoTheIdolIs()
    {
        DateTime created = today.AddYears(-1);
        Idol idol = new() { IdolId = IdolId, Name = "idol", GroupId = 9, CreatedOn = created, ModifiedOn = created };
        ExtendedBiasData data = new()
        {
            ProfileUrl = "https://example.com/profile",
            StageName = "Stage",
            FullName = "Full Name",
            KoreanFullName = "Korean Full",
            KoreanStageName = "Korean Stage",
            DateOfBirth = new DateOnly(2000, 1, 2),
            GroupName = "other group"
        };
        DateTime before = DateTime.UtcNow;

        mapper.Map(data, idol);

        Assert.Equal("https://example.com/profile", idol.ProfileUrl);
        Assert.Equal("Stage", idol.StageName);
        Assert.Equal("Full Name", idol.FullName);
        Assert.Equal("Korean Full", idol.KoreanFullName);
        Assert.Equal("Korean Stage", idol.KoreanStageName);
        Assert.Equal(new DateOnly(2000, 1, 2), idol.DateOfBirth);
        Assert.Equal(IdolId, idol.IdolId);
        Assert.Equal("idol", idol.Name);
        Assert.Equal(9, idol.GroupId);
        Assert.Equal(created, idol.CreatedOn);
        Assert.True(idol.ModifiedOn >= before);
    }

    [Fact]
    public void Map_ProfileDataOntoAGroup_FillsItsNamesAndDebutAndKeepsItsName()
    {
        DateTime created = today.AddYears(-1);
        IdolGroup group = new() { GroupId = 9, Name = "group", CreatedOn = created, ModifiedOn = created };
        AdditionalIdolData data = new() { DebutDate = groupDebut, GroupFullName = "Group Full", GroupFullKoreanName = "Group Korean", ImageUrl = NewestImageUrl };
        DateTime before = DateTime.UtcNow;

        mapper.Map(data, group);

        Assert.Equal(9, group.GroupId);
        Assert.Equal("group", group.Name);
        Assert.Equal(groupDebut, group.DebutDate);
        Assert.Equal("Group Full", group.FullName);
        Assert.Equal("Group Korean", group.FullKoreanName);
        Assert.Equal(created, group.CreatedOn);
        Assert.True(group.ModifiedOn >= before);
    }

    private static Idol NewIdol()
    {
        Idol idol = new()
        {
            IdolId = IdolId,
            Name = "idol",
            StageName = "Stage",
            KoreanStageName = "Korean Stage",
            Group = new IdolGroup { Name = "group", FullName = "Group Full", FullKoreanName = "Group Korean", DebutDate = groupDebut }
        };
        //Out of order, so the mapping has to find the newest
        idol.IdolImages.Add(new IdolImage { ImageUrl = "https://example.com/old.png", CreatedOn = today.AddDays(-30) });
        idol.IdolImages.Add(new IdolImage { ImageUrl = NewestImageUrl, CreatedOn = today });
        idol.IdolImages.Add(new IdolImage { ImageUrl = "https://example.com/older.png", CreatedOn = today.AddDays(-60) });
        return idol;
    }

    private static WeeklyPoll NewPoll()
    {
        WeeklyPoll poll = new()
        {
            WeeklyPollId = PollId,
            Name = "weekly",
            Title = "Weekly question",
            RepeatOnDayOfWeek = "Monday",
            IsMultipleAnswer = true,
            CloseInTimeSpanTicks = TimeSpan.FromDays(1).Ticks,
            Server = Entities.Server()
        };
        poll.WeeklyPollOptions.Add(new WeeklyPollOption { WeeklyPollId = PollId, OrderNumber = 2, Title = "Second" });
        poll.WeeklyPollOptions.Add(new WeeklyPollOption { WeeklyPollId = PollId, OrderNumber = 1, Title = "First" });
        return poll;
    }
}
