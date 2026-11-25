using DatabaseORM.Communication;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Biases;

//A user's bias list (!bias add/remove/clear, !my biases, !bias list) and !ping, through Discord.Net's command service
public sealed class BiasCommandTests : IDisposable
{
    private readonly IUserIdolService userIdolService = Substitute.For<IUserIdolService>();
    private readonly IIdolService idolService = Substitute.For<IIdolService>();
    private readonly IIdolGroupService idolGroupService = Substitute.For<IIdolGroupService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly Random random = Substitute.For<Random>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public BiasCommandTests()
    {
        host = new TestHost().Replace(userIdolService).Replace(idolService).Replace(idolGroupService).Replace(serverService).Replace(random);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        idolGroupService.GroupExistsAsnyc("blackpink").Returns(true);
        userIdolService.AddUserIdolAsync(default, default!, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        userIdolService.AddUserIdolGroupAsync(default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        userIdolService.RemoveUserIdolAsync(default, default!, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        userIdolService.RemoveUserIdolGroupAsync(default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        //!my biases adds a remark 1 time in 10, when the roll is 0
        random.Next(0, 10).Returns(1);
        member.Nickname.Returns("Ali");
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData("jisoo-blackpink", "jisoo", "blackpink")]
    [InlineData("Jisoo - BLACKPINK", "jisoo", "blackpink")]
    [InlineData("jisoo", "jisoo", "")]
    public async Task BiasAdd_IdolWithOrWithoutItsGroup_AddsThatIdol(string text, string name, string group)
    {
        CommandResult result = await RunAsync($"!bias add {text}");

        Assert.True(result.IsSuccess, result.ErrorReason);
        await userIdolService.Received().AddUserIdolAsync(DiscordIds.User, name, group);
        Assert.Equal("Bias(es) added to your list of biases.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BiasAdd_GroupName_AddsTheWholeGroup()
    {
        await RunAsync("!bias add BLACKPINK");

        await userIdolService.Received().AddUserIdolGroupAsync(DiscordIds.User, "blackpink");
        await userIdolService.DidNotReceiveWithAnyArgs().AddUserIdolAsync(default, default!, default!);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.MultipleResults, "There are multiple biases with that name.\nWrite down the group name too in the following format: [name]-[group].")]
    [InlineData(DbProcessResultEnum.AlreadyExists, "You already have this bias/these biases on your list.")]
    [InlineData(DbProcessResultEnum.MultipleExists, "A bias with that name was found in your list, but you did not specify a group. (format: [name]-[group])")]
    [InlineData(DbProcessResultEnum.NotFound, "Bias not in database.")]
    [InlineData(DbProcessResultEnum.Failure, "Bias(es) could not be added!")]
    public async Task BiasAdd_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        userIdolService.AddUserIdolAsync(DiscordIds.User, "jisoo", "").Returns(outcome);

        await RunAsync("!bias add jisoo");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BiasAdd_InDirectMessages_Works()
    {
        RecordingChannel directMessages = RecordingChannel.DirectMessage();

        await host.RunCommandAsync(DiscordMocks.TextCommand("!bias add jisoo", DiscordMocks.User(), directMessages.Channel));

        Assert.Equal("Bias(es) added to your list of biases.", Assert.Single(directMessages.Sent).Text);
    }

    [Fact]
    public async Task BiasRemove_IdolAndGroup_RemovesThatIdol()
    {
        await RunAsync("!bias remove jisoo-blackpink");

        await userIdolService.Received().RemoveUserIdolAsync(DiscordIds.User, "jisoo", "blackpink");
        Assert.Equal("Bias(es) removed from your list of biases.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BiasRemove_GroupName_RemovesTheWholeGroup()
    {
        await RunAsync("!bias remove blackpink");

        await userIdolService.Received().RemoveUserIdolGroupAsync(DiscordIds.User, "blackpink");
    }

    [Theory]
    [InlineData(DbProcessResultEnum.MultipleResults, "You have multiple biases with that name.\nWrite down the group name too in the following format: [name]-[group].")]
    [InlineData(DbProcessResultEnum.PartialNotFound, "You do not have this bias/these biases on your list.")]
    public async Task BiasRemove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        userIdolService.RemoveUserIdolAsync(DiscordIds.User, "jisoo", "").Returns(outcome);

        await RunAsync("!bias remove jisoo");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Your biases have been cleared.")]
    [InlineData(DbProcessResultEnum.Failure, "You did not have any biases to clear!")]
    public async Task BiasClear_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        userIdolService.ClearUserIdolAsync(DiscordIds.User).Returns(outcome);

        await RunAsync("!bias clear");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task MyBiases_GivesAMenuOfTheGroupsUnderTheUsersNickname()
    {
        userIdolService.GetUserIdolsListAsync(DiscordIds.User, "").Returns([Idol("jisoo", "blackpink"), Idol("karina", "aespa")]);

        await RunAsync("!my biases");

        SentMessage sent = Assert.Single(channel.Sent);
        Assert.Equal("Ali's biases by group", sent.Text);
        Assert.NotNull(sent.Components);
    }

    [Fact]
    public async Task MyBiases_OneGroup_ListsItsMembers()
    {
        userIdolService.GetUserIdolsListAsync(DiscordIds.User, "blackpink").Returns([Idol("jisoo", "blackpink")]);

        await RunAsync("!my biases BLACKPINK");

        Assert.Equal("BLACKPINK:\n`JISOO`\n", Assert.Single(channel.Sent).Text);
    }

    //The roll used to come from a new Random of its own; it is the injected one since 2026-11-23
    [Fact]
    public async Task MyBiases_RollOfZero_AddsARemarkAboutOneOfThem()
    {
        userIdolService.GetUserIdolsListAsync(DiscordIds.User, "").Returns([Idol("jisoo", "blackpink"), Idol("karina", "aespa")]);
        random.Next(0, 10).Returns(0);
        random.Next(0, 2).Returns(1);
        random.Next(0, 4).Returns(1);

        await RunAsync("!my biases");

        Assert.Equal(2, channel.Sent.Count);
        Assert.Equal("KARINA? Good choice!", channel.Sent[1].Text);
    }

    [Theory]
    [InlineData("", "You do not have any biases set yet!")]
    [InlineData(" blackpink", "No biases from that group are in your list!")]
    public async Task MyBiases_NoneFound_SaysSo(string group, string reply)
    {
        userIdolService.GetUserIdolsListAsync(default, default!).ReturnsForAnyArgs([]);

        await RunAsync($"!my biases{group}");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("", "No biases have been added to database yet!")]
    [InlineData(" blackpink", "No biases from that group are in the database!")]
    public async Task BiasList_NoneFound_SaysSo(string group, string reply)
    {
        idolService.GetIdolsByGroupAsync(default!).ReturnsForAnyArgs([]);

        await RunAsync($"!bias list{group}");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task BiasList_AllGroups_AsksWhichToSee()
    {
        idolService.GetIdolsByGroupAsync("").Returns([Idol("jisoo", "blackpink")]);

        await RunAsync("!bias list");

        Assert.Equal("Which group do you want to see?", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Ping_MentionsTheMembersWithTheBiasButNotTheSenderOrThoseWhoLeft()
    {
        PingFinds(DiscordIds.User, DiscordIds.OtherUser, DiscordIds.ThirdUser, 100_004);
        ServerHasMembers(DiscordIds.OtherUser, DiscordIds.ThirdUser);
        ICommandContext context = Command("!ping jisoo");

        CommandResult result = await host.RunCommandAsync(context);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await idolService.Received().GetUsersByIdolsAsync(Arg.Is<string[]>(names => names.SequenceEqual(new[] { "jisoo" })));
        await context.Message.Received().DeleteAsync();
        Assert.Equal($"<@{DiscordIds.OtherUser}> <@{DiscordIds.ThirdUser}>", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Ping_SeveralNames_LooksThemAllUp()
    {
        PingFinds(DiscordIds.OtherUser);
        ServerHasMembers(DiscordIds.OtherUser);

        await RunAsync("!ping Jisoo, Lisa-BLACKPINK");

        await idolService.Received().GetUsersByIdolsAsync(Arg.Is<string[]>(names => names.SequenceEqual(new[] { "jisoo", "lisa-blackpink" })));
    }

    [Theory]
    [InlineData(true, "Only you have that bias for now! Time to convert people.")]
    [InlineData(false, "Not even you have this bias, that's just shameful really...")]
    public async Task Ping_NobodyElseOnTheServer_SaysSo(bool senderHasIt, string reply)
    {
        PingFinds(senderHasIt ? [DiscordIds.User, 100_004] : [100_004]);
        ServerHasMembers();
        ICommandContext context = Command("!ping jisoo");

        await host.RunCommandAsync(context);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
        await context.Message.DidNotReceive().DeleteAsync();
    }

    [Theory]
    [InlineData(DbProcessResultEnum.NotFound, "No bias found with that name/those names!")]
    [InlineData(DbProcessResultEnum.Failure, "Exception during search for users!")]
    public async Task Ping_LookupFails_SaysSo(DbProcessResultEnum outcome, string reply)
    {
        idolService.GetUsersByIdolsAsync(default!).ReturnsForAnyArgs(new ListWithDbResult<UserResource>([], outcome));

        await RunAsync("!ping jisoo");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    //Discord takes 2000 characters per message, about 90 mentions. Until 2026-11-23 they went in one message,
    //which Discord.Net refuses, after the command had been deleted, so nobody was pinged
    [Fact]
    public async Task Ping_MoreMentionsThanOneMessageHolds_SendsThemInSeveral()
    {
        ulong[] fans = Enumerable.Range(1, 150).Select(i => 100_000_000_000_000_000UL + (ulong) i).ToArray();
        PingFinds(fans);
        ServerHasMembers(fans);

        await RunAsync("!ping jisoo");

        Assert.True(channel.Sent.Count > 1);
        Assert.All(channel.Sent, sent => Assert.True(sent.Text!.Length <= DiscordConfig.MaxMessageSize));
        Assert.Equal(fans.Select(MentionUtils.MentionUser), channel.Sent.SelectMany(sent => sent.Text!.Split(' ')));
    }

    private void PingFinds(params ulong[] userIds)
    {
        idolService.GetUsersByIdolsAsync(default!).ReturnsForAnyArgs(new ListWithDbResult<UserResource>(userIds.Select(id => new UserResource { DiscordId = id }).ToList(), DbProcessResultEnum.Success));
    }

    //Anyone else is not on the server (any more)
    private void ServerHasMembers(params ulong[] userIds)
    {
        guild.GetUserAsync(default, default).ReturnsForAnyArgs((IGuildUser?) null);
        guild.GetUserAsync(DiscordIds.User, CacheMode.CacheOnly).Returns(member);
        foreach (ulong id in userIds)
        {
            IGuildUser user = DiscordMocks.GuildUser(id);
            guild.GetUserAsync(id, CacheMode.CacheOnly).Returns(user);
        }
    }

    private static IdolResource Idol(string name, string group)
    {
        return new IdolResource { Name = name, GroupName = group };
    }

    private ICommandContext Command(string text)
    {
        return DiscordMocks.TextCommand(text, member, channel.Channel);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(Command(text));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
