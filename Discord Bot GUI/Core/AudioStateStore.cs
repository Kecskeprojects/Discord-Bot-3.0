using Discord_Bot.Communication;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Discord_Bot.Core;

/// <summary>
/// The music state of each server: its request queue and its voice connection.
/// </summary>
public sealed class AudioStateStore
{
    private readonly ConcurrentDictionary<ulong, ServerAudioResource> servers = [];

    public ServerAudioResource GetOrAdd(ulong serverId)
    {
        return servers.GetOrAdd(serverId, id => new ServerAudioResource(id));
    }

    public bool TryGet(ulong serverId, [NotNullWhen(true)] out ServerAudioResource? audioResource)
    {
        return servers.TryGetValue(serverId, out audioResource);
    }

    //Stops the playback in every server, when the bot shuts down or Discord closes the connection
    public void CancelAll()
    {
        foreach (ServerAudioResource audioResource in servers.Values)
        {
            audioResource.AudioVariables.CancellationTokenSource?.Cancel(false);
        }
    }
}
