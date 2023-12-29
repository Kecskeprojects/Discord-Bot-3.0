using Discord;
using Discord.Commands;
using Discord.Net;
using Discord_Bot.CommandsService;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class TwitterScraperCommands(ITwitterScraper twitterScraper, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), ITwitterScraperCommands
    {
        private static readonly string[] baseURLs = ["https://twitter.com/", "https://x.com/"];
        private readonly ITwitterScraper twitterScraper = twitterScraper;

        [Command("twt")]
        [Summary("For embedding twitter messages, replacing the built in discord embeds")]
        public async Task ScrapeFromUrl([Remainder] string message)
        {
            try
            {
                List<Uri> urls = UrlTools.LinkSearch(message, true, baseURLs);

                //Check if message is an instagram link
                if (urls != null)
                {
                    urls = urls.Where(x => x.Segments.Length >= 3 && x.Segments[2] == "status/").ToList();
                    if (!CollectionTools.IsNullOrEmpty(urls))
                    {
                        logger.Log($"Embed message from following links: \n{string.Join("\n", urls)}");

                        TwitterScrapingResult result = await twitterScraper.GetDataFromUrls(urls);

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
                                logger.Log(result.ErrorMessage);
                            }

                            if (CollectionTools.IsNullOrEmpty(result.Images) && CollectionTools.IsNullOrEmpty(result.Videos))
                            {
                                return;
                            }
                        }

                        List<FileAttachment> attachments = TwitterScraperService.AllContentInRegularMessage(result.Videos, result.Images);
                        if (!CollectionTools.IsNullOrEmpty(attachments))
                        {
                            try
                            {
                                await Context.Channel.SendFilesAsync(attachments, messageReference: refer);
                            }
                            catch (HttpException ex)
                            {
                                if (ex.Message.Contains("40005"))
                                {
                                    logger.Warning("ServiceDiscordCommunication.cs SendTwitterMessage", "Embed too large, only sending images!", LogOnly: true);
                                    logger.Warning("ServiceDiscordCommunication.cs SendTwitterMessage", ex.ToString(), LogOnly: true);

                                    attachments = TwitterScraperService.AllContentInRegularMessage(result.Videos, result.Images, false);
                                    if (!CollectionTools.IsNullOrEmpty(attachments))
                                    {
                                        await Context.Channel.SendFilesAsync(attachments, messageReference: refer);
                                        await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                                    }
                                    else
                                    {
                                        await ReplyAsync("Post content too large to send!");
                                    }

                                    return;
                                }
                            }

                            await Context.Message.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
                            return;
                        }

                        await ReplyAsync("No image/videos in tweet.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("TwitterScraperCommands.cs ScrapeFromUrl", ex.ToString());
            }
        }
    }
}
