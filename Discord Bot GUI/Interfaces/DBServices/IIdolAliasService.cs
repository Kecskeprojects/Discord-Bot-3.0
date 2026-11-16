using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IIdolAliasService
{
    Task<DbProcessResultEnum> AddIdolAliasAsync(string idolAlias, string idolName, string idolGroup);
    Task<DbProcessResultEnum> RemoveIdolAliasAsync(string idolAlias, string idolName, string idolGroup);
}
