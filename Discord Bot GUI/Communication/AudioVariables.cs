﻿using Discord.Audio;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Communication
{
    public class AudioVariables
    {
        public bool Playing { get; set; }

        public bool AbruptDisconnect { get; set; }

        public ulong FallbackVoiceChannelId { get; set; }

        public IAudioClient AudioClient { get; set; }

        public Process FFmpeg { get; set; }

        public Stream Output { get; set; }

        public AudioOutStream Discord { get; set; }

        public Stopwatch Stopwatch { get; set; }


        public AudioVariables()
        {
            AbruptDisconnect = false;
            Playing = false;
        }
    }
}
