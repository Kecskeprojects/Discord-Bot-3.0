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
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class InstagramEmbedFeature(IInstaLoader instaLoader, IServerService serverService, BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IInstaLoader instaLoader = instaLoader;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            List<Uri> urls = UrlTools.LinkSearch(Context.Message.Content, false, "https://instagram.com/");

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
        InstagramMessageResult result = new();
        List<FileAttachment> attachments = [];
        string postId = uri.Segments[2].EndsWith('/') ? uri.Segments[2][..^1] : uri.Segments[2];

        try
        {
            string errorDuringDownload = instaLoader.DownloadFromInstagram(postId);

            if (!string.IsNullOrEmpty(errorDuringDownload))
            {
                logger.Error("ServiceDiscordCommunication.cs GetImagesFromPost", errorDuringDownload);
                if (errorDuringDownload.Contains("Fetching Post metadata failed"))
                {
                    await Context.Channel.SendMessageAsync("Posts cannot be accessed anonymously from this account.");
                }
                return;
            }

            string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}"));
            MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);

            try
            {
                result = await SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, false);
            }
            catch (Exception ex)
            {
                if (ex is not HttpException and not ArgumentException)
                {
                    throw;
                }

                logger.Warning("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", "Embed too large, only sending images!");
                result = await SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, ignoreVideos: true);
            }
        }
        catch (Exception ex)
        {
            logger.Error("InstagramEmbedFeature.cs SendInstagramPostEmbedAsync", ex);
        }

        if (result.HasFileDownloadHappened)
        {
            attachments.ForEach(x => x.Dispose());
            attachments.Clear();
            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}"), true);
        }

        if (result.ShouldMessageBeSuppressed)
        {
            await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
        }
    }

    private async Task<InstagramMessageResult> SendInstagramMessageAsync(List<FileAttachment> attachments, string[] files, string url, MessageReference refer, bool ignoreVideos)
    {
        string message = InstagramMessageProcessor.GetEmbedContent(attachments, files, url, ignoreVideos);

        InstagramMessageResult result = new();
        if (attachments.Count > 0)
        {
            result.HasFileDownloadHappened = true;
            await Context.Channel.SendFilesAsync(attachments, message, messageReference: refer, allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            result.ShouldMessageBeSuppressed = true;
        }
        //Ignore videos is a second try at sending so that is when we can know if the post is too large to send
        else if (ignoreVideos == true)
        {
            await Context.Channel.SendMessageAsync("Post content too large to send!");
        }

        return result;
    }
}
