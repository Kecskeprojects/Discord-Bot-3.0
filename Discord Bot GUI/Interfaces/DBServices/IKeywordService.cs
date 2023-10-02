using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IKeywordService
    {
        Task<DbProcessResultEnum> AddKeywordAsync(ulong serverId, string trigger, string response);
        Task<KeywordResource> GetKeywordAsync(ulong serverId, string trigger);
        Task<DbProcessResultEnum> RemoveKeywordAsync(ulong serverId, string trigger);
    }
}
