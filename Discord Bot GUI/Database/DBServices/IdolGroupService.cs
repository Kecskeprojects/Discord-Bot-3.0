using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class IdolGroupService(IIdolGroupRepository idolGroupRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IIdolGroupService
    {
        private readonly IIdolGroupRepository idolGroupRepository = idolGroupRepository;

        public async Task<IdolGroupExtendedResource> GetIdolGroupDetailsAsync(string group)
        {
            IdolGroupExtendedResource resource = null;
            try
            {
                IdolGroup idol = await idolGroupRepository.GetGroupAsync(group);

                if (idol == null)
                {
                    return resource;
                }

                resource = mapper.Map<IdolGroup, IdolGroupExtendedResource>(idol);
            }
            catch (Exception ex)
            {
                logger.Error("IdolGroupService.cs GetIdolGroupDetailsAsync", ex.ToString());
            }

            return resource;
        }
    }
}
