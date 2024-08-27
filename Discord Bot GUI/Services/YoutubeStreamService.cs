using Discord.Audio;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Services;

public class YoutubeStreamService(Config config, BotLogger logger) : IYoutubeDownloadService
{
    private readonly Config config = config;
    private readonly BotLogger logger = logger;

    public async Task StreamAsync(ServerAudioResource audioResource, string url)
    {
        Process ffmpeg = CreateYoutubeStream(url);
        try
        {
            audioResource.AudioVariables.Stopwatch = Stopwatch.StartNew();
            audioResource.AudioVariables.CancellationTokenSource = new();

            //Audio streaming
            using (AudioOutStream stream = audioResource.AudioVariables.AudioClient.CreatePCMStream(AudioApplication.Mixed, config.Bitrate, 3000))
            {
                logger.Log("Audio stream starting!");
                await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream, audioResource.AudioVariables.CancellationTokenSource.Token);
                await stream.FlushAsync();
            };
        }
        //Exception thrown with current version of skipping song or bot disconnecting from channel
        catch (OperationCanceledException ex)
        {
            logger.Log("Exception thrown when audio stream is cancelled!");
            logger.Warning("YoutubeStreamService.cs StreamAsync", ex, LogOnly: true);

            ffmpeg?.Kill();
        }
        catch (Exception ex)
        {
            logger.Error("YoutubeStreamService.cs StreamAsync", ex);
        }
        finally
        {
            if (ffmpeg?.HasExited == false)
            {
                ffmpeg?.WaitForExit(new TimeSpan(0, 1, 0));
            }
            ffmpeg.Dispose();
            audioResource.AudioVariables.Stopwatch?.Stop();

            logger.Log("Audio stream finished!");
        }
    }

    //Use yt-dlp and ffmpeg to stream audio from youtube to discord
    private Process CreateYoutubeStream(string url)
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
