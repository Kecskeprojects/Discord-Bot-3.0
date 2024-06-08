using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using System.Collections.Concurrent;

namespace Discord_Bot.Core;

public static class Global
{
    public static ConcurrentDictionary<ulong, ServerAudioResource> ServerAudioResources { get; private set; } = [];
    public static ConcurrentDictionary<ulong, BiasGameData> BiasGames { get; private set; } = [];
    public static ConcurrentDictionary<string, int> YoutubeApiKeys { get; private set; } = [];
}
