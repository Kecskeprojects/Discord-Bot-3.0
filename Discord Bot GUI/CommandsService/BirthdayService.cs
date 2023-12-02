using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.CommandsService
{
    internal class BirthdayService
    {
        internal static EmbedBuilder BuildBirthdayListEmbed(List<BirthdayResource> list, List<string> users)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Server birthdays:");

            List<string> embedFields = [""];
            int index = 0;
            for (int i = 0; i < list.Count && embedFields.Count < 5; i++)
            {
                embedFields[index] += $"- **{list[i].Date:yyyy.MM.dd}**: {users[i]}\n";

                if (i > 0 && i % 15 == 0)
                {
                    index++;
                    embedFields.Add("");
                }
            }

            foreach (string field in embedFields)
            {
                builder.AddField("\u200b", field);
            }
            builder.WithColor(Color.LightOrange);
            return builder;
        }
    }
}
