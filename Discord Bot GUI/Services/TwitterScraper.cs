using AngleSharp;
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
        private List<Uri> Videos { get; set; } = new();

        //Storing all video links before filtering
        private List<Uri> TempVideos { get; set; } = new();

        //The standard image url is the following:
        //https://pbs.twimg.com/media/[image_id]?format=[image_format]&name=[width]x[height]
        private List<Uri> Images { get; set; } = new();
        private List<string> Exceptions { get; } = new List<string>();
        #endregion

        #region Main Methods
        public static async void OpenBroser()
        {
            BrowserFetcher browserFetcher = new(SupportedBrowser.Chrome);
            await browserFetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId);
            IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new string[]
                {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-web-security",
                        "--disable-features=IsolateOrigins",
                        "--disable-site-isolation-trials",
                        "--disable-features=BlockInsecurePrivateNetworkRequests",
                        "--ignore-certificate-errors"
                },
            });
            Browser = browser;
        }

        public static async void CloseBrowser() => await Browser.CloseAsync();

        public async Task<TwitterScrapingResult> GetDataFromUrls(List<Uri> uris)
        {
            try
            {
                if (Browser.IsClosed)
                {
                    OpenBroser();
                }

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

                string messages = "";
                foreach (Uri uri in uris)
                {
                    string mess = await ExtractFromUrl(mainPage, uri);
                    if (mess != "")
                    {
                        messages += $"\n<{uri.OriginalString}> could not be embedded:\n{mess}\n";
                    }
                }

                await mainPage.CloseAsync();

                messages += string.Join("\n", Exceptions);
                return string.IsNullOrEmpty(messages)
                    ? new TwitterScrapingResult(Videos, Images)
                    : new TwitterScrapingResult(Videos, Images, messages);
            }
            catch (Exception ex)
            {
                return new TwitterScrapingResult(ex.Message);
            }
        }
        #endregion



        #region Helper functions
        private async Task<string> ExtractFromUrl(IPage page, Uri uri)
        {
            try
            {
                int videosBeforeMainCount = 0;
                int videoCount = 0;

                await page.DeleteCookieAsync();
                await page.GoToAsync(uri.OriginalString, 15000, new WaitUntilNavigation[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.Networkidle0 });
                await Task.Delay(1000);
                string content = await page.GetContentAsync();

                IBrowsingContext context = BrowsingContext.New(Configuration.Default);
                IDocument document = await context.OpenAsync(req => req.Content(content));

                IHtmlCollection<IElement> articles = document.QuerySelectorAll("article");

                //The timestamp link contains the post's relative path, we can find the post's article by that, we also get it's index in the list
                IElement main = articles.First(x => x.QuerySelectorAll("a[href]").FirstOrDefault(e => e.GetAttribute("href").ToLower().Contains(uri.AbsolutePath.ToLower())) != null);
                int mainPos = articles.Index(main);

                if (main.Text().Contains("sensitive content"))
                {
                    return "Post is flagged as potentially sensitive content!";
                }

                //Getting the Images, if there are any
                List<Uri> currImages = new();
                foreach (IElement element in main.QuerySelectorAll("a"))
                {
                    IElement img = element.QuerySelector("img");
                    if (img != null)
                    {
                        Uri newUri = new(img.GetAttribute("src"));
                        if (newUri.Segments[1] == "media/") currImages.Add(new Uri(img.GetAttribute("src")));
                    }
                }

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

                Images.AddRange(currImages);
                Videos.AddRange(TempVideos.Skip(videosBeforeMainCount).Take(videoCount));
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
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
