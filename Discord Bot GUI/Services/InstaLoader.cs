using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.Services
{
    public class InstaLoader : IInstaLoader
    {
        private readonly Logging logger;

        public InstaLoader(Logging logger) => this.logger = logger;

        //https://instaloader.github.io/basic-usage.html
        //https://github.com/instaloader/instaloader
        //instaloader.exe --quiet --filename-pattern={date_utc}_{shortcode} --dirname-pattern={ shortcode} --no-video-thumbnails --no-compress-json -- -CfGa5x0FCak -Cppwn5qjMJA
        public void DownloadFromInstagram(string postId)
        {
            try
            {
                ProcessStartInfo instaloader = new()
                {
                    FileName = "cmd.exe",
                    Arguments = @"/C instaloader.exe " +
                                @"--quiet --no-video-thumbnails --no-compress-json " +
                                @"--filename-pattern={date_utc}_{shortcode} --dirname-pattern={shortcode} -- -" +
                                postId,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Instagram")
                };
                logger.Log("Downloading images");
                Process process = Process.Start(instaloader);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                logger.Error("InstaLoader.cs DownloadFromInstagram", ex.ToString());
            }
        }
    }
}
