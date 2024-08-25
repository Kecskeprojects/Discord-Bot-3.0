using Discord.Audio;
using System.Diagnostics;
using System.Threading;

namespace Discord_Bot.Communication;

public class AudioVariables
{
    public bool Playing { get; set; } = false;

    public ulong FallbackVoiceChannelId { get; set; }

    public IAudioClient AudioClient { get; set; }

    public Stopwatch Stopwatch { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }
}
