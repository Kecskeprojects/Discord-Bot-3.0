using AngleSharp.Dom;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord_Bot.Interfaces.Services;
using AngleSharp;
using Discord_Bot.Communication;
using Discord_Bot.Core;

namespace Discord_Bot.Services
{
    public class BiasDatabaseService (Logging logger) : IBiasDatabaseService
    {
        private const string BaseUrl = "https://dbkpop.com/db/all-k-pop-idols/";
        private readonly Logging logger = logger;

        //Get the db links and get image links using that, if they turn out to be too old, a manual command will be in place to switch out the link origin

        public async Task<List<ExtendedBiasData>> GetBiasDataAsync()
        {
            List<ExtendedBiasData> biasDataList = [];
            try
            {
                if (BrowserService.Browser == null || BrowserService.Browser.IsClosed)
                {
                    await BrowserService.OpenBroser();
                }

                IPage mainPage = await BrowserService.CreateNewPage();

                biasDataList = await ExtractFromDatabaseTable(mainPage);

                await mainPage.CloseAsync();
            }
            catch (NavigationException ex)
            {
                logger.Warning("BiasDatabaseService.cs GetBiasDataAsync", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("BiasDatabaseService.cs GetBiasDataAsync", ex.ToString());
            }

            return biasDataList;
        }

        private async Task<List<ExtendedBiasData>> ExtractFromDatabaseTable(IPage page)
        {
            List<ExtendedBiasData> biasDataList = [];
            try
            {
                Uri uri = new(BaseUrl);
                IDocument document = await GetFullList(page, uri);

                IElement table = document.QuerySelector("#table_1>tbody");

                IHtmlCollection<IElement> rows = table.GetElementsByTagName("tr");

                foreach (IElement row in rows)
                {
                    biasDataList.Add(new ExtendedBiasData(row));
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasDatabaseService.cs ExtractFromUrl", ex.ToString());
            }

            return biasDataList;
        }

        private async Task<IDocument> GetFullList(IPage page, Uri uri)
        {
            await page.DeleteCookieAsync();
            try
            {
                await page.GoToAsync(uri.OriginalString, 15000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.Networkidle0]);
            }
            catch (Exception) { }

            await RemovePopUps(page);

            //Click on Full Name, Korean Name, Date of Birth to make said rows not visible
            //await page.ClickAsync(".buttons-collection");
            //await page.WaitForSelectorAsync(".buttons-columnVisibility", new WaitForSelectorOptions() { Visible = true, Timeout = 0 });

            //await page.ClickAsync("button[data-cv-idx=\"2\"]");
            //await page.ClickAsync("button[data-cv-idx=\"3\"]");
            //await page.ClickAsync("button[data-cv-idx=\"5\"]");

            //Input "Profile" into profile search
            await page.TypeAsync(".column-profile>span>input", "Profile");

            //Show all entries
            await page.ClickAsync("#table_1_length .dropdown-toggle");
            await page.WaitForSelectorAsync("#table_1_length .dropdown-menu", new WaitForSelectorOptions() { Visible = true, Timeout = 0 });

            await page.ClickAsync("#table_1_length .dropdown-menu>ul>li[data-original-index=\"6\"]>a");
            await page.WaitForSelectorAsync("#table_1_length .dropdown-menu", new WaitForSelectorOptions() { Hidden = true, Timeout = 0 });

            string content = await page.GetContentAsync();

            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            IDocument document = await context.OpenAsync(req => req.Content(content));
            return document;
        }

        private static async Task RemovePopUps(IPage page)
        {
            try
            {
                await page.ClickAsync("#onesignal-slidedown-cancel-button");
            }
            catch (Exception) { }

            try
            {
                await page.ClickAsync(".fc-cta-consent");
            }
            catch (Exception) { }

            try
            {
                await page.ClickAsync("#cn-accept-cookie");
            }
            catch (Exception) { }
        }
    }
}
