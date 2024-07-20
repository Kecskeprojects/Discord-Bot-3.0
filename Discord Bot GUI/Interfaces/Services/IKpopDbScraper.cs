using Discord_Bot.Communication.Bias;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;
public interface IKpopDbScraper
{
    Task<List<ExtendedBiasData>> ExtractFromDatabaseTableAsync();
    Task<AdditionalIdolData> GetProfileDataAsync(string url, bool getGroupData);
}
