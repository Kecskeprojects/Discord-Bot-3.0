using AutoMapper;
using Discord_Bot.Communication;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class IdolService(IIdolRepository idolRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IIdolService
    {
        private readonly IIdolRepository idolRepository = idolRepository;

        public Task<DbProcessResultEnum> AddBiasAsync(string biasName, string biasGroup)
        {
            throw new NotImplementedException();
        }

        public Task<DbProcessResultEnum> AddUserBiasAsync(ulong id, string biasName, string biasGroup)
        {
            throw new NotImplementedException();
        }

        public Task<DbProcessResultEnum> ClearUserBiasAsync(ulong id)
        {
            throw new NotImplementedException();
        }

        public Task<List<IdolResource>> GetBiasesByGroupAsync(string groupName)
        {
            throw new NotImplementedException();
        }

        public Task<List<IdolResource>> GetUserBiasesByGroupAsync(string groupName, ulong userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ListWithDbResult<UserResource>> GetUsersWithBiasesAsync(string[] nameList)
        {
            ListWithDbResult<UserResource> result = new(null, DbProcessResultEnum.Failure);
            try
            {
                List<Idol> idols = await idolRepository.GetBiasesByNamesAsync(nameList);

                if (CollectionTools.IsNullOrEmpty(idols))
                {
                    result.ProcessResultEnum = DbProcessResultEnum.NotFound;
                    return result;
                }

                List<User> users = idols.SelectMany(i => i.Users).DistinctBy(u => u.UserId).ToList();

                result.List = mapper.Map<List<User>, List<UserResource>>(users);
                result.ProcessResultEnum = DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("GreetingService.cs GetAllGreetingAsync", ex.ToString());
            }
            return result;
        }

        public Task<DbProcessResultEnum> RemoveBiasAsync(string biasName, string biasGroup)
        {
            throw new NotImplementedException();
        }

        public List<IdolResource> UserBiasesListAsync(ulong userId, string groupName)
        {
            throw new NotImplementedException();
        }

        ListWithDbResult<UserResource> IIdolService.GetUsersWithBiasesAsync(string[] nameList)
        {
            throw new NotImplementedException();
        }
    }
}
