using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using MapsterMapper;

namespace DatabaseORM.Service;

internal sealed class IdolImageService(
    IIdolRepository idolRepository,
    IIdolImageRepository idolImageRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IIdolImageService
{
    private readonly IIdolRepository idolRepository = idolRepository;
    private readonly IIdolImageRepository idolImageRepository = idolImageRepository;

    public async Task<DbProcessResultEnum> AddOverrideAsync(int idolId, string imageUrl)
    {
        try
        {
            IdolImage? currentImage = await idolImageRepository.GetLatestByIdolIdAsync(idolId);

            IdolImage newImage = new()
            {
                IdolId = idolId,
                ImageUrl = imageUrl,
                OverriddenUrl = currentImage?.ImageUrl,
            };

            _ = await idolImageRepository.AddAsync(newImage);
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
            Idol? idolForImage = await idolRepository.FirstOrDefaultAsync(
                i => i.Name == idol
                && i.Group.Name == group,
                i => i.Group,
                i => i.IdolImages);
            if (idolForImage != null)
            {
                _ = await idolImageRepository.RemoveAsync(idolForImage.IdolImages);

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