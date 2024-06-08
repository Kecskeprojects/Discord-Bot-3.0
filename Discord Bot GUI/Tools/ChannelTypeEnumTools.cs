using Discord_Bot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Tools;
public static class ChannelTypeEnumTools
{
    public static List<string> GetCommandArray()
    {
        ChannelTypeEnum[] values = (ChannelTypeEnum[]) Enum.GetValues(typeof(ChannelTypeEnum));
        return values.Select(x => x.ToCommandString()).ToList();
    }

    public static Dictionary<ChannelTypeEnum, string> GetNameDictionary()
    {
        ChannelTypeEnum[] values = (ChannelTypeEnum[]) Enum.GetValues(typeof(ChannelTypeEnum));
        return values.ToDictionary(x => x, x => x.ToFriendlyString());
    }

    public static bool TryGetEnumFromCommandText(string value, out ChannelTypeEnum? enumItem)
    {
        ChannelTypeEnum[] values = (ChannelTypeEnum[]) Enum.GetValues(typeof(ChannelTypeEnum));
        enumItem = values.FirstOrDefault(x => x.EqualsCommandString(value));
        return enumItem != null;
    }
}
