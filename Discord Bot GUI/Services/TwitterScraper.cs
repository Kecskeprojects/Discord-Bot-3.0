using AngleSharp;
using AngleSharp.Dom;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.Services;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class TwitterScraper(Logging logger) : ITwitterScraper
    {
        private readonly Logging logger = logger;

        #region Variables
        //The standard video url is the following:
        //https://video.twimg.com/[folder_type]/[folder_id]/pu/vid/[width]x[height]/[video_id].mp4?tag=12
        private List<Uri> Videos { get; set; } = [];

        //Storing all video links before filtering
        private List<Uri> TempVideos { get; set; } = [];

        //The standard image url is the following:
        //https://pbs.twimg.com/media/[image_id]?format=[image_format]&name=[width]x[height]
        private List<Uri> Images { get; set; } = [];
        private List<string> Exceptions { get; } = [];
        private bool HasVideo { get; set; } = false;
        #endregion

        #region Main Methods
        public async Task<TwitterScrapingResult> GetDataFromUrls(List<Uri> uris)
        {
            try
            {
                if (BrowserService.Browser == null || BrowserService.Browser.IsClosed)
                {
                    await BrowserService.OpenBroser();
                }

                IPage mainPage = await BrowserService.CreateNewPage();
                mainPage.Response += TwitterScraperResponse;

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
            catch (NavigationException ex)
            {
                logger.Warning("TwitterScraper.cs GetDataFromUrls", ex.ToString());
                return new TwitterScrapingResult("Open link operation timed out after 15 seconds.");
            }
            catch (Exception ex)
            {
                logger.Error("TwitterScraper.cs GetDataFromUrls", ex.ToString());
                return new TwitterScrapingResult("Unexpected error occured");
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

                Images.AddRange(currImages);

                if (HasVideo)
                {
                    GetVideos(main, articles, mainPos, ref videosBeforeMainCount, ref videoCount);
                    Videos.AddRange(TempVideos.Skip(videosBeforeMainCount).Take(videoCount));
                }
            }
            catch (Exception ex)
            {
                logger.Error("TwitterScraper.cs ExtractFromUrl", ex.ToString());
            }

            return "";
        }

        private async Task<IDocument> OpenPage(IPage page, Uri uri)
        {
            await page.DeleteCookieAsync();
            await page.GoToAsync(uri.OriginalString);
            try
            {
                await page.WaitForSelectorAsync("div[data-testid=\"tweetPhoto\"]>img,div[data-testid=\"videoPlayer\"],div[data-testid=\"tweetText\"]", new WaitForSelectorOptions() { Timeout = 15000 });
                try
                {
                    await page.WaitForSelectorAsync("div[data-testid=\"videoPlayer\"]", new WaitForSelectorOptions() { Timeout = 2000 });
                    HasVideo = true;
                }
                catch (Exception)
                {
                    HasVideo = true;
                }
            }
            catch (Exception) { }
            string content = await page.GetContentAsync();

            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            IDocument document = await context.OpenAsync(req => req.Content(content));
            return document;
        }

        private void GetVideos(IElement main, IHtmlCollection<IElement> articles, int mainPos, ref int videosBeforeMainCount, ref int videoCount)
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
            while (TempVideos.Count < videoCount + videosBeforeMainCount && count < 10)
            {
                Task.Delay(500).Wait();
                count++;
            }
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
