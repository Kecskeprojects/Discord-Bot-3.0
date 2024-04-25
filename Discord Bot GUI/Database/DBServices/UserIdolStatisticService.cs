using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Tools.ModelTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class UserIdolStatisticService(
        IUserIdolStatisticRepository userIdolStatisticRepository,
        IUserRepository userRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IUserIdolStatisticService
    {
        private readonly IUserIdolStatisticRepository userIdolStatisticRepository = userIdolStatisticRepository;

        public async Task UpdateUserStatisticsAsync(ulong userId, Stack<int> ranking)
        {
            try
            {
                int i = 1;
                while (ranking != null && ranking.Count > 0)
                {
                    int idolId = ranking.Pop();
                    UserIdolStatistic userIdolStatistic = await userIdolStatisticRepository
                        .FirstOrDefaultAsync(x => x.User.DiscordId == userId.ToString() && idolId == x.IdolId, x => x.User);

                    if (userIdolStatistic == null)
                    {
                        userIdolStatistic = new() { IdolId = idolId };

                        User user = await userRepository.FirstOrDefaultAsync(x => x.DiscordId == userId.ToString());

                        if (user == null)
                        {
                            user = new() { DiscordId = userId.ToString() };
                            await userRepository.AddAsync(user);
                        }

                        userIdolStatistic.User = user;

                        await userIdolStatisticRepository.AddAsync(userIdolStatistic, saveChanges: false);
                    }

                    userIdolStatistic.User.BiasGameCount++;
                    UserIdolStatisticTools.AddRanking(userIdolStatistic, i);
                    await userIdolStatisticRepository.SaveChangesAsync();

                    i++;
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserIdolStatisticService.cs UpdateUserStatisticsAsync", ex.ToString());
            }
}
    }
}
