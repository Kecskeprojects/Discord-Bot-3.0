using Discord;
using Discord.Commands;
using System.IO;

namespace Discord_Bot.Tools;

public static class RainbowRandomizerTools
{

    public static List<string> GetOperatorList(string optionString)
    {
        List<string> operatorList = [];
        if (optionString is "a" or "attacker")
        {
            operatorList = File.ReadAllLines("R6_Attackers.txt").ToList();
        }
        else if (optionString is "d" or "defender")
        {
            operatorList = File.ReadAllLines("R6_Defenders.txt").ToList();
        }
        return operatorList;
    }

    //Everyone in the sender's voice channel, or only the sender outside one (or in a DM)
    public static async Task<List<IUser>> GetUserListAsync(ICommandContext context)
    {
        List<IUser> userList = [];
        if (context.User is IGuildUser { VoiceChannel: not null } sender)
        {
            //From the member cache, the way Discord.Net works out a voice channel's connected users
            IReadOnlyCollection<IGuildUser> members = await context.Guild.GetUsersAsync(CacheMode.CacheOnly);
            userList.AddRange(members.Where(member => member.VoiceChannel?.Id == sender.VoiceChannel.Id));
        }
        else
        {
            userList.Add(context.User);
        }
        return userList;
    }

    //A different operator for each user; the caller's Random decides which (the injected one, so tests can)
    public static List<string> GetResponseForRound(List<string> operatorList, List<IUser> userList, Random random)
    {
        //Ensuring the original list remains intact
        List<string> tempOperatorList = [];
        tempOperatorList.AddRange(operatorList);

        List<string> responseParts = [];
        foreach (IUser user in userList)
        {
            int index = random.Next(0, tempOperatorList.Count);
            string chosen = tempOperatorList[index];
            responseParts.Add($"{user.GlobalName ?? user.Username}: {chosen}");
            tempOperatorList.RemoveAt(index);
        }
        return responseParts;
    }
}
