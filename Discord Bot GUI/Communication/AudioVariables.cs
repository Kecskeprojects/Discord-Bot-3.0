using Discord.Audio;
using System;
using System.Diagnostics;
using System.Threading;

namespace Discord_Bot.Communication;

public partial class AudioVariables : IDisposable
{
    private bool _isDisposed;

    public bool Playing { get; set; } = false;

    public ulong FallbackVoiceChannelId { get; set; }

    public IAudioClient AudioClient { get; set; }

    public Stopwatch Stopwatch { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            //Null when no connection was made, this also runs from the finalizer, where an exception would crash the whole process
            AudioClient?.Dispose();
            _isDisposed = true;
        }
    }

    ~AudioVariables()
    {
        Dispose();
    }
}
