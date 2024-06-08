using AngleSharp;
using AngleSharp.Dom;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.Services;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services;

public class KpopDbScraper(BotLogger logger, BrowserService browserService) : IKpopDbScraper
{
    private readonly BotLogger logger = logger;
    private readonly BrowserService browserService = browserService;

    public async Task<List<ExtendedBiasData>> ExtractFromDatabaseTableAsync()
    {
        List<ExtendedBiasData> biasDataList = [];
        try
        {
            IPage mainPage = await browserService.NewPage();

            IDocument document = await GetPageAndSetSettingsAsync(mainPage);

            IElement table = document.QuerySelector("#table_1>tbody");

            IHtmlCollection<IElement> rows = table.GetElementsByTagName("tr");

            foreach (IElement row in rows)
            {
                biasDataList.Add(new ExtendedBiasData(row));
            }

            await mainPage.CloseAsync();
        }
        catch (NavigationException ex)
        {
            logger.Warning("KpopDbScraper.cs ExtractFromDatabaseTableAsync", ex);
        }
        catch (Exception ex)
        {
            logger.Error("KpopDbScraper.cs ExtractFromDatabaseTableAsync", ex);
        }

        return biasDataList;
    }

    private static async Task<IDocument> GetPageAndSetSettingsAsync(IPage page)
    {
        await page.DeleteCookieAsync();
        try
        {
            await page.GoToAsync(Constant.DbKpopScrapeBaseUri.OriginalString, 600000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded]);
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

    public async Task<AdditionalIdolData> GetProfileDataAsync(string url, bool getGroupData)
    {
        AdditionalIdolData idolData = null;
        try
        {
            IPage mainPage = await browserService.NewPage();

            Uri uri = new(url);
            IDocument document = await GetPageByUrl(mainPage, uri, url.StartsWith(Constant.KProfilesBaseUrl));

            //A profile link could lead to dbkpop or kprofiles
            idolData = new();
            if (!url.StartsWith(Constant.KProfilesBaseUrl))
            {
                idolData.ImageUrl = document.QuerySelector(".attachment-post-thumbnail")?.GetAttribute("src");
                await ScrapeGroupData(idolData, mainPage, document, getGroupData);
            }
            else
            {
                idolData.ImageUrl = document.QuerySelector(".entry-content img")?.GetAttribute("src");
            }

            await mainPage.CloseAsync();
        }
        catch (NavigationException ex)
        {
            logger.Warning("BiasDatabaseService.cs GetAdditionalBiasDataAsync", ex);
        }
        catch (Exception ex)
        {
            logger.Error("BiasDatabaseService.cs GetAdditionalBiasDataAsync", ex);
        }
        return idolData;
    }
    private static async Task ScrapeGroupData(AdditionalIdolData data, IPage page, IDocument document, bool getGroupData)
    {
        IHtmlCollection<IElement> groups = document.QuerySelectorAll($"li>a[href*=\"{Path.Combine(Constant.DbKpopBaseUrl, "group")}\"]");
        string groupUrl = "";
        if (groups != null && groups.Length > 0)
        {
            //The first one should be the most likely accurate group the idol was a member of
            groupUrl = groups.First().GetAttribute("href");
        }

        if (getGroupData && !string.IsNullOrEmpty(groupUrl))
        {
            document = await GetPageByUrl(page, new Uri(groupUrl), false);
            IHtmlCollection<IElement> details = document.QuerySelectorAll(".wpb-content-wrapper .vc_sw-align-left");
            if (details.Length >= 3)
            {
                data.GroupFullName = Uri.UnescapeDataString(details[0].InnerHtml);
                data.GroupFullKoreanName = Uri.UnescapeDataString(details[1].InnerHtml);
                data.DebutDate = DateOnly.TryParse(Uri.UnescapeDataString(details[2].InnerHtml), out DateOnly date) ? date : null;
            }
        }
    }

    #region Helper Methods
    private static async Task<IDocument> GetPageByUrl(IPage page, Uri uri, bool isKprofiles)
    {
        await page.DeleteCookieAsync();
        try
        {
            await page.GoToAsync(uri.OriginalString, 60000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded]);
        }
        catch (Exception) { }

        await RemovePopUps(page, isKprofiles);

        string content = await page.GetContentAsync();

        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        IDocument document = await context.OpenAsync(req => req.Content(content));
        return document;
    }

    private static async Task RemovePopUps(IPage page, bool isKprofiles = false)
    {
        if (isKprofiles)
        {
            try
            {
                await page.ClickAsync(".last-focusable-el");
            }
            catch (Exception) { }
        }
        else
        {
            try
            {
                await page.ClickAsync("#ez-accept-all");
            }
            catch (Exception) { }

            try
            {
                await page.WaitForSelectorAsync("ins.ezfound");
                await page.ClickAsync("ins.ezfound", new ClickOptions() { Button = MouseButton.Right, OffSet = new Offset(10, 10) });
            }
            catch (Exception) { }

            try
            {
                await page.ClickAsync("#onesignal-slidedown-cancel-button");
            }
            catch (Exception) { }

            try
            {
                await page.ClickAsync(".ezmob-footer-close");
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
    #endregion
}
