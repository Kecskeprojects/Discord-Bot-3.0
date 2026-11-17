namespace DatabaseORM.Interface.Service;

public interface IUserIdolStatisticService
{
    Task UpdateUserStatisticsAsync(ulong userId, Stack<int> ranking);
}
