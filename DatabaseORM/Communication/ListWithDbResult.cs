using DatabaseORM.Enums;

namespace DatabaseORM.Communication;

public class ListWithDbResult<T>(List<T> list, DbProcessResultEnum result)
{
    public List<T> List { get; set; } = list;
    public DbProcessResultEnum ProcessResultEnum { get; set; } = result;
}
