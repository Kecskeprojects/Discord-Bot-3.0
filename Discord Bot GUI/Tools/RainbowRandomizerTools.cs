using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    public static List<SocketUser> GetUserList(SocketCommandContext context)
    {
        List<SocketVoiceChannel> voiceChannels = context.Guild?.Channels?.Where(x => x is SocketVoiceChannel).Select(x => (SocketVoiceChannel) x).ToList();
        SocketVoiceChannel currentVoiceChannel = voiceChannels?.FirstOrDefault(x => x.ConnectedUsers.Any(y => y.Id == context.User.Id));

        List<SocketUser> userList = [];
        if (currentVoiceChannel is not null)
        {
            userList.AddRange(currentVoiceChannel.ConnectedUsers);
        }
        else
        {
            userList.Add(context.User);
        }
        return userList;
    }

    public static List<string> GetResponseForRound(List<string> operatorList, List<SocketUser> userList)
    {
        //Ensuring the original list remains intact
        List<string> tempOperatorList = [];
        tempOperatorList.AddRange(operatorList);

        Random r = new();
        List<string> responseParts = [];
        foreach (SocketUser user in userList)
        {
            int index = r.Next(0, tempOperatorList.Count);
            string chosen = tempOperatorList[index];
            responseParts.Add($"{user.GlobalName ?? user.Username}: {chosen}");
            tempOperatorList.RemoveAt(index);
        }
        return responseParts;
    }
}
