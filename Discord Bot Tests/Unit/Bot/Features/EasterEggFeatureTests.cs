using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Features;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;

namespace Discord_Bot.Tests.Unit.Bot.Features;

//Runs on every message that isn't a command or a role request, in servers and direct messages
public sealed class EasterEggFeatureTests
{
    private readonly IGreetingService greetingService = Substitute.For<IGreetingService>();
    private readonly IDiscordClient client = Substitute.For<IDiscordClient>();
    //Random's Next is virtual, so a substitute decides each roll; an unset roll would give 0
    private readonly Random random = Substitute.For<Random>();
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();
    private readonly EasterEggFeature feature;

    private readonly RecordingChannel channel = RecordingChannel.Text();

    public EasterEggFeatureTests()
    {
        feature = new EasterEggFeature(greetingService, client, random, Substitute.For<IServerService>(), logger);
        ISelfUser bot = Substitute.For<ISelfUser>();
        bot.Id.Returns(DiscordIds.Bot);
        client.CurrentUser.Returns(bot);
        IGuild guild = DiscordMocks.Guild();
        ((IGuildChannel) channel.Channel).Guild.Returns(guild);
        greetingService.GetAllGreetingAsync().Returns([]);

        //No easter egg unless a test rolls for one
        random.Next(0, 5000).Returns(1);
        random.Next(0, 20).Returns(1);
    }

    [Theory]
    [InlineData($"hi <@100000>!")]
    [InlineData($"hi <@!100000>!")]
    public async Task Run_MentionOfTheBot_PostsARandomGreeting(string text)
    {
        greetingService.GetAllGreetingAsync().Returns([Greeting("https://example.com/a.gif"), Greeting("https://example.com/b.gif")]);
        random.Next(0, 2).Returns(1);

        await feature.Run(Message(text));

        Assert.Equal("https://example.com/b.gif", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Run_MentionWithoutGreetings_SendsNothing()
    {
        Assert.True(await feature.Run(Message($"hi <@{DiscordIds.Bot}>")));
        Assert.Empty(channel.Sent);
    }

    [Fact]
    public async Task Run_OneInFiveThousand_SendsAnEasterEggMessage()
    {
        random.Next(0, 5000).Returns(0);
        random.Next(0, Constant.EasterEggMessages.Length).Returns(2);

        Assert.True(await feature.Run(Message("anything")));

        Assert.Equal(Constant.EasterEggMessages[2], Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Run_IThink_OneInTwenty_AgreesInAReply()
    {
        random.Next(0, 20).Returns(0);
        random.Next(0, 2).Returns(0);

        await feature.Run(Message("I think cats are great"));

        SentMessage reply = Assert.Single(channel.Sent);
        Assert.Equal("I agree wholeheartedly!", reply.Text);
        Assert.Equal(DiscordIds.Message, reply.Reference?.MessageId.GetValueOrDefault());
    }

    [Fact]
    public async Task Run_IThink_OneInTwenty_OtherwiseMocksTheMessage()
    {
        random.Next(0, 20).Returns(0);
        random.Next(0, 2).Returns(1);

        await feature.Run(Message("I think cats are great"));

        //ToMockText changes the letters' case at random
        Assert.Equal("i think cats are great", Assert.Single(channel.Sent).Text?.ToLowerInvariant());
    }

    [Theory]
    [InlineData("I am hungry", "Hey hungry, I'm Kim Synthji!")]
    [InlineData("I'm tired", "Hey tired, I'm Kim Synthji!")]
    public async Task Run_IAm_OneInTwenty_IntroducesItself(string text, string expected)
    {
        random.Next(0, 20).Returns(0);

        await feature.Run(Message(text));

        Assert.Equal(expected, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("i am")]
    [InlineData("just a message")]
    public async Task Run_OneInTwentyWithoutAMatchingStart_SaysNothing(string text)
    {
        random.Next(0, 20).Returns(0);

        Assert.True(await feature.Run(Message(text)));
        Assert.Empty(channel.Sent);
    }

    [Fact]
    public async Task Run_IThinkWithoutTheRoll_SaysNothing()
    {
        Assert.True(await feature.Run(Message("I think cats are great")));
        Assert.Empty(channel.Sent);
    }

    //CommandHandler runs this feature for direct messages too, where there is no server
    [Fact]
    public async Task Run_IThinkInADirectMessage_RepliesThere()
    {
        random.Next(0, 20).Returns(0);
        random.Next(0, 2).Returns(0);
        RecordingChannel directMessages = RecordingChannel.DirectMessage();

        Assert.True(await feature.Run(DiscordMocks.TextCommand("I think so", DiscordMocks.User(), directMessages.Channel)));

        Assert.Equal("I agree wholeheartedly!", Assert.Single(directMessages.Sent).Text);
    }

    private ICommandContext Message(string text)
    {
        return DiscordMocks.TextCommand(text, DiscordMocks.GuildUser(), channel.Channel);
    }

    private static GreetingResource Greeting(string url)
    {
        return new GreetingResource { Url = url };
    }
}
