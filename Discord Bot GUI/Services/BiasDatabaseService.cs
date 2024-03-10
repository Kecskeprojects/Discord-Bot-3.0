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
    public class BiasDatabaseService(Logging logger, IIdolService idolService, IKpopDbScraper kpopDbScraper) : IBiasDatabaseService
    {
        private readonly Logging logger = logger;
        private readonly IIdolService idolService = idolService;
        private readonly IKpopDbScraper kpopDbScraper = kpopDbScraper;

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

                    await idolService.UpdateIdolDetailsAsync(localIdols[i], data, additional);
                    count++;
                }

                logger.Log($"Updated {count} idol's details.");
                logger.Log("Update Bias Data Logic ended!");
            }
            catch (Exception ex)
            {
                logger.Error("CoreLogic.cs UpdateExtendedBiasData", ex.ToString());
            }
        }
        #endregion

        #region Helper
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

                biasDataList = await kpopDbScraper.ExtractFromDatabaseTable(mainPage);

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

                idolData = await kpopDbScraper.GetProfileDataAsync(mainPage, url, getGroupData);

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
        #endregion
    }
}
