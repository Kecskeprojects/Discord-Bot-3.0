using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.MessageProcessor;
public static class BiasListGroupMemberMessageProcessor
{
    public static string CreateMessage(string[] selectedIdolGroups, List<IdolResource> idols)
    {
        string message = "";

        //Add Group name
        message += $"{selectedIdolGroups[0].Split("><")[0].ToUpper()}:\n";

        //Add individual members
        foreach (IdolResource member in idols)
        {
            if (member != idols[0])
            {
                message += ", ";
            }

            message += $"`{member.Name.ToUpper()}`";
        }

        return message;
    }
}
