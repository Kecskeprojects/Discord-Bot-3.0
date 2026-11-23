using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Twitch;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.ServerSetup;

//!server settings, !channel add/remove, !twitch add/remove and !twitch role add/remove, sent by an administrator in a server channel
public sealed class ServerSettingCommandTests : IDisposable
{
    private const string ChannelTypes = "role, notification, music, musicvoice, command, birthday";

    private readonly IChannelService channelService = Substitute.For<IChannelService>();
    private readonly ITwitchChannelService twitchChannelService = Substitute.For<ITwitchChannelService>();
    private readonly ITwitchCLI twitchCLI = Substitute.For<ITwitchCLI>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser admin = DiscordMocks.GuildUser(DiscordIds.OtherUser);
    private readonly RecordingChannel channel = RecordingChannel.Text();
    private readonly ITextChannel musicChannel = Substitute.For<ITextChannel>();

    public ServerSettingCommandTests()
    {
        host = new TestHost().Replace(channelService).Replace(twitchChannelService).Replace(twitchCLI).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        channelService.AddSettingChannelAsync(default, default, default).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        twitchChannelService.AddTwitchChannelAsync(default, default!, default!, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);
        twitchChannelService.RemoveTwitchChannelAsync(default, default!).ReturnsForAnyArgs(DbProcessResultEnum.Success);

        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));
        ((ITextChannel) channel.Channel).Guild.Returns(guild);

        //Discord.Net's channel reader finds a channel by its name (or mention or ID) among the server's cached channels
        musicChannel.Id.Returns(DiscordIds.OtherChannel);
        musicChannel.Name.Returns("music");
        guild.GetChannelAsync(DiscordIds.OtherChannel, CacheMode.CacheOnly).Returns(musicChannel);
        guild.GetChannelsAsync(CacheMode.CacheOnly).Returns([musicChannel]);
    }

    [Fact]
    public async Task ServerSettings_GivesTheSettingsWithTheChannelsNames()
    {
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource
        {
            DiscordId = DiscordIds.Server,
            SettingsChannels = new() { [ChannelTypeEnum.MusicText] = [DiscordIds.OtherChannel] }
        });
        guild.GetTextChannelsAsync(CacheMode.CacheOnly).Returns([musicChannel]);

        CommandResult result = await RunAsync("!server settings");

        Assert.True(result.IsSuccess, result.ErrorReason);
        Embed embed = Assert.Single(Assert.Single(channel.Sent).Embeds);
        Assert.Equal("The server's settings are the following:", embed.Title);
        Assert.Equal("``music``", embed.Fields.Single(field => field.Name == "Music Channel:").Value);
    }

    [Fact]
    public async Task ServerSettings_AskedByAMember_IsRefusedByTheCommandService()
    {
        CommandResult result = await host.RunCommandAsync(DiscordMocks.TextCommand("!server settings", DiscordMocks.GuildUser(), channel.Channel));

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        Assert.Empty(channel.Sent);
    }

    [Theory]
    [InlineData("!channel add")]
    [InlineData("!channel remove")]
    public async Task Channel_NoType_ListsTheTypes(string command)
    {
        await RunAsync(command);

        Assert.Equal($"Current types: {ChannelTypes}", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task ChannelAdd_UnknownType_SaysSoAndListsTheTypes()
    {
        await RunAsync("!channel add memes music");

        await channelService.DidNotReceiveWithAnyArgs().AddSettingChannelAsync(default, default, default);
        Assert.Equal($"Channel type does not currently exist.\nCurrent types: {ChannelTypes}", Assert.Single(channel.Sent).Text);
    }

    //Notification channels are one per server, so a new one replaces the old
    [Theory]
    [InlineData("music", ChannelTypeEnum.MusicText, "Server settings updated.")]
    [InlineData("notification", ChannelTypeEnum.TwitchNotificationText, "Server settings updated! Previous channel (if there was one) was overwritten as only one of it's type is allowed.")]
    public async Task ChannelAdd_TypeAndChannelName_SavesThem(string type, ChannelTypeEnum channelType, string reply)
    {
        CommandResult result = await RunAsync($"!channel add {type} music");

        Assert.True(result.IsSuccess, result.ErrorReason);
        await channelService.Received().AddSettingChannelAsync(DiscordIds.Server, channelType, DiscordIds.OtherChannel);
        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    //It answered nothing until 2026-11-23
    [Fact]
    public async Task ChannelAdd_NoChannel_SaysToNameOne()
    {
        await RunAsync("!channel add music");

        await channelService.DidNotReceiveWithAnyArgs().AddSettingChannelAsync(default, default, default);
        Assert.Equal("Name the channel too: `!channel add music [channel name]`", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task ChannelRemove_TypeAndChannelName_RemovesThatChannel()
    {
        channelService.RemoveSettingChannelAsync(DiscordIds.Server, ChannelTypeEnum.MusicText, DiscordIds.OtherChannel).Returns(DbProcessResultEnum.Success);

        await RunAsync("!channel remove music music");

        Assert.Equal("Server settings updated.", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Server settings updated.")]
    [InlineData(DbProcessResultEnum.NotFound, "This channel is not currently set for this type.")]
    public async Task ChannelRemove_TypeOnly_RemovesEveryChannelOfIt(DbProcessResultEnum outcome, string reply)
    {
        channelService.RemoveSettingChannelsAsync(DiscordIds.Server, ChannelTypeEnum.MusicText).Returns(outcome);

        await RunAsync("!channel remove music");

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("tester")]
    [InlineData("https://www.twitch.tv/tester")]
    [InlineData("https://www.twitch.tv/tester/videos")]
    public async Task TwitchAdd_NameOrLink_AddsTheChannelUnderItsLogin(string channelOrLink)
    {
        twitchCLI.GetChannel("tester").Returns(new UserData { Id = "123", Login = "tester" });

        await RunAsync($"!twitch add {channelOrLink}");

        await twitchChannelService.Received().AddTwitchChannelAsync(DiscordIds.Server, "123", "tester", "https://www.twitch.tv/tester");
        Assert.Equal("Twitch channel added.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task TwitchAdd_LinkWithoutAChannel_SaysSo()
    {
        await RunAsync("!twitch add https://www.twitch.tv/");

        twitchCLI.DidNotReceiveWithAnyArgs().GetChannel(default!);
        Assert.Equal("Url is not channel url!", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task TwitchAdd_UnknownUser_SaysSo()
    {
        twitchCLI.GetChannel("nobody").Returns((UserData?) null);

        await RunAsync("!twitch add nobody");

        await twitchChannelService.DidNotReceiveWithAnyArgs().AddTwitchChannelAsync(default, default!, default!, default!);
        Assert.Equal("Twitch user not found!", Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData("tester")]
    [InlineData("https://www.twitch.tv/tester")]
    public async Task TwitchRemove_NameOrLink_RemovesThatChannel(string channelOrLink)
    {
        await RunAsync($"!twitch remove {channelOrLink}");

        await twitchChannelService.Received().RemoveTwitchChannelAsync(DiscordIds.Server, "tester");
        Assert.Equal("Twitch channel(s) removed.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task TwitchRemove_NoChannel_RemovesAllOfTheServers()
    {
        await RunAsync("!twitch remove");

        await twitchChannelService.Received().RemoveTwitchChannelAsync(DiscordIds.Server, "");
    }

    [Fact]
    public async Task TwitchRoleAdd_Role_SetsItAsTheNotificationRole()
    {
        IRole role = Substitute.For<IRole>();
        role.Id.Returns(DiscordIds.Role);
        role.Name.Returns("streams");
        guild.GetRole(DiscordIds.Role).Returns(role);
        serverService.AddNotificationRoleAsync(DiscordIds.Server, DiscordIds.Role, "streams").Returns(DbProcessResultEnum.Success);

        await RunAsync($"!twitch role add <@&{DiscordIds.Role}>");

        Assert.Equal("Notification role updated.", Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task TwitchRoleRemove_RemovesTheNotificationRole()
    {
        serverService.RemoveNotificationRoleAsync(DiscordIds.Server).Returns(DbProcessResultEnum.Success);

        await RunAsync("!twitch role remove");

        Assert.Equal("Notification role removed.", Assert.Single(channel.Sent).Text);
    }

    private Task<CommandResult> RunAsync(string text)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, admin, channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
