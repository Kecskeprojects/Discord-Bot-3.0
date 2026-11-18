using Discord;
using Discord_Bot.Communication.Bias;

namespace Discord_Bot.Processors.EmbedProcessors.BiasGame;

public static class BiasGameDebutEmbedProcessor
{
    public static MessageComponent CreateComponent(BiasGameData data, int currentYear)
    {
        List<int> options = [1990, 2000, 2010, 2014, 2018];
        for (int i = 2020; i <= currentYear; i += 2)
        {
            options.Add(i);
        }

        if ((currentYear - 2020) % 2 > 0)
        {
            options.Add(currentYear);
        }

        SelectMenuBuilder selectMenu = new();
        _ = selectMenu.WithCustomId($"BiasGame_Setup_Debut_{data.UserId}");
        _ = selectMenu.WithPlaceholder("Select TWO years as a start and end date!");
        _ = selectMenu.WithMinValues(2);
        _ = selectMenu.WithMaxValues(2);

        options.ForEach(y => selectMenu.AddOption(y.ToString(), y.ToString()));

        ComponentBuilder components = new();
        _ = components.WithSelectMenu(selectMenu);
        return components.Build();
    }
}
