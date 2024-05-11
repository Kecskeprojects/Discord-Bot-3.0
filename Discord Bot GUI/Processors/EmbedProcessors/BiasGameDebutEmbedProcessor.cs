using Discord;
using Discord_Bot.Communication.Bias;
using System;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public class BiasGameDebutEmbedProcessor
    {
        public static ComponentBuilder CreateEmbed(BiasGameData data)
        {
            List<int> options = [1990, 2000, 2010, 2014, 2018];
            for (int i = 2020; i <= DateTime.UtcNow.Year; i += 2)
            {
                options.Add(i);
            }

            if ((DateTime.UtcNow.Year - 2020) % 2 > 0)
            {
                options.Add(DateTime.UtcNow.Year);
            }

            SelectMenuBuilder selectMenu = new();
            selectMenu.WithCustomId($"BiasGame_Setup_Debut_{data.UserId}");
            selectMenu.WithPlaceholder("Select TWO years as a start and end date!");
            selectMenu.WithMinValues(2);
            selectMenu.WithMaxValues(2);

            options.ForEach(y => selectMenu.AddOption(y.ToString(), y.ToString()));

            ComponentBuilder components = new();
            components.WithSelectMenu(selectMenu);
            return components;
        }
    }
}
