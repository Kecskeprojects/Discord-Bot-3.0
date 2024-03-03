using AngleSharp;
using AngleSharp.Dom;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class BiasDatabaseService(Logging logger, IIdolService idolService) : IBiasDatabaseService
    {
        private const string BaseUrl = "https://dbkpop.com/db/all-k-pop-idols/";
        private readonly Logging logger = logger;
        private readonly IIdolService idolService = idolService;

        #region Bias List methods
        public async Task RunUpdateBiasDataAsync()
        {
            try
            {
                logger.Log("Update Bias Data Logic started!");
                List<ExtendedBiasData> completeList = await GetBiasWebDataAsync();
                logger.Log($"Found {completeList.Count} idols on site that have profile pages.");

                List<IdolResource> localIdols = await idolService.GetAllIdolsAsync();
                logger.Log($"Found {localIdols.Count} idols in our database.");

                int count = 0;
                for (int i = 0; i < localIdols.Count; i++)
                {
                    string profileUrl = GetProfileUrl(localIdols[i], completeList, out ExtendedBiasData data);

                    if (string.IsNullOrEmpty(profileUrl))
                    {
                        logger.Warning("CoreLogic.cs UpdateExtendedBiasData", $"ProfileUrl empty. DATA: {data?.StageName} of {data?.GroupName} | DB: {localIdols[i].Name} of {localIdols[i].GroupName}");
                        continue;
                    }

                    AdditionalIdolData additional = await GetAdditionalBiasDataAsync(profileUrl, getGroupData: localIdols[i].GroupDebutDate == null);

                    if (localIdols[i].CurrentImageUrl == additional.ImageUrl)
                    {
                        continue;
                    }

                    if (data != null)
                    {
                        logger.Log($"Updating details. DATA: {data.StageName} of {data.GroupName} | DB: {localIdols[i].Name} of {localIdols[i].GroupName}");

                        if (!localIdols[i].GroupName.Equals(data.GroupName.RemoveSpecialCharacters(), StringComparison.OrdinalIgnoreCase))
                        {
                            additional = null;
                            logger.Warning("CoreLogic.cs UpdateExtendedBiasData", "Idol's group in database and site do not match, the result may be inconsistent.");
                        }
                    }
                    else
                    {
                        logger.Log($"Updating details. DATA: Already gathered | DB: {localIdols[i].Name} of {localIdols[i].GroupName}");
                    }

                    count++;
                    await idolService.UpdateIdolDetailsAsync(localIdols[i], data, additional);
                }
                logger.Log($"Updated {count} idol's details.");
                logger.Log("Update Bias Data Logic ended!");
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs UpdateExtendedBiasData", ex.ToString());
            }
        }

        private async Task<List<ExtendedBiasData>> GetBiasWebDataAsync()
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

        private async Task<AdditionalIdolData> GetAdditionalBiasDataAsync(string url, bool getGroupData)
        {
            AdditionalIdolData idolData = null;
            try
            {
                if (BrowserService.Browser == null || BrowserService.Browser.IsClosed)
                {
                    await BrowserService.OpenBroser();
                }

                IPage mainPage = await BrowserService.CreateNewPage();

                idolData = await GetProfileDataAsync(mainPage, url, getGroupData);

                await mainPage.CloseAsync();
            }
            catch (NavigationException ex)
            {
                logger.Warning("BiasDatabaseService.cs GetAdditionalBiasDataAsync", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("BiasDatabaseService.cs GetAdditionalBiasDataAsync", ex.ToString());
            }
            return idolData;
        }
        #endregion

        #region Helper
        private static string GetProfileUrl(IdolResource resource, List<ExtendedBiasData> completeList, out ExtendedBiasData data)
        {
            string profileUrl = "";
            data = null;
            if (!string.IsNullOrEmpty(resource.ProfileUrl))
            {
                profileUrl = resource.ProfileUrl;
            }
            else
            {
                List<ExtendedBiasData> datas = completeList.Where(x => x.StageName.Equals(resource.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (datas.Count > 1)
                {
                    data = datas.FirstOrDefault(x => x.GroupName.RemoveSpecialCharacters().Equals(resource.GroupName, StringComparison.OrdinalIgnoreCase) ||
                                                    (resource.GroupName == "soloist" && string.IsNullOrEmpty(x.GroupName)));
                }
                else if (datas.Count == 1)
                {
                    data = datas[0];
                }
                profileUrl = data?.ProfileUrl;
            }
            return profileUrl;
        }

        private static async Task<List<ExtendedBiasData>> ExtractFromDatabaseTable(IPage page)
        {
            Uri uri = new(BaseUrl);
            IDocument document = await GetFullList(page, uri);

            IElement table = document.QuerySelector("#table_1>tbody");

            IHtmlCollection<IElement> rows = table.GetElementsByTagName("tr");

            List<ExtendedBiasData> biasDataList = [];
            foreach (IElement row in rows)
            {
                biasDataList.Add(new ExtendedBiasData(row));
            }

            return biasDataList;
        }

        private static async Task<IDocument> GetFullList(IPage page, Uri uri)
        {
            await page.DeleteCookieAsync();
            try
            {
                await page.GoToAsync(uri.OriginalString, 15000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.Networkidle0]);
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

        private static async Task<AdditionalIdolData> GetProfileDataAsync(IPage page, string url, bool getGroupData)
        {
            Uri uri = new(url);
            IDocument document = await GetPageByUrl(page, uri);

            //A profile link could lead to dbkpop or kprofiles
            AdditionalIdolData data = new();
            if (!url.StartsWith("https://kprofiles.com/"))
            {
                data.ImageUrl = document.QuerySelector(".attachment-post-thumbnail")?.GetAttribute("src");
                await GetGroupData(data, page, document, getGroupData);
            }
            else
            {
                data.ImageUrl = document.QuerySelector(".entry-content img").GetAttribute("src");
            }

            return data;
        }

        private static async Task GetGroupData(AdditionalIdolData data, IPage page, IDocument document, bool getGroupData)
        {
            IHtmlCollection<IElement> groups = document.QuerySelectorAll("li>a[href*=\"https://dbkpop.com/group\"]");
            string groupUrl = "";
            if (groups != null && groups.Length > 0)
            {
                //The last one should be the most recent group the idol was a member of
                groupUrl = groups.Last().GetAttribute("href");
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
                await page.GoToAsync(uri.OriginalString, 15000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.Networkidle0]);
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
