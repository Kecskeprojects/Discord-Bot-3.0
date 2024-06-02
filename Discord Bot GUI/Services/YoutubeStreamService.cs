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
        try
        {
            audioResource.AudioVariables.FFmpeg = CreateYoutubeStream(url);
            audioResource.AudioVariables.Output = audioResource.AudioVariables.FFmpeg.StandardOutput.BaseStream;

            audioResource.AudioVariables.Stopwatch = Stopwatch.StartNew();

            //Audio streaming
            using (audioResource.AudioVariables.Discord = audioResource.AudioVariables.AudioClient.CreatePCMStream(AudioApplication.Mixed, config.Bitrate, 3000))
            {
                logger.Log("Audio stream starting!");

                await audioResource.AudioVariables.Output.CopyToAsync(audioResource.AudioVariables.Discord);
                await audioResource.AudioVariables.Discord.FlushAsync();
            };

            audioResource.AudioVariables.FFmpeg.WaitForExit();
        }
        //Exception thrown with current version of skipping song
        catch (ObjectDisposedException)
        {
            logger.Log("Exception throw when skipping song!");
        }
        //Exception thrown when bot abruptly leaves voice channel
        catch (OperationCanceledException ex)
        {
            logger.Log("Exception thrown when audio stream is cancelled!");
            logger.Warning("AudioService.cs Stream", ex, LogOnly: true);

            audioResource.AudioVariables.FFmpeg.Kill();
            audioResource.AudioVariables.AbruptDisconnect = true;
        }
        catch (Exception ex)
        {
            logger.Error("AudioService.cs Stream", ex);
        }

        if (!audioResource.AudioVariables.FFmpeg.HasExited)
        {
            audioResource.AudioVariables.FFmpeg.WaitForExit();
        }
        audioResource.AudioVariables.Stopwatch.Stop();

        logger.Log("Audio stream finished!");
        return;
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
