using DatabaseORM.Communication.Bias;

namespace Discord_Bot.Interfaces.Services;

public interface IKpopDbScraper
{
    Task<List<ExtendedBiasData>> ExtractFromDatabaseTableAsync();
    Task<AdditionalIdolData?> GetProfileDataAsync(string url, bool getGroupData);
}
