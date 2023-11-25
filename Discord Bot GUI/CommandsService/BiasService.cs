using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;

namespace Discord_Bot.CommandsService
{
    public class BiasService
    {
        public static BiasMessageResult BuildBiasMessage(List<IdolResource> list, string groupName, string headMessage, ulong userId, bool isUser)
        {
            SortedDictionary<string, List<string>> groups = [];
            foreach (var bias in list)
            {
                if (bias.Group != null)
                {
                    //Check if key exists for group, if not, make it
                    if (!groups.ContainsKey(bias.Group.Name)) groups.Add(bias.Group.Name, []);

                    //We make the name uppercase when adding
                    groups[bias.Group.Name].Add(bias.Name.ToUpper());
                }
                else
                {
                    //Check if key exists for group, if not, make it
                    if (!groups.ContainsKey("unsorted")) groups.Add("unsorted", []);

                    //We make the name uppercase when adding
                    groups["unsorted"].Add(bias.Name.ToUpper());
                }
            }

            if (groupName != "")
            {
                //Make a list out of all the groups and their members
                string message = "";
                foreach (var group in groups)
                {
                    //Add Group name
                    message += $"{group.Key.ToUpper()}:\n";

                    //Add individual members
                    foreach (var member in group.Value)
                    {
                        if (member != group.Value[0]) message += ", ";

                        message += $"`{member}`";
                    }
                    message += "\n";
                }

                return new BiasMessageResult() { Message = message };
            }
            else
            {
                int selectCount = 0;
                List<string> keys = [.. groups.Keys];
                var builder = new ComponentBuilder();
                while (Math.Ceiling(keys.Count / 25.0) > selectCount)
                {
                    int remaininglistCount = (groups.Keys.Count - selectCount * 25) > 25 ? (selectCount + 1 * 25) : selectCount * 25 + (groups.Keys.Count - selectCount * 25);
                    //Make a selector out of all the groups and their members
                    var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Select a group")
                    .WithCustomId($"biasMenu_{selectCount + 1}");

                    for (int i = 25 * selectCount; i < remaininglistCount; i++)
                    {
                        menuBuilder.AddOption(keys[i].ToUpper(), keys[i] + (isUser ? $"><{userId}" : ""), $"{groups[keys[i]].Count} biases...");
                    }

                    builder.WithSelectMenu(menuBuilder);

                    selectCount++;
                }

                return new BiasMessageResult() { Message = headMessage, Builder = builder };
            }
        }
    }
}
