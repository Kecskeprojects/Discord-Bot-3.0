using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class KeywordService : BaseService, IKeywordService
    {
        private readonly IKeywordRepository keywordRepository;
        public KeywordService(IMapper mapper, Logging logger, Cache cache, IKeywordRepository keywordRepository) : base(mapper, logger, cache) => this.keywordRepository = keywordRepository;

        public async Task<KeywordResource> GetKeywordAsync(ulong serverId, string trigger)
        {
            KeywordResource result = null;
            try
            {
                Keyword keyword = await keywordRepository.GetRoleAsync(serverId, trigger);
                result = mapper.Map<Keyword, KeywordResource>(keyword);
            }
            catch (Exception ex)
            {
                logger.Error("KeywordService.cs GetKeywordAsync", ex.ToString());
            }
            return result;
        }
    }
}
