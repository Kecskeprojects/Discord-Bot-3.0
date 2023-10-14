using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class BirthdayService : BaseService, IBirthdayService
    {
        public BirthdayService(IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache)
        {
        }

        public Task<DbProcessResultEnum> AddBirthdayAsync(ulong serverId, ulong userId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<List<BirthdayResource>> GetBirthdaysByDateAsync(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Task<DbProcessResultEnum> RemoveBirthdayAsync(ulong serverId, ulong userId)
        {
            throw new NotImplementedException();
        }
    }
}
