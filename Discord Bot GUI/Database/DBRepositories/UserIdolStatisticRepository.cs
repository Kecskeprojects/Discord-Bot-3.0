﻿using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Resources;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class UserIdolStatisticRepository(MainDbContext context) : GenericRepository<UserIdolStatistic>(context), IUserIdolStatisticRepository
    {
        public Task<List<UserIdolStatisticResource>> GetTop10ForUserAsync(int userId, GenderType gender)
        {
            return context.UserIdolStatistics
                .Include(stat => stat.Idol)
                .Include(i => i.Idol.Group)
                .Include(i => i.Idol.IdolImages)
                .Where(stat => stat.UserId == userId && (gender == GenderType.None.ToString() || stat.Idol.Gender == gender))
                .Select(stat => new UserIdolStatisticResource()
                {
                    IdolGroupFullName = stat.Idol.Group.FullName,
                    IdolStageName = stat.Idol.StageName,
                    Weight =
                        (stat.Placed1 * 2) +
                        (stat.Placed2 * 0.5) +
                        (stat.Placed3 * 0.25) +
                        (stat.Placed4 * 0.125) +
                        (stat.Placed5 * 0.001),
                })
                .OrderByDescending(statres => statres.Weight)
                .Take(10)
                .ToListAsync();
        }
    }
}
