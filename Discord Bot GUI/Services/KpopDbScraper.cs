using AngleSharp;
using AngleSharp.Dom;
using Discord_Bot.Communication;
using Discord_Bot.Interfaces.Services;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class KpopDbScraper : IKpopDbScraper
    {
        private static Uri BaseUrl { get; } = new("https://dbkpop.com/db/all-k-pop-idols/");

        public async Task<List<ExtendedBiasData>> ExtractFromDatabaseTable(IPage page)
        {
            IDocument document = await GetPageAndSetSettings(page);

            IElement table = document.QuerySelector("#table_1>tbody");

            IHtmlCollection<IElement> rows = table.GetElementsByTagName("tr");

            List<ExtendedBiasData> biasDataList = [];
            foreach (IElement row in rows)
            {
                biasDataList.Add(new ExtendedBiasData(row));
            }

            return biasDataList;
        }

        public async Task<AdditionalIdolData> GetProfileDataAsync(IPage page, string url, bool getGroupData)
        {
            Uri uri = new(url);
            IDocument document = await GetPageByUrl(page, uri);

            //A profile link could lead to dbkpop or kprofiles
            AdditionalIdolData data = new();
            if (!url.StartsWith("https://kprofiles.com/"))
            {
                data.ImageUrl = document.QuerySelector(".attachment-post-thumbnail")?.GetAttribute("src");
                await ScrapeGroupData(data, page, document, getGroupData);
            }
            else
            {
                data.ImageUrl = document.QuerySelector(".entry-content img").GetAttribute("src");
            }

            return data;
        }

        #region Helper Methods
        private static async Task<IDocument> GetPageAndSetSettings(IPage page)
        {
            await page.DeleteCookieAsync();
            try
            {
                await page.GoToAsync(BaseUrl.OriginalString, 15000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.Networkidle0]);
            }
            catch (Exception) { }

            await RemovePopUps(page);

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

        private static async Task ScrapeGroupData(AdditionalIdolData data, IPage page, IDocument document, bool getGroupData)
        {
            IHtmlCollection<IElement> groups = document.QuerySelectorAll("li>a[href*=\"https://dbkpop.com/group\"]");
            string groupUrl = "";
            if (groups != null && groups.Length > 0)
            {
                //The first one should be the most likely accurate group the idol was a member of
                groupUrl = groups.First().GetAttribute("href");
            }

            if (getGroupData && !string.IsNullOrEmpty(groupUrl))
            {
                document = await GetPageByUrl(page, new Uri(groupUrl));
                IHtmlCollection<IElement> details = document.QuerySelectorAll(".wpb-content-wrapper .vc_sw-align-left");
                if (details.Length >= 3)
                {
                    data.GroupFullName = details[0].InnerHtml;
                    data.GroupFullKoreanName = details[1].InnerHtml;
                    data.DebutDate = DateOnly.TryParse(details[2].InnerHtml, out DateOnly date) ? date : null;
                }
            }
        }

        private static async Task<IDocument> GetPageByUrl(IPage page, Uri uri)
        {
            await page.DeleteCookieAsync();
            try
            {
                await page.GoToAsync(uri.OriginalString, 15000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded]);
            }
            catch (Exception) { }

            await RemovePopUps(page);

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
                await page.ClickAsync("#ez-accept-all");
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
        #endregion
    }
}
