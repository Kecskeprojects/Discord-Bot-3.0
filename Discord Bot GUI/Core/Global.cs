using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using System.Collections.Concurrent;

namespace Discord_Bot.Core;

//Todo: these list may be placed somewhere else, or put into some form of singleton service
public static class Global
{
    public static ConcurrentDictionary<ulong, ServerAudioResource> ServerAudioResources { get; private set; } = [];
    public static ConcurrentDictionary<ulong, BiasGameData> BiasGames { get; private set; } = [];
}
