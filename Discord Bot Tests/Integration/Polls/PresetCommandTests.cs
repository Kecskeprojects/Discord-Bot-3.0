using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Polls;

//!weekly poll preset create/edit/remove/list through Discord.Net's command service, sent by the bot's owner, as presets are
//shared by every server's polls (until 2026-11-23 "Create Polls" on any server was enough)
public sealed class PresetCommandTests : IDisposable
{
    private readonly IWeeklyPollOptionPresetService presetService = Substitute.For<IWeeklyPollOptionPresetService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public PresetCommandTests()
    {
        host = new TestHost().Replace(presetService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData("!weekly poll preset create")]
    [InlineData("!wpp c")]
    public async Task Create_ShowsTheEditMessageForTheNewPreset(string command)
    {
        presetService.GetOrCreateDummyPresetAsync().Returns(new WeeklyPollOptionPresetResource { WeeklyPollOptionPresetId = 4 });

        CommandResult result = await RunAsync(command, member);

        Assert.True(result.IsSuccess, result.ErrorReason);
        SentMessage sent = Assert.Single(channel.Sent);
        Assert.Single(sent.Embeds);
        Assert.NotNull(sent.Components);
    }

    [Fact]
    public async Task Create_PresetCouldNotBeMade_SaysSo()
    {
        presetService.GetOrCreateDummyPresetAsync().Returns((WeeklyPollOptionPresetResource?) null);

        await RunAsync("!weekly poll preset create", member);

        Assert.Equal("The preset could not be created.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task Edit_PresetByName_ShowsItsEditMessage()
    {
        presetService.GetPresetByNameAsync("days").Returns(new WeeklyPollOptionPresetResource { WeeklyPollOptionPresetId = 4, Name = "days" });

        await RunAsync("!weekly poll preset edit days", member);

        Assert.Single(Assert.Single(channel.Sent).Embeds);
    }

    [Fact]
    public async Task Edit_UnknownPreset_SaysSo()
    {
        await RunAsync("!weekly poll preset edit nothing", member);

        Assert.Equal("Preset not found!", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "The 'days' preset has been removed.")]
    [InlineData(DbProcessResultEnum.NotFound, "Preset does not exist.")]
    [InlineData(DbProcessResultEnum.InUse, "The 'days' preset is still used by a weekly poll. Switch that poll to another preset or to its own answers first.")]
    [InlineData(DbProcessResultEnum.Failure, "Preset could not be removed!")]
    public async Task Remove_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        presetService.RemovePresetByNameAsync("days").Returns(outcome);

        await RunAsync("!weekly poll preset remove days", member);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task List_SendsThePresets()
    {
        presetService.GetPresetsAsync().Returns([new WeeklyPollOptionPresetResource { Name = "days", Description = "The days of the week" }]);

        await RunAsync("!wpp l", member);

        Assert.Single(Assert.Single(channel.Sent).Embeds);
    }

    [Fact]
    public async Task Commands_SomeoneOtherThanTheOwner_AreRefusedByTheCommandService()
    {
        IGuildUser other = DiscordMocks.GuildUser(DiscordIds.OtherUser);
        other.GetPermissions(Arg.Any<IGuildChannel>()).Returns(new ChannelPermissions((ulong) ChannelPermission.SendPolls));

        CommandResult result = await RunAsync("!weekly poll preset remove days", other);

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        await presetService.DidNotReceiveWithAnyArgs().RemovePresetByNameAsync(default!);
    }

    //The bot's owner is the default member
    private Task<CommandResult> RunAsync(string text, IGuildUser author)
    {
        IUser owner = DiscordMocks.User(DiscordIds.User);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(owner);
        ICommandContext context = DiscordMocks.TextCommand(text, author, channel.Channel);
        context.Client.TokenType.Returns(TokenType.Bot);
        context.Client.GetApplicationInfoAsync().Returns(application);
        return host.RunCommandAsync(context);
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
