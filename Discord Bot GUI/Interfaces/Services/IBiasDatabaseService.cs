using Discord_Bot.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface IBiasDatabaseService
    {
        Task<List<ExtendedBiasData>> GetBiasDataAsync();
    }
}
