using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IIdolGroupService
{
    Task<IdolGroupExtendedResource?> GetIdolGroupDetailsAsync(string groupName);
    Task<bool> GroupExistsAsnyc(string groupName);
    Task<DbProcessResultEnum> UpdateAsync(int groupId, IdolGroupEditData data);
}
