using AutoMapper;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class IdolService(
        IIdolRepository idolRepository,
        IIdolGroupRepository idolGroupRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IIdolService
    {
        private readonly IIdolRepository idolRepository = idolRepository;
        private readonly IIdolGroupRepository idolGroupRepository = idolGroupRepository;

        public async Task<DbProcessResultEnum> AddIdolAsync(string idolName, string idolGroup)
        {
            try
            {
                if (await idolRepository.IdolExistsAsync(idolName, idolGroup))
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] is already in database!");
                    return DbProcessResultEnum.AlreadyExists;
                }

                IdolGroup group = await idolGroupRepository.GetGroupAsync(idolGroup);

                group ??= new()
                {
                    GroupId = 0,
                    Name = idolGroup,
                };

                Idol idol = new()
                {
                    IdolId = 0,
                    Group = group,
                    Name = idolName,
                };
                await idolRepository.AddIdolAsync(idol);

                logger.Log($"Idol [{idolName}]-[{idolGroup}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs AddIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<IdolResource>> GetIdolsByGroupAsync(string groupName)
        {
            List<IdolResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetIdolsByGroupAsync(groupName);

                result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetIdolsByGroupAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveIdolAsync(string idolName, string idolGroup)
        {
            try
            {
                Idol idol = await idolRepository.GetIdolByNameAndGroupAsync(idolName, idolGroup);
                if (idol != null)
                {
                    await idolRepository.RemoveIdolAsync(idol);

                    logger.Log($"Idol [{idolName}]-[{idolGroup}] removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs RemoveIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<IdolResource>> GetAllIdolsAsync()
        {
            List<IdolResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetAllIdolsAsync();

                result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetAllIdolsAsync", ex.ToString());
            }
            return result;
        }

        public async Task UpdateIdolDetailsAsync(IdolResource idolResource, ExtendedBiasData data, AdditionalIdolData additional)
        {
            try
            {
                //Todo: A removal method will potentially be needed for old images

                Idol idol = await idolRepository.FindByIdAsync(idolResource.IdolId);

                if (string.IsNullOrEmpty(idol.ProfileUrl) && data != null)
                {
                    mapper.Map(data, idol);
                }

                if (additional != null)
                {
                    idol.DebutDate = additional.DebutDate;
                    idol.IdolImages.Add(new IdolImage() { ImageUrl = additional.ImageUrl });

                    if (idol.Group.Name != "soloist" && idol.Group.DebutDate == null)
                    {
                        mapper.Map(additional, idol.Group);
                    }
                }

                await idolRepository.UpdateIdolAsync(idol);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs UpdateIdolDetailsAsync", ex.ToString());
            }
        }

        public async Task<IdolExtendedResource> GetIdolDetailsAsync(string idolName, string idolGroup)
        {
            IdolExtendedResource resource = null;
            try
            {
                Idol idol = await idolRepository.GetIdolByNameAndGroupAsync(idolName, idolGroup);

                if (idol == null)
                {
                    return resource;
                }

                resource = mapper.Map<Idol, IdolExtendedResource>(idol);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetIdolDetailsAsync", ex.ToString());
            }

            return resource;
        }
    }
}
