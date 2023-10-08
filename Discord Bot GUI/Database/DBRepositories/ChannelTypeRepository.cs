﻿using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ChannelTypeRepository : BaseRepository, IChannelTypeRepository
    {
        public ChannelTypeRepository(MainDbContext context) : base(context)
        {
        }

        public Task<ChannelType> GetChannelTypeByIdAsync(ChannelTypeEnum channelTypeId)
        {
            return context.ChannelTypes
                .FirstOrDefaultAsync(ct => ct.ChannelTypeId == (int)channelTypeId);
        }
    }
}
