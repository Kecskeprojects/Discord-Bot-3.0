using System;
using System.Collections.Generic;

namespace Discord_Bot.Communication;

//No finalizer: it holds only managed objects, which clean up after themselves
public sealed partial class ServerAudioResource(ulong serverDiscordId) : IDisposable
{
    private bool isDisposed;

    public ulong ServerDiscordId { get; set; } = serverDiscordId;
    public AudioVariables AudioVariables { get; set; } = new();
    public List<MusicRequest> MusicRequests { get; set; } = [];

    public void Dispose()
    {
        if (!isDisposed)
        {
            AudioVariables?.Dispose();
            isDisposed = true;
        }
    }
}
