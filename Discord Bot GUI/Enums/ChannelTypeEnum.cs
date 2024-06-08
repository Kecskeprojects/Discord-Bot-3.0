using Discord_Bot.Core;
using System;
using System.Linq;

namespace Discord_Bot.Enums;

public enum ChannelTypeEnum
{
    None = 0,
    RoleText = 1,
    TwitchNotificationText = 2,
    MusicText = 3,
    MusicVoice = 4,
    CommandText = 5,
    BirthdayText = 6,
}

public static class ChannelTypeEnumExtension
{
    public static bool IsRestrictedChannelType(this ChannelTypeEnum channelType)
    {
        return Constant.RestrictedChannelTypes.Contains(channelType);
    }

    public static string ToCommandString(this ChannelTypeEnum channelTypeEnum)
    {
        return channelTypeEnum switch
        {
            ChannelTypeEnum.RoleText => "role",
            ChannelTypeEnum.TwitchNotificationText => "notification",
            ChannelTypeEnum.MusicText => "music",
            ChannelTypeEnum.MusicVoice => "musicvoice",
            ChannelTypeEnum.CommandText => "command",
            ChannelTypeEnum.BirthdayText => "birthday",
            _ => "unspecified"
        };
    }

    public static bool EqualsCommandString(this ChannelTypeEnum channelTypeEnum, string value)
    {
        return value.Equals(channelTypeEnum.ToFriendlyString(), StringComparison.OrdinalIgnoreCase);
    }

    public static string ToFriendlyString(this ChannelTypeEnum channelTypeEnum)
    {
        return channelTypeEnum switch
        {
            ChannelTypeEnum.RoleText => "Role Channel",
            ChannelTypeEnum.TwitchNotificationText => "Notification Channel",
            ChannelTypeEnum.MusicText => "Music Channel",
            ChannelTypeEnum.MusicVoice => "Music Voice Channel",
            ChannelTypeEnum.CommandText => "Command Channel",
            ChannelTypeEnum.BirthdayText => "Birthday Channel",
            _ => "Unspecified Channel"
        };
    }
}