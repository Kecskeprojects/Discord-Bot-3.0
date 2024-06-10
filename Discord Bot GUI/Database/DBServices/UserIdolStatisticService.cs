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

namespace Discord_Bot.Database.DBServices;

public class UserIdolStatisticService(
    IUserIdolStatisticRepository userIdolStatisticRepository,
    IUserRepository userRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IUserIdolStatisticService
{
    private readonly IUserIdolStatisticRepository userIdolStatisticRepository = userIdolStatisticRepository;

    public async Task UpdateUserStatisticsAsync(ulong userId, Stack<int> ranking)
    {
        try
        {
            int i = 1;

            User user = await userRepository.FirstOrDefaultAsync(x => x.DiscordId == userId.ToString());

            if (user == null)
            {
                user = new()
                {
                    DiscordId = userId.ToString()
                };
                await userRepository.AddAsync(user);
            }

            while (ranking != null && ranking.Count > 0)
            {
                int idolId = ranking.Pop();
                UserIdolStatistic userIdolStatistic = await userIdolStatisticRepository
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

                    await userIdolStatisticRepository.AddAsync(userIdolStatistic, saveChanges: false);
                }

                UserIdolStatisticTools.AddRanking(userIdolStatistic, i);
                await userIdolStatisticRepository.SaveChangesAsync();

                i++;
            }

            user.BiasGameCount++;
            await userRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolStatisticService.cs UpdateUserStatisticsAsync", ex);
        }
    }
}
