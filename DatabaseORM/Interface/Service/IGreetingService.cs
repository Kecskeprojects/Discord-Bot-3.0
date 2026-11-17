using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IGreetingService
{
    Task<List<GreetingResource>> GetAllGreetingAsync();
    Task<DbProcessResultEnum> AddGreetingAsync(string url);
    Task<DbProcessResultEnum> RemoveGreetingAsync(int id);
}
