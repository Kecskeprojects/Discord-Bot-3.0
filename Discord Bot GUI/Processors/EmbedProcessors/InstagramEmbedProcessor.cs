using Discord;
using Discord_Bot.Services.Models.Instagram;
using Discord_Bot.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public static class InstagramEmbedProcessor
    {
        public static string GetEmbedContent(List<FileAttachment> attachments, string[] files, string url, bool ignoreVideos)
        {
            string caption = "";
            Node metadata = null;
            ReadFiles(attachments, files, ref caption, ref metadata, ignoreVideos);

            string message = $" **{metadata.Owner.Username}**'s [post](<{url}>)\n\n";
            if (!string.IsNullOrEmpty(caption))
            {
                string text = caption.Split("\n\n#")[0];
                int indexOfTag = text.ToLower().LastIndexOf("tags:");
                if (indexOfTag != -1)
                {
                    text = text[..indexOfTag];
                }
                text = UrlTools.SanitizeText(text);

                message += $"{text}\n";
            }
            string currDate = DateTimeOffset.FromUnixTimeSeconds(metadata.TakenAtTimestamp).ToString("yyyy\\.MM\\.dd");
            message = message.Insert(0, currDate);

            if (message.Length > 2000)
            {
                message = message[..1999];
            }

            return message;
        }

        private static void ReadFiles(List<FileAttachment> attachments, string[] files, ref string caption, ref Node metadata, bool ignoreVideos)
        {
            if (attachments.Count > 0)
            {
                attachments.ForEach(x => x.Dispose());
                attachments.Clear();
            }

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
        }
    }
}
