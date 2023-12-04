using PuppeteerSharp;
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
    }
}
