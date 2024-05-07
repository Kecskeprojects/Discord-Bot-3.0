using Discord_Bot.Communication.Bias;
using Discord_Bot.Communication.BiasScrape;
using PuppeteerSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface IKpopDbScraper
    {
        Task<List<ExtendedBiasData>> ExtractFromDatabaseTable(IPage page);
        Task<AdditionalIdolData> GetProfileDataAsync(IPage page, string url, bool getGroupData);
    }
}
