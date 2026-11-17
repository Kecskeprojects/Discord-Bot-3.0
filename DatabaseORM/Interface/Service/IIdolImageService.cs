using DatabaseORM.Enums;

namespace DatabaseORM.Interface.Service;

public interface IIdolImageService
{
    Task<DbProcessResultEnum> AddOverrideAsync(int idolId, string imageUrl);
    Task<DbProcessResultEnum> RemoveIdolImagesAsync(string idol, string group);
}
