using Discord_Bot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Tools;
public static class ChannelTypeEnumTools
{
    public static List<string> GetCommandArray()
    {
        ChannelTypeEnum[] values = Enum.GetValues<ChannelTypeEnum>();
        return values.Where(x => x != ChannelTypeEnum.None).Select(x => x.ToCommandString()).ToList();
    }

    public static Dictionary<ChannelTypeEnum, string> GetNameDictionary()
    {
        ChannelTypeEnum[] values = Enum.GetValues<ChannelTypeEnum>();
        return values.Where(x => x != ChannelTypeEnum.None).ToDictionary(x => x, x => x.ToChannelString());
    }

    public static bool TryGetEnumFromCommandText(string value, out ChannelTypeEnum? enumItem)
    {
        ChannelTypeEnum[] values = Enum.GetValues<ChannelTypeEnum>();
        enumItem = values.FirstOrDefault(x => x.EqualsCommandString(value));
        return enumItem != null;
    }
}
