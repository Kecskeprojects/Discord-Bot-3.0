using Discord;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using System.Collections.Concurrent;

namespace Discord_Bot.Core;

//Todo: remove all functions from here when it becomes possible, these list may be placed somewhere else, or put into some form of singleton service
public static class Global
{
    public static ConcurrentDictionary<ulong, ServerAudioResource> ServerAudioResources { get; private set; } = [];
    public static ConcurrentDictionary<ulong, BiasGameData> BiasGames { get; private set; } = [];

    //Check if user has a nickname
    public static string GetNickName(ISocketMessageChannel channel, SocketUser user)
    {
        return channel.GetChannelType() != ChannelType.DM ?
            (user as SocketGuildUser).Nickname ?? user.Username :
            user.Username;
    }
}
