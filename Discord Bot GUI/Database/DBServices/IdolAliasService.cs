using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class IdolAliasService(
        IIdolAliasRepository idolAliasRepository,
        IIdolRepository idolRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IIdolAliasService
    {
        private readonly IIdolAliasRepository idolAliasRepository = idolAliasRepository;
        private readonly IIdolRepository idolRepository = idolRepository;

        public async Task<DbProcessResultEnum> AddIdolAliasAsync(string idolAlias, string idolName, string idolGroup)
        {
            try
            {
                if (await idolAliasRepository.ExistsAsync(
                    ia => ia.Alias == idolAlias &&
                    ia.Idol.Name == idolName &&
                    ia.Idol.Group.Name == idolGroup,
                    ia => ia.Idol,
                    ia => ia.Idol.Group))
                {
                    logger.Log($"Alias [{idolAlias}-{idolName}]-[{idolGroup}] is already in database!");
                    return DbProcessResultEnum.AlreadyExists;
                }

                Idol idol = await idolRepository.GetIdolWithAliasesAsync(idolName, idolGroup);

                if (idol == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                IdolAlias alias = new()
                {
                    IdolId = 0,
                    Alias = idolAlias,
                };
                idol.IdolAliases.Add(alias);

                await idolRepository.UpdateIdolAsync(idol);

                logger.Log($"Idol [{idolAlias}-{idolName}]-[{idolGroup}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs AddIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveIdolAliasAsync(string idolAlias, string idolName, string idolGroup)
        {
            try
            {
                IdolAlias idolAliasItem = await idolAliasRepository.FirstOrDefaultAsync(
                    ia => ia.Alias == idolAlias &&
                    ia.Idol.Name == idolName &&
                    ia.Idol.Group.Name == idolGroup,
                    ia => ia.Idol,
                    ia => ia.Idol.Group);
                if (idolAlias != null)
                {
                    await idolAliasRepository.RemoveAsync(idolAliasItem);

                    logger.Log($"Idol [{idolAlias}-{idolName}]-[{idolGroup}] removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Idol [{idolAlias}-{idolName}]-[{idolGroup}] could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs RemoveIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}
