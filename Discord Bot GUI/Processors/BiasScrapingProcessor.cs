﻿using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Processors
{
    public class BiasScrapingProcessor(
        IIdolService idolService,
        IKpopDbScraper kpopDbScraper,
        Logging logger)
    {
        private readonly IIdolService idolService = idolService;
        private readonly IKpopDbScraper kpopDbScraper = kpopDbScraper;
        private readonly Logging logger = logger;

        public async Task RunUpdateBiasDataAsync()
        {
            try
            {
                logger.Log("Update Bias Data Logic started!");
                List<ExtendedBiasData> completeList = await kpopDbScraper.ExtractFromDatabaseTableAsync();
                logger.Log($"Found {completeList.Count} idols on site that have profile pages.");

                List<IdolResource> localIdols = await idolService.GetAllIdolsAsync();
                logger.Log($"Found {localIdols.Count} idols in our database.");

                int count = 0;
                for (int i = 0; i < localIdols.Count; i++)
                {
                    await UpdateBiasAsync(localIdols, completeList, i);
                    count++;
                }

                logger.Log($"Updated {count} idol's details.");

                int correctionCount = await idolService.CorrectUpdateErrorsAsync();
                logger.Log($"Corrected {correctionCount} idols with errors created during update.");

                //Sometimes spam pages are opened so we clean the browser
                await BrowserService.CloseBrowser();

                logger.Log("Update Bias Data Logic ended!");
            }
            catch (Exception ex)
            {
                logger.Error("BiasScrapingProcessor.cs RunUpdateBiasDataAsync", ex);
            }
        }

        private async Task UpdateBiasAsync(List<IdolResource> localIdols, List<ExtendedBiasData> completeList, int i)
        {
            string profileUrl = GetProfileUrl(localIdols[i], completeList, out ExtendedBiasData data);

            if (string.IsNullOrEmpty(profileUrl))
            {
                logger.Warning("CoreLogic.cs UpdateExtendedBiasData", $"ProfileUrl empty. DATA: {data?.StageName} of {data?.GroupName} | DB: {localIdols[i].Name} of {localIdols[i].GroupName}");
                return;
            }

            AdditionalIdolData additional = await kpopDbScraper.GetProfileDataAsync(profileUrl, getGroupData: localIdols[i].GroupDebutDate == null);

            if (additional.ImageUrl == null)
            {
                logger.Warning("CoreLogic.cs UpdateExtendedBiasData", $"Image not found. DATA: {data?.StageName} of {data?.GroupName} | DB: {localIdols[i].Name} of {localIdols[i].GroupName}");
                return;
            }

            if (localIdols[i].CurrentImageUrl == additional.ImageUrl)
            {
                logger.Log($"Data up to date. DATA: already gathered | DB: {localIdols[i].Name} of {localIdols[i].GroupName}");
                return;
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
    }
}