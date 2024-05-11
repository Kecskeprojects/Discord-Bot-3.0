using Discord_Bot.Communication.Modal;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IIdolGroupService
    {
        Task<IdolGroupExtendedResource> GetIdolGroupDetailsAsync(string groupName);
        Task<bool> GroupExistsAsnyc(string groupName);
        Task<DbProcessResultEnum> UpdateAsync(int groupId, EditGroupModal modal);
        Task<IdolGroup> UpdateOrCreateGroupAsync(IdolGroup group, string groupName);
    }
}
