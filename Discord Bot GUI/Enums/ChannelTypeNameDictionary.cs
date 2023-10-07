using System.Collections.Generic;

namespace Discord_Bot.Enums
{
    public class ChannelTypeNameDictionary
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
            { ChannelTypeEnum.MusicText, "Music Channels" },
            { ChannelTypeEnum.MusicVoice, "Music Voice Channels" },
            { ChannelTypeEnum.CommandText, "Command Channels" },
            { ChannelTypeEnum.BirthdayText, "Birthday Channel" },
        };
    }
}
