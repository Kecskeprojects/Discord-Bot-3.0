using DatabaseORM.Enums;
using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Tools;

//The channel types that !channel add and !channel remove accept
public sealed class ChannelTypeEnumToolsTests
{
    [Fact]
    public void GetCommandArray_ListsEveryTypeButNone()
    {
        Assert.Equal(["role", "notification", "music", "musicvoice", "command", "birthday"], ChannelTypeEnumTools.GetCommandArray());
    }

    [Fact]
    public void GetNameDictionary_NamesEveryTypeButNone()
    {
        Dictionary<ChannelTypeEnum, string> names = ChannelTypeEnumTools.GetNameDictionary();

        Assert.DoesNotContain(ChannelTypeEnum.None, names.Keys);
        Assert.Equal("Birthday Channel", names[ChannelTypeEnum.BirthdayText]);
        Assert.Equal(6, names.Count);
    }

    [Theory]
    [InlineData("music", ChannelTypeEnum.MusicText)]
    [InlineData("MusicVoice", ChannelTypeEnum.MusicVoice)]
    [InlineData("notification", ChannelTypeEnum.TwitchNotificationText)]
    public void TryGetEnumFromCommandText_KnownWordInAnyCase_GivesTheType(string text, ChannelTypeEnum expected)
    {
        Assert.True(ChannelTypeEnumTools.TryGetEnumFromCommandText(text, out ChannelTypeEnum? type));
        Assert.Equal(expected, type);
    }

    //"unspecified" is None's command text, which has no database row
    [Theory]
    [InlineData("unspecified")]
    [InlineData("chat")]
    public void TryGetEnumFromCommandText_NoneOrUnknown_GivesNothing(string text)
    {
        Assert.False(ChannelTypeEnumTools.TryGetEnumFromCommandText(text, out ChannelTypeEnum? type));
        Assert.Null(type);
    }
}
