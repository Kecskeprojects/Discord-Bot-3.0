﻿using AngleSharp;
using AngleSharp.Dom;
using Discord_Bot.Communication;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class TwitterScraper
    {
        #region Variables
        private static IBrowser Browser { get; set; }

        //The standard video url is the following:
        //https://video.twimg.com/[folder_type]/[folder_id]/pu/vid/[width]x[height]/[video_id].mp4?tag=12
        private List<Uri> Videos { get; set; } = [];

        //Storing all video links before filtering
        private List<Uri> TempVideos { get; set; } = [];

        //The standard image url is the following:
        //https://pbs.twimg.com/media/[image_id]?format=[image_format]&name=[width]x[height]
        private List<Uri> Images { get; set; } = [];
        private List<string> Exceptions { get; } = [];
        #endregion

        #region Main Methods
        public static async Task OpenBroser()
        {
            BrowserFetcher browserFetcher = new(SupportedBrowser.Chrome);
            await browserFetcher.DownloadAsync("117.0.5938.62"); //Todo: PuppeteerSharp.BrowserData.Chrome.DefaultBuildId should be revisited in future, new builds remove option to download the way it is currently done
            IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args =
                [
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-web-security",
                    "--disable-features=IsolateOrigins",
                    "--disable-site-isolation-trials",
                    "--disable-features=BlockInsecurePrivateNetworkRequests",
                    "--ignore-certificate-errors"
                ],
                ExecutablePath = "Chrome\\Win64-117.0.5938.62\\chrome-win64\\chrome.exe"
            });
            Browser = browser;
        }

        public static async Task CloseBrowser()
        {
            if (Browser != null && !Browser.IsClosed)
            {
                await Browser.CloseAsync();
            }
        }

        public async Task<TwitterScrapingResult> GetDataFromUrls(List<Uri> uris)
        {
            try
            {
                if (Browser == null || Browser.IsClosed)
                {
                    await OpenBroser();
                }

                IPage mainPage = await CreateNewPage();

                string messages = "";
                for (int i = 0; i < uris.Count; i++)
                {
                    string mess = await ExtractFromUrl(mainPage, uris[i]);
                    if (mess != "")
                    {
                        messages += $"{i + 1}. link could not be embedded:\n{mess}\n";
                    }
                }

                await mainPage.CloseAsync();

                messages += string.Join("\n", Exceptions);
                return new TwitterScrapingResult(Videos, Images, messages);
            }
            catch (Exception ex)
            {
                return new TwitterScrapingResult(ex.Message);
            }
        }

        private async Task<IPage> CreateNewPage()
        {
            IPage mainPage = await Browser.NewPageAsync();
            Dictionary<string, string> headers = new()
                {
                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36" },
                    { "upgrade-insecure-requests", "1" },
                    { "accept", "text/html,application/xhtml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3" },
                    { "accept-encoding", "gzip, deflate, br" },
                    { "accept-language", "en-US,en;q=0.9,en;q=0.8" }
                };
            await mainPage.SetExtraHttpHeadersAsync(headers);

            mainPage.Response += TwitterScraperResponse;
            return mainPage;
        }
        #endregion

        #region Helper functions
        private async Task<string> ExtractFromUrl(IPage page, Uri uri)
        {
            try
            {
                int videosBeforeMainCount = 0;
                int videoCount = 0;
                IDocument document = await OpenPage(page, uri);

                IHtmlCollection<IElement> articles = document.QuerySelectorAll("article");

                //The timestamp link contains the post's relative path, we can find the post's article by that, we also get it's index in the list
                IElement main = articles.First(x => x.QuerySelectorAll("a[href]").FirstOrDefault(e => e.GetAttribute("href").Contains(uri.AbsolutePath, StringComparison.OrdinalIgnoreCase)) != null);
                int mainPos = articles.Index(main);

                if (main.Text().Contains("sensitive content"))
                {
                    return "Post is flagged as potentially sensitive content!";
                }

                List<Uri> currImages = GetImages(main);

                await GetVideos(main, articles, mainPos, videosBeforeMainCount, videoCount);

                Images.AddRange(currImages);
                Videos.AddRange(TempVideos.Skip(videosBeforeMainCount).Take(videoCount));
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }

        private async Task GetVideos(IElement main, IHtmlCollection<IElement> articles, int mainPos, int videosBeforeMainCount, int videoCount)
        {
            //We also search for Videos in the post
            videoCount = main.QuerySelectorAll("div[data-testid]").Where(e => e.GetAttribute("data-testid") == "videoPlayer").Count();

            //Checking any articles before main post
            for (int i = 0; i < mainPos; i++)
            {
                int tempCount = articles[i].QuerySelectorAll("div[data-testid]").Where(e => e.GetAttribute("data-testid") == "videoPlayer").Count();
                videosBeforeMainCount += tempCount;
            }

            //As the video links are caught in the TwitterScraper_Response event handler,
            //the only way to verify if we got them all is to wait until the right amount of links are stored in the list
            int count = 0;
            while (Videos.Count < videoCount + videosBeforeMainCount && count < 10)
            {
                await Task.Delay(500);
                count++;
            }
        }

        private static async Task<IDocument> OpenPage(IPage page, Uri uri)
        {
            await page.DeleteCookieAsync();
            await page.GoToAsync(uri.OriginalString, 15000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.Networkidle0]);
            //await Task.Delay(1000);
            string content = await page.GetContentAsync();

            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            IDocument document = await context.OpenAsync(req => req.Content(content));
            return document;
        }

        private static List<Uri> GetImages(IElement main)
        {
            //Getting the Images, if there are any
            List<Uri> currImages = [];
            foreach (IElement element in main.QuerySelectorAll("a"))
            {
                IElement img = element.QuerySelector("img");
                if (img != null)
                {
                    Uri newUri = new(img.GetAttribute("src"));
                    if (newUri.Segments[1] == "media/")
                    {
                        currImages.Add(new Uri(img.GetAttribute("src")));
                    }
                }
            }

            return currImages;
        }

        private void TwitterScraperResponse(object sender, ResponseCreatedEventArgs e)
        {
            try
            {
                //The standard video url we want is the following:
                //https://video.twimg.com/ext_tw_video/[folder_id]/pu/vid/[width]x[height]/[video_id].mp4?tag=12
                //https://video.twimg.com/amplify_video/[folder_id]/vid/[width]x[height]/[video_id].mp4?tag=14
                //https://video.twimg.com/tweet_video/[video_id].mp4
                if (e.Response.Url.StartsWith("https://video.twimg.com/") && e.Response.Url.Contains(".mp4"))
                {
                    Uri currUrl = new(e.Response.Url);

                    //Links get repeatedly sent by twitter, so we only want to save one of each
                    if (TempVideos.FirstOrDefault(x => x.Segments[2] == currUrl.Segments[2]) == null)
                    {
                        TempVideos.Add(currUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex.Message);
            }
        }
        #endregion
    }
}
