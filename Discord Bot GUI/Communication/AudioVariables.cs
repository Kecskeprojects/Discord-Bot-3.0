using Discord.Audio;
using System.Diagnostics;

namespace Discord_Bot.Communication;

//No finalizer: it holds only managed objects, which clean up after themselves
public sealed partial class AudioVariables : IDisposable
{
    private bool isDisposed;

    public bool Playing { get; set; }

    public ulong FallbackVoiceChannelId { get; set; }

    public IAudioClient? AudioClient { get; set; }

    public Stopwatch? Stopwatch { get; set; }

    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public void Dispose()
    {
        if (!isDisposed)
        {
            //Null when no connection was made
            AudioClient?.Dispose();
            isDisposed = true;
        }
    }
}
