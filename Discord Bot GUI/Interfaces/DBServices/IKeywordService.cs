using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IKeywordService
{
    Task<DbProcessResultEnum> AddKeywordAsync(ulong serverId, string trigger, string response);
    Task<KeywordResource> GetKeywordAsync(ulong serverId, string trigger);
    Task<List<KeywordResource>> ListKeywordsByServerIdAsync(ulong serverId);
    Task<DbProcessResultEnum> RemoveKeywordAsync(ulong serverId, string trigger);
}
