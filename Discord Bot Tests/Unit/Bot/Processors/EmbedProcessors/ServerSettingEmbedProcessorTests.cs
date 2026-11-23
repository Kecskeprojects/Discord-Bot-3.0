using DatabaseORM.Enums;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors;

//!server settings
public sealed class ServerSettingEmbedProcessorTests
{
    [Fact]
    public void CreateEmbed_GivesEachChannelTypeItsChannelsByName()
    {
        ServerResource server = new()
        {
            SettingsChannels = new() { [ChannelTypeEnum.MusicText] = [DiscordIds.Channel, DiscordIds.OtherChannel], [ChannelTypeEnum.RoleText] = [DiscordIds.Channel] }
        };

        Embed embed = Assert.Single(ServerSettingEmbedProcessor.CreateEmbed(server, [Channel(DiscordIds.Channel, "music"), Channel(DiscordIds.OtherChannel, "music-2")], TestValues.AvatarUrl));

        //Discord shows the doubled backticks as one code span
        Assert.Equal("``music, music-2``", Field(embed, "Music Channel:"));
        Assert.Equal("``music``", Field(embed, "Role Channel:"));
        Assert.Equal("`none`", Field(embed, "Birthday Channel:"));
        Assert.Equal(TestValues.AvatarUrl, embed.Thumbnail?.Url);
    }

    [Fact]
    public void CreateEmbed_GivesTheRolesAndTwitchChannelsOrNone()
    {
        ServerResource server = new()
        {
            MuteRoleName = "muted",
            TwitchChannels = [new() { TwitchId = "123", TwitchLink = "https://www.twitch.tv/first" }, new() { TwitchId = "456", TwitchLink = "https://www.twitch.tv/second" }]
        };

        Embed embed = Assert.Single(ServerSettingEmbedProcessor.CreateEmbed(server, [], TestValues.AvatarUrl));

        Assert.Equal("`muted`", Field(embed, "Mute role:"));
        Assert.Equal("`none`", Field(embed, "Notification role:"));
        Assert.Equal("`123,456`", Field(embed, "Notified Twitch Channel IDs:"));
        Assert.Equal("`https://www.twitch.tv/first,https://www.twitch.tv/second`", Field(embed, "Notified Twitch channel URLs:"));
    }

    private static ITextChannel Channel(ulong id, string name)
    {
        ITextChannel channel = Substitute.For<ITextChannel>();
        channel.Id.Returns(id);
        channel.Name.Returns(name);
        return channel;
    }

    private static string Field(Embed embed, string name)
    {
        return embed.Fields.Single(field => field.Name == name).Value;
    }
}
