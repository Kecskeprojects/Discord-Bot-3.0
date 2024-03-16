using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IIdolImageRepository
    {
        Task RemoveRangeAsync(ICollection<IdolImage> idolImages);
    }
}
