using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Instagram;
using Discord_Bot.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Discord_Bot.Services;

public class InstaScraper(BotLogger logger) : IInstaScraper
{
    private readonly BotLogger logger = logger;

    #region Main Methods
    public async Task<SocialScrapingResult> GetDataFromUrl(Uri uri)
    {
        try
        {
            SocialScrapingResult result = await ExtractFromUrl(uri);

            return result;
        }
        catch (Exception ex)
        {
            logger.Error("InstaScraper.cs GetDataFromUrls", ex);
            return new SocialScrapingResult("Unexpected error occured");
        }
    }

    private async Task<SocialScrapingResult> ExtractFromUrl(Uri uri)
    {
        try
        {
            string lastSegment = uri.Segments[^1].Replace("\\", "/");
            int slashIndex = lastSegment.IndexOf('/');

            string shortcode = lastSegment[..slashIndex];

            string response =
                await WebTools.GetBody($"https://www.instagram.com/graphql/query/?doc_id=8845758582119845&variables={{\"shortcode\":\"{shortcode}\"}}");

            Root body = JsonConvert.DeserializeObject<Root>(response);

            if (body?.Data?.XdtShortcodeMedia == null)
            {
                return new SocialScrapingResult("Error during request.");
            }

            string status = body.Status;
            return status != "ok"
                ? new SocialScrapingResult($"Request was returned with status: {status}")
                : !body.Data.XdtShortcodeMedia.IsVideo && body.Data.XdtShortcodeMedia.EdgeSidecarToChildren?.Edges?.Count == 0
                ? new SocialScrapingResult("No content was found in post.")
                : GetData(body, uri);
        }
        catch (Exception ex)
        {
            logger.Error("InstaScraper.cs ExtractFromUrl", ex);
        }

        return null;
    }

    private static SocialScrapingResult GetData(Root body, Uri uri)
    {
        SocialScrapingResult result = new();
        List<Edge> content = body.Data.XdtShortcodeMedia.EdgeSidecarToChildren?.Edges;

        if (body.Data.XdtShortcodeMedia.EdgeMediaToCaption?.Edges?.Count > 0)
        {
            GetTextContent(body, result, uri);
        }

        if (body.Data.XdtShortcodeMedia.IsVideo)
        {
            result.Content.Add(new(new Uri(body.Data.XdtShortcodeMedia.VideoUrl), MediaContentTypeEnum.Video));
        }
        else if (content == null)
        {
            result.Content.Add(new(new Uri(body.Data.XdtShortcodeMedia.DisplayResources.Last().Src), MediaContentTypeEnum.Image));
        }
        else
        {
            GetMediaUris(result, content);
        }

        return result;
    }

    private static void GetTextContent(Root body, SocialScrapingResult result, Uri uri)
    {
        result.TextContent = DateTimeOffset.FromUnixTimeSeconds(body.Data.XdtShortcodeMedia.TakenAtTimestamp).ToString("yyyy\\.MM\\.dd");
        result.TextContent += $" **{body.Data.XdtShortcodeMedia.Owner.Username}**'s [post](<{uri.OriginalString}>)\n\n";

        string caption = string.Join("\n\n", body.Data.XdtShortcodeMedia.EdgeMediaToCaption.Edges.Select(x => x.Node.Text));
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
            Uri url = new(item.Node.DisplayResources.Last().Src);
            result.Content.Add(new(url, MediaContentTypeEnum.Image));
            //if (item.Node.IsVideo)
            //{
            //    Uri url = new(item.Node.VideoUrl);
            //    result.Content.Add(new(url, MediaContentTypeEnum.Video));
            //}
            //else
            //{
            //    Uri url = new(item.Node.DisplayResources.Last().Src);
            //    result.Content.Add(new(url, MediaContentTypeEnum.Image));
            //}
        }
    }
    #endregion
}
