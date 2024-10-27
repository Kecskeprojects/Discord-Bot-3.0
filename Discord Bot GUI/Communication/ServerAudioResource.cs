using System;
using System.Collections.Generic;

namespace Discord_Bot.Communication;

public partial class ServerAudioResource(ulong serverDiscordId) : IDisposable
{
    private bool _isDisposed;

    public ulong ServerDiscordId { get; set; } = serverDiscordId;
    public AudioVariables AudioVariables { get; set; } = new();
    public List<MusicRequest> MusicRequests { get; set; } = [];

    public void Dispose()
    {
        if (!_isDisposed)
        {
            AudioVariables.Dispose();
            _isDisposed = true;
        }
    }

    ~ServerAudioResource()
    {
        Dispose();
    }
}
