using DatabaseORM.Enums;

namespace DatabaseORM.Interface.Service;

public interface IIdolAliasService
{
    Task<DbProcessResultEnum> AddIdolAliasAsync(string idolAlias, string idolName, string idolGroup);
    Task<DbProcessResultEnum> RemoveIdolAliasAsync(string idolAlias, string idolName, string idolGroup);
}
