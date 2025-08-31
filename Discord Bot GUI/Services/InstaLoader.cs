using Discord_Bot.Core;
using Discord_Bot.Interfaces.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Services;

public class InstaLoader(BotLogger logger) : IInstaLoader
{
    private readonly BotLogger logger = logger;

    //https://instaloader.github.io/basic-usage.html
    //https://github.com/instaloader/instaloader
    //instaloader.exe --quiet --filename-pattern={date_utc}_{shortcode} --dirname-pattern={shortcode} --no-resume --no-video-thumbnails --no-compress-json -- -[postId]
    public string DownloadFromInstagram(string postId)
    {
        string errorDuringDownload = "";
        try
        {
            ProcessStartInfo instaloader = new()
            {
                FileName = "cmd.exe",
                Arguments = @"/C instaloader.exe " +
                            @"--quiet --no-video-thumbnails --no-compress-json --no-iphone --no-resume " +
                            @"--filename-pattern={date_utc}_{shortcode} --dirname-pattern={shortcode} -- -" +
                            postId,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Dependencies\\Instagram")
            };
            logger.Log("Downloading images");
            using (Process process = Process.Start(instaloader))
            {
                errorDuringDownload = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }

            return errorDuringDownload;
        }
        catch (Exception ex)
        {
            logger.Error("InstaLoader.cs DownloadFromInstagram", ex);
        }

        return errorDuringDownload;
    }
}
