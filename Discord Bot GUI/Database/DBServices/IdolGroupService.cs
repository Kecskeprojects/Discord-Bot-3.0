using AutoMapper;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
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

        public async Task<DbProcessResultEnum> UpdateAsync(int groupId, EditGroupModal modal)
        {
            try
            {
                IdolGroup idolGroup = await idolGroupRepository.FirstOrDefaultByIdAsync(groupId);

                await idolGroupRepository.SaveChangesAsync();

                logger.Log($"Group with ID {groupId} updated successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolGroupService.cs UpdateAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}
