using AutoMapper;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
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
    public class IdolService(
        IIdolRepository idolRepository,
        IIdolGroupRepository idolGroupRepository,
        IIdolGroupService idolGroupService,
        IIdolImageRepository idolImageRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IIdolService
    {
        private readonly IIdolRepository idolRepository = idolRepository;
        private readonly IIdolGroupRepository idolGroupRepository = idolGroupRepository;
        private readonly IIdolGroupService idolGroupService = idolGroupService;
        private readonly IIdolImageRepository idolImageRepository = idolImageRepository;

        public async Task<DbProcessResultEnum> AddIdolAsync(string idolName, string idolGroup)
        {
            try
            {
                if (await idolRepository.ExistsAsync(
                    i => i.Name == idolName && i.Group.Name == idolGroup,
                    i => i.Group))
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] is already in database!");
                    return DbProcessResultEnum.AlreadyExists;
                }

                IdolGroup group = await idolGroupRepository.FirstOrDefaultAsync(ig => ig.Name == idolGroup);

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
                await idolRepository.AddAsync(idol);

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
                List<Idol> idols = await idolRepository.GetListAsync(
                    i => string.IsNullOrEmpty(groupName) ||
                    i.Group.Name == groupName,
                    ascending: true,
                    orderBy: i => i.Name,
                    includes: i => i.Group);

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
                Idol idol = await idolRepository.FirstOrDefaultAsync(
                    i => i.Name == idolName &&
                    i.Group.Name == idolGroup,
                    i => i.Group);
                if (idol != null)
                {
                    await idolRepository.RemoveAsync(idol);

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
                List<Idol> idols = await idolRepository.GetAllAsync(i => i.Group, i => i.IdolImages);

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
                Idol idol = await idolRepository.FirstOrDefaultAsync(i => i.IdolId == idolResource.IdolId, i => i.Group, i => i.IdolImages);

                if (idol.IdolImages.Count > 3)
                {
                    await idolImageRepository.RemoveAsync(idol.IdolImages.OrderBy(x => x.CreatedOn).Skip(3).ToList());
                }

                if (string.IsNullOrEmpty(idol.ProfileUrl) && data != null)
                {
                    mapper.Map(data, idol);
                }

                if (additional != null)
                {
                    idol.IdolImages.Add(new IdolImage() { ImageUrl = additional.ImageUrl });

                    if (idol.Group.Name != "soloist" && idol.Group.DebutDate == null)
                    {
                        mapper.Map(additional, idol.Group);
                    }
                }

                await idolRepository.UpdateAsync(idol);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs UpdateIdolDetailsAsync", ex.ToString());
            }
        }

        public async Task<int> CorrectUpdateErrorsAsync()
        {
            int count = 0;
            try
            {
                List<Idol> idols =
                    await idolRepository.GetListAsync(i =>
                        i.Group.DebutDate != null &&
                        i.DebutDate == null &&
                        i.ProfileUrl != null,
                        i => i.Group);

                if (idols.Count > 0)
                {
                    foreach (Idol item in idols)
                    {
                        item.DebutDate = item.Group.DebutDate;
                    }

                    await idolRepository.SaveChangesAsync();
                    count = idols.Count;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs CorrectUpdateErrorsAsync", ex.ToString());
            }

            return count;
        }

        public async Task<IdolExtendedResource> GetIdolDetailsAsync(string idolName, string idolGroup)
        {
            IdolExtendedResource resource = null;
            try
            {
                Idol idol = await idolRepository.FirstOrDefaultAsync(
                    i => i.Name == idolName &&
                    i.Group.Name == idolGroup,
                    i => i.Group);

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
                Idol idol = await idolRepository.FirstOrDefaultAsync(i => i.IdolId == idolId, i => i.Group);
                idol.ProfileUrl = modal.ProfileURL.Trim();
                idol.DateOfBirth = DateOnly.TryParse(modal.DateOfBirth, out DateOnly dateOfBirth) ? dateOfBirth : idol.DateOfBirth;
                idol.Name = modal.Name.ToLower().Trim();
                idol.Gender =
                    modal.Gender.Equals(GenderType.Female, StringComparison.OrdinalIgnoreCase) ?
                        "F" :
                        (modal.Gender.Equals(GenderType.Male, StringComparison.OrdinalIgnoreCase) ?
                            "M" :
                            idol.Gender);
                idol.Group = await idolGroupService.UpdateOrCreateGroupAsync(idol.Group, modal.Group.ToLower().Trim());
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
                Idol idol = await idolRepository.FirstOrDefaultAsync(i => i.IdolId == idolId);
                idol.StageName = modal.StageName.Trim();
                idol.KoreanStageName = modal.KoreanStageName.Trim();
                idol.FullName = modal.FullName.Trim();
                idol.KoreanFullName = modal.KoreanFullName.Trim();
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
                Idol idol = await idolRepository.FirstOrDefaultAsync(i => i.IdolId == idolId);
                idol.ProfileUrl = modal.ProfileURL.Trim();
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
                Idol idol = await idolRepository.FirstOrDefaultAsync(i => i.IdolId == idolId);
                idol.Group = await idolGroupService.UpdateOrCreateGroupAsync(idol.Group, modal.Group.ToLower().Trim());
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

        public async Task<ListWithDbResult<UserResource>> GetUsersByIdolsAsync(string[] nameList)
        {
            ListWithDbResult<UserResource> result = new(null, DbProcessResultEnum.Failure);
            try
            {
                List<Idol> idols = [];
                foreach (string item in nameList)
                {
                    string[] parts = item.Split("-");
                    string idolOrGroupName = parts[0];
                    string groupName = parts.Length == 2 ? parts[1] : "";
                    List<Idol> newIdols = await idolRepository.GetListByNamesAsync(idolOrGroupName, groupName);
                    idols = idols.Union(newIdols).ToList();
                }

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
                logger.Error("IdolService.cs GetUsersByIdolsAsync", ex.ToString());
            }
            return result;
        }

        public async Task<List<IdolGameResource>> GetListForGameAsync(GenderType gender, int debutAfter, int debutBefore)
        {
            List<IdolGameResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetListForGameAsync(gender, debutAfter, debutBefore);

                result = mapper.Map<List<Idol>, List<IdolGameResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetListForGameAsync", ex.ToString());
            }
            return result;
        }

        public async Task<IdolGameResource> GetIdolByIdAsync(int idolId)
        {
            IdolGameResource result = null;
            try
            {
                Idol idol = await idolRepository.FirstOrDefaultAsync(x => x.IdolId == idolId, x => x.Group, x => x.IdolImages);

                result = mapper.Map<Idol, IdolGameResource>(idol);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetIdolByIdAsync", ex.ToString());
            }

            return result;
        }
    }
}
