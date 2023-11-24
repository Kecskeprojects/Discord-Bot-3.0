using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Services;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Services
{
    public class YoutubeDownloadService(Logging logger) : IYoutubeDownloadService
    {
        private readonly Logging logger = logger;

        //Use yt-dlp and ffmpeg to stream audio from youtube to discord
        public Process CreateYoutubeStream(string url)
        {
            ProcessStartInfo ffmpeg = new()
            {
                FileName = "cmd.exe",
                Arguments = $@"/C yt-dlp.exe --no-check-certificate -f bestaudio -o - {url} | ffmpeg.exe -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Dependencies\\Audio")
            };

            logger.Log($"Yt-dlp audio stream created for '{url}'!");
            return Process.Start(ffmpeg);
        }
    }
}
