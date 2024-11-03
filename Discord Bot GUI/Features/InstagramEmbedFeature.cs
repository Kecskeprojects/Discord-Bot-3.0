using Discord;
using Discord.Net;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class InstagramEmbedFeature(
    //IInstaLoader instaLoader,
    IInstaScraper instaScraper,
    IServerService serverService,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IInstaScraper instaScraper = instaScraper;

    //private readonly IInstaLoader instaLoader = instaLoader;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            List<Uri> urls = UrlTools.LinkSearch(Context.Message.Content, false, Constant.InstagramBaseUrl);

            //Check if message is an instagram link
            if (!CollectionTools.IsNullOrEmpty(urls))
            {
                for (int i = 0; i < urls.Count; i++)
                {
                    logger.Log($"Embed message from following link: {urls[i]}");
                    if (urls[i].Segments.Length != 2 &&
                        urls[i].Segments[1][..^1] != "stories" &&
                        urls[i].Segments[1][..^1] != "live" &&
                        urls[i].Segments[2][..^1] != "live")
                    {
                        await SendInstagramPostEmbedAsync(urls[i]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("InstagramEmbedFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }

    private async Task SendInstagramPostEmbedAsync(Uri uri)
    {
        try
        {
            SocialScrapingResult result = await instaScraper.GetDataFromUrl(uri);

            MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if (result.ErrorMessage.Length < 150)
                {
                    await Context.Channel.SendMessageAsync(result.ErrorMessage);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Error message too long to display!");
                    logger.Error("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", result.ErrorMessage);
                }

                if (CollectionTools.IsNullOrEmpty(result.Content))
                {
                    return;
                }
            }

            List<FileAttachment> attachments = await SocialMessageProcessor.GetAttachments("instagram", result.Content, limit: 1000);
            if (!CollectionTools.IsNullOrEmpty(attachments))
            {
                try
                {
                    await SendInstagramMessageAsync(attachments, result.TextContent, refer, false);
                    await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                }
                catch (HttpException ex)
                {
                    if (ex.Message.Contains("40005"))
                    {
                        logger.Warning("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", "Embed too large, only sending images!");

                        foreach (FileAttachment item in attachments)
                        {
                            item.Dispose();
                        }
                        attachments.Clear();
                        attachments = await SocialMessageProcessor.GetAttachments("instagram", result.Content, sendVideos: false, limit: 1000);
                        if (!CollectionTools.IsNullOrEmpty(attachments))
                        {
                            await SendInstagramMessageAsync(attachments, result.TextContent, refer, true);
                            await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Post content too large to send!");
                        }
                    }
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("No image/videos in tweet.");
            }

            foreach (FileAttachment item in attachments)
            {
                item.Dispose();
            }
            attachments.Clear();
        }
        catch (Exception ex)
        {
            logger.Error("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", ex);
        }
    }

    private async Task<InstagramMessageResult> SendInstagramMessageAsync(List<FileAttachment> attachments, string message, MessageReference refer, bool ignoreVideos)
    {
        InstagramMessageResult result = new();
        if (attachments.Count > 0)
        {
            for (int i = 0; i < Math.Ceiling(attachments.Count / 10.0); i++)
            {
                int count = attachments.Count - (i * 10) >= 10 ? 10 : attachments.Count - (i * 10);
                if (i == 0)
                {
                    await Context.Channel.SendFilesAsync(attachments.GetRange(i * 10, count), message, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                }
                else
                {
                    await Context.Channel.SendFilesAsync(attachments.GetRange(i * 10, count));
                }
            }
        }
        //Ignore videos is a second try at sending so that is when we can know if the post is too large to send
        else if (ignoreVideos == true)
        {
            await Context.Channel.SendMessageAsync("Post content too large to send!");
        }

        return result;
    }

    //private async Task SendInstagramPostEmbedAsync(Uri uri)
    //{
    //    InstagramMessageResult result = new();
    //    List<FileAttachment> attachments = [];
    //    string postId = uri.Segments[2].EndsWith('/') ? uri.Segments[2][..^1] : uri.Segments[2];

    //    try
    //    {
    //        string errorDuringDownload = instaLoader.DownloadFromInstagram(postId);

    //        if (!string.IsNullOrEmpty(errorDuringDownload))
    //        {
    //            logger.Error("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", errorDuringDownload);
    //            if (errorDuringDownload.Contains("Fetching Post metadata failed"))
    //            {
    //                await Context.Channel.SendMessageAsync("Posts cannot be accessed anonymously from this account.");
    //            }
    //            if (!errorDuringDownload.Contains("retrying") || !Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}")))
    //            {
    //                return;
    //            }
    //        }

    //        string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}"));
    //        MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);

    //        try
    //        {
    //            result = await SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, false);
    //        }
    //        catch (Exception ex)
    //        {
    //            if (ex is not HttpException and not ArgumentException)
    //            {
    //                throw;
    //            }

    //            logger.Warning("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", "Embed too large, only sending images!");
    //            result = await SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, ignoreVideos: true);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.Error("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", ex);
    //    }

    //    if (result.HasFileDownloadHappened)
    //    {
    //        foreach (FileAttachment item in attachments)
    //        {
    //            item.Dispose();
    //        }
    //        attachments.Clear();
    //        Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}"), true);
    //    }

    //    if (result.ShouldMessageBeSuppressed)
    //    {
    //        await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
    //    }
    //}

    //private async Task<InstagramMessageResult> SendInstagramMessageAsync(List<FileAttachment> attachments, string[] files, string url, MessageReference refer, bool ignoreVideos)
    //{
    //    string message = InstagramMessageProcessor.GetEmbedContent(attachments, files, url, ignoreVideos);

    //    InstagramMessageResult result = new();
    //    if (attachments.Count > 0)
    //    {
    //        result.HasFileDownloadHappened = true;
    //        for (int i = 0; i < Math.Ceiling(attachments.Count / 10.0); i++)
    //        {
    //            int count = attachments.Count - i * 10 >= 10 ? 10 : attachments.Count - i * 10;
    //            if(i == 0)
    //            {
    //                await Context.Channel.SendFilesAsync(attachments.GetRange(i * 10, count), message, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
    //            }
    //            else
    //            {
    //                await Context.Channel.SendFilesAsync(attachments.GetRange(i * 10, count));
    //            }
    //        }
    //        result.ShouldMessageBeSuppressed = true;
    //    }
    //    //Ignore videos is a second try at sending so that is when we can know if the post is too large to send
    //    else if (ignoreVideos == true)
    //    {
    //        await Context.Channel.SendMessageAsync("Post content too large to send!");
    //    }

    //    return result;
    //}
}
