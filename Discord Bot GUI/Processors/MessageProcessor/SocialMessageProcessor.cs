using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Processors.MessageProcessor;

public static class SocialMessageProcessor
{
    public static async Task<List<FileAttachment>> GetAttachments(string socialMedia, List<MediaContent> content, bool sendVideos = true, int limit = 10)
    {
        List<FileAttachment> Embeds = [];
        string commonFileName = $"{socialMedia}_{DateTime.UtcNow:yyMMdd}_{DateTime.UtcNow:HHmmss}";

        for (int i = 0; i < content.Count && Embeds.Count < limit; i++)
        {
            if (!sendVideos && content[i].Type == MediaContentTypeEnum.Video)
            {
                continue;
            }

            string fileName = content[i].Type switch
            {
                MediaContentTypeEnum.Video => $"{commonFileName}_video_{i + 1}.mp4",
                MediaContentTypeEnum.Image => $"{commonFileName}_image_{i + 1}.png",
                _ => ""
            };

            Embeds.Add(new FileAttachment(await WebTools.GetStream(content[i].Url.OriginalString), fileName));
        }

        return Embeds;
    }
}
