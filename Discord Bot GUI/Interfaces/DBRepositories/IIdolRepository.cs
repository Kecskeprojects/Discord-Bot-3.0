using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories;

public interface IIdolRepository : IGenericRepository<Idol>
{
    Task<List<Idol>> GetListByNamesAsync(string idolOrGroupName, string idolGroup, string userId = null);
    Task<List<Idol>> GetListForGameAsync(GenderEnum gender, int debutAfter, int debutBefore);
}
