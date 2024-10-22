using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Instagram;
using Discord_Bot.Tools;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Discord_Bot.Services;
public class InstaScraper(BotLogger logger, BrowserService browserService) : IInstaScraper
{
    private readonly BotLogger logger = logger;
    private readonly BrowserService browserService = browserService;

    private Root Body { get; set; }

    #region Main Methods
    public async Task<SocialScrapingResult> GetDataFromUrl(Uri uri)
    {
        try
        {
            SocialScrapingResult result = new();

            using (IPage mainPage = await browserService.NewPage())
            {
                mainPage.Response += InstaScraperResponse;
                //await mainPage.SetCacheEnabledAsync(false);

                Body = null;
                string messages = await ExtractFromUrl(mainPage, uri, result);

                await mainPage.CloseAsync();

                result.ErrorMessage = messages;
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.Error("InstaScraper.cs GetDataFromUrls", ex);
            return new SocialScrapingResult("Unexpected error occured");
        }
    }

    private async void InstaScraperResponse(object sender, ResponseCreatedEventArgs e)
    {
        try
        {
            if (e.Response.Url == "https://www.instagram.com/graphql/query")
            {
                Root body = await e.Response.JsonAsync<Root>();
                if (body?.Data?.XdtShortcodeMedia == null)
                {
                    return;
                }
                Body = body;
            }
        }
        catch (Exception ex)
        {
            logger.Error("InstaScraper.cs InstaScraperResponse", ex);
        }
    }

    private async Task<string> ExtractFromUrl(IPage page, Uri uri, SocialScrapingResult result)
    {
        try
        {
            await page.DeleteCookieAsync();
            await page.GoToAsync(uri.OriginalString);

            int count = 0;
            while (Body == null && count < 120)
            {
                count++;

                //Refresh and retry, the query call is sometimes missing
                if (count % 30 == 0 && count != 0 && count != 120)
                {
                    logger.Log($"Reload {count / 30} for getting data");
                    await page.DeleteCookieAsync();
                    await page.ReloadAsync();
                }

                await Task.Delay(500);
            }

            if (Body == null)
            {
                return "Timeout while getting content.";
            }

            string status = Body.Status;
            if (status != "ok")
            {
                return $"Request was returned with status: {status}";
            }

            if (!Body.Data.XdtShortcodeMedia.IsVideo && Body.Data.XdtShortcodeMedia.EdgeSidecarToChildren?.Edges?.Count == 0)
            {
                return "No content was found in post.";
            }

            GetData(result, uri);
        }
        catch (Exception ex)
        {
            logger.Error("InstaScraper.cs ExtractFromUrl", ex);
        }

        return "";
    }

    private void GetData(SocialScrapingResult result, Uri uri)
    {
        List<Edge> content = Body.Data.XdtShortcodeMedia.EdgeSidecarToChildren?.Edges;

        if (Body.Data.XdtShortcodeMedia.EdgeMediaToCaption?.Edges?.Count > 0)
        {
            GetTextContent(result, uri);
        }

        if (Body.Data.XdtShortcodeMedia.IsVideo)
        {
            result.Content.Add(new(new Uri(Body.Data.XdtShortcodeMedia.VideoUrl), MediaContentTypeEnum.Video));
        }
        else
        {
            GetMediaUris(result, content);
        }
    }

    private void GetTextContent(SocialScrapingResult result, Uri uri)
    {
        result.TextContent = DateTimeOffset.FromUnixTimeSeconds(Body.Data.XdtShortcodeMedia.TakenAtTimestamp).ToString("yyyy\\.MM\\.dd");
        result.TextContent += $" **{Body.Data.XdtShortcodeMedia.Owner.Username}**'s [post](<{uri.OriginalString}>)\n\n";

        string caption = string.Join("\n\n", Body.Data.XdtShortcodeMedia.EdgeMediaToCaption.Edges.Select(x => x.Node.Text));
        caption = caption.Split("\n\n#")[0];
        int indexOfTag = caption.ToLower().LastIndexOf("tags:");
        if (indexOfTag != -1)
        {
            caption = caption[..indexOfTag];
        }
        caption = HttpUtility.HtmlDecode(caption);
        caption = UrlTools.SanitizeText(caption);

        result.TextContent += caption;

        if (result.TextContent.Length > 2000)
        {
            result.TextContent = result.TextContent[..1999];
        }
    }
    #endregion

    #region Helper Methods
    private static void GetMediaUris(SocialScrapingResult result, List<Edge> list)
    {
        foreach (Edge item in list)
        {
            if (item.Node.IsVideo)
            {
                Uri url = new(item.Node.VideoUrl);
                result.Content.Add(new(url, MediaContentTypeEnum.Video));
            }
            else
            {
                Uri url = new(item.Node.DisplayResources.Last().Src);
                result.Content.Add(new(url, MediaContentTypeEnum.Image));
            }
        }
    }
    #endregion
}
