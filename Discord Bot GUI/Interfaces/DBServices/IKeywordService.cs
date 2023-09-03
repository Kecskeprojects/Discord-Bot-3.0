using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IKeywordService
    {
        Task<KeywordResource> GetKeywordAsync(ulong serverId, string trigger);
    }
}
