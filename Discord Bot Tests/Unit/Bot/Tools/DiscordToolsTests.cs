using DatabaseORM.Enums;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Tools;

public sealed class DiscordToolsTests
{
    [Fact]
    public void UtcTimestamp_ReadsADateWithoutAKindAsUtc()
    {
        //How SQL Server hands back a stored UTC time: 14:00 with no DateTimeKind
        DateTime fromDatabase = new(2026, 11, 18, 14, 0, 0, DateTimeKind.Unspecified);

        TimestampTag tag = DiscordTools.UtcTimestamp(fromDatabase, TimestampTagStyles.ShortDateTime);

        //1795010400 is 2026-11-18 14:00 UTC
        Assert.Equal("<t:1795010400:f>", tag.ToString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsTypeOfChannel_WhenTheServerSetNoChannelOfThatType_FollowsAllowLackOfType(bool allowLackOfType)
    {
        ServerResource server = new();

        Assert.Equal(allowLackOfType, DiscordTools.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, DiscordIds.Channel, allowLackOfType));
    }

    [Theory]
    [InlineData(DiscordIds.Channel, true)]
    [InlineData(DiscordIds.OtherChannel, false)]
    public void IsTypeOfChannel_WhenTheServerSetChannelsOfThatType_AllowsOnlyThose(ulong channelId, bool expected)
    {
        ServerResource server = new() { SettingsChannels = { [ChannelTypeEnum.CommandText] = [DiscordIds.Channel] } };

        Assert.Equal(expected, DiscordTools.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, channelId));
    }
}
