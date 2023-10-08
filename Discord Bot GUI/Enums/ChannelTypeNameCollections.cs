using System.Collections.Generic;

namespace Discord_Bot.Enums
{
    public class ChannelTypeNameCollections
    {
        public static Dictionary<string, ChannelTypeEnum> NameEnum { get; } = new()
        {
            { "role", ChannelTypeEnum.RoleText },
            { "notification", ChannelTypeEnum.TwitchNotificationText },
            { "music", ChannelTypeEnum.MusicText },
            { "musicvoice", ChannelTypeEnum.MusicVoice },
            { "command", ChannelTypeEnum.CommandText },
            { "birthday", ChannelTypeEnum.BirthdayText },
        };
        public static Dictionary<ChannelTypeEnum, string> EnumName { get; } = new()
        {
            { ChannelTypeEnum.RoleText, "Role Channel" },
            { ChannelTypeEnum.TwitchNotificationText, "Notification Channel" },
            { ChannelTypeEnum.MusicText, "Music Channel" },
            { ChannelTypeEnum.MusicVoice, "Music Voice Channel" },
            { ChannelTypeEnum.CommandText, "Command Channel" },
            { ChannelTypeEnum.BirthdayText, "Birthday Channel" },
        };

        public static ChannelTypeEnum[] RestrictedChannelTypes { get; } = new ChannelTypeEnum[]
        {
            ChannelTypeEnum.BirthdayText,
            ChannelTypeEnum.TwitchNotificationText,
            ChannelTypeEnum.RoleText
        };
    }
}
