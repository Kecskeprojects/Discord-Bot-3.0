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
            await browserFetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId); //117.0.5938.62 is the last version where videos were sent as media http responses
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
                //HeadlessMode = HeadlessMode.Shell
                //ExecutablePath = "Chrome\\Win64-117.0.5938.62\\chrome-win64\\chrome.exe"
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
