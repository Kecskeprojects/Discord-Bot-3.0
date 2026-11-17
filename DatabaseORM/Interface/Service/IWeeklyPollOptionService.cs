using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IWeeklyPollOptionService
{
    Task<WeeklyPollOptionResource?> GetOrCreateOptionAsync(bool isPresetOption, int Id, int optionId, byte orderNumber);
    Task<DbProcessResultEnum> UpdateAsync(int pollOptionId, string optionTitle);
}
