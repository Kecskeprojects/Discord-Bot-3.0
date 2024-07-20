using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Twitter;
using Discord_Bot.Tools;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Discord_Bot.Services;
public class TwitterScraper(BotLogger logger, BrowserService browserService) : ITwitterScraper
{
    private readonly BotLogger logger = logger;
    private readonly BrowserService browserService = browserService;

    private Root Body { get; set; }

    #region Main Methods
    public async Task<TwitterScrapingResult> GetDataFromUrls(List<Uri> uris)
    {
        try
        {
            IPage mainPage = await browserService.NewPage();
            mainPage.Response += TwitterScraperResponse;

            TwitterScrapingResult result = new();

            string messages = "";
            for (int i = 0; i < uris.Count; i++)
            {
                Body = null;
                string mess = await ExtractFromUrl(mainPage, uris[i], uris.Count == 1, result);
                if (uris.Count > 1 && mess != "")
                {
                    messages += $"\n#{i + 1} ";
                }
                if (mess != "")
                {
                    messages += $"Link could not be embedded:\n{mess}";
                }
            }

            await mainPage.CloseAsync();

            result.ErrorMessage = messages;
            return result;
        }
        catch (Exception ex)
        {
            logger.Error("TwitterScraper.cs GetDataFromUrls", ex);
            return new TwitterScrapingResult("Unexpected error occured");
        }
    }

    private async void TwitterScraperResponse(object sender, ResponseCreatedEventArgs e)
    {
        try
        {
            if (e.Response.Url.Contains("TweetResultByRestId"))
            {
                Body = await e.Response.JsonAsync<Root>();
            }
        }
        catch (Exception ex)
        {
            logger.Error("TwitterScraper.cs TwitterScraperResponse", ex);
        }
    }

    private async Task<string> ExtractFromUrl(IPage page, Uri uri, bool singleLink, TwitterScrapingResult result)
    {
        try
        {
            await page.DeleteCookieAsync();
            await page.GoToAsync(uri.OriginalString);

            int count = 0;
            while (Body == null && count < 120)
            {
                count++;
                await Task.Delay(500);
            }

            if (Body == null)
            {
                return "Timeout while getting content.";
            }

            string reason = Body?.Data?.TweetResult?.Result?.Reason;
            string quoteReason = Body?.Data?.TweetResult?.Result?.QuotedStatusResult?.Result?.Reason;
            if (!string.IsNullOrEmpty(reason) || !string.IsNullOrEmpty(quoteReason))
            {
                if (reason == "NsfwLoggedOut" || quoteReason == "NsfwLoggedOut")
                {
                    return "Post is flagged as potentially sensitive content!";
                }
            }

            GetData(singleLink, result);
        }
        catch (Exception ex)
        {
            logger.Error("TwitterScraper.cs ExtractFromUrl", ex);
        }

        return "";
    }

    private void GetData(bool singleLink, TwitterScrapingResult result)
    {
        List<Uri> links = [];
        Legacy tweet = Body?.Data?.TweetResult?.Result?.Legacy;
        Legacy quote = Body?.Data?.TweetResult?.Result?.QuotedStatusResult?.Result?.Legacy;

        if (tweet?.Entities?.Media?.Count > 0)
        {
            links.AddRange(GetMediaUris(result, tweet.Entities?.Media));
        }

        if (quote?.Entities?.Media?.Count > 0)
        {
            links.AddRange(GetMediaUris(result, quote.Entities?.Media));
        }

        if (singleLink)
        {
            if (tweet?.FullText?.Length > 0)
            {
                result.TextContent += tweet.FullText;
            }

            if (quote?.FullText?.Length > 0)
            {
                result.TextContent += $"\n\n**Quoting**\n{quote.FullText}";
            }

            if (result.TextContent.Length > 2000)
            {
                result.TextContent = result.TextContent[..1999];
            }

            result.TextContent = HttpUtility.HtmlDecode(result.TextContent);
            result.TextContent = UrlTools.SanitizeText(result.TextContent);
        }
    }
    #endregion

    #region Helper Methods

    private static List<Uri> GetMediaUris(TwitterScrapingResult result, List<Medium> list)
    {
        List<Uri> tempMedia = [];
        foreach (Medium item in list)
        {
            if (item.Type == "photo")
            {
                Uri url = ModifyImageUrl(item.MediaUrlHttps);
                result.Content.Add(new(url, TwitterContentTypeEnum.Image));
            }
            else if (item.VideoInfo != null)
            {
                Uri url = new(item.VideoInfo.Variants[^1].Url);
                result.Content.Add(new(url, TwitterContentTypeEnum.Video));
            }
        }
        return tempMedia;
    }

    private static Uri ModifyImageUrl(string url)
    {
        if (url.Split(".").Length > 1)
        {
            return new(url.Split("?")[0]);
        }

        string query = new Uri(url).Query;

        string newQuery = query.Contains("jpg") ? "?format=jpg" : "?format=png";
        newQuery += Constant.TwitterSmallSizingStrings.Any(query.Contains) //If not in these, the value is either "large" or "orig"
                ? "&name=medium"
                : "&name=orig";

        return new(url.Split("?")[0] + newQuery);
    }
    #endregion
}
