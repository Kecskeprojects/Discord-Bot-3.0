using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IIdolAliasRepository
    {
        Task<IdolAlias> GetIdolAliasAsync(string idolAlias, string idolName, string idolGroup);
        Task<bool> IdolAliasExistsAsync(string idolAlias, string idolName, string idolGroup);
        Task RemoveIdolAliasAsync(IdolAlias idolAliasItem);
    }
}
