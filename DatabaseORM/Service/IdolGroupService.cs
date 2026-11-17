using DatabaseORM.Caching;
using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;

namespace DatabaseORM.Service;

internal sealed class IdolGroupService(
    IIdolGroupRepository idolGroupRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IIdolGroupService
{
    private readonly IIdolGroupRepository idolGroupRepository = idolGroupRepository;

    public async Task<IdolGroupExtendedResource?> GetIdolGroupDetailsAsync(string groupName)
    {
        IdolGroupExtendedResource? resource = null;
        try
        {
            IdolGroup? idol = await idolGroupRepository.FirstOrDefaultAsync(ig => ig.Name == groupName);

            if (idol == null)
            {
                return resource;
            }

            resource = mapper.Map<IdolGroup, IdolGroupExtendedResource>(idol);
        }
        catch (Exception ex)
        {
            logger.Error("IdolGroupService.cs GetIdolGroupDetailsAsync", ex);
        }

        return resource;
    }

    public async Task<bool> GroupExistsAsnyc(string groupName)
    {
        bool result = false;
        try
        {
            result = await idolGroupRepository.ExistsAsync(x => x.Name == groupName);
        }
        catch (Exception ex)
        {
            logger.Error("IdolGroupService.cs GroupExistsAsnyc", ex);
        }

        return result;
    }

    public async Task<DbProcessResultEnum> UpdateAsync(int groupId, IdolGroupEditData data)
    {
        try
        {
            IdolGroup? idolGroup = await idolGroupRepository.FindByIdAsync(groupId);
            if (idolGroup == null)
            {
                return DbProcessResultEnum.NotFound;
            }
            idolGroup.Name = data.Name.ToLower().Trim();
            idolGroup.FullName = data.FullName.Trim();
            idolGroup.FullKoreanName = data.FullKoreanName.Trim();
            idolGroup.DebutDate = DateOnly.TryParse(data.DebutDate, out DateOnly debutDate) ? debutDate : idolGroup.DebutDate;

            _ = await idolGroupRepository.SaveChangesAsync();

            logger.Log($"Group with ID {groupId} updated successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("IdolGroupService.cs UpdateAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
