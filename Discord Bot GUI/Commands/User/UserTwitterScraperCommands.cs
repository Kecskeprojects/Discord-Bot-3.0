using Discord;
using Discord.Commands;
using Discord.Net;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Twitter")]
[Remarks("User")]
[Summary("Twitter related commands")]
public class UserTwitterScraperCommands(
    ITwitterScraper twitterScraper,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly ITwitterScraper twitterScraper = twitterScraper;

    [Command("twt")]
    [Summary("For embedding twitter links, replacing the built in discord embeds\n*Links have to be separated by at least a space")]
    public async Task ScrapeFromUrl([Name("links*")][Remainder] string message)
    {
        if (!config.Enable_Twitter_Embed)
        {
            return;
        }

        try
        {
            List<Uri> urls = UrlTools.LinkSearch(message, true, Constant.TwitterBaseURLs);

            if (urls != null)
            {
                urls = urls.Where(x => x.Segments.Length >= 3 && x.Segments[2] == "status/").ToList();
                if (!CollectionTools.IsNullOrEmpty(urls))
                {
                    logger.Log($"Embed message from following links: \n{string.Join("\n", urls)}");

                    SocialScrapingResult result = await twitterScraper.GetDataFromUrls(urls);

                    MessageReference refer = new(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        if (result.ErrorMessage.Length < 150)
                        {
                            await ReplyAsync(result.ErrorMessage);
                        }
                        else
                        {
                            await ReplyAsync("Error message too long to display!");
                            logger.Error("UserTwitterScraperCommands.cs ScrapeFromUrl", result.ErrorMessage);
                        }

                        if (CollectionTools.IsNullOrEmpty(result.Content))
                        {
                            return;
                        }
                    }

                    List<FileAttachment> attachments = await SocialMessageProcessor.GetAttachments("twitter", result.Content, limit: 30);
                    if (!CollectionTools.IsNullOrEmpty(attachments))
                    {
                        try
                        {
                            await SendTwitterMessageAsync(attachments, result.TextContent, refer, true);
                            await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                        }
                        catch (HttpException ex)
                        {
                            if (ex.Message.Contains("40005"))
                            {
                                logger.Warning("UserTwitterScraperCommands.cs ScrapeFromUrl", "Embed too large, only sending images!");

                                attachments = await SocialMessageProcessor.GetAttachments("twitter", result.Content, sendVideos: false, limit: 30);
                                if (!CollectionTools.IsNullOrEmpty(attachments))
                                {
                                    await SendTwitterMessageAsync(attachments, result.TextContent, refer, true);
                                    await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                                }
                                else
                                {
                                    await ReplyAsync("Post content too large to send!");
                                }
                            }
                        }
                    }
                    else
                    {
                        await ReplyAsync("No image/videos in tweet.");
                    }

                    foreach (FileAttachment item in attachments)
                    {
                        item.Dispose();
                    }
                    attachments.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserTwitterScraperCommands.cs ScrapeFromUrl", ex);
        }
    }

    private async Task<InstagramMessageResult> SendTwitterMessageAsync(List<FileAttachment> attachments, string message, MessageReference refer, bool ignoreVideos)
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
}
