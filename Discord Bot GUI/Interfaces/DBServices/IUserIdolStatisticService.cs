using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IUserIdolStatisticService
{
    Task UpdateUserStatisticsAsync(ulong userId, Stack<int> ranking);
}
