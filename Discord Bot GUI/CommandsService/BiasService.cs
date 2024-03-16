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
            foreach (IdolResource bias in list)
            {
                if (bias.GroupName != null)
                {
                    //Check if key exists for group, if not, make it
                    if (!groups.TryGetValue(bias.GroupName, out List<string> memberList))
                    {
                        groups.Add(bias.GroupName, [bias.Name.ToUpper()]);
                        continue;
                    }

                    memberList.Add(bias.Name.ToUpper());
                }
                else
                {
                    //Check if key exists for group, if not, make it
                    if (!groups.TryGetValue("unsorted", out List<string> memberList))
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
                List<string> keys = [.. groups.Keys];
                ComponentBuilder builder = new();
                while (Math.Ceiling(keys.Count / 25.0) > selectCount)
                {
                    int remaininglistCount = (groups.Keys.Count - (selectCount * 25)) > 25 ? (selectCount + (1 * 25)) : (selectCount * 25) + (groups.Keys.Count - (selectCount * 25));
                    //Make a selector out of all the groups and their members
                    SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Select a group")
                    .WithCustomId($"BiasMenu_{selectCount + 1}");

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
