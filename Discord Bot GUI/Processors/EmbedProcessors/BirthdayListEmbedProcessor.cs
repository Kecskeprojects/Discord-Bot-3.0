using DatabaseORM.Resources;
using Discord;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class BirthdayListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<BirthdayResource> list, List<string> users)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle("Server birthdays:");

        List<string> embedFields = [""];
        int index = 0;
        int shown = 0;
        for (int i = 0; i < list.Count && embedFields.Count < 5; i++)
        {
            embedFields[index] += $"- **{list[i].Date:yyyy.MM.dd}**: {users[i]}\n";
            shown++;

            if (i > 0 && i % 15 == 0)
            {
                index++;
                embedFields.Add("");
            }
        }

        //The loop opens a new field after every 15th birthday, which stays empty when the list or the loop ends there,
        //and Discord refuses an empty field (16, 31, 46 or more than 60 birthdays failed the whole list until 2026-11-23)
        foreach (string field in embedFields.Where(field => field.Length > 0))
        {
            _ = builder.AddField("\u200b", field);
        }

        if (list.Count > shown)
        {
            _ = builder.WithFooter($"{list.Count - shown} more not shown");
        }
        _ = builder.WithColor(Color.LightOrange);
        return [builder.Build()];
    }
}
