using DatabaseORM.Caching;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Tools;
using MapsterMapper;

namespace DatabaseORM.Service;

public class UserIdolStatisticService(
    IUserIdolStatisticRepository userIdolStatisticRepository,
    IUserRepository userRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IUserIdolStatisticService
{
    private readonly IUserIdolStatisticRepository userIdolStatisticRepository = userIdolStatisticRepository;

    public async Task UpdateUserStatisticsAsync(ulong userId, Stack<int> ranking)
    {
        try
        {
            User? user = await userRepository.FirstOrDefaultAsync(x => x.DiscordId == userId.ToString());

            if (user == null)
            {
                user = new()
                {
                    DiscordId = userId.ToString()
                };
                _ = await userRepository.AddAsync(user);
            }

            int i = 1;
            while (ranking != null && ranking.Count > 0)
            {
                int idolId = ranking.Pop();
                UserIdolStatistic? userIdolStatistic = await userIdolStatisticRepository
                    .FirstOrDefaultAsync(i =>
                        i.User.DiscordId == userId.ToString()
                        && idolId == i.IdolId,
                        i => i.User);

                if (userIdolStatistic == null)
                {
                    userIdolStatistic = new()
                    {
                        IdolId = idolId,
                        User = user
                    };

                    _ = await userIdolStatisticRepository.AddAsync(userIdolStatistic, saveChanges: false);
                }

                UserIdolStatisticTools.AddRanking(userIdolStatistic, i);
                _ = await userIdolStatisticRepository.SaveChangesAsync();

                i++;
            }

            user.BiasGameCount++;
            _ = await userRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolStatisticService.cs UpdateUserStatisticsAsync", ex);
        }
    }
}
