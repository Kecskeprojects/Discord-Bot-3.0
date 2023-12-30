using Discord_Bot.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface IBiasDatabaseService
    {
        Task<AdditionalIdolData> GetAdditionalBiasDataAsync(string url, bool getGroupData);
        Task<List<ExtendedBiasData>> GetBiasDataAsync();
    }
}
