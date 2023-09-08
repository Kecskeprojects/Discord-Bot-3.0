using Discord;
using Discord_Bot.Services.Models.Instagram;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public static List<FileAttachment> ReadFiles(string[] files, ref string caption, ref Node metadata, bool ignoreVideos)
        {
            List<FileAttachment> attachments = new();
            for (int i = 0; i < files.Length; i++)
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
                else if (attachments.Count <= 10 && (!ignoreVideos || files[i].EndsWith(".jpg") || files[i].EndsWith(".png")))
                {
                    attachments.Add(new FileAttachment(files[i]));
                }
            }

            return attachments;
        }
    }
}
