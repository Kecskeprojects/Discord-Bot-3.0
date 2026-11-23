using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Communication.Bias;

namespace Discord_Bot.Processors.MessageProcessor;

public static class BiasListMessageProcessor
{
    public static BiasMessageResult BuildBiasMessage(List<IdolResource> list, string groupName, string headMessage, ulong userId, bool isUser)
    {
        SortedDictionary<string, List<string>> groups = [];
        foreach (IdolResource bias in list)
        {
            if (bias.GroupName != null)
            {
                //Check if key exists for group, if not, make it
                if (!groups.TryGetValue(bias.GroupName, out List<string>? memberList))
                {
                    groups.Add(bias.GroupName, [bias.Name.ToUpper()]);
                    continue;
                }

                memberList.Add(bias.Name.ToUpper());
            }
            else
            {
                //Check if key exists for group, if not, make it
                if (!groups.TryGetValue("unsorted", out List<string>? memberList))
                {
                    groups.Add("unsorted", [bias.Name.ToUpper()]);
                    continue;
                }

                memberList.Add(bias.Name.ToUpper());
            }
        }

        if (groupName != "")
        {
            //Make a list out of all the groups and their members
            string message = "";
            foreach (KeyValuePair<string, List<string>> group in groups)
            {
                //Add Group name
                message += $"{group.Key.ToUpper()}:\n";

                //Add individual members
                foreach (string member in group.Value)
                {
                    if (member != group.Value[0])
                    {
                        message += ", ";
                    }

                    message += $"`{member}`";
                }
                message += "\n";
            }

            return new BiasMessageResult() { Message = message };
        }
        else
        {
            int selectCount = 0;
            //A message holds 5 rows, so 5 menus of 25 groups (more failed the whole list until 2026-11-23); the rest can still be named
            List<string> keys = [.. groups.Keys.Take(ComponentBuilder.MaxActionRowCount * SelectMenuBuilder.MaxOptionCount)];
            ComponentBuilder builder = new();
            while (Math.Ceiling(keys.Count / 25.0) > selectCount)
            {
                //Each select menu holds up to 25 groups; this one ends at the next multiple of 25, or at the last group
                int pageEnd = Math.Min((selectCount + 1) * 25, keys.Count);
                //Make a selector out of all the groups and their members
                SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select a group")
                .WithCustomId($"BiasMenu_{selectCount + 1}");

                for (int i = 25 * selectCount; i < pageEnd; i++)
                {
                    _ = menuBuilder.AddOption(keys[i].ToUpper(), keys[i] + (isUser ? $"><{userId}" : ""), $"{groups[keys[i]].Count} biases...");
                }

                _ = builder.WithSelectMenu(menuBuilder);

                selectCount++;
            }

            string message = groups.Count > keys.Count ? $"{headMessage}\n{groups.Count - keys.Count} more not shown, give a group's name to see its members" : headMessage;
            return new BiasMessageResult() { Message = message, Component = builder.Build() };
        }
    }
}
