using Discord;
using Discord_Bot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.CommandsService
{
    public class TwitterScraperService
    {
        private static readonly string[] smallSizingStrings = ["thumb", "small", "medium" ];
        private static readonly string[] largeSizingStrings = ["large", "orig"];
        public static List<FileAttachment> AllContentInRegularMessage(List<Uri> videos, List<Uri> images, bool sendVideos = true)
        {
            List<FileAttachment> Embeds = [];
            string commonFileName = $"twitter_{DateTime.Now:yyMMdd}_{DateTime.Now:HHmmss}";

            for (int i = 0; i < (images.Count < 10 ? images.Count : 10) && Embeds.Count < 10; i++)
            {
                string query = images[i].Query;
                query = smallSizingStrings.Any(query.Contains)
                        ? "?format=jpg&name=medium"
                        : "?format=jpg&name=orig";

                images[i] = new Uri(images[i].OriginalString.Split("?")[0] + query);
                Embeds.Add(new FileAttachment(Global.GetStream(images[i].OriginalString), $"{commonFileName}_image_{i + 1}.png"));
            }

            for (int i = 0; sendVideos && i < (videos.Count < 10 ? videos.Count : 10) && Embeds.Count < 10; i++)
            {
                Embeds.Add(new FileAttachment(Global.GetStream(videos[i].OriginalString), $"{commonFileName}_video_{i + 1}.mp4"));
            }

            return Embeds;
        }
    }
}
