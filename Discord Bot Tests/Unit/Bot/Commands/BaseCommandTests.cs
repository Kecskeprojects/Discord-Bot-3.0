using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Commands;
using Discord_Bot.Interfaces;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;

namespace Discord_Bot.Tests.Unit.Bot.Commands;

//The checks and argument splitting every text command uses
public sealed class BaseCommandTests
{
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public BaseCommandTests()
    {
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsCommandAllowed_DirectMessage_IsUpToTheCommand(bool canBeDM)
    {
        ProbeCommand command = Command(RecordingChannel.DirectMessage(), DiscordMocks.User());

        Assert.Equal(canBeDM, await command.IsAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: canBeDM));
        await serverService.DidNotReceiveWithAnyArgs().GetByDiscordIdAsync(default);
    }

    [Fact]
    public async Task IsCommandAllowed_ServerWithoutThatChannelType_AllowsEveryChannel()
    {
        ServerHas(new() { [ChannelTypeEnum.MusicText] = [DiscordIds.OtherChannel] });

        Assert.True(await Command(channel, DiscordMocks.GuildUser()).IsAllowedAsync(ChannelTypeEnum.CommandText));
    }

    [Theory]
    [InlineData(DiscordIds.Channel, true)]
    [InlineData(DiscordIds.OtherChannel, false)]
    public async Task IsCommandAllowed_ServerWithThatChannelType_AllowsOnlyThoseChannels(ulong commandChannel, bool allowed)
    {
        ServerHas(new() { [ChannelTypeEnum.CommandText] = [commandChannel] });

        Assert.Equal(allowed, await Command(channel, DiscordMocks.GuildUser()).IsAllowedAsync(ChannelTypeEnum.CommandText));
    }

    //Not passed by any command yet
    [Fact]
    public async Task IsCommandAllowed_TypeRequiredButNotSet_Refuses()
    {
        ServerHas([]);

        Assert.False(await Command(channel, DiscordMocks.GuildUser()).IsAllowedAsync(ChannelTypeEnum.CommandText, allowLackOfType: false));
    }

    //CommandHandler adds every server it sees, so a missing row means the database failed
    [Fact]
    public async Task GetCurrentServer_ServerNotLoaded_Throws()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns((ServerResource?) null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Command(channel, DiscordMocks.GuildUser()).IsAllowedAsync(ChannelTypeEnum.CommandText));
    }

    [Theory]
    [InlineData("Jisoo - BLACKPINK", '-', true, new[] { "jisoo", "blackpink" })]
    [InlineData("Jisoo - BLACKPINK", '-', false, new[] { "Jisoo", "BLACKPINK" })]
    [InlineData(" a >> b > ", '>', true, new[] { "a", "b" })]
    public void GetParametersBySplit_TrimsAndDropsEmptyParts(string text, char separator, bool toLower, string[] parts)
    {
        Assert.Equal(parts, ProbeCommand.Split(text, separator, toLower));
    }

    [Fact]
    public void GetParametersBySplit_TextSeparator_SplitsOnTheWholeText()
    {
        Assert.Equal(["pizza", "pasta"], ProbeCommand.Split("Pizza or Pasta", " or "));
    }

    [Theory]
    [InlineData("2000.03.04")]
    [InlineData("2000. 03. 04.")]
    [InlineData("2000-03-04")]
    [InlineData(@"2000\03/04")]
    [InlineData("2000, 03 04")]
    public void GetDateParameterParts_SplitsOnEverySeparator(string text)
    {
        Assert.Equal(["2000", "03", "04"], ProbeCommand.DateParts(text));
    }

    private void ServerHas(Dictionary<ChannelTypeEnum, List<ulong>> channels)
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server, SettingsChannels = channels });
    }

    private ProbeCommand Command(RecordingChannel commandChannel, IUser author)
    {
        ProbeCommand command = new(Substitute.For<IBotLogger>(), Substitute.For<IConfig>(), serverService);
        ((IModuleBase) command).SetContext(DiscordMocks.TextCommand("!probe", author, commandChannel.Channel));
        return command;
    }

    //Opens up BaseCommand's protected helpers; the bot's command service never sees it
    private sealed class ProbeCommand(IBotLogger logger, IConfig config, IServerService serverService) : BaseCommand(logger, config, serverService)
    {
        public Task<bool> IsAllowedAsync(ChannelTypeEnum type, bool allowLackOfType = true, bool canBeDM = false)
        {
            return IsCommandAllowedAsync(type, allowLackOfType, canBeDM);
        }

        public static string[] Split(string text, char separator, bool toLower = true)
        {
            return GetParametersBySplit(text, separator, toLower);
        }

        public static string[] Split(string text, string separator)
        {
            return GetParametersBySplit(text, separator);
        }

        public static string[] DateParts(string text)
        {
            return GetDateParameterParts(text);
        }
    }
}
