using Discord;
using Discord_Bot.Services.Models.InstaLoader;
using Discord_Bot.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discord_Bot.Processors.MessageProcessor;

public static class InstagramMessageProcessor
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
            foreach (FileAttachment item in attachments)
            {
                item.Dispose();
            }
            attachments.Clear();
        }

        IOrderedEnumerable<string> orderedFiles = files.OrderBy(x =>
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(x);
            bool parsed = int.TryParse(fileNameWithoutExtension.Split("_")[^1], out int position);
            return (parsed ? position : attachments.Count) - 1;
        });

        foreach (string file in orderedFiles)
        {
            if (file.EndsWith(".txt"))
            {
                caption = File.ReadAllText(file);
                continue;
            }
            else if (file.EndsWith(".json"))
            {
                metadata = JsonConvert.DeserializeObject<InstaLoaderBase>(File.ReadAllText(file)).Node;
            }
            else if (!ignoreVideos || file.EndsWith(".jpg") || file.EndsWith(".png"))
            {
                attachments.Add(new FileAttachment(file));
            }
        }
        for (int i = 0; i < orderedFiles.Count(); i++)
        {
        }
    }
}
