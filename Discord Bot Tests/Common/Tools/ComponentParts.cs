using Discord;

namespace Discord_Bot.Tests.Common.Tools;

//The buttons and select menus inside a message's action rows, in the order Discord shows them
public static class ComponentParts
{
    public static List<ButtonComponent> Buttons(MessageComponent component)
    {
        return [.. component.Components.OfType<ActionRowComponent>().SelectMany(row => row.Components).OfType<ButtonComponent>()];
    }

    public static List<SelectMenuComponent> SelectMenus(MessageComponent component)
    {
        return [.. component.Components.OfType<ActionRowComponent>().SelectMany(row => row.Components).OfType<SelectMenuComponent>()];
    }
}
