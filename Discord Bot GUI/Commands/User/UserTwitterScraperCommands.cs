﻿using Discord;
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

                    List<FileAttachment> attachments = await SocialMessageProcessor.GetAttachments("twitter", result.Content);
                    if (!CollectionTools.IsNullOrEmpty(attachments))
                    {
                        try
                        {
                            await Context.Channel.SendFilesAsync(attachments, result.TextContent, messageReference: refer);
                            await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                        }
                        catch (HttpException ex)
                        {
                            if (ex.Message.Contains("40005"))
                            {
                                logger.Warning("UserTwitterScraperCommands.cs ScrapeFromUrl", "Embed too large, only sending images!");

                                attachments = await SocialMessageProcessor.GetAttachments("twitter", result.Content, false);
                                if (!CollectionTools.IsNullOrEmpty(attachments))
                                {
                                    await Context.Channel.SendFilesAsync(attachments, result.TextContent, messageReference: refer);
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
}
