﻿using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using PuppeteerSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class BrowserService(Logging logger, Config config)
    {
        private readonly Logging logger = logger;
        private readonly Config config = config;
        private IBrowser Browser { get; set; }
        public async Task OpenBroser()
        {
            BrowserFetcher browserFetcher = new(SupportedBrowser.Chrome);
            await browserFetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId); //117.0.5938.62 is the last version where videos were sent as media http responses
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = config.Headless,
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
        }

        public async Task CloseBrowser()
        {
            if (Browser != null && !Browser.IsClosed)
            {
                await Browser.CloseAsync();
                logger.Log("Browser closed!");
            }
        }

        public  async Task<IPage> NewPage()
        {
            if (Browser == null || Browser.IsClosed)
            {
                await OpenBroser();
            }

            IPage[] pages = await Browser.PagesAsync();

            IPage mainPage = await Browser.NewPageAsync();
            logger.Log($"New Browser Page opened. (Total pages: {pages.Length + 1})", LogOnly: true);

            Dictionary<string, string> headers = new()
                {
                    //{ "user-agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{PuppeteerSharp.BrowserData.Chrome.DefaultBuildId} Safari/537.36" },
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
