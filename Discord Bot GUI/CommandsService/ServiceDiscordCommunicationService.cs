﻿using Discord;
using Discord_Bot.Core.Logger;
using Discord_Bot.Services.Models.Instagram;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Discord_Bot.CommandsService
{
    public class ServiceDiscordCommunicationService
    {
        public static string GetCaption(Uri uri, string caption, Node metadata)
        {
            string message = $" **{metadata.Owner.Username}**'s [post](<{uri.OriginalString}>)\n\n";
            if (!string.IsNullOrEmpty(caption))
            {
                string text = caption.Split("\n\n#")[0];
                int indexOfTag = text.ToLower().LastIndexOf("tags:");
                if (indexOfTag != -1)
                {
                    text = text[..indexOfTag];
                }

                message += $"{text}\n";
            }
            string currDate = DateTimeOffset.FromUnixTimeSeconds(metadata.TakenAtTimestamp).ToString("yyyy\\.MM\\.dd");
            message = message.Insert(0, currDate);
            return message;
        }

        public static void DownloadFromInstagram(Uri uri, Logging logger)
        {
            ProcessStartInfo instaloader = new()
            {
                FileName = "cmd.exe",
                Arguments = @"/C instaloader.exe " +
                                            @"--quiet --no-video-thumbnails --no-compress-json " +
                                            @"--filename-pattern={date_utc}_{shortcode} --dirname-pattern={shortcode} -- -" +
                                            uri.Segments[2][..^1],
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Instagram")
            };
            logger.Log("Downloading images");
            Process process = Process.Start(instaloader);
            process.WaitForExit();
        }

        public static List<FileAttachment> ReadFiles(string[] files, ref string caption, ref Node metadata, bool ignoreVideos)
        {
            List<FileAttachment> attachments = new();
            int maxCount = files.Length < 10 ? files.Length : 10;
            for (int i = 0; i < maxCount; i++)
            {
                if (files[i].EndsWith(".txt"))
                {
                    caption = File.ReadAllText(files[i]);
                    continue;
                }
                else if (files[i].EndsWith(".json"))
                {
                    metadata = JsonConvert.DeserializeObject<InstaLoaderBase>(File.ReadAllText(files[i])).Node;
                }
                else if (!ignoreVideos || files[i].EndsWith(".jpg") || files[i].EndsWith(".png"))
                {
                    attachments.Add(new FileAttachment(files[i]));
                }
            }

            return attachments;
        }
    }
}
