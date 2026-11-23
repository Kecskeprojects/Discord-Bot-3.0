using System.Net.Http;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Wotd;
using Discord_Bot.Tests.Common.Builders;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using NSubstitute.ExceptionExtensions;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.WordOfTheDay;

//!wotd through Discord.Net's command service, with the word-of-the-day feed replaced (the real service reads wotd.transparent.com)
public sealed class WotdCommandTests : IDisposable
{
    private readonly IWordOfTheDayService wordOfTheDayService = Substitute.For<IWordOfTheDayService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public WotdCommandTests()
    {
        host = new TestHost().Replace(wordOfTheDayService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData("!wotd", "korean")]
    [InlineData("!word of the day spanish", "spanish")]
    public async Task Wotd_Language_SendsItsWordOfTheDay(string command, string language)
    {
        wordOfTheDayService.GetDataAsync(language).Returns(ServiceModels.WordOfTheDay("Korean"));

        CommandResult result = await RunAsync(command);

        Assert.True(result.IsSuccess, result.ErrorReason);
        Assert.Equal("Korean word of the day:", Assert.Single(Assert.Single(channel.Sent).Embeds).Title);
    }

    [Fact]
    public async Task Wotd_UnknownLanguage_ListsTheLanguages()
    {
        wordOfTheDayService.GetDataAsync("klingon").Returns((WotdBase?) null);

        await RunAsync("!wotd klingon");

        string reply = Assert.Single(channel.Sent).Text!;
        Assert.StartsWith("Language is not supported, here is the list of languages:\n", reply, StringComparison.Ordinal);
        Assert.Contains("korean", reply, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Wotd_SiteDown_SaysSo()
    {
        wordOfTheDayService.GetDataAsync("korean").ThrowsAsync(new HttpRequestException("503"));

        await RunAsync("!wotd");

        Assert.Equal("Site is currently unavailable, try again in a little bit", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Wotd_InDirectMessages_Works()
    {
        wordOfTheDayService.GetDataAsync("korean").Returns(ServiceModels.WordOfTheDay("Korean"));
        RecordingChannel directMessages = RecordingChannel.DirectMessage();

        await host.RunCommandAsync(DiscordMocks.TextCommand("!wotd", DiscordMocks.User(), directMessages.Channel));

        Assert.Single(Assert.Single(directMessages.Sent).Embeds);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, DiscordMocks.GuildUser(), channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
