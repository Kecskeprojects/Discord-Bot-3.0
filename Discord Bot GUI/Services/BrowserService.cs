using PuppeteerSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class BrowserService
    {
        public static IBrowser Browser { get; set; }
        public static async Task OpenBroser()
        {
            BrowserFetcher browserFetcher = new(SupportedBrowser.Chrome);
            await browserFetcher.DownloadAsync("117.0.5938.62"); //Todo: PuppeteerSharp.BrowserData.Chrome.DefaultBuildId should be revisited in future, new builds remove option to download the way it is currently done
            IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
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

        public static async Task<IPage> CreateNewPage()
        {
            IPage mainPage = await BrowserService.Browser.NewPageAsync();
            Dictionary<string, string> headers = new()
                {
                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36" },
                    { "upgrade-insecure-requests", "1" },
                    { "accept", "text/html,application/xhtml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3" },
                    { "accept-encoding", "gzip, deflate, br" },
                    { "accept-language", "en-US,en;q=0.9,en;q=0.8" }
                };
            await mainPage.SetExtraHttpHeadersAsync(headers);

            return mainPage;
        }
    }
}
