using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Processors.MessageProcessor;

public static class TwitterMessageProcessor
{
    public static async Task<List<FileAttachment>> GetAttachments(List<TwitterContent> content, bool sendVideos = true)
    {
        List<FileAttachment> Embeds = [];
        string commonFileName = $"twitter_{DateTime.UtcNow:yyMMdd}_{DateTime.UtcNow:HHmmss}";

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

            Embeds.Add(new FileAttachment(await WebTools.GetStream(content[i].Url.OriginalString), fileName));
        }

        return Embeds;
    }
}
