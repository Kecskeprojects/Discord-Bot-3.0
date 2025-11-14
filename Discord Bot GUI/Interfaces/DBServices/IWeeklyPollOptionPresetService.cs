using Discord_Bot.Communication.Modal;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IWeeklyPollOptionPresetService
{
    Task<List<WeeklyPollOptionPresetResource>> GetActivePresetsAsync();
    Task<WeeklyPollOptionPresetResource> GetOrCreateDummyPresetAsync();
    Task<WeeklyPollOptionPresetResource> GetPresetByIdAsync(int presetId);
    Task<WeeklyPollOptionPresetResource> GetPresetByNameAsync(string presetName);
    Task<List<WeeklyPollOptionPresetResource>> GetPresetsAsync();
    Task<DbProcessResultEnum> RemovePresetByNameAsync(string presetName);
    Task<DbProcessResultEnum> UpdateAsync(int presetId, EditWeeklyPollOptionPresetModal modal);
    Task<DbProcessResultEnum> UpdateFieldAsync(int presetId, string fieldName, string newValue);
}
