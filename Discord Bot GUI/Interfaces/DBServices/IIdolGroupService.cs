using Discord_Bot.Communication.Modal;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IIdolGroupService
    {
        Task<IdolGroupExtendedResource> GetIdolGroupDetailsAsync(string group);
        Task<DbProcessResultEnum> UpdateAsync(int groupId, EditGroupModal modal);
    }
}
