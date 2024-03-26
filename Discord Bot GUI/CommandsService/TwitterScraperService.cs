using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.CommandsService
{
    public class TwitterScraperService
    {
        public static async Task<List<FileAttachment>> AllContentInRegularMessage(List<TwitterContent> content, bool sendVideos = true)
        {
            List<FileAttachment> Embeds = [];
            string commonFileName = $"twitter_{DateTime.Now:yyMMdd}_{DateTime.Now:HHmmss}";

            for (int i = 0; i < content.Count && Embeds.Count < 10; i++)
            {
                if (!sendVideos && content[i].Type == TwitterContentTypeEnum.Video)
                {
                    continue;
                }

                string fileName = content[i].Type switch
                {
                    TwitterContentTypeEnum.Video => $"{commonFileName}_video_{i + 1}.mp4",
                    TwitterContentTypeEnum.Image => $"{commonFileName}_image_{i + 1}.png",
                    _ => ""
                };

                Embeds.Add(new FileAttachment(await Global.GetStream(content[i].Url.OriginalString), fileName));
            }

            return Embeds;
        }
    }
}
