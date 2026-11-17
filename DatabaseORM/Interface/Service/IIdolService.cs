using DatabaseORM.Communication;
using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IIdolService
{
    Task<DbProcessResultEnum> AddIdolAsync(string idolName, string idolGroup);
    Task<List<IdolResource>> GetIdolsByGroupAsync(string groupName);
    Task<DbProcessResultEnum> RemoveIdolAsync(string idolName, string idolGroup);
    Task<List<IdolResource>> GetAllIdolsAsync();
    Task<bool> UpdateIdolDetailsAsync(IdolResource idolResource, ExtendedBiasData? data, AdditionalIdolData? additional);
    Task<IdolExtendedResource?> GetIdolDetailsAsync(string idolName, string idolGroup);
    Task<DbProcessResultEnum> UpdateAsync(int idolId, IdolEditData data);
    Task<DbProcessResultEnum> UpdateAsync(int idolId, IdolExtendedEditData data);
    Task<DbProcessResultEnum> UpdateProfileUrlAsync(int idolId, string profileUrl);
    Task<DbProcessResultEnum> ChangeGroupAsync(int idolId, string groupName);
    Task<ListWithDbResult<UserResource>> GetUsersByIdolsAsync(string[] nameList);
    Task<List<IdolGameResource>> GetListForGameAsync(GenderEnum gender, int debutAfter, int debutBefore);
    Task<int> CorrectUpdateErrorsAsync();
    Task<IdolGameResource?> GetIdolByIdAsync(int idolId);
}
