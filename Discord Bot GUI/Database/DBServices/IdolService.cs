using AutoMapper;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class IdolService(
        IIdolRepository idolRepository,
        IIdolGroupRepository idolGroupRepository,
        IIdolImageRepository idolImageRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IIdolService
    {
        private readonly IIdolRepository idolRepository = idolRepository;
        private readonly IIdolGroupRepository idolGroupRepository = idolGroupRepository;
        private readonly IIdolImageRepository idolImageRepository = idolImageRepository;

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
                Idol idol = await idolRepository.FirstOrDefaultByIdAsync(idolResource.IdolId);

                if (idol.IdolImages.Count > 3)
                {
                    await idolImageRepository.RemoveRangeAsync(idol.IdolImages.OrderBy(x => x.CreatedOn).Skip(3).ToList());
                }

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

        public async Task<DbProcessResultEnum> UpdateAsync(int idolId, EditIdolModal modal)
        {
            try
            {
                Idol idol = await idolRepository.FirstOrDefaultByIdAsync(idolId);
                idol.ProfileUrl = modal.ProfileURL;
                idol.DateOfBirth = DateOnly.TryParse(modal.DateOfBirth, out DateOnly dateOfBirth) ? dateOfBirth : idol.DateOfBirth;
                idol.Name = modal.Name.ToLower();
                idol.Gender =
                    modal.Gender.Equals(GenderType.Female, StringComparison.OrdinalIgnoreCase) ?
                        "F" :
                        (modal.Gender.Equals(GenderType.Male, StringComparison.OrdinalIgnoreCase) ?
                            "M" :
                            idol.Gender);
                idol.Group = await UpdateGroupAsync(idol.Group, modal.Group);
                await idolRepository.SaveChangesAsync();

                logger.Log($"Idol with ID {idolId} updated successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs UpdateAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> UpdateAsync(int idolId, EditIdolExtendedModal modal)
        {
            try
            {
                Idol idol = await idolRepository.FirstOrDefaultByIdAsync(idolId);
                idol.StageName = modal.StageName;
                idol.KoreanStageName = modal.KoreanStageName;
                idol.FullName = modal.FullName;
                idol.KoreanFullName = modal.KoreanFullName;
                idol.DebutDate = DateOnly.TryParse(modal.DebutDate, out DateOnly debutDate) ? debutDate : idol.DebutDate;
                await idolRepository.SaveChangesAsync();

                logger.Log($"Idol with ID {idolId} updated successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs UpdateAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> UpdateAsync(int idolId, ChangeIdolProfileLinkModal modal)
        {
            try
            {
                Idol idol = await idolRepository.FirstOrDefaultByIdAsync(idolId);
                idol.ProfileUrl = modal.ProfileURL;
                await idolRepository.SaveChangesAsync();

                logger.Log($"Idol with ID {idolId} updated successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs UpdateAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> UpdateAsync(int idolId, ChangeIdolGroupModal modal)
        {
            try
            {
                Idol idol = await idolRepository.FirstOrDefaultByIdAsync(idolId);
                idol.Group = await UpdateGroupAsync(idol.Group, modal.Group);
                await idolRepository.SaveChangesAsync();

                logger.Log($"Idol with ID {idolId} updated successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs UpdateAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        private async Task<IdolGroup> UpdateGroupAsync(IdolGroup group, string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return group;
            }

            IdolGroup newGroup = await idolGroupRepository.GetGroupAsync(groupName);
            newGroup ??= new IdolGroup()
            {
                Name = groupName,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };

            await idolGroupRepository.AddGroupAsync(newGroup);
            return newGroup;
        }
    }
}
