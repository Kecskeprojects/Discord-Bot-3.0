using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IWeeklyPollOptionPresetService
{
    Task<List<WeeklyPollOptionPresetResource>> GetActivePresetsAsync();
    Task<WeeklyPollOptionPresetResource?> GetOrCreateDummyPresetAsync();
    Task<WeeklyPollOptionPresetResource?> GetPresetByIdAsync(int presetId);
    Task<WeeklyPollOptionPresetResource?> GetPresetByNameAsync(string presetName);
    Task<List<WeeklyPollOptionPresetResource>> GetPresetsAsync();
    Task<DbProcessResultEnum> RemovePresetByNameAsync(string presetName);
    Task<DbProcessResultEnum> UpdateAsync(int presetId, string name, string description);
    Task<DbProcessResultEnum> UpdateFieldAsync(int presetId, string fieldName, string newValue);
}
