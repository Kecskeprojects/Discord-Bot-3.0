using AutoMapper;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;
public class IdolImageService(
    IIdolRepository idolRepository,
    IIdolImageRepository idolImageRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IIdolImageService
{
    private readonly IIdolRepository idolRepository = idolRepository;
    private readonly IIdolImageRepository idolImageRepository = idolImageRepository;

    public async Task<DbProcessResultEnum> AddOverrideAsync(int idolId, OverrideImageModal modal)
    {
        try
        {
            IdolImage currentImage = await idolImageRepository.GetLatestByIdolIdAsync(idolId);

            IdolImage newImage = new()
            {
                IdolId = idolId,
                ImageUrl = modal.ImageUrl,
                OverriddenUrl = currentImage?.ImageUrl,
            };

            await idolImageRepository.AddAsync(newImage);
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("IdolImageService.cs AddOverrideAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveIdolImagesAsync(string idol, string group)
    {
        try
        {
            Idol idolForImage = await idolRepository.FirstOrDefaultAsync(
                i => i.Name == idol
                && i.Group.Name == group,
                i => i.Group,
                i => i.IdolImages);
            if (idolForImage != null)
            {
                await idolImageRepository.RemoveAsync(idolForImage.IdolImages);

                logger.Log("Idol Images removed successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log("Idol not found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("IdolImageService.cs RemoveIdolImagesAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}