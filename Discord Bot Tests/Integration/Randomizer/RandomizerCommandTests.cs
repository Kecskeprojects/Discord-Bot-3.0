using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Randomizer;

//!8ball, !coin flip, !decide and the R6 operator randomizers through Discord.Net's command service. Every roll comes from
//the injected Random (since 2026-11-23), so each test sets the rolls it needs; an unset roll is 0
public sealed class RandomizerCommandTests : IDisposable
{
    //R6_Attackers.txt and R6_Defenders.txt (External Files) hold 39 operators each, Striker and Sentry first
    private const int OperatorCount = 39;

    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly Random random = Substitute.For<Random>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public RandomizerCommandTests()
    {
        host = new TestHost().Replace(serverService).Replace(random);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        member.GlobalName.Returns((string?) null);
        //Not in a voice channel unless a test puts them in one (an unset substitute would give a channel)
        member.VoiceChannel.Returns((IVoiceChannel?) null);
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData(0, "It is certain")]
    [InlineData(1, "It is decidedly so")]
    public async Task EightBall_GivesTheRolledAnswer(int roll, string answer)
    {
        random.Next(Arg.Is(0), Arg.Any<int>()).Returns(roll);

        CommandResult result = await RunAsync("!8ball will it rain?");

        Assert.True(result.IsSuccess, result.ErrorReason);
        Assert.Equal(answer, Assert.Single(channel.Sent).Text);
    }

    //A roll of 50 or 51 out of 100 lands the coin on its edge
    [Theory]
    [InlineData(0, "The coin landed on: Heads")]
    [InlineData(49, "The coin landed on: Heads")]
    [InlineData(50, "The coin landed on it's edge")]
    [InlineData(51, "The coin landed on it's edge")]
    [InlineData(52, "The coin landed on: Tails")]
    public async Task CoinFlip_GivesTheRolledSide(int roll, string reply)
    {
        random.Next(0, 100).Returns(roll);

        await RunAsync("!coin flip");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task CoinFlip_TwoChoices_PicksBetweenThem()
    {
        random.Next(0, 100).Returns(99);

        await RunAsync("!cf Pizza or Pasta");

        Assert.Equal("The coin landed on: pasta", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Decide_PicksTheRolledOptionAsWritten()
    {
        random.Next(0, 3).Returns(2);

        await RunAsync("!decide Pizza, Pasta, Sushi");

        Assert.Equal("Sushi", Assert.Single(channel.Sent).Text);
    }

    //It threw, logged the exception and answered nothing until 2026-11-24
    [Fact]
    public async Task Decide_OnlyCommas_SaysHowToGiveOptions()
    {
        await RunAsync("!decide , ,");

        Assert.Equal("Give at least one option, separated by commas, e.g.: `!decide pizza, pasta`", Assert.Single(channel.Sent).Text);
        host.Logger.DidNotReceiveWithAnyArgs().Error(default!, default(Exception)!);
    }

    [Fact]
    public async Task R6Randomizer_OutsideAVoiceChannel_GivesOnlyTheSenderAnOperator()
    {
        await RunAsync("!r6 randomizer attacker");

        Assert.Equal($"{TestValues.Username}: Striker", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task R6Randomizer_InAVoiceChannel_GivesEveryoneThereADifferentOperator()
    {
        ServerHasVoiceChannelWith("alice", "bob");
        //The second roll is out of one fewer, as the first operator is taken
        random.Next(0, OperatorCount).Returns(1);
        random.Next(0, OperatorCount - 1).Returns(1);

        await RunAsync("!r6 randomizer d");

        Assert.Equal("alice: Smoke\nbob: Mute", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task R6Randomizer_NeitherSide_AsksForOne()
    {
        await RunAsync("!r6 randomizer both");

        Assert.Equal("Please specify either `attacker` or `defender`", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task R6Randomizer_ChannelGiven_SendsItThere()
    {
        RecordingChannel general = RecordingChannel.Text(DiscordIds.OtherChannel);
        ITextChannel generalChannel = (ITextChannel) general.Channel;
        generalChannel.Name.Returns("general");
        guild.GetChannelsAsync(CacheMode.CacheOnly).Returns([generalChannel]);

        await RunAsync("!r6 randomizer a general");

        Assert.Empty(channel.Sent);
        Assert.Equal($"{TestValues.Username}: Striker", Assert.Single(general.Sent).Text);
    }

    [Fact]
    public async Task R6FullRandomizer_GivesThreeRoundsOfTheSideThenThreeOfTheOther()
    {
        await RunAsync("!r6 full randomizer a");

        string reply = Assert.Single(channel.Sent).Text!;
        Assert.Equal(["Attacker Round 1:", "Attacker Round 2:", "Attacker Round 3:", "Defender Round 4:", "Defender Round 5:", "Defender Round 6:"], Rounds(reply));
        Assert.Contains($"{TestValues.Username}: Sentry", reply, StringComparison.Ordinal);
    }

    [Fact]
    public async Task R6OvertimeRandomizer_AlternatesTheSidesForThreeRounds()
    {
        await RunAsync("!r6 overtime randomizer defender");

        Assert.Equal(["Defender Round 1:", "Attacker Round 2:", "Defender Round 3:"], Rounds(Assert.Single(channel.Sent).Text!));
    }

    private static List<string> Rounds(string reply)
    {
        return reply.Split('\n').Where(line => line.Contains("Round", StringComparison.Ordinal)).ToList();
    }

    //The sender and the named members share a voice channel
    private void ServerHasVoiceChannelWith(params string[] names)
    {
        IVoiceChannel voice = Substitute.For<IVoiceChannel>();
        voice.Id.Returns(300_009UL);
        List<IGuildUser> members = [];
        for (int i = 0; i < names.Length; i++)
        {
            IGuildUser user = DiscordMocks.GuildUser(DiscordIds.OtherUser + (ulong) i, username: names[i]);
            user.GlobalName.Returns((string?) null);
            user.VoiceChannel.Returns(voice);
            members.Add(user);
        }
        member.VoiceChannel.Returns(voice);
        guild.GetUsersAsync(CacheMode.CacheOnly).Returns(members);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, member, channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
