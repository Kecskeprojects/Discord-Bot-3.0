using DatabaseORM.Enums;
using System.Diagnostics.CodeAnalysis;

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

    //None is not a real channel type (it has no database row), so its command text is not accepted
    public static bool TryGetEnumFromCommandText(string value, [NotNullWhen(true)] out ChannelTypeEnum? enumItem)
    {
        foreach (ChannelTypeEnum item in Enum.GetValues<ChannelTypeEnum>())
        {
            if (item != ChannelTypeEnum.None && item.EqualsCommandString(value))
            {
                enumItem = item;
                return true;
            }
        }

        enumItem = null;
        return false;
    }
}
